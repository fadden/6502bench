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
    /// Compact representation of a set of typed integers that tend to be adjacent.
    /// We expect there to be relatively few different types of things.
    ///
    /// <para>The default enumeration is a series of integers, not a series of ranges.  Use
    /// RangeListIterator to get the latter.</para>
    ///
    /// <para>Most operations operate in log(N) time, where N is the number of
    /// regions.</para>
    /// </summary>
    public class TypedRangeSet : IEnumerable<TypedRangeSet.Tuple> {
        /// <summary>
        /// List of ranges, in sorted order.
        /// </summary>
        private List<TypedRange> mRangeList = new List<TypedRange>();

        /// <summary>
        /// Number of values in the set.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Returns the number of Range elements in the list.
        /// </summary>
        public int RangeCount { get { return mRangeList.Count; } }

        /// <summary>
        /// Represents a contiguous range of values.
        /// </summary>
        public struct TypedRange {
            /// <summary>
            /// Lowest value (inclusive).
            /// </summary>
            public int Low { get; set; }

            /// <summary>
            /// Highest value (inclusive).
            /// </summary>
            public int High { get; set; }

            /// <summary>
            /// Value type in this range.
            /// </summary>
            public int Type { get; set; }

            public TypedRange(int low, int high, int type) {
                Debug.Assert(low <= high);
                Low = low;
                High = high;
                Type = type;
            }

            public bool Contains(int val) {
                return (val >= Low && val <= High);
            }
        }

        /// <summary>
        /// Value + type pair.  Returned from foreach enumerator.
        /// </summary>
        public struct Tuple {
            public int Value;
            public int Type;

            public Tuple(int value, int type) {
                Value = value;
                Type = type;
            }

            public static bool operator ==(Tuple a, Tuple b) {
                return a.Value == b.Value && a.Type == b.Type;
            }
            public static bool operator !=(Tuple a, Tuple b) {
                return !(a == b);
            }
            public override bool Equals(object obj) {
                return obj is Tuple && this == (Tuple)obj;
            }
            public override int GetHashCode() {
                return Value ^ Type;
            }
            public override string ToString() {
                return Value + " (" + Type + ")";
            }
        }

        /// <summary>
        /// Iterator definition.
        /// </summary>
        private class TypedRangeSetIterator : IEnumerator<Tuple> {
            /// <summary>
            /// The TypedRangeSet we're iterating over.
            /// </summary>
            private TypedRangeSet mSet;

            // Index of current Range element in mSet.mRangeList.
            private int mListIndex = -1;

            // Current range, extracted from mRangeList.
            private TypedRange mCurrentRange;

            // Current value in mCurrentRange.
            private int mCurrentVal;


            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="set">TypedRangeSet to iterate over.</param>
            public TypedRangeSetIterator(TypedRangeSet set) {
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
                    return new Tuple(mCurrentVal, mCurrentRange.Type);
                }
            }

            // IEnumerator<Tuple>
            Tuple IEnumerator<Tuple>.Current {
                get {
                    return (Tuple)Current;
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

            // IEnumerator<Tuple>
            public void Dispose() {
                mSet = null;
            }
        }


        /// <summary>
        /// Constructor. Creates an empty set.
        /// </summary>
        public TypedRangeSet() {
            Count = 0;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the range list, returning Range objects.
        /// </summary>
        public IEnumerator<TypedRange> RangeListIterator {
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
            return new TypedRangeSetIterator(this);
        }

        // IEnumerable<Tuple>
        IEnumerator<Tuple> IEnumerable<Tuple>.GetEnumerator() {
            return (IEnumerator<Tuple>)GetEnumerator();
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
                TypedRange midRange = mRangeList[mid];

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
        /// Gets the type of the specified value.
        /// </summary>
        /// <param name="val">Value to query.</param>
        /// <param name="type">Receives the type, or -1 if the value is not in the set.</param>
        /// <returns>True if the value is in the set.</returns>
        public bool GetType(int val, out int type) {
            int listIndex = FindValue(val);
            if (listIndex >= 0) {
                type = mRangeList[listIndex].Type;
                return true;
            } else {
                type = -1;
                return false;
            }
        }

        /// <summary>
        /// Adds or changes a value to the set.  If the value is already present and has
        /// a matching type, nothing changes.
        /// </summary>
        /// <param name="val">Value to add.</param>
        /// <param name="type">Value's type.</param>
        public void Add(int val, int type) {
            int listIndex = FindValue(val);
            if (listIndex >= 0) {
                // Value is present in set, check type.
                if (mRangeList[listIndex].Type == type) {
                    // It's a match, do nothing.
                    return;
                }

                // Wrong type. Remove previous entry, then fall through to add new.
                Remove(val);
                listIndex = FindValue(val);     // get insertion point
            }
            Count++;

            if (mRangeList.Count == 0) {
                // Empty list, skip the gymnastics.
                mRangeList.Add(new TypedRange(val, val, type));
                return;
            }

            // Negate and decrement to get insertion index.  This value may == Count if
            // the value is higher than all current members.
            listIndex = -listIndex - 1;

            if (listIndex > 0 && mRangeList[listIndex - 1].High == val - 1 &&
                    mRangeList[listIndex - 1].Type == type) {
                // Expand prior range.  Check to see if it blends into next as well.
                if (listIndex < mRangeList.Count && mRangeList[listIndex].Low == val + 1 &&
                        mRangeList[listIndex].Type == type) {
                    // Combine ranges.
                    TypedRange prior = mRangeList[listIndex - 1];
                    TypedRange next = mRangeList[listIndex];
                    Debug.Assert(prior.High + 2 == next.Low);
                    prior.High = next.High;
                    mRangeList[listIndex - 1] = prior;
                    mRangeList.RemoveAt(listIndex);
                } else {
                    // Nope, just expand the prior range.
                    TypedRange prior = mRangeList[listIndex - 1];
                    Debug.Assert(prior.High == val - 1);
                    prior.High = val;
                    mRangeList[listIndex - 1] = prior;
                }
            } else if (listIndex < mRangeList.Count && mRangeList[listIndex].Low == val + 1 &&
                    mRangeList[listIndex].Type == type) {
                // Expand next range.
                TypedRange next = mRangeList[listIndex];
                Debug.Assert(next.Low == val + 1);
                next.Low = val;
                mRangeList[listIndex] = next;
            } else {
                // Nothing adjacent, add a new single-entry element.
                mRangeList.Insert(listIndex, new TypedRange(val, val, type));
            }
        }

        /// <summary>
        /// Adds a range of contiguous values to the set.
        /// </summary>
        /// <param name="low">Lowest value (inclusive).</param>
        /// <param name="high">Highest value (inclusive).</param>
        /// <param name="high">Value type.</param>
        public void AddRange(int low, int high, int type) {
            // There's probably some very efficient way to do this.  Keeping it simple for now.
            for (int i = low; i <= high; i++) {
                Add(i, type);
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

            TypedRange rng = mRangeList[listIndex];
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
                TypedRange next = new TypedRange(val + 1, rng.High, rng.Type);
                rng.High = val - 1;
                mRangeList[listIndex] = rng;
                mRangeList.Insert(listIndex + 1, next);
            }
        }


        /// <summary>
        /// Internal test function.
        /// </summary>
        private static bool CheckTypedRangeSet(TypedRangeSet set, int expectedRanges,
                Tuple[] expected) {
            if (set.RangeCount != expectedRanges) {
                Debug.WriteLine("Expected " + expectedRanges + " ranges, got " +
                    set.RangeCount);
                return false;
            }

            // Compare actual vs. expected. If we have more actual than expected we'll
            // throw on the array access.
            int expIndex = 0;
            foreach (TypedRangeSet.Tuple val in set) {
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

            TypedRangeSet one = new TypedRangeSet();
            one.Add(7, 100);
            one.Add(5, 100);
            one.Add(3, 100);
            one.Add(9, 100);
            one.Add(7, 100);
            one.Add(8, 100);
            one.Add(2, 100);
            one.Add(4, 100);
            result &= CheckTypedRangeSet(one, 2, new Tuple[] {
                new Tuple(2, 100),
                new Tuple(3, 100),
                new Tuple(4, 100),
                new Tuple(5, 100),
                new Tuple(7, 100),
                new Tuple(8, 100),
                new Tuple(9, 100) });

            one.Remove(2);
            one.Remove(9);
            one.Remove(4);
            result &= CheckTypedRangeSet(one, 3, new Tuple[] {
                new Tuple(3, 100),
                new Tuple(5, 100),
                new Tuple(7, 100),
                new Tuple(8, 100) });

            one.Clear();
            one.Add(1, 200);
            one.Add(3, 100);
            one.Add(7, 100);
            one.Add(5, 100);
            one.Add(9, 100);
            one.Add(6, 100);
            one.Add(8, 100);
            one.Add(6, 200);
            one.Add(2, 200);
            one.Add(4, 300);
            one.Add(4, 100);
            result &= CheckTypedRangeSet(one, 4, new Tuple[] {
                new Tuple(1, 200),
                new Tuple(2, 200),
                new Tuple(3, 100),
                new Tuple(4, 100),
                new Tuple(5, 100),
                new Tuple(6, 200),
                new Tuple(7, 100),
                new Tuple(8, 100),
                new Tuple(9, 100) });

            one.Add(6, 100);
            result &= CheckTypedRangeSet(one, 2, new Tuple[] {
                new Tuple(1, 200),
                new Tuple(2, 200),
                new Tuple(3, 100),
                new Tuple(4, 100),
                new Tuple(5, 100),
                new Tuple(6, 100),
                new Tuple(7, 100),
                new Tuple(8, 100),
                new Tuple(9, 100) });

            Debug.WriteLine("TypedRangeSet: test complete (ok=" + result + ")");
            return result;
        }
    }
}
