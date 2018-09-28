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

namespace CommonUtil {
    /// <summary>
    /// Compact representation of a set of integers that tend to be adjacent.
    ///
    /// <para>The default enumeration is a series of integers, not a series of ranges.  Use
    /// RangeListIterator to get the latter.</para>
    ///
    /// <para>Most operations operate in log(N) time, where N is the number of
    /// regions.</para>
    /// </summary>
    public class RangeSet : IEnumerable<int> {
        /// <summary>
        /// List of ranges, in sorted order.
        /// </summary>
        private List<Range> mRangeList = new List<Range>();

        /// <summary>
        /// Number of values in the set.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// For unit tests: return the number of Range elements in the list.
        /// </summary>
        public int DebugRangeCount { get { return mRangeList.Count; } }

        /// <summary>
        /// Represents a contiguous range of values.
        /// </summary>
        public struct Range {
            /// <summary>
            /// Lowest value (inclusive).
            /// </summary>
            public int Low { get; set; }

            /// <summary>
            /// Highest value (inclusive).
            /// </summary>
            public int High { get; set; }

            public Range(int low, int high) {
                Debug.Assert(low <= high);
                Low = low;
                High = high;
            }

            /// <summary>
            /// Returns true if the specified value falls in this range.
            /// </summary>
            public bool Contains(int val) {
                return (val >= Low && val <= High);
            }
        }

        /// <summary>
        /// Iterator definition.
        /// </summary>
        private class RangeSetIterator : IEnumerator {
            /// <summary>
            /// The RangeSet we're iterating over.
            /// </summary>
            private RangeSet mSet;

            // Index of current Range element in mSet.mRangeList.
            private int mListIndex = -1;

            // Current range, extracted from mRangeList.
            private Range mCurrentRange;

            // Current value in mCurrentRange.
            private int mCurrentVal;


            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="set">RangeSet to iterate over.</param>
            public RangeSetIterator(RangeSet set) {
                mSet = set;
                Reset();
            }

            // IEnumerator: current element
            public object Current {
                get {
                    if (mListIndex < 0) {
                        // not started
                        return null;
                    }
                    return mCurrentVal;
                }
            }

            /// <summary>
            /// Puts the next range in the list in mCurrentRange.
            /// </summary>
            /// <returns>True on success, false if we reached the end of the list.</returns>
            private bool GetNextRange() {
                mListIndex++;   // increments to 0 on first invocation
                if (mListIndex == mSet.mRangeList.Count) {
                    // no more ranges
                    return false;
                }

                mCurrentRange = mSet.mRangeList[mListIndex];
                mCurrentVal = mCurrentRange.Low;
                return true;
            }

            // IEnumerator: move to the next element, returning false if there isn't one
            public bool MoveNext() {
                if (mListIndex < 0) {
                    // just started
                    return GetNextRange();
                } else {
                    // iterating within range object
                    mCurrentVal++;
                    if (mCurrentVal > mCurrentRange.High) {
                        // finished with this one, move on to the next
                        return GetNextRange();
                    } else {
                        return true;
                    }
                }
            }

            // IEnumerator: reset state
            public void Reset() {
                mListIndex = -1;
            }
        }


        /// <summary>
        /// General constructor. Creates an empty set.
        /// </summary>
        public RangeSet() {
            Count = 0;
        }

