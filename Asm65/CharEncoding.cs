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
using System.Text;

namespace Asm65 {
    /// <summary>
    /// Character encoding helper methods.
    /// </summary>
    public static class CharEncoding {
        public const char UNPRINTABLE_CHAR = '\ufffd';

        /// <summary>
        /// Determines whether the byte represents a character in the character set.
        /// </summary>
        public delegate bool InclusionTest(byte val);

        /// <summary>
        /// Converts the byte to a printable character.  Returns UNPRINTABLE_CHAR if the value
        /// does not map to something printable.
        /// </summary>
        /// <remarks>
        /// Yes, I'm assuming it all fits in the Unicode BMP.  Should be a safe assumption
        /// for 8-bit computer character sets.
        /// </remarks>
        public delegate char Convert(byte val);

        //
        // Standard ASCII.
        //
        public static bool IsPrintableLowAscii(byte val) {
            return (val >= 0x20 && val < 0x7f);
        }
        public static bool IsExtendedLowAscii(byte val) {
            return IsPrintableLowAscii(val) || val == 0x0a || val == 0x0d;
        }
        public static char ConvertLowAscii(byte val) {
            if (IsPrintableLowAscii(val)) {
                return (char)val;
            } else {
                return UNPRINTABLE_CHAR;
            }
        }

        //
        // Standard ASCII, but with the high bit set.
        //
        public static bool IsPrintableHighAscii(byte val) {
            return (val >= 0xa0 && val < 0xff);
        }
        public static bool IsExtendedHighAscii(byte val) {
            return IsPrintableHighAscii(val) || val == 0x8a || val == 0x8d;
        }
        public static char ConvertHighAscii(byte val) {
            if (IsPrintableHighAscii(val)) {
                return (char)(val & 0x7f);
            } else {
                return UNPRINTABLE_CHAR;
            }
        }

        //
        // High *or* low ASCII.
        //
        public static char ConvertLowAndHighAscii(byte val) {
            if (IsPrintableLowAscii(val) || IsPrintableHighAscii(val)) {
                return (char)(val & 0x7f);
            } else {
                return UNPRINTABLE_CHAR;
            }
        }

    }
}
