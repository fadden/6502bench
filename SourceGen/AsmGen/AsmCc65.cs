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
    /// Generate source code compatible with the cc65 assembler (https://github.com/cc65/cc65).
    /// </summary>
    public class GenCc65 : IGenerator {
        private const string ASM_FILE_SUFFIX = "_cc65.S";       // must start with underscore
        private const string CFG_FILE_SUFFIX = "_cc65.cfg";     // ditto
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
        /// The first time we output a high-ASCII string, we generate a macro for it.
        /// </summary>
        private bool mHighAsciiMacroOutput;

        /// <summary>
        /// Holds detected version of configured assembler.
        /// </summary>
        private CommonUtil.Version mAsmVersion = CommonUtil.Version.NO_VERSION;

        // Interesting versions.
        private static CommonUtil.Version V2_17 = new CommonUtil.Version(2, 17);
        private static CommonUtil.Version V2_18 = new CommonUtil.Version(2, 18);


        // Pseudo-op string constants.
        private static PseudoOp.PseudoOpNames sDataOpNames =
            new PseudoOp.PseudoOpNames(new Dictionary<string, string> {
                { "EquDirective", "=" },
                { "VarDirective", ".set" },
                { "OrgDirective", ".org" },
                //RegWidthDirective         // .a8, .a16, .i8, .i16
                { "DefineData1", ".byte" },
                { "DefineData2", ".word" },
                { "DefineData3", ".faraddr" },
                { "DefineData4", ".dword" },
                { "DefineBigData2", ".dbyt" },
                //DefineBigData3
                //DefineBigData4
                { "Fill", ".res" },
                //Dense                     // no equivalent, use .byte with comma-separated args
                //Junk
                { "StrGeneric", ".byte" },
                //StrReverse
                { "StrNullTerm", ".asciiz" },
                //StrLen8                   // macro with .strlen?
                //StrLen16
                //StrDci
        });


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
            if (asmVersion != null) {
                // Use the actual version.  If it's > 2.17 we'll try to take advantage of
                // bug fixes.
                mAsmVersion = asmVersion.Version;
            } else {
                // No assembler installed.  Use 2.18.
                mAsmVersion = V2_18;
            }

            if (mAsmVersion <= V2_17) {
                // cc65 v2.17: https://github.com/cc65/cc65/issues/717
                // see also https://github.com/cc65/cc65/issues/926
                Quirks.BlockMoveArgsReversed = true;
            }

            // cc65 v2.17: https://github.com/cc65/cc65/issues/754
            // still broken in v2.18
            Quirks.NoPcRelBankWrap = true;

            // Special handling for forward references to zero-page labels is required.
            Quirks.SinglePassAssembler = true;

            mWorkDirectory = workDirectory;
            mFileNameBase = fileNameBase;
            Settings = settings;

            mLongLabelNewLine = Settings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);

            AssemblerConfig config = AssemblerConfig.GetConfig(settings,
                AssemblerInfo.Id.Cc65);
            mColumnWidths = (int[])config.ColumnWidths.Clone();
        }

        /// <summary>
        /// Configures the assembler-specific format items.
        /// </summary>
        private void SetFormatConfigValues(ref Formatter.FormatConfig config) {
            config.mForceDirectOpcodeSuffix = string.Empty;
            config.mForceAbsOpcodeSuffix = string.Empty;
            config.mForceLongOpcodeSuffix = string.Empty;
            config.mForceDirectOperandPrefix = "z:";    // zero
            config.mForceAbsOperandPrefix = "a:";       // absolute
            config.mForceLongOperandPrefix = "f:";      // far
            config.mEndOfLineCommentDelimiter = ";";
            config.mFullLineCommentDelimiterBase = ";";
            config.mBoxLineCommentDelimiter = ";";
            config.mExpressionMode = Formatter.FormatConfig.ExpressionMode.Cc65;

            Formatter.DelimiterSet charSet = new Formatter.DelimiterSet();
            charSet.Set(CharEncoding.Encoding.Ascii, Formatter.SINGLE_QUOTE_DELIM);
            charSet.Set(CharEncoding.Encoding.HighAscii,
                new Formatter.DelimiterDef(string.Empty, '\'', '\'', " | $80"));
            config.mCharDelimiters = charSet;
        }

        // IGenerator
        public List<string> GenerateSource(BackgroundWorker worker) {
            List<string> pathNames = new List<string>(1);

            string pathName = Path.Combine(mWorkDirectory, mFileNameBase + ASM_FILE_SUFFIX);
            pathNames.Add(pathName);
            string cfgName = Path.Combine(mWorkDirectory, mFileNameBase + CFG_FILE_SUFFIX);
            pathNames.Add(cfgName);

            Formatter.FormatConfig config = new Formatter.FormatConfig();
            GenCommon.ConfigureFormatterFromSettings(Settings, ref config);
            SetFormatConfigValues(ref config);
            SourceFormatter = new Formatter(config);

            string msg = string.Format(Res.Strings.PROGRESS_GENERATING_FMT, pathName);
            worker.ReportProgress(0, msg);

            mLocalizer = new LabelLocalizer(Project);
            if (!Settings.GetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, false)) {
                mLocalizer.LocalPrefix = "@";
                mLocalizer.QuirkVariablesEndScope = true;
                mLocalizer.Analyze();
            }
            mLocalizer.FixOpcodeLabels();

            // Use UTF-8 encoding, without a byte-order mark.
            using (StreamWriter sw = new StreamWriter(cfgName, false, new UTF8Encoding(false))) {
                GenerateLinkerScript(sw);
            }
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

                    OutputLine(SourceFormatter.FullLineCommentDelimiter +
                        string.Format(Res.Strings.GENERATED_FOR_VERSION_FMT,
                        "cc65", V2_18,
                        AsmCc65.OPTIONS + " -C " + Path.GetFileName(cfgName)));
                }

                GenCommon.Generate(this, sw, worker);
            }
            mOutStream = null;

            return pathNames;
        }

        private void GenerateLinkerScript(StreamWriter sw) {
            sw.WriteLine("# 6502bench SourceGen generated linker script for " + mFileNameBase);

            sw.WriteLine("MEMORY {");
            sw.WriteLine("    MAIN: file=%O, start=%S, size=65536;");
            for (int i = 0; i < Project.AddrMap.Count; i++) {
                AddressMap.AddressMapEntry ame = Project.AddrMap[i];
                sw.WriteLine(string.Format("#    MEM{0:D3}: file=%O, start=${1:x4}, size={2};",
                    i, ame.Addr, ame.Length));
            }
            sw.WriteLine("}");

            sw.WriteLine("SEGMENTS {");
            sw.WriteLine("    CODE: load=MAIN, type=rw;");
            for (int i = 0; i < Project.AddrMap.Count; i++) {
                sw.WriteLine(string.Format("#    SEG{0:D3}: load=MEM{0:D3}, type=rw;", i));
            }
            sw.WriteLine("}");

            sw.WriteLine("FEATURES {}");
            sw.WriteLine("SYMBOLS {}");
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
        /// Map the undocumented opcodes to the cc65 mnemonics.  There's almost no difference
        /// vs. the Unintended Opcodes mnemonics.
        /// 
        /// We don't include the double- and triple-byte NOPs here, as cc65 doesn't
        /// appear to have a definition for them (as of 2.17).  We also omit the alias
        /// for SBC.  These will all be output as hex.
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
        public string ModifyOpcode(int offset, OpDef op) {
            if (op == OpDef.OpBRK_StackInt || (op == OpDef.OpWDM_WDM && mAsmVersion < V2_18)) {
                // cc65 v2.17 doesn't support WDM, and assembles BRK <arg> to opcode $05.
                // cc65 v2.18 only supports two-byte BRK on 65816 code.
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
                // Unmapped values include DOP, TOP, and the alternate SBC.  Output hex.
                return null;
            } else {
                return string.Empty;
            }
        }

        // IGenerator
        public FormatDescriptor ModifyInstructionOperandFormat(int offset, FormatDescriptor dfd,
                int operand) {
            return dfd;
        }

        // IGenerator
        public void UpdateCharacterEncoding(FormatDescriptor dfd) { }

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
                    if ((string.IsNullOrEmpty(opcodeStr))) {
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
                case FormatDescriptor.Type.Junk:
                    // The ca65 .align directive has a dependency on the alignment of the
                    // segment as a whole.  We're not currently declaring multiple segments,
                    // so we can't use .align without generating complaints.
                    int fillVal = Helper.CheckRangeHoldsSingleValue(data, offset, length);
                    if (fillVal >= 0) {
                        // treat same as Fill
                        opcodeStr = sDataOpNames.Fill;
                        operandStr = length + "," + formatter.FormatHexValue(fillVal, 2);
                    } else {
                        // treat same as Dense
                        multiLine = true;
                        opcodeStr = operandStr = null;
                        OutputDenseHex(offset, length, labelStr, commentStr);
                    }
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

            if (singleValue && length > 1) {
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
        public void OutputLocalVariableTable(int offset, List<DefSymbol> newDefs,
                LocalVariableTable allDefs) {
            foreach (DefSymbol defSym in newDefs) {
                // Use an operand length of 1 so values are shown as concisely as possible.
                string valueStr = PseudoOp.FormatNumericOperand(SourceFormatter,
                    Project.SymbolTable, null, defSym.DataDescriptor, defSym.Value, 1,
                    PseudoOp.FormatNumericOpFlags.None);
                OutputLine(SourceFormatter.FormatVariableLabel(defSym.Label),
                    SourceFormatter.FormatPseudoOp(sDataOpNames.VarDirective),
                    valueStr, SourceFormatter.FormatEolComment(defSym.Comment));
            }
        }

        // IGenerator
        public void OutputOrgDirective(int offset, int address) {
            // Linear search for offset.  List should be small, so this should be quick.
            int index = 0;
            foreach (AddressMap.AddressMapEntry ame in Project.AddrMap) {
                if (ame.Offset == offset) {
                    break;
                }
                index++;
            }

            mLineBuilder.Clear();
            TextUtil.AppendPaddedString(mLineBuilder, ";", 0);
            // using +1 to make it look like the comment ';' shifted it over
            TextUtil.AppendPaddedString(mLineBuilder, SourceFormatter.FormatPseudoOp(".segment"),
                mColumnWidths[0] + 1);
            TextUtil.AppendPaddedString(mLineBuilder, string.Format("\"SEG{0:D3}\"", index),
                mColumnWidths[0] + mColumnWidths[1] + 1);
            OutputLine(mLineBuilder.ToString());

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
                    !string.Equals(opcode, sDataOpNames.EquDirective,
                        StringComparison.InvariantCultureIgnoreCase) &&
                    !string.Equals(opcode, sDataOpNames.VarDirective,
                        StringComparison.InvariantCultureIgnoreCase)) {
                label += ':';

                if (mLongLabelNewLine && label.Length >= mColumnWidths[0]) {
                    mOutStream.WriteLine(label);
                    label = string.Empty;
                }
            }

            mLineBuilder.Clear();
            TextUtil.AppendPaddedString(mLineBuilder, label, 0);
            TextUtil.AppendPaddedString(mLineBuilder, opcode, mColumnWidths[0]);
            TextUtil.AppendPaddedString(mLineBuilder, operand,
                mColumnWidths[0] + mColumnWidths[1]);
            TextUtil.AppendPaddedString(mLineBuilder, comment,
                mColumnWidths[0] + mColumnWidths[1] + mColumnWidths[2]);

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

            Anattrib attr = Project.GetAnattrib(offset);
            FormatDescriptor dfd = attr.DataDescriptor;
            Debug.Assert(dfd != null);
            Debug.Assert(dfd.IsString);
            Debug.Assert(dfd.Length > 0);

            CharEncoding.Convert charConv;
            bool isHighAscii = false;
            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.Ascii:
                    charConv = CharEncoding.ConvertAscii;
                    break;
                case FormatDescriptor.SubType.HighAscii:
                    if (dfd.FormatType != FormatDescriptor.Type.StringGeneric) {
                        OutputNoJoy(offset, dfd.Length, labelStr, commentStr);
                        return;
                    }
                    charConv = CharEncoding.ConvertHighAscii;
                    isHighAscii = true;
                    break;
                case FormatDescriptor.SubType.C64Petscii:
                case FormatDescriptor.SubType.C64Screen:
                default:
                    OutputNoJoy(offset, dfd.Length, labelStr, commentStr);
                    return;
            }

            Formatter formatter = SourceFormatter;
            byte[] data = Project.FileData;
            int leadingBytes = 0;
            int trailingBytes = 0;

            switch (dfd.FormatType) {
                case FormatDescriptor.Type.StringGeneric:
                case FormatDescriptor.Type.StringReverse:
                case FormatDescriptor.Type.StringDci:
                    break;
                case FormatDescriptor.Type.StringNullTerm:
                    trailingBytes = 1;
                    break;
                case FormatDescriptor.Type.StringL8:
                    leadingBytes = 1;
                    break;
                case FormatDescriptor.Type.StringL16:
                    leadingBytes = 2;
                    break;
                default:
                    Debug.Assert(false);
                    return;
            }

            StringOpFormatter stropf = new StringOpFormatter(SourceFormatter,
                Formatter.DOUBLE_QUOTE_DELIM, StringOpFormatter.RawOutputStyle.CommaSep,
                MAX_OPERAND_LEN, charConv);
            stropf.FeedBytes(data, offset, dfd.Length - trailingBytes, leadingBytes,
                StringOpFormatter.ReverseMode.Forward);

            string opcodeStr = formatter.FormatPseudoOp(sDataOpNames.StrGeneric);

            if (isHighAscii) {
                // Does this fit the narrow definition of what we can do with a macro?
                Debug.Assert(dfd.FormatType == FormatDescriptor.Type.StringGeneric);
                if (stropf.Lines.Count == 1 && !stropf.HasEscapedText) {
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
                } else {
                    // didn't work out, dump hex
                    OutputNoJoy(offset, dfd.Length, labelStr, commentStr);
                    return;
                }
            }

            if (dfd.FormatType == FormatDescriptor.Type.StringNullTerm) {
                if (stropf.Lines.Count == 1 && !stropf.HasEscapedText) {
                    // Keep it.
                    opcodeStr = sDataOpNames.StrNullTerm;
                } else {
                    // Didn't fit, so re-emit it, this time with the terminating null byte.
                    stropf.Reset();
                    stropf.FeedBytes(data, offset, dfd.Length, leadingBytes,
                        StringOpFormatter.ReverseMode.Forward);
                }
            }

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
    public class AsmCc65 : IAssembler {
        // Fixed options.  "--target none" is needed to neutralize the character encoding,
        // which seems to default to PETSCII.
        public const string OPTIONS = "--target none";

        // Paths from generator.
        private List<string> mPathNames;

        // Directory to make current before executing assembler.
        private string mWorkDirectory;


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
            mPathNames = new List<string>(pathNames.Count);
            foreach (string str in pathNames) {
                mPathNames.Add(str);
            }

            mWorkDirectory = workDirectory;
        }

        // IAssembler
        public AssemblerResults RunAssembler(BackgroundWorker worker) {
            Debug.Assert(mPathNames.Count == 2);
            string pathName = StripWorkDirectory(mPathNames[0]);
            string cfgName = StripWorkDirectory(mPathNames[1]);

            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, AssemblerInfo.Id.Cc65);
            if (string.IsNullOrEmpty(config.ExecutablePath)) {
                Debug.WriteLine("Assembler not configured");
                return null;
            }

            string cfgOpt = " -C \"" + cfgName + "\"";

            worker.ReportProgress(0, Res.Strings.PROGRESS_ASSEMBLING);

            // Wrap pathname in quotes in case it has spaces.
            // (Do we need to shell-escape quotes in the pathName?)
            ShellCommand cmd = new ShellCommand(config.ExecutablePath,
                OPTIONS + cfgOpt + " \"" + pathName + "\"", mWorkDirectory, null);
            cmd.Execute();

            // Can't really do anything with a "cancel" request.

            // Output filename is the input filename without the ".S".  Since the filename
            // was generated by us we can be confident in the format.
            string outputFile = mPathNames[0].Substring(0, mPathNames[0].Length - 2);

            return new AssemblerResults(cmd.FullCommandLine, cmd.ExitCode, cmd.Stdout,
                cmd.Stderr, outputFile);
        }

        /// <summary>
        /// Reduce input file to a partial path if possible.  This is just to make
        /// what we display to the user a little easier to read.
        /// </summary>
        /// <param name="pathName">Full pathname of file.</param>
        /// <returns>Pathname with working directory prefix stripped off.</returns>
        private string StripWorkDirectory(string pathName) {
            if (pathName.StartsWith(mWorkDirectory)) {
                return pathName.Remove(0, mWorkDirectory.Length + 1);
            } else {
                // Unexpected, but shouldn't be a problem.
                Debug.WriteLine("NOTE: source file is not in work directory");
                return pathName;
            }
        }
    }

    #endregion IAssembler
}
