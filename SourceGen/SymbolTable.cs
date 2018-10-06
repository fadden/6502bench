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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SourceGen {
    /// <summary>
    /// List of all symbols, arranged primarily by label, but also accessible by value.  All
    /// symbols have a unique label.
    /// </summary>
    public class SymbolTable : IEnumerable<Symbol> {
        /// <summary>
        /// Primary storage.  Provides fast lookup by label.  The StringComparer we choose
        /// determines how case sensitivity and Culture is handled.
        private SortedList<string, Symbol> mSymbols =
            new SortedList<string, Symbol>(Asm65.Label.LABEL_COMPARER);

        /// <summary>
        /// Same content, but ordered by value.  Note the key and the value are the same object.
        /// </summary>
        private SortedList<Symbol, Symbol> mSymbolsByValue =
            new SortedList<Symbol, Symbol>(new CompareByValue());

        /// <summary>
        /// Compare two symbols, primarily by value, secondarily by source, and tertiarily
        /// by label.  The primary SortedList guarantees that the label is unique, so we
        /// should never have two equal Symbols in the list.
        /// 
        /// The type comparison ensures that project symbols appear before platform symbols,
        /// so that you can "overwrite" a platform symbol with the same value.
        /// </summary>
        private class CompareByValue : IComparer<Symbol> {
            public int Compare(Symbol a, Symbol b) {
                if (a.Value < b.Value) {
                    return -1;
                } else if (a.Value > b.Value) {
                    return 1;
                }

                if ((int)a.SymbolSource < (int)b.SymbolSource) {
                    return -1;
                } else if ((int)a.SymbolSource > (int)b.SymbolSource) {
                    return 1;
                }

                // Equal values, check string.  We'll get a match on Remove or when
                // replacing an entry with itself, but no two Symbols in the list
                // should have the same label.
                return Asm65.Label.LABEL_COMPARER.Compare(a.Label, b.Label);
            }
        }

        /// <summary>
        /// This is incremented whenever the contents of the symbol table change.  External
        /// code can compare this against a previous value to see if anything has changed
        /// since the last visit.
        /// 
        /// We could theoretically miss something at the 2^32 rollover.  Not worried.
        /// </summary>
        public int ChangeSerial { get; private set; }


        public SymbolTable() { }

        // IEnumerable
        public IEnumerator<Symbol> GetEnumerator() {
            // .Values is documented as O(1)
            return mSymbols.Values.GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return mSymbols.Values.GetEnumerator();
        }

        /// <summary>
        /// Clears the symbol table.
        /// </summary>
        public void Clear() {
            mSymbols.Clear();
            mSymbolsByValue.Clear();
            ChangeSerial++;
        }

        /// <summary>
        /// Returns the number of symbols in the table.
        /// </summary>
        public int Count() {
            Debug.Assert(mSymbolsByValue.Count == mSymbols.Count);
            return mSymbols.Count;
        }

        /// <summary>
        /// Adds the specified symbol to the list.  Throws an exception if the symbol is
        /// already present.
        /// </summary>
        public void Add(Symbol sym) {
            // If Symbol with matching label is in list, this will throw an exception,
            // and the by-value add won't happen.
            mSymbols.Add(sym.Label, sym);
            mSymbolsByValue.Add(sym, sym);
            ChangeSerial++;
        }

        /// <summary>
        /// Finds the specified symbol by label.  Throws an exception if it's not found.
        /// 
        /// Adds the specified symbol to the list, or replaces it if it's already present.
        /// </summary>
        public Symbol this[string key] {
            get {
                Debug.Assert(mSymbolsByValue.Count == mSymbols.Count);
                return mSymbols[key];
            }
            set {
                // Replacing {"foo", 1} with ("foo", 2} works correctly for mSymbols, because
                // the label is the unique key.  For mSymbolsByValue we have to explicitly
                // remove it, because the entire Symbol is used as the key.
                mSymbols.TryGetValue(key, out Symbol oldValue);
                if (oldValue != null) {
                    mSymbolsByValue.Remove(oldValue);
                }
                mSymbols[key] = value;
                mSymbolsByValue[value] = value;
                ChangeSerial++;
            }
        }

        /// <summary>
        /// Searches the table for symbols with matching address values.  Ignores constants.
        /// </summary>
        /// <param name="value">Value to find.</param>
        /// <returns>First matching symbol found, or null if nothing matched.</returns>
        public Symbol FindAddressByValue(int value) {
            // Get sorted list of values.  This is documented as efficient.
            IList<Symbol> values = mSymbolsByValue.Values;

            //for (int i = 0; i < values.Count; i++) {
            //    if (values[i].Value == value && values[i].SymbolType != Symbol.Type.Constant) {
            //        return values[i];
            //    }
            //}

            int low = 0;
            int high = values.Count - 1;
            while (low <= high) {
                int mid = (low + high) / 2;
                Symbol midValue = values[mid];

                if (midValue.Value == value) {
                    // found a match, walk back to find first match
                    while (mid > 0 && values[mid - 1].Value == value) {
                        mid--;
                    }
                    // now skip past constants
                    while (mid < values.Count && values[mid].SymbolType == Symbol.Type.Constant) {
                        //Debug.WriteLine("disregarding " + values[mid]);
                        mid++;
                    }
                    if (mid < values.Count && values[mid].Value == value) {
                        return values[mid];
                    }
                    //Debug.WriteLine("Found value " + value + " but only constants");
                    return null;
                } else if (midValue.Value < value) {
                    // move the low end in
                    low = mid + 1;
                } else {
                    // move the high end in
                    Debug.Assert(midValue.Value > value);
                    high = mid - 1;
                }
            }

            // not found
            return null;
        }

        /// <summary>
        /// Gets the value associated with the key.
        /// </summary>
        /// <param name="key">Label to look up.</param>
        /// <param name="sym">Symbol, or null if not found.</param>
        /// <returns>True if the key is present, false otherwise.</returns>
        public bool TryGetValue(string key, out Symbol sym) {
            return mSymbols.TryGetValue(key, out sym);
        }

        /// <summary>
        /// Removes the specified symbol.
        /// </summary>
        public void Remove(Symbol sym) {
            mSymbols.Remove(sym.Label);
            mSymbolsByValue.Remove(sym);
            ChangeSerial++;
        }

        /// <summary>
        /// Generates a unique address symbol.  Does not add the symbol to the list.
        /// </summary>
        /// <param name="addr">Address label will be applied to</param>
        /// <param name="symbols">Symbol table.</param>
        /// <param name="prefix">Prefix to use; must start with a letter.</param>
        /// <returns>Newly-created, unique symbol.</returns>
        public static Symbol GenerateUniqueForAddress(int addr, SymbolTable symbols,
                string prefix) {
            // $1234 == L1234, $05/1234 == L51234.
            string label = prefix + addr.ToString("X4");    // always upper-case
            if (symbols.TryGetValue(label, out Symbol unused)) {
                const int MAX_RENAME = 999;
                string baseLabel = label;
                StringBuilder sb = new StringBuilder(baseLabel.Length + 8);
                int index = -1;

                do {
                    // This is expected to be unlikely and infrequent, so a simple linear
                    // probe for uniqueness is fine.
                    index++;
                    sb.Clear();
                    sb.Append(baseLabel);
                    sb.Append('_');
                    sb.Append(index);
                    label = sb.ToString();
                } while (index <= MAX_RENAME && symbols.TryGetValue(label, out unused));
                if (index == MAX_RENAME) {
                    // I give up
                    throw new Exception("Too many identical symbols");
                }
            }
            Symbol sym = new Symbol(label, addr, Symbol.Source.Auto,
                Symbol.Type.LocalOrGlobalAddr);
            return sym;
        }
    }
}
