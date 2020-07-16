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
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

using CommonUtil;

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

        // Commands.
        private const string SET_COMMENT = "set-comment";
        private const string SET_LONG_COMMENT = "set-long-comment";
        private const string SET_NOTE = "set-note";

        // Regular expression used for parsing lines.
        private const string LINE_PATTERN = @"^([A-Za-z-]+)\s([^:]+):(.+)$";
        private static Regex sLineRegex = new Regex(LINE_PATTERN);
        private const int GROUP_CMD = 1;
        private const int GROUP_POS = 2;
        private const int GROUP_VALUE = 3;

        private static bool OUTPUT_ADDR = true;     // addr vs. offset

        // Lifted from ProjectFile.
        public class SerMultiLineComment {
            // NOTE: Text must be CRLF at line breaks.
            public string Text { get; set; }
            public bool BoxMode { get; set; }
            public int MaxWidth { get; set; }
            public int BackgroundColor { get; set; }

            public SerMultiLineComment() { }
            public SerMultiLineComment(MultiLineComment mlc) {
                Text = mlc.Text;
                BoxMode = mlc.BoxMode;
                MaxWidth = mlc.MaxWidth;
                BackgroundColor = CommonWPF.Helper.ColorToInt(mlc.BackgroundColor);
            }
        }

        /// <summary>
        /// Generates a position string, which may be a file offset, an address, or a
        /// position delta.
        /// </summary>
        private static string PositionStr(int offset, int prevOffset, AddressMap addrMap,
                bool relMode) {
            if (prevOffset < 0 || !relMode) {
                // hex offset or address
                if (OUTPUT_ADDR) {
                    return '$' + addrMap.OffsetToAddress(offset).ToString("x4");
                } else {
                    return '+' + offset.ToString("x6");
                }
            } else {
                // decimal delta
                return '>' + (offset - prevOffset).ToString();
            }
        }

        /// <summary>
        /// Exports comments in SGEC format.
        /// </summary>
        /// <param name="pathName">File to write to.</param>
        /// <param name="proj">Project object.</param>
        /// <param name="detailMsg">Details on failure or success.</param>
        /// <returns>True on success.</returns>
        public static bool ExportToFile(string pathName, DisasmProject proj, bool relMode,
                out string detailMsg) {
            int numItems = 0;

            JavaScriptSerializer ser = new JavaScriptSerializer();

            int prevOffset = -1;
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                for (int offset = 0; offset < proj.FileDataLength; offset++) {
                    if (!string.IsNullOrEmpty(proj.Comments[offset])) {
                        sw.WriteLine(SET_COMMENT + " " +
                            PositionStr(offset, prevOffset, proj.AddrMap, relMode) + ':' +
                            proj.Comments[offset]);
                        prevOffset = offset;
                        numItems++;
                    }
                    if (proj.LongComments.TryGetValue(offset, out MultiLineComment lc)) {
                        SerMultiLineComment serCom = new SerMultiLineComment(lc);
                        string cereal = ser.Serialize(serCom);
                        sw.WriteLine(SET_LONG_COMMENT + " " +
                            PositionStr(offset, prevOffset, proj.AddrMap, relMode) + ':' +
                            cereal);
                        prevOffset = offset;
                        numItems++;
                    }
                    if (proj.Notes.TryGetValue(offset, out MultiLineComment nt)) {
                        SerMultiLineComment serCom = new SerMultiLineComment(nt);
                        string cereal = ser.Serialize(serCom);
                        sw.WriteLine(SET_NOTE + " " +
                            PositionStr(offset, prevOffset, proj.AddrMap, relMode) + ':' +
                            cereal);
                        prevOffset = offset;
                        numItems++;
                    }
                }
            }
            detailMsg = "exported " + numItems + " items.";
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

            JavaScriptSerializer ser = new JavaScriptSerializer();

            int lineNum = 0;
            int prevOffset = -1;
            foreach (string line in lines) {
                lineNum++;      // first line is 1
                if (string.IsNullOrEmpty(line) || line[0] == '#') {
                    // ignore
                    continue;
                }
                MatchCollection matches = sLineRegex.Matches(line);
                if (matches.Count != 1) {
                    detailMsg = "Line " + lineNum + ": unable to parse into tokens";
                    return false;
                }

                string posStr = matches[0].Groups[GROUP_POS].Value;
                int offset;
                if (posStr[0] == '+') {
                    // offset
                    if (!Asm65.Number.TryParseIntHex(posStr.Substring(1), out offset)) {
                        detailMsg = "Line " + lineNum + ": unable to parse offset '" +
                            posStr + "'";
                        return false;
                    }
                } else if (posStr[0] == '$') {
                    // address
                    if (!Asm65.Address.ParseAddress(posStr, (1 << 24) - 1, out int addr)) {
                        detailMsg = "Line " + lineNum + ": unable to parse address '" +
                            posStr + "'";
                        return false;
                    }
                    offset = proj.AddrMap.AddressToOffset(0, addr);
                } else if (posStr[0] == '>') {
                    // relative offset
                    if (prevOffset < 0) {
                        detailMsg = "Line " + lineNum + ": first address/offset cannot be relative";
                        return false;
                    }
                    if (!Asm65.Number.TryParseInt(posStr.Substring(1), out int delta, out int _)) {
                        detailMsg = "Line " + lineNum + ": unable to parse delta";
                        return false;
                    }
                    offset = prevOffset + delta;
                } else {
                    detailMsg = "Line " + lineNum + ": unknown position type '" + posStr[0] + "'";
                    return false;
                }

                prevOffset = offset;

                string cmdStr = matches[0].Groups[GROUP_CMD].Value;
                string valueStr = matches[0].Groups[GROUP_VALUE].Value;
                switch (cmdStr) {
                    case SET_COMMENT: {
                            string oldComment = proj.Comments[offset];
                            string newComment = valueStr;
                            if (oldComment == newComment) {
                                // no change
                                break;
                            }
                            if (!string.IsNullOrEmpty(oldComment)) {
                                // overwriting existing entry; make a note
                                Debug.WriteLine("Replacing comment +" + offset.ToString("x6") +
                                    " '" + oldComment + "'");
                            }
                            UndoableChange uc = UndoableChange.CreateCommentChange(offset,
                                oldComment, newComment);
                            cs.Add(uc);
                        }
                        break;
                    case SET_LONG_COMMENT: {
                            if (!DeserializeMlc(ser, valueStr, false,
                                    out MultiLineComment newComment)) {
                                detailMsg = "Line " + lineNum + ": failed to deserialize value";
                                return false;
                            }
                            proj.LongComments.TryGetValue(offset, out MultiLineComment oldComment);
                            if (oldComment == newComment) {
                                // no change
                                break;
                            }
                            UndoableChange uc = UndoableChange.CreateLongCommentChange(offset,
                                oldComment, newComment);
                            cs.Add(uc);
                        }
                        break;
                    case SET_NOTE: {
                            if (!DeserializeMlc(ser, valueStr, true,
                                    out MultiLineComment newNote)) {
                                detailMsg = "Line " + lineNum + ": failed to deserialize value";
                                return false;
                            }
                            proj.Notes.TryGetValue(offset, out MultiLineComment oldNote);
                            if (oldNote == newNote) {
                                // no change
                                break;
                            }
                            UndoableChange uc = UndoableChange.CreateNoteChange(offset,
                                oldNote, newNote);
                            cs.Add(uc);

                        }
                        break;
                    default:
                        detailMsg = "Line " + lineNum + ": unknown command '" + cmdStr + "'";
                        return false;
                }

            }

            detailMsg = "applied " + cs.Count + " changes.";
            return true;
        }

        private static bool DeserializeMlc(JavaScriptSerializer ser, string cereal, bool isNote,
                out MultiLineComment mlc) {
            mlc = null;
            SerMultiLineComment smlc;
            try {
                smlc = ser.Deserialize<SerMultiLineComment>(cereal);
            } catch (Exception ex) {
                Debug.WriteLine("Deserialization failed: " + ex.Message);
                return false;
            }

            if (isNote) {
                mlc = new MultiLineComment(smlc.Text,
                    CommonWPF.Helper.ColorFromInt(smlc.BackgroundColor));
            } else {
                mlc = new MultiLineComment(smlc.Text, smlc.BoxMode, smlc.MaxWidth);
            }
            return true;
        }
    }
}
