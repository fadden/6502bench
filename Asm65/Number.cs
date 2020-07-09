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

namespace Asm65 {
    public class Number {
        /// <summary>
        /// Parses an integer in a variety of formats (hex, decimal, binary).  We allow
        /// hex to be identified with a leading '$' as well as "0x".
        ///
        /// Trim whitespace before calling here.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="val">Integer value of string.</param>
        /// <param name="intBase">What base the string was in (2, 10, or 16).</param>
        /// <returns>True if the parsing was successful.</returns>
        public static bool TryParseInt(string str, out int val, out int intBase) {
            if (string.IsNullOrEmpty(str)) {
                val = intBase = 0;
                return false;
            }

            if (str[0] == '$') {
                intBase = 16;
                str = str.Substring(1);     // Convert functions don't like '$'
            } else if (str.Length > 2 && str[0] == '0' && (str[1] == 'x' || str[1] == 'X')) {
                intBase = 16;
            } else if (str[0] == '%') {
                intBase = 2;
                str = str.Substring(1);     // Convert functions don't like '%'
            } else {
                intBase = 10;               // try it as decimal
            }

            try {
                val = Convert.ToInt32(str, intBase);
                //Debug.WriteLine("GOT " + val + " - " + intBase);
            } catch (Exception) {
                //Debug.WriteLine("TryParseInt failed on '" + str + "': " + ex.Message);
                val = 0;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parses a hexadecimal integer.  Leading '$' or "0x" will be ignored.
        ///
        /// Trim whitespace before calling here.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="val">Integer value of string.</param>
        /// <returns>True if the parsing was successful.</returns>
        public static bool TryParseIntHex(string str, out int val) {
            if (str.Length == 0) {
                val = 0;
                return false;
            }
            if (str[0] == '$') {
                str = str.Substring(1);
            }
            // leading "0x" will be handled by Convert

            try {
                val = Convert.ToInt32(str, 16);
            } catch (Exception) {
                val = 0;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses a long integer in a variety of formats (hex, decimal, binary).  We allow
        /// hex to be identified with a leading '$' as well as "0x".
        ///
        /// Trim whitespace before calling here.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <param name="val">Integer value of string.</param>
        /// <param name="intBase">What base the string was in (2, 10, or 16).</param>
        /// <returns>True if the parsing was successful.</returns>
        public static bool TryParseLong(string str, out long val, out int intBase) {
            if (string.IsNullOrEmpty(str)) {
                val = intBase = 0;
                return false;
            }

            if (str[0] == '$') {
                intBase = 16;
                str = str.Substring(1);     // Convert functions don't like '$'
            } else if (str.Length > 2 && str[0] == '0' && (str[1] == 'x' || str[1] == 'X')) {
                intBase = 16;
            } else if (str[0] == '%') {
                intBase = 2;
                str = str.Substring(1);     // Convert functions don't like '%'
            } else {
                intBase = 10;               // try it as decimal
            }

            try {
                val = Convert.ToInt64(str, intBase);
                //Debug.WriteLine("GOT " + val + " - " + intBase);
            } catch (Exception) {
                //Debug.WriteLine("TryParseInt failed on '" + str + "': " + ex.Message);
                val = 0;
                return false;
            }

            return true;
        }
    }
}
