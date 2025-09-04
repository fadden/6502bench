﻿/*
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
        private const string ASCII_ENC_NAME = "sg_ascii";
        private const string HIGH_ASCII_ENC_NAME = "sg_hiascii";

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

        // IGenerator
        public int StartOffset {
            get {
                return mHasPrgHeader ? 2 : 0;
            }
        }

        /// <summary>
        /// List of binary include sections found in the project.
        /// </summary>
        private List<BinaryInclude.Excision> mBinaryIncludes = new List<BinaryInclude.Excision>();

        /// <summary>
        /// Working directory, i.e. where we write our output file(s).
        /// </summary>
        private string mWorkDirectory;

        /// <summary>
        /// Influences whether labels are put on their own line.
        /// </summary>
        private GenCommon.LabelPlacement mLabelNewLine;

        /// <summary>
        /// Output column widths.
        /// </summary>
        private int[] mColumnWidths;

        /// <summary>
        /// Base filename.  Typically the project file name without the ".dis65" extension.
        /// </summary>
        private string mFileNameBase;

        /// <summary>
        /// True if the first two bytes look like the header of a PRG file.
        /// </summary>
        private bool mHasPrgHeader;

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
        /// What encoding are we currently set up for.
        /// </summary>
        private CharEncoding.Encoding mCurrentEncoding;

        /// <summary>
        /// True if we defined macros for big-endian numeric values.
        /// </summary>
        private bool mBigEndianMacrosDefined;
        private const string BIG_ENDIAN_16_MACRO = "bigendian";

        /// <summary>
        /// Output mode; determines how ORG is handled.
        /// </summary>
        private enum OutputMode {
            Unknown = 0, Loadable = 1, Streamable = 2
        }
        private OutputMode mOutputMode;

        /// <summary>
        /// Current pseudo-PC depth.  0 is the "real" PC.
        /// </summary>
        private int mPcDepth;
        private bool mFirstIsOpen;

        /// <summary>
        /// Holds detected version of configured assembler.
        /// </summary>
        private CommonUtil.Version mAsmVersion = CommonUtil.Version.NO_VERSION;

        // Version we're coded against.
        private static CommonUtil.Version V1_53 = new CommonUtil.Version(1, 53, 1515);
        private static CommonUtil.Version V1_54 = new CommonUtil.Version(1, 54, 1900);
        private static CommonUtil.Version V1_55 = new CommonUtil.Version(1, 55, 2176);
        private static CommonUtil.Version V1_56 = new CommonUtil.Version(1, 56, 2625);

        // Pseudo-op string constants.
        private static PseudoOp.PseudoOpNames sDataOpNames =
            new PseudoOp.PseudoOpNames(new Dictionary<string, string> {
                { "EquDirective", "=" },
                { "VarDirective", ".var" },
                { "ArStartDirective", ".logical" },
                { "ArEndDirective", ".here" },
                //RegWidthDirective         // .as, .al, .xs, .xl
                //DataBankDirective
                { "DefineData1", ".byte" },
                { "DefineData2", ".word" },
                { "DefineData3", ".long" },
                { "DefineData4", ".dword" },
                //DefineBigData2
                //DefineBigData3
                //DefineBigData4
                { "Fill", ".fill" },
                { "Dense", ".byte" },       // not really dense, just comma-separated bytes
                { "Uninit", ".fill" },
                //Junk
                { "Align", ".align" },
                { "BinaryInclude", ".binary" },
                { "StrGeneric", ".text" },
                //StrReverse
                { "StrNullTerm", ".null" },
                { "StrLen8", ".ptext" },
                //StrLen16
                { "StrDci", ".shift" }
        });

        private const string MACRO_DIRECTIVE = ".macro";


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
                mAsmVersion = asmVersion.Version;       // Use the actual version.
            } else {
                mAsmVersion = V1_56;                    // No assembler installed, use default.
            }

            Quirks.StackIntOperandIsImmediate = true;
            Quirks.LeadingUnderscoreSpecial = true;
            Quirks.Need24BitsForAbsPBR = true;
            Quirks.BitNumberIsArg = true;
            Quirks.BankZeroAbsPBRRestrict = true;

            mWorkDirectory = workDirectory;
            mFileNameBase = fileNameBase;
            Settings = settings;

            mLabelNewLine = Settings.GetEnum(AppSettings.SRCGEN_LABEL_NEW_LINE,
                    GenCommon.LabelPlacement.SplitIfTooLong);

            AssemblerConfig config = AssemblerConfig.GetConfig(settings,
                AssemblerInfo.Id.Tass64);
            mColumnWidths = (int[])config.ColumnWidths.Clone();

            // 64tass emulates a loader on a 64K system.  The address you specify with
            // "* = <addr>" tells the loader where the code lives.  If the project runs off the
            // end of memory, you get a warning message and an output file that has the last
            // part as the first part, because the loader wraps around.
            //
            // If (start_addr + total_len) doesn't fit without wrapping, we want to start
            // the code with "* = 0" (or omit it entirely) and use ".logical" for the first.
            // chunk.  This allows us to generate the full 64K.  Note that 65816 code that
            // starts outside bank 0 will always fail this test.
            //
            // Thus there are two modes: "loadable" and "streamable".  We could output everything
            // as streamable but that's kind of ugly and prevents the PRG optimization.
            //
            // If the file has more than 64K of data in it, we need to add "--long-address" to
            // the command-line arguments.

            // Get start address.  If this is a PRG file, the start address is the address
            // of offset +000002.
            bool hasPrgHeader = GenCommon.HasPrgHeader(project);
            int offAdj = hasPrgHeader ? 2 : 0;
            int startAddr = project.AddrMap.OffsetToAddress(offAdj);
            if (startAddr + project.FileDataLength - offAdj > 65536) {
                // Does not fit into memory at load address.
                mOutputMode = OutputMode.Streamable;
                mHasPrgHeader = false;
            } else {
                mOutputMode = OutputMode.Loadable;
                mHasPrgHeader = hasPrgHeader;
            }
            //Debug.WriteLine("startAddr=$" + startAddr.ToString("x6") +
            //    " outputMode=" + mOutputMode + " hasPrg=" + mHasPrgHeader);
        }

        /// <summary>
        /// Configures the assembler-specific format items.  May be called without a Project.
        /// </summary>
        private void SetFormatConfigValues(ref Formatter.FormatConfig config) {
            // Must be lower case when --case-sensitive is used.
            config.UpperOpcodes = false;
            config.UpperPseudoOpcodes = false;
            config.UpperOperandA = false;
            config.UpperOperandS = false;
            config.UpperOperandXY = false;
            config.OperandWrapLen = 64;

            config.BankSelectBackQuote = true;

            config.ForceDirectOpcodeSuffix = string.Empty;
            config.ForceAbsOpcodeSuffix = string.Empty;
            config.ForceLongOpcodeSuffix = string.Empty;
            config.ForceDirectOperandPrefix = string.Empty;
            config.ForceAbsOperandPrefix = "@w";       // word
            config.ForceLongOperandPrefix = "@l";      // long
            config.EndOfLineCommentDelimiter = ";";
            config.FullLineCommentDelimiterBase = ";";
            config.NonUniqueLabelPrefix = "";      // should be '_', but that's a valid label char
            config.CommaSeparatedDense = true;
            config.ExprMode = Formatter.FormatConfig.ExpressionMode.Common;
        }

        // IGenerator
        public GenerationResults GenerateSource(BackgroundWorker worker) {
            List<string> pathNames = new List<string>(1);

            string fileName = mFileNameBase + ASM_FILE_SUFFIX;
            string pathName = Path.Combine(mWorkDirectory, fileName);
            pathNames.Add(pathName);

            Formatter.FormatConfig config = new Formatter.FormatConfig();
            GenCommon.ConfigureFormatterFromSettings(Settings, ref config);
            SetFormatConfigValues(ref config);

            // Configure delimiters for single-character operands.
            Formatter.DelimiterSet charDelimSet = new Formatter.DelimiterSet();
            charDelimSet.Set(CharEncoding.Encoding.C64Petscii, Formatter.SINGLE_QUOTE_DELIM);
            charDelimSet.Set(CharEncoding.Encoding.C64ScreenCode, Formatter.SINGLE_QUOTE_DELIM);
            charDelimSet.Set(CharEncoding.Encoding.Ascii, Formatter.SINGLE_QUOTE_DELIM);
            charDelimSet.Set(CharEncoding.Encoding.HighAscii,
                new Formatter.DelimiterDef(string.Empty, '\'', '\'', " | $80"));

            config.CharDelimiters = charDelimSet;

            SourceFormatter = new Formatter(config);

            string msg = string.Format(Res.Strings.PROGRESS_GENERATING_FMT, pathName);
            worker.ReportProgress(0, msg);

            mLocalizer = new LabelLocalizer(Project);
            mLocalizer.LocalPrefix = "_";
            mLocalizer.QuirkNoOpcodeMnemonics = true;
            mLocalizer.Analyze();

            bool needLongAddress = Project.FileDataLength > 65536 + (mHasPrgHeader ? 2 : 0);
            string extraOptions = string.Empty +
                (needLongAddress ? AsmTass64.LONG_ADDRESS : string.Empty) +
                (mHasPrgHeader ? string.Empty : AsmTass64.NOSTART);

            mPcDepth = 0;
            mFirstIsOpen = true;

            // Use UTF-8 encoding, without a byte-order mark.
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                mOutStream = sw;

                if (Settings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false)) {
                    OutputLine(SourceFormatter.FullLineCommentDelimiterPlus +
                        string.Format(Res.Strings.GENERATED_FOR_VERSION_FMT,
                        "64tass", mAsmVersion, AsmTass64.BASE_OPTIONS + extraOptions));
                }

                GenCommon.Generate(this, sw, worker);
            }
            mOutStream = null;

            return new GenerationResults(pathNames, extraOptions, mBinaryIncludes);
        }

        // IGenerator
        public void OutputAsmConfig() {
            CpuDef cpuDef = Project.CpuDef;
            string cpuStr;
            if (cpuDef.Type == CpuDef.CpuType.Cpu65816) {
                cpuStr = "65816";
            } else if (cpuDef.Type == CpuDef.CpuType.Cpu65C02) {
                cpuStr = "65c02";
            } else if (cpuDef.Type == CpuDef.CpuType.CpuW65C02) {
                cpuStr = "w65c02";
            } else if (cpuDef.Type == CpuDef.CpuType.Cpu6502 && cpuDef.HasUndocumented) {
                cpuStr = "6502i";
            } else {
                cpuStr = "6502";
            }

            OutputLine(string.Empty, SourceFormatter.FormatPseudoOp(".cpu"),
                '\"' + cpuStr + '\"', string.Empty);

            // C64 PETSCII and C64 screen codes are built in.  Define ASCII if we also
            // need that.
            mCurrentEncoding = CharEncoding.Encoding.C64Petscii;

            ScanFormats(out bool hasAscii, out bool hasHighAscii, out bool hasBigEndian);
            if (hasHighAscii) {
                OutputLine(string.Empty, ".enc", '"' + HIGH_ASCII_ENC_NAME + '"', string.Empty);
                OutputLine(string.Empty, ".cdef", "$20,$7e,$a0", string.Empty);
                mCurrentEncoding = CharEncoding.Encoding.HighAscii;
            }
            if (hasAscii) {
                OutputLine(string.Empty, ".enc", '"' + ASCII_ENC_NAME + '"', string.Empty);
                OutputLine(string.Empty, ".cdef", "$20,$7e,$20", string.Empty);
                mCurrentEncoding = CharEncoding.Encoding.Ascii;
            }
            if (hasBigEndian) {
                OutputLine(BIG_ENDIAN_16_MACRO, MACRO_DIRECTIVE, string.Empty, string.Empty);
                OutputLine(string.Empty, ".byte", "(\\1)>>8,(\\1)&$ff", string.Empty);
                OutputLine(string.Empty, ".endmacro", string.Empty, string.Empty);
                mBigEndianMacrosDefined = true;
            }
        }

        private void ScanFormats(out bool hasAscii, out bool hasHighAscii, out bool hasBigEndian) {
            int offset = 0;
            hasAscii = hasHighAscii = hasBigEndian = false;
            while (offset < Project.FileData.Length) {
                Anattrib attr = Project.GetAnattrib(offset);
                FormatDescriptor dfd = attr.DataDescriptor;
                if (dfd != null) {
                    if (dfd.FormatSubType == FormatDescriptor.SubType.Ascii) {
                        Debug.Assert(dfd.IsNumeric || dfd.IsString);
                        hasAscii = true;
                    } else if (dfd.FormatSubType == FormatDescriptor.SubType.HighAscii) {
                        hasHighAscii = true;
                    } else if (dfd.FormatType == FormatDescriptor.Type.NumericBE) {
                        hasBigEndian = true;
                    }
                }
                if (hasAscii && hasHighAscii && hasBigEndian) {
                    return;
                }

                if (attr.IsInstructionStart) {
                    // look for embedded instructions, which might have formatted char data
                    int len;
                    for (len = 1; len < attr.Length; len++) {
                        if (Project.GetAnattrib(offset + len).IsInstructionStart) {
                            break;
                        }
                    }
                    offset += len;
                } else {
                    // data items
                    Debug.Assert(attr.Length > 0);
                    offset += attr.Length;
                }
            }
        }

        // IGenerator
        public string ModifyOpcode(int offset, OpDef op) {
            if (op.IsUndocumented) {
                if (Project.CpuDef.Type == CpuDef.CpuType.Cpu65C02 ||
                        Project.CpuDef.Type == CpuDef.CpuType.CpuW65C02) {
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
                    if (mAsmVersion < V1_55) {
                        return null;
                    }
                }
            }
            if (op == OpDef.OpWDM_WDM) {
                // 64tass v1.53 doesn't know what this is.
                // 64tass v1.55 doesn't like this to have an operand.
                // Output as hex.
                return null;
            }
            return string.Empty;        // indicate original is fine
        }

        // IGenerator
        public FormatDescriptor ModifyInstructionOperandFormat(int offset, FormatDescriptor dfd,
                int operand) {
            return dfd;
        }

        // IGenerator
        public void UpdateCharacterEncoding(FormatDescriptor dfd) {
            CharEncoding.Encoding newEnc = PseudoOp.SubTypeToEnc(dfd.FormatSubType);
            if (newEnc == CharEncoding.Encoding.Unknown) {
                // probably not a character operand
                return;
            }
            if (newEnc != mCurrentEncoding) {
                switch (newEnc) {
                    case CharEncoding.Encoding.Ascii:
                        OutputLine(string.Empty, ".enc", '"' + ASCII_ENC_NAME + '"', string.Empty);
                        break;
                    case CharEncoding.Encoding.HighAscii:
                        // If this is a numeric operand (not string), and we're currently in
                        // ASCII mode, the "| $80" in the delimiter will handle this without
                        // the need for a .enc.  Much less clutter for sources that have plain
                        // ASCII strings but test high ASCII constants.
                        if (mCurrentEncoding == CharEncoding.Encoding.Ascii && !dfd.IsString) {
                            newEnc = mCurrentEncoding;
                        } else {
                            OutputLine(string.Empty, ".enc", '"' + HIGH_ASCII_ENC_NAME + '"',
                                string.Empty);
                        }
                        break;
                    case CharEncoding.Encoding.C64Petscii:
                        OutputLine(string.Empty, ".enc", "\"none\"", string.Empty);
                        break;
                    case CharEncoding.Encoding.C64ScreenCode:
                        OutputLine(string.Empty, ".enc", "\"screen\"", string.Empty);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                mCurrentEncoding = newEnc;
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
                labelStr = Localizer.ConvLabel(attr.Symbol.Label);
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
                    UpdateCharacterEncoding(dfd);
                    operandStr = PseudoOp.FormatNumericOperand(formatter, Project.SymbolTable,
                        Localizer.LabelMap, dfd, operand, length,
                        PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix);
                    break;
                case FormatDescriptor.Type.NumericBE:
                    opcodeStr = sDataOpNames.GetDefineBigData(length);
                    if (string.IsNullOrEmpty(opcodeStr) && length == 2) {
                        // Special handling for 16-bit big-endian operands.
                        Debug.Assert(mBigEndianMacrosDefined);
                        opcodeStr = BIG_ENDIAN_16_MACRO;
                    }
                    if (!(string.IsNullOrEmpty(opcodeStr))) {
                        UpdateCharacterEncoding(dfd);
                        operand = RawData.GetWord(data, offset, length, true);
                        operandStr = PseudoOp.FormatNumericOperand(formatter, Project.SymbolTable,
                            Localizer.LabelMap, dfd, operand, length,
                            PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix);
                    } else {
                        // Nothing defined, output as comma-separated single-byte values.
                        GenerateShortSequence(offset, length, out opcodeStr, out operandStr);
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
                case FormatDescriptor.Type.Uninit:
                    // TODO: use the special syntax for uninit byte/word/dword if possible.
                case FormatDescriptor.Type.Junk:
                    bool canAlign = (dfd.FormatType == FormatDescriptor.Type.Junk);
                    int fillVal = Helper.CheckRangeHoldsSingleValue(data, offset, length);
                    if (canAlign && fillVal >= 0 &&
                            GenCommon.CheckJunkAlign(offset, dfd, Project.AddrMap)) {
                        // .align <expression>[, <fill>]
                        opcodeStr = sDataOpNames.Align;
                        int alignVal = 1 << FormatDescriptor.AlignmentToPower(dfd.FormatSubType);
                        operandStr = alignVal.ToString() +
                            "," + formatter.FormatHexValue(fillVal, 2);
                    } else if (fillVal >= 0 && (length > 1 || fillVal == 0x00)) {
                        // If multi-byte, or single byte and zero, treat same as Fill.
                        opcodeStr = sDataOpNames.Fill;
                        operandStr = length + "," + formatter.FormatHexValue(fillVal, 2);
                    } else {
                        // treat same as Dense
                        multiLine = true;
                        opcodeStr = operandStr = null;
                        OutputDenseHex(offset, length, labelStr, commentStr);
                    }
                    break;
                case FormatDescriptor.Type.BinaryInclude:
                    opcodeStr = sDataOpNames.BinaryInclude;
                    string biPath = BinaryInclude.ConvertPathNameFromStorage(dfd.Extra);
                    operandStr = '"' + biPath + '"';
                    mBinaryIncludes.Add(new BinaryInclude.Excision(offset, length, biPath));
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
            int maxPerLine = formatter.OperandWrapLen / formatter.CharsPerDenseByte;

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
                string valueStr = PseudoOp.FormatNumericOperand(SourceFormatter,
                    Project.SymbolTable, null, defSym.DataDescriptor, defSym.Value, 1,
                    PseudoOp.FormatNumericOpFlags.OmitLabelPrefixSuffix);
                OutputLine(SourceFormatter.FormatVariableLabel(defSym.Label),
                    SourceFormatter.FormatPseudoOp(sDataOpNames.VarDirective),
                    valueStr, SourceFormatter.FormatEolComment(defSym.Comment));
            }
        }

        // IGenerator
        public void OutputArDirective(CommonUtil.AddressMap.AddressChange change) {
            // 64tass separates the "compile offset", which determines where the output fits
            // into the generated binary, and "program counter", which determines the code
            // the assembler generates.  Since we need to explicitly specify every byte in
            // the output file, having a distinct compile offset isn't useful here.  We want
            // to set it once before the first line of code, then leave it alone.
            //
            // Any subsequent ORG changes are made to the program counter, and take the form
            // of a pair of ops (".logical <addr>" to open, ".here" to end).  Omitting the .here
            // causes an error.
            //
            // If this is a "streamable" file, meaning it won't actually load into 64K of RAM
            // without wrapping around, then we skip the "* = addr" (same as "* = 0") and just
            // start with ".logical" segments.
            //
            // The assembler's approach is best represented by having an address region that
            // spans the entire file, with one or more "logical" regions inside.  In practice
            // (especially for multi-bank 65816 code) that may not be the case, but the
            // assembler is still expecting us to start with a "* =" and then fit everything
            // inside that.  So we treat the first region specially, whether or not it wraps
            // the rest of the file.
            Debug.Assert(mPcDepth >= 0);
            int nextAddress = change.Address;
            if (nextAddress == Address.NON_ADDR) {
                // Start non-addressable regions at zero to ensure they don't overflow bank.
                nextAddress = 0;
            }
            if (change.IsStart) {
                if (change.Region.HasValidPreLabel) {
                    string labelStr = mLocalizer.ConvLabel(change.Region.PreLabel);
                    OutputLine(labelStr, string.Empty, string.Empty, string.Empty);
                }
                if (mPcDepth == 0 && mFirstIsOpen) {
                    mPcDepth++;

                    // Set the "real" PC for the first address change.  If we're in "loadable"
                    // mode, just set "*=".  If we're in "streaming" mode, we set "*=" to zero
                    // and then use a pseudo-PC.
                    if (mOutputMode == OutputMode.Loadable) {
                        OutputLine("*", "=",
                            SourceFormatter.FormatHexValue(nextAddress, 4), string.Empty);
                        return;
                    } else {
                        // Set the real PC to address zero to ensure we get a full 64KB.  The
                        // assembler assumes this as a default, so it can be omitted.
                        //OutputLine("*", "=", SourceFormatter.FormatHexValue(0, 4), string.Empty);
                    }
                }

                AddressMap.AddressRegion region = change.Region;
                string addrStr;
                if (region.HasValidIsRelative) {
                    int diff = nextAddress - region.PreLabelAddress;
                    string pfxStr;
                    if (diff >= 0) {
                        pfxStr = "*+";
                    } else {
                        pfxStr = "*-";
                        diff = -diff;
                    }
                    addrStr = pfxStr + SourceFormatter.FormatHexValue(diff, 4);
                } else {
                    addrStr = SourceFormatter.FormatHexValue(nextAddress, 4);
                }
                OutputLine(string.Empty,
                    SourceFormatter.FormatPseudoOp(sDataOpNames.ArStartDirective),
                    addrStr,
                    string.Empty);
                mPcDepth++;
            } else {
                mPcDepth--;
                if (mPcDepth > 0 || !mFirstIsOpen) {
                    // close previous block
                    OutputLine(string.Empty,
                        SourceFormatter.FormatPseudoOp(sDataOpNames.ArEndDirective),
                        string.Empty, string.Empty);
                } else {
                    // mark initial "*=" region as closed, but don't output anything
                    mFirstIsOpen = false;
                }
            }
        }

        // IGenerator
        public void FlushArDirectives() { }

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
            // Break the line if the label is long and it's not a .EQ/.VAR/.MACRO directive.
            if (!string.IsNullOrEmpty(label) && !string.IsNullOrEmpty(opcode) &&
                    !string.Equals(opcode, sDataOpNames.EquDirective,
                        StringComparison.InvariantCultureIgnoreCase) &&
                    !string.Equals(opcode, sDataOpNames.VarDirective,
                        StringComparison.InvariantCultureIgnoreCase) &&
                    !string.Equals(opcode, MACRO_DIRECTIVE,
                        StringComparison.InvariantCultureIgnoreCase)) {

                if (mLabelNewLine == GenCommon.LabelPlacement.PreferSeparateLine ||
                        (mLabelNewLine == GenCommon.LabelPlacement.SplitIfTooLong &&
                            label.Length >= mColumnWidths[0])) {
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
            // Generic strings whose encoding matches the configured text encoding are output
            // with a simple .text directive.
            //
            // CString and L8String have directives (.null, .ptext), but we can only use
            // them if the string fits on one line and doesn't include delimiters.
            //
            // We might be able to define a macro for Reverse.
            //
            // We don't currently switch character encodings in the middle of a file.  We could
            // do so to flip between PETSCII, screen codes, low ASCII, and high ASCII, but it
            // adds a lot of noise and it's unclear that this is generally useful.

            Anattrib attr = Project.GetAnattrib(offset);
            FormatDescriptor dfd = attr.DataDescriptor;
            Debug.Assert(dfd != null);
            Debug.Assert(dfd.IsString);
            Debug.Assert(dfd.Length > 0);

            CharEncoding.Convert charConv = null;
            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.Ascii:
                    charConv = CharEncoding.ConvertAscii;
                    break;
                case FormatDescriptor.SubType.HighAscii:
                    charConv = CharEncoding.ConvertHighAscii;
                    break;
                case FormatDescriptor.SubType.C64Petscii:
                    charConv = CharEncoding.ConvertC64Petscii;
                    break;
                case FormatDescriptor.SubType.C64Screen:
                    charConv = CharEncoding.ConvertC64ScreenCode;
                    break;
                default:
                    break;
            }
            if (charConv == null) {
                OutputNoJoy(offset, dfd.Length, labelStr, commentStr);
                return;
            }

            // Issue a .enc, if needed.
            UpdateCharacterEncoding(dfd);

            Formatter formatter = SourceFormatter;
            byte[] data = Project.FileData;
            int hiddenLeadingBytes = 0;
            int shownLeadingBytes = 0;
            int trailingBytes = 0;
            string opcodeStr;

            switch (dfd.FormatType) {
                case FormatDescriptor.Type.StringGeneric:
                case FormatDescriptor.Type.StringReverse:
                    opcodeStr = sDataOpNames.StrGeneric;
                    break;
                case FormatDescriptor.Type.StringNullTerm:
                    opcodeStr = sDataOpNames.StrNullTerm;
                    trailingBytes = 1;
                    break;
                case FormatDescriptor.Type.StringL8:
                    opcodeStr = sDataOpNames.StrLen8;
                    hiddenLeadingBytes = 1;
                    break;
                case FormatDescriptor.Type.StringL16:
                    opcodeStr = sDataOpNames.StrGeneric;
                    shownLeadingBytes = 2;
                    break;
                case FormatDescriptor.Type.StringDci:
                    opcodeStr = sDataOpNames.StrDci;
                    if ((Project.FileData[offset + dfd.Length - 1] & 0x80) == 0) {
                        // ".shift" directive only works for strings where the low bit starts
                        // clear and ends high.
                        // TODO(maybe): this is sub-optimal for high-ASCII DCI strings.
                        OutputNoJoy(offset, dfd.Length, labelStr, commentStr);
                        return;
                    }
                    break;
                default:
                    Debug.Assert(false);
                    return;
            }

            StringOpFormatter stropf = new StringOpFormatter(SourceFormatter,
                Formatter.DOUBLE_QUOTE_DELIM,StringOpFormatter.RawOutputStyle.CommaSep, charConv,
                false);
            stropf.IsDciString = (dfd.FormatType == FormatDescriptor.Type.StringDci);

            // Feed bytes in, skipping over hidden bytes (leading L8, trailing null).
            stropf.FeedBytes(data, offset + hiddenLeadingBytes,
                dfd.Length - hiddenLeadingBytes - trailingBytes, shownLeadingBytes,
                StringOpFormatter.ReverseMode.Forward);
            Debug.Assert(stropf.Lines.Count > 0);

            // See if we need to do this over.
            bool redo = false;
            switch (dfd.FormatType) {
                case FormatDescriptor.Type.StringGeneric:
                case FormatDescriptor.Type.StringReverse:
                case FormatDescriptor.Type.StringL16:
                    // All good the first time.
                    break;
                case FormatDescriptor.Type.StringNullTerm:
                case FormatDescriptor.Type.StringL8:
                case FormatDescriptor.Type.StringDci:
                    if (stropf.Lines.Count != 1) {
                        // Must be single-line.
                        opcodeStr = sDataOpNames.StrGeneric;
                        stropf.IsDciString = false;
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
                stropf.FeedBytes(data, offset, dfd.Length, hiddenLeadingBytes,
                    StringOpFormatter.ReverseMode.Forward);
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
    public class AsmTass64 : IAssembler {
        // Standard options.  For historical reasons the assembler expects PETSCII input by
        // default, and requires "--ascii" for ASCII/UTF-8 input.  This flag switches the
        // default "none" encoding from "raw" to something that converts characters to
        // PETSCII, so if you want to output strings in another format (such as ASCII) an
        // explicit encoding must be specified.
        public const string BASE_OPTIONS = "--ascii --case-sensitive -Wall";
        public const string LONG_ADDRESS = " --long-address";
        public const string NOSTART = " --nostart";

        // Paths from generator.
        private List<string> mPathNames;

        // Directory to make current before executing assembler.
        private string mWorkDirectory;

        // Additional options specified by the source generator.
        private string mExtraOptions;


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
        public void Configure(GenerationResults results, string workDirectory) {
            // Clone path names, in case the caller decides to modify the original.
            mPathNames = CommonUtil.Container.CopyStringList(results.PathNames);
            mExtraOptions = results.ExtraOptions;
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

            worker.ReportProgress(0, Res.Strings.PROGRESS_ASSEMBLING);

            string outFileName = pathName.Substring(0, pathName.Length - 2);

            // Wrap pathname in quotes in case it has spaces.
            // (Do we need to shell-escape quotes in the pathName?)
            ShellCommand cmd = new ShellCommand(config.ExecutablePath,
                BASE_OPTIONS + mExtraOptions +
                    " \"" + pathName + "\"" + " -o \"" + outFileName + "\"",
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
