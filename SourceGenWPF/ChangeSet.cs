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
    /// Holds information about a set of changes.
    /// 
    /// Does not have hooks into other data structures.  This just holds the information
    /// about the changes.
    /// </summary>
    public class ChangeSet : IEnumerable<UndoableChange> {
        private List<UndoableChange> mChanges;

        /// <summary>
        /// Constructs an empty ChangeSet with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">Initial number of elements that the set can contain.</param>
        public ChangeSet(int capacity) {
            mChanges = new List<UndoableChange>(capacity);
        }

        /// <summary>
        /// Constructs a ChangeSet with a single change.
        /// </summary>
        public ChangeSet(UndoableChange ac) {
            mChanges = new List<UndoableChange>(1);
            mChanges.Add(ac);
        }

        /// <summary>
        /// The number of changes in the set.
        /// </summary>
        public int Count { get { return mChanges.Count; } }

        /// <summary>
        /// Returns the Nth change in the set.
        /// </summary>
        /// <param name="key">Change index.</param>
        public UndoableChange this[int key] {
            get {
                return mChanges[key];
            }
        }

        /// <summary>
        /// Adds a change to the change set.
        /// </summary>
        /// <param name="change">Change to add.</param>
        public void Add(UndoableChange change) {
            Debug.Assert(change != null);
            mChanges.Add(change);
        }

        /// <summary>
        /// Adds a change to the change set if the object is non-null.
        /// </summary>
        /// <param name="change">Change to add, or null.</param>
        public void AddNonNull(UndoableChange change) {
            if (change != null) {
                Add(change);
            }
        }

        /// <summary>
        /// Trims unused capacity from the set.
        /// </summary>
        public void TrimExcess() {
            mChanges.TrimExcess();
        }

        // IEnumerable, so we can use foreach syntax when going forward
        public IEnumerator GetEnumerator() {
            return mChanges.GetEnumerator();
        }

        // IEnumerable: generic version
        IEnumerator<UndoableChange> IEnumerable<UndoableChange>.GetEnumerator() {
            return mChanges.GetEnumerator();
        }

        // TODO(maybe): reverse-order enumerator?

        public override string ToString() {
            string str = "[CS: count=" + mChanges.Count;
            if (mChanges.Count > 0) {
                str += " {0:" + mChanges[0] + "}";
            }
            str += "]";
            return str;
        }
    }
}
