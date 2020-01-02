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
using System.Diagnostics;
using System.Text;

using AddressMode = Asm65.OpDef.AddressMode;

namespace Asm65 {
    /// <summary>
    /// Functions used for formatting bits of 65xx code into human-readable form.
    /// 
    /// There are a variety of ways to format a given thing, based on personal preference
    /// (e.g. whether opcodes are upper- or lower-case) and assembler syntax requirements.
    /// 
    /// The functions in this class serve two purposes: (1) produce consistent output
    /// throughout the program; (2) cache format strings and other components to reduce
    /// string manipulation overhead.  Note the caching is per-Formatter, so it's best to
    /// create just one and share it around.
    /// 
    /// The configuration of a Formatter may not be altered once created.  This is important
    /// in situations where we compute output size in one pass and generate it in another,
    /// because it guarantees that a given Formatter object will produce the same number of
    /// lines of output.
    ///
    /// NOTE: if the CpuDef changes, the cached values in the Formatter will become invalid
    /// (e.g. mOpcodeStrings).  Discard the Formatter and create a new one.  (This could be
    /// fixed by keying off of the OpDef instead of OpDef.Opcode, but that's less convenient.)
    /// </summary>
    public class Formatter {
        /// <summary>
        /// Various format configuration options.  Fill one of these out and pass it to
        /// the Formatter constructor.
        /// </summary>
        public struct FormatConfig {
            // alpha case for some case-insensitive items
            public bool mUpperHexDigits;        // display hex values in upper case?
            public bool mUpperOpcodes;          // display opcodes in upper case?
            public bool mUpperPseudoOpcodes;    // display pseudo-opcodes in upper case?
            public bool mUpperOperandA;         // display acc operand in upper case?
            public bool mUpperOperandS;         // display stack operand in upper case?
            public bool mUpperOperandXY;        // display index register operand in upper case?

            public bool mAddSpaceLongComment;   // insert space after delimiter for long comments?

            // functional changes to assembly output
            public bool mSuppressHexNotation;       // omit '$' before hex digits
            public bool mSuppressImpliedAcc;        // emit just "LSR" rather than "LSR A"?
            public bool mBankSelectBackQuote;       // use '`' rather than '^' for bank selector?

            public string mForceDirectOperandPrefix;    // these may be null or empty
            public string mForceAbsOpcodeSuffix;
            public string mForceAbsOperandPrefix;
            public string mForceDirectOpcodeSuffix;
            public string mForceLongOpcodeSuffix;
            public string mForceLongOperandPrefix;

            public string mLocalVariableLabelPrefix;    // e.g. Merlin 32 puts ']' before var names
            public string mNonUniqueLabelPrefix;        // e.g. ':' or '@' before local label

            public string mEndOfLineCommentDelimiter;   // usually ';'
            public string mFullLineCommentDelimiterBase; // usually ';' or '*', WITHOUT extra space
            public string mBoxLineCommentDelimiter;     // usually blank or ';'

            // delimiter patterns for single character constants
            public DelimiterSet mCharDelimiters;
            public DelimiterSet mStringDelimiters;

            // miscellaneous
            public bool mSpacesBetweenBytes;            // "20edfd" vs. "20 ed fd"
            public bool mCommaSeparatedDense;           // "20edfd" vs. "$20,$ed,$fd"

            // hex dumps
            public bool mHexDumpAsciiOnly;              // disallow non-ASCII chars in hex dumps?
            public enum CharConvMode {
                // TODO(maybe): just pass in a CharEncoding.Convert delegate
                Unknown = 0,
                Ascii,
                LowHighAscii,
                C64Petscii,
                C64ScreenCode
            };
            public CharConvMode mHexDumpCharConvMode;   // character conversion mode for dumps

            // This determines what operators are available and what their precedence is.
            // Hopefully we don't need a separate mode for every assembler in existence.
            public enum ExpressionMode { Unknown = 0, Common, Cc65, Merlin };
            public ExpressionMode mExpressionMode;      // symbol rendering mode

            // Deserialization helper.
            public static ExpressionMode ParseExpressionMode(string str) {
                ExpressionMode em = ExpressionMode.Common;
                if (!string.IsNullOrEmpty(str)) {
                    if (Enum.TryParse<ExpressionMode>(str, out ExpressionMode pem)) {
                        em = pem;
                    }
                }
                return em;
            }
        }

        #region Text Delimiters

        /// <summary>
        /// Container for character and string delimiter pieces.  Instances are immutable.
        /// </summary>
        /// <remarks>
        /// For single-character operands, a simple concatenation of the four fields, with the
        /// character in the middle, is performed.
        ///
        /// For strings, the prefix is included at the start of the first line, but not included
        /// on subsequent lines.  This is primarily intended for the on-screen display, not
        /// assembly source generation.  The suffix is not used at all.
        /// </remarks>
        public class DelimiterDef {
            public string Prefix { get; private set; }
            public char OpenDelim { get; private set; }
            public char CloseDelim { get; private set; }
            public string Suffix { get; private set; }
            public string FormatStr { get; private set; }

