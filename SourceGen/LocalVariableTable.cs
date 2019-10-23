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
    /// offsets.
    ///
    /// The class is mutable, but may only be modified by the LvTable editor (which makes
    /// changes to a work object that moves through the undo/redo buffer) or the
    /// deserializer.
    /// </summary>
    /// <remarks>
    /// The contents of later tables overwrite the contents of earlier tables.  A
    /// variable is replaced if the name is re-used (because a symbol can have only one
    /// value at a time) or if the value is re-used (because they're applied automatically
    /// and we need to know which symbol to use).
    ///
    /// The DefSymbols should have symbol type Constant or ExternalAddr.  These do not clash
    /// with each other, e.g. the statements "LDA $10,S" and "STA $10" use two different
    /// variables, because one is an 8-bit stack offset while the other is an 8-bit direct page
    /// address.
    ///
    /// (Referring to these as "local" variables is a bit of a misnomer, since they have
    /// global scope from the point where they're defined.  The name reflects their intended
    /// usage, rather than how the assembler will treat them.)
    /// </remarks>
    public class LocalVariableTable {
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
        /// List of variables, sorted by label.
        /// </summary>
        private SortedList<string, DefSymbol> mVarByLabel;

        /// <summary>
        /// List of variables.  This is manually sorted when needed.  The key is a combination
        /// of the value and the symbol type, so we can't use a simple SortedList.
        /// </summary>
        private List<DefSymbol> mVarByValue;
        private bool mNeedSort = true;


        /// <summary>
        /// Constructs an empty table.
        /// </summary>
        public LocalVariableTable() {
            mVarByLabel = new SortedList<string, DefSymbol>();
            mVarByValue = new List<DefSymbol>();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="src">Object to clone.</param>
        public LocalVariableTable(LocalVariableTable src) : this() {
            ClearPrevious = src.ClearPrevious;

            foreach (KeyValuePair<string, DefSymbol> kvp in src.mVarByLabel) {
                mVarByLabel[kvp.Value.Label] = kvp.Value;
                mVarByValue.Add(kvp.Value);
            }

            Debug.Assert(this == src);
        }

        /// <summary>
        /// Number of entries in the variable table.
        /// </summary>
        public int Count { get { return mVarByLabel.Count; } }

        /// <summary>
        /// Returns the Nth item, sorted by value.  This is NOT a lookup by value.
        /// </summary>
        public DefSymbol this[int index] {
            get {
                SortIfNeeded();
                return mVarByValue[index];
            }
        }

        private void SortIfNeeded() {
            if (mNeedSort) {
                // Currently sorting primarily by value, secondarily by symbol type.  This
                // ordering determines how it appears in the code list.  If we want to make it
                // configurable we just need to replace the sort function.
                mVarByValue.Sort((a, b) => {
                    // Numeric ascending.
                    int diff = a.Value - b.Value;
                    if (diff != 0) {
                        return diff;
                    }
                    // DP addr first, StackRel const second
                    if (a.SymbolType == Symbol.Type.ExternalAddr) {
                        return -1;
                    } else {
                        return 1;
                    }
                    //return a.Label.CompareTo(b.Label);
                });
                mNeedSort = false;
            }
        }

        /// <summary>
        /// Clears the tables.
        /// </summary>
        public void Clear() {
            mVarByLabel.Clear();
            mVarByValue.Clear();
        }

        /// <summary>
        /// Returns the symbol that matches the label, or null if not found.
        /// </summary>
        public DefSymbol GetByLabel(string label) {
            mVarByLabel.TryGetValue(label, out DefSymbol defSym);
            return defSym;
        }

        /// <summary>
        /// Removes the symbol with the matching label.
        /// </summary>
        public void RemoveByLabel(string label) {
            if (mVarByLabel.TryGetValue(label, out DefSymbol defSym)) {
                mVarByLabel.Remove(defSym.Label);
                mVarByValue.Remove(defSym);
            }
            Debug.Assert(mVarByValue.Count == mVarByLabel.Count);

            // Should not be necessary to re-sort the by-value list.
        }

        /// <summary>
        /// Finds symbols that overlap with the specified value and width.  If more than one
        /// matching symbol is found, an arbitrary match will be returned.  Comparisons are
        /// only performed between symbols of the same type, so addresses and constants do
        /// not clash.
        /// </summary>
        /// <param name="value">Value to compare.</param>
        /// <param name="width">Width to check, useful when checking for collisions.  When
        ///   doing a simple variable lookup, this should be set to 1.</param>
        /// <returns>One matching symbol, or null if none matched.</returns>
        public DefSymbol GetByValueRange(int value, int width, Symbol.Type type) {
            foreach (KeyValuePair<string, DefSymbol> kvp in mVarByLabel) {
                if (DefSymbol.CheckOverlap(kvp.Value, value, width, type)) {
                    return kvp.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a symbol to the variable table.  Existing entries with the same name or
        /// overlapping values will be removed.
        /// </summary>
        /// <param name="newSym">Symbol to add.</param>
        public void AddOrReplace(DefSymbol newSym) {
            if (!newSym.IsConstant && newSym.SymbolType != Symbol.Type.ExternalAddr) {
                Debug.Assert(false, "Unexpected symbol type " + newSym.SymbolType);
                return;
            }
            if (!newSym.IsVariable) {
                Debug.Assert(false, "Unexpected symbol source " + newSym.SymbolSource);
                return;
            }

            // Remove existing entries that match on label or value.  The value check must
            // take the width into account.
            if (mVarByLabel.TryGetValue(newSym.Label, out DefSymbol labelSym)) {
                mVarByLabel.Remove(labelSym.Label);
                mVarByValue.Remove(labelSym);
            }

            // Inefficient, but the list should be small.
            DefSymbol valSym;
            while ((valSym = GetByValueRange(newSym.Value,
                        newSym.DataDescriptor.Length, newSym.SymbolType)) != null) {
                mVarByLabel.Remove(valSym.Label);
                mVarByValue.Remove(valSym);
            }

            mVarByLabel.Add(newSym.Label, newSym);
            mVarByValue.Add(newSym);
            Debug.Assert(mVarByValue.Count == mVarByLabel.Count);

            mNeedSort = true;
        }

        /// <summary>
        /// Returns a reference to the sorted-by-label list.  The caller must not modify it.
        /// </summary>
        /// <remarks>
        /// This exists primarily for EditDefSymbol, which wants a list of this type to
        /// perform uniqueness checks.
        /// </remarks>
        public SortedList<string, DefSymbol> GetSortedByLabel() {
            return mVarByLabel;
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
            if (a.mVarByLabel.Count != b.mVarByLabel.Count) {
                return false;
            }
            // Compare all list entries.
            for (int i = 0; i < a.mVarByLabel.Count; i++) {
                if (a.mVarByLabel.Values[i] != b.mVarByLabel.Values[i]) {
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
            foreach (KeyValuePair<string, DefSymbol> kvp in mVarByLabel) {
                hashCode ^= kvp.Value.GetHashCode();
            }
            if (ClearPrevious) {
                hashCode++;
            }
            return hashCode;
        }

        public void DebugDump(int offset) {
            Debug.WriteLine("LocalVariableTable +" + offset.ToString("x6") + " count=" +
                Count + " clear-previous=" + ClearPrevious);
            for (int i = 0; i < Count; i++) {
                Debug.WriteLine("  " + i + ": " + this[i]);
            }
        }
    }
}
