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
    /// Table of redefinable variables.  A project may have several of these, at different
    /// offsets.  The contents of later tables overwrite the contents of earlier tables.
    ///
    /// The class is mutable, but may only be modified by the LvTable editor (which makes
    /// changes to a work object that moves through the undo/redo buffer) or the
    /// deserializer.
    ///
    /// (Referring to these as "local" variables is a bit of a misnomer, since they have
    /// global scope from the point where they're defined.  The name reflects their intended
    /// usage, rather than how the assembler will treat them.)
    /// </summary>
    public class LocalVariableTable {
        /// <summary>
        /// List of variables.  The symbol's label must be unique within a table, so we sort
        /// on that.
        /// </summary>
        public SortedList<string, DefSymbol> Variables;

        /// <summary>
        /// If set, all values from previous VariableTables should be discarded when this
        /// table is encountered.
        /// </summary>
        /// <remarks>
        /// This does not correspond to any output in generated assembly code.  We simply stop
        /// trying to associate the symbols with instructions.  The code will either use a
        /// less tightly-scoped value (e.g. project symbol) or output as hex.  There is no need
        /// to tell the assembler to forget the symbol.
        ///
        /// It might be useful to allow addresses (DP ops) and constants (StackRel ops) to be
        /// cleared independently, but I suspect the typical compiled-language scenario will
        /// involve StackRel for args and a sliding DP for locals, so generally it makes
        /// sense to just clear everything.
        /// </remarks>
        public bool ClearPrevious { get; set; }


        /// <summary>
        /// Constructs an empty table.
        /// </summary>
        public LocalVariableTable() {
            Variables = new SortedList<string, DefSymbol>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="src">Object to clone.</param>
        public LocalVariableTable(LocalVariableTable src) : this() {
            ClearPrevious = src.ClearPrevious;

            foreach (KeyValuePair<string, DefSymbol> kvp in src.Variables) {
                Variables[kvp.Key] = kvp.Value;
            }

            Debug.Assert(this == src);
        }

        public static bool operator ==(LocalVariableTable a, LocalVariableTable b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal.
            if (a.ClearPrevious != b.ClearPrevious) {
                return false;
            }
            if (a.Variables.Count != b.Variables.Count) {
                return false;
            }
            // Compare all list entries.
            for (int i = 0; i < a.Variables.Count; i++) {
                if (a.Variables.Values[i] != b.Variables.Values[i]) {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(LocalVariableTable a, LocalVariableTable b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is LocalVariableTable && this == (LocalVariableTable)obj;
        }
        public override int GetHashCode() {
            int hashCode = 0;
            foreach (KeyValuePair<string, DefSymbol> kvp in Variables) {
                hashCode ^= kvp.Value.GetHashCode();
            }
            if (ClearPrevious) {
                hashCode++;
            }
            return hashCode;
        }
    }
}
