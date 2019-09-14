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
using System.Windows;
using System.Windows.Media;

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

        #region HTML

        private const string HTML_EXPORT_TEMPLATE = "ExportTemplate.html";
        private const string HTML_EXPORT_CSS_FILE = "SGStyle.css";
        private const string LABEL_LINK_PREFIX = "Sym";

        public void OutputToHtml(string pathName, bool overwriteCss) {
            string exportTemplate = RuntimeDataAccess.GetPathName(HTML_EXPORT_TEMPLATE);
            string tmplStr;
            try {
                // exportTemplate will be null if Runtime access failed
                tmplStr = File.ReadAllText(exportTemplate);
            } catch (Exception ex) {
                string msg = string.Format(Res.Strings.ERR_FILE_READ_FAILED_FMT,
                    pathName, ex.Message);
                MessageBox.Show(msg, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Perform some quick substitutions.
            tmplStr = tmplStr.Replace("$ProjectName$", mProject.DataFileName);
            tmplStr = tmplStr.Replace("$AppVersion$", App.ProgramVersion.ToString());

            // Generate and substitute the symbol table.  This should be small enough that
            // we won't break the world by doing it with string.Replace().
            string symTabStr = GenerateHtmlSymbolTable();
            tmplStr = tmplStr.Replace("$SymbolTable$", symTabStr);

            // For the main event we split the template in half, and generate the code lines
            // directly into the stream writer.
            const string CodeLinesStr = "$CodeLines$";
            int splitPoint = tmplStr.IndexOf(CodeLinesStr);
            if (splitPoint < 0) {
                Debug.WriteLine("No place to put code");
                return;
            }
            string template1 = tmplStr.Substring(0, splitPoint);
            string template2 = tmplStr.Substring(splitPoint + CodeLinesStr.Length);


            // Generate UTF-8 text, without a byte-order mark.
            using (StreamWriter sw = new StreamWriter(pathName, false, new UTF8Encoding(false))) {
                sw.Write(template1);

                // With the style "code { white-space: pre; }", leading spaces and EOL markers
                // are preserved.
                sw.Write("<code style=\"white-space: pre;\">");
                StringBuilder sb = new StringBuilder(128);
                for (int lineIndex = 0; lineIndex < mCodeLineList.Count; lineIndex++) {
                    if (Selection != null && !Selection[lineIndex]) {
                        continue;
                    }

                    if (GenerateHtmlLine(lineIndex, sb)) {
                        sw.WriteLine(sb.ToString());
                        //sw.WriteLine("<br/>");
                    }
                    sb.Clear();
                }
                sw.WriteLine("</code>\r\n");

                sw.Write(template2);
            }

            string cssFile = RuntimeDataAccess.GetPathName(HTML_EXPORT_CSS_FILE);
            string outputDir = Path.GetDirectoryName(pathName);
            string outputPath = Path.Combine(outputDir, HTML_EXPORT_CSS_FILE);
            if (File.Exists(cssFile) && (overwriteCss || !File.Exists(outputPath))) {
                Debug.WriteLine("Copying '" + cssFile + "' -> '" + outputPath + "'");
                try {
                    File.Copy(cssFile, outputPath, true);
                } catch (Exception ex) {
                    string msg = string.Format(Res.Strings.ERR_FILE_COPY_FAILED_FMT,
                        cssFile, outputPath, ex.Message);
                    MessageBox.Show(msg, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }
        /// <summary>
        /// Generates a line of HTML output.  The line will not have EOL markers added.
        /// </summary>
        /// <remarks>
        /// Currently just generating a line of pre-formatted text.  We could also output
        /// every line as a table row, with HTML column definitions for each of our columns.
        /// </remarks>
        /// <param name="index">Index of line to output.</param>
        /// <param name="sb">String builder to append text to.  Must be cleared before
        ///   calling here.  (This is a minor optimization.)</param>
        private bool GenerateHtmlLine(int index, StringBuilder sb) {
            Debug.Assert(sb.Length == 0);

            // Width of "bytes" field, without '+' or trailing space.
            int bytesWidth = mColEnd[(int)Col.Bytes] - mColEnd[(int)Col.Address] - 2;

            LineListGen.Line line = mCodeLineList[index];
            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);

            // TODO: linkify label and operand fields

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
                    int labelOffset = sb.Length;
                    TextUtil.AppendPaddedString(sb, parts.Label, mColEnd[(int)Col.Label]);
                    TextUtil.AppendPaddedString(sb, parts.Opcode, mColEnd[(int)Col.Opcode]);
                    int operandOffset = sb.Length;
                    TextUtil.AppendPaddedString(sb, parts.Operand, mColEnd[(int)Col.Operand]);
                    if (string.IsNullOrEmpty(parts.Comment)) {
                        // Trim trailing spaces off opcode or operand.  Would be more efficient
                        // to just not generate the spaces, but this is simpler and we're not
                        // in a hurry.
                        TextUtil.TrimEnd(sb);
                    } else {
                        sb.Append(parts.Comment);
                    }

                    // Replace label with anchor label.  We do it this late because we need the
                    // spacing to be properly set, and I don't feel like changing how all the
                    // AppendPaddedString code works.
                    if ((line.LineType == LineListGen.Line.Type.Code ||
                            line.LineType == LineListGen.Line.Type.Data ||
                            line.LineType == LineListGen.Line.Type.EquDirective) &&
                            !string.IsNullOrEmpty(parts.Label)) {
                        string linkLabel = "<a name=\"" + LABEL_LINK_PREFIX + parts.Label +
                            "\">" + parts.Label + "</a>";
                        sb.Remove(labelOffset, parts.Label.Length);
                        sb.Insert(labelOffset, linkLabel);

                        // Adjust operand position.
                        operandOffset += linkLabel.Length - parts.Label.Length;
                    }

                    if ((line.LineType == LineListGen.Line.Type.Code ||
                            line.LineType == LineListGen.Line.Type.Data) &&
                            parts.Operand.Length > 0) {
                        string linkOperand = GetLinkOperand(index, parts.Operand);
                        if (!string.IsNullOrEmpty(linkOperand)) {
                            sb.Remove(operandOffset, parts.Operand.Length);
                            sb.Insert(operandOffset, linkOperand);
                        }
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

                    // Notes have a background color.  Use this to highlight the text.  We
                    // don't apply it to the padding on the left columns.
                    int rgb = 0;
                    if (parts.HasBackgroundColor) {
                        SolidColorBrush b = parts.BackgroundBrush as SolidColorBrush;
                        if (b != null) {
                            rgb = (b.Color.R << 16) | (b.Color.G << 8) | (b.Color.B);
                        }
                    }
                    if (rgb != 0) {
                        sb.AppendFormat("<span style=\"background-color: #{0:x6}\">", rgb);
                        sb.Append(parts.Comment);
                        sb.Append("</span>");
                    } else {
                        sb.Append(parts.Comment);
                    }
                    break;
                case LineListGen.Line.Type.Blank:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
            return true;
        }

        /// <summary>
        /// Wraps the symbolic part of the operand with HTML link notation.  If the operand
        /// doesn't have a linkable symbol, this return null.
        /// </summary>
        /// <remarks>
        /// We're playing games with string substitution that feel a little flimsy, but this
        /// is much simpler than reformatting the operand from scratch.
        /// </remarks>
        /// <param name="index">Display line index.</param>
        private string GetLinkOperand(int index, string operand) {
            LineListGen.Line line = mCodeLineList[index];
            if (line.FileOffset < 0) {
                // EQU directive - shouldn't be here
                Debug.Assert(false);
                return null;
            }

            // Check for a format descriptor with a symbol.
            Debug.Assert(line.LineType == LineListGen.Line.Type.Code ||
                line.LineType == LineListGen.Line.Type.Data);
            Anattrib attr = mProject.GetAnattrib(line.FileOffset);
            if (attr.DataDescriptor == null || !attr.DataDescriptor.HasSymbol) {
                return null;
            }

            // Symbol refs are weak.  If the symbol doesn't exist, the value will be
            // formatted in hex.  We can't simply check to see if the formatted operand
            // contains the symbol, because we could false-positive on the use of symbols
            // whose label is a valid hex value, e.g. "ABCD = $ABCD".
            //
            // We also want to exclude references to local variables, since those aren't
            // unique.  To handle local refs we could just create anchors by line number or
            // some other means of unique identification.
            if (!mProject.SymbolTable.TryGetNonVariableValue(attr.DataDescriptor.SymbolRef.Label,
                    out Symbol sym)) {
                return null;
            }

            string linkified = "<a href=#" + LABEL_LINK_PREFIX + sym.Label + ">" +
                sym.Label + "</a>";
            return operand.Replace(sym.Label, linkified);
        }

        /// <summary>
        /// Generates a table of global/exported symbols.  If none exist, a "no symbols found"
        /// message is generated instead.
        /// </summary>
        private string GenerateHtmlSymbolTable() {
            StringBuilder sb = new StringBuilder();
            int count = 0;

            foreach (Symbol sym in mProject.SymbolTable) {
                if (sym.SymbolType != Symbol.Type.GlobalAddrExport) {
                    continue;
                }
                if (count == 0) {
                    sb.Append("<table>\r\n");
                }
                sb.Append("  <tr>");
                sb.Append("<td><a href=#" + LABEL_LINK_PREFIX + sym.Label + ">" +
                    sym.Label + "</a></td>");
                sb.Append("<td>" + mFormatter.FormatHexValue(sym.Value, 2) + "</td>");
                sb.Append("</tr>\r\n");
                count++;
            }

            if (count == 0) {
                sb.AppendFormat("<p>{0}</p>\r\n", Res.Strings.NO_EXPORTED_SYMBOLS_FOUND);
            } else {
                sb.Append("</table>\r\n");
            }

            return sb.ToString();
        }

        #endregion HTML
    }
}
