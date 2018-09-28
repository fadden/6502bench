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
using System.Text;

namespace CommonUtil {
    /// <summary>
    /// File load item, identifying the location, severity, and details of the issue.
    /// </summary>
    public class FileLoadItem {
        public const int NO_LINE = -1;
        public const int NO_COLUMN = -1;

        public enum Type {
            Unknown = 0,
            Notice,
            Warning,
            Error
        }

        public int Line { get; private set; }
        public int Column { get; private set; }
        public Type MsgType { get; private set; }
        public string Message { get; private set; }

        public FileLoadItem(int line, int col, Type msgType, string msg) {
            Line = line;
            Column = col;
            MsgType = msgType; ;
            Message = msg;
        }
    }

    /// <summary>
    /// A structured collection of errors and warnings generated when reading data from a file.
    /// </summary>
    public class FileLoadReport : IEnumerable<FileLoadItem> {
        // List of items.  Currently unsorted; items will appear in the order they were added.
        private List<FileLoadItem> mItems = new List<FileLoadItem>();

        public string FileName { get; private set; }
        public bool HasWarnings { get; private set; }
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="fileName">Name of file being loaded.  This is just for output; this
        ///    class doesn't actually try to access the file.</param>
        public FileLoadReport(string fileName) {
            FileName = fileName;
        }

        public IEnumerator<FileLoadItem> GetEnumerator() {
            return mItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return mItems.GetEnumerator();
        }

        public int Count { get { return mItems.Count; } }

        /// <summary>
        /// Adds a new message to the report.
        /// </summary>
        /// <param name="isWarn">Is this a warning or an error?</param>
        /// <param name="msg">Human-readable message.</param>
        public void Add(FileLoadItem.Type msgType, string msg) {
            Add(FileLoadItem.NO_LINE, FileLoadItem.NO_COLUMN, msgType, msg);
        }

        /// <summary>
        /// Adds a new message to the report.
        /// </summary>
        /// <param name="line">Line where the issue was seen.</param>
        /// <param name="col">Column where the problem starts, or NO_COLUMN.</param>
        /// <param name="isWarn">Is this a warning or an error?</param>
        /// <param name="msg">Human-readable message.</param>
        public void Add(int line, int col, FileLoadItem.Type msgType, string msg) {
            mItems.Add(new FileLoadItem(line, col, msgType, msg));
            switch (msgType) {
                case FileLoadItem.Type.Warning:
                    HasWarnings = true;
                    break;
                case FileLoadItem.Type.Error:
                    HasErrors = true;
                    break;
            }
        }

        /// <summary>
        /// Formats the entire collection into a single multi-line string.
        /// </summary>
        /// <returns>Formatted string.</returns>
        public string Format() {
            StringBuilder sb = new StringBuilder();
            if (mItems.Count > 0) {
                sb.AppendFormat("File {0}:\r\n", FileName);
            }
            foreach (FileLoadItem item in mItems) {
                if (item.Line != FileLoadItem.NO_LINE && item.Column != FileLoadItem.NO_COLUMN) {
                    sb.AppendFormat("  Line {0}.{1}: {2}: {3}\r\n", item.Line, item.Column,
                        item.MsgType.ToString().ToLower(), item.Message);
                } else if (item.Line != FileLoadItem.NO_LINE) {
                    sb.AppendFormat("  Line {0}: {1}: {2}\r\n", item.Line,
                        item.MsgType.ToString().ToLower(), item.Message);
                } else {
                    // Capitalized form looks nicer here.
                    sb.AppendFormat("  {0}: {1}\r\n", item.MsgType.ToString(), item.Message);
                }
            }
            return sb.ToString();
        }

        public override string ToString() {
            return "FileLoadReport: count=" + mItems.Count + " hasWarn=" +
                HasWarnings + " hasErr=" + HasErrors;
        }
    }
}