            public DelimiterDef(char delim) : this(string.Empty, delim, delim, string.Empty) {
            }
            public DelimiterDef(string prefix, char openDelim, char closeDelim, string suffix) {
                Debug.Assert(prefix != null);
                Debug.Assert(suffix != null);
                Prefix = prefix;
                OpenDelim = openDelim;
                CloseDelim = closeDelim;
                Suffix = suffix;

                // Generate format string.
                StringBuilder sb = new StringBuilder();
                sb.Append(Prefix);
                sb.Append(OpenDelim);
                sb.Append("{0}");
                sb.Append(CloseDelim);
                sb.Append(Suffix);
                FormatStr = sb.ToString();
            }
            public override string ToString() {
                return Prefix + OpenDelim + '#' + CloseDelim + Suffix;
            }
        }
        public static readonly DelimiterDef SINGLE_QUOTE_DELIM = new DelimiterDef('\'');
        public static readonly DelimiterDef DOUBLE_QUOTE_DELIM = new DelimiterDef('"');

        public class DelimiterSet {
            private Dictionary<CharEncoding.Encoding, DelimiterDef> mDelimiters =
                new Dictionary<CharEncoding.Encoding, DelimiterDef>();

            /// <summary>
            /// Returns the specified DelimiterDef, or null if not found.
            /// </summary>
            public DelimiterDef Get(CharEncoding.Encoding enc) {
                mDelimiters.TryGetValue(enc, out DelimiterDef def);
                return def;
            }
            public void Set(CharEncoding.Encoding enc, DelimiterDef def) {
                mDelimiters[enc] = def;
            }
            public override string ToString() {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<CharEncoding.Encoding, DelimiterDef> kvp in mDelimiters) {
                    sb.Append("[" + kvp.Key + ": " + kvp.Value + "]");
                }
                return sb.ToString();
            }

            public static DelimiterSet GetDefaultCharDelimiters() {
                DelimiterSet chrDel = new DelimiterSet();
                chrDel.Set(CharEncoding.Encoding.Ascii,
                    new DelimiterDef(string.Empty, '\u2018', '\u2019', string.Empty));
                chrDel.Set(CharEncoding.Encoding.HighAscii,
                    new DelimiterDef(string.Empty, '\u2018', '\u2019', " | $80"));
                chrDel.Set(CharEncoding.Encoding.C64Petscii,
                    new DelimiterDef("pet:", '\u2018', '\u2019', string.Empty));
                chrDel.Set(CharEncoding.Encoding.C64ScreenCode,
                    new DelimiterDef("scr:", '\u2018', '\u2019', string.Empty));
                return chrDel;
            }

            public static DelimiterSet GetDefaultStringDelimiters() {
                DelimiterSet strDel = new DelimiterSet();
                strDel.Set(CharEncoding.Encoding.Ascii,
                    new DelimiterDef(string.Empty, '\u201c', '\u201d', string.Empty));
                strDel.Set(CharEncoding.Encoding.HighAscii,
                    new DelimiterDef("\u2191", '\u201c', '\u201d', string.Empty));
                strDel.Set(CharEncoding.Encoding.C64Petscii,
                    new DelimiterDef("pet:", '\u201c', '\u201d', string.Empty));
                strDel.Set(CharEncoding.Encoding.C64ScreenCode,
                    new DelimiterDef("scr:", '\u201c', '\u201d', string.Empty));
                return strDel;
            }

            /// <summary>
            /// Serializes a DelimiterSet.
            /// </summary>
            /// <remarks>
            /// Can't use Javascript from a .NET Standard library.  XmlSerializer doesn't
            /// handle Lists or Dictionaries.  Do it the old-fashioned way.
            /// </remarks>
            public string Serialize() {
                Debug.Assert(mDelimiters.Count < 10);
                StringBuilder sb = new StringBuilder();
                sb.Append('*');     // if the format changes, start with something else
                foreach (KeyValuePair<CharEncoding.Encoding, DelimiterDef> kvp in mDelimiters) {
                    string name = kvp.Key.ToString();
                    AddLenString(sb, name);
                    AddLenString(sb, kvp.Value.Prefix);
                    sb.Append(kvp.Value.OpenDelim);
                    sb.Append(kvp.Value.CloseDelim);
                    AddLenString(sb, kvp.Value.Suffix);
                }
                sb.Append('!');
                return sb.ToString();
            }
            private void AddLenString(StringBuilder sb, string str) {
                sb.Append(str.Length.ToString());
                sb.Append(',');
                sb.Append(str);
            }
            public static DelimiterSet Deserialize(string cereal) {
                try {
                    DelimiterSet delimSet = new DelimiterSet();

                    int offset = 0;
                    if (cereal[offset++] != '*') {
                        throw new Exception("missing leading asterisk");
                    }
                    while (cereal[offset] != '!') {
                        string str = GetLenString(cereal, ref offset);
                        if (!Enum.TryParse(str, out CharEncoding.Encoding enc)) {
                            Debug.WriteLine("Ignoring unknown encoding " + str);
                            enc = CharEncoding.Encoding.Unknown;
                        }
                        string prefix = GetLenString(cereal, ref offset);
                        char open = cereal[offset++];
                        char close = cereal[offset++];
                        string suffix = GetLenString(cereal, ref offset);
                        if (enc != CharEncoding.Encoding.Unknown) {
                            delimSet.Set(enc, new DelimiterDef(prefix, open, close, suffix));
                        }
                    }
                    return delimSet;
                } catch (Exception ex) {
                    Debug.WriteLine("DelimiterSet deserialization failed: " + ex.Message);
                    return new DelimiterSet();
                }
            }
            private static string GetLenString(string str, ref int offset) {
                int commaIndex = str.IndexOf(',', offset);
                if (commaIndex < 0) {
                    throw new Exception("no comma in length string");
                }
                string lenStr = str.Substring(offset, commaIndex - offset);
                int len = int.Parse(lenStr);
                string resultStr = str.Substring(commaIndex + 1, len);
                offset = commaIndex + 1 + len;
                return resultStr;
            }
        }

