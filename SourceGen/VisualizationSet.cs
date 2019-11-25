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
using System.Text;

namespace SourceGen {
    public class VisualizationSet : IEnumerable<Visualization> {
        /// <summary>
        /// Ordered list of visualization objects.
        /// </summary>
        private List<Visualization> mList;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cap">Initial capacity.</param>
        public VisualizationSet(int cap = 1) {
            mList = new List<Visualization>(cap);
        }

        // IEnumerable
        public IEnumerator<Visualization> GetEnumerator() {
            return mList.GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return mList.GetEnumerator();
        }

        /// <summary>
        /// The number of entries in the table.
        /// </summary>
        public int Count {
            get { return mList.Count; }
        }

        public void Add(Visualization vis) {
            mList.Add(vis);
        }


        public override string ToString() {
            return "[VS: " + mList.Count + " items]";
        }

        public static bool operator ==(VisualizationSet a, VisualizationSet b) {
            if (ReferenceEquals(a, b)) {
                return true;    // same object, or both null
            }
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) {
                return false;   // one is null
            }
            // All fields must be equal.
            if (a.mList.Count != b.mList.Count) {
                return false;
            }
            // Order matters.
            for (int i = 0; i < a.mList.Count; i++) {
                if (a.mList[i] != b.mList[i]) {
                    return false;
                }
            }
            return true;
        }
        public static bool operator !=(VisualizationSet a, VisualizationSet b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is VisualizationSet && this == (VisualizationSet)obj;
        }
        public override int GetHashCode() {
            int hashCode = 0;
            foreach (Visualization vis in mList) {
                hashCode ^= vis.GetHashCode();
            }
            return hashCode;
        }
    }
}
