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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

using Asm65;
using CommonUtil;

namespace SourceGen {
    /// <summary>
    /// Data pseudo-op formatter.  Long operands, notably strings and dense hex blocks, may
    /// be broken across multiple lines.
    /// 
    /// Assembler output will use Opcode and Operand, emitting multiple lines of ASC, HEX,
    /// etc.  The display list may treat it as a single item that is split across
    /// multiple lines.
    /// </summary>
    public class PseudoOp {
        /// <summary>
        /// One piece of the pseudo-instruction.
        /// </summary>
        public struct PseudoOut {
            /// <summary>
            /// Opcode.  Same for all entries in the list.
            /// </summary>
            public string Opcode { get; set; }

            /// <summary>
            /// Formatted form of this piece of the operand.
            /// </summary>
            public string Operand { get; set; }

            /// <summary>
            /// Copy constructor.
            /// </summary>
            public PseudoOut(PseudoOut src) {
                Opcode = src.Opcode;
                Operand = src.Operand;
            }
        }

        #region PseudoOpNames

        /// <summary>
        /// Pseudo-op name collection.  Instances are immutable.
        /// </summary>
        public class PseudoOpNames {
            public string EquDirective { get; private set; }
            public string VarDirective { get; private set; }
            public string ArStartDirective { get; private set; }
            public string ArEndDirective { get; private set; }
            public string RegWidthDirective { get; private set; }
            public string DataBankDirective { get; private set; }

            public string DefineData1 { get; private set; }
            public string DefineData2 { get; private set; }
            public string DefineData3 { get; private set; }
            public string DefineData4 { get; private set; }
            public string DefineBigData2 { get; private set; }
            public string DefineBigData3 { get; private set; }
            public string DefineBigData4 { get; private set; }
            public string Fill { get; private set; }
            public string Dense { get; private set; }
            public string Uninit { get; private set; }
            public string Junk { get; private set; }
            public string Align { get; private set; }
            public string StrGeneric { get; private set; }
            public string StrReverse { get; private set; }
            public string StrLen8 { get; private set; }
            public string StrLen16 { get; private set; }
            public string StrNullTerm { get; private set; }
            public string StrDci { get; private set; }

            /// <summary>
            /// Constructs an empty PseudoOp, for deserialization.
            /// </summary>
            public PseudoOpNames() : this(new Dictionary<string, string>()) { }

            /// <summary>
            /// Constructor.  Pass in a dictionary with name/value pairs.  Unknown names
            /// will be ignored, missing names will be assigned the empty string.
            /// </summary>
            /// <param name="dict">Dictionary of values.</param>
            public PseudoOpNames(Dictionary<string, string> dict) {
                foreach (PropertyInfo prop in GetType().GetProperties()) {
                    dict.TryGetValue(prop.Name, out string value);
                    if (value == null) {
                        value = string.Empty;
                    }
                    prop.SetValue(this, value);
                }
            }

            public static bool operator ==(PseudoOpNames a, PseudoOpNames b) {
                if (ReferenceEquals(a, b)) {
                    return true;    // same object, or both null
                }
                if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                    return false;   // one is null
                }
                return a.EquDirective == b.EquDirective &&
                    a.VarDirective == b.VarDirective &&
                    a.ArStartDirective == b.ArStartDirective &&
                    a.ArEndDirective == b.ArEndDirective &&
                    a.RegWidthDirective == b.RegWidthDirective &&
                    a.DataBankDirective == b.DataBankDirective &&
                    a.DefineData1 == b.DefineData1 &&
                    a.DefineData2 == b.DefineData2 &&
                    a.DefineData3 == b.DefineData3 &&
                    a.DefineData4 == b.DefineData4 &&
                    a.DefineBigData2 == b.DefineBigData2 &&
                    a.DefineBigData3 == b.DefineBigData3 &&
                    a.DefineBigData4 == b.DefineBigData4 &&
                    a.Fill == b.Fill &&
                    a.Dense == b.Dense &&
                    a.Uninit == b.Uninit &&
                    a.Junk == b.Junk &&
                    a.Align == b.Align &&
                    a.StrGeneric == b.StrGeneric &&
                    a.StrReverse == b.StrReverse &&
                    a.StrLen8 == b.StrLen8 &&
                    a.StrLen16 == b.StrLen16 &&
                    a.StrNullTerm == b.StrNullTerm &&
                    a.StrDci == b.StrDci;
            }
            public static bool operator !=(PseudoOpNames a, PseudoOpNames b) {
                return !(a == b);
            }
            public override bool Equals(object obj) {
                return obj is PseudoOpNames && this == (PseudoOpNames)obj;
            }
            public override int GetHashCode() {
                // should be enough
                return (EquDirective == null ? 0 : EquDirective.GetHashCode()) ^
                    (ArStartDirective == null ? 0 : ArStartDirective.GetHashCode()) ^
                    (DefineData1 == null ? 0 : DefineData1.GetHashCode()) ^
                    (Fill == null ? 0 : Fill.GetHashCode());
            }

            public string GetDefineData(int width) {
                switch (width) {
                    case 1: return DefineData1;
                    case 2: return DefineData2;
                    case 3: return DefineData3;
                    case 4: return DefineData4;
                    default: Debug.Assert(false); return ".?!!";
                }
            }
            public string GetDefineBigData(int width) {
                switch (width) {
                    case 1: return DefineData1;
                    case 2: return DefineBigData2;
                    case 3: return DefineBigData3;
                    case 4: return DefineBigData4;
                    default: Debug.Assert(false); return ".!!?";
                }
            }

