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

namespace SourceGenWPF {
    /// <summary>
    /// Map file offsets to 65xx addresses and vice-versa.  Useful for sources with
    /// multiple ORG directives.
    /// 
    /// It's possible to generate code that would overlap once relocated at run time,
    /// which means a given address could map to multiple offsets.  For this reason
    /// it's useful to know the offset of the referring code when evaluating a
    /// reference, so that a "local" match can take priority.
    /// </summary>
    public class AddressMap : IEnumerable<AddressMap.AddressMapEntry> {
        /// <summary>
        /// Code starting at the specified offset will have the specified address.
        /// 
        /// The entries are held in the list in order, sorted by offset, with no gaps.
        /// This makes the "length" field redundant, as it can be computed by
        /// (entry[N+1].mOffset - entry[N].mOffset), with a special case for the last
        /// entry in the list.  It's convenient to maintain it explicitly however, as
        /// the list is read far more often than it is updated.
        /// 
        /// Entries are mutable, but must only be altered by AddressMap.  Don't retain
        /// instances of this across other activity.
        /// </summary>
        public class AddressMapEntry {
            public int Offset { get; set; }
            public int Addr { get; set; }
            public int Length { get; set; }

            public AddressMapEntry(int offset, int addr, int len) {
                Offset = offset;
                Addr = addr;
                Length = len;
            }
        }

        /// <summary>
        /// Total length, in bytes, spanned by this map.
        /// </summary>
        private int mTotalLength;

        /// <summary>
        /// List of definitions, in sorted order.
        /// </summary>
        private List<AddressMapEntry> mAddrList = new List<AddressMapEntry>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="length">Total length, in bytes, spanned by this map.</param>
        public AddressMap(int length) {
            /// There must always be at least one entry, defining the target address
            /// for file offset 0.  This can be changed, but can't be removed.
            mTotalLength = length;
            mAddrList.Add(new AddressMapEntry(0, 0, length));
        }

        // IEnumerable
        public IEnumerator<AddressMapEntry> GetEnumerator() {
            return ((IEnumerable<AddressMapEntry>)mAddrList).GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return ((IEnumerable<AddressMapEntry>)mAddrList).GetEnumerator();
        }

        /// <summary>
        /// Returns the Nth entry in the address map.
        /// </summary>
        public AddressMapEntry this[int i] {
            get { return mAddrList[i]; }
        }

        /// <summary>
        /// Number of entries in the address map.
        /// </summary>
        public int Count { get { return mAddrList.Count; } }

        /// <summary>
        /// Returns the Address value of the address map entry associated with the specified
        /// offset, or -1 if there is no address map entry there.  The offset must match exactly.
        /// </summary>
        public int Get(int offset) {
            foreach (AddressMapEntry ad in mAddrList) {
                if (ad.Offset == offset) {
                    return ad.Addr;
                }
            }
            return -1;
        }

        /// <summary>
        /// Returns the index of the address map entry that contains the given offset.
        /// We assume the offset is valid.
        /// </summary>
        private int IndexForOffset(int offset) {
            for (int i = 1; i < mAddrList.Count; i++) {
                if (mAddrList[i].Offset > offset) {
                    return i - 1;
                }
            }

            return mAddrList.Count - 1;
        }

        /// <summary>
        /// Adds, updates, or removes a map entry.
        /// </summary>
        /// <param name="offset">File offset at which the address changes.</param>
        /// <param name="addr">24-bit address.</param>
        public void Set(int offset, int addr) {
            Debug.Assert(offset >= 0);
            if (addr == -1) {
                if (offset != 0) {      // ignore attempts to remove entry at offset zero
                    Remove(offset);
                }
                return;
            }
            Debug.Assert(addr >= 0 && addr < 0x01000000);   // 24-bit address space

            int i;
            for (i = 0; i < mAddrList.Count; i++) {
                AddressMapEntry ad = mAddrList[i];
                if (ad.Offset == offset) {
                    // update existing
                    ad.Addr = addr;
                    mAddrList[i] = ad;
                    return;
                } else if (ad.Offset > offset) {
                    // The i'th entry is one past the interesting part.
                    break;
                }
            }

            // Carve a chunk out of the previous entry.
            AddressMapEntry prev = mAddrList[i - 1];
            int prevOldLen = prev.Length;
            int prevNewLen = offset - prev.Offset;
            prev.Length = prevNewLen;
            mAddrList[i - 1] = prev;

            mAddrList.Insert(i,
                new AddressMapEntry(offset, addr, prevOldLen - prevNewLen));

            DebugValidate();
        }

