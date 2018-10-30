/*
 * Copyright 2018 faddenSoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;

using Asm65;
using CommonUtil;

namespace SourceGen.AsmGen {
    #region IGenerator

    /// <summary>
    /// Generate source code compatible with the 64tass assembler
    /// (https://sourceforge.net/projects/tass64/).
    /// 
    /// The assembler is officially called "64tass", but it's sometimes written "tass64" because
    /// in some cases you can't start an identifier with a number.
    /// 
    /// We need to deal with a couple of unusual aspects:
    ///  (1) The prefix for a local label is '_', which is generally a legal character.  So
    ///    if somebody creates a label with a leading '_', and it's not actually local, we have
    ///    to "de-local" it somehow.
    ///  (2) By default, labels are handled in a case-insensitive fashion, which is extremely
    ///    rare for programming languages.  Case sensitivity can be enabled with the "-C" flag.
    ///    Anybody who wants to assemble the generated code will need to be aware of this.
    /// </summary>
    public class GenTass64 : IGenerator {
        private const string ASM_FILE_SUFFIX = "_64tass.S"; // must start with underscore
        private const int MAX_OPERAND_LEN = 64;

        // IGenerator
        public DisasmProject Project { get; private set; }

        // IGenerator
        public Formatter SourceFormatter { get; private set; }

        // IGenerator
        public AppSettings Settings { get; private set; }

        // IGenerator
        public AssemblerQuirks Quirks { get; private set; }

        // IGenerator
        public LabelLocalizer Localizer { get { return mLocalizer; } }

        /// <summary>
        /// Working directory, i.e. where we write our output file(s).
        /// </summary>
        private string mWorkDirectory;

        /// <summary>
        /// If set, long labels get their own line.
        /// </summary>
        private bool mLongLabelNewLine;

        /// <summary>
        /// Output column widths.
        /// </summary>
        private int[] mColumnWidths;

        /// <summary>
        /// Base filename.  Typically the project file name without the ".dis65" extension.
        /// </summary>
        private string mFileNameBase;

        /// <summary>
        /// StringBuilder to use when composing a line.  Held here to reduce allocations.
        /// </summary>
        private StringBuilder mLineBuilder = new StringBuilder(100);

        /// <summary>
        /// Label localization helper.
        /// </summary>
        private LabelLocalizer mLocalizer;

        /// <summary>
        /// Stream to send the output to.
        /// </summary>
        private StreamWriter mOutStream;

        /// <summary>
        /// If we output a ".logical", we will need a ".here" eventually.
        /// </summary>
        private bool mNeedHereOp;

        /// <summary>
        /// Holds detected version of configured assembler.
        /// </summary>
        private CommonUtil.Version mAsmVersion = CommonUtil.Version.NO_VERSION;

        // Version we're coded against.
        private static CommonUtil.Version V1_53 = new CommonUtil.Version(1, 53, 1515);


        // Pseudo-op string constants.
        private static PseudoOp.PseudoOpNames sDataOpNames = new PseudoOp.PseudoOpNames() {
            EquDirective = "=",
            OrgDirective = ".logical",
            //RegWidthDirective         // .as, .al, .xs, .xl
            DefineData1 = ".byte",
            DefineData2 = ".word",
            DefineData3 = ".long",
            DefineData4 = ".dword",
            //DefineBigData2
            //DefineBigData3
            //DefineBigData4
            Fill = ".fill",
            //Dense                     // no equivalent, use .byte with comma-separated args
            StrGeneric = ".text",
            //StrReverse
            StrNullTerm = ".null",
            StrLen8 = ".ptext",
            //StrLen16
            //StrDci
            //StrDciReverse
        };
        private const string HERE_PSEUDO_OP = ".here";


        // IGenerator
        public void GetDefaultDisplayFormat(out PseudoOp.PseudoOpNames pseudoOps,
                out Formatter.FormatConfig formatConfig) {
            pseudoOps = sDataOpNames;

            formatConfig = new Formatter.FormatConfig();
            SetFormatConfigValues(ref formatConfig);
        }

        // IGenerator
        public void Configure(DisasmProject project, string workDirectory, string fileNameBase,
                AssemblerVersion asmVersion, AppSettings settings) {
            Debug.Assert(project != null);
            Debug.Assert(!string.IsNullOrEmpty(workDirectory));
            Debug.Assert(!string.IsNullOrEmpty(fileNameBase));

            Project = project;
            Quirks = new AssemblerQuirks();

            mWorkDirectory = workDirectory;
            mFileNameBase = fileNameBase;
            Settings = settings;

            mLongLabelNewLine = Settings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);

            AssemblerConfig config = AssemblerConfig.GetConfig(settings,
                AssemblerInfo.Id.Tass64);
            mColumnWidths = (int[])config.ColumnWidths.Clone();
        }

        /// <summary>
        /// Configures the assembler-specific format items.
        /// </summary>
        private void SetFormatConfigValues(ref Formatter.FormatConfig config) {
            // Must be lower case when --case-sensitive is used.
            config.mUpperOpcodes = false;
            config.mUpperPseudoOpcodes = false;
            config.mUpperOperandA = false;
            config.mUpperOperandS = false;
            config.mUpperOperandXY = false;

            config.mBankSelectBackQuote = true;

            config.mForceAbsOpcodeSuffix = string.Empty;
            config.mForceLongOpcodeSuffix = string.Empty;
            config.mForceAbsOperandPrefix = "@w";       // word
            config.mForceLongOperandPrefix = "@l";      // long
            config.mEndOfLineCommentDelimiter = ";";
            config.mFullLineCommentDelimiterBase = ";";
            config.mBoxLineCommentDelimiter = ";";
            config.mAllowHighAsciiCharConst = false;
            config.mExpressionMode = Formatter.FormatConfig.ExpressionMode.Common;
        }

        // IGenerator
        public List<string> GenerateSource(BackgroundWorker worker) {
            List<string> pathNames = new List<string>(1);

            string fileName = mFileNameBase + ASM_FILE_SUFFIX;
            string pathName = Path.Combine(mWorkDirectory, fileName);
            pathNames.Add(pathName);

            Formatter.FormatConfig config = new Formatter.FormatConfig();
            GenCommon.ConfigureFormatterFromSettings(Settings, ref config);
            SetFormatConfigValues(ref config);
            SourceFormatter = new Formatter(config);

            string msg = string.Format(Properties.Resources.PROGRESS_GENERATING_FMT, pathName);
            worker.ReportProgress(0, msg);

            mLocalizer = new LabelLocalizer(Project);
            if (!Settings.GetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, false)) {
                mLocalizer.LocalPrefix = "_";
                mLocalizer.Analyze();
            }
            mLocalizer.MaskLeadingUnderscores();

            // Use UTF-8 encoding, without a byte-order mark.
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                mOutStream = sw;

                if (Settings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false)) {
                    OutputLine(SourceFormatter.FullLineCommentDelimiter +
                        string.Format(Properties.Resources.GENERATED_FOR_VERSION,
                        "64tass", V1_53));
                }

                GenCommon.Generate(this, sw, worker);

                if (mNeedHereOp) {
                    OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(HERE_PSEUDO_OP),
                        string.Empty, string.Empty);
                }
            }
            mOutStream = null;

            return pathNames;
        }

        // IGenerator
        public void OutputAsmConfig() {
            CpuDef cpuDef = Project.CpuDef;
            string cpuStr;
            if (cpuDef.Type == CpuDef.CpuType.Cpu65816) {
                cpuStr = "65816";
            } else if (cpuDef.Type == CpuDef.CpuType.Cpu65C02) {
                cpuStr = "65c02";
            } else if (cpuDef.Type == CpuDef.CpuType.Cpu6502 && cpuDef.HasUndocumented) {
                cpuStr = "6502i";
            } else {
                // 6502 def includes undocumented ops
                cpuStr = "6502";
            }

            OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(".cpu"),
                '\"' + cpuStr + '\"', string.Empty);
        }

        // IGenerator
        public string ReplaceMnemonic(OpDef op) {
            if (op.IsUndocumented) {
                if (Project.CpuDef.Type == CpuDef.CpuType.Cpu65C02) {
                    // none of the "LDD" stuff is handled
                    return null;
                }
                if ((op.Mnemonic == OpName.ANC && op.Opcode != 0x0b) ||
                        (op.Mnemonic == OpName.JAM && op.Opcode != 0x02)) {
                    // There are multiple opcodes that match the mnemonic.  Output the
                    // mnemonic for the first one and hex for the rest.
                    return null;
                } else if (op.Mnemonic == OpName.NOP || op.Mnemonic == OpName.DOP ||
                        op.Mnemonic == OpName.TOP) {
                    // the various undocumented no-ops aren't handled
                    return null;
                } else if (op.Mnemonic == OpName.SBC) {
                    // this is the alternate reference to SBC
                    return null;
                } else if (op == OpDef.OpSHA_DPIndIndexY) {
                    // not recognized ($93)
                    return null;
                }
            }
            if (op == OpDef.OpBRK_StackInt || op == OpDef.OpCOP_StackInt ||
                    op == OpDef.OpWDM_WDM) {
                // 64tass doesn't like these to have an operand.  Output as hex.
                return null;
            }
            return string.Empty;        // indicate original is fine
        }

        // IGenerator
        public void GenerateShortSequence(int offset, int length, out string opcode,
                out string operand) {
            Debug.Assert(length >= 1 && length <= 4);

            // Use a comma-separated list of individual hex bytes.
            opcode = sDataOpNames.DefineData1;

            StringBuilder sb = new StringBuilder(length * 4);
            for (int i = 0; i < length; i++) {
                if (i != 0) {
                    sb.Append(',');
                }
                sb.Append(SourceFormatter.FormatHexValue(Project.FileData[offset + i], 2));
            }
            operand = sb.ToString();
        }

        // IGenerator
        public void OutputDataOp(int offset) {
            Formatter formatter = SourceFormatter;
            byte[] data = Project.FileData;
            Anattrib attr = Project.GetAnattrib(offset);

            string labelStr = string.Empty;
            if (attr.Symbol != null) {
                labelStr = mLocalizer.ConvLabel(attr.Symbol.Label);
            }

            string commentStr = SourceFormatter.FormatEolComment(Project.Comments[offset]);
            string opcodeStr, operandStr;

            FormatDescriptor dfd = attr.DataDescriptor;
            Debug.Assert(dfd != null);
            int length = dfd.Length;
            Debug.Assert(length > 0);

            bool multiLine = false;
            switch (dfd.FormatType) {
                case FormatDescriptor.Type.Default:
                    if (length != 1) {
                        Debug.Assert(false);
                        length = 1;
                    }
                    opcodeStr = sDataOpNames.DefineData1;
                    int operand = RawData.GetWord(data, offset, length, false);
                    operandStr = formatter.FormatHexValue(operand, length * 2);
                    break;
                case FormatDescriptor.Type.NumericLE:
                    opcodeStr = sDataOpNames.GetDefineData(length);
                    operand = RawData.GetWord(data, offset, length, false);
                    operandStr = PseudoOp.FormatNumericOperand(formatter, Project.SymbolTable,
                        mLocalizer.LabelMap, dfd, operand, length,
                        PseudoOp.FormatNumericOpFlags.None);
                    break;
                case FormatDescriptor.Type.NumericBE:
                    opcodeStr = sDataOpNames.GetDefineBigData(length);
                    if (opcodeStr == null) {
                        // Nothing defined, output as comma-separated single-byte values.
                        GenerateShortSequence(offset, length, out opcodeStr, out operandStr);
                    } else {
                        operand = RawData.GetWord(data, offset, length, true);
                        operandStr = PseudoOp.FormatNumericOperand(formatter, Project.SymbolTable,
                            mLocalizer.LabelMap, dfd, operand, length,
                            PseudoOp.FormatNumericOpFlags.None);
                    }
                    break;
                case FormatDescriptor.Type.Fill:
                    opcodeStr = sDataOpNames.Fill;
                    operandStr = length + "," + formatter.FormatHexValue(data[offset], 2);
                    break;
                case FormatDescriptor.Type.Dense:
                    multiLine = true;
                    opcodeStr = operandStr = null;
                    OutputDenseHex(offset, length, labelStr, commentStr);
                    break;
                case FormatDescriptor.Type.String:
                    multiLine = true;
                    opcodeStr = operandStr = null;
                    OutputString(offset, labelStr, commentStr);
                    break;
                default:
                    opcodeStr = "???";
                    operandStr = "***";
                    break;
            }

            if (!multiLine) {
                opcodeStr = formatter.FormatPseudoOp(opcodeStr);
                OutputLine(labelStr, opcodeStr, operandStr, commentStr);
            }
        }

        private void OutputDenseHex(int offset, int length, string labelStr, string commentStr) {
            Formatter formatter = SourceFormatter;
            byte[] data = Project.FileData;
            StringBuilder sb = new StringBuilder(MAX_OPERAND_LEN);

            string opcodeStr = formatter.FormatPseudoOp(sDataOpNames.DefineData1);

            int maxPerLine = MAX_OPERAND_LEN / 4;
            int numChunks = (length + maxPerLine - 1) / maxPerLine;
            for (int chunk = 0; chunk < numChunks; chunk++) {
                int chunkStart = chunk * maxPerLine;
                int chunkEnd = Math.Min((chunk + 1) * maxPerLine, length);
                for (int i = chunkStart; i < chunkEnd; i++) {
                    if (i != chunkStart) {
                        sb.Append(',');
                    }
                    sb.Append(formatter.FormatHexValue(data[offset + i], 2));
                }

                OutputLine(labelStr, opcodeStr, sb.ToString(), commentStr);
                labelStr = commentStr = string.Empty;
                sb.Clear();
            }
        }

        /// <summary>
        /// Outputs formatted data in an unformatted way, because the code generator couldn't
        /// figure out how to do something better.
        /// </summary>
        private void OutputNoJoy(int offset, int length, string labelStr, string commentStr) {
            byte[] data = Project.FileData;
            Debug.Assert(length > 0);
            Debug.Assert(offset >= 0 && offset < data.Length);

            bool singleValue = true;
            byte val = data[offset];
            for (int i = 1; i < length; i++) {
                if (data[offset + i] != val) {
                    singleValue = false;
                    break;
                }
            }

            if (singleValue) {
                string opcodeStr = SourceFormatter.FormatPseudoOp(sDataOpNames.Fill);
                string operandStr = length + "," + SourceFormatter.FormatHexValue(val, 2);
                OutputLine(labelStr, opcodeStr, operandStr, commentStr);
            } else {
                OutputDenseHex(offset, length, labelStr, commentStr);
            }
        }

        // IGenerator
        public void OutputEquDirective(string name, string valueStr, string comment) {
            OutputLine(name, SourceFormatter.FormatPseudoOp(sDataOpNames.EquDirective),
                valueStr, SourceFormatter.FormatEolComment(comment));
        }

        // IGenerator
        public void OutputOrgDirective(int offset, int address) {
            // 64tass separates the "compile offset", which determines where the output fits
            // into the generated binary, and "program counter", which determines the code
            // the assembler generates.  Since we need to explicitly specify every byte in
            // the output file, the compile offset isn't very useful.  We want to set it once
            // before the first line of code, then leave it alone.
            //
            // Any subsequent ORG changes are made to the program counter, and take the form
            // of a pair of ops (.logical <addr> to open, .here to end).  Omitting the .here
            // causes an error.
            if (offset == 0) {
                // Set the "compile offset", which determines where assembled code goes in the
                // output file.  This 
                //
                // This is different from the "program counter", which determines how code is
                // actually assembled.
                OutputLine("*", "=", SourceFormatter.FormatHexValue(Project.AddrMap.Get(0), 4),
                    string.Empty);
            } else {
                if (mNeedHereOp) {
                    OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(HERE_PSEUDO_OP),
                        string.Empty, string.Empty);
                }
                OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(sDataOpNames.OrgDirective),
                    SourceFormatter.FormatHexValue(address, 4), string.Empty);
                mNeedHereOp = true;
            }
        }

        // IGenerator
        public void OutputRegWidthDirective(int offset, int prevM, int prevX, int newM, int newX) {
            if (prevM != newM) {
                string mop = (newM == 0) ? ".al" : ".as";
                OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(mop),
                    string.Empty, string.Empty);
            }
            if (prevX != newX) {
                string xop = (newX == 0) ? ".xl" : ".xs";
                OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(xop),
                    string.Empty, string.Empty);
            }
        }

        // IGenerator
        public void OutputLine(string fullLine) {
            mOutStream.WriteLine(fullLine);
        }

        // IGenerator
        public void OutputLine(string label, string opcode, string operand, string comment) {
            // Break the line if the label is long and it's not a .EQ directive.
            if (!string.IsNullOrEmpty(label) &&
                    !string.Equals(opcode, sDataOpNames.EquDirective,
                        StringComparison.InvariantCultureIgnoreCase)) {

                if (mLongLabelNewLine && label.Length >= mColumnWidths[0]) {
                    mOutStream.WriteLine(label);
                    label = string.Empty;
                }
            }

            mLineBuilder.Clear();
            TextUtil.AppendPaddedString(mLineBuilder, label, mColumnWidths[0]);
            TextUtil.AppendPaddedString(mLineBuilder, opcode, mColumnWidths[0] + mColumnWidths[1]);
            TextUtil.AppendPaddedString(mLineBuilder, operand,
                mColumnWidths[0] + mColumnWidths[1] + mColumnWidths[2]);
            if (string.IsNullOrEmpty(comment)) {
                // Trim trailing spaces off of opcode or operand.  If they want trailing
                // spaces at the end of a comment, that's fine.
                CommonUtil.TextUtil.TrimEnd(mLineBuilder);
            } else {
                mLineBuilder.Append(comment);
            }

            mOutStream.WriteLine(mLineBuilder.ToString());
        }

        private void OutputString(int offset, string labelStr, string commentStr) {
            // Normal ASCII strings are handled with a simple .text directive.
            //
            // CString and L8String have directives (.null, .ptext), but we can only use
            // them if the string fits on one line and doesn't include delimiters.
            //
            // We could probably do something fancy with the character encoding options to
            // make high-ASCII work nicely.
            //
            // We might be able to define a macro for DCI and Reverse.

            Formatter formatter = SourceFormatter;
            byte[] data = Project.FileData;
            Anattrib attr = Project.GetAnattrib(offset);
            FormatDescriptor dfd = attr.DataDescriptor;
            Debug.Assert(dfd != null);
            Debug.Assert(dfd.FormatType == FormatDescriptor.Type.String);
            Debug.Assert(dfd.Length > 0);

            bool highAscii = false;
            int leadingBytes = 0;
            int trailingBytes = 0;
            bool showLeading = false;
            bool showTrailing = false;

            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.None:
                    highAscii = (data[offset] & 0x80) != 0;
                    break;
                case FormatDescriptor.SubType.Dci:
                    highAscii = (data[offset] & 0x80) != 0;
                    trailingBytes = 1;
                    showTrailing = true;
                    break;
                case FormatDescriptor.SubType.Reverse:
                    highAscii = (data[offset] & 0x80) != 0;
                    break;
                case FormatDescriptor.SubType.DciReverse:
                    highAscii = (data[offset + dfd.Length - 1] & 0x80) != 0;
                    leadingBytes = 1;
                    showLeading = true;
                    break;
                case FormatDescriptor.SubType.CString:
                    highAscii = (data[offset] & 0x80) != 0;
                    trailingBytes = 1;
                    showTrailing = true;
                    break;
                case FormatDescriptor.SubType.L8String:
                    if (dfd.Length > 1) {
                        highAscii = (data[offset + 1] & 0x80) != 0;
                    }
                    leadingBytes = 1;
                    showLeading = true;
                    break;
                case FormatDescriptor.SubType.L16String:
                    if (dfd.Length > 2) {
                        highAscii = (data[offset + 2] & 0x80) != 0;
                    }
                    leadingBytes = 2;
                    showLeading = true;
                    break;
                default:
                    Debug.Assert(false);
                    return;
            }

            char delim = '"';
            StringGather gath = null;

            // Run the string through so we can see if it'll fit on one line.  As a minor
            // optimization, we skip this step for "generic" strings, which are probably
            // the most common thing.
            if (dfd.FormatSubType != FormatDescriptor.SubType.None || highAscii) {
                gath = new StringGather(this, labelStr, "???", commentStr, delim,
                        delim, StringGather.ByteStyle.CommaSep, MAX_OPERAND_LEN, true);
                FeedGath(gath, data, offset, dfd.Length, leadingBytes, showLeading,
                    trailingBytes, showTrailing);
                Debug.Assert(gath.NumLinesOutput > 0);
            }

            string opcodeStr = formatter.FormatPseudoOp(sDataOpNames.StrGeneric);

            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.None:
                    // TODO(someday): something fancy with encodings to handle high-ASCII text?
                    break;
                case FormatDescriptor.SubType.Dci:
                case FormatDescriptor.SubType.Reverse:
                case FormatDescriptor.SubType.DciReverse:
                    // Fully configured above.
                    break;
                case FormatDescriptor.SubType.CString:
                    if (gath.NumLinesOutput == 1 && !gath.HasDelimiter) {
                        opcodeStr = sDataOpNames.StrNullTerm;
                        showTrailing = false;
                    }
                    break;
                case FormatDescriptor.SubType.L8String:
                    if (gath.NumLinesOutput == 1 && !gath.HasDelimiter) {
                        opcodeStr = sDataOpNames.StrLen8;
                        showLeading = false;
                    }
                    break;
                case FormatDescriptor.SubType.L16String:
                    // Implement as macro?
                    break;
                default:
                    Debug.Assert(false);
                    return;
            }

            if (highAscii) {
                OutputNoJoy(offset, dfd.Length, labelStr, commentStr);
                return;
            }

            // Create a new StringGather, with the final opcode choice.
            gath = new StringGather(this, labelStr, opcodeStr, commentStr, delim,
                delim, StringGather.ByteStyle.CommaSep, MAX_OPERAND_LEN, false);
            FeedGath(gath, data, offset, dfd.Length, leadingBytes, showLeading,
                trailingBytes, showTrailing);
        }

        /// <summary>
        /// Feeds the bytes into the StringGather.
        /// </summary>
        private void FeedGath(StringGather gath, byte[] data, int offset, int length,
                int leadingBytes, bool showLeading, int trailingBytes, bool showTrailing) {
            int startOffset = offset;
            int strEndOffset = offset + length - trailingBytes;

            if (showLeading) {
                while (leadingBytes-- > 0) {
                    gath.WriteByte(data[offset++]);
                }
            } else {
                offset += leadingBytes;
            }
            for (; offset < strEndOffset; offset++) {
                gath.WriteChar((char)(data[offset] & 0x7f));
            }
            while (showTrailing && trailingBytes-- > 0) {
                gath.WriteByte(data[offset++]);
            }
            gath.Finish();
        }
    }

    #endregion IGenerator


    #region IAssembler

    /// <summary>
    /// Cross-assembler execution interface.
    /// </summary>
    public class AsmTass64 : IAssembler {
        // Paths from generator.
        private List<string> mPathNames;

        // Directory to make current before executing assembler.
        private string mWorkDirectory;


        // IAssembler
        public void GetExeIdentifiers(out string humanName, out string exeName) {
            humanName = "64tass Assembler";
            exeName = "64tass";
        }

        // IAssembler
        public AssemblerConfig GetDefaultConfig() {
            return new AssemblerConfig(string.Empty, new int[] { 8, 8, 11, 73 });
        }

        // IAssembler
        public AssemblerVersion QueryVersion() {
            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Tass64);
            if (config == null || string.IsNullOrEmpty(config.ExecutablePath)) {
                return null;
            }

            ShellCommand cmd = new ShellCommand(config.ExecutablePath, "--version",
                Directory.GetCurrentDirectory(), null);
            cmd.Execute();
            if (string.IsNullOrEmpty(cmd.Stdout)) {
                return null;
            }

            // Windows - Stdout: "64tass Turbo Assembler Macro V1.53.1515\r\n"
            // Linux - Stdout:   "64tass Turbo Assembler Macro V1.53.1515?\n"

            const string PREFIX = "Macro V";
            string str = cmd.Stdout;
            int start = str.IndexOf(PREFIX);
            int end = (start < 0) ? -1 : str.IndexOfAny(new char[] { '?', '\r', '\n' }, start + 1);

            if (start < 0 || end < 0 || start + PREFIX.Length >= end) {
                Debug.WriteLine("Couldn't find version in " + str);
                return null;
            }
            start += PREFIX.Length;
            string versionStr = str.Substring(start, end - start);
            CommonUtil.Version version = CommonUtil.Version.Parse(versionStr);
            if (!version.IsValid) {
                return null;
            }
            return new AssemblerVersion(versionStr, version);
        }

        // IAssembler
        public void Configure(List<string> pathNames, string workDirectory) {
            // Clone pathNames, in case the caller decides to modify the original.
            mPathNames = new List<string>(pathNames.Count);
            foreach (string str in pathNames) {
                mPathNames.Add(str);
            }

            mWorkDirectory = workDirectory;
        }

        // IAssembler
        public AssemblerResults RunAssembler(BackgroundWorker worker) {
            // Reduce input file to a partial path if possible.  This is really just to make
            // what we display to the user a little easier to read.
            string pathName = mPathNames[0];
            if (pathName.StartsWith(mWorkDirectory)) {
                pathName = pathName.Remove(0, mWorkDirectory.Length + 1);
            } else {
                // Unexpected, but shouldn't be a problem.
                Debug.WriteLine("NOTE: source file is not in work directory");
            }

            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Tass64);
            if (string.IsNullOrEmpty(config.ExecutablePath)) {
                Debug.WriteLine("Assembler not configured");
                return null;
            }

            worker.ReportProgress(0, Properties.Resources.PROGRESS_ASSEMBLING);

            string options = "--case-sensitive --nostart --long-address -Wall";
            string outFileName = pathName.Substring(0, pathName.Length - 2);

            // Wrap pathname in quotes in case it has spaces.
            // (Do we need to shell-escape quotes in the pathName?)
            ShellCommand cmd = new ShellCommand(config.ExecutablePath,
                options + " \"" + pathName + "\"" + " -o \"" + outFileName + "\"",
                mWorkDirectory, null);
            cmd.Execute();

            // Can't really do anything with a "cancel" request.

            // Output filename is the input filename without the ".S".  Since the filename
            // was generated by us we can be confident in the format.
            string outputFile = mPathNames[0].Substring(0, mPathNames[0].Length - 2);

            return new AssemblerResults(cmd.FullCommandLine, cmd.ExitCode, cmd.Stdout,
                cmd.Stderr, outputFile);
        }
    }

    #endregion IAssembler
}