        #endregion Text Delimiters

        private static readonly char[] sHexCharsLower = {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f'
        };
        private static readonly char[] sHexCharsUpper = {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'
        };

        /// <summary>
        /// Formatter configuration options.  Fixed at construction time.
        /// </summary>
        private FormatConfig mFormatConfig;

        /// <summary>
        /// Get a copy of the format config.
        /// </summary>
        public FormatConfig Config { get { return mFormatConfig; } }

        // Cached bits and pieces.
        char mHexFmtChar;
        string mHexPrefix;
        string mAccChar;
        char mXregChar;
        char mYregChar;
        char mSregChar;

        // Format string for offsets.
        private string mOffset24Format;

        // Format strings for addresses.
        private string mAddrFormatNoBank;
        private string mAddrFormatWithBank;

        // Generated opcode strings.  The index is the bitwise OR of the opcode value and
        // the disambiguation value.  In most cases this just helps us avoid calling
        // ToUpper incessantly.
        private Dictionary<int, string> mOpcodeStrings = new Dictionary<int, string>();

        // Generated pseudo-opcode strings.
        private Dictionary<string, string> mPseudoOpStrings = new Dictionary<string, string>();

        // Generated format strings for operands.  The index is the bitwise OR of the
        // address mode and the disambiguation value.
        private Dictionary<int, string> mOperandFormats = new Dictionary<int, string>();

        // Generated format strings for bytes.
        private const int MAX_BYTE_DUMP = 4;
        private string[] mByteDumpFormats = new string[MAX_BYTE_DUMP];

        // Generated format strings for hex values.
        private string[] mHexValueFormats = new string[4];

        private string mFullLineCommentDelimiterPlus;

        // Buffer to use when generating hex dump lines.
        private char[] mHexDumpBuffer;

        private CharEncoding.Convert mHexDumpCharConv;

        /// <summary>
        /// A 16-character array with 0-9a-f, for hex conversions.  The letters will be
        /// upper or lower case, per the format config.
        /// </summary>
        public char[] HexDigits {
            get {
                return mFormatConfig.mUpperHexDigits ? sHexCharsUpper : sHexCharsLower;
            }
        }

        /// <summary>
        /// String to put between the operand and the end-of-line comment.
        /// </summary>
        public string EndOfLineCommentDelimiter {
            get { return mFormatConfig.mEndOfLineCommentDelimiter; }
        }

        /// <summary>
        /// String to put at the start of a line with a full-line comment.
        /// </summary>
        public string FullLineCommentDelimiter {
            get { return mFullLineCommentDelimiterPlus; }
        }

        /// <summary>
        /// String to put at the start of a line that has a box comment.  This is usually
        /// blank, as it's only needed if the assembler doesn't recognize the box character
        /// as a comment.
        /// </summary>
        public string BoxLineCommentDelimiter {
            get { return mFormatConfig.mBoxLineCommentDelimiter; }
        }

        /// <summary>
        /// Prefix for non-unique address labels.
        /// </summary>
        public string NonUniqueLabelPrefix {
            get { return mFormatConfig.mNonUniqueLabelPrefix; }
        }

        /// <summary>
        /// When formatting a symbol with an offset, if this flag is set, generate code that
        /// assumes the assembler applies the adjustment, then shifts the result.  If not,
        /// assume the assembler shifts the operand before applying the adjustment.
        /// </summary>
        public FormatConfig.ExpressionMode ExpressionMode {
            get { return mFormatConfig.mExpressionMode; }
        }


