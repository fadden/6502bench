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
using System.Text;

namespace Asm65 {
    /// <summary>
    /// Status flag holder.  Each flag may be known to be zero, known to be one, or
    /// hold an indeterminate value (represented as a negative number).
    /// 
    /// For the 65802/65816, we also keep track of the E flag (emulation bit), even though
    /// that's not actually held in the P register.
    /// 
    /// Note this is a value type, not a reference type.
    /// 
    /// The default value is UNSPECIFIED for all bits.
    /// </summary>
    public struct StatusFlags {
        private TriState16 mState;


        /// <summary>
        /// Flag bits, from processor status register definition.  The 'e' (emulation)
        /// flag from the 65816 is tacked onto the end.
        /// 
        /// The enumerated value matches the bit number in the P register.
        /// </summary>
        public enum FlagBits {
            C = 0,
            Z = 1,
            I = 2,
            D = 3,
            B = 4,      // all CPUs except 65802/65816 in native mode
            X = 4,      // 65802/65816 in native mode
            M = 5,      // 65802/65816 in native mode (always 1 on other CPUs)
            V = 6,
            N = 7,
            E = 8       // not actually part of P-reg; accessible only through XCE
        }

        /// <summary>
        /// Default value (all flags UNSPECIFIED).  A newly-created array of StatusFlags will
        /// all have this value.
        /// </summary>
        public static readonly StatusFlags DefaultValue =
            new StatusFlags { mState = new TriState16(0, 0) };

        /// <summary>
        /// All flags are INDETERMINATE.
        /// </summary>
        public static readonly StatusFlags AllIndeterminate =
            new StatusFlags() { mState = new TriState16(0x01ff, 0x01ff) };

