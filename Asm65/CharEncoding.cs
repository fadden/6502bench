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
using System.Text;

namespace Asm65 {
    /// <summary>
    /// Character encoding helper methods.
    /// </summary>
    public static class CharEncoding {
        public const char UNPRINTABLE_CHAR = '\ufffd';  // Unicode REPLACEMENT CHARACTER

        /// <summary>
        /// Determines whether the byte represents a member of the character set.  The
        /// specifics (e.g. printable only) are defined by the method.
        /// </summary>
        public delegate bool InclusionTest(byte val);

        /// <summary>
        /// Converts the byte to a printable character.  Returns UNPRINTABLE_CHAR if the value
        /// does not map to something printable.
        /// </summary>
        /// <remarks>
        /// Yes, I'm assuming it all fits in a UTF-16 char.  PETSCII has some glyphs that
        /// aren't part of the BMP, but we're targeting a variety of cross-assemblers with
        /// potentially different notions of Unicode mappings, so anything non-ASCII is
        /// getting hexified anyway.
        /// </remarks>
        public delegate char Convert(byte val);

        /// <summary>
        /// Character encoding.
        /// </summary>
        public enum Encoding {
            Unknown = 0,
            Ascii,
            HighAscii,
            C64Petscii,
            C64ScreenCode,
        }

        //
        // Plain ASCII.
        //
        // We recognize BELL, LF, and CR as control characters that may be present in
        // text strings.  This allows use to generate:
        //
        //  .str "hello",$0d
        //
        // instead of:
        //
        //  .str "hello"
        //  .dd1  $0d
        //
        public static bool IsPrintableAscii(byte val) {
            return (val >= 0x20 && val < 0x7f);
        }
        public static bool IsExtendedAscii(byte val) {
            return IsPrintableAscii(val) || val == 0x07 || val == 0x0a || val == 0x0d;
        }
        public static char ConvertAscii(byte val) {
            if (IsPrintableAscii(val)) {
                return (char)val;
            } else {
                return UNPRINTABLE_CHAR;
            }
        }

        //
        // High ASCII: plain ASCII with the high bit set.
        //
        public static bool IsPrintableHighAscii(byte val) {
            return (val >= 0xa0 && val < 0xff);
        }
        public static bool IsExtendedHighAscii(byte val) {
            return IsPrintableHighAscii(val) || val == 0x87 || val == 0x8a || val == 0x8d;
        }
        public static char ConvertHighAscii(byte val) {
            if (IsPrintableHighAscii(val)) {
                return (char)(val & 0x7f);
            } else {
                return UNPRINTABLE_CHAR;
            }
        }

        //
        // High and/or low ASCII.
        //
        public static bool IsPrintableLowOrHighAscii(byte val) {
            return IsPrintableAscii((byte)(val & 0x7f));
        }
        public static bool IsExtendedLowOrHighAscii(byte val) {
            return IsExtendedAscii((byte)(val & 0x7f));
        }
        public static char ConvertLowAndHighAscii(byte val) {
            if (IsPrintableAscii(val) || IsPrintableHighAscii(val)) {
                return (char)(val & 0x7f);
            } else {
                return UNPRINTABLE_CHAR;
            }
        }

        //
        // ATASCII (Atari 400/800)
        //
        // Substantially similar to ASCII, but with printable symbols in the control character
        // range ($00-1f).  Characters $60 and $7b-7f don't correspond to ASCII symbols.
        //
        // Characters with the high bit set are shown with colors reversed.
        //


