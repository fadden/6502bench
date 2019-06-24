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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CommonUtil {
    /// <summary>
    /// Text utility functions.
    /// </summary>
    public static class TextUtil {
        // 100 spaces, useful when padding things out.
        private const string SPACES = "                                                  " +
                                      "                                                  ";
        private static readonly char[] CSV_ESCAPE_CHARS = { ',', '"' };
        private const string NonPrintableAsciiPattern = @"[^\u0020-\u007e]";
        private static Regex sNonPrintableAsciiRegex = new Regex(NonPrintableAsciiPattern);

        /// <summary>
        /// Converts a string to an ASCII-only string, replacing iso-latin characters
        /// with their ASCII equivalents.  This may change the length of the string.
        /// 
        /// For example, "¿Dónde está Über bären?" becomes "Donde esta Uber baren?".
        /// </summary>
        /// <param name="inString"></param>
        /// <returns>Converted string.</returns>
        public static string LatinToAscii(string inString) {
            // https://stackoverflow.com/questions/140422/
            var newStringBuilder = new StringBuilder();
            newStringBuilder.Append(inString.Normalize(NormalizationForm.FormKD)
                                            .Where(x => x < 128)
                                            .ToArray());
            return newStringBuilder.ToString();

            // Alternatively?
            //   System.Text.Encoding.ASCII.GetString(
            //     System.Text.Encoding.GetEncoding(1251).GetBytes(text))
        }

        /// <summary>
        /// Returns true if the value is valid high- or low-ASCII.
        /// </summary>
        /// <param name="val">Value to test.</param>
        /// <returns>True if val is valid ASCII.</returns>
        public static bool IsHiLoAscii(int val) {
            return (val >= 0x20 && val < 0x7f) || (val >= 0xa0 && val < 0xff);
        }

        /// <summary>
        /// Determines whether the character is printable ASCII.
        /// </summary>
        /// <param name="ch">Character to evaluate.</param>
        /// <returns>True if the character is printable ASCII.</returns>
        public static bool IsPrintableAscii(char ch) {
            return ch >= 0x20 && ch < 0x7f;
        }

        /// <summary>
        /// Determines whether the string has nothing but printable ASCII characters in it.
        /// </summary>
        /// <param name="str">String to evaluate.</param>
        /// <returns>True if all characters are printable ASCII.</returns>
        public static bool IsPrintableAscii(string str) {
            // Linq version: return str.Any(c => c < 0x20 || c > 0x7e);

            MatchCollection matches = sNonPrintableAsciiRegex.Matches(str);
            return matches.Count == 0;
        }

        /// <summary>
        /// Converts high-ASCII bytes to a string.
        /// </summary>
        /// <param name="data">Array of bytes with ASCII data.</param>
        /// <param name="offset">Start offset.</param>
        /// <param name="length">String length.</param>
        /// <returns>Converted string.</returns>
        public static string HighAsciiToString(byte[] data, int offset, int length) {
            StringBuilder sb = new StringBuilder(length);
            for (int i = offset; i < offset + length; i++) {
                sb.Append((char)(data[i] & 0x7f));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Trims whitespace off the end of a StringBuilder.
        /// </summary>
        /// <param name="sb">StringBuilder reference.</param>
        public static void TrimEnd(StringBuilder sb) {
            const string WSPC = " \t\r\n";
            int len = sb.Length;
            while (WSPC.IndexOf(sb[len - 1]) >= 0) {
                len--;
            }
            sb.Length = len;
        }

        /// <summary>
        /// Replaces all occurrences of a string with another string, but only if they appear
        /// outside quoted text.  Useful for replacing structural bits of a JSON string without
        /// messing with the quoted items.  Assumes that quoted quotes (backslash-quote) only
        /// appear inside quoted text.
        /// </summary>
        /// <param name="inStr"></param>
        /// <param name="findStr"></param>
        /// <param name="repStr"></param>
        public static string NonQuoteReplace(string inStr, string findStr, string repStr) {
            // There's probably a better way to do this...
            StringBuilder sb = new StringBuilder(inStr.Length + inStr.Length / 20);
            int cmpLen = findStr.Length;
            bool findStrQuote = findStr.Contains('\"'); // find/rep str switches to in-quote
            Debug.Assert(findStrQuote == repStr.Contains('\"'));

            bool inQuote = false;
            for (int i = 0; i < inStr.Length; i++) {
                char ch = inStr[i];
                if (inQuote) {
                    // Check to see if the double-quote is quoted.  It's safe to back up
                    // one because we don't start in-quote.
                    if (ch == '\"' && inStr[i-1] != '\\') {
                        inQuote = false;
                    } else {
                        // in quoted text, keep going
                    }
                    sb.Append(ch);
                } else {
                    if (string.Compare(inStr, i, findStr, 0, cmpLen) == 0) {
                        sb.Append(repStr);
                        i += cmpLen - 1;
                        inQuote = findStrQuote;
                    } else {
                        if (ch == '"') {
                            // There are no quoted-quotes outside of quotes, so we don't need
                            // to check for a '\'.
                            inQuote = true;
                        }
                        sb.Append(ch);
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Pads a string with trailing spaces so that the total length of the line, including
        /// previous contents, is the length specified.  One trailing space will be added even
        /// if the string's length is >= toLen.
        /// 
        /// You must begin with an empty StringBuilder at the start of each line.
        /// </summary>
        /// <param name="sb">StringBuilder to append to.</param>
        /// <param name="str">String to add.</param>
        /// <param name="toLen">Total line width to pad to.</param>
        public static void AppendPaddedString(StringBuilder sb, string str, int toLen) {
            if (str == null) {
                str = string.Empty;
            }
            int newLen = sb.Length + str.Length;
            if (newLen >= toLen) {
                sb.Append(str);
                sb.Append(' ');
            } else {
                sb.Append(str);
                // would be nice to avoid this allocation/copy
                sb.Append(SPACES.Substring(0, toLen - newLen));
            }
        }

        /// <summary>
        /// Escapes a string for CSV.
        /// </summary>
        /// <param name="str">String to process.</param>
        /// <returns>Escaped string, or an empty string if the input was null.</returns>
        public static string EscapeCSV(string str) {
            if (str == null) {
                return string.Empty;
            }
            bool needQuote = (str.IndexOfAny(CSV_ESCAPE_CHARS) >= 0);
            if (needQuote) {
                return '"' + str.Replace("\"", "\"\"") + '"';
            } else {
                return str;
            }
        }

        /// <summary>
        /// Serializes an integer array into a string.
        /// </summary>
        /// <param name="values">Array to serialize.</param>
        /// <returns>Serialized data.</returns>
        public static string SerializeIntArray(int[] values) {
            StringBuilder sb = new StringBuilder(64);
            sb.Append("int[]");
            for (int i = 0; i < values.Length; i++) {
                sb.Append(',');
                sb.Append(values[i]);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Deserializes an integer array from a string.  Throws an exception if the format
        /// is incorrect.
        /// </summary>
        /// <param name="cereal">Serialized data.</param>
        /// <returns>Integer array with contents.</returns>
        public static int[] DeserializeIntArray(string cereal) {
            string[] splitted = cereal.Split(',');
            if (splitted.Length == 0) {
                throw new Exception("Bad serialized int[]");
            }
            if (splitted[0] != "int[]") {
                throw new Exception("Bad serialized int[], started with " + splitted[0]);
            }
            int[] arr = new int[splitted.Length - 1];
            try {
                for (int i = 1; i < splitted.Length; i++) {
                    arr[i - 1] = int.Parse(splitted[i]);
                }
            } catch (Exception ex) {
                throw new Exception("Bad serialized int[]: " + ex.Message);
            }
            return arr;
        }

        /// <summary>
        /// Converts a char[] to a string, inserting line numbers at the start of each line.
        /// Assumes lines end with '\n' (with or without a preceding '\r').
        /// </summary>
        /// <param name="data">Character data to process.</param>
        /// <returns>String with line numbers.</returns>
        public static string CharArrayToLineNumberedString(char[] data) {
            StringBuilder sb = new StringBuilder(data.Length + data.Length / 40);   // guess
            int lineStart = 0;
            int lineNum = 0;

            for (int i = 0; i < data.Length; i++) {
                if (data[i] == '\n') {
                    sb.AppendFormat("{0,4:D0}  ", ++lineNum);
                    sb.Append(data, lineStart, i - lineStart + 1);
                    lineStart = i + 1;
                }
            }

            return sb.ToString();
        }
    }
}
