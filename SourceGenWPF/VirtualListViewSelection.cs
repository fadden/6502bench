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

using CommonUtil;

namespace SourceGenWPF {
    /// <summary>
    /// Tracks the items selected in a list view.
    /// 
    /// Forward the ItemSelectionChanged and VirtualItemsSelectionRangeChanged.
    /// </summary>
    public class VirtualListViewSelection {
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

        public VirtualListViewSelection() {
            mSelection = new BitArray(0);
        }

        public VirtualListViewSelection(int length) {
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

#if false // TODO
        /// <summary>
        /// Handle a state change for a single item.
        /// </summary>
        public void ItemSelectionChanged(ListViewItemSelectionChangedEventArgs e) {
            //Debug.WriteLine("ItemSelectionChanged: " + e.ItemIndex + " (" + e.IsSelected + ")");
            if (e.ItemIndex >= mSelection.Length) {
                Debug.WriteLine("GLITCH: selection index " + e.ItemIndex + " out of range");
                Debug.Assert(false);
                return;
            }
            mSelection.Set(e.ItemIndex, e.IsSelected);
        }

        /// <summary>
        /// Handle a state change for a range of items.
        /// </summary>
        public void VirtualItemsSelectionRangeChanged(
                ListViewVirtualItemsSelectionRangeChangedEventArgs e) {
            //Debug.WriteLine("VirtualRangeChange: " + e.StartIndex + " - " + e.EndIndex +
            //    " (" + e.IsSelected + ")");

            if (e.StartIndex == 0 && e.EndIndex == mSelection.Length - 1) {
                // Set all elements.  The list view control seems to like to set all elements
                // to false whenever working with multi-select, so this should be fast.
                //Debug.WriteLine("VirtualRangeChange: set all to " + e.IsSelected);
                mSelection.SetAll(e.IsSelected);
            } else {
                if (e.EndIndex >= mSelection.Length) {
                    Debug.WriteLine("GLITCH: selection end index " + e.EndIndex + " out of range");
                    Debug.Assert(false);
                    return;
                }
                bool val = e.IsSelected;
                for (int i = e.StartIndex; i <= e.EndIndex; i++) {
                    mSelection.Set(i, val);
                }
            }
        }
#endif

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