        /// <summary>
        /// Constructor.  Initializes various fields based on the configuration.  We want to
        /// do as much work as possible here.
        /// </summary>
        public Formatter(FormatConfig config) {
            mFormatConfig = config;     // copy struct
            if (mFormatConfig.mEndOfLineCommentDelimiter == null) {
                mFormatConfig.mEndOfLineCommentDelimiter = string.Empty;
            }
            if (mFormatConfig.mFullLineCommentDelimiterBase == null) {
                mFormatConfig.mFullLineCommentDelimiterBase = string.Empty;
            }
            if (mFormatConfig.mBoxLineCommentDelimiter == null) {
                mFormatConfig.mBoxLineCommentDelimiter = string.Empty;
            }

            if (string.IsNullOrEmpty(mFormatConfig.mNonUniqueLabelPrefix)) {
                mFormatConfig.mNonUniqueLabelPrefix = "@";
            }

            if (mFormatConfig.mAddSpaceLongComment) {
                mFullLineCommentDelimiterPlus = mFormatConfig.mFullLineCommentDelimiterBase + " ";
            } else {
                mFullLineCommentDelimiterPlus = mFormatConfig.mFullLineCommentDelimiterBase;
            }

            // Prep the static parts of the hex dump buffer.
            mHexDumpBuffer = new char[73];
            for (int i = 0; i < mHexDumpBuffer.Length; i++) {
                mHexDumpBuffer[i] = ' ';
            }
            mHexDumpBuffer[6] = ':';

            // Resolve boolean flags to character or string values.
            if (mFormatConfig.mUpperHexDigits) {
                mHexFmtChar = 'X';
            } else {
                mHexFmtChar = 'x';
            }
            if (mFormatConfig.mSuppressHexNotation) {
                mHexPrefix = "";
            } else {
                mHexPrefix = "$";
            }
            if (mFormatConfig.mSuppressImpliedAcc) {
                mAccChar = "";
            } else if (mFormatConfig.mUpperOperandA) {
                mAccChar = "A";
            } else {
                mAccChar = "a";
            }
            if (mFormatConfig.mUpperOperandXY) {
                mXregChar = 'X';
                mYregChar = 'Y';
            } else {
                mXregChar = 'x';
                mYregChar = 'y';
            }
            if (mFormatConfig.mUpperOperandS) {
                mSregChar = 'S';
            } else {
                mSregChar = 's';
            }

            // process the delimiter patterns
            DelimiterSet chrDelim = mFormatConfig.mCharDelimiters;
            if (chrDelim == null) {
                Debug.WriteLine("NOTE: char delimiters not set");
                chrDelim = DelimiterSet.GetDefaultCharDelimiters();
            }

            switch (mFormatConfig.mHexDumpCharConvMode) {
                case FormatConfig.CharConvMode.Ascii:
                    mHexDumpCharConv = CharEncoding.ConvertAscii;
                    break;
                case FormatConfig.CharConvMode.LowHighAscii:
                    mHexDumpCharConv = CharEncoding.ConvertLowAndHighAscii;
                    break;
                case FormatConfig.CharConvMode.C64Petscii:
                    mHexDumpCharConv = CharEncoding.ConvertC64Petscii;
                    break;
                case FormatConfig.CharConvMode.C64ScreenCode:
                    mHexDumpCharConv = CharEncoding.ConvertC64ScreenCode;
                    break;
                default:
                    // most some things don't configure the hex dump; this is fine
                    mHexDumpCharConv = CharEncoding.ConvertLowAndHighAscii;
                    break;
            }
        }

        /// <summary>
        /// Formats a 24-bit offset value as hex.
        /// </summary>
        /// <param name="offset">Offset to format.</param>
        /// <returns>Formatted string.</returns>
        public string FormatOffset24(int offset) {
            if (string.IsNullOrEmpty(mOffset24Format)) {
                mOffset24Format = "+{0:" + mHexFmtChar + "6}";
            }
            return string.Format(mOffset24Format, offset & 0x0fffff);
        }

        /// <summary>
        /// Formats a value in hexadecimal.  The width is padded with zeroes to make the
        /// length even (so it'll be $00, $0100, $010000, etc.)  If minDigits is nonzero,
        /// additional zeroes may be added.
        /// </summary>
        /// <param name="value">Value to format, up to 32 bits.</param>
        /// <param name="minDigits">Minimum width, in printed digits (e.g. 4 is "0000").</param>
        /// <returns>Formatted string.</returns>
        public string FormatHexValue(int value, int minDigits) {
            int width = minDigits > 2 ? minDigits : 2;
            if (width < 8 && value > 0xffffff) {
                width = 8;
            } else if (width < 6 && value > 0xffff) {
                width = 6;
            } else if (width < 4 && value > 0xff) {
                width = 4;
            }
            int index = (width / 2) - 1;
            if (mHexValueFormats[index] == null) {
                mHexValueFormats[index] = mHexFmtChar + width.ToString();
            }
            return mHexPrefix + value.ToString(mHexValueFormats[index]);
        }

        /// <summary>
        /// Format a value as a number in the specified base.
        /// </summary>
        /// <param name="value">Value to format.</param>
        /// <param name="numBase">Numeric base (2, 10, or 16).</param>
        /// <returns>Formatted string.</returns>
        public string FormatValueInBase(int value, int numBase) {
            switch (numBase) {
                case 2:
                    return FormatBinaryValue(value, 8);
                case 10:
                    return FormatDecimalValue(value);
                case 16:
                    return FormatHexValue(value, 2);
                default:
                    Debug.Assert(false);
                    return "???";
            }
        }

        /// <summary>
        /// Formats a value as decimal.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <returns>Formatted string.</returns>
        public string FormatDecimalValue(int value) {
            return value.ToString();
        }

        /// <summary>
        /// Formats a value in binary, padding with zeroes so the length is a multiple of 8.
        /// </summary>
        /// <param name="value">Value to convert.</param>
        /// <param name="minDigits">Minimum width, in printed digits.  Will be rounded up to
        ///   a multiple of 8.</param>
        /// <returns>Formatted string.</returns>
        public string FormatBinaryValue(int value, int minDigits) {
            string binaryStr = Convert.ToString(value, 2);
            int desiredWidth = ((binaryStr.Length + 7) / 8) * 8;
            if (desiredWidth < minDigits) {
                desiredWidth = ((minDigits + 7) / 8) * 8;
            }
            return '%' + binaryStr.PadLeft(desiredWidth, '0');
        }

