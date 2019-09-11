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
using System.Text;

using Asm65;
using CommonUtil;
using ClipLineFormat = SourceGen.MainController.ClipLineFormat;

namespace SourceGen {
    public class Exporter {
        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// List of formatted parts.
        /// </summary>
        private LineListGen mCodeLineList;

        /// <summary>
        /// Text formatter.
        /// </summary>
        private Formatter mFormatter;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Exporter(DisasmProject project, LineListGen codeLineList, Formatter formatter) {
            mProject = project;
            mCodeLineList = codeLineList;
            mFormatter = formatter;
        }

        /// <summary>
        /// Converts the selected lines to formatted text.
        /// </summary>
        /// <param name="selection">Set of lines to select.</param>
        /// <param name="lineFormat">Line format.</param>
        /// <param name="fullText">Result; holds text of all selected lines.</param>
        /// <param name="csvText">Result; holds text of all selected lines, in CSV format.</param>
        public void SelectionToText(DisplayListSelection selection, ClipLineFormat lineFormat,
                bool addCsv, out string fullText, out string csvText) {

            StringBuilder plainText = new StringBuilder(selection.Count * 50);
            StringBuilder sb = new StringBuilder(100);
            StringBuilder csv = null;
            if (addCsv) {
                csv = new StringBuilder(selection.Count * 40);
            }

            int addrAdj = mProject.CpuDef.HasAddr16 ? 6 : 9;
            int disAdj = 0;
            int bytesWidth = 0;
            if (lineFormat == MainController.ClipLineFormat.Disassembly) {
                // A limit of 8 gets us 4 bytes from dense display ("20edfd60") and 3 if spaces
                // are included ("20 ed fd") with no excess.  We want to increase it to 11 so
                // we can always show 4 bytes.
                bytesWidth = (mFormatter.Config.mSpacesBetweenBytes ? 11 : 8);
                disAdj = addrAdj + bytesWidth + 2;
            }

            // Walking through the selected indices can be slow for a large file, so we
            // run through the full list and pick out the selected items with our parallel
            // structure.  (I'm assuming that "select all" will be a common precursor.)
            foreach (int index in selection) {
                LineListGen.Line line = mCodeLineList[index];
                DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);
                switch (line.LineType) {
                    case LineListGen.Line.Type.Code:
                    case LineListGen.Line.Type.Data:
                    case LineListGen.Line.Type.EquDirective:
                    case LineListGen.Line.Type.RegWidthDirective:
                    case LineListGen.Line.Type.OrgDirective:
                    case LineListGen.Line.Type.LocalVariableTable:
                        if (lineFormat == ClipLineFormat.Disassembly) {
                            if (!string.IsNullOrEmpty(parts.Addr)) {
                                sb.Append(parts.Addr);
                                sb.Append(": ");
                            }

                            // Shorten the "...".
                            string bytesStr = parts.Bytes;
                            if (bytesStr != null && bytesStr.Length > bytesWidth) {
                                bytesStr = bytesStr.Substring(0, bytesWidth) + "+";
                            }
                            TextUtil.AppendPaddedString(sb, bytesStr, disAdj);
                        }
                        TextUtil.AppendPaddedString(sb, parts.Label, disAdj + 9);
                        TextUtil.AppendPaddedString(sb, parts.Opcode, disAdj + 9 + 8);
                        TextUtil.AppendPaddedString(sb, parts.Operand, disAdj + 9 + 8 + 11);
                        if (string.IsNullOrEmpty(parts.Comment)) {
                            // Trim trailing spaces off opcode or operand.
                            TextUtil.TrimEnd(sb);
                        } else {
                            sb.Append(parts.Comment);
                        }
                        sb.Append("\r\n");
                        break;
                    case LineListGen.Line.Type.LongComment:
                        if (lineFormat == ClipLineFormat.Disassembly) {
                            TextUtil.AppendPaddedString(sb, string.Empty, disAdj);
                        }
                        sb.Append(parts.Comment);
                        sb.Append("\r\n");
                        break;
                    case LineListGen.Line.Type.Note:
                        // don't include notes
                        break;
                    case LineListGen.Line.Type.Blank:
                        sb.Append("\r\n");
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }
                plainText.Append(sb);
                sb.Clear();

                if (addCsv) {
                    csv.Append(TextUtil.EscapeCSV(parts.Offset)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Addr)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Bytes)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Flags)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Attr)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Label)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Opcode)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Operand)); csv.Append(',');
                    csv.Append(TextUtil.EscapeCSV(parts.Comment));
                    csv.Append("\r\n");
                }
            }

            fullText = plainText.ToString();
            csvText = csv.ToString();
        }
    }
}
