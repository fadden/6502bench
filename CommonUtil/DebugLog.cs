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
using System.IO;
using System.Text;

namespace CommonUtil {
    /// <summary>
    /// Debug log facility, with priority levels and time stamps.
    /// 
    /// The logs are held in memory.  The internal storage expands as logs are added
    /// until the maximum size is reached, then switches to circular buffering.  This
    /// minimizes overhead for small logs while avoiding infinite expansion.
    /// </summary>
    public class DebugLog {
        /// <summary>
        /// Log priority levels, in ascending order.  "Silent" is only used as an argument
        /// when setting the minimum priority level.
        /// </summary>
        public enum Priority {
            Verbose = 0, Debug, Info, Warning, Error, Silent
        }

        private static char[] sSingleLetter = { 'V', 'D', 'I', 'W', 'E', 'S' };

        /// <summary>
        /// Holds a single log entry.
        /// </summary>
        private struct LogEntry {
            public DateTime mWhen;
            public Priority mPriority;
            public string mText;

            public LogEntry(Priority prio, string msg) {
                mWhen = DateTime.Now;
                mPriority = prio;
                mText = msg;
            }
        }

        /// <summary>
        /// Log collection.
        /// </summary>
        private List<LogEntry> mEntries = new List<LogEntry>();
        private int mTopEntry = 0;

        /// <summary>
        /// Date/time when the log object was created.  Used for relative time display mode.
        /// </summary>
        private DateTime mStartWhen;

        /// <summary>
        /// If set, display time stamps as relative time rather than absolute.
        /// </summary>
        private bool mShowRelTime = false;

        /// <summary>
        /// Minimum priority level.  Anything below this is ignored.
        /// </summary>
        private Priority mMinPriority = Priority.Debug;

        /// <summary>
        /// Maximum number of lines we'll hold in memory.  This is a simple measure
        /// to keep the process from expanding without bound.
        /// </summary>
        private int mMaxLines = 100000;

        /// <summary>
        /// Constructor. Configures min priority to Info.
        /// </summary>
        public DebugLog() : this(Priority.Info) { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="prio">Minimum log priority level.</param>
        public DebugLog(Priority prio) {
            mMinPriority = prio;
            mStartWhen = DateTime.Now;
            LogI("Log started at " + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss zzz"));
        }

        /// <summary>
        /// Sets the message priority threshold. Messages below the specified priority
        /// will be ignored.
        /// </summary>
        /// <param name="prio">Minimum priority value.</param>
        public void SetMinPriority(Priority prio) {
            mMinPriority = prio;
        }

        /// <summary>
        /// Sets the "show relative time" flag.  If set, the timestamp in the log is
        /// relative to when the log object was created, instead of wall-clock time.
        /// </summary>
        /// <param name="showRelTime"></param>
        public void SetShowRelTime(bool showRelTime) {
            mShowRelTime = showRelTime;
        }

        /// <summary>
        /// Returns true if a message logged at the specified priority would be accepted.
        /// </summary>
        /// <param name="prio"></param>
        /// <returns></returns>
        public bool IsLoggable(Priority prio) {
            return prio >= mMinPriority;
        }

        /// <summary>
        /// Clears all entries.
        /// </summary>
        public void Clear() {
            mEntries.Clear();
        }

        /// <summary>
        /// Adds a message to the log buffer.
        /// </summary>
        /// <param name="prio">Log priority.</param>
        /// <param name="message">Message to log.</param>
        public void Log(Priority prio, string message) {
            if (prio < mMinPriority) {
                return;
            }
            LogEntry ent = new LogEntry(prio, message);
            if (mEntries.Count < mMaxLines) {
                // Still growing.
                mEntries.Add(ent);
            } else {
                // Circular replacement.  Adding to the end then removing [0] has
                // significant performance issues.
                mEntries[mTopEntry++] = ent;
                if (mTopEntry == mMaxLines) {
                    mTopEntry = 0;
                }
            }
        }

        public void LogV(string message) {
            Log(Priority.Verbose, message);
        }
        public void LogD(string message) {
            Log(Priority.Debug, message);
        }
        public void LogI(string message) {
            Log(Priority.Info, message);
        }
        public void LogW(string message) {
            Log(Priority.Warning, message);
        }
        public void LogE(string message) {
            Log(Priority.Error, message);
        }

        /// <summary>
        /// Dumps the contents of the log to a file.
        /// </summary>
        /// <param name="pathName">Full or partial pathname.</param>
        public void WriteToFile(string pathName) {
            StringBuilder sb = new StringBuilder();
            using (StreamWriter sw = new StreamWriter(pathName, false, Encoding.UTF8)) {
                for (int i = mTopEntry; i < mEntries.Count; i++) {
                    WriteEntry(sw, mEntries[i], sb);
                }
                for (int i = 0; i < mTopEntry; i++) {
                    WriteEntry(sw, mEntries[i], sb);
                }
            }
        }

        /// <summary>
        /// Writes a single entry to a file.  Pass in a StringBuilder so we don't have
        /// to create a new one every time.
        /// </summary>
        private void WriteEntry(StreamWriter sw, LogEntry ent, StringBuilder sb) {
            sb.Clear();
            FormatEntry(ent, sb);
            sw.WriteLine(sb.ToString());
        }

        /// <summary>
        /// Formats an entry, appending the text to the provided StringBuilder.
        /// </summary>
        private void FormatEntry(LogEntry ent, StringBuilder sb) {
            if (mShowRelTime) {
                sb.Append((ent.mWhen - mStartWhen).ToString(@"mm\:ss\.fff"));
            } else {
                sb.Append(ent.mWhen.ToString(@"hh\:mm\:ss\.fff"));
            }
            sb.Append(' ');
            sb.Append(sSingleLetter[(int)ent.mPriority]);
            sb.Append(' ');
            sb.Append(ent.mText);
        }

        /// <summary>
        /// Dumps the contents of the log to a string.  This is intended for display in a
        /// text box, so lines are separated with CRLF.
        /// </summary>
        /// <returns></returns>
        public string WriteToString() {
            StringBuilder sb = new StringBuilder();
            for (int i = mTopEntry; i < mEntries.Count; i++) {
                FormatEntry(mEntries[i], sb);
                sb.Append("\r\n");
            }
            for (int i = 0; i < mTopEntry; i++) {
                FormatEntry(mEntries[i], sb);
                sb.Append("\r\n");
            }
            return sb.ToString();
        }

        public override string ToString() {
            return "DebugLog has " + mEntries.Count + " entries";
        }
    }
}