        /// <summary>
        /// Formats a single-character operand.  Output will be a delimited printable character
        /// when possible, a hex value when the converted character is unprintable.
        /// </summary>
        /// <param name="value">Value to format.  Could be a 16-bit immediate value.</param>
        /// <param name="enc">Character encoding to use for value.</param>
        /// <returns>Formatted string.</returns>
        public string FormatCharacterValue(int value, CharEncoding.Encoding enc) {
            if (value < 0 || value > 0xff) {
                return FormatHexValue(value, 2);
            }

            DelimiterDef delimDef = mFormatConfig.mCharDelimiters.Get(enc);
            if (delimDef == null) {
                return FormatHexValue(value, 2);
            }

            string fmt = delimDef.FormatStr;
            Debug.Assert(fmt != null);

            CharEncoding.Convert conv;
            switch (enc) {
                case CharEncoding.Encoding.Ascii:
                    conv = CharEncoding.ConvertAscii;
                    break;
                case CharEncoding.Encoding.HighAscii:
                    conv = CharEncoding.ConvertHighAscii;
                    break;
                case CharEncoding.Encoding.C64Petscii:
                    conv = CharEncoding.ConvertC64Petscii;
                    break;
                case CharEncoding.Encoding.C64ScreenCode:
                    conv = CharEncoding.ConvertC64ScreenCode;
                    break;
                default:
                    return FormatHexValue(value, 2);
            }

            char ch = conv((byte)value);
            if (ch == CharEncoding.UNPRINTABLE_CHAR || ch == delimDef.OpenDelim ||
                    ch == delimDef.CloseDelim) {
                // We might be able to do better with delimiter clashes, e.g. '\'', but
                // that's assembler-specific.
                return FormatHexValue(value, 2);
            } else {
                // Possible optimization: replace fmt with a prefix/suffix pair, and just concat
                return string.Format(fmt, ch);
            }
        }

        /// <summary>
        /// Formats a 16- or 24-bit address value.  This is intended for the left column
        /// of something (hex dump, code listing), not as an operand.
        /// </summary>
        /// <param name="address">Address to format.</param>
        /// <param name="showBank">Set to true for CPUs with 24-bit address spaces.</param>
        /// <returns>Formatted string.</returns>
        public string FormatAddress(int address, bool showBank) {
            if (mAddrFormatNoBank == null) {
                mAddrFormatNoBank = "{0:" + mHexFmtChar + "4}";
                mAddrFormatWithBank = "{0:" + mHexFmtChar + "2}/{1:" + mHexFmtChar + "4}";
            }
            if (showBank) {
                return string.Format(mAddrFormatWithBank, address >> 16, address & 0xffff);
            } else {
                return string.Format(mAddrFormatNoBank, address & 0xffff);
            }
        }

        /// <summary>
        /// Formats a local variable label, prepending an identifying prefix if one has been
        /// specified.
        /// </summary>
        public string FormatVariableLabel(string label) {
            if (!string.IsNullOrEmpty(mFormatConfig.mLocalVariableLabelPrefix)) {
                return mFormatConfig.mLocalVariableLabelPrefix + label;
            } else {
                return label;
            }
        }

        /// <summary>
        /// Formats an adjustment.  Small values are formatted as "+decimal" or "-decimal",
        /// larger values are formatted as hex.  If no adjustment is required, an empty string
        /// is returned.
        /// </summary>
        /// <param name="adjValue">Adjustment value.</param>
        /// <returns>Formatted string.</returns>
        public string FormatAdjustment(int adjValue) {
            if (adjValue == 0) {
                return string.Empty;
            } else if (Math.Abs(adjValue) >= 256) {
                // not using mHexPrefix here, since dec vs. hex matters
                if (adjValue < 0) {
                    return "-$" + (-adjValue).ToString(mHexValueFormats[0]);
                } else {
                    return "+$" + adjValue.ToString(mHexValueFormats[0]);
                }
            } else {
                // This formats in decimal with a leading '+' or '-'.  To avoid adding a plus
                // on zero, we'd use "+#;-#;0", but we took care of the zero case above.
                return adjValue.ToString("+0;-#");
            }
        }

        /// <summary>
        /// Formats the instruction opcode mnemonic, and caches the result.
        /// 
        /// It may be necessary to modify the mnemonic for some assemblers, e.g. LDA from a
        /// 24-bit address might need to be LDAL, even if the high byte is nonzero.
        /// </summary>
        /// <param name="op">Opcode to format</param>
        /// <param name="wdis">Width disambiguation specifier.</param>
        /// <returns>Formatted string.</returns>
        public string FormatOpcode(OpDef op, OpDef.WidthDisambiguation wdis) {
            // TODO(someday): using op.Opcode as the key is a bad idea, as the operation may
            // not be the same on different CPUs.  We currently rely on the caller to discard
            // the Formatter when the CPU definition changes.  We'd be better off keying off of
            // the OpDef object and factoring wdis in some other way.
            int key = op.Opcode | ((int)wdis << 8);
            if (!mOpcodeStrings.TryGetValue(key, out string opcodeStr)) {
                // Not found, generate value.
                opcodeStr = FormatMnemonic(op.Mnemonic, wdis);

                // Memoize.
                mOpcodeStrings[key] = opcodeStr;
            }
            return opcodeStr;
        }

