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

using Asm65;

/*
A few words about by-value lookups of external addresses.

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
project files.  Every address is represented, so a symbol with a width of 10 would have 10
entries in the dictionary.  If we're adding a symbol at an address that already has an entry,
we do a priority check, and either leave it alone or replace it with the new value.

For 8-bit code it would be slightly more efficient to use a 64K array representing all of
memory, but that doesn't scale for 65816.  That said, we probably want to break it up by
bank anyway to allow for partial updates.

It's worth noting that the only public by-address interface is the find-by-address method.
Everything is done by symbol or label, and the by-address stuff happens behind the scenes.

-----

A few words about I/O direction.

Some memory-mapped I/O locations have different behavior when read than they do when written.
For example, on later models of the Apple II, reading from $C000 returns the last key hit,
while writing to $C000 disables 80-column mode.  When doing an external address lookup, we
may need to know what sort of access is happening.

We need to be able to define independent symbols for reading and writing, which means either
having two separate address lookup tables, or one table with two references per entry.  If
we use an array of structs as our table, having two refs per entry is efficient.  If we use
a collection class like Dictionary, two refs per entry requires allocating an additional
object to hold the object pair, so it's more memory efficient to have two separate dictionaries.
However, separate data structures means double the dictionary lookups (which are O(1) in an
unsorted Dictionary).

Not all instructions perform an access.  For example, JSR doesn't immediately read from the
location, it just sets the program counter.  On the 65816, instructions like PEA take an
address, but don't cause any access at all.  We can't generally know what's coming next, so
for access type "none" we try Read then Write.

-----

A few words about address masks.

On the Atari 2600, you can access registers, RAM, and ROM from multiple addresses.  For
example, the first TIA register can be accessed at $0000, $0040, $0100, $0140, and so on,
but only in "even" 4K pages ($0000, $2000, $4000, ...).  Because the underlying hardware is
just watching for specific values on certain address lines, the set of matching addresses can
be described with a pair of bit masks.  We need one more mask to define which address bits
are used to select a specific register.

The question is how to handle a by-address lookup here.  There are two basic approaches:

 1. Add all possibly matching addresses to the dictionary.
 2. Maintain a separate list of masked symbols, and match against those in a separate step.

Option #1 makes adding a symbol expensive, but lookups very cheap.  We have to add
potentially thousands of entries to the dictionary for each masked symbol.  When we want
to look up a symbol, though, we just check the entry for the address.

Option #2 makes adding a symbol cheap, but lookups are inconsistent with the established rules.
The problem arises if a masked symbol overlaps with a non-masked symbol.  If we want the priority
to work the way we described earlier, some non-masked symbols might have priority over a given
masked symbol while others don't.

v---------
It's possible to mitigate the problems of both with a hybrid approach:
 - Non-masked symbols get added to the dictionary as usual.
 - Masked symbols are compared to all existing dictionary entries.  If the mask matches, the
   existing entry is kept or replaced according to the usual rules.
 - On lookup-by-value, we check for a match in the dictionary.  If we don't find one, we
   test it against all masked values.

We still have to iterate for each masked symbol, but only over the set of non-masked symbols
rather than an entire bank (or more... need to define what masking means for 16-bit code),
and we don't have to create any new entries.

We can improve the lookup speed by keeping the symbols grouped by CompareMask/CompareValue.  If
the test fails we can ignore all symbols in the group.  If more than one symbol has the same
value (after being masked with AddressMask), we replace symbols in priority order.

The behavior when two masked groups overlap is somewhat unspecified, especially if we combine
symbols with identical mask sets.  We could set the mask set to ABC, define symbols, switch
to DEF, define symbols, then switch back to ABC and define more symbols.  If ABC is a subset
of DEF, it's possible that symbols are defined in the third set that should replace symbols
in the second set.  But because ABC was defined first, an ordered list would check DEF for a
match before checking ABC.  (I'm pretty comfortable with declaring that the behavior of
overlapping mask sets is undefined... if it's ambiguous to the hardware, I'm just not going
to worry about it.)
---------^

I think approach #2 is entirely reasonable.  Make masked lookups the lowest priority, so
that specific overrides can be defined in the usual way.  The masked symbols catch anything
that falls through the cracks.  We can revisit this if there turns out to be an interesting
use case that justifies the additional work.

All of the above must be done twice for ReadWrite symbols, once per direction.

-----

A few words about updating the by-address table.

The previous notes were largely concerned with populating the table.  We also need to worry
about updating the table when new entries are added, edited, or removed.  We want, whenever,
possible, to avoid updating the entire table.

The troubles arise when we remove an entry, or we add/edit an entry with a label that conflicts
with another entry, effectively adding or removing the conflicting symbol.  Because of the
layered approach we use, it's sometimes necessary to regenerate the by-address table from the
contents of the by-label table.  For example, if we have a platform symbol named "FOO" with a
width of 10, and we create a user label named "FOO", the platform symbol disappears completely.
Overlapping symbols that had been hidden due to lower priority must be restored.

We could reduce the update cost by making each table entry a priority-ordered list of symbols.
I'm expecting conflicts to be rare in practice, so no need to worry about this yet.
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
        private Dictionary<int, Symbol> mSymbolsByReadAddress = new Dictionary<int, Symbol>();
        private Dictionary<int, Symbol> mSymbolsByWriteAddress = new Dictionary<int, Symbol>();

        /// <summary>
        /// Container for a collection of symbols that share a common mask definition.
        /// </summary>
        private class MaskGroup {
            private DefSymbol.MultiAddressMask mMultiMask;

            // Keyed by minimal address, not canonical address.
            private Dictionary<int, DefSymbol> mByReadAddress = new Dictionary<int, DefSymbol>();
            private Dictionary<int, DefSymbol> mByWriteAddress = new Dictionary<int, DefSymbol>();

            public MaskGroup(DefSymbol.MultiAddressMask multiMask) {
                Debug.Assert(multiMask != null);
                mMultiMask = multiMask;
            }

            public void Add(DefSymbol defSym) {
                bool doRead = true;
                bool doWrite = true;
                if (defSym.Direction == DefSymbol.DirectionFlags.Read) {
                    doWrite = false;
                } else if (defSym.Direction == DefSymbol.DirectionFlags.Write) {
                    doRead = false;
                }

                for (int i = 0; i < defSym.DataDescriptor.Length; i++) {
                    // See if there's already something here.  If we reach the end of the
                    // bank, wrap around.
                    int addr = (defSym.Value & 0xff0000) + ((defSym.Value + i) & 0xffff);
                    addr &= mMultiMask.AddressMask;     // use minimal address
                    DefSymbol curSym;
                    if (doRead) {
                        mByReadAddress.TryGetValue(addr, out curSym);
                        mByReadAddress[addr] = (curSym == null) ? defSym :
                            (DefSymbol)HighestPriority(defSym, curSym);
                    }
                    if (doWrite) {
                        mByWriteAddress.TryGetValue(addr, out curSym);
                        mByWriteAddress[addr] = (curSym == null) ? defSym :
                            (DefSymbol)HighestPriority(defSym, curSym);
                    }
                }
            }

            public DefSymbol Find(int addr, bool tryRead, bool tryWrite) {
                addr &= mMultiMask.AddressMask;
                DefSymbol defSym;
                if (tryRead && mByReadAddress.TryGetValue(addr, out defSym)) {
                    return defSym;
                }
                if (tryWrite && mByWriteAddress.TryGetValue(addr, out defSym)) {
                    return defSym;
                }
                return null;
            }
        }

        /// <summary>
        /// Collection of MaskGroups.
        /// </summary>
        private Dictionary<DefSymbol.MultiAddressMask, MaskGroup> mMaskGroups =
            new Dictionary<DefSymbol.MultiAddressMask, MaskGroup>();


        /// <summary>
        /// Constructor.
        /// </summary>
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
            mSymbolsByReadAddress.Clear();
            mSymbolsByWriteAddress.Clear();
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
            }
        }

        /// <summary>
        /// Gets the value associated with the label.
        /// </summary>
        /// <param name="key">Label to look up.</param>
        /// <param name="sym">Symbol, or null if not found.</param>
        /// <returns>True if the key is present, false otherwise.</returns>
        public bool TryGetValue(string key, out Symbol sym) {
            return mSymbols.TryGetValue(key, out sym);
        }

        /// <summary>
        /// Gets the value associated with the label, unless it's a variable.
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
            if (sym is DefSymbol && ((DefSymbol)sym).MultiMask != null) {
                AddMultiMaskEntry((DefSymbol)sym);
                return;
            }

            bool doRead = true;
            bool doWrite = true;
            int width = 1;
            if (sym is DefSymbol) {
                DefSymbol defSym = (DefSymbol)sym;
                width = defSym.DataDescriptor.Length;
                if (defSym.Direction == DefSymbol.DirectionFlags.Read) {
                    doWrite = false;
                } else if (defSym.Direction == DefSymbol.DirectionFlags.Write) {
                    doRead = false;
                }
            }

            for (int i = 0; i < width; i++) {
                // See if there's already something here.  If we reach the end of the
                // bank, wrap around.
                int addr = (sym.Value & 0xff0000) + ((sym.Value + i) & 0xffff);
                Symbol curSym;
                if (doRead) {
                    mSymbolsByReadAddress.TryGetValue(addr, out curSym);
                    mSymbolsByReadAddress[addr] = (curSym == null) ? sym :
                        HighestPriority(sym, curSym);
                }
                if (doWrite) {
                    mSymbolsByWriteAddress.TryGetValue(addr, out curSym);
                    mSymbolsByWriteAddress[addr] = (curSym == null) ? sym :
                        HighestPriority(sym, curSym);
                }
            }
        }

        private void AddMultiMaskEntry(DefSymbol defSym) {
            DefSymbol.MultiAddressMask multiMask = defSym.MultiMask;
            mMaskGroups.TryGetValue(multiMask, out MaskGroup group);
            if (group == null) {
                group = new MaskGroup(multiMask);
                mMaskGroups.Add(multiMask, group);
            }
            group.Add(defSym);
        }

        private static Symbol HighestPriority(Symbol sym1, Symbol sym2) {
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
                mSymbolsByReadAddress.Remove(sym.Value);
                mSymbolsByWriteAddress.Remove(sym.Value);
                return;
            }

            // Removing a project/platform symbol requires re-evaluating the by-address table.
            Debug.WriteLine("SymbolTable: regenerating table after removal of " + sym);
            RegenerateAddressTable();
        }

        /// <summary>
        /// Regenerates the entire by-address table, from the contents of the by-label list.
        /// </summary>
        /// <remarks>
        /// This is a little painful, but if a symbol gets removed we don't have a way to
        /// restore lower-priority items.  If this becomes a performance issue we can create
        /// an ordered list of symbols at each address, but even with a few hundred symbols this
        /// should take very little time.
        /// </remarks>
        private void RegenerateAddressTable() {
            //Debug.WriteLine("SymbolTable: regenerating address table");
            mSymbolsByReadAddress.Clear();
            mSymbolsByWriteAddress.Clear();

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
        public Symbol FindNonVariableByAddress(int addr, OpDef.MemoryEffect effect) {
            bool tryRead, tryWrite;
            if (effect == OpDef.MemoryEffect.Read) {
                tryRead = true;
                tryWrite = false;
            } else if (effect == OpDef.MemoryEffect.Write) {
                tryRead = false;
                tryWrite = true;
            } else if (effect == OpDef.MemoryEffect.ReadModifyWrite ||
                    effect == OpDef.MemoryEffect.None) {
                tryRead = tryWrite = true;
            } else {
                Debug.Assert(false);
                return null;
            }

            Symbol sym = null;
            if (tryRead) {
                mSymbolsByReadAddress.TryGetValue(addr, out sym);
            }
            if (tryWrite && sym == null) {
                mSymbolsByWriteAddress.TryGetValue(addr, out sym);
            }

            if (sym == null) {
                // Nothing matched, check the match groups.
                foreach (KeyValuePair<DefSymbol.MultiAddressMask, MaskGroup> kvp in mMaskGroups) {
                    DefSymbol.MultiAddressMask multiMask = kvp.Key;
                    if ((addr & multiMask.CompareMask) == multiMask.CompareValue) {
                        MaskGroup group = kvp.Value;
                        DefSymbol defSym = kvp.Value.Find(addr, tryRead, tryWrite);
                        if (defSym != null) {
                            sym = defSym;
                            break;
                        }
                    }
                }
            }
            return sym;
        }

        public override string ToString() {
            return "SymbolTable: " + mSymbols.Count + " by label, " +
                mSymbolsByReadAddress.Count + " by addr(r), " +
                mSymbolsByWriteAddress.Count + " by addr(w), " +
                mMaskGroups.Count + " mask groups";
        }
    }
}
