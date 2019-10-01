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

namespace SourceGen {
    /// <summary>
    /// Format descriptor for data items and instruction operands.  Instances are immutable.
    ///
    /// A list of these is saved as part of the project definition.  Code and data that
    /// doesn't have one of these will be formatted with default behavior.  For data that
    /// means a single hexadecimal byte.
    ///
    /// These are referenced from the project and the Anattribs array.  Entries in the
    /// latter may come from the project (as specified by the user), or be auto-generated
    /// by the data analysis pass.
    ///
    /// There may be a large number of these, so try to keep the size down.  These are usually
    /// stored in lists, not arrays, so declaring as a struct wouldn't help with that.
    ///
    /// IMPORTANT: The stringified names of the enum values are currently serialized into
    /// the project file.  DO NOT rename members of the enumerations without creating an
    /// upgrade path.  If new values are added, the project file version number should
    /// be incremented.
    /// </summary>
    public class FormatDescriptor {
        /// <summary>
        /// General data type.  Generally corresponds to the pseudo-opcode.
        /// 
        /// The UI only allows big-endian values in certain situations.  Internally we want
        /// to be orthogonal in case the policy changes.
        /// </summary>
        public enum Type : byte {
            Unknown = 0,
            REMOVE,             // special type, only used by operand editor
            Default,            // means "unformatted", same as not having a FormatDescriptor

            NumericLE,          // 1-4 byte number, little-endian
            NumericBE,          // 1-4 byte number, big-endian

            StringGeneric,      // character string
            StringReverse,      // character string, in reverse order
            StringNullTerm,     // C-style null-terminated string
            StringL8,           // string with 8-bit length prefix
            StringL16,          // string with 16-bit length prefix
            StringDci,          // string terminated by flipped high bit (Dextral Char Inverted)

            Dense,              // raw data, represented as compactly as possible
            Fill                // fill memory with a value
        }

        /// <summary>
        /// Additional data type detail.  Generally affects the operand.
        /// 
        /// Some things are extracted from the data itself, e.g. we don't need to specify
        /// what value to use for Fill.
        /// </summary>
        public enum SubType : byte {
            None = 0,
            ASCII_GENERIC,      // internal place-holder, used when loading older projects

            // NumericLE/BE; default is "raw", which can have a context-specific display format
            Hex,
            Decimal,
            Binary,
            Address,            // wants to be an address, but no symbol defined
            Symbol,             // symbolic ref; replace with Expression, someday?

            // Strings and NumericLE/BE (single character)
            Ascii,              // ASCII (high bit clear)
            HighAscii,          // ASCII (high bit set)
            C64Petscii,         // C64 PETSCII (lower case $41-5a, upper case $c1-da)
            C64Screen,          // C64 screen code

            // Dense; no sub-types

            // Fill; default is non-ignore
            Ignore              // TODO(someday): use this for "don't care" sections
        }

        // Maximum length of a NumericLE/BE item (32-bit value or 4-byte instruction).
        public const int MAX_NUMERIC_LEN = 4;

        // Create some "stock" descriptors.  For simple cases we return one of these
        // instead of allocating a new object.
        private static FormatDescriptor ONE_DEFAULT = new FormatDescriptor(1,
            Type.Default, SubType.None);
        private static FormatDescriptor ONE_NONE = new FormatDescriptor(1,
            Type.NumericLE, SubType.None);
        private static FormatDescriptor ONE_HEX = new FormatDescriptor(1,
            Type.NumericLE, SubType.Hex);
        private static FormatDescriptor ONE_DECIMAL = new FormatDescriptor(1,
            Type.NumericLE, SubType.Decimal);
        private static FormatDescriptor ONE_BINARY = new FormatDescriptor(1,
            Type.NumericLE, SubType.Binary);
        private static FormatDescriptor ONE_LOW_ASCII = new FormatDescriptor(1,
            Type.NumericLE, SubType.Ascii);

