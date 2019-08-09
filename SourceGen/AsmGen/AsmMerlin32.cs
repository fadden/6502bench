/*
 * Copyright 2019 faddenSoft
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
    /// Generate source code compatible with Brutal Deluxe's Merlin 32 assembler
    /// (https://www.brutaldeluxe.fr/products/crossdevtools/merlin/).
    /// </summary>
    public class GenMerlin32 : IGenerator {
        private const string ASM_FILE_SUFFIX = "_Merlin32.S";   // must start with underscore
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
        /// Holds detected version of configured assembler.
        /// </summary>
        private CommonUtil.Version mAsmVersion = CommonUtil.Version.NO_VERSION;


        // Semi-convenient way to hold all the interesting string constants in one place.
        // Note the actual usage of the pseudo-op may not match what the main app does,
        // e.g. RegWidthDirective behaves differently from "mx".  I'm just trying to avoid
        // having string constants scattered all over.
        private static PseudoOp.PseudoOpNames sDataOpNames = new PseudoOp.PseudoOpNames() {
            EquDirective = "equ",
            OrgDirective = "org",
            RegWidthDirective = "mx",
            DefineData1 = "dfb",
            DefineData2 = "dw",
            DefineData3 = "adr",
            DefineData4 = "adrl",
            DefineBigData2 = "ddb",
            //DefineBigData3
            //DefineBigData4
            Fill = "ds",
            Dense = "hex",
            StrGeneric = "asc",
            StrGenericHi = "asc",
            StrReverse = "rev",
            StrReverseHi = "rev",
            //StrNullTerm
            StrLen8 = "str",
            StrLen8Hi = "str",
            StrLen16 = "strl",
            StrLen16Hi = "strl",
            StrDci = "dci",
            StrDciHi = "dci",
        };


        // IGenerator
        public void GetDefaultDisplayFormat(out PseudoOp.PseudoOpNames pseudoOps,
                out Formatter.FormatConfig formatConfig) {
            // This is not intended to match up with the Merlin generator, which uses
            // the same pseudo-op for low/high ASCII but different string delimiters.  We
            // don't change the delimiters for the display list, so instead we tweak the
            // opcode slightly.
            char hiAscii = '\u2191';
            pseudoOps = new PseudoOp.PseudoOpNames() {
                EquDirective = "equ",
                OrgDirective = "org",
                DefineData1 = "dfb",
                DefineData2 = "dw",
                DefineData3 = "adr",
                DefineData4 = "adrl",
                DefineBigData2 = "ddb",
                Fill = "ds",
                Dense = "hex",
                StrGeneric = "asc",
                StrGenericHi = "asc" + hiAscii,
                StrReverse = "rev",
                StrReverseHi = "rev" + hiAscii,
                StrLen8 = "str",
                StrLen8Hi = "str" + hiAscii,
                StrLen16 = "strl",
                StrLen16Hi = "strl" + hiAscii,
                StrDci = "dci",
                StrDciHi = "dci" + hiAscii,
            };

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
            Quirks.TracksSepRepNotEmu = true;
            Quirks.NoPcRelBankWrap = true;

            mWorkDirectory = workDirectory;
            mFileNameBase = fileNameBase;
            Settings = settings;

            mLongLabelNewLine = Settings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);

            AssemblerConfig config = AssemblerConfig.GetConfig(settings,
                AssemblerInfo.Id.Merlin32);
            mColumnWidths = (int[])config.ColumnWidths.Clone();
        }

        /// <summary>
        /// Configures the assembler-specific format items.
        /// </summary>
        private void SetFormatConfigValues(ref Formatter.FormatConfig config) {
            config.mForceDirectOpcodeSuffix = string.Empty;
            config.mForceAbsOpcodeSuffix = ":";
            config.mForceLongOpcodeSuffix = "l";
            config.mForceDirectOperandPrefix = string.Empty;
            config.mForceAbsOperandPrefix = string.Empty;
            config.mForceLongOperandPrefix = string.Empty;
            config.mEndOfLineCommentDelimiter = ";";
            config.mFullLineCommentDelimiterBase = ";";
            config.mBoxLineCommentDelimiter = string.Empty;
            config.mAllowHighAsciiCharConst = true;
            config.mExpressionMode = Formatter.FormatConfig.ExpressionMode.Merlin;
        }

        // IGenerator; executes on background thread
        public List<string> GenerateSource(BackgroundWorker worker) {
            List<string> pathNames = new List<string>(1);

            string fileName = mFileNameBase + ASM_FILE_SUFFIX;
            string pathName = Path.Combine(mWorkDirectory, fileName);
            pathNames.Add(pathName);

            Formatter.FormatConfig config = new Formatter.FormatConfig();
            GenCommon.ConfigureFormatterFromSettings(Settings, ref config);
            SetFormatConfigValues(ref config);
            SourceFormatter = new Formatter(config);

            string msg = string.Format(Res.Strings.PROGRESS_GENERATING_FMT, pathName);
            worker.ReportProgress(0, msg);

            mLocalizer = new LabelLocalizer(Project);
            if (!Settings.GetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, false)) {
                mLocalizer.LocalPrefix = ":";
                mLocalizer.Analyze();
            }

            // Use UTF-8 encoding, without a byte-order mark.
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                mOutStream = sw;

                if (Settings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false)) {
                    // No version-specific stuff yet.  We're generating code for v1.0.
                    OutputLine(SourceFormatter.FullLineCommentDelimiter +
                        string.Format(Res.Strings.GENERATED_FOR_VERSION_FMT,
                            "Merlin 32", new CommonUtil.Version(1, 0), string.Empty));
                }

                GenCommon.Generate(this, sw, worker);
            }
            mOutStream = null;

            return pathNames;
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
                case FormatDescriptor.Type.StringGeneric:
                case FormatDescriptor.Type.StringReverse:
                case FormatDescriptor.Type.StringNullTerm:
                case FormatDescriptor.Type.StringL8:
                case FormatDescriptor.Type.StringL16:
                case FormatDescriptor.Type.StringDci:
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
            int maxPerLine = MAX_OPERAND_LEN / 2;

            string opcodeStr = formatter.FormatPseudoOp(sDataOpNames.Dense);
            for (int i = 0; i < length; i += maxPerLine) {
                int subLen = length - i;
                if (subLen > maxPerLine) {
                    subLen = maxPerLine;
                }
                string operandStr = formatter.FormatDenseHex(data, offset + i, subLen);

                OutputLine(labelStr, opcodeStr, operandStr, commentStr);
                labelStr = commentStr = string.Empty;
            }
        }

        // IGenerator
        public string ModifyOpcode(int offset, OpDef op) {
            if (op.IsUndocumented) {
                return null;
            }

            // The assembler works correctly if the symbol is defined as a two-digit hex
            // value (e.g. "foo equ $80") but fails if it's four (e.g. "foo equ $0080").  We
            // output symbols with minimal digits, but this doesn't help if the code itself
            // lives on zero page.  If the operand is a reference to a zero-page user label,
            // we need to output the instruction as hex.
            // More info: https://github.com/apple2accumulator/merlin32/issues/8
            if (op == OpDef.OpPEI_StackDPInd ||
                    op == OpDef.OpSTY_DPIndexX ||
                    op == OpDef.OpSTX_DPIndexY ||
                    op.AddrMode == OpDef.AddressMode.DPIndLong ||
                    op.AddrMode == OpDef.AddressMode.DPInd ||
                    op.AddrMode == OpDef.AddressMode.DPIndexXInd) {
                FormatDescriptor dfd = Project.GetAnattrib(offset).DataDescriptor;
                if (dfd != null && dfd.HasSymbol) {
                    // It has a symbol.  See if the symbol target is a label (auto or user).
                    if (Project.SymbolTable.TryGetValue(dfd.SymbolRef.Label, out Symbol sym)) {
                        if (sym.IsInternalLabel) {
                            return null;
                        }
                    }
                }
            }

            return string.Empty;
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
        public void OutputAsmConfig() {
            // nothing to do
        }

        // IGenerator
        public void OutputEquDirective(string name, string valueStr, string comment) {
            OutputLine(name, SourceFormatter.FormatPseudoOp(sDataOpNames.EquDirective),
                valueStr, SourceFormatter.FormatEolComment(comment));
        }

        // IGenerator
        public void OutputOrgDirective(int offset, int address) {
            OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(sDataOpNames.OrgDirective),
                SourceFormatter.FormatHexValue(address, 4), string.Empty);
        }

        // IGenerator
        public void OutputRegWidthDirective(int offset, int prevM, int prevX, int newM, int newX) {
            // prevM/prevX may be ambiguous for offset 0, but otherwise everything
            // should be either 0 or 1.
            Debug.Assert(newM == 0 || newM == 1);
            Debug.Assert(newX == 0 || newX == 1);

            if (offset == 0 && newM == 1 && newX == 1) {
                // Assembler defaults to short regs, so we can skip this.
                return;
            }
            OutputLine(string.Empty,
                SourceFormatter.FormatPseudoOp(sDataOpNames.RegWidthDirective),
                "%" + newM + newX, string.Empty);
        }

        // IGenerator
        public void OutputLine(string fullLine) {
            mOutStream.WriteLine(fullLine);
        }

        // IGenerator
        public void OutputLine(string label, string opcode, string operand, string comment) {
            // Split long label, but not on EQU directives (confuses the assembler).
            if (mLongLabelNewLine && label.Length >= mColumnWidths[0] &&
                    !string.Equals(opcode, sDataOpNames.EquDirective,
                        StringComparison.InvariantCultureIgnoreCase)) {
                mOutStream.WriteLine(label);
                label = string.Empty;
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
            // This gets complicated.
            //
            // For Dci, L8String, and L16String, the entire string needs to fit in the
            // operand of one line.  If it can't, we need to separate the length byte/word
            // or inverted character out, and just dump the rest as ASCII.  Computing the
            // line length requires factoring delimiter character escapes.  (NOTE: contrary
            // to the documentation, STR and STRL do include trailing hex characters in the
            // length calculation, so it's possible to escape delimiters.)
            //
            // For Reverse, we can span lines, but only if we emit the lines in
            // backward order.  Also, Merlin doesn't allow hex to be embedded in a REV
            // operation, so we can't use REV if the string contains a delimiter.
            //
            // For aesthetic purposes, zero-length CString, L8String, and L16String
            // should be output as DFB/DW zeroes rather than an empty string -- makes
            // it easier to read.
            //
            // NOTE: we generally assume that the input is in the correct format, e.g.
            // the length byte in a StringL8 matches dfd.Length, and the high bits in DCI strings
            // have the right pattern.  If not, we will generate bad output.  This would need
            // to be scanned and corrected at a higher level.

            Formatter formatter = SourceFormatter;
            byte[] data = Project.FileData;
            Anattrib attr = Project.GetAnattrib(offset);
            FormatDescriptor dfd = attr.DataDescriptor;
            Debug.Assert(dfd != null);
            Debug.Assert(dfd.IsString);
            Debug.Assert(dfd.Length > 0);

            bool highAscii = false;
            bool reverse = false;
            int leadingBytes = 0;
            string opcodeStr;

            switch (dfd.FormatType) {
                case FormatDescriptor.Type.StringGeneric:
                    opcodeStr = sDataOpNames.StrGeneric;
                    highAscii = (data[offset] & 0x80) != 0;
                    break;
                case FormatDescriptor.Type.StringReverse:
                    opcodeStr = sDataOpNames.StrReverse;
                    highAscii = (data[offset] & 0x80) != 0;
                    reverse = true;
                    break;
                case FormatDescriptor.Type.StringNullTerm:
                    opcodeStr = sDataOpNames.StrGeneric;        // no pseudo-op for this
                    highAscii = (data[offset] & 0x80) != 0;
                    if (dfd.Length == 1) {
                        // Empty string.  Just output the length byte(s) or null terminator.
                        GenerateShortSequence(offset, 1, out string opcode, out string operand);
                        OutputLine(labelStr, opcode, operand, commentStr);
                        return;
                    }
                    break;
                case FormatDescriptor.Type.StringL8:
                    opcodeStr = sDataOpNames.StrLen8;
                    if (dfd.Length > 1) {
                        highAscii = (data[offset + 1] & 0x80) != 0;
                    }
                    leadingBytes = 1;
                    break;
                case FormatDescriptor.Type.StringL16:
                    opcodeStr = sDataOpNames.StrLen16;
                    if (dfd.Length > 2) {
                        highAscii = (data[offset + 2] & 0x80) != 0;
                    }
                    leadingBytes = 2;
                    break;
                case FormatDescriptor.Type.StringDci:
                    opcodeStr = sDataOpNames.StrDci;
                    highAscii = (data[offset] & 0x80) != 0;
                    break;
                default:
                    Debug.Assert(false);
                    return;
            }

            // Merlin 32 uses single-quote for low ASCII, double-quote for high ASCII.  When
            // quoting the delimiter we use a hexadecimal value.  We need to bear in mind that
            // we're forcing the characters to low ASCII, but the actual character being
            // escaped might be in high ASCII.  Hence delim vs. delimReplace.
            char delim = highAscii ? '"' : '\'';
            CharEncoding.Convert charConv;
            if (highAscii) {
                charConv = CharEncoding.ConvertHighAscii;
            } else {
                charConv = CharEncoding.ConvertLowAscii;
            }

            StringOpFormatter stropf = new StringOpFormatter(SourceFormatter, delim,
                StringOpFormatter.RawOutputStyle.DenseHex, MAX_OPERAND_LEN, charConv);
            if (dfd.FormatType == FormatDescriptor.Type.StringDci) {
                // DCI is awkward because the character encoding flips on the last byte.  Rather
                // than clutter up StringOpFormatter for this rare item, we just accept both
                // throughout.
                stropf.CharConv = CharEncoding.ConvertLowAndHighAscii;
            }

            // Feed bytes in, skipping over the leading length bytes.
            stropf.FeedBytes(data, offset + leadingBytes,
                dfd.Length - leadingBytes, 0, reverse);
            Debug.Assert(stropf.Lines.Count > 0);

            // See if we need to do this over.
            bool redo = false;
            switch (dfd.FormatType) {
                case FormatDescriptor.Type.StringGeneric:
                case FormatDescriptor.Type.StringNullTerm:
                    break;
                case FormatDescriptor.Type.StringReverse:
                    if (stropf.HasEscapedText) {
                        // can't include escaped characters in REV
                        opcodeStr = sDataOpNames.StrGeneric;
                        reverse = false;
                        redo = true;
                    }
                    break;
                case FormatDescriptor.Type.StringL8:
                    if (stropf.Lines.Count != 1) {
                        // single-line only
                        opcodeStr = sDataOpNames.StrGeneric;
                        leadingBytes = 1;
                        redo = true;
                    }
                    break;
                case FormatDescriptor.Type.StringL16:
                    if (stropf.Lines.Count != 1) {
                        // single-line only
                        opcodeStr = sDataOpNames.StrGeneric;
                        leadingBytes = 2;
                        redo = true;
                    }
                    break;
                case FormatDescriptor.Type.StringDci:
                    if (stropf.Lines.Count != 1) {
                        // single-line only
                        opcodeStr = sDataOpNames.StrGeneric;
                        stropf.CharConv = charConv;
                        redo = true;
                    }
                    break;
                default:
                    Debug.Assert(false);
                    return;
            }

            if (redo) {
                //Debug.WriteLine("REDO off=+" + offset.ToString("x6") + ": " + dfd.FormatType);

                // This time, instead of skipping over leading length bytes, we include them
                // explicitly.
                stropf.Reset();
                stropf.FeedBytes(data, offset, dfd.Length, leadingBytes, reverse);
            }

            opcodeStr = formatter.FormatPseudoOp(opcodeStr);

            foreach (string str in stropf.Lines) {
                OutputLine(labelStr, opcodeStr, str, commentStr);
                labelStr = commentStr = string.Empty;       // only show on first
            }
        }
    }

    #endregion IGenerator


    #region IAssembler

    /// <summary>
    /// Cross-assembler execution interface.
    /// </summary>
    public class AsmMerlin32 : IAssembler {
        // Paths from generator.
        private List<string> mPathNames;

        // Directory to make current before executing assembler.
        private string mWorkDirectory;


        // IAssembler
        public void GetExeIdentifiers(out string humanName, out string exeName) {
            humanName = "Merlin Assembler";
            exeName = "Merlin32";
        }

        // IAssembler
        public AssemblerConfig GetDefaultConfig() {
            return new AssemblerConfig(string.Empty, new int[] { 9, 6, 11, 74 });
        }

        // IAssembler
        public AssemblerVersion QueryVersion() {
            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Merlin32);
            if (config == null || string.IsNullOrEmpty(config.ExecutablePath)) {
                return null;
            }

            ShellCommand cmd = new ShellCommand(config.ExecutablePath, string.Empty,
                Directory.GetCurrentDirectory(), null);
            cmd.Execute();
            if (string.IsNullOrEmpty(cmd.Stdout)) {
                return null;
            }

            // Stdout: "C:\Src\WorkBench\Merlin32.exe v 1.0, (c) Brutal Deluxe ..."
            // Other platforms may not have the ".exe".  Find first occurrence of " v ".

            const string PREFIX = " v ";    // not expecting this to appear in the path
            string str = cmd.Stdout;
            int start = str.IndexOf(PREFIX);
            int end = (start < 0) ? -1 : str.IndexOf(',', start);

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
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Merlin32);
            if (string.IsNullOrEmpty(config.ExecutablePath)) {
                Debug.WriteLine("Assembler not configured");
                return null;
            }

            worker.ReportProgress(0, Res.Strings.PROGRESS_ASSEMBLING);

            // Wrap pathname in quotes in case it has spaces.
            // (Do we need to shell-escape quotes in the pathName?)
            //
            // Merlin 32 has no options.  The second argument is the macro include file path.
            ShellCommand cmd = new ShellCommand(config.ExecutablePath, ". \"" + pathName + "\"",
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
