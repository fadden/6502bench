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
        /// The cumulative width of the columns determines the start point.
        /// </summary>
        private int[] mColStart;

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
        /// If set, labels that are wider than the label column should go on their own line.
        /// </summary>
        private bool mLongLabelNewLine;


        /// <summary>
        /// Constructor.
        /// </summary>
        public Exporter(DisasmProject project, LineListGen codeLineList, Formatter formatter,
                ActiveColumnFlags leftFlags, int[] rightWidths) {
            mProject = project;
            mCodeLineList = codeLineList;
            mFormatter = formatter;
            mLeftFlags = leftFlags;

            // Go ahead and latch this here.
            mLongLabelNewLine =
                AppSettings.Global.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);

            ConfigureColumns(leftFlags, rightWidths);
        }

        private void ConfigureColumns(ActiveColumnFlags leftFlags, int[] rightWidths) {
            mColStart = new int[(int)Col.COUNT];
            int total = 0;
            int width;

            // mColStart[(int)Col.Offset] = 0

            // offset "+123456"
            if ((leftFlags & ActiveColumnFlags.Offset) != 0) {
                total = mColStart[(int)Col.Offset + 1] = total + 7 + 1;
            } else {
                mColStart[(int)Col.Offset + 1] = total;
            }

            // address "1234:" or "12/4567:"
            if ((leftFlags & ActiveColumnFlags.Address) != 0) {
                width = mProject.CpuDef.HasAddr16 ? 5 : 8;
                total = mColStart[(int)Col.Address + 1] = total + width + 1;
            } else {
                mColStart[(int)Col.Address + 1] = total;
            }

            // bytes "12345678+" or "12 45 78 01+"
            if ((leftFlags & ActiveColumnFlags.Bytes) != 0) {
                // A limit of 8 gets us 4 bytes from dense display ("20edfd60") and 3 if spaces
                // are included ("20 ed fd") with no excess.  We want to increase it to 11 so
                // we can always show 4 bytes.  Add one for a trailing "+".
                width = mFormatter.Config.mSpacesBetweenBytes ? 12 : 9;
                total = mColStart[(int)Col.Bytes + 1] = total + width + 1;
            } else {
                mColStart[(int)Col.Bytes + 1] = total;
            }

            // flags "NVMXDIZC" or "NVMXDIZC E"
            if ((leftFlags & ActiveColumnFlags.Flags) != 0) {
                width = mProject.CpuDef.HasEmuFlag ? 10 : 8;
                total = mColStart[(int)Col.Flags + 1] = total + width + 1;
            } else {
                mColStart[(int)Col.Flags + 1] = total;
            }

            // attributes "@H!#>"
            if ((leftFlags & ActiveColumnFlags.Attr) != 0) {
                total = mColStart[(int)Col.Attr + 1] = total + 5 + 1;
            } else {
                mColStart[(int)Col.Attr + 1] = total;
            }

            total = mColStart[(int)Col.Label + 1] = total + rightWidths[0];
            total = mColStart[(int)Col.Opcode + 1] = total + rightWidths[1];
            total = mColStart[(int)Col.Operand + 1] = total + rightWidths[2];
            //total = mColStart[(int)Col.Comment] = total + rightWidths[3];

            Debug.WriteLine("Export col starts:");
            for (int i = 0; i < (int)Col.COUNT; i++) {
                Debug.WriteLine("  " + i + "(" + ((Col)i) + ") " + mColStart[i]);
            }
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
            int bytesWidth = mColStart[(int)Col.Bytes + 1] - mColStart[(int)Col.Bytes] - 2;

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
                        TextUtil.AppendPaddedString(sb, parts.Comment, mColStart[(int)Col.Label]);
                        break;
                    }

                    if ((mLeftFlags & ActiveColumnFlags.Offset) != 0) {
                        TextUtil.AppendPaddedString(sb, parts.Offset,
                            mColStart[(int)Col.Offset]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Address) != 0) {
                        if (!string.IsNullOrEmpty(parts.Addr)) {
                            TextUtil.AppendPaddedString(sb, parts.Addr + ":",
                                mColStart[(int)Col.Address]);
                        }
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Bytes) != 0) {
                        // Shorten the "...".
                        string bytesStr = parts.Bytes;
                        if (bytesStr != null && bytesStr.Length > bytesWidth) {
                            bytesStr = bytesStr.Substring(0, bytesWidth) + "+";
                        }
                        TextUtil.AppendPaddedString(sb, bytesStr, mColStart[(int)Col.Bytes]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Flags) != 0) {
                        TextUtil.AppendPaddedString(sb, parts.Flags, mColStart[(int)Col.Flags]);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Attr) != 0) {
                        TextUtil.AppendPaddedString(sb, parts.Attr, mColStart[(int)Col.Attr]);
                    }
                    TextUtil.AppendPaddedString(sb, parts.Label, mColStart[(int)Col.Label]);
                    TextUtil.AppendPaddedString(sb, parts.Opcode, mColStart[(int)Col.Opcode]);
                    TextUtil.AppendPaddedString(sb, parts.Operand, mColStart[(int)Col.Operand]);
                    TextUtil.AppendPaddedString(sb, parts.Comment, mColStart[(int)Col.Comment]);
                    break;
                case LineListGen.Line.Type.LongComment:
                case LineListGen.Line.Type.Note:
                    TextUtil.AppendPaddedString(sb, parts.Comment, mColStart[(int)Col.Label]);
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

                //sw.Write("<code style=\"white-space: pre;\">");
                sw.Write("<pre>");
                StringBuilder sb = new StringBuilder(128);
                for (int lineIndex = 0; lineIndex < mCodeLineList.Count; lineIndex++) {
                    if (Selection != null && !Selection[lineIndex]) {
                        continue;
                    }

                    if (GenerateHtmlLine(lineIndex, sb)) {
                        sw.Write(sb.ToString());
                    }
                    sb.Clear();
                }
                sw.WriteLine("</pre>\r\n");

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
        /// Generates HTML output for one display line.  This may result in more than one line
        /// of HTML output, e.g. if the label is longer than the field.  EOL markers will
        /// be added.
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
            int bytesWidth = mColStart[(int)Col.Bytes + 1] - mColStart[(int)Col.Bytes] - 2;

            LineListGen.Line line = mCodeLineList[index];
            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);

            int maxLabelLen = mColStart[(int)Col.Label + 1] - mColStart[(int)Col.Label] - 1;

            string anchorLabel = null;
            if ((line.LineType == LineListGen.Line.Type.Code ||
                        line.LineType == LineListGen.Line.Type.Data ||
                        line.LineType == LineListGen.Line.Type.EquDirective) &&
                    !string.IsNullOrEmpty(parts.Label)) {
                anchorLabel = "<a name=\"" + LABEL_LINK_PREFIX + parts.Label +
                    "\">" + parts.Label + "</a>";
            }

            string linkOperand = null;
            if ((line.LineType == LineListGen.Line.Type.Code ||
                        line.LineType == LineListGen.Line.Type.Data) &&
                    parts.Operand.Length > 0) {
                linkOperand = GetLinkOperand(index, parts.Operand);
            }

            bool suppressLabel = false;
            if (mLongLabelNewLine && (line.LineType == LineListGen.Line.Type.Code ||
                    line.LineType == LineListGen.Line.Type.Data)) {
                int labelLen = string.IsNullOrEmpty(parts.Label) ? 0 : parts.Label.Length;
                if (labelLen > maxLabelLen) {
                    // put on its own line
                    string lstr;
                    if (anchorLabel != null) {
                        lstr = anchorLabel;
                    } else {
                        lstr = parts.Label;
                    }
                    AddSpacedString(sb, 0, mColStart[(int)Col.Label], lstr, parts.Label.Length);
                    sb.Append("\r\n");
                    suppressLabel = true;
                }
            }

            int colPos = 0;

            switch (line.LineType) {
                case LineListGen.Line.Type.Code:
                case LineListGen.Line.Type.Data:
                case LineListGen.Line.Type.EquDirective:
                case LineListGen.Line.Type.RegWidthDirective:
                case LineListGen.Line.Type.OrgDirective:
                case LineListGen.Line.Type.LocalVariableTable:
                    if (parts.IsLongComment) {
                        // This happens for long comments embedded in LV tables, e.g.
                        // "clear table".
                        AddSpacedString(sb, 0, mColStart[(int)Col.Label],
                            TextUtil.EscapeHTML(parts.Comment), parts.Comment.Length);
                        break;
                    }

                    // these columns are optional

                    if ((mLeftFlags & ActiveColumnFlags.Offset) != 0) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Offset],
                            parts.Offset, parts.Offset.Length);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Address) != 0) {
                        if (!string.IsNullOrEmpty(parts.Addr)) {
                            string str = parts.Addr + ":";
                            colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Address],
                                str, str.Length);
                        }
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Bytes) != 0) {
                        // Shorten the "...".
                        string bytesStr = parts.Bytes;
                        if (bytesStr != null) {
                            if (bytesStr.Length > bytesWidth) {
                                bytesStr = bytesStr.Substring(0, bytesWidth) + "+";
                            }
                            colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Bytes],
                                bytesStr, bytesStr.Length);
                        }
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Flags) != 0) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Flags],
                            parts.Flags, parts.Flags.Length);
                    }
                    if ((mLeftFlags & ActiveColumnFlags.Attr) != 0) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Attr],
                            TextUtil.EscapeHTML(parts.Attr), parts.Attr.Length);
                    }

                    // remaining columns are mandatory, but may be empty

                    if (suppressLabel) {
                        // label on previous line
                    } else if (anchorLabel != null) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Label],
                            anchorLabel, parts.Label.Length);
                    } else if (parts.Label != null) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Label],
                            parts.Label, parts.Label.Length);
                    }

                    colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Opcode],
                            parts.Opcode, parts.Opcode.Length);

                    if (linkOperand != null) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Operand],
                            linkOperand, parts.Operand.Length);
                    } else {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Operand],
                            TextUtil.EscapeHTML(parts.Operand), parts.Operand.Length);
                    }

                    if (parts.Comment != null) {
                        colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Comment],
                            TextUtil.EscapeHTML(parts.Comment), parts.Comment.Length);
                    }
                    break;
                case LineListGen.Line.Type.LongComment:
                case LineListGen.Line.Type.Note:
                    if (line.LineType == LineListGen.Line.Type.Note && !IncludeNotes) {
                        return false;
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
                    string cstr;
                    if (rgb != 0) {
                        cstr = string.Format("<span style=\"background-color: #{0:x6}\">{1}</span>",
                            rgb, TextUtil.EscapeHTML(parts.Comment));
                    } else {
                        cstr = TextUtil.EscapeHTML(parts.Comment);
                    }
                    colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Label], cstr,
                        parts.Comment.Length);
                    break;
                case LineListGen.Line.Type.Blank:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            sb.Append("\r\n");
            return true;
        }

        /// <summary>
        /// Appends a string to the string buffer.  If the number of characters in the buffer
        /// is less than the desired start position, spaces will be added.  At least one space
        /// will always be added if the start position is greater than zero and the string
        /// is non-empty.
        /// </summary>
        /// <remarks>
        /// This is useful for things like linkified HTML, where we want to pad out the
        /// string with spaces based on the text that will be presented to the user, rather
        /// than the text that has HTML markup and other goodies.
        /// </remarks>
        /// <param name="sb">Line being constructed.</param>
        /// <param name="initialPosn">Line position on entry.</param>
        /// <param name="colStart">Desired starting position.</param>
        /// <param name="str">String to append.</param>
        /// <param name="virtualLength">Length of string we're pretending to add.</param>
        /// <returns>Updated line position.</returns>
        private int AddSpacedString(StringBuilder sb, int initialPosn, int colStart, string str,
                int virtualLength) {
            if (string.IsNullOrEmpty(str)) {
                return initialPosn;
            }
            int toAdd = colStart - initialPosn;
            if (toAdd < 1 && colStart > 0) {
                // Already some text present, and we're adding more text, but we're past the
                // column start.  Add a space so the columns don't run into each other.
                toAdd = 1;
            }

            int newPosn = initialPosn;
            while (toAdd-- > 0) {
                sb.Append(' ');
                newPosn++;
            }
            sb.Append(str);
            return newPosn + virtualLength;
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
            return TextUtil.EscapeHTML(operand).Replace(sym.Label, linkified);
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