        /// <summary>
        /// Length, in bytes, of the data to be formatted.
        /// 
        /// For an instruction, this must match what the code analyzer found as the length
        /// of the entire instruction, or the descriptor will be ignored.
        /// 
        /// For data items, this determines the length of the formatted region.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Primary format.  The actual data must match the format:
        /// - Numeric values must be 1-4 bytes.
        /// - String values must be ASCII characters with a common high bit (although
        ///   the start or end may diverge from this based on the sub-type).
        /// - Fill areas must contain identical bytes.
        /// </summary>
        public Type FormatType { get; private set; }

        /// <summary>
        /// Sub-format specifier.  Each primary format has specific sub-formats, but we
        /// lump them all together for convenience.
        /// </summary>
        public SubType FormatSubType { get; private set; }

        /// <summary>
        /// Symbol reference for Type=Numeric SubType=Symbol.  null otherwise.
        /// 
        /// Numeric values, such as addresses and constants, can be generated with an
        /// expression.  Currently we only support using a single symbol, but the goal
        /// is to allow free-form expressions like "(sym1+sym2+$80)/3".
        ///
        /// If the symbol exists, the symbol's name will be shown, possibly with an adjustment
        /// to make the symbol value match the operand or data item.
        ///
        /// Note this reference has a "part" modifier, so we can use it for e.g. "#>label".
        /// </summary>
        public WeakSymbolRef SymbolRef { get; private set; }

        // Crude attempt to see how effective the prefab object creation is.  Note we create
        // these for DefSymbols, so there will be one prefab for every platform symbol entry.
        public static int DebugCreateCount { get; private set; }
        public static int DebugPrefabCount { get; private set; }
        public static void DebugPrefabBump(int adj=1) {
            DebugCreateCount += adj;
            DebugPrefabCount += adj;
        }


        /// <summary>
        /// Constructor for base type data item.
        /// </summary>
        /// <param name="length">Length, in bytes.</param>
        /// <param name="fmt">Format type.</param>
        /// <param name="subFmt">Format sub-type.</param>
        private FormatDescriptor(int length, Type fmt, SubType subFmt) {
            Debug.Assert(length > 0);
            Debug.Assert(length <= MAX_NUMERIC_LEN || !IsNumeric);
            Debug.Assert(fmt != Type.Default || length == 1);

            Length = length;
            FormatType = fmt;
            FormatSubType = subFmt;
        }

        /// <summary>
        /// Constructor for symbol item.
        /// </summary>
        /// <param name="length">Length, in bytes.</param>
        /// <param name="sym">Weak symbol reference.</param>
        /// <param name="isBigEndian">Set to true for big-endian data.</param>
        private FormatDescriptor(int length, WeakSymbolRef sym, bool isBigEndian) {
            Debug.Assert(sym != null);
            Debug.Assert(length > 0 && length <= MAX_NUMERIC_LEN);
            Length = length;
            FormatType = isBigEndian ? Type.NumericBE : Type.NumericLE;
            FormatSubType = SubType.Symbol;
            SymbolRef = sym;
        }

        /// <summary>
        /// Returns a descriptor with the requested characteristics.  For common cases this
        /// returns a pre-allocated object, for less-common cases this allocates a new object.
        /// 
        /// Objects are immutable and do not specify a file offset, so they may be re-used
        /// by the caller.
        /// </summary>
        /// <param name="length">Length, in bytes.</param>
        /// <param name="fmt">Format type.</param>
        /// <param name="subFmt">Format sub-type.</param>
        /// <returns>New or pre-allocated descriptor.</returns>
        public static FormatDescriptor Create(int length, Type fmt, SubType subFmt) {
            DebugCreateCount++;
            DebugPrefabCount++;
            if (length == 1) {
                if (fmt == Type.Default) {
                    Debug.Assert(subFmt == SubType.None);
                    return ONE_DEFAULT;
                } else if (fmt == Type.NumericLE) {
                    switch (subFmt) {
                        case SubType.None:
                            return ONE_NONE;
                        case SubType.Hex:
                            return ONE_HEX;
                        case SubType.Decimal:
                            return ONE_DECIMAL;
                        case SubType.Binary:
                            return ONE_BINARY;
                        case SubType.Ascii:
                            return ONE_LOW_ASCII;
                    }
                }
            }
            // For a new file, this will be mostly strings and Fill.
            DebugPrefabCount--;
            return new FormatDescriptor(length, fmt, subFmt);
        }