        /// <summary>
        /// Formats the string as an opcode mnemonic.
        /// 
        /// It may be necessary to modify the mnemonic for some assemblers, e.g. LDA from a
        /// 24-bit address might need to be LDAL, even if the high byte is nonzero.
        /// </summary>
        /// <param name="mnemonic">Instruction mnemonic string.</param>
        /// <param name="wdis">Width disambiguation specifier.</param>
        /// <returns></returns>
        public string FormatMnemonic(string mnemonic, OpDef.WidthDisambiguation wdis) {
            string opcodeStr = mnemonic;
            if (wdis == OpDef.WidthDisambiguation.ForceDirect) {
                if (!string.IsNullOrEmpty(mFormatConfig.mForceDirectOpcodeSuffix)) {
                    opcodeStr += mFormatConfig.mForceDirectOpcodeSuffix;
                }
            } else if (wdis == OpDef.WidthDisambiguation.ForceAbs) {
                if (!string.IsNullOrEmpty(mFormatConfig.mForceAbsOpcodeSuffix)) {
                    opcodeStr += mFormatConfig.mForceAbsOpcodeSuffix;
                }
            } else if (wdis == OpDef.WidthDisambiguation.ForceLong ||
                       wdis == OpDef.WidthDisambiguation.ForceLongMaybe) {
                if (!string.IsNullOrEmpty(mFormatConfig.mForceLongOpcodeSuffix)) {
                    opcodeStr += mFormatConfig.mForceLongOpcodeSuffix;
                }
            } else {
                Debug.Assert(wdis == OpDef.WidthDisambiguation.None);
            }
            if (mFormatConfig.mUpperOpcodes) {
                opcodeStr = opcodeStr.ToUpperInvariant();
            }
            return opcodeStr;
        }

        /// <summary>
        /// Generates an operand format.
        /// </summary>
        /// <param name="addrMode">Addressing mode.</param>
        /// <param name="wdis">Width disambiguation mode.</param>
        /// <returns>Format string.</returns>
        private string GenerateOperandFormat(OpDef.AddressMode addrMode,
                OpDef.WidthDisambiguation wdis) {
            string fmt;
            string wdisStr = string.Empty;

            if (wdis == OpDef.WidthDisambiguation.ForceDirect) {
                if (!string.IsNullOrEmpty(mFormatConfig.mForceDirectOperandPrefix)) {
                    wdisStr = mFormatConfig.mForceDirectOperandPrefix;
                }
            } else if (wdis == OpDef.WidthDisambiguation.ForceAbs) {
                if (!string.IsNullOrEmpty(mFormatConfig.mForceAbsOperandPrefix)) {
                    wdisStr = mFormatConfig.mForceAbsOperandPrefix;
                }
            } else if (wdis == OpDef.WidthDisambiguation.ForceLong) {
                if (!string.IsNullOrEmpty(mFormatConfig.mForceLongOperandPrefix)) {
                    wdisStr = mFormatConfig.mForceLongOperandPrefix;
                }
            } else if (wdis == OpDef.WidthDisambiguation.ForceLongMaybe) {
                // Don't add a width disambiguator to an operand that is unambiguously long.
            } else {
                Debug.Assert(wdis == OpDef.WidthDisambiguation.None);
            }

            switch (addrMode) {
                case AddressMode.Abs:
                case AddressMode.AbsLong:
                case AddressMode.BlockMove:
                case AddressMode.StackAbs:
                case AddressMode.DP:
                case AddressMode.PCRel:
                case AddressMode.PCRelLong:         // BRL
                case AddressMode.StackInt:          // COP and two-byte BRK
                case AddressMode.StackPCRelLong:    // PER
                case AddressMode.WDM:
                    fmt = wdisStr + "{0}";
                    break;
                case AddressMode.AbsIndexX:
                case AddressMode.AbsIndexXLong:
                case AddressMode.DPIndexX:
                    fmt = wdisStr + "{0}," + mXregChar;
                    break;
                case AddressMode.DPIndexY:
                case AddressMode.AbsIndexY:
                    fmt = wdisStr + "{0}," + mYregChar;
                    break;
                case AddressMode.AbsIndexXInd:
                case AddressMode.DPIndexXInd:
                    fmt = wdisStr + "({0}," + mXregChar + ")";
                    break;
                case AddressMode.AbsInd:
                case AddressMode.DPInd:
                case AddressMode.StackDPInd:        // PEI
                    fmt = "({0})";
                    break;
                case AddressMode.AbsIndLong:
                case AddressMode.DPIndLong:
                    // IIgs monitor uses "()" for AbsIndLong, E&L says "[]".  Assemblers
                    // seem to expect the latter.
                    fmt = "[{0}]";
                    break;
                case AddressMode.Acc:
                    fmt = mAccChar;
                    break;
                case AddressMode.DPIndIndexY:
                    fmt = "({0})," + mYregChar;
                    break;
                case AddressMode.DPIndIndexYLong:
                    fmt = "[{0}]," + mYregChar;
                    break;
                case AddressMode.Imm:
                case AddressMode.ImmLongA:
                case AddressMode.ImmLongXY:
                    fmt = "#{0}";
                    break;
                case AddressMode.Implied:
                case AddressMode.StackPull:
                case AddressMode.StackPush:
                case AddressMode.StackRTI:
                case AddressMode.StackRTL:
                case AddressMode.StackRTS:
                    fmt = string.Empty;
                    break;
                case AddressMode.StackRel:
                    fmt = "{0}," + mSregChar;
                    break;
                case AddressMode.StackRelIndIndexY:
                    fmt = "({0}," + mSregChar + ")," + mYregChar;
                    break;

                case AddressMode.Unknown:
                default:
                    Debug.Assert(false);
                    fmt = "???";
                    break;
            }

            return fmt;
        }

