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
using System.Collections.Generic;
using System.Diagnostics;

using Asm65;

namespace SourceGen {
    /// <summary>
    /// Holds a cache of formatted operands that may span multiple lines.
    /// </summary>
    /// <remarks>
    /// <para>This is intended for multi-line items with line counts that are non-trivial to
    /// compute, such as strings which may be a mix of characters and hex data.  The on-demand
    /// line formatter needs to be able to render the Nth line of a multi-line operand, and will
    /// potentially be very inefficient if it has to render lines 0 through N-1 as well.  (Imagine
    /// the list is rendered from end to start...)  Single-line items, and multi-line items that
    /// are easy to generate at an arbitrary offset (dense hex), aren't stored here.</para>
    ///
    /// <para>The trick is knowing when the cached data must be invalidated.  For example, a
    /// fully formatted string line must be invalidated if:</para>
    /// <list type="bullet">
    ///   <item>The Formatter changes (different delimiter definition)</item>
    ///   <item>The FormatDescriptor changes (different length, different text encoding, different
    ///     type of string)</item>
    ///   <item>The PseudoOpNames table changes (potentially altering the pseudo-op
    ///     string used)</item>
    /// </list>
    ///
    /// <para>Doing a full .equals() on the various items would reduce performance, so we use a
    /// simple test on reference equality when possible, and expect that the client will try
    /// to ensure that the various bits that are depended upon don't get replaced
    /// unnecessarily.</para>
    /// <para>We don't make much of an effort to purge stale entries, since that can only happen
    /// when the operand at a specific offset changes to something that doesn't require fancy
    /// formatting.  The total memory required for all entries is relatively small.</para>
    /// </remarks>
    public class FormattedOperandCache {
        /// <summary>
        /// One entry in the cache.
        /// </summary>
        private class FormattedStringEntry {
            public List<string> Lines { get; private set; }
            public string PseudoOpcode { get; private set; }

            private Formatter mFormatter;
            private FormatDescriptor mFormatDescriptor;
            private PseudoOp.PseudoOpNames mPseudoOpNames;

            public FormattedStringEntry(List<string> lines, string popcode, Formatter formatter,
                    FormatDescriptor formatDescriptor, PseudoOp.PseudoOpNames pseudoOpNames) {
                // Can't be sure the list won't change, so duplicate it.
                Lines = new List<string>(lines.Count);
                foreach (string str in lines) {
                    Lines.Add(str);
                }
                PseudoOpcode = popcode;

                mFormatter = formatter;
                mFormatDescriptor = formatDescriptor;
                mPseudoOpNames = pseudoOpNames;
            }

            /// <summary>
            /// Checks the entry's dependencies.
            /// </summary>
            /// <remarks>
            /// The data analyzer regenerates stuff in Anattribs, so we can't expect to have
            /// the same FormatDescriptor object.
            /// </remarks>
            /// <returns>True if the dependencies match.</returns>
            public bool CheckDeps(Formatter formatter, FormatDescriptor formatDescriptor,
                    PseudoOp.PseudoOpNames pseudoOpNames) {
                bool ok = (ReferenceEquals(mFormatter, formatter) &&
                    ReferenceEquals(mPseudoOpNames, pseudoOpNames) &&
                    mFormatDescriptor == formatDescriptor);
                //if (!ok) {
                //    Debug.WriteLine("CheckDeps:" +
                //        (ReferenceEquals(mFormatter, formatter) ? "" : " fmt") +
                //        (ReferenceEquals(mPseudoOpNames, pseudoOpNames) ? "" : " pop") +
                //        (mFormatDescriptor == formatDescriptor ? "" : " dfd"));
                //}
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
        /// <param name="formatDescriptor">FormatDescriptor dependency.</param>
        /// <param name="pseudoOpNames">PseudoOpNames dependency.</param>
        /// <param name="PseudoOpcode">Result: pseudo-op for this string.</param>
        /// <returns>A reference to the string list, or null if the entry is absent or invalid.
        ///   The caller must not modify the list.</returns>
        public List<string> GetStringEntry(int offset, Formatter formatter,
                FormatDescriptor formatDescriptor, PseudoOp.PseudoOpNames pseudoOpNames,
                out string PseudoOpcode) {
            PseudoOpcode = null;
            if (!mStringEntries.TryGetValue(offset, out FormattedStringEntry entry)) {
                DebugNotFoundCount++;
                return null;
            }
            if (!entry.CheckDeps(formatter, formatDescriptor, pseudoOpNames)) {
                //Debug.WriteLine("  stale entry at +" + offset.ToString("x6"));
                DebugFoundStaleCount++;
                return null;
            }
            DebugFoundValidCount++;
            PseudoOpcode = entry.PseudoOpcode;
            return entry.Lines;
        }

        /// <summary>
        /// Sets the string data entry for the specified offset.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <param name="lines">String data.</param>
        /// <param name="pseudoOpcode">Pseudo-opcode for this line.</param>
        /// <param name="formatter">Formatter dependency.</param>
        /// <param name="formatDescriptor">FormatDescriptor dependency.</param>
        /// <param name="pseudoOpNames">PseudoOpNames dependency.</param>
        public void SetStringEntry(int offset, List<string> lines, string pseudoOpcode,
                Formatter formatter, FormatDescriptor formatDescriptor,
                PseudoOp.PseudoOpNames pseudoOpNames) {
            Debug.Assert(lines != null);
            FormattedStringEntry fse = new FormattedStringEntry(lines, pseudoOpcode,
                formatter, formatDescriptor, pseudoOpNames);
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
            Debug.WriteLine("Operand cache: valid=" + DebugFoundValidCount + ", stale=" +
                DebugFoundStaleCount + ", missing=" + DebugNotFoundCount);
        }
    }
}