        /// <summary>
        /// Returns a descriptor with a symbol.
        /// </summary>
        /// <param name="length">Length, in bytes.</param>
        /// <param name="sym">Weak symbol reference.</param>
        /// <param name="isBigEndian">Set to true for big-endian data.</param>
        /// <returns>New or pre-allocated descriptor.</returns>
        public static FormatDescriptor Create(int length, WeakSymbolRef sym, bool isBigEndian) {
            DebugCreateCount++;
            return new FormatDescriptor(length, sym, isBigEndian);
        }

        /// <summary>
        /// True if the descriptor is okay to use on an instruction operand.  The CPU only
        /// understands little-endian numeric values, so that's all we allow.
        /// </summary>
        public bool IsValidForInstruction {
            get {
                switch (FormatType) {
                    case Type.Default:
                    case Type.NumericLE:
                    //case Type.NumericBE:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True if the FormatDescriptor has a symbol.
        /// </summary>
        public bool HasSymbol {
            get {
                Debug.Assert(SymbolRef == null || (IsNumeric && FormatSubType == SubType.Symbol));
                return SymbolRef != null;
            }
        }

        /// <summary>
        /// True if the FormatDescriptor is a numeric type (NumericLE or NumericBE).
        /// </summary>
        public bool IsNumeric {
            get {
                return FormatType == Type.NumericLE || FormatType == Type.NumericBE;
            }
        }

        /// <summary>
        /// True if the FormatDescriptor is a string type.
        /// </summary>
        public bool IsString {
            get {
                switch (FormatType) {
                    case Type.StringGeneric:
                    case Type.StringReverse:
                    case Type.StringNullTerm:
                    case Type.StringL8:
                    case Type.StringL16:
                    case Type.StringDci:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True if the FormatDescriptor is a string or character.
        /// </summary>
        public bool IsStringOrCharacter {
            get {
                switch (FormatSubType) {
                    case SubType.ASCII_GENERIC:
                    case SubType.Ascii:
                    case SubType.HighAscii:
                    case SubType.C64Petscii:
                    case SubType.C64Screen:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True if the FormatDescriptor has a symbol or is Numeric/Address.
        /// </summary>
        public bool HasSymbolOrAddress {
            // Derived from other fields, so you can ignore this in equality tests.  This is
            // of interest to undo/redo, since changing a symbol reference can affect data scan.
            get {
                return HasSymbol || FormatSubType == SubType.Address;
            }
        }

        /// <summary>
        /// Numeric base specific by format/sub-format.  Returns 16 when uncertain.
        /// </summary>
        public int NumBase {
            get {
                if (FormatType != Type.NumericLE && FormatType != Type.NumericBE) {
                    Debug.Assert(false);
                    return 16;
                }
                switch (FormatSubType) {
                    case SubType.None:
                    case SubType.Hex:
                        return 16;
                    case SubType.Decimal:
                        return 10;
                    case SubType.Binary:
                        return 2;
                    default:
                        Debug.Assert(false);
                        return 16;
                }
            }
        }

        /// <summary>
        /// Returns the FormatSubType enum constant for the specified numeric base.
        /// </summary>
        /// <param name="numBase">Base (2, 10, or 16).</param>
        /// <returns>Enum value.</returns>
        public static SubType GetSubTypeForBase(int numBase) {
            switch (numBase) {
                case 2: return SubType.Binary;
                case 10: return SubType.Decimal;
                case 16: return SubType.Hex;
                default:
                    Debug.Assert(false);
                    return SubType.Hex;
            }
        }

        /// <summary>
        /// Generates a string describing the format, suitable for use in the UI.
        /// </summary>
        public string ToUiString() {
            // NOTE: this should be made easier to localize

            if (IsString) {
                string descr;
                switch (FormatSubType) {
                    case SubType.Ascii:
                        descr = "ASCII";
                        break;
                    case SubType.HighAscii:
                        descr = "ASCII (high)";
                        break;
                    case SubType.C64Petscii:
                        descr = "C64 PETSCII";
                        break;
                    case SubType.C64Screen:
                        descr = "C64 Screen";
                        break;
                    default:
                        descr = "???";
                        break;
                }
                switch (FormatType) {
                    case Type.StringGeneric:
                        descr += " string";
                        break;
                    case Type.StringReverse:
                        descr += " string (reverse)";
                        break;
                    case Type.StringNullTerm:
                        descr += " string (null term)";
                        break;
                    case Type.StringL8:
                        descr += " string (1-byte len)";
                        break;
                    case Type.StringL16:
                        descr += " string (2-byte len)";
                        break;
                    case Type.StringDci:
                        descr += " string (DCI)";
                        break;
                    default:
                        descr += " ???";
                        break;
                }
                return descr;
            }

            switch (FormatSubType) {
                case SubType.None:
                    switch (FormatType) {
                        case Type.Default:
                        case Type.NumericLE:
                            return "Numeric (little-endian)";
                        case Type.NumericBE:
                            return "Numeric (big-endian)";
                        case Type.Dense:
                            return "Dense";
                        case Type.Fill:
                            return "Fill";
                        default:
                            // strings handled earlier
                            return "???";
                    }
                case SubType.Hex:
                    return "Numeric, Hex";
                case SubType.Decimal:
                    return "Numeric, Decimal";
                case SubType.Binary:
                    return "Numeric, Binary";
                case SubType.Address:
                    return "Address";
                case SubType.Symbol:
                    if (SymbolRef.IsVariable) {
                        return "Local var \"" + SymbolRef.Label + "\"";
                    } else {
                        return "Symbol \"" + SymbolRef.Label + "\"";
                    }
                case SubType.Ascii:
                    return "Numeric, ASCII";
                case SubType.HighAscii:
                    return "Numeric, ASCII (high)";
                case SubType.C64Petscii:
                    return "Numeric, C64 PETSCII";
                case SubType.C64Screen:
                    return "Numeric, C64 Screen";

                default:
                    return "???";
            }
        }

        public override string ToString() {
            return "[FmtDesc: len=" + Length + " fmt=" + FormatType + " sub=" + FormatSubType +
                " sym=" + SymbolRef + "]";
        }


        public static bool operator ==(FormatDescriptor a, FormatDescriptor b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            return a.Length == b.Length && a.FormatType == b.FormatType &&
                a.FormatSubType == b.FormatSubType && a.SymbolRef == b.SymbolRef;
        }
        public static bool operator !=(FormatDescriptor a, FormatDescriptor b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is FormatDescriptor && this == (FormatDescriptor)obj;
        }
        public override int GetHashCode() {
            int hashCode = 0;
            if (SymbolRef != null) {
                hashCode = SymbolRef.GetHashCode();
            }
            hashCode ^= Length;
            hashCode ^= (int)FormatType;
            hashCode ^= (int)FormatSubType;
            return hashCode;
        }


        /// <summary>
        /// Debugging utility function to dump a sorted list of objects.
        /// </summary>
        public static void DebugDumpSortedList(SortedList<int, FormatDescriptor> list) {
            if (list == null) {
                Debug.WriteLine("FormatDescriptor list is empty");
                return;
            }
            Debug.WriteLine("FormatDescriptor list (" + list.Count + " entries)");
            foreach (KeyValuePair<int, FormatDescriptor> kvp in list) {
                int offset = kvp.Key;
                FormatDescriptor dfd = kvp.Value;
                Debug.WriteLine(" +" + offset.ToString("x6") + ",+" +
                    (offset + dfd.Length - 1).ToString("x6") + ": " + dfd.FormatType +
                    "(" + dfd.FormatSubType + ")");
            }
        }
    }
}
