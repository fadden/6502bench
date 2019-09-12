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
using System.Diagnostics;
using System.IO;
using System.Text;

using Asm65;
using CommonUtil;

namespace SourceGen {
    /// <summary>
    /// Source code export functions.
    /// </summary>
    /// <remarks>
    /// The five columns on the left (offset, address, bytes, flags, attributes) are optional,
    /// and have a fixed width.  The four columns on the right (label, opcode, operand, comment)
    /// are mandatory, and have configurable widths.
    /// </remarks>
    public class Exporter {
        /// <summary>
        /// Optional selection specifier.  If null, the entire file is included.
        /// </summary>
        public DisplayListSelection Selection { get; set; }

        /// <summary>
        /// Should notes be included in the output?
        /// </summary>
        public bool IncludeNotes { get; set; }

        /// <summary>
        /// Bit flags, used to indicate which of the optional columns are active.
        /// </summary>
        [FlagsAttribute]
        public enum ActiveColumnFlags {
            None        = 0,
            Offset      = 1,
            Address     = 1 << 1,
            Bytes       = 1 << 2,
            Flags       = 1 << 3,
            Attr        = 1 << 4,

            ALL         = 0x7f
        }

        /// <summary>
        /// Flags indicating active optional columns.
        /// </summary>
        private ActiveColumnFlags mLeftFlags;

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
        /// The cumulative width of the columns determines the end point.
        /// </summary>
        private int[] mColEnd;

        private enum Col {
            Offset = 0,
            Address = 1,
            Bytes = 2,
            Flags = 3,
            Attr = 4,
            Label = 5,
            Opcode = 6,
            Operand = 7,
            Comment = 8,
            COUNT           // number of elements, must be last
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Exporter(DisasmProject project, LineListGen codeLineList, Formatter formatter,
                ActiveColumnFlags leftFlags, int[] rightWidths) {
            mProject = project;
            mCodeLineList = codeLineList;
            mFormatter = formatter;
            mLeftFlags = leftFlags;

            ConfigureColumns(leftFlags, rightWidths);
        }

        private void ConfigureColumns(ActiveColumnFlags leftFlags, int[] rightWidths) {
            mColEnd = new int[(int)Col.COUNT];
            int total = 0;
            int width;

            // offset "+123456"
            if ((leftFlags & ActiveColumnFlags.Offset) != 0) {
                total = mColEnd[(int)Col.Offset] = total + 7 + 1;
            } else {
                mColEnd[(int)Col.Offset] = total;
            }

            // address "1234:" or "12/4567:"
            if ((leftFlags & ActiveColumnFlags.Address) != 0) {
                width = mProject.CpuDef.HasAddr16 ? 5 : 8;
                total = mColEnd[(int)Col.Address] = total + width + 1;
            } else {
                mColEnd[(int)Col.Address] = total;
            }

            // bytes "12345678+" or "12 45 78 01+"
            if ((leftFlags & ActiveColumnFlags.Bytes) != 0) {
                // A limit of 8 gets us 4 bytes from dense display ("20edfd60") and 3 if spaces
                // are included ("20 ed fd") with no excess.  We want to increase it to 11 so
                // we can always show 4 bytes.  Add one for a trailing "+".
                width = mFormatter.Config.mSpacesBetweenBytes ? 12 : 9;
                total = mColEnd[(int)Col.Bytes] = total + width + 1;
            } else {
                mColEnd[(int)Col.Bytes] = total;
            }

            // flags "NVMXDIZC" or "NVMXDIZC E"
            if ((leftFlags & ActiveColumnFlags.Flags) != 0) {
                width = mProject.CpuDef.HasEmuFlag ? 10 : 8;
                total = mColEnd[(int)Col.Flags] = total + width + 1;
            } else {
                mColEnd[(int)Col.Flags] = total;
            }

            // attributes "@H!#>"
            if ((leftFlags & ActiveColumnFlags.Attr) != 0) {
                total = mColEnd[(int)Col.Attr] = total + 5 + 1;
            } else {
                mColEnd[(int)Col.Attr] = total;
            }

            total = mColEnd[(int)Col.Label] = total + rightWidths[0];
            total = mColEnd[(int)Col.Opcode] = total + rightWidths[1];
            total = mColEnd[(int)Col.Operand] = total + rightWidths[2];
            total = mColEnd[(int)Col.Comment] = total + rightWidths[3];

            //Debug.WriteLine("Export col ends:");
            //for (int i = 0; i < (int)Col.COUNT; i++) {
            //    Debug.WriteLine("  " + i + "(" + ((Col)i) + ") " + mColEnd[i]);
            //}
        }