        /// <summary>
        /// Formats the instruction operand.
        /// </summary>
        /// <param name="op">Opcode definition (needed for address mode).</param>
        /// <param name="contents">Label or numeric operand value.</param>
        /// <param name="wdis">Width disambiguation value.</param>
        /// <returns>Formatted string.</returns>
        public string FormatOperand(OpDef op, string contents, OpDef.WidthDisambiguation wdis) {
            Debug.Assert(((int)op.AddrMode & 0xff) == (int) op.AddrMode);
            int key = (int) op.AddrMode | ((int)wdis << 8);

            if (!mOperandFormats.TryGetValue(key, out string format)) {
                format = mOperandFormats[key] = GenerateOperandFormat(op.AddrMode, wdis);
            }
            return string.Format(format, contents);
        }

        /// <summary>
        /// Formats a pseudo-opcode.
        /// </summary>
        /// <param name="opstr">Pseudo-op string to format.</param>
        /// <returns>Formatted string.</returns>
        public string FormatPseudoOp(string opstr) {
            if (!mPseudoOpStrings.TryGetValue(opstr, out string result)) {
                if (mFormatConfig.mUpperPseudoOpcodes) {
                    result = mPseudoOpStrings[opstr] = opstr.ToUpperInvariant();
                } else {
                    result = mPseudoOpStrings[opstr] = opstr;
                }
            }
            return result;
        }

        /// <summary>
        /// Generates a format string for N hex bytes.
        /// </summary>
        /// <param name="len">Number of bytes to handle in the format.</param>
        private void GenerateByteFormat(int len) {
            Debug.Assert(len <= MAX_BYTE_DUMP);

            StringBuilder sb = new StringBuilder(len * 7);
            for (int i = 0; i < len; i++) {
                if (i != 0 && mFormatConfig.mSpacesBetweenBytes) {
                    sb.Append(' ');
                }
                // e.g. "{0:x2}"
                sb.Append("{" + i + ":" + mHexFmtChar + "2}");
            }
            mByteDumpFormats[len - 1] = sb.ToString();
        }

        /// <summary>
        /// Formats 1-4 bytes as hex values.
        /// </summary>
        /// <param name="data">Data source.</param>
        /// <param name="offset">Start offset within data array.</param>
        /// <param name="length">Number of bytes to print.  Fewer than this many may
        ///   actually appear.</param>
        /// <returns>Formatted data string.</returns>
        public string FormatBytes(byte[] data, int offset, int length) {
            Debug.Assert(length > 0);
            int printLen = length < MAX_BYTE_DUMP ? length : MAX_BYTE_DUMP;
            if (string.IsNullOrEmpty(mByteDumpFormats[printLen - 1])) {
                GenerateByteFormat(printLen);
            }
            string format = mByteDumpFormats[printLen - 1];
            string result;

            // The alternative is to allocate a temporary object[] and copy the integers
            // into it, which requires boxing.  We know we're only printing 1-4 bytes, so
            // it's easier to just handle each case individually.
            switch (printLen) {
                case 1:
                    result = string.Format(format, data[offset]);
                    break;
                case 2:
                    result = string.Format(format, data[offset], data[offset + 1]);
                    break;
                case 3:
                    result = string.Format(format,
                        data[offset], data[offset + 1], data[offset + 2]);
                    break;
                case 4:
                    result = string.Format(format,
                        data[offset], data[offset + 1], data[offset + 2], data[offset + 3]);
                    break;
                default:
                    result = "INTERNAL ERROR";
                    break;
            }
            if (length > printLen) {
                result += "...";
            }

            return result;
        }

        /// <summary>
        /// Formats an end-of-line comment, prepending an end-of-line comment delimiter.
        /// </summary>
        /// <param name="comment">Comment string; may be empty.</param>
        /// <returns>Formatted string.</returns>
        public string FormatEolComment(string comment) {
            if (string.IsNullOrEmpty(comment) ||
                    string.IsNullOrEmpty(mFormatConfig.mEndOfLineCommentDelimiter)) {
                return comment;
            } else {
                return mFormatConfig.mEndOfLineCommentDelimiter + comment;
            }
        }

        /// <summary>
        /// Formats a collection of bytes as a dense hex string.
        /// </summary>
        /// <param name="data">Data source.</param>
        /// <param name="offset">Start offset within data array.</param>
        /// <param name="length">Number of bytes to print.</param>
        /// <returns>Formatted data string.</returns>
        public string FormatDenseHex(byte[] data, int offset, int length) {
            char[] hexChars = mFormatConfig.mUpperHexDigits ? sHexCharsUpper : sHexCharsLower;
            char[] text;
            if (mFormatConfig.mCommaSeparatedDense) {
                text = new char[length * 4 - 1];
                for (int i = 0; i < length; i++) {
                    byte val = data[offset + i];
                    text[i * 4] = '$';
                    text[i * 4 + 1] = hexChars[val >> 4];
                    text[i * 4 + 2] = hexChars[val & 0x0f];
                    if (i != length - 1) {
                        text[i * 4 + 3] = ',';
                    }
                }
            } else {
                text = new char[length * 2];
                for (int i = 0; i < length; i++) {
                    byte val = data[offset + i];
                    text[i * 2] = hexChars[val >> 4];
                    text[i * 2 + 1] = hexChars[val & 0x0f];
                }
            }
            return new string(text);
        }

