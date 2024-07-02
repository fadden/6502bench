/*
 * Copyright 2024 faddenSoft
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
using System.Collections.Generic;
using System.Diagnostics;

using Asm65;

namespace SourceGen {
    /// <summary>
    /// Holds a cache of formatted multi-line comments.
    /// </summary>
    /// <remarks>
    /// <para>We need to discard the entry if the MLC changes or the formatting parameters
    /// change.  MLCs are immutable and Formatters can't be reconfigured, so we can just do
    /// a quick reference equality check.</para>
    /// </remarks>
    public class FormattedMlcCache {
        /// <summary>
        /// One entry in the cache.
        /// </summary>
        private class FormattedStringEntry {
            public List<string> Lines { get; private set; }

            private MultiLineComment mMlc;
            private Formatter mFormatter;

            public FormattedStringEntry(List<string> lines, MultiLineComment mlc,
                    Formatter formatter) {
                // Can't be sure the list won't change, so duplicate it.
                Lines = new List<string>(lines.Count);
                foreach (string str in lines) {
                    Lines.Add(str);
                }

                mMlc = mlc;
                mFormatter = formatter;
            }

            /// <summary>
            /// Checks the entry's dependencies.
            /// </summary>
            /// <returns>True if the dependencies match.</returns>
            public bool CheckDeps(MultiLineComment mlc, Formatter formatter) {
                bool ok = (ReferenceEquals(mMlc, mlc) && ReferenceEquals(mFormatter, formatter));
                return ok;
            }
        }

        /// <summary>
        /// Cached entries, keyed by file offset.
        /// </summary>
        private Dictionary<int, FormattedStringEntry> mStringEntries =
            new Dictionary<int, FormattedStringEntry>();


        /// <summary>
        /// Retrieves the formatted string data for the specified offset.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <param name="formatter">Formatter dependency.</param>
        /// <returns>A reference to the string list, or null if the entry is absent or invalid.
        ///   The caller must not modify the list.</returns>
        public List<string> GetStringEntry(int offset, MultiLineComment mlc, Formatter formatter) {
            if (!mStringEntries.TryGetValue(offset, out FormattedStringEntry entry)) {
                DebugNotFoundCount++;
                return null;
            }
            if (!entry.CheckDeps(mlc, formatter)) {
                //Debug.WriteLine("  stale entry at +" + offset.ToString("x6"));
                DebugFoundStaleCount++;
                return null;
            }
            DebugFoundValidCount++;
            return entry.Lines;
        }

        /// <summary>
        /// Sets the string data entry for the specified offset.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <param name="lines">String data.</param>
        /// <param name="mlc">Multi-line comment to be formatted.</param>
        /// <param name="formatter">Formatter dependency.</param>
        public void SetStringEntry(int offset, List<string> lines, MultiLineComment mlc,
                Formatter formatter) {
            Debug.Assert(lines != null);
            FormattedStringEntry fse = new FormattedStringEntry(lines, mlc, formatter);
            mStringEntries[offset] = fse;
        }

        // Some counters for evaluating efficacy.
        public int DebugFoundValidCount { get; private set; }
        public int DebugFoundStaleCount { get; private set; }
        public int DebugNotFoundCount { get; private set; }
        public void DebugResetCounters() {
            DebugFoundValidCount = DebugFoundStaleCount = DebugNotFoundCount = 0;
        }
        public void DebugLogCounters() {
            Debug.WriteLine("MLC cache: valid=" + DebugFoundValidCount + ", stale=" +
                DebugFoundStaleCount + ", missing=" + DebugNotFoundCount);
        }
    }
}