        /// <summary>
        /// Removes an entry from the set.
        /// </summary>
        /// <param name="offset">The initial offset of the mapping to remove. This
        ///   must be the initial value, not a mid-range value.</param>
        /// <returns>True if something was removed.</returns>
        public bool Remove(int offset) {
            if (offset == 0) {
                throw new Exception("Not allowed to remove entry 0");
            }

            for (int i = 1; i < mAddrList.Count; i++) {
                if (mAddrList[i].Offset == offset) {
                    // Add the length to the previous entry.
                    AddressMapEntry prev = mAddrList[i - 1];
                    prev.Length += mAddrList[i].Length;
                    mAddrList[i - 1] = prev;

                    mAddrList.RemoveAt(i);
                    DebugValidate();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given address falls into the range spanned by the
        /// address map entry.
        /// </summary>
        /// <param name="index">Address map entry index.</param>
        /// <param name="addr">Address to check.</param>
        /// <returns></returns>
        private bool IndexContainsAddress(int index, int addr) {
            return addr >= mAddrList[index].Addr &&
                    addr < mAddrList[index].Addr + mAddrList[index].Length;
        }

        /// <summary>
        /// Determines the file offset that best contains the specified target address.
        /// </summary>
        /// <param name="srcOffset">Offset of the address reference.</param>
        /// <param name="targetAddr">Address to look up.</param>
        /// <returns>The file offset, or -1 if the address falls outside the file.</returns>
        public int AddressToOffset(int srcOffset, int targetAddr) {
            if (mAddrList.Count == 1) {
                // Trivial case.
                if (IndexContainsAddress(0, targetAddr)) {
                    Debug.Assert(targetAddr >= mAddrList[0].Addr);
                    return targetAddr - mAddrList[0].Addr;
                } else {
                    return -1;
                }
            }

            // We have multiple, potentially overlapping address ranges.  Start by
            // looking for a match in the srcOffset range; if that fails, scan
            // forward from the start.
            int srcOffIndex = IndexForOffset(srcOffset);
            if (IndexContainsAddress(srcOffIndex, targetAddr)) {
                Debug.Assert(targetAddr >= mAddrList[srcOffIndex].Addr);
                return (targetAddr - mAddrList[srcOffIndex].Addr) + mAddrList[srcOffIndex].Offset;
            }

            for (int i = 0; i < mAddrList.Count; i++) {
                if (i == srcOffIndex) {
                    // optimization -- we already checked this one
                    continue;
                }
                if (IndexContainsAddress(i, targetAddr)) {
                    Debug.Assert(targetAddr >= mAddrList[i].Addr);
                    return (targetAddr - mAddrList[i].Addr) + mAddrList[i].Offset;
                }
            }

            return -1;
        }

        /// <summary>
        /// Converts a file offset to an address.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <returns>24-bit address.</returns>
        public int OffsetToAddress(int offset) {
            int srcOffIndex = IndexForOffset(offset);
            return mAddrList[srcOffIndex].Addr + (offset - mAddrList[srcOffIndex].Offset);
        }


        /// <summary>
        /// Internal consistency checks.
        /// </summary>
        private void DebugValidate() {
            if (mAddrList.Count < 1) {
                throw new Exception("AddressMap: empty");
            }
            if (mAddrList[0].Offset != 0) {
                throw new Exception("AddressMap: bad offset 0");
            }

            if (mAddrList.Count == 1) {
                if (mAddrList[0].Length != mTotalLength) {
                    throw new Exception("AddressMap: single entry len bad");
                }
            } else {
                int totalLen = 0;
                for (int i = 0; i < mAddrList.Count; i++) {
                    AddressMapEntry ent = mAddrList[i];
                    if (i != 0) {
                        if (ent.Offset != mAddrList[i - 1].Offset + mAddrList[i - 1].Length) {
                            throw new Exception("Bad offset step to " + i);
                        }
                    }

                    totalLen += ent.Length;
                }

                if (totalLen != mTotalLength) {
                    throw new Exception("AddressMap: bad length sum (" + totalLen + " vs " +
                        mTotalLength + ")");
                }
            }
        }

        public override string ToString() {
            return "[AddressMap: " + mAddrList.Count + " entries]";
        }
    }
}
