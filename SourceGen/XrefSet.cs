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

namespace SourceGen {
    /// <summary>
    /// Tracks a set of offsets that reference a single address or label.
    /// 
    /// This is used internally, when refactoring labels, as well as for the "references"
    /// UI panel and label localizer.
    /// </summary>
    public class XrefSet : IEnumerable<XrefSet.Xref> {
        /// <summary>
        /// Reference type.  This is mostly useful for display to the user.
        /// </summary>
        public enum XrefType {
            Unknown = 0,
            InstrOperand,       // generic instruction operand
            BranchOperand,      // branch instruction
            DataOperand         // e.g. ".dd2 <address>"
        }

        /// <summary>
        /// Cross-reference descriptor.  Instances are immutable.
        /// </summary>
        public class Xref {
            /// <summary>
            /// Offset of start of instruction or data with the reference.
            /// </summary>
            public int Offset { get; private set; }

            /// <summary>
            /// True if this reference is by name.
            /// </summary>
            public bool IsSymbolic { get; private set; }

            /// <summary>
            /// Type of reference.
            /// </summary>
            public XrefType Type { get; private set; }

            /// <summary>
            /// Adjustment to symbol.  For example, "LDA label+2" adds an xref entry to
            /// "label", with an adjustment of +2.
            /// </summary>
            public int Adjustment { get; private set; }

            public Xref(int offset, bool isSymbolic, XrefType type, int adjustment) {
                Offset = offset;
                IsSymbolic = isSymbolic;
                Type = type;
                Adjustment = adjustment;
            }

            public override string ToString() {
                return "Xref off=+" + Offset.ToString("x6") + " sym=" + IsSymbolic +
                    " type=" + Type + " adj=" + Adjustment;
            }
        }

        /// <summary>
        /// Internal storage for xrefs.
        /// </summary>
        private List<Xref> mRefs = new List<Xref>();


        /// <summary>
        /// Constructs an empty set.
        /// </summary>
        public XrefSet() { }

        /// <summary>
        /// Returns the number of cross-references in the set.
        /// </summary>
        public int Count { get { return mRefs.Count; } }

        /// <summary>
        /// Removes all entries from the set.
        /// </summary>
        public void Clear() {
            mRefs.Clear();
        }

        /// <summary>
        /// Returns the Nth entry in the set.
        /// </summary>
        public Xref this[int index] {
            get {
                return mRefs[index];
            }
        }

        /// <summary>
        /// Adds an xref to the set.
        /// </summary>
        public void Add(Xref xref) {
            // TODO(someday): not currently enforcing set behavior; start by adding .equals to
            //   Xref, then check Contains before allowing Add.  (Should probably complain
            //   loudly if item already exists, since we're not expecting that.)
            mRefs.Add(xref);
        }

        // IEnumerable
        public IEnumerator GetEnumerator() {
            return ((IEnumerable)mRefs).GetEnumerator();
        }

        // IEnumerable, generic
        IEnumerator<Xref> IEnumerable<Xref>.GetEnumerator() {
            return ((IEnumerable<Xref>)mRefs).GetEnumerator();
        }
    }
}