        /// <summary>
        /// Converts the selected lines to formatted text.
        /// </summary>
        /// <param name="fullText">Result; holds text of all selected lines.</param>
        /// <param name="csvText">Result; holds text of all selected lines, in CSV format.</param>
        public void SelectionToString(bool addCsv, out string fullText, out string csvText) {
            StringBuilder sb = new StringBuilder(128);
            StringBuilder plainText = new StringBuilder(Selection.Count * 50);
            StringBuilder csv = null;
            if (addCsv) {
                csv = new StringBuilder(Selection.Count * 40);
            }

            for (int lineIndex = 0; lineIndex < mCodeLineList.Count; lineIndex++) {
                if (!Selection[lineIndex]) {
                    continue;
                }
                if (GenerateTextLine(lineIndex, sb)) {
                    plainText.Append(sb.ToString());
                    plainText.Append("\r\n");
                }
                sb.Clear();
                if (addCsv) {
                    GenerateCsvLine(lineIndex, sb);
                    csv.Append(sb.ToString());
                    csv.Append("\r\n");
                    sb.Clear();
                }
            }

            fullText = plainText.ToString();
            if (addCsv) {
                csvText = csv.ToString();
            } else {
                csvText = null;
            }
        }

        /// <summary>
        /// Generates a full listing and writes it to the specified file.
        /// </summary>
        /// <param name="pathName">Full path to output file.</param>
        /// <param name="asCsv">Output as Comma Separated Values rather than plain text.</param>
        public void OutputToText(string pathName, bool asCsv) {
            // Generate UTF-8 text.  For plain text we omit the byte-order mark, for CSV
            // it appears to be meaningful (tested w/ very old version of Excel).
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(asCsv))) {
                StringBuilder sb = new StringBuilder(128);
                for (int lineIndex = 0; lineIndex < mCodeLineList.Count; lineIndex++) {
                    if (Selection != null && !Selection[lineIndex]) {
                        continue;
                    }

                    if (asCsv) {
                        GenerateCsvLine(lineIndex, sb);
                        sw.WriteLine(sb.ToString());
                    } else {
                        if (GenerateTextLine(lineIndex, sb)) {
                            sw.WriteLine(sb.ToString());
                        }
                    }
                    sb.Clear();
                }
            }
        }