            /// <summary>
            /// Merges the non-null, non-empty strings.
            /// </summary>
            public static PseudoOpNames Merge(PseudoOpNames basePon, PseudoOpNames newPon) {
                Dictionary<string, string> baseDict = PropsToDict(basePon);
                Dictionary<string, string> newDict = PropsToDict(newPon);

                foreach (KeyValuePair<string, string> kvp in newDict) {
                    if (string.IsNullOrEmpty(kvp.Value)) {
                        continue;
                    }
                    baseDict[kvp.Key] = kvp.Value;
                }

                return new PseudoOpNames(baseDict);
            }

            private static Dictionary<string, string> PropsToDict(PseudoOpNames pon) {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                foreach (PropertyInfo prop in pon.GetType().GetProperties()) {
                    string value = (string)prop.GetValue(pon);
                    if (!string.IsNullOrEmpty(value)) {
                        dict[prop.Name] = value;
                    }
                }
                return dict;
            }

            public string Serialize() {
                // This results in a JSON-encoded string being stored in a JSON-encoded file,
                // which means a lot of double-quote escaping.  We could do something here
                // that stored more nicely but it doesn't seem worth the effort.
                JavaScriptSerializer ser = new JavaScriptSerializer();
                Dictionary<string, string> dict = PropsToDict(this);
                return ser.Serialize(dict);
            }

            public static PseudoOpNames Deserialize(string cereal) {
                JavaScriptSerializer ser = new JavaScriptSerializer();
                try {
                    Dictionary<string, string> dict =
                        ser.Deserialize<Dictionary<string, string>>(cereal);
                    return new PseudoOpNames(dict);
                } catch (Exception ex) {
                    Debug.WriteLine("PseudoOpNames deserialization failed: " + ex.Message);
                    return new PseudoOpNames();
                }
            }
        }

        /// <summary>
        /// Returns a PseudoOpNames instance with some reasonable defaults for on-screen display.
        /// </summary>
        public static PseudoOpNames DefaultPseudoOpNames { get; } =
            new PseudoOpNames(new Dictionary<string, string> {
                { "EquDirective", ".eq" },
                { "VarDirective", ".var" },
                { "ArStartDirective", ".addrs" },
                { "ArEndDirective", ".adrend" },
                { "RegWidthDirective", ".rwid" },
                { "DataBankDirective", ".dbank" },

                { "DefineData1", ".dd1" },
                { "DefineData2", ".dd2" },
                { "DefineData3", ".dd3" },
                { "DefineData4", ".dd4" },
                { "DefineBigData2", ".dbd2" },
                { "DefineBigData3", ".dbd3" },
                { "DefineBigData4", ".dbd4" },
                { "Fill", ".fill" },
                { "Dense", ".bulk" },
                { "Uninit", ".ds" },
                { "Junk", ".junk" },
                { "Align", ".align" },

                { "StrGeneric", ".str" },
                { "StrReverse", ".rstr" },
                { "StrLen8", ".l1str" },
                { "StrLen16", ".l2str" },
                { "StrNullTerm", ".zstr" },
                { "StrDci", ".dstr" }
        });

        #endregion PseudoOpNames

        /// <summary>
        /// Computes the number of lines of output required to hold the formatted output.
        /// </summary>
        /// <param name="formatter">Format definition.</param>
        /// <param name="dfd">Data format descriptor.</param>
        /// <returns>Line count.</returns>
        public static int ComputeRequiredLineCount(Formatter formatter, PseudoOpNames opNames,
                FormatDescriptor dfd, byte[] data, int offset) {
            if (dfd.IsString) {
                Debug.Assert(false);        // shouldn't be calling here anymore
                List<string> lines = FormatStringOp(formatter, opNames, dfd, data,
                    offset, out string popcode);
                return lines.Count;
            }

            switch (dfd.FormatType) {
                case FormatDescriptor.Type.Default:
                case FormatDescriptor.Type.NumericLE:
                case FormatDescriptor.Type.NumericBE:
                case FormatDescriptor.Type.Fill:
                case FormatDescriptor.Type.Uninit:
                case FormatDescriptor.Type.Junk:
                    return 1;
                case FormatDescriptor.Type.Dense: {
                        // no delimiter, two output bytes per input byte
                        int maxLen = formatter.OperandWrapLen;
                        int textLen = dfd.Length * formatter.CharsPerDenseByte;
                        return (textLen + maxLen - 1) / maxLen;
                    }
                default:
                    Debug.Assert(false);
                    return 1;
            }
        }

