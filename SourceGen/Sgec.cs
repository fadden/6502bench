/*
 * Copyright 2020 faddenSoft
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
using System.IO;
using System.Text;

namespace SourceGen {
    /// <summary>
    /// SourceGen Edit Commands implementation.
    /// </summary>
    /// <remarks>
    /// This is an "experimental feature", meaning it's not in its final form and may be
    /// lacking in features or error checking.
    /// </remarks>
    public static class Sgec {
        public const string SGEC_EXT = "_sgec.txt";

        /// <summary>
        /// Exports comments in SGEC format.
        /// </summary>
        /// <param name="pathName">File to write to.</param>
        /// <param name="proj">Project object.</param>
        /// <param name="detailMsg">Details on failure or success.</param>
        /// <returns>True on success.</returns>
        public static bool ExportToFile(string pathName, DisasmProject proj, out string detailMsg) {
            int numComments = 0;

            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                for (int offset = 0; offset < proj.FileDataLength; offset++) {
                    if (!string.IsNullOrEmpty(proj.Comments[offset])) {
                        sw.WriteLine("set-comment +" + offset.ToString("x6") + ':' +
                            proj.Comments[offset]);
                        numComments++;
                    }
                }
            }
            detailMsg = "Exported " + numComments + " comments.";
            return true;
        }

        /// <summary>
        /// Import comments in SGEC format.
        /// </summary>
        /// <param name="pathName">File to read from.</param>
        /// <param name="proj">Project object.</param>
        /// <param name="cs">Change set that will hold changes.</param>
        /// <param name="detailMsg">Failure detail, or null on success.</param>
        /// <returns>True on success.</returns>
        public static bool ImportFromFile(string pathName, DisasmProject proj, ChangeSet cs,
                out string detailMsg) {
            string[] lines;
            try {
                lines = File.ReadAllLines(pathName);
            } catch (IOException ex) {
                // not expecting this to happen
                detailMsg = ex.Message;
                return false;
            }

            List<int> changed = new List<int>(lines.Length);

            string setComment = "set-comment +";
            foreach (string line in lines) {
                if (!line.StartsWith(setComment)) {
                    Debug.WriteLine("Ignoring " + line);
                    continue;
                }

                int offset;
                try {
                    offset = Convert.ToInt32(line.Substring(setComment.Length, 6), 16);
                } catch (Exception ex) {
                    Debug.WriteLine("Failed on " + line);
                    detailMsg = ex.Message;
                    return false;
                }

                if (changed.Contains(offset)) {
                    Debug.WriteLine("Skipping repeated entry +" + offset.ToString("X6"));
                    continue;
                }

                string oldComment = proj.Comments[offset];
                string newComment = line.Substring(setComment.Length + 7);
                if (oldComment == newComment) {
                    // no change
                    continue;
                }
                if (!string.IsNullOrEmpty(oldComment)) {
                    // overwriting existing entry
                    Debug.WriteLine("Replacing comment +" + offset.ToString("x6") +
                        " '" + oldComment + "'");
                }

                UndoableChange uc = UndoableChange.CreateCommentChange(offset,
                    oldComment, newComment);
                cs.Add(uc);
                changed.Add(offset);
            }

            detailMsg = "applied " + cs.Count + " changes.";
            return true;
        }
    }
}