        /// <summary>
        /// Generates a line of plain text output.  The line will not have EOL markers added.
        /// </summary>
        /// <param name="index">Index of line to output.</param>
        /// <param name="sb">String builder to append text to.  Must be cleared before
        ///   calling here.  (This is a minor optimization.)</param>
        private bool GenerateTextLine(int index, StringBuilder sb) {
            Debug.Assert(sb.Length == 0);

            // Width of "bytes" field, without '+' or trailing space.
            int bytesWidth = mColEnd[(int)Col.Bytes] - mColEnd[(int)Col.Address] - 2;

            LineListGen.Line line = mCodeLineList[index];
            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);
            switch (line.LineType) {
                case LineListGen.Line.Type.Code:
                case LineListGen.Line.Type.Data:
                case LineListGen.Line.Type.EquDirective:
                case LineListGen.Line.Type.RegWidthDirective:
                case LineListGen.Line.Type.OrgDirective:
                case LineListGen.Line.Type.LocalVariableTable:
                    if (parts.IsLongComment) {
                        // This happens for long comments embedded in LV tables.
                        if (mColEnd[(int)Col.Attr] != 0) {
                            TextUtil.AppendPaddedString(sb, string.Empty, mColEnd[(int)Col.Attr]);
                        }
                        sb.Append(parts.Comment);
                        break;
                    }

                    if ((mLeftFlags & ActiveColumnFlags.Offset) != 0) {
                        TextUtil.AppendPaddedString(sb, parts.Offset,
                            mColEnd[(int)Col.Offset]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Address) != 0) {
                        string str;
                        if (!string.IsNullOrEmpty(parts.Addr)) {
                            str = parts.Addr + ":";
                        } else {
                            str = string.Empty;
                        }
                        TextUtil.AppendPaddedString(sb, str, mColEnd[(int)Col.Address]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Bytes) != 0) {
                        // Shorten the "...".
                        string bytesStr = parts.Bytes;
                        if (bytesStr != null && bytesStr.Length > bytesWidth) {
                            bytesStr = bytesStr.Substring(0, bytesWidth) + "+";
                        }
                        TextUtil.AppendPaddedString(sb, bytesStr, mColEnd[(int)Col.Bytes]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Flags) != 0) {
                        TextUtil.AppendPaddedString(sb, parts.Flags, mColEnd[(int)Col.Flags]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Attr) != 0) {
                        TextUtil.AppendPaddedString(sb, parts.Attr, mColEnd[(int)Col.Attr]);
                    }
                    TextUtil.AppendPaddedString(sb, parts.Label, mColEnd[(int)Col.Label]);
                    TextUtil.AppendPaddedString(sb, parts.Opcode, mColEnd[(int)Col.Opcode]);
                    TextUtil.AppendPaddedString(sb, parts.Operand, mColEnd[(int)Col.Operand]);
                    if (string.IsNullOrEmpty(parts.Comment)) {
                        // Trim trailing spaces off opcode or operand.  Would be more efficient
                        // to just not generate the spaces, but this is simpler and we're not
                        // in a hurry.
                        TextUtil.TrimEnd(sb);
                    } else {
                        sb.Append(parts.Comment);
                    }
                    break;
                case LineListGen.Line.Type.LongComment:
                case LineListGen.Line.Type.Note:
                    if (line.LineType == LineListGen.Line.Type.Note && !IncludeNotes) {
                        return false;
                    }
                    if (mColEnd[(int)Col.Attr] != 0) {
                        // Long comments aren't the left-most field, so pad it out.
                        TextUtil.AppendPaddedString(sb, string.Empty, mColEnd[(int)Col.Attr]);
                    }
                    sb.Append(parts.Comment);
                    break;
                case LineListGen.Line.Type.Blank:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            return true;
        }

        private void GenerateCsvLine(int index, StringBuilder sb) {
            LineListGen.Line line = mCodeLineList[index];
            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);

            if ((mLeftFlags & ActiveColumnFlags.Offset) != 0) {
                sb.Append(TextUtil.EscapeCSV(parts.Offset)); sb.Append(',');
            }
            if ((mLeftFlags & ActiveColumnFlags.Address) != 0) {
                sb.Append(TextUtil.EscapeCSV(parts.Addr)); sb.Append(',');
            }
            if ((mLeftFlags & ActiveColumnFlags.Bytes) != 0) {
                sb.Append(TextUtil.EscapeCSV(parts.Bytes)); sb.Append(',');
            }
            if ((mLeftFlags & ActiveColumnFlags.Flags) != 0) {
                sb.Append(TextUtil.EscapeCSV(parts.Flags)); sb.Append(',');
            }
            if ((mLeftFlags & ActiveColumnFlags.Attr) != 0) {
                sb.Append(TextUtil.EscapeCSV(parts.Attr)); sb.Append(',');
            }
            if (parts.IsLongComment) {
                // put the comment in the Label column
                sb.Append(TextUtil.EscapeCSV(parts.Comment)); sb.Append(",,,");
            } else {
                sb.Append(TextUtil.EscapeCSV(parts.Label)); sb.Append(',');
                sb.Append(TextUtil.EscapeCSV(parts.Opcode)); sb.Append(',');
                sb.Append(TextUtil.EscapeCSV(parts.Operand)); sb.Append(',');
                sb.Append(TextUtil.EscapeCSV(parts.Comment));
            }
        }

        public void OutputToHtml(string pathName, bool overwriteCss) {
            Debug.WriteLine("HTML");    // TODO
        }
    }
}