        /// <summary>
        /// Generates a pseudo-op statement for the specified data operation.
        /// 
        /// For most operations, only one output line will be generated.  For larger items,
        /// like dense hex, the value may be split into multiple lines.  The sub-index
        /// indicates which line should be formatted.
        /// </summary>
        /// <param name="formatter">Format definition.</param>
        /// <param name="opNames">Table of pseudo-op names.</param>
        /// <param name="symbolTable">Project symbol table.</param>
        /// <param name="labelMap">Symbol label map.  May be null.</param>
        /// <param name="dfd">Data format descriptor.</param>
        /// <param name="data">File data array.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="subIndex">For multi-line items, which line.</param>
        public static PseudoOut FormatDataOp(Formatter formatter, PseudoOpNames opNames,
                SymbolTable symbolTable, Dictionary<string, string> labelMap,
                FormatDescriptor dfd, byte[] data, int offset, int subIndex) {
            if (dfd == null) {
                // should never happen
                //Debug.Assert(false, "Null dfd at offset+" + offset.ToString("x6"));
                PseudoOut failed = new PseudoOut();
                failed.Opcode = failed.Operand = "!FAILED!+" + offset.ToString("x6");
                return failed;
            }

            int length = dfd.Length;
            Debug.Assert(length > 0);

            // All outputs for a given offset show the same offset and length, even for
            // multi-line items.
            PseudoOut po = new PseudoOut();

            if (dfd.IsString) {
                Debug.Assert(false);        // shouldn't be calling here anymore
                List<string> lines = FormatStringOp(formatter, opNames, dfd, data,
                    offset, out string popcode);
                po.Opcode = popcode;
                po.Operand = lines[subIndex];
            } else {
                switch (dfd.FormatType) {
                    case FormatDescriptor.Type.Default:
                        if (length != 1) {
                            // This shouldn't happen.
                            Debug.Assert(false);
                            length = 1;
                        }
                        po.Opcode = opNames.GetDefineData(length);
                        int operand = RawData.GetWord(data, offset, length, false);
                        po.Operand = formatter.FormatHexValue(operand, length * 2);
                        break;
                    case FormatDescriptor.Type.NumericLE:
                        po.Opcode = opNames.GetDefineData(length);
                        operand = RawData.GetWord(data, offset, length, false);
                        po.Operand = FormatNumericOperand(formatter, symbolTable, labelMap,
                            dfd, operand, length, FormatNumericOpFlags.None);
                        break;
                    case FormatDescriptor.Type.NumericBE:
                        po.Opcode = opNames.GetDefineBigData(length);
                        operand = RawData.GetWord(data, offset, length, true);
                        po.Operand = FormatNumericOperand(formatter, symbolTable, labelMap,
                            dfd, operand, length, FormatNumericOpFlags.None);
                        break;
                    case FormatDescriptor.Type.Fill:
                        po.Opcode = opNames.Fill;
                        po.Operand = length + "," + formatter.FormatHexValue(data[offset], 2);
                        break;
                    case FormatDescriptor.Type.Uninit:
                        po.Opcode = opNames.Uninit;
                        po.Operand = length.ToString();
                        break;
                    case FormatDescriptor.Type.Junk:
                        if (dfd.FormatSubType != FormatDescriptor.SubType.None) {
                            po.Opcode = opNames.Align;
                            int alignPow = FormatDescriptor.AlignmentToPower(dfd.FormatSubType);
                            po.Operand = formatter.FormatHexValue(1 << alignPow, 2) +
                                " (" + length.ToString() + " bytes)";
                        } else {
                            po.Opcode = opNames.Junk;
                            po.Operand = length.ToString();
                        }
                        break;
                    case FormatDescriptor.Type.Dense: {
                            int maxPerLine = formatter.OperandWrapLen / formatter.CharsPerDenseByte;
                            offset += subIndex * maxPerLine;
                            length -= subIndex * maxPerLine;
                            if (length > maxPerLine) {
                                length = maxPerLine;
                            }
                            po.Opcode = opNames.Dense;
                            po.Operand = formatter.FormatDenseHex(data, offset, length);
                            //List<PseudoOut> outList = new List<PseudoOut>();
                            //GenerateTextLines(text, "", "", po, outList);
                            //po = outList[subIndex];
                        }
                        break;
                    default:
                        Debug.Assert(false);
                        po.Opcode = ".???";
                        po.Operand = "$" + data[offset].ToString("x2");
                        break;
                }
            }

            return po;
        }