        //
        // PETSCII (C64 variant)
        //
        // Assemblers like ACME use the C64 character set 2, a/k/a shifted mode, lower case
        // mode, or text mode.
        //
        // Comparison to ASCII:
        //  $00-1f: control codes, many with C64-specific meanings
        //  $20-3f: same as ASCII
        //  $40-5f: lower case letters (rather than upper case); backslash, caret, and underscore
        //   replaced with non-ASCII symbols (though the up-arrow in place of caret is close)
        //  $60-7f: upper case letters (rather than lower case); backquote, curly braces,
        //   vertical bar, and tilde replaced with non-ASCII symbols
        //  $80-9f: more control codes
        //  $a0-bf: non-ASCII symbols
        //  $c0-df: clone of $60-7f; by convention this is used for upper case, since it's
        //   equal to lower case with the high bit set
        //  $e0-ff: non-ASCII symbols (mostly a clone of $a0-bf)
        //
        // The printable ASCII set (glyphs in [$20,$7e]) is [$20,$5b]+$5d+[$c1,$da].
        // (Looks like the Pet had $5c=backslash, but C64 went with a \u00a3 POUND SIGN instead.)
        // Anything outside that range will get printed as hex to ensure proper conversion.
        //
        // Note for the pedantic: in ASCII-1963, up-arrow and left-arrow characters were
        // assigned to the caret and underscore values.  So arguably those are "ASCII" as
        // well, unless you're sane and define ASCII more narrowly.
        //
        // Control codes that we might expect to appear in the middle of a string:
        //  $05 1c 1e 1f 81 90 95 96 97 98 99 9a 9b 9c 9e 9f - set text color
        //  $93 - clear
        //  $12 92 - reverse on/off
        //  $07 0a 0d - bell, LF, CR (note CR is favored for EOL)
        //
        // Other Commodore systems use variants on PETSCII, but the ASCII correspondence remains
        // the same -- only the non-ASCII symbols change.  (On the original PET, $60-7f was a
        // duplicate of $20-3f rather than a duplicate of the upper-case letters, which might be
        // why $c0-df is preferred for upper case.)
        //
        // For full details, see the chart at https://www.aivosto.com/articles/petscii.pdf
        //
        private static bool[] sPrintablePetscii = CreatePrintablePetsciiMap();
        private static bool[] sExtendedPetscii = CreateExtendedPetsciiMap();
        private static bool[] CreatePrintablePetsciiMap() {
            bool[] map = new bool[256];
            for (int i = 0x20; i <= 0x5b; i++) {
                map[i] = true;
            }
            map[0x5d] = true;
            for (int i = 0xc1; i <= 0xda; i++) {
                map[i] = true;
            }
            return map;
        }
        private static bool[] CreateExtendedPetsciiMap() {
            bool[] map = CreatePrintablePetsciiMap();
            // control codes that we might expect to find in strings
            map[0x05] = map[0x1c] = map[0x1e] = map[0x1f] = map[0x81] = map[0x90] = map[0x95] =
                map[0x96] = map[0x97] = map[0x98] = map[0x99] = map[0x9a] = map[0x9b] =
                map[0x9c] = map[0x9e] = map[0x9f] = true;
            map[0x93] = map[0x12] = map[0x92] = true;
            map[0x07] = map[0x0a] = map[0x0d] = true;
            return map;
        }
        public static bool IsPrintablePetscii(byte val) {
            return sPrintablePetscii[val];
        }
        public static bool IsExtendedPetscii(byte val) {
            return sExtendedPetscii[val];
        }

        //
        // C64 Screen Codes
        //
        // Using character set 2, which includes lower case letters.
        //
        //  $00-1f: lower case letters (PETSCII $40-5f)
        //  $20-3f: same as ASCII (PETSCII $20-3f)
        //  $40-5f: upper case letters (PETSCII $60-7f)
        //  $60-7f: non-ASCII symbols (PETSCII $a0-bf)
        //
        // With the high bit set, character colors are reversed.  The printable ASCII set
        // is [$00,$1b]+$1d+[$20,$3f]+[$41,$5a].  By definition, only printable characters
        // are included in the set, so there are no control codes.
        //
        // For full details, see the chart at https://www.aivosto.com/articles/petscii.pdf
        //
        private static bool[] sPrintableScreenCode = CreatePrintableScreenCodeMap();
        private static bool[] CreatePrintableScreenCodeMap() {
            bool[] map = new bool[256];
            for (int i = 0x00; i <= 0x1b; i++) {
                map[i] = true;
            }
            map[0x1d] = true;
            for (int i = 0x20; i <= 0x3f; i++) {
                map[i] = true;
            }
            for (int i = 0x41; i <= 0x5a; i++) {
                map[i] = true;
            }
            return map;
        }
        public static bool IsPrintableScreenCode(byte val) {
            return sPrintableScreenCode[val];
        }
        public static bool IsExtendedScreenCode(byte val) {
            return sPrintableScreenCode[val];
        }
    }
}
