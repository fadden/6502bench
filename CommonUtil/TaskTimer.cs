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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CommonUtil {
    /// <summary>
    /// Collects timestamps from a series of events.  Events may be nested, but may not overlap.
    /// A summary of task durations can be written to a log.
    /// </summary>
    public class TaskTimer {
        // TODO(maybe): create a start/end pair that works with a "using" directive to ensure
        // that a given item is always closed.

        /// <summary>
        /// Timed task info.
        /// </summary>
        private class TimedItem {
            public string mTag;
            public int mIndentLevel;
            public DateTime mStartWhen;
            public DateTime mEndWhen;

            public TimedItem(string tag, int indentLevel) {
                mTag = tag;
                mIndentLevel = indentLevel;
                mStartWhen = DateTime.Now;
            }
        }

        /// <summary>
        /// List of items.  They are ordered by when the tasks ended.
        /// </summary>
        private List<TimedItem> mItems = new List<TimedItem>();

        /// <summary>
        /// Indentation level.  Cosmetic.
        /// </summary>
        private int mIndentLevel = 0;

        /// <summary>
        /// Place where next record is to be inserted.
        /// 
        /// We keep inserting records ahead of whatever we inserted last, only advancing
        /// the insertion point when we close a record.  Essentially a stack that moves
        /// forward when you pop() instead of removing the element.  This lets us handle
        /// nested tasks correctly.
        /// </summary>
        private int mInsertPoint = 0;


        /// <summary>
        /// Resets object to initial state.
        /// </summary>
        public void Clear() {
            mItems.Clear();
            mIndentLevel = mInsertPoint = 0;
        }

        /// <summary>
        /// Adds a start record for a task.
        /// </summary>
        /// <param name="tag">Task tag.</param>
        public void StartTask(string tag) {
            TimedItem ti = new TimedItem(tag, mIndentLevel);
            mItems.Insert(mInsertPoint, ti);
            mIndentLevel++;
        }

        /// <summary>
        /// Closes out a record.  The tag must match the most recently started task.
        /// </summary>
        /// <param name="tag">Task tag.</param>
        public void EndTask(string tag) {
            TimedItem lastItem = mItems[mInsertPoint];
            if (lastItem.mTag != tag) {
                Debug.WriteLine("ERROR: tag mismatch: " + tag + " vs. " + lastItem.mTag);
                Debug.Assert(false);
                return;
            }

            lastItem.mEndWhen = DateTime.Now;
            mIndentLevel--;
            mInsertPoint++;
            Debug.Assert(mIndentLevel >= 0);
        }

        /// <summary>
        /// Prints the timing data into a log object.
        /// </summary>
        /// <param name="log">Output destination.</param>
        /// <param name="msg">Header message.</param>
        public void DumpTimes(string msg, DebugLog log) {
            if (mItems.Count == 0) {
                return;
            }
            if (!string.IsNullOrEmpty(msg)) {
                log.LogI(msg);
            }
            StringBuilder sb = new StringBuilder();
            int lastIndent = 0;
            foreach (TimedItem ti in mItems) {
                sb.Clear();
                FormatItem(ti, ref lastIndent, sb);
                log.LogI(sb.ToString());
            }

            //DateTime firstStart = mItems[0].mStartWhen;
            //DateTime lastEnd = mItems[mItems.Count - 1].mEndWhen;
            //log.LogI(" Total: " + (lastEnd - firstStart).TotalMilliseconds + " ms");
        }

        /// <summary>
        /// Prints the timing data into the debug log.
        /// </summary>
        /// <param name="msg">Header message.</param>
        public void DumpTimes(string msg) {
            if (mItems.Count == 0) {
                return;
            }
            if (!string.IsNullOrEmpty(msg)) {
                Debug.WriteLine(msg);
            }
            StringBuilder sb = new StringBuilder();
            int lastIndent = 0;
            foreach (TimedItem ti in mItems) {
                sb.Clear();
                FormatItem(ti, ref lastIndent, sb);
                Debug.WriteLine(sb.ToString());
            }
        }

        /// <summary>
        /// Prints the timing data into a string with newlines.
        /// </summary>
        /// <param name="log">Output destination.</param>
        /// <param name="msg">Header message.</param>
        public string DumpToString(string msg) {
            if (mItems.Count == 0) {
                return msg;
            }
            StringBuilder sb = new StringBuilder();
#if DEBUG
            sb.Append("[NOTE: debug build -- assertions and extra checks are enabled]\r\n\r\n");
#endif
            if (!string.IsNullOrEmpty(msg)) {
                sb.Append(msg);
                sb.Append("\r\n\r\n");
            }
            int lastIndent = 0;
            foreach (TimedItem ti in mItems) {
                FormatItem(ti, ref lastIndent, sb);
                sb.Append("\r\n");
            }

            return sb.ToString();
        }


        /// <summary>
        /// Formats the specified item, appending it to the StringBuilder.
        /// </summary>
        /// <param name="ti">Item to format.</param>
        /// <param name="lastIndent">Previous indentation level.</param>
        /// <param name="sb">StringBuilder to append to.</param>
        private void FormatItem(TimedItem ti, ref int lastIndent, StringBuilder sb) {
            for (int i = 0; i <= ti.mIndentLevel - 1; i++) {
                sb.Append("| ");
            }
            if (lastIndent < ti.mIndentLevel) {
                //sb.Append("/-");
                sb.Append("/ ");
            } else /*if (lastIndent == ti.mIndentLevel)*/ {
                sb.Append("| ");
            }
            sb.Append(ti.mTag);
            sb.Append(": ");
            sb.Append((ti.mEndWhen - ti.mStartWhen).TotalMilliseconds.ToString());
            sb.Append(" ms");

            lastIndent = ti.mIndentLevel;
        }
    }
}
