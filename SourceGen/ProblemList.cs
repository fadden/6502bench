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

namespace SourceGen {
    /// <summary>
    /// List of problems noted during analysis of the project.
    /// </summary>
    /// <remarks>
    /// Could also make a place for load-time problems, though those tend to be resolved by
    /// discarding the offending data, so not much value in continuing to report them.
    /// </remarks>
    public class ProblemList : IEnumerable<ProblemList.ProblemEntry> {
        /// <summary>
        /// One problem entry.
        /// 
        /// Instances are immutable.
        /// </summary>
        public class ProblemEntry {
            public enum SeverityLevel {
                Unknown = 0,
                Info,
                Warning,
                Error
            }
            public SeverityLevel Severity { get; private set; }

            public int Offset { get; private set; }

            public enum ProblemType {
                Unknown = 0,
                HiddenLabel,
                UnresolvedWeakRef,
            }
            public ProblemType Problem { get; private set; }

            // Context object.  Could be a label string, a format descriptor, etc.
            public object Context { get; private set; }

            public enum ProblemResolution {
                Unknown = 0,
                LabelIgnored,
                FormatDescriptorIgnored,
            }
            public ProblemResolution Resolution { get; private set; }


            public ProblemEntry(SeverityLevel severity, int offset, ProblemType problem,
                    object context, ProblemResolution resolution) {
                Severity = severity;
                Offset = offset;
                Problem = problem;
                Context = context;
                Resolution = resolution;
            }

            public override string ToString() {
                return Severity.ToString() + " +" + Offset.ToString("x6") + " " +
                    Problem + "(" + Context.ToString() + "): " + Resolution;
            }
        }

        /// <summary>
        /// Maximum file offset.  Used to flag offsets as invalid.
        /// </summary>
        public int MaxOffset { get; set; }

        /// <summary>
        /// List of problems.  This is not kept in sorted order, because the DataGrid used to
        /// display it will do the sorting for us.
        /// </summary>
        private List<ProblemEntry> mList;


        /// <summary>
        /// Constructor.
        /// </summary>
        public ProblemList() {
            mList = new List<ProblemEntry>();
        }

        // IEnumerable
        public IEnumerator<ProblemEntry> GetEnumerator() {
            // .Values is documented as O(1)
            return mList.GetEnumerator();
        }

        // IEnumerable
        IEnumerator IEnumerable.GetEnumerator() {
            return mList.GetEnumerator();
        }

        public int Count {
            get { return mList.Count; }
        }

        public void Add(ProblemEntry entry) {
            mList.Add(entry);
        }

        public void Clear() {
            mList.Clear();
        }

        public void DebugDump() {
            Debug.WriteLine("Problem list:");
            foreach (ProblemEntry entry in mList) {
                Debug.WriteLine(entry);
            }
        }
    }
}
