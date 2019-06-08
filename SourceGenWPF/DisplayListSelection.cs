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

namespace SourceGenWPF {
    /// <summary>
    /// Tracks the items selected in the DisplayList.
    /// 
    /// Forward the SelectionChanged event.  In WPF you can't get indices, only items, so we
    /// have to store the item index in the item itself.
    /// </summary>
    public class DisplayListSelection {
        private BitArray mSelection;

        /// <summary>
        /// Retrieves the total number of boolean values in the set.  This is NOT the
        /// number of selected items.
        /// </summary>
        public int Length { get { return mSelection.Length; } }

        /// <summary>
        /// Sets or gets the Nth element.  True means the line is selected.
        /// </summary>
        public bool this[int key] {
            get {
                return mSelection[key];
            }
            set {
                mSelection[key] = value;
            }
        }

        public DisplayListSelection() {
            mSelection = new BitArray(0);
        }

        public DisplayListSelection(int length) {
            mSelection = new BitArray(length);
        }

        /// <summary>
        /// Sets the length of the selection array.
        /// 
        /// If the new length is longer, the new elements are initialized to false.  If the
        /// new length is shorter, the excess elements are discarded.  (This matches the behavior
        /// of a virtual ListView selection set.)
        /// </summary>
        /// <param name="length">New length.</param>
        public void SetLength(int length) {
            //Debug.WriteLine("VirtualListViewSelection length now " + length);
            mSelection.Length = length;
        }

        /// <summary>
        /// Handles selection change.
        /// </summary>
        /// <param name="e">Argument from SelectionChanged event.</param>
        public void SelectionChanged(SelectionChangedEventArgs e) {
            foreach (DisplayList.FormattedParts parts in e.AddedItems) {
                Debug.Assert(parts.ListIndex >= 0 && parts.ListIndex < mSelection.Length);
                mSelection.Set(parts.ListIndex, true);
            }
            foreach (DisplayList.FormattedParts parts in e.RemovedItems) {
                Debug.Assert(parts.ListIndex >= 0 && parts.ListIndex < mSelection.Length);
                mSelection.Set(parts.ListIndex, false);
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
        /// Confirms that the selection count matches the number of set bits.  Pass
        /// in {ListView}.SelectedIndices.Count.
        /// </summary>
        /// <param name="expected">Expected number of selected entries.</param>
        /// <returns>True if count matches.</returns>
        public bool DebugValidateSelectionCount(int expected) {
            int actual = 0;
            foreach (bool bit in mSelection) {
                if (bit) {
                    actual++;
                }
            }
            if (actual != expected) {
                Debug.WriteLine("SelectionCount expected " + expected + ", actual " + actual);
            }
            return (actual == expected);
        }

        public void DebugDump() {
            RangeSet rangeSet = new RangeSet();
            for (int i = 0; i < mSelection.Length; i++) {
                if (mSelection[i]) {
                    rangeSet.Add(i);
                }
            }
            Debug.WriteLine("VirtualListViewSelection ranges:");
            IEnumerator<RangeSet.Range> iter = rangeSet.RangeListIterator;
            while (iter.MoveNext()) {
                RangeSet.Range range = iter.Current;
                Debug.WriteLine(" [" + range.Low.ToString() + "," + range.High.ToString() + "]");
            }
        }
    }
}
