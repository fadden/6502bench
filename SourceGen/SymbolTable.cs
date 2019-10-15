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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

/*
A few words about by-value lookups.

We guarantee that symbol labels are unique, but multiple symbols can have the same value.
This becomes interesting when we're trying to match external address references to symbols
defined in a platform file or the project properties.  It becomes especially interesting
when the symbols have widths larger than 1, and partially overlap each other.

For deterministic behavior it's necessary to define a priority order in event of overlap.
The basic rules for determining the symbol associated with a given address are:

 1. Newer platform symbol definitions replace older definitions, at file granularity.
 2. As an extension of rule #1, project symbols override platform symbols.
 3. User symbols override both platform and project symbols.  They can't overlap by value,
    since one is internal and the others are external, but if a user symbol's label matches
    a project/platform symbol, the project/platform symbol will be hidden.
 4. If two multi-byte symbol definitions overlap, we use whichever was defined closest
    to the actual address while still appearing before it.  So if we have FOO=$2000/10 and
    BAR=$2005/10, $2004 would be FOO and $2005 would be BAR.  (Note this can only happen for
    two symbols inside the same platform file or in the project symbol definitions; otherwise
    one of the previous rules would have determined it.)
 5. If everything else is equal, e.g. we have FOO=$2000 and BAR=$2000 in the same file,
    the winner is determined alphabetically.  (We don't track symbol definition line numbers,
    and there's no definite order in project properties, so there's not much else to do.)

Working through the math on every access could get painful, so we create a dictionary with
the value as the key, and add symbols to it as we work our way through that platform and
project files.  Every address is represented, so a label with a width of 10 would have 10
entries in the dictionary.  If we're adding a symbol at an address that already has an entry,
we do a priority check, and either leave it alone or replace it with the new value.

For 8-bit code it would be slightly more efficient to use a 64K array representing all of
memory, but that doesn't scale for 65816.  That said, we probably want to break it up by
bank anyway to allow for partial updates.

----------

A few words about address masks.

On the Atari 2600, you can access registers, RAM, and ROM from multiple addresses.  For
example, the first TIA register can be accessed at $0000, $0040, $0100, $0140, and so on,
but only in "even" 4K pages ($0000, $2000, $4000, ...).  Because the underlying hardware is
just watching for specific values on certain address lines, the set of matching addresses can
be described with a pair of bit masks, plus one more mask to define which lines are relevant.

The question is how to handle a by-address lookup here.  There are two basic approaches:

 1. Add all possible entries to the dictionary.
 2. Maintain a separate list of masked symbols, and match against those.

Option #1 makes adding a symbol expensive, but lookups very cheap.  We have to add
potentially thousands of entries to the dictionary for each masked symbol.  When we want
to look up a symbol, though, we don't have to do anything different.

Option #2 makes adding a symbol cheap, but lookups are problematic.  The problem arises if
a masked symbol overlaps with a non-masked symbol.  If we want the priority to work the way
we described earlier, some non-masked symbols might have priority over a given masked symbol
while others don't.

(I kinda feel like I'm solving problems I don't have, but consistent behavior is a Good Thing.)

It's possible to mitigate the problems of both with a hybrid approach:
 - Non-masked symbols get added to the dictionary as usual.
 - Masked symbols are compared to all dictionary entries.  If the mask matches, the
   existing entry is kept or replaced according to the usual rules.
*/

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
        /// By-address lookup table.  Because symbols can span more than one byte, there may
        /// be more than one entry per symbol here.  If two symbols cover the same address,
        /// only the highest-priority symbol is kept, so not all symbols are represented here.
        /// 
        /// This does not contain constants or local variables.
        /// </summary>
        /// <remarks>
        /// For efficiency on larger data files, we may want to break this up by bank.  That
        /// way we can do a partial update.
        /// </remarks>
        private Dictionary<int, Symbol> mSymbolsByAddress = new Dictionary<int, Symbol>();


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
            mSymbolsByAddress.Clear();
            //ChangeSerial++;
        }

        /// <summary>
        /// Returns the number of symbols in the table.
        /// </summary>
        public int Count() {
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
            AddAddressTableEntry(sym);
            //ChangeSerial++;
        }

        /// <summary>
        /// get: Finds the specified symbol by label.  Throws an exception if it's not found.
        /// 
        /// set: Adds the specified symbol to the list, or replaces it if it's already present.
        /// </summary>
        public Symbol this[string key] {
            get {
                return mSymbols[key];
            }
            set {
                mSymbols.TryGetValue(key, out Symbol oldValue);
                mSymbols[key] = value;
                if (oldValue != null) {
                    ReplaceAddressTableEntry(oldValue, value);
                } else {
                    AddAddressTableEntry(value);
                }
                //ChangeSerial++;
            }
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
        /// Gets the value associated with the key, unless it's a variable.
        /// </summary>
        /// <param name="key">Label to look up.</param>
        /// <param name="sym">Symbol, or null if not found, or found but it's a variable.</param>
        /// <returns>True if the key is present, false otherwise.</returns>
        public bool TryGetNonVariableValue(string key, out Symbol sym) {
            bool found = mSymbols.TryGetValue(key, out sym);
            if (found && sym.IsVariable) {
                sym = null;
                found = false;
            }
            return found;
        }

        /// <summary>
        /// Removes the specified symbol.
        /// </summary>
        public void Remove(Symbol sym) {
            mSymbols.Remove(sym.Label);
            RemoveAddressTableEntry(sym);
            //ChangeSerial++;
        }

        /// <summary>
        /// Adds a symbol to the address table.  All affected addresses are updated.  If an
        /// existing symbol is already present at an address, the new or old symbol will be
        /// selected in priority order.
        /// </summary>
        /// <param name="sym">Symbol to add.</param>
        private void AddAddressTableEntry(Symbol sym) {
            if (sym.SymbolType == Symbol.Type.Constant) {
                return;
            }
            if (sym.SymbolSource == Symbol.Source.Variable) {
                return;
            }

            int width = 1;
            if (sym is DefSymbol) {
                width = ((DefSymbol)sym).DataDescriptor.Length;
            }
            // we could restore some older behavior by giving user labels a width of 3, but
            // we'd have to make sure that they didn't win for addresses outside the file

            for (int i = 0; i < width; i++) {
                // See if there's already something here.  If we reach the end of the
                // bank, wrap around.
                int addr = (sym.Value & 0xff0000) + ((sym.Value + i) & 0xffff);
                mSymbolsByAddress.TryGetValue(addr, out Symbol curSym);
                mSymbolsByAddress[addr] = (curSym == null) ? sym :
                    HighestPriority(sym, curSym);
            }
        }

        private Symbol HighestPriority(Symbol sym1, Symbol sym2) {
            // First determinant is symbol source.  User labels have highest priority, then
            // project symbols, then platform symbols, then auto labels.
            if ((int)sym1.SymbolSource < (int)sym2.SymbolSource) {
                return sym1;
            } else if ((int)sym1.SymbolSource > (int)sym2.SymbolSource) {
                return sym2;
            }

            // Same source.  Are they platform symbols?
            if (sym1.SymbolSource == Symbol.Source.Platform) {
                // Sort by file load order.  Symbols from files loaded later, which will have
                // a higher ordinal, have priority.
                int lo1 = ((DefSymbol)sym1).LoadOrdinal;
                int lo2 = ((DefSymbol)sym2).LoadOrdinal;
                if (lo1 > lo2) {
                    return sym1;
                } else if (lo1 < lo2) {
                    return sym2;
                }
            }

            // Same source, so this is e.g. two project symbol definitions that overlap.  We
            // handle this by selecting whichever one was defined closer to the target address,
            // i.e. whichever one has the higher value.
            // TODO(someday): this mishandles bank wrap... do we care?
            if (sym1.Value > sym2.Value) {
                return sym1;
            } else if (sym1.Value < sym2.Value) {
                return sym2;
            }

            // In the absence of anything better, we select them alphabetically.  (If they have
            // the same name, value, and source, there's not much to distinguish them anyway.)
            if (Asm65.Label.LABEL_COMPARER.Compare(sym1.Label, sym2.Label) < 0) {
                return sym1;
            } else {
                return sym2;
            }
        }

        /// <summary>
        /// Replaces an entry in the address table.  Must be called AFTER the by-label list
        /// has been updated.
        /// </summary>
        /// <param name="oldSym">Symbol being replaced.</param>
        /// <param name="newSym">New symbol.</param>
        private void ReplaceAddressTableEntry(Symbol oldSym, Symbol newSym) {
            RemoveAddressTableEntry(oldSym);
            AddAddressTableEntry(newSym);
        }

        /// <summary>
        /// Removes an entry from the address table.  Must be called AFTER the by-label list
        /// has been updated.
        /// </summary>
        /// <param name="sym">Symbol to remove.</param>
        private void RemoveAddressTableEntry(Symbol sym) {
            // Easiest thing to do is just regenerate the table.  Since we don't track
            // constants or variables, we can just ignore those.
            if (sym.SymbolType == Symbol.Type.Constant) {
                return;
            }
            if (sym.SymbolSource == Symbol.Source.Variable) {
                return;
            }
            if (sym.SymbolSource == Symbol.Source.User || sym.SymbolSource == Symbol.Source.Auto) {
                // These have a width of 1 and can't overlap with anything meaningful... even
                // if there's a project symbol for the address, it won't be used, because it's
                // an in-file address.  So we can just remove the entry.
                //
                // Note we do this *a lot* when the fancier auto labels are enabled, because we
                // generate plain labels and then replace them with annotated labels.
                mSymbolsByAddress.Remove(sym.Value);
                return;
            }

            // Removing a project/platform symbol requires re-evaluating the by-address table.
            RegenerateAddressTable();
        }

        /// <summary>
        /// Regenerates the entire by-address table, from the contents of the by-label list.
        /// </summary>
        /// <remarks>
        /// This is a little painful, but if a symbol gets removed we don't have a way to
        /// restore lower-priority items.  If this becomes a performance issue we can create
        /// an ordered list of symbols at each address, but with a few hundred symbols this
        /// should take very little time.
        /// </remarks>
        private void RegenerateAddressTable() {
            Debug.WriteLine("SymbolTable: regenerating address table");
            mSymbolsByAddress.Clear();

            foreach (KeyValuePair<string, Symbol> kvp in mSymbols) {
                AddAddressTableEntry(kvp.Value);
            }
        }

        /// <summary>
        /// Searches the table for symbols with matching address values.  Ignores constants and
        /// variables.
        /// </summary>
        /// <param name="addr">Address to find.</param>
        /// <returns>First matching symbol found, or null if nothing matched.</returns>
        public Symbol FindNonVariableByAddress(int addr) {
            mSymbolsByAddress.TryGetValue(addr, out Symbol sym);
            return sym;
        }
    }
}