        /// <summary>
        /// Constructs set from an iterator.
        /// </summary>
        /// <param name="iter">Iterator that generates a set of integers in ascending order.</param>
        public RangeSet(IEnumerator iter) : this() {
            if (!iter.MoveNext()) {
                return;
            }
            int first = (int) iter.Current;
            Count++;
            Range curRange = new Range(first, first);

            while (iter.MoveNext()) {
                int val = (int) iter.Current;
                Count++;
                if (val == curRange.High + 1) {
                    // Add to current range.
                    curRange.High = val;
                } else {
                    // Not contiguous, create new range.
                    mRangeList.Add(curRange);
                    curRange = new Range(val, val);
                }
            }

            mRangeList.Add(curRange);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the range list, returning Range objects.
        /// </summary>
        public IEnumerator<Range> RangeListIterator {
            get { return mRangeList.GetEnumerator(); }
        }

        /// <summary>
        /// Removes all values from the set.
        /// </summary>
        public void Clear() {
            mRangeList.Clear();
            Count = 0;
        }

        // IEnumerable: get an enumerator instance that returns integer values
        public IEnumerator GetEnumerator() {
            return new RangeSetIterator(this);
        }

        // IEnumerable<int>
        IEnumerator<int> IEnumerable<int>.GetEnumerator() {
            return (IEnumerator<int>)GetEnumerator();
        }

        /// <summary>
        /// Finds the range that contains "val", or an appropriate place in the list to
        /// insert a new range.
        /// </summary>
        /// <param name="val">Value to find.</param>
        /// <returns>The index of the matching element, or a negative value indicating
        /// the index to insert at.  2C doesn't support negative 0, so the insertion
        /// index will be incremented before negation.</returns>
        private int FindValue(int val) {
            int low = 0;
            int high = mRangeList.Count - 1;
            while (low <= high) {
                int mid = (low + high) / 2;
                Range midRange = mRangeList[mid];

                if (midRange.Contains(val)) {
                    // found it
                    return mid;
                } else if (val < midRange.Low) {
                    // too big, move the high end in
                    high = mid - 1;
                } else if (val > midRange.High) {
                    // too small, move the low end in
                    low = mid + 1;
                } else {
                    // WTF... list not sorted?
                    throw new Exception("Bad binary search");
                }
            }

            // Not found, insert before "low".
            return -(low + 1);
        }

        /// <summary>
        /// Determines whether val is a member of the set.
        /// </summary>
        /// <param name="val">Value to check.</param>
        /// <returns>True if the value is a member of the set.</returns>
        public bool Contains(int val) {
            return (FindValue(val) >= 0);
        }

        /// <summary>
        /// Adds a value to the set.  If the value is already present, nothing changes.
        /// </summary>
        /// <param name="val">Value to add.</param>
        public void Add(int val) {
            int listIndex = FindValue(val);
            if (listIndex >= 0) {
                // Already present in set.
                return;
            }
            Count++;

            if (mRangeList.Count == 0) {
                // Empty list, skip the gymnastics.
                mRangeList.Add(new Range(val, val));
                return;
            }

            // Negate and decrement to get insertion index.  This value may == Count if
            // the value is higher than all current members.
            listIndex = -listIndex - 1;

            if (listIndex > 0 && mRangeList[listIndex - 1].High == val - 1) {
                // Expand prior range.  Check to see if it blends into next.
                if (listIndex < mRangeList.Count && mRangeList[listIndex].Low == val + 1) {
                    // Combine ranges.
                    Range prior = mRangeList[listIndex - 1];
                    Range next = mRangeList[listIndex];
                    Debug.Assert(prior.High + 2 == next.Low);
                    prior.High = next.High;
                    mRangeList[listIndex - 1] = prior;
                    mRangeList.RemoveAt(listIndex);
                } else {
                    // Nope, just expand the prior range.
                    Range prior = mRangeList[listIndex - 1];
                    Debug.Assert(prior.High == val - 1);
                    prior.High = val;
                    mRangeList[listIndex - 1] = prior;
                }
            } else if (listIndex < mRangeList.Count && mRangeList[listIndex].Low == val + 1) {
                // Expand next range.
                Range next = mRangeList[listIndex];
                Debug.Assert(next.Low == val + 1);
                next.Low = val;
                mRangeList[listIndex] = next;
            } else {
                // Add a new single-entry element.
                mRangeList.Insert(listIndex, new Range(val, val));
            }
        }

        /// <summary>
        /// Adds a range of contiguous values to the set.
        /// </summary>
        /// <param name="low">Lowest value (inclusive).</param>
        /// <param name="high">Highest value (inclusive).</param>
        public void AddRange(int low, int high) {
            // There's probably some very efficient way to do this.  Keeping it simple for now.
            for (int i = low; i <= high; i++) {
                Add(i);
            }
        }

        /// <summary>
        /// Removes a value from the set.  If the value is not present, nothing changes.
        /// </summary>
        /// <param name="val">Value to remove.</param>
        public void Remove(int val) {
            int listIndex = FindValue(val);
            if (listIndex < 0) {
                // not found
                return;
            }

            Count--;

            Range rng = mRangeList[listIndex];
            if (rng.Low == val && rng.High == val) {
                // Single-value range.  Remove.
                mRangeList.RemoveAt(listIndex);
            } else if (rng.Low == val) {
                // We're at the low end, reduce range.
                rng.Low = val + 1;
                mRangeList[listIndex] = rng;
            } else if (rng.High == val) {
                // We're at the high end, reduce range.
                rng.High = val - 1;
                mRangeList[listIndex] = rng;
            } else {
                // We're in the middle, split the range.
                Range next = new Range(val + 1, rng.High);
                rng.High = val - 1;
                mRangeList[listIndex] = rng;
                mRangeList.Insert(listIndex + 1, next);
            }
        }


        /// <summary>
        /// Internal test function.
        /// </summary>
        private static bool CheckRangeSet(RangeSet set, int expectedRanges, int[] expected) {
            if (set.DebugRangeCount != expectedRanges) {
                Debug.WriteLine("Expected " + expectedRanges + " ranges, got " +
                    set.DebugRangeCount);
                return false;
            }

            // Compare actual vs. expected. If we have more actual than expected we'll
            // throw on the array access.
            int expIndex = 0;
            foreach (int val in set) {
                if (val != expected[expIndex]) {
                    Debug.WriteLine("Expected " + expected[expIndex] + ", got " + val);
                    return false;
                }
                expIndex++;
            }

            // See if we have more expected than actual.
            if (expIndex != expected.Length) {
                Debug.WriteLine("Expected " + expected.Length + " elements, found " + expIndex);
                return false;
            }

            // The count is maintained separately, so check it.
            if (set.Count != expected.Length) {
                Debug.WriteLine("Expected Count=" + expected.Length + ", got " + set.Count);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Executes unit tests.
        /// </summary>
        /// <returns>True if all goes well.</returns>
        public static bool Test() {
            bool result = true;

            RangeSet one = new RangeSet();
            one.Add(7);
            one.Add(5);
            one.Add(3);
            one.Add(9);
            one.Add(7);
            one.Add(8);
            one.Add(2);
            one.Add(4);
            result &= CheckRangeSet(one, 2, new int[] { 2, 3, 4, 5, 7, 8, 9 });

            one.Remove(2);
            one.Remove(9);
            one.Remove(4);
            result &= CheckRangeSet(one, 3, new int[] { 3, 5, 7, 8 });

            one.Clear();
            one.AddRange(10, 15);
            result &= CheckRangeSet(one, 1, new int[] { 10, 11, 12, 13, 14, 15 });

            one.Add(-1);
            one.Add(0);
            one.Add(-2);
            result &= CheckRangeSet(one, 2, new int[] { -2, -1, 0, 10, 11, 12, 13, 14, 15 });

            Debug.WriteLine("RangeSet: test complete (ok=" + result + ")");
            return result;
        }
    }
}
