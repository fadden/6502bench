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

namespace CommonUtil {
    public class BitTwiddle {
        /// <summary>
        /// Returns the argument, rounded up to the next highest power of 2.  If the argument
        /// is an exact power of two, it is returned unmodified.
        /// </summary>
        public static int RoundUpPowerOf2(int val) {
            val--;              // handle exact power of 2 case; works correctly for val=0
            return NextHighestPowerOf2(val);
        }

        /// <summary>
        /// Returns the first power of 2 value that is higher than val.  If the argument is
        /// an exact power of two, the next power of 2 is returned.
        /// </summary>
        /// <remarks>
        /// Classic bit-twiddling approach.  I can't find a "count leading zeroes" function
        /// in C# that turns into a CPU instruction; if we had that, we could just use 1<<N.
        /// </remarks>
        public static int NextHighestPowerOf2(int val) {
            val |= val >> 1;    // "smear" bits across integer
            val |= val >> 2;
            val |= val >> 4;
            val |= val >> 8;
            val |= val >> 16;
            return val + 1;
        }

        /// <summary>
        /// Returns an integer in which the only bit set is the least-significant set bit in
        /// the argument.
        /// </summary>
        /// <remarks>
        /// If you pass in 10110100, this will return 00000100.
        ///
        /// Two's complement negation inverts and adds one, so 01100 --> 10011+1 --> 10100.  The
        /// only set bit they have in common is the one we want.
        /// </remarks>
        public static int IsolateLeastSignificantOne(int val) {
            return val & -val;
        }

        /// <summary>
        /// Returns the number of bits that are set in the argument.
        /// </summary>
        /// <remarks>
        /// If you pass in 10110100, this will return 4.
        /// 
        /// This comes from http://aggregate.org/MAGIC/#Population%20Count%20(Ones%20Count) .
        /// </remarks>
        public static int CountOneBits(int val) {
            // 32-bit recursive reduction using SWAR...
            // but first step is mapping 2-bit values
            // into sum of 2 1-bit values in sneaky way
            val -= ((val >> 1) & 0x55555555);
            val = (((val >> 2) & 0x33333333) + (val & 0x33333333));
            val = (((val >> 4) + val) & 0x0f0f0f0f);
            val += (val >> 8);
            val += (val >> 16);
            return (val & 0x0000003f);
        }

        /// <summary>
        /// Returns the number of trailing zero bits in the argument.
        /// </summary>
        /// <remarks>
        /// If you pass in 10110100, this will return 2.
        /// 
        /// Also from http://aggregate.org/MAGIC/ .
        /// </remarks>
        public static int CountTrailingZeroes(int val) {
            // Lazy: reduce to least-significant 1 bit, subtract one to clear that bit and set
            // all the bits to the right of it, then just count the 1s.
            return CountOneBits(IsolateLeastSignificantOne(val) - 1);
        }
    }
}