        /// <summary>
        /// Returns the number of characters output for each byte when formatting dense hex.
        /// </summary>
        /// <remarks>
        /// This isn't quite right, because you don't need a comma after the very last element
        /// in the list for comma-separated values.  Handling this correctly for multi-line
        /// items is more trouble than it's worth though.
        /// </remarks>
        public int CharsPerDenseByte {
            get {
                if (mFormatConfig.mCommaSeparatedDense) {
                    return 4;
                } else {
                    return 2;
                }
            }
        }

        /// <summary>
        /// Formats up to 16 bytes of data into a single line hex dump, in this format:
        /// <pre>012345: 00 11 22 33 44 55 66 77 88 99 aa bb cc dd ee ff  0123456789abcdef</pre>
        /// </summary>
        /// <param name="data">Reference to data.</param>
        /// <param name="offset">Start offset.</param>
        /// <returns>Formatted string.</returns>
        public string FormatHexDump(byte[] data, int offset) {
            int length = Math.Min(16, data.Length - offset);
            FormatHexDumpCommon(data, offset, offset, length);
            // this is the only allocation
            return new string(mHexDumpBuffer);
        }

        /// <summary>
        /// Formats up to 16 bytes of data into a single line hex dump.  The output is
        /// appended to the StringBuilder.
        /// </summary>
        /// <param name="data">Reference to data.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="sb">StringBuilder that receives output.</param>
        public void FormatHexDump(byte[] data, int offset, StringBuilder sb) {
            int length = Math.Min(16, data.Length - offset);
            FormatHexDumpCommon(data, offset, offset, length);
            sb.Append(mHexDumpBuffer);
        }

        public void FormatHexDump(byte[] data, int offset, int addr, int length,
                StringBuilder sb) {
            FormatHexDumpCommon(data, offset, addr, length);
            sb.Append(mHexDumpBuffer);
        }

        /// <summary>
        /// Formats up to 16 bytes of data into mHexDumpBuffer.
        /// </summary>
        private void FormatHexDumpCommon(byte[] data, int offset, int addr, int length) {
            Debug.Assert(offset >= 0 && offset < data.Length);
            Debug.Assert(data.Length < (1 << 24));
            const int dataCol = 8;
            const int asciiCol = 57;

            char[] hexChars = mFormatConfig.mUpperHexDigits ? sHexCharsUpper : sHexCharsLower;
            char[] outBuf = mHexDumpBuffer;

            int skip = addr & 0x0f;     // we skip this many entries...
            offset -= skip;             // ...so adjust offset to balance it
            addr &= ~0x0f;

            // address field
            for (int i = 5; i >= 0; i--) {
                outBuf[i] = hexChars[addr & 0x0f];
                addr >>= 4;
            }

            // If addr doesn't start at xxx0, pad it.
            int index;
            for (index = 0; index < skip; index++) {
                outBuf[dataCol + index * 3] = outBuf[dataCol + index * 3 + 1] =
                    outBuf[asciiCol + index] = ' ';
            }

            // hex digits and characters
            for (int i = 0; i < length; i++) {
                byte val = data[offset + index];
                outBuf[dataCol + index * 3] = hexChars[val >> 4];
                outBuf[dataCol + index * 3 + 1] = hexChars[val & 0x0f];
                outBuf[asciiCol + index] = CharConv(val);
                index++;
            }

            // for partial line, clear out previous contents
            for (; index < 16; index++) {
                outBuf[dataCol + index * 3] =
                    outBuf[dataCol + index * 3 + 1] =
                    outBuf[asciiCol + index] = ' ';
            }
        }

        /// <summary>
        /// Converts a byte into printable form according to the current hex dump
        /// character conversion mode.
        /// </summary>
        /// <param name="val">Value to convert.</param>
        /// <returns>Printable character.</returns>
        private char CharConv(byte val) {
            char ch = mHexDumpCharConv(val);
            if (ch != CharEncoding.UNPRINTABLE_CHAR) {
                return ch;
            } else if (mFormatConfig.mHexDumpAsciiOnly) {
                return '.';
            } else {
                // Certain values make the hex dump ListView freak out in WinForms, but work
                // fine in WPF.  The "control pictures" are a nice idea, but in practice they're
                // unreadably small and provide no benefit.  The black-diamond "replacement
                // character" is dark and makes everything feel noisy.  Middle-dot is subtle,
                // but sufficiently different from a '.' to be useful.

                //if (ch < 0x20) {
                //    return (char)(ch + '\u2400');   // Unicode "control pictures" block
                //}
                //return '\ufffd';                    // Unicode "replacement character"
                //return '\u00bf';    // INVERTED QUESTION MARK

                return '\u00b7';    // MIDDLE DOT
            }
        }
    }
}
