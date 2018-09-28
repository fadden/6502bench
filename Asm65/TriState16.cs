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
    /// <summary>
    /// A value with 16 tri-state bits.
    /// </summary>
    public struct TriState16 {
        /// <summary>
        /// Two 16-bit values.  The low 16 bits indicate that bit N is zero, the high
        /// 16 bits indicate that bit N is one.  If neither or both are set, the value
        /// is undetermined and could be either.
        ///
        /// While 0/0 and 1/1 both represent an indeterminate value, they're not equivalent.
        /// When two values are merged, a 0/0 bit has no effect on the result, while
        /// a 1/1 bit will force the merged bit to be indeterminate.  We use UNSPECIFIED for
        /// 0/0 and INDETERMINATE for 1/1.
        /// 
        /// The default value for new instances is UNSPECIFIED for all bits.
        /// </summary>
        private ushort mZero, mOne;

        public const int INDETERMINATE = -1;
        public const int UNSPECIFIED = -2;

        /// <summary>
        /// Constructor; sets initial zero/one values.
        /// </summary>
        /// <param name="zeroes">16-bit value, with bits set for each known-zero.</param>
        /// <param name="ones">16-bit value, with bits set for each known-one.</param>
        public TriState16(ushort zeroes, ushort ones) {
            mZero = zeroes;
            mOne = ones;
        }

        /// <summary>
        /// Access the value as a single integer.  Used for serialization.
        /// </summary>
        public int AsInt {
            get {
                return (int)mZero | ((int)mOne << 16);
            }
            set {
                mZero = (ushort) value;
                mOne = (ushort) (value >> 16);
            }
        }

        /// <summary>
        /// Sets bit N to zero.
        /// </summary>
        public void SetZero(int bit) {
            Debug.Assert(bit >= 0 && bit < 16);

            // clear 1-flag, set 0-flag
            //mValue = (mValue & ~(1U << (bit + 16))) | (1U << bit);
            mZero |= (ushort) (1U << bit);
            mOne &= (ushort) ~(1U << bit);
        }

        /// <summary>
        /// Sets bit N to one.
        /// </summary>
        public void SetOne(int bit) {
            Debug.Assert(bit >= 0 && bit < 16);

            // clear 0-flag, set 1-flag
            //mValue = (mValue & ~(1U << bit)) | (1U << (bit + 16));
            mZero &= (ushort)~(1U << bit);
            mOne |= (ushort)(1U << bit);
        }

        /// <summary>
        /// Sets bit N to indeterminate.
        /// </summary>
        public void SetIndeterminate(int bit) {
            Debug.Assert(bit >= 0 && bit < 16);

            // set both flags
            mZero |= (ushort)(1U << bit);
            mOne |= (ushort)(1U << bit);
        }

        /// <summary>
        /// Sets bit N to unspecified.
        /// </summary>
        public void SetUnspecified(int bit) {
            Debug.Assert(bit >= 0 && bit < 16);

            // clear both flags
            mZero &= (ushort)~(1U << bit);
            mOne &= (ushort)~(1U << bit);
        }

        /// <summary>
        /// Merges bit states.
        /// </summary>
        /// <param name="other">Value to merge in.</param>
        public void Merge(TriState16 other) {
            //mValue |= other.mValue;
            mZero |= other.mZero;
            mOne |= other.mOne;
        }

        /// <summary>
        /// Applies a set of bits to an existing set, overriding any bits that aren't set
        /// to "unspecified" in the input.
        /// </summary>
        public void Apply(TriState16 overrides) {
            ushort mask = (ushort) ~(overrides.mZero | overrides.mOne);
            mZero = (ushort)((mZero & mask) | overrides.mZero);
            mOne = (ushort)((mOne & mask) | overrides.mOne);
        }

        /// <summary>
        /// Returns 0, 1, -1, or -2 depending on whether the specified bit is 0, 1,
        /// indeterminate, or unspecified.
        /// </summary>
        public int GetBit(int bit) {
            bool zero = ((mZero >> bit) & 0x01) != 0;
            bool one = ((mOne >> bit) & 0x01) != 0;
            if (zero ^ one) {
                // Only one of the bits is set.
                if (one) {
                    return 1;
                } else {
                    return 0;
                }
            } else {
                // Both or neither are set.
                if (zero) {
                    return INDETERMINATE;
                } else {
                    return UNSPECIFIED;
                }
            }
        }


        public static bool operator ==(TriState16 a, TriState16 b) {
            return a.mZero == b.mZero && a.mOne == b.mOne;
        }
        public static bool operator !=(TriState16 a, TriState16 b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is TriState16 && this == (TriState16)obj;
        }
        public override int GetHashCode() {
            return (mOne << 16) | mZero;
        }

        public override string ToString() {
            return mZero.ToString("x4") + "-" + mOne.ToString("x4");
        }
    }
}
