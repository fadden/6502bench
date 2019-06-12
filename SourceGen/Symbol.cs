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

namespace SourceGen {
    /// <summary>
    /// Symbolic representation of a value.  Instances are immutable.
    /// </summary>
    public class Symbol {
        /// <summary>
        /// Was the symbol defined by the user, or generated automatically?
        /// </summary>
        public enum Source {
            // These are in order of highest to lowest precedence.  This matters when
            // looking up a symbol by value, since multiple symbols can have the same value.
            Unknown = 0,
            User,               // user-defined label
            Project,            // from project configuration file
            Platform,           // from platform definition file
            Auto                // auto-generated label
        }

        /// <summary>
        /// Local internal label, global internal label, or reference to an
        /// external address?  Constants get a separate type in case we need to
        /// distinguish them from addresses.
        /// </summary>
        public enum Type {
            Unknown = 0,
            LocalOrGlobalAddr,  // local symbol, may be promoted to global
            GlobalAddr,         // user wants this to be a global symbol
            GlobalAddrExport,   // global symbol that is exported to linkers
            ExternalAddr,       // reference to address outside program (e.g. platform sym file)
            Constant            // constant value
        }

        /// Returns true if the symbol's type is an internal label (auto or user).  Returns
        /// false for external addresses and constants.
        /// </summary>
        public bool IsInternalLabel {
            get {
                // Could also check Type instead.  Either works for now.
                return SymbolSource == Source.User || SymbolSource == Source.Auto;
            }
        }


        /// <summary>
        /// Label sent to assembler.
        /// </summary>
        public string Label { get; private set; }

        /// <summary>
        /// Symbol's numeric value.
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Symbol origin, e.g. auto-generated or entered by user.
        /// </summary>
        public Source SymbolSource { get; private set; }

        /// <summary>
        /// Type of symbol, e.g. local or global.
        /// </summary>
        public Type SymbolType { get; private set; }

        /// <summary>
        /// Two-character string representation of Source and Type, for display in the UI.
        /// </summary>
        public string SourceTypeString { get; private set; }


        // No nullary constructor.
        private Symbol() { }

        /// <summary>
        /// Constructs immutable object.
        /// </summary>
        /// <param name="label">Label string.  Syntax assumed valid.</param>
        /// <param name="source">User-defined or auto-generated?</param>
        /// <param name="type">Type of symbol this is.</param>
        ///   user-defined.</param>
        public Symbol(string label, int value, Source source, Type type) {
            Debug.Assert(!string.IsNullOrEmpty(label));
            Label = label;
            Value = value;
            SymbolType = type;
            SymbolSource = source;

            // Generate SourceTypeString.
            string sts;
            switch (SymbolSource) {
                case Source.Auto:               sts = "A";  break;
                case Source.User:               sts = "U";  break;
                case Source.Platform:           sts = "P";  break;
                case Source.Project:            sts = "R";  break;
                default:                        sts = "?";  break;
            }
            switch (SymbolType) {
                case Type.LocalOrGlobalAddr:    sts += "L";  break;
                case Type.GlobalAddr:           sts += "G";  break;
                case Type.GlobalAddrExport:     sts += "X";  break;
                case Type.ExternalAddr:         sts += "E";  break;
                case Type.Constant:             sts += "C";  break;
                default:                        sts += "?";  break;
            }
            SourceTypeString = sts;
        }

        public override string ToString() {
            return Label + "{" + SymbolSource  + "," + SymbolType +
                ",val=$" + Value.ToString("x4") + "}";
        }

        public static bool operator ==(Symbol a, Symbol b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal.  Ignore SourceTypeString, since it's generated
            // from Source and Type.
            return Asm65.Label.LABEL_COMPARER.Equals(a.Label, b.Label) && a.Value == b.Value &&
                a.SymbolSource == b.SymbolSource && a.SymbolType == b.SymbolType;
        }
        public static bool operator !=(Symbol a, Symbol b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is Symbol && this == (Symbol)obj;
        }
        public override int GetHashCode() {
            // Convert the label to upper case before computing the hash code, so that
            // symbols with "foo" and "FOO" (which are equal) have the same hash code.
            return Asm65.Label.ToNormal(Label).GetHashCode() ^
                Value ^ (int)SymbolType ^ (int)SymbolSource;
        }
    }
}