        /// <summary>
        /// Adds an additional annotation to an EQU directive, indicating whether the symbol
        /// is a constant or an address, and (if address) how many bytes it spans.
        /// </summary>
        /// <param name="formatter">Formatter object.</param>
        /// <param name="operand">Formatted operand string.</param>
        /// <param name="defSym">Project/platform/variable symbol.</param>
        /// <returns></returns>
        public static string AnnotateEquDirective(Formatter formatter, string operand,
                DefSymbol defSym) {
            string typeStr;
            if (defSym.IsConstant) {
                if (defSym.SymbolSource == Symbol.Source.Variable) {
                    typeStr = Res.Strings.EQU_STACK_RELATIVE;
                } else {
                    typeStr = Res.Strings.EQU_CONSTANT;
                }
            } else {
                typeStr = Res.Strings.EQU_ADDRESS;
            }

            if (!defSym.HasWidth && !defSym.IsConstant) {
                // It's an address without an explicit width, do not annotate.
                return operand;
            }

            StringBuilder sb = new StringBuilder(operand.Length + typeStr.Length + 16);
            sb.Append(operand);

            int spacesNeeded = 7 - operand.Length;
            do {    // always output at least one space
                sb.Append(' ');
            } while (--spacesNeeded > 0);

            sb.Append("{");
            sb.Append(typeStr);
            if (defSym.HasWidth) {
                sb.Append('/');
                sb.Append(defSym.DataDescriptor.Length);
            }
            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        /// Converts a collection of bytes that represent a string into an array of formatted
        /// string operands.
        /// </summary>
        /// <param name="formatter">Formatter object.</param>
        /// <param name="opNames">Pseudo-opcode name table.</param>
        /// <param name="dfd">Format descriptor.</param>
        /// <param name="data">File data.</param>
        /// <param name="offset">Offset, within data, of start of string.</param>
        /// <param name="popcode">Receives the pseudo-opcode string.</param>
        /// <returns>Array of operand strings.</returns>
        public static List<string> FormatStringOp(Formatter formatter, PseudoOpNames opNames,
                FormatDescriptor dfd, byte[] data, int offset, out string popcode) {

            StringOpFormatter.ReverseMode revMode = StringOpFormatter.ReverseMode.Forward;
            Formatter.DelimiterSet delSet = formatter.Config.mStringDelimiters;
            Formatter.DelimiterDef delDef;

            CharEncoding.Convert charConv;
            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.Ascii:
                    charConv = CharEncoding.ConvertAscii;
                    delDef = delSet.Get(CharEncoding.Encoding.Ascii);
                    break;
                case FormatDescriptor.SubType.HighAscii:
                    charConv = CharEncoding.ConvertHighAscii;
                    delDef = delSet.Get(CharEncoding.Encoding.HighAscii);
                    break;
                case FormatDescriptor.SubType.C64Petscii:
                    charConv = CharEncoding.ConvertC64Petscii;
                    delDef = delSet.Get(CharEncoding.Encoding.C64Petscii);
                    break;
                case FormatDescriptor.SubType.C64Screen:
                    charConv = CharEncoding.ConvertC64ScreenCode;
                    delDef = delSet.Get(CharEncoding.Encoding.C64ScreenCode);
                    break;
                default:
                    Debug.Assert(false);
                    charConv = CharEncoding.ConvertAscii;
                    delDef = delSet.Get(CharEncoding.Encoding.Ascii);
                    break;
            }

            if (delDef == null) {
                delDef = Formatter.DOUBLE_QUOTE_DELIM;
            }

            StringOpFormatter stropf = new StringOpFormatter(formatter, delDef,
                StringOpFormatter.RawOutputStyle.CommaSep, charConv, false);

            int hiddenLeadingBytes = 0;
            int trailingBytes = 0;
            switch (dfd.FormatType) {
                case FormatDescriptor.Type.StringGeneric:
                    // Generic character data.
                    popcode = opNames.StrGeneric;
                    break;
                case FormatDescriptor.Type.StringReverse:
                    // Character data, full width specified by formatter.  Show characters
                    // in reverse order.
                    popcode = opNames.StrReverse;
                    revMode = StringOpFormatter.ReverseMode.FullReverse;
                    break;
                case FormatDescriptor.Type.StringNullTerm:
                    // Character data with a terminating null.  Don't show the null byte.
                    popcode = opNames.StrNullTerm;
                    trailingBytes = 1;
                    break;
                case FormatDescriptor.Type.StringL8:
                    // Character data with a leading length byte.  Don't show the length.
                    hiddenLeadingBytes = 1;
                    popcode = opNames.StrLen8;
                    break;
                case FormatDescriptor.Type.StringL16:
                    // Character data with a leading length word.  Don't show the length.
                    Debug.Assert(dfd.Length > 1);
                    hiddenLeadingBytes = 2;
                    popcode = opNames.StrLen16;
                    break;
                case FormatDescriptor.Type.StringDci:
                    // High bit on last byte is flipped.
                    popcode = opNames.StrDci;
                    stropf.IsDciString = true;
                    break;
                default:
                    Debug.Assert(false);
                    popcode = ".!!!";
                    break;
            }

            stropf.FeedBytes(data, offset + hiddenLeadingBytes,
                dfd.Length - hiddenLeadingBytes - trailingBytes, 0, revMode);

            return stropf.Lines;
        }

        /// <summary>
        /// Special formatting flags for the FormatNumericOperand() method.
        /// </summary>
        [Flags]
        public enum FormatNumericOpFlags {
            None                    = 0,
            IsPcRel                 = 1,        // opcode is PC relative, e.g. branch or PER
            IsAbsolutePBR           = 1 << 1,   // operand implicitly uses 'K' on 65816 (JMP/JSR)
            HasHashPrefix           = 1 << 2,   // operand has a leading '#', reducing ambiguity
            OmitLabelPrefixSuffix   = 1 << 3,   // don't show annotation char or non-unique prefix
            Is64Tass                = 1 << 4,   // hack to allow 64tass to keep using "common" exp
        }

        /// <summary>
        /// Converts a FormatDescriptor SubType to a CharEncoding.Encoding value.
        /// </summary>
        /// <param name="subType">FormatDescriptor sub-type.</param>
        /// <returns>The corresponding CharEncoding.Encoding value, or Encoding.Unknown
        ///   if the sub-type isn't a character encoding.</returns>
        public static CharEncoding.Encoding SubTypeToEnc(FormatDescriptor.SubType subType) {
            switch (subType) {
                case FormatDescriptor.SubType.Ascii:
                    return CharEncoding.Encoding.Ascii;
                case FormatDescriptor.SubType.HighAscii:
                    return CharEncoding.Encoding.HighAscii;
                case FormatDescriptor.SubType.C64Petscii:
                    return CharEncoding.Encoding.C64Petscii;
                case FormatDescriptor.SubType.C64Screen:
                    return CharEncoding.Encoding.C64ScreenCode;
                default:
                    return CharEncoding.Encoding.Unknown;
            }
        }

