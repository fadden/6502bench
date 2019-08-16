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
using System.Windows.Controls;

using CommonUtil;

namespace SourceGen {
    /// <summary>
    /// Tracks the items selected in the DisplayList, using forwarded SelectionChanged events.
    /// When enumerated, provides an ordered list of selected indices.
    /// </summary>
    /// <remarks>
    /// In WPF you can't get indices, only items, so we have to store the item index in the
    /// item itself.
    /// </remarks>
    public class DisplayListSelection : IEnumerable<int> {
        private BitArray mSelection;

        /// <summary>
        /// Retrieves the total number of boolean values in the set.  This is NOT the
        /// number of selected items.
        /// </summary>
        public int Length { get { return mSelection.Length; } }

        /// <summary>
        /// Retrieves the number of values that are set.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Sets or gets the Nth element.  True means the line is selected.
        /// </summary>
        public bool this[int key] {
            get {
                return mSelection[key];
            }
            set {
                // If an entry has changed, update the count of set items.
                if (mSelection[key] != value) {
                    Count += value ? 1 : -1;
                    mSelection[key] = value;
                }
                Debug.Assert(Count >= 0 && Count <= Length);
            }
        }

        /// <summary>
        /// Constructs an empty list.
        /// </summary>
        public DisplayListSelection() {
            mSelection = new BitArray(0);
        }

        /// <summary>
        /// Constructs a list of the specified length.
        /// </summary>
        /// <param name="length">Number of elements.</param>
        public DisplayListSelection(int length) {
            mSelection = new BitArray(length);
        }

        /// <summary>
        /// Returns an enumeration of selected indices, in ascending order.
        /// </summary>
        public IEnumerator<int> GetEnumerator() {
            for (int i = 0; i < mSelection.Length; i++) {
                if (mSelection[i]) {
                    yield return i;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <summary>
        /// Sets the length of the selection array.
        /// 
        /// If the new length is longer, the new elements are initialized to false.  If the
        /// new length is shorter, the excess elements are discarded.
        /// </summary>
        /// <param name="length">New length.</param>
        //public void SetLength(int length) {
        //    mSelection.Length = length;
        //}

        /// <summary>
        /// Handles selection change.
        /// </summary>
        /// <param name="e">Argument from SelectionChanged event.</param>
        public void SelectionChanged(SelectionChangedEventArgs e) {
            //Debug.WriteLine("SelectionChanged event: Add=" + e.AddedItems.Count +
            //    " Rem=" + e.RemovedItems.Count);
            foreach (DisplayList.FormattedParts parts in e.AddedItems) {
                Debug.Assert(parts.ListIndex >= 0 && parts.ListIndex < mSelection.Length);
                this[parts.ListIndex] = true;
            }
            foreach (DisplayList.FormattedParts parts in e.RemovedItems) {
                Debug.Assert(parts.ListIndex >= 0);
                if (parts.ListIndex < mSelection.Length) {
                    this[parts.ListIndex] = false;
                } else {
                    Debug.WriteLine("Attempted to remove selected item off end of list: " + parts);
                }
            }
        }

        /// <summary>
        /// Returns the index of the first selected item, or -1 if nothing is selected.
        /// </summary>
        public int GetFirstSelectedIndex() {
            int idx;
            for (idx = 0; idx < mSelection.Length; idx++) {
                if (mSelection[idx]) {
                    break;
                }
            }
            if (idx == mSelection.Length) {
                idx = -1;
            }
            return idx;
        }

        /// <summary>
        /// Returns the index of the last selected item, or -1 if nothing is selected.
        /// </summary>
        public int GetLastSelectedIndex() {
            int idx;
            for (idx = mSelection.Length - 1; idx >= 0; idx--) {
                if (mSelection[idx]) {
                    break;
                }
            }
            return idx;
        }

        /// <summary>
        /// Returns true if all items are selected.
        /// </summary>
        public bool IsAllSelected() {
            return Count == Length;
        }

        /// <summary>
        /// Confirms that the selection count matches the number of set bits.  Pass
        /// in {ListView}.SelectedIndices.Count.
        /// </summary>
        /// <param name="expected">Expected number of selected entries.</param>
        /// <returns>True if count matches.</returns>
        public bool DebugValidateSelectionCount(int expected) {
            if (Count != expected) {
                Debug.WriteLine("SelectionCount expected " + expected + ", count=" + Count);
            }
            int computed = 0;
            foreach (bool bit in mSelection) {
                if (bit) {
                    computed++;
                }
            }
            if (Count != computed) {
                Debug.WriteLine("SelectionCount internal error: computed=" + computed +
                    ", count=" + Count);
            }
            return (Count == expected);
        }

        public void DebugDump() {
            RangeSet rangeSet = new RangeSet();
            for (int i = 0; i < mSelection.Length; i++) {
                if (mSelection[i]) {
                    rangeSet.Add(i);
                }
            }
            Debug.WriteLine("DisplayListSelection ranges:");
            IEnumerator<RangeSet.Range> iter = rangeSet.RangeListIterator;
            while (iter.MoveNext()) {
                RangeSet.Range range = iter.Current;
                Debug.WriteLine(" [" + range.Low.ToString() + "," + range.High.ToString() + "]");
            }
        }
    }
}