        public int C {
            get {
                return mState.GetBit((int)FlagBits.C);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.C);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.C);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.C);
                } else {
                    mState.SetIndeterminate((int)FlagBits.C);
                }
            }
        }

        public int Z {
            get {
                return mState.GetBit((int)FlagBits.Z);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.Z);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.Z);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.Z);
                } else {
                    mState.SetIndeterminate((int)FlagBits.Z);
                }
            }
        }

        public int I {
            get {
                return mState.GetBit((int)FlagBits.I);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.I);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.I);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.I);
                } else {
                    mState.SetIndeterminate((int)FlagBits.I);
                }
            }
        }

        public int D {
            get {
                return mState.GetBit((int)FlagBits.D);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.D);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.D);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.D);
                } else {
                    mState.SetIndeterminate((int)FlagBits.D);
                }
            }
        }

        /// <summary>
        /// X (index register width) flag.  For an unambiguous value, use IsShortX.
        /// </summary>
        public int X {
            get {
                return mState.GetBit((int)FlagBits.X);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.X);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.X);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.X);
                } else {
                    mState.SetIndeterminate((int)FlagBits.X);
                }
            }
        }

        /// <summary>
        /// M (accumulator width) flag.  For an unambiguous value, use IsShortM.
        /// </summary>
        public int M {
            get {
                return mState.GetBit((int)FlagBits.M);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.M);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.M);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.M);
                } else {
                    mState.SetIndeterminate((int)FlagBits.M);
                }
            }
        }

        public int V {
            get {
                return mState.GetBit((int)FlagBits.V);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.V);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.V);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.V);
                } else {
                    mState.SetIndeterminate((int)FlagBits.V);
                }
            }
        }

        public int N {
            get {
                return mState.GetBit((int)FlagBits.N);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.N);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.N);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.N);
                } else {
                    mState.SetIndeterminate((int)FlagBits.N);
                }
            }
        }

        /// <summary>
        /// E (emulation) flag.  For an unambiguous value, use IsEmulationMode.
        /// </summary>
        public int E {
            get {
                return mState.GetBit((int)FlagBits.E);
            }
            set {
                if (value == 0) {
                    mState.SetZero((int)FlagBits.E);
                } else if (value == 1) {
                    mState.SetOne((int)FlagBits.E);
                } else if (value == TriState16.UNSPECIFIED) {
                    mState.SetUnspecified((int)FlagBits.E);
                } else {
                    mState.SetIndeterminate((int)FlagBits.E);
                }
            }
        }

        public int GetBit(FlagBits index) {
            return mState.GetBit((int) index);
        }

        /// <summary>
        /// Returns true if the current processor status flags are configured for a short
        /// (8-bit) accumulator.
        /// </summary>
        /// <remarks>
        /// This is where we decide how to treat ambiguous status flags.
        /// </remarks>
        public bool IsShortM {
            get {
                // E==1 --> true (we're in emulation mode)
                // E==0 || E==? : native / assumed native
                //   M==1 || M==? --> true (native mode, configured short or assumed short)
                //   M==0 --> false (native mode, configured long)
                return (E == 1) || (M != 0);
            }
        }

        /// <summary>
        /// Returns true if the current processor status flags are configured for short
        /// (8-bit) X/Y registers.
        /// </summary>
        public bool IsShortX {
            get {
                // (same logic as ShortM)
                return (E == 1) || (X != 0);
            }
        }

        /// <summary>
        /// Returns true if the current processor status flags are configured for execution
        /// in native mode.
        /// </summary>
        public bool IsEmulationMode {
            get {
                // E==1 : emulation --> true
                // E==0 || E==? : native / assumed native --> false
                return E == 1;
            }
        }

        /// <summary>
        /// Access the value as a single integer.  Used for serialization.
        /// </summary>
        public int AsInt {
            get {
                return mState.AsInt;
            }
        }

        /// <summary>
        /// Set the value from an integer.  Used for serialization.
        /// </summary>
        public static StatusFlags FromInt(int value) {
            if ((value & ~0x01ff01ff) != 0) {
                throw new InvalidOperationException("Bad StatusFlags value " +
                    value.ToString("x8"));
            }
            StatusFlags newFlags = new StatusFlags();
            newFlags.mState.AsInt = value;
            return newFlags;
        }

        /// <summary>
        /// Merge a set of status flags into this one.
        /// </summary>
        public void Merge(StatusFlags other) {
            mState.Merge(other.mState);
        }

        /// <summary>
        /// Applies flags, overwriting existing values.  This will set one or more flags
        /// to 0, 1, or indeterminate.  Unspecified (0/0) values have no effect.
        /// 
        /// This is useful when merging "overrides" in.
        /// </summary>
        public void Apply(StatusFlags overrides) {
            mState.Apply(overrides.mState);
        }

        /// <summary>
        /// Returns a string representation of the flags.
        /// </summary>
        /// <param name="showMXE">If set, include the 'E' flag, and show M/X.</param>
        public string ToString(bool showMXE) {
            StringBuilder sb = new StringBuilder(showMXE ? 10 : 8);
            sb.Append("-?nN"[N + 2]);
            sb.Append("-?vV"[V + 2]);
            sb.Append(showMXE ? "-?mM"[M + 2] : '-');
            sb.Append(showMXE ? "-?xX"[X + 2] : '-');
            sb.Append("-?dD"[D + 2]);
            sb.Append("-?iI"[I + 2]);
            sb.Append("-?zZ"[Z + 2]);
            sb.Append("-?cC"[C + 2]);
            if (showMXE) {
                sb.Append(' ');
                sb.Append("-?eE"[E + 2]);
            }
            return sb.ToString();
        }


        public static bool operator ==(StatusFlags a, StatusFlags b) {
            return a.mState == b.mState;
        }
        public static bool operator !=(StatusFlags a, StatusFlags b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is StatusFlags && this == (StatusFlags)obj;
        }
        public override int GetHashCode() {
            return mState.GetHashCode();
        }

        public override string ToString() {
            return ToString(true);
            // + " [" + mState.ToString() + "]"
        }
    }
}