        /// <summary>
        /// Format a numeric operand value according to the specified sub-format.
        /// </summary>
        /// <param name="formatter">Text formatter.</param>
        /// <param name="symbolTable">Full table of project symbols.</param>
        /// <param name="labelMap">Symbol label remap, for local label conversion.  May be
        ///   null.</param>
        /// <param name="dfd">Operand format descriptor.</param>
        /// <param name="operandValue">Operand's value.  For most things this comes directly
        ///   out of the code, for relative branches it's a 24-bit absolute address.</param>
        /// <param name="operandLen">Length of operand, in bytes.  For an instruction, this
        ///   does not include the opcode byte.  For a relative branch, this will be 2.</param>
        /// <param name="flags">Special handling.</param>
        public static string FormatNumericOperand(Formatter formatter, SymbolTable symbolTable,
                Dictionary<string, string> labelMap, FormatDescriptor dfd, int operandValue,
                int operandLen, FormatNumericOpFlags flags) {
            return FormatNumericOperand(formatter, symbolTable, null, labelMap, dfd, -1,
                operandValue, operandLen, flags);
        }

        /// <summary>
        /// Format a numeric operand value according to the specified sub-format.  This
        /// version takes additional arguments to support local variables.
        /// </summary>
        /// <param name="formatter">Text formatter.</param>
        /// <param name="symbolTable">Full table of project symbols.</param>
        /// <param name="lvLookup">Local variable lookup object.  May be null if not
        ///   formatting an instruction.</param>
        /// <param name="labelMap">Symbol label remap, for local label conversion.  May be
        ///   null.</param>
        /// <param name="dfd">Operand format descriptor.</param>
        /// <param name="offset">Offset of start of instruction or data pseudo-op, for
        ///   variable name lookup.  Okay to pass -1 when not formatting an instruction.</param>
        /// <param name="operandValue">Operand's value.  For most things this comes directly
        ///   out of the code, for relative branches it's a 24-bit absolute address.</param>
        /// <param name="operandLen">Length of operand, in bytes.  For an instruction, this
        ///   does not include the opcode byte.  For a relative branch, this will be 2.</param>
        /// <param name="flags">Special handling.</param>
        public static string FormatNumericOperand(Formatter formatter, SymbolTable symbolTable,
                LocalVariableLookup lvLookup, Dictionary<string, string> labelMap,
                FormatDescriptor dfd, int offset, int operandValue, int operandLen,
                FormatNumericOpFlags flags) {
            Debug.Assert(operandLen > 0);
            int hexMinLen = operandLen * 2;

            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.None:
                case FormatDescriptor.SubType.Hex:
                case FormatDescriptor.SubType.Address:
                    if ((formatter.ExpressionMode == Formatter.FormatConfig.ExpressionMode.Cc65 ||
                                formatter.ExpressionMode == Formatter.FormatConfig.ExpressionMode.Merlin) &&
                            (flags & FormatNumericOpFlags.IsAbsolutePBR) != 0) {
                        // cc65 really doesn't like 24-bit values for JMP/JSR.  If it sees a
                        // 24-bit hex constant it emits JML/JSL.  Merlin works either way, and
                        // I think it looks better as a 16-bit value.
                        operandValue &= 0xffff;
                    }
                    return formatter.FormatHexValue(operandValue, hexMinLen);
                case FormatDescriptor.SubType.Decimal:
                    return formatter.FormatDecimalValue(operandValue);
                case FormatDescriptor.SubType.Binary:
                    return formatter.FormatBinaryValue(operandValue, hexMinLen * 4);
                case FormatDescriptor.SubType.Ascii:
                case FormatDescriptor.SubType.HighAscii:
                case FormatDescriptor.SubType.C64Petscii:
                case FormatDescriptor.SubType.C64Screen:
                    CharEncoding.Encoding enc = SubTypeToEnc(dfd.FormatSubType);
                    return formatter.FormatCharacterValue(operandValue, enc);
                case FormatDescriptor.SubType.Symbol:
                    if (lvLookup != null && dfd.SymbolRef.IsVariable) {
                        Debug.Assert(operandLen == 1);      // only doing 8-bit stuff
                        DefSymbol defSym = lvLookup.GetSymbol(offset, dfd.SymbolRef);
                        if (defSym != null) {
                            // For local variables we're doing a trivial add and don't
                            // wrap, so the "common" format works for everybody.
                            StringBuilder sb = new StringBuilder();
                            FormatNumericSymbolCommon(formatter, defSym, null,
                                dfd, operandValue, operandLen, flags, sb);
                            return sb.ToString();
                        } else {
                            Debug.WriteLine("Local variable format failed");
                            Debug.Assert(false);
                            return formatter.FormatHexValue(operandValue, hexMinLen);
                        }
                    } else if (symbolTable.TryGetNonVariableValue(dfd.SymbolRef.Label,
                            out Symbol sym)) {
                        StringBuilder sb = new StringBuilder();

                        switch (formatter.ExpressionMode) {
                            case Formatter.FormatConfig.ExpressionMode.Common:
                                FormatNumericSymbolCommon(formatter, sym, labelMap,
                                    dfd, operandValue, operandLen, flags, sb);
                                break;
                            case Formatter.FormatConfig.ExpressionMode.Cc65:
                                FormatNumericSymbolCc65(formatter, sym, labelMap,
                                    dfd, operandValue, operandLen, flags, sb);
                                break;
                            case Formatter.FormatConfig.ExpressionMode.Merlin:
                                FormatNumericSymbolMerlin(formatter, sym, labelMap,
                                    dfd, operandValue, operandLen, flags, sb);
                                break;
                            default:
                                Debug.Assert(false, "Unknown expression mode " +
                                    formatter.ExpressionMode);
                                return "???";
                        }

                        return sb.ToString();
                    } else {
                        return formatter.FormatHexValue(operandValue, hexMinLen);
                    }
                default:
                    // should not see REMOVE or ASCII_GENERIC here
                    Debug.Assert(false);
                    return "???";
            }
        }

        /// <summary>
        /// Format the symbol and adjustment using common expression syntax.
        /// </summary>
        private static void FormatNumericSymbolCommon(Formatter formatter, Symbol sym,
                Dictionary<string, string> labelMap, FormatDescriptor dfd,
                int operandValue, int operandLen, FormatNumericOpFlags flags, StringBuilder sb) {
            // We could have some simple code that generated correct output, shifting and
            // masking every time, but that's ugly and annoying.  For single-byte ops we can
            // just use the byte-select operators, for wider ops we get only as fancy as we
            // need to be.

            // Start by remapping the label, if necessary.  The remapped label may have a
            // local-variable prefix character.
            string symLabel = sym.Label;
            if (labelMap != null && labelMap.TryGetValue(symLabel, out string newLabel)) {
                symLabel = newLabel;
            }
            if (sym.IsVariable) {
                symLabel = formatter.FormatVariableLabel(symLabel);
            }

            // Now put the prefix/suffix back on if desired.  We don't want to mess with it
            // if it's from the assembler.
            if ((flags & FormatNumericOpFlags.OmitLabelPrefixSuffix) == 0) {
                symLabel = Symbol.ConvertLabelForDisplay(symLabel, sym.LabelAnno,
                    true, formatter);
            }

            if (operandLen == 1) {
                // Use the byte-selection operator to get the right piece.  In 64tass the
                // selection operator has a very low precedence, similar to Merlin 32.
                int symbolValue;
                string selOp;
                if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.Bank) {
                    symbolValue = (sym.Value >> 16) & 0xff;
                    if (formatter.Config.mBankSelectBackQuote) {
                        selOp = "`";
                    } else {
                        selOp = "^";
                    }
                } else if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.High) {
                    symbolValue = (sym.Value >> 8) & 0xff;
                    selOp = ">";
                } else {
                    symbolValue = sym.Value & 0xff;
                    if (symbolValue == sym.Value) {
                        selOp = string.Empty;
                    } else {
                        selOp = "<";
                    }
                }

                operandValue &= 0xff;

                if (operandValue != symbolValue &&
                        dfd.SymbolRef.ValuePart != WeakSymbolRef.Part.Low) {
                    // Adjustment is required to an upper-byte part.
                    sb.Append('(');
                    sb.Append(selOp);
                    sb.Append(symLabel);
                    sb.Append(')');
                } else {
                    // no adjustment required, or no byte-selection
                    sb.Append(selOp);
                    sb.Append(symLabel);
                }
                int adjustment = operandValue - symbolValue;
                sb.Append(formatter.FormatAdjustment(adjustment));
            } else if (operandLen <= 4) {
                // Operands and values should be 8/16/24 bit unsigned quantities.  32-bit
                // support is really there so you can have a 24-bit pointer in a 32-bit hole.
                // Might need to adjust this if 32-bit signed quantities become interesting.
                uint mask = 0xffffffff >> ((4 - operandLen) * 8);
                int rightShift, symbolValue;
                if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.Bank) {
                    symbolValue = (sym.Value >> 16);
                    rightShift = 16;
                } else if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.High) {
                    symbolValue = (sym.Value >> 8);
                    rightShift = 8;
                } else {
                    symbolValue = sym.Value;
                    rightShift = 0;
                }
                bool hasShift = (rightShift != 0);

                if ((flags & FormatNumericOpFlags.IsPcRel) != 0) {
                    // PC-relative operands are funny, because an 8- or 16-bit value is always
                    // expanded to 24 bits.  We output a 16-bit value that the assembler will
                    // convert back to 8-bit or 16-bit.  In any event, the bank byte is never
                    // relevant to our computations.
                    operandValue &= 0xffff;
                    symbolValue &= 0xffff;
                }

                bool needCommaK = false;
                if ((flags & FormatNumericOpFlags.IsAbsolutePBR) != 0) {
                    if ((operandValue & 0x00ff0000) == (symbolValue & 0x00ff0000)) {
                        // JMP or JSR to something within the same bank.  We don't need to
                        // mask the value.
                        symbolValue &= 0xffff;
                    } else {
                        // This is an absolute JMP/JSR to an out-of-bank location, which is
                        // bogus and should probably be prevented at a higher level.  Most
                        // assemblers are perfectly happy with this because we're passing a
                        // 16-bit argument to a 16-bit operation, but 64tass doesn't like it
                        // when you do this.  We used to just alter the mask to generate a
                        // (potentially very large) offset, but 64tass stopped accepting that.
                        // The correct approach is to add ",k" to the operand.
                        //mask = 0x00ffffff;
                        if ((flags & FormatNumericOpFlags.Is64Tass) != 0) {
                            needCommaK = true;
                        }
                    }
                }

                bool needMask = false;
                if (symbolValue > mask) {
                    // Post-shift value won't fit in an operand-size box.
                    symbolValue = (int) (symbolValue & mask);
                    needMask = true;
                }

                operandValue = (int)(operandValue & mask);
                int adjustment = operandValue - symbolValue;

                // Possibilities:
                //  label
                //  label + adj
                //  label >> rightShift
                //  (label >> rightShift) + adj
                //  label & mask
                //  (label & mask) + adj
                //  (label >> rightShift) & mask
                //  ((label >> rightShift) & mask) + adj

                sb.Append(symLabel);

                if (rightShift != 0) {
                    sb.Append(" >> ");
                    sb.Append(rightShift.ToString());
                }

                if (needMask) {
                    if (rightShift != 0) {
                        sb.Insert(0, '(');
                        sb.Append(')');
                    }

                    sb.Append(" & ");
                    sb.Append(formatter.FormatHexValue((int)mask, 2));
                }

                if (adjustment != 0) {
                    if (needMask || rightShift != 0) {
                        sb.Insert(0, '(');
                        sb.Append(')');
                    }

                    sb.Append(formatter.FormatAdjustment(adjustment));
                }

                // Starting with a '(' makes it look like an indirect operand, so we need
                // to prefix the expression with a no-op addition.
                if (sb[0] == '(' && (flags & FormatNumericOpFlags.HasHashPrefix) == 0) {
                    sb.Insert(0, "0+");
                }

                if (needCommaK) {
                    sb.Append(",k");
                }
            } else {
                Debug.Assert(false, "bad numeric len");
                sb.Append("?????");
            }
        }

        /// <summary>
        /// Format the symbol and adjustment using cc65 expression syntax.
        /// </summary>
        private static void FormatNumericSymbolCc65(Formatter formatter, Symbol sym,
                Dictionary<string, string> labelMap, FormatDescriptor dfd,
                int operandValue, int operandLen, FormatNumericOpFlags flags, StringBuilder sb) {
            // The key difference between cc65 and other assemblers with general expressions
            // is that the bitwise shift and AND operators have higher precedence than the
            // arithmetic ops like add and subtract.  (The bitwise ops are equal to multiply
            // and divide.)  This means that, if we want to mask off the low 16 bits and add one
            // to a label, we can write "start & $ffff + 1" rather than "(start & $ffff) + 1".
            //
            // This is particularly convenient for PEA, since "PEA (start & $ffff) + 1" looks like
            // we're trying to use a (non-existent) indirect form of PEA.  We can write things
            // in a simpler way.

            int adjustment, symbolValue;

            string symLabel = sym.Label;
            if (labelMap != null && labelMap.TryGetValue(symLabel, out string newLabel)) {
                symLabel = newLabel;
            }
            if ((flags & FormatNumericOpFlags.OmitLabelPrefixSuffix) == 0) {
                symLabel = Symbol.ConvertLabelForDisplay(symLabel, sym.LabelAnno,
                    true, formatter);
            }

            if (operandLen == 1) {
                // Use the byte-selection operator to get the right piece.
                string selOp;
                if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.Bank) {
                    symbolValue = (sym.Value >> 16) & 0xff;
                    selOp = "^";
                } else if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.High) {
                    symbolValue = (sym.Value >> 8) & 0xff;
                    selOp = ">";
                } else {
                    symbolValue = sym.Value & 0xff;
                    if (symbolValue == sym.Value) {
                        selOp = string.Empty;
                    } else {
                        selOp = "<";
                    }
                }
                sb.Append(selOp);
                sb.Append(symLabel);

                operandValue &= 0xff;
            } else if (operandLen <= 4) {
                uint mask = 0xffffffff >> ((4 - operandLen) * 8);
                string shOp;
                if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.Bank) {
                    symbolValue = (sym.Value >> 16);
                    shOp = " >> 16";
                } else if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.High) {
                    symbolValue = (sym.Value >> 8);
                    shOp = " >> 8";
                } else {
                    symbolValue = sym.Value;
                    shOp = "";
                }

                if ((flags & FormatNumericOpFlags.IsPcRel) != 0) {
                    // PC-relative operands are funny, because an 8- or 16-bit value is always
                    // expanded to 24 bits.  We output a 16-bit value that the assembler will
                    // convert back to 8-bit or 16-bit.  In any event, the bank byte is never
                    // relevant to our computations.
                    operandValue &= 0xffff;
                    symbolValue &= 0xffff;
                }

                sb.Append(symLabel);
                sb.Append(shOp);
                if (symbolValue > mask) {
                    // Post-shift value won't fit in an operand-size box.
                    symbolValue = (int)(symbolValue & mask);
                    sb.Append(" & ");
                    sb.Append(formatter.FormatHexValue((int)mask, 2));
                }
                operandValue = (int)(operandValue & mask);

                // If we've added stuff, and we're going to add an adjustment later, stick
                // an extra space in between for readability.
                if (sb.Length != symLabel.Length && operandValue != symbolValue) {
                    sb.Append(' ');
                }
            } else {
                Debug.Assert(false, "bad numeric len");
                sb.Append("?????");
                symbolValue = 0;
            }

            adjustment = operandValue - symbolValue;

            sb.Append(formatter.FormatAdjustment(adjustment));
        }

        /// <summary>
        /// Format the symbol and adjustment using Merlin expression syntax.
        /// </summary>
        private static void FormatNumericSymbolMerlin(Formatter formatter, Symbol sym,
                Dictionary<string, string> labelMap, FormatDescriptor dfd,
                int operandValue, int operandLen, FormatNumericOpFlags flags, StringBuilder sb) {
            // Merlin expressions are compatible with the original 8-bit Merlin.  They're
            // evaluated from left to right, with (almost) no regard for operator precedence.
            //
            // The part-selection operators differ from "simple" in two ways:
            //  (1) They always happen last.  If FOO=$10f0, "#>FOO+$18" == $11.  One of the
            //      few cases where left-to-right evaluation is overridden.
            //  (2) They select words, not bytes.  If FOO=$123456, "#>FOO" is $1234.  This is
            //      best thought of as a shift operator, rather than byte-selection.  For
            //      8-bit code this doesn't matter.
            //
            // This behavior leads to simpler expressions for simple symbol adjustments.

            string symLabel = sym.Label;
            if (labelMap != null && labelMap.TryGetValue(symLabel, out string newLabel)) {
                symLabel = newLabel;
            }
            if ((flags & FormatNumericOpFlags.OmitLabelPrefixSuffix) == 0) {
                symLabel = Symbol.ConvertLabelForDisplay(symLabel, sym.LabelAnno,
                    true, formatter);
            }

            int adjustment;

            // If we add or subtract an adjustment, it will be done on the full value, which
            // is then shifted to the appropriate part.  So we need to left-shift the operand
            // value to match.  We fill in the low bytes with the contents of the symbol, so
            // that the adjustment doesn't include unnecessary values.  (For example, let
            // FOO=$10f0, with operand "#>FOO" ($10).  We shift the operand to get $1000, then
            // OR in the low byte to get $10f0, so that when we subtract we get adjustment==0.)
            int adjOperand, keepLen;
            if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.Bank) {
                adjOperand = operandValue << 16 | (int)(sym.Value & 0xff00ffff);
                keepLen = 3;
            } else if (dfd.SymbolRef.ValuePart == WeakSymbolRef.Part.High) {
                adjOperand = (operandValue << 8) | (sym.Value & 0xff);
                keepLen = 2;
            } else {
                adjOperand = operandValue;
                keepLen = 1;
            }

            keepLen = Math.Max(keepLen, operandLen);
            adjustment = adjOperand - sym.Value;

            if (keepLen == 1) {
                int origAdjust = adjustment;
                adjustment %= 256;
                // Adjust for aesthetics.  The assembler implicitly applies a modulo operation,
                // so we can use the value closest to zero.
                if (adjustment > 127) {
                    adjustment = -(256 - adjustment) /*% 256*/;
                } else if (adjustment < -128) {
                    adjustment = (256 + adjustment) /*% 256*/;
                }

                // We have a problem with ambiguous direct-page arguments if the adjusted
                // value crosses a bank boundary.  For example, "LDA $fff0+24" is computed
                // as $010008, which is too big for a DP arg, so Merlin treats it as absolute
                // (LDA $0008) instead of DP.  If Merlin had done the implicit "& $ffff" before
                // testing the value for DP range, this would behave correctly.  Unfortunately
                // there is no "force DP" modifier, so we either need to add an explicit mask
                // or just punt and use the original adjustment.
                //
                // Note DP is only relevant for bank zero.
                //
                // TODO(someday): we only need to do this for ambiguous DP.  If the instruction
                // is imm or doesn't have an abs equivalent, or it's a fixed-width data item
                // like .DD1, we can still use the nicer-looking adjustment.  We don't currently
                // pass the OpDef in here.
                if ((sym.Value & 0xff0000) == 0 && ((sym.Value + adjustment) & 0xff0000) != 0) {
                    adjustment = origAdjust;
                }
            } else if (keepLen == 2) {
                adjustment %= 65536;
                if (adjustment > 32767) {
                    adjustment = -(65536 - adjustment) /*% 65536*/;
                } else if (adjustment < -32768) {
                    adjustment = (65536 + adjustment) /*% 65536*/;
                }
            }

            // Use the label from sym, not dfd's weak ref; might be different if label
            // comparisons are case-insensitive.
            switch (dfd.SymbolRef.ValuePart) {
                case WeakSymbolRef.Part.Unknown:
                case WeakSymbolRef.Part.Low:
                    // For Merlin, "<" is effectively a no-op.  We can put it in for
                    // aesthetics when grabbing the low byte of a 16-bit value.
                    if ((operandLen == 1) && sym.Value > 0xff) {
                        sb.Append('<');
                    }
                    sb.Append(symLabel);
                    break;
                case WeakSymbolRef.Part.High:
                    sb.Append('>');
                    sb.Append(symLabel);
                    break;
                case WeakSymbolRef.Part.Bank:
                    sb.Append('^');
                    sb.Append(symLabel);
                    break;
                default:
                    Debug.Assert(false, "bad part");
                    sb.Append("???");
                    break;
            }

            sb.Append(formatter.FormatAdjustment(adjustment));
        }
    }
}
