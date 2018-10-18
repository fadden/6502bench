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
    /// Generate source code compatible with the cc65 assembler (https://github.com/cc65/cc65).
    /// </summary>
    public class GenCc65 : IGenerator {
        private const string ASM_FILE_SUFFIX = "_cc65.S";
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
        /// The first time we output a high-ASCII string, we generate a macro for it.
        /// </summary>
        private bool mHighAsciiMacroOutput;

        /// <summary>
        /// Holds detected version of configured assembler.
        /// </summary>
        private CommonUtil.Version mAsmVersion = CommonUtil.Version.NO_VERSION;

        // We test against this in a few places.
        private static CommonUtil.Version V2_17 = new CommonUtil.Version(2, 17);


        // Pseudo-op string constants.
        private static PseudoOp.PseudoOpNames sDataOpNames = new PseudoOp.PseudoOpNames() {
            EquDirective = "=",
            OrgDirective = ".org",
            //RegWidthDirective         // .a8, .a16, .i8, .i16
            DefineData1 = ".byte",
            DefineData2 = ".word",
            DefineData3 = ".faraddr",
            DefineData4 = ".dword",
            DefineBigData2 = ".dbyt",
            //DefineBigData3
            //DefineBigData4
            Fill = ".res",
            //Dense                     // no equivalent, use .byte with comma-separated args
            StrGeneric = ".byte",
            //StrReverse
            StrNullTerm = ".asciiz",
            //StrLen8                   // macro with .strlen?
            //StrLen16
            //StrDci
            //StrDciReverse
        };


        // IGenerator
        public void Configure(DisasmProject project, string workDirectory, string fileNameBase,
                AssemblerVersion asmVersion, AppSettings settings) {
            Debug.Assert(project != null);
            Debug.Assert(!string.IsNullOrEmpty(workDirectory));
            Debug.Assert(!string.IsNullOrEmpty(fileNameBase));

            Project = project;
            Quirks = new AssemblerQuirks();
            if (asmVersion != null) {
                // Use the actual version.  If it's > 2.17 we'll try to take advantage of
                // bug fixes.
                mAsmVersion = asmVersion.Version;
            } else {
                // No assembler installed.  Use 2.17.
                mAsmVersion = V2_17;
            }
            if (mAsmVersion <= V2_17) {
                // cc65 v2.17: https://github.com/cc65/cc65/issues/717
                Quirks.BlockMoveArgsReversed = true;
                // cc65 v2.17: https://github.com/cc65/cc65/issues/754
                Quirks.NoPcRelBankWrap = true;
            }

            mWorkDirectory = workDirectory;
            mFileNameBase = fileNameBase;
            Settings = settings;

            mLongLabelNewLine = Settings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);
        }

        // IGenerator
        public List<string> GenerateSource(BackgroundWorker worker) {
            List<string> pathNames = new List<string>(1);

            string fileName = mFileNameBase + ASM_FILE_SUFFIX;
            string pathName = Path.Combine(mWorkDirectory, fileName);
            pathNames.Add(pathName);

            Formatter.FormatConfig config = new Formatter.FormatConfig();
            GenCommon.ConfigureFormatterFromSettings(Settings, ref config);
            config.mForceAbsOpcodeSuffix = string.Empty;
            config.mForceLongOpcodeSuffix = string.Empty;
            config.mForceAbsOperandPrefix = "a:";       // absolute
            config.mForceLongOperandPrefix = "f:";      // far
            config.mEndOfLineCommentDelimiter = ";";
            config.mFullLineCommentDelimiterBase = ";";
            config.mBoxLineCommentDelimiter = ";";
            config.mAllowHighAsciiCharConst = false;
            config.mExpressionMode = Formatter.FormatConfig.ExpressionMode.Simple;
            SourceFormatter = new Formatter(config);

            string msg = string.Format(Properties.Resources.PROGRESS_GENERATING_FMT, pathName);
            worker.ReportProgress(0, msg);

            mLocalizer = new LabelLocalizer(Project);
            if (!Settings.GetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, false)) {
                mLocalizer.LocalPrefix = "@";
                mLocalizer.Analyze();
            }

            // Use UTF-8 encoding, without a byte-order mark.
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                mOutStream = sw;

                if (Settings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false)) {
                    //if (mAsmVersion.IsValid && mAsmVersion <= V2_17) {
                    //    OutputLine(SourceFormatter.FullLineCommentDelimiter +
                    //        string.Format(Properties.Resources.GENERATED_FOR_VERSION,
                    //        "cc65", mAsmVersion.ToString()));
                    //} else {
                    //    OutputLine(SourceFormatter.FullLineCommentDelimiter +
                    //        string.Format(Properties.Resources.GENERATED_FOR_LATEST, "cc65"));
                    //}

                    // Currently generating code for v2.17.
                    OutputLine(SourceFormatter.FullLineCommentDelimiter +
                        string.Format(Properties.Resources.GENERATED_FOR_VERSION,
                        "cc65", V2_17));
                }

                GenCommon.Generate(this, sw, worker);
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
                cpuStr = "65C02";
            } else if (cpuDef.Type == CpuDef.CpuType.Cpu6502 && cpuDef.HasUndocumented) {
                cpuStr = "6502X";
            } else {
                cpuStr = "6502";
            }

            OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(".setcpu"),
                '\"' + cpuStr + '\"', string.Empty);
        }

        /// <summary>
        /// Map the mnemonics we chose for undocumented opcodes to the cc65 mnemonics.
        /// After switching to the Unintended Opcodes mnemonics there's almost no difference.
        /// 
        /// We don't include the double- and triple-byte NOPs here, as cc65 doesn't
        /// appear to have a definition for them (as of 2.17).
        /// </summary>
        private static Dictionary<string, string> sUndocMap = new Dictionary<string, string>() {
            { OpName.ALR, "alr" },      // imm 0x4b
            { OpName.ANC, "anc" },      // imm 0x0b (and others)
            { OpName.ANE, "ane" },      // imm 0x8b
            { OpName.ARR, "arr" },      // imm 0x6b
            { OpName.DCP, "dcp" },      // abs 0xcf
            { OpName.ISC, "isc" },      // abs 0xef
            { OpName.JAM, "jam" },      // abs 0x02 (and others)
            { OpName.LAS, "las" },      // abs,y 0xbb
            { OpName.LAX, "lax" },      // imm 0xab; abs 0xaf
            { OpName.RLA, "rla" },      // abs 0x2f
            { OpName.RRA, "rra" },      // abs 0x6f
            { OpName.SAX, "sax" },      // abs 0x8f
            { OpName.SBX, "axs" },      //* imm 0xcb
            { OpName.SHA, "sha" },      // abs,y 0x9f
            { OpName.SHX, "shx" },      // abs,y 0x9e
            { OpName.SHY, "shy" },      // abs,x 0x9c
            { OpName.SLO, "slo" },      // abs 0x0f
            { OpName.SRE, "sre" },      // abs 0x4f
            { OpName.TAS, "tas" },      // abs,y 0x9b
        };

        // IGenerator
        public string ReplaceMnemonic(OpDef op) {
            if ((op == OpDef.OpWDM_WDM || op == OpDef.OpBRK_StackInt) && mAsmVersion <= V2_17) {
                // cc65 v2.17 doesn't support WDM, and assembles BRK <arg> to opcode $05.
                // https://github.com/cc65/cc65/issues/715
                // https://github.com/cc65/cc65/issues/716
                return null;
            } else if (op.IsUndocumented) {
                if (sUndocMap.TryGetValue(op.Mnemonic, out string newValue)) {
                    if ((op.Mnemonic == OpName.ANC && op.Opcode != 0x0b) ||
                            (op.Mnemonic == OpName.JAM && op.Opcode != 0x02)) {
                        // There are multiple opcodes for the same thing.  cc65 outputs
                        // one specific thing, so we need to match that, and just do a hex
                        // dump for the others.
                        return null;
                    }
                    return newValue;
                }
                return null;
            } else {
                return string.Empty;
            }
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
                        mLocalizer.LabelMap, dfd, operand, length, false);
                    break;
                case FormatDescriptor.Type.NumericBE:
                    opcodeStr = sDataOpNames.GetDefineBigData(length);
                    if (opcodeStr == null) {
                        // Nothing defined, output as comma-separated single-byte values.
                        GenerateShortSequence(offset, length, out opcodeStr, out operandStr);
                    } else {
                        operand = RawData.GetWord(data, offset, length, true);
                        operandStr = PseudoOp.FormatNumericOperand(formatter, Project.SymbolTable,
                            mLocalizer.LabelMap, dfd, operand, length, false);
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
        public void OutputOrgDirective(int address) {
            OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(sDataOpNames.OrgDirective),
                SourceFormatter.FormatHexValue(address, 4), string.Empty);
        }

        // IGenerator
        public void OutputRegWidthDirective(int offset, int prevM, int prevX, int newM, int newX) {
            if (prevM != newM) {
                string mop = (newM == 0) ? ".a16" : ".a8";
                OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(mop),
                    string.Empty, string.Empty);
            }
            if (prevX != newX) {
                string xop = (newX == 0) ? ".i16" : ".i8";
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
            // If a label is provided, and it doesn't start with a '.' (indicating that it's
            // a directive), and this isn't an EQU directive, add a ':'.  Might be easier to
            // just ".feature labels_without_colons", but I'm trying to do things the way
            // that cc65 users will expect.
            if (!string.IsNullOrEmpty(label) && label[0] != '.' &&
                    !String.Equals(opcode, sDataOpNames.EquDirective,
                        StringComparison.InvariantCultureIgnoreCase)) {
                label += ':';

                if (mLongLabelNewLine && label.Length >= 9) {
                    mOutStream.WriteLine(label);
                    label = string.Empty;
                }
            }

            mLineBuilder.Clear();
            TextUtil.AppendPaddedString(mLineBuilder, label, 9);
            TextUtil.AppendPaddedString(mLineBuilder, opcode, 9 + 8);
            TextUtil.AppendPaddedString(mLineBuilder, operand, 9 + 8 + 11);
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
            // Normal ASCII strings are straightforward: they're just part of a .byte
            // directive, and can mix with anything else in the .byte.
            //
            // For CString we can use .asciiz, but only if the string fits on one line
            // and doesn't include delimiters.  For L8String and L16String we can
            // define simple macros, but their use has a similar restriction.  High-ASCII
            // strings also require a macro.
            //
            // We might be able to define a macro for DCI and Reverse as well.
            //
            // The limitation on strings with delimiters arises because (1) I don't see a
            // way to escape them within a string, and (2) the simple macro workarounds
            // only take a single argument, not a comma-separated list of stuff.
            //
            // Some ideas here:
            // https://groups.google.com/forum/#!topic/comp.sys.apple2.programmer/5Wkw8mUPcU0

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
                    // Special case for simple short high-ASCII strings.  These have no
                    // leading or trailing bytes.  We can improve this a bit by handling
                    // arbitrarily long strings by simply breaking them across lines.
                    Debug.Assert(leadingBytes == 0);
                    Debug.Assert(trailingBytes == 0);
                    if (highAscii && gath.NumLinesOutput == 1 && !gath.HasDelimiter) {
                        if (!mHighAsciiMacroOutput) {
                            mHighAsciiMacroOutput = true;
                            // Output a macro for high-ASCII strings.
                            OutputLine(".macro", "HiAscii", "Arg", string.Empty);
                            OutputLine(string.Empty, ".repeat", ".strlen(Arg), I", string.Empty);
                            OutputLine(string.Empty, ".byte", ".strat(Arg, I) | $80", string.Empty);
                            OutputLine(string.Empty, ".endrep", string.Empty, string.Empty);
                            OutputLine(".endmacro", string.Empty, string.Empty, string.Empty);
                        }
                        opcodeStr = formatter.FormatPseudoOp("HiAscii");
                        highAscii = false;
                    }
                    break;
                case FormatDescriptor.SubType.Dci:
                case FormatDescriptor.SubType.Reverse:
                case FormatDescriptor.SubType.DciReverse:
                    // Full configured above.
                    break;
                case FormatDescriptor.SubType.CString:
                    if (gath.NumLinesOutput == 1 && !gath.HasDelimiter) {
                        opcodeStr = sDataOpNames.StrNullTerm;
                        showTrailing = false;
                    }
                    break;
                case FormatDescriptor.SubType.L8String:
                case FormatDescriptor.SubType.L16String:
                    // Implement macros?
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
    public class AsmCc65 : IAssembler {
        private List<string> PathNames { get; set; }

        private string WorkDirectory { get; set; }

        // IAssembler
        public void GetExeIdentifiers(out string humanName, out string exeName) {
            humanName = "cc65 CL";
            exeName = "cl65";
        }

        // IAssembler
        public AssemblerConfig GetDefaultConfig() {
            return new AssemblerConfig(string.Empty, new int[] { 9, 8, 11, 72 });
        }

        // IAssembler
        public AssemblerVersion QueryVersion() {
            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Cc65);
            if (config == null || string.IsNullOrEmpty(config.ExecutablePath)) {
                return null;
            }

            ShellCommand cmd = new ShellCommand(config.ExecutablePath, "--version",
                Directory.GetCurrentDirectory(), null);
            cmd.Execute();
            if (string.IsNullOrEmpty(cmd.Stdout)) {
                return null;
            }

            // Windows - Stderr: "cl65.exe V2.17\r\n"
            // Linux - Stderr:   "cl65 V2.17 - Git N/A\n"
            // Other platforms may not have the ".exe".  Find first occurrence of " V".

            const string PREFIX = " V";
            string str = cmd.Stderr;
            int start = str.IndexOf(PREFIX);
            int end = (start < 0) ? -1 : str.IndexOfAny(new char[] { ' ', '\r', '\n' }, start + 1);

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
            PathNames = new List<string>(pathNames.Count);
            foreach (string str in pathNames) {
                PathNames.Add(str);
            }

            WorkDirectory = workDirectory;
        }

        // IAssembler
        public AssemblerResults RunAssembler(BackgroundWorker worker) {
            // Reduce input file to a partial path if possible.  This is really just to make
            // what we display to the user a little easier to read.
            string pathName = PathNames[0];
            if (pathName.StartsWith(WorkDirectory)) {
                pathName = pathName.Remove(0, WorkDirectory.Length + 1);
            } else {
                // Unexpected, but shouldn't be a problem.
                Debug.WriteLine("NOTE: source file is not in work directory");
            }

            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Cc65);
            if (string.IsNullOrEmpty(config.ExecutablePath)) {
                Debug.WriteLine("Assembler not configured");
                return null;
            }

            // Wrap pathname in quotes in case it has spaces.
            // (Do we need to shell-escape quotes in the pathName?)
            ShellCommand cmd = new ShellCommand(config.ExecutablePath,
                "--target none \"" + pathName + "\"", WorkDirectory, null);
            cmd.Execute();

            // Can't really do anything with a "cancel" request.

            // Output filename is the input filename without the ".S".  Since the filename
            // was generated by us we can be confident in the format.
            string outputFile = PathNames[0].Substring(0, PathNames[0].Length - 2);

            return new AssemblerResults(cmd.FullCommandLine, cmd.ExitCode, cmd.Stdout,
                cmd.Stderr, outputFile);
        }
    }

    #endregion IAssembler
}
