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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Asm65;
using CommonUtil;
using CommonWPF;

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
        /// Should image files be generated?
        /// </summary>
        public bool GenerateImageFiles { get; set; }

        /// <summary>
        /// If set, labels that are wider than the label column should go on their own line.
        /// </summary>
        public bool LongLabelNewLine { get; set; }

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
        /// Directory path for image files.
        /// </summary>
        private string mImageDirPath;

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

        private string mParameterStringBase;


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
            mParameterStringBase = GenerateParameterStringBase(leftFlags, rightWidths);
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
                width = mFormatter.Config.SpacesBetweenBytes ? 12 : 9;
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
            //total = mColStart[(int)Col.Comment + 1] = total + rightWidths[3];

            //Debug.WriteLine("Export col starts:");
            //for (int i = 0; i < (int)Col.COUNT; i++) {
            //    Debug.WriteLine("  " + i + "(" + ((Col)i) + ") " + mColStart[i]);
            //}
        }

        /// <summary>
        /// Generates description of some parameters that we only have during construction.
        /// </summary>
        private static string GenerateParameterStringBase(ActiveColumnFlags leftFlags,
                int[] rightWidths) {
            StringBuilder sb = new StringBuilder();

            sb.Append("cols=");
            for (int i = 0; i < rightWidths.Length; i++) {
                if (i != 0) {
                    sb.Append(',');
                }
                sb.Append(rightWidths[i]);
            }

            sb.Append(";extraCols=");
            bool first = true;
            foreach (ActiveColumnFlags flag in Enum.GetValues(typeof(ActiveColumnFlags))) {
                if (flag == ActiveColumnFlags.ALL) {
                    continue;
                }
                if ((leftFlags & flag) != 0) {
                    if (!first) {
                        sb.Append(',');
                    }
                    sb.Append(flag);
                    first = false;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates a description of configured parameters.  Intended to be human-readable,
        /// but possibly machine-readable as well.
        /// </summary>
        private string GenerateParameterString() {
            StringBuilder sb = new StringBuilder(mParameterStringBase);

            sb.Append(";byteSpc=");
            sb.Append(mFormatter.Config.SpacesBetweenBytes.ToString());
            sb.Append(";commaBulk=");
            sb.Append(mFormatter.Config.CommaSeparatedDense.ToString());
            sb.Append(";nonuPfx='");
            sb.Append(mFormatter.Config.NonUniqueLabelPrefix);
            sb.Append('\'');
            sb.Append(";varPfx='");
            sb.Append(mFormatter.Config.LocalVariableLabelPrefix);
            sb.Append('\'');
            sb.Append(";flcd='");
            sb.Append(mFormatter.Config.FullLineCommentDelimiterBase);
            sb.Append('\'');
            sb.Append(";labelBrk=");
            sb.Append(LongLabelNewLine.ToString());
            sb.Append(";notes=");
            sb.Append(IncludeNotes.ToString());
            sb.Append(";gfx=");
            sb.Append(GenerateImageFiles.ToString());
            sb.Append(";opWrap=");
            sb.Append(mFormatter.Config.OperandWrapLen);
            sb.Append(";upper=");
            if (mFormatter.Config.UpperHexDigits) { sb.Append('D'); }
            if (mFormatter.Config.UpperOpcodes) { sb.Append('O'); }
            if (mFormatter.Config.UpperPseudoOpcodes) { sb.Append('P'); }
            if (mFormatter.Config.UpperOperandA) { sb.Append('A'); }
            if (mFormatter.Config.UpperOperandS) { sb.Append('S'); }
            if (mFormatter.Config.UpperOperandXY) { sb.Append('X'); }

            // Not included: pseudo-op definitions; delimiter definitions

            return sb.ToString();
        }

        /// <summary>
        /// Converts the selected lines to formatted text.
        /// </summary>
        /// <param name="fullText">Result; holds text of all selected lines.</param>
        /// <param name="csvText">Result; holds text of all selected lines, in CSV format.</param>
        public void SelectionToString(bool addCsv, out string fullText, out string csvText) {
            StringBuilder sb = new StringBuilder(128);
            StringWriter plainText = new StringWriter();
            StringWriter csv = null;
            if (addCsv) {
                csv = new StringWriter();
            }

            for (int lineIndex = 0; lineIndex < mCodeLineList.Count; lineIndex++) {
                if (!Selection[lineIndex]) {
                    continue;
                }
                GenerateTextLine(lineIndex, plainText, sb);
                if (addCsv) {
                    GenerateCsvLine(lineIndex, csv, sb);
                }
            }

            plainText.Close();
            fullText = plainText.ToString();
            if (addCsv) {
                csv.Close();
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
                        GenerateCsvLine(lineIndex, sw, sb);
                    } else {
                        GenerateTextLine(lineIndex, sw, sb);
                    }
                }
            }
        }

        /// <summary>
        /// Generates text output for one display line.  This may result in more than one line
        /// of output, e.g. if the label is longer than the field.  EOL markers will be added.
        /// </summary>
        /// <param name="index">Index of line to output.</param>
        /// <param name="tw">Text output destination.</param>
        /// <param name="sb">Pre-allocated string builder (this is a minor optimization).</param>
        private void GenerateTextLine(int index, TextWriter tw, StringBuilder sb) {
            LineListGen.Line line = mCodeLineList[index];
            if (line.LineType == LineListGen.Line.Type.Note && !IncludeNotes) {
                return;
            }

            // Width of "bytes" field, without '+' or trailing space.
            int bytesWidth = mColStart[(int)Col.Bytes + 1] - mColStart[(int)Col.Bytes] - 2;
            // Width of "label" field, without trailing space.
            int maxLabelLen = mColStart[(int)Col.Label + 1] - mColStart[(int)Col.Label] - 1;

            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);
            sb.Clear();

            // Put long labels on their own line if desired.
            bool suppressLabel = false;
            if (LongLabelNewLine && (line.LineType == LineListGen.Line.Type.Code ||
                    line.LineType == LineListGen.Line.Type.Data)) {
                int labelLen = string.IsNullOrEmpty(parts.Label) ? 0 : parts.Label.Length;
                if (labelLen > maxLabelLen) {
                    // put on its own line
                    TextUtil.AppendPaddedString(sb, parts.Label, mColStart[(int)Col.Label]);
                    tw.WriteLine(sb);
                    sb.Clear();
                    suppressLabel = true;
                }
            }

            switch (line.LineType) {
                case LineListGen.Line.Type.Code:
                case LineListGen.Line.Type.Data:
                case LineListGen.Line.Type.EquDirective:
                case LineListGen.Line.Type.RegWidthDirective:
                case LineListGen.Line.Type.DataBankDirective:
                case LineListGen.Line.Type.ArStartDirective:
                case LineListGen.Line.Type.ArEndDirective:
                case LineListGen.Line.Type.LocalVariableTable:
                    if (parts.IsLongComment) {
                        // This happens for long comments generated for LV tables (e.g. "empty
                        // variable table").
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
                    if (!suppressLabel) {
                        TextUtil.AppendPaddedString(sb, parts.Label, mColStart[(int)Col.Label]);
                    }
                    TextUtil.AppendPaddedString(sb, parts.Opcode, mColStart[(int)Col.Opcode]);
                    TextUtil.AppendPaddedString(sb, parts.Operand, mColStart[(int)Col.Operand]);
                    TextUtil.AppendPaddedString(sb, parts.Comment, mColStart[(int)Col.Comment]);
                    break;
                case LineListGen.Line.Type.LongComment:
                case LineListGen.Line.Type.Note:
                    TextUtil.AppendPaddedString(sb, parts.Comment, mColStart[(int)Col.Label]);
                    break;
                case LineListGen.Line.Type.VisualizationSet:
                    return;     // show nothing
                case LineListGen.Line.Type.Blank:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            tw.WriteLine(sb);
        }

        private void GenerateCsvLine(int index, TextWriter tw, StringBuilder sb) {
            LineListGen.Line line = mCodeLineList[index];
            if (line.LineType == LineListGen.Line.Type.Note && !IncludeNotes) {
                return;
            }
            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);
            sb.Clear();

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

            tw.WriteLine(sb);
        }

        #region HTML

        private const string HTML_EXPORT_TEMPLATE = "ExportTemplate.html";
        private const string HTML_EXPORT_CSS_FILE = "SGStyle.css";
        private const string LABEL_LINK_PREFIX = "Sym";
        private const char NU_LINK_CHAR = '!';       // replaces Symbol.UNIQUE_TAG_CHAR

        private class ExportWorker : WorkProgress.IWorker {
            private Exporter mExporter;
            private string mPathName;
            private bool mOverwriteCss;

            public bool Success { get; private set; }

            public ExportWorker(Exporter exp, string pathName, bool overwriteCss) {
                mExporter = exp;
                mPathName = pathName;
                mOverwriteCss = overwriteCss;
            }
            public object DoWork(BackgroundWorker worker) {
                return mExporter.OutputToHtml(worker, mPathName, mOverwriteCss);
            }
            public void RunWorkerCompleted(object results) {
                if (results != null) {
                    Success = (bool)results;
                }
            }
        }

        /// <summary>
        /// Generates HTML output to the specified path.
        /// </summary>
        /// <param name="pathName">Full pathname of output file (including ".html").  This
        ///   defines the root directory if there are additional files.</param>
        /// <param name="overwriteCss">If set, existing CSS file will be replaced.</param>
        public void OutputToHtml(Window parent, string pathName, bool overwriteCss) {
            ExportWorker ew = new ExportWorker(this, pathName, overwriteCss);
            WorkProgress dlg = new WorkProgress(parent, ew, false);
            if (dlg.ShowDialog() != true) {
                Debug.WriteLine("Export unsuccessful");
            } else {
                Debug.WriteLine("Export complete");
            }
        }

        private bool OutputToHtml(BackgroundWorker worker, string pathName, bool overwriteCss) {
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
                return false;
            }

            // We should only need the _IMG directory if there are visualizations.
            if (GenerateImageFiles && mProject.VisualizationSets.Count != 0) {
                string imageDirName = Path.GetFileNameWithoutExtension(pathName) + "_IMG";
                string imageDirPath = Path.Combine(Path.GetDirectoryName(pathName), imageDirName);
                bool exists = false;
                try {
                    FileAttributes attr = File.GetAttributes(imageDirPath);
                    if ((attr & FileAttributes.Directory) != FileAttributes.Directory) {
                        string msg = string.Format(Res.Strings.ERR_FILE_EXISTS_NOT_DIR_FMT,
                            imageDirPath);
                        MessageBox.Show(msg, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                    exists = true;
                } catch (FileNotFoundException) {
                } catch (DirectoryNotFoundException) {
                }

                if (!exists) {
                    try {
                        Directory.CreateDirectory(imageDirPath);
                    } catch (Exception ex) {
                        string msg = string.Format(Res.Strings.ERR_DIR_CREATE_FAILED_FMT,
                            imageDirPath, ex.Message);
                        MessageBox.Show(msg, Res.Strings.ERR_FILE_GENERIC_CAPTION,
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }

                // All good.
                mImageDirPath = imageDirPath;
            }

            if (mImageDirPath == null) {
                worker.ReportProgress(0, Res.Strings.EXPORTING_HTML);
            } else {
                worker.ReportProgress(0, Res.Strings.EXPORTING_HTML_AND_IMAGES);
            }


            // Perform some quick substitutions.  This could be done more efficiently,
            // but we're only doing this on the template file, which should be small.
            tmplStr = tmplStr.Replace("$ProjectName$", mProject.DataFileName);
            tmplStr = tmplStr.Replace("$AppVersion$", App.ProgramVersion.ToString());
            string expModeStr = AppSettings.Global.GetEnum(AppSettings.FMT_EXPRESSION_MODE,
                    Formatter.FormatConfig.ExpressionMode.Unknown).ToString();
            tmplStr = tmplStr.Replace("$ExpressionStyle$", expModeStr);
            string dateStr = DateTime.Now.ToString("yyyy/MM/dd");
            string timeStr = DateTime.Now.ToString("HH:mm:ss zzz");
            tmplStr = tmplStr.Replace("$CurrentDate$", dateStr);
            tmplStr = tmplStr.Replace("$CurrentTime$", timeStr);
            tmplStr = tmplStr.Replace("$GenParameters$", GenerateParameterString());

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
                return false;
            }
            string template1 = tmplStr.Substring(0, splitPoint);
            string template2 = tmplStr.Substring(splitPoint + CodeLinesStr.Length);

            int lastProgressPerc = 0;

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

                    GenerateHtmlLine(lineIndex, sw, sb);

                    if (worker.CancellationPending) {
                        break;
                    }
                    int perc = (lineIndex * 100) / mCodeLineList.Count;
                    if (perc != lastProgressPerc) {
                        lastProgressPerc = perc;
                        worker.ReportProgress(perc);
                    }
                }
                sw.WriteLine("</pre>\r\n");

                sw.Write(template2);
            }

            if (worker.CancellationPending) {
                Debug.WriteLine("Cancel requested, deleting " + pathName);
                File.Delete(pathName);
                return false;
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
                    return false;
                }
            }

            return true;
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
        /// <param name="tw">Text output destination.</param>
        /// <param name="sb">String builder to append text to.  Must be cleared before
        ///   calling here.  (This is a minor optimization.)</param>
        private void GenerateHtmlLine(int index, TextWriter tw, StringBuilder sb) {
            LineListGen.Line line = mCodeLineList[index];
            if (line.LineType == LineListGen.Line.Type.Note && !IncludeNotes) {
                return;
            }

            sb.Clear();

            // Width of "bytes" field, without '+' or trailing space.
            int bytesWidth = mColStart[(int)Col.Bytes + 1] - mColStart[(int)Col.Bytes] - 2;
            // Width of "label" field, without trailing space.
            int maxLabelLen = mColStart[(int)Col.Label + 1] - mColStart[(int)Col.Label] - 1;

            DisplayList.FormattedParts parts = mCodeLineList.GetFormattedParts(index);

            // If needed, create an HTML anchor for the label field.
            string anchorLabel = null;
            if ((line.LineType == LineListGen.Line.Type.Code ||
                        line.LineType == LineListGen.Line.Type.Data ||
                        line.LineType == LineListGen.Line.Type.EquDirective) &&
                    !string.IsNullOrEmpty(parts.Label)) {
                string labelText;
                if (parts.Label.StartsWith(mFormatter.NonUniqueLabelPrefix)) {
                    // We need the symbol with the uniquification tag.  The UNIQUE_TAG_CHAR
                    // is only found in non-labels, so there's no risk of collision, but it
                    // can cause uglification.
                    Anattrib attr = mProject.GetAnattrib(line.FileOffset);
                    labelText = attr.Symbol.Label.Replace(Symbol.UNIQUE_TAG_CHAR, NU_LINK_CHAR);
                } else {
                    labelText = Symbol.TrimAndValidateLabel(parts.Label,
                        mFormatter.NonUniqueLabelPrefix, out bool isValid, out bool unused1,
                        out bool unused2, out bool unused3, out Symbol.LabelAnnotation unusedAnno);
                }
                anchorLabel = "<span id=\"" + LABEL_LINK_PREFIX + labelText +
                    "\">" + parts.Label + "</span>";
            }

            // If needed, create an HTML link for the operand field.
            string linkOperand = null;
            if ((line.LineType == LineListGen.Line.Type.Code ||
                        line.LineType == LineListGen.Line.Type.Data) &&
                    parts.Operand.Length > 0) {
                linkOperand = GetLinkOperand(index, parts.Operand);
            }

            // Put long labels on their own line if desired.
            bool suppressLabel = false;
            if (LongLabelNewLine && (line.LineType == LineListGen.Line.Type.Code ||
                    line.LineType == LineListGen.Line.Type.Data)) {
                int labelLen = parts.Label.Length;
                if (labelLen > maxLabelLen) {
                    // put on its own line
                    string lstr;
                    if (anchorLabel != null) {
                        lstr = anchorLabel;
                    } else {
                        lstr = parts.Label;
                    }
                    AddSpacedString(sb, 0, mColStart[(int)Col.Label], lstr, parts.Label.Length);
                    tw.WriteLine(sb);
                    sb.Clear();
                    suppressLabel = true;
                }
            }

            int colPos = 0;

            switch (line.LineType) {
                case LineListGen.Line.Type.Code:
                case LineListGen.Line.Type.Data:
                case LineListGen.Line.Type.EquDirective:
                case LineListGen.Line.Type.RegWidthDirective:
                case LineListGen.Line.Type.DataBankDirective:
                case LineListGen.Line.Type.ArStartDirective:
                case LineListGen.Line.Type.ArEndDirective:
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
                            string str;
                            if (parts.IsNonAddressable) {
                                str = "<span class=\"greytext\">" + parts.Addr + "</span>";
                            } else {
                                str = parts.Addr;
                            }
                            str += ":";
                            colPos = AddSpacedString(sb, colPos, mColStart[(int)Col.Address],
                                str, parts.Addr.Length + 1);
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
                case LineListGen.Line.Type.VisualizationSet:
                    if (!GenerateImageFiles) {
                        // generate nothing at all
                        return;
                    }
                    while (colPos < mColStart[(int)Col.Label]) {
                        sb.Append(' ');
                        colPos++;
                    }
                    OutputVisualizationSet(line.FileOffset, sb);
                    break;
                case LineListGen.Line.Type.Blank:
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            tw.WriteLine(sb);
        }

        /// <summary>
        /// Generate one or more GIF image files, and output references to them.
        /// </summary>
        /// <param name="offset">Visualization set file offset.</param>
        /// <param name="sb">String builder for the HTML output.</param>
        private void OutputVisualizationSet(int offset, StringBuilder sb) {
            const int IMAGE_SIZE = 64;
            const int MAX_WIDTH_PER_LINE = 768;

            if (!mProject.VisualizationSets.TryGetValue(offset,
                    out VisualizationSet visSet)) {
                sb.Append("Internal error - visualization set missing");
                Debug.Assert(false);
                return;
            }
            if (visSet.Count == 0) {
                sb.Append("Internal error - empty visualization set");
                Debug.Assert(false);
                return;
            }

            string imageDirFileName = Path.GetFileName(mImageDirPath);
            int outputWidth = 0;

            for (int index = 0; index < visSet.Count; index++) {
                string fileName = "vis" + offset.ToString("x6") + "_" + index.ToString("d2");

                int dispWidth, dispHeight;

                Visualization vis = visSet[index];
                if (vis is VisBitmapAnimation) {
                    // Animated visualization.
                    VisBitmapAnimation visAnim = (VisBitmapAnimation)vis;
                    int frameDelay = PluginCommon.Util.GetFromObjDict(visAnim.VisGenParams,
                        VisBitmapAnimation.P_FRAME_DELAY_MSEC_PARAM, 330);
                    AnimatedGifEncoder encoder = new AnimatedGifEncoder();

                    // Gather list of frames.
                    for (int i = 0; i < visAnim.Count; i++) {
                        Visualization avis = VisualizationSet.FindVisualizationBySerial(
                            mProject.VisualizationSets, visAnim[i]);
                        if (avis != null) {
                            encoder.AddFrame(BitmapFrame.Create(avis.CachedImage), frameDelay);
                        } else {
                            Debug.Assert(false);        // not expected
                        }
                    }
#if false
                    // try feeding the animated GIF into our GIF unpacker
                    using (MemoryStream ms = new MemoryStream()) {
                        encoder.Save(ms, out dispWidth, out dispHeight);
                        Debug.WriteLine("TESTING");
                        UnpackedGif anim = UnpackedGif.Create(ms.GetBuffer());
                        anim.DebugDump();
                    }
#endif

                    // Create new or replace existing image file.
                    fileName += "_ani.gif";
                    string pathName = Path.Combine(mImageDirPath, fileName);
                    try {
                        using (FileStream stream = new FileStream(pathName, FileMode.Create)) {
                            encoder.Save(stream, out dispWidth, out dispHeight);
                        }
                    } catch (Exception ex) {
                        // TODO: add an error report
                        Debug.WriteLine("Error creating animated GIF file '" + pathName +
                            "': " + ex.Message);
                        dispWidth = dispHeight = 1;
                    }
                } else if (vis is VisWireframeAnimation) {
                    AnimatedGifEncoder encoder = new AnimatedGifEncoder();
                    ((VisWireframeAnimation)vis).EncodeGif(encoder, IMAGE_SIZE);

                    // Create new or replace existing image file.
                    fileName += "_ani.gif";
                    string pathName = Path.Combine(mImageDirPath, fileName);
                    try {
                        using (FileStream stream = new FileStream(pathName, FileMode.Create)) {
                            encoder.Save(stream, out dispWidth, out dispHeight);
                        }
                    } catch (Exception ex) {
                        // TODO: add an error report
                        Debug.WriteLine("Error creating animated WF GIF file '" + pathName +
                            "': " + ex.Message);
                        dispWidth = dispHeight = 1;
                    }
                } else {
                    // Bitmap visualization -or- non-animated wireframe visualization.
                    //
                    // Encode a GIF the same size as the original bitmap.  For a wireframe
                    // visualization this means the bitmap will be the same size as the
                    // generated thumbnail.
                    GifBitmapEncoder encoder = new GifBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(vis.CachedImage));

                    // Create new or replace existing image file.
                    fileName += ".gif";
                    string pathName = Path.Combine(mImageDirPath, fileName);
                    try {
                        using (FileStream stream = new FileStream(pathName, FileMode.Create)) {
                            encoder.Save(stream);
                        }
                    } catch (Exception ex) {
                        // Something went wrong with file creation.  We don't have an error
                        // reporting mechanism, so this will just appear as a broken or stale
                        // image reference.
                        // TODO: add an error report
                        Debug.WriteLine("Error creating GIF file '" + pathName + "': " +
                            ex.Message);
                    }

                    dispWidth = (int)vis.CachedImage.Width;
                    dispHeight = (int)vis.CachedImage.Height;
                }

                // Output thumbnail-size IMG element, preserving proportions.  I'm assuming
                // images will be small enough that generating a separate thumbnail would be
                // counter-productive.  This seems to look best if the height is consistent
                // across all visualization lines, but that can create some monsters (e.g.
                // a bitmap that's 1 pixel high and 40 wide), so we cap the width.
                int dimMult = IMAGE_SIZE;
                double maxDim = dispHeight;
                if (dispWidth > dispHeight * 2) {
                    // Too proportionally wide, so use the width as the limit.  Allow it to
                    // up to 2x the max width (which can't cause the thumb height to exceed
                    // the height limit).
                    maxDim = dispWidth;
                    dimMult *= 2;
                }
                int thumbWidth = (int)Math.Round(dimMult * (dispWidth / maxDim));
                int thumbHeight = (int)Math.Round(dimMult * (dispHeight / maxDim));
                //Debug.WriteLine(dispWidth + "x" + dispHeight + " --> " +
                //    thumbWidth + "x" + thumbHeight + " (" + maxDim + ")");

                if (outputWidth > MAX_WIDTH_PER_LINE) {
                    // Add a line break.  In "pre" mode the bitmaps just run off the right
                    // edge of the screen.  The way we're doing it is imprecise and doesn't
                    // flow with changes to the browser width, but it'll do for now.
                    sb.AppendLine("<br/>");
                    for (int i = 0; i < mColStart[(int)Col.Label]; i++) {
                        sb.Append(' ');
                    }
                    outputWidth = 0;
                } else if (index != 0) {
                    sb.Append("&nbsp;");
                }
                outputWidth += thumbWidth;

                sb.Append("<img class=\"vis\" alt=\"vis\" src=\"");
                sb.Append(imageDirFileName);
                sb.Append('/');
                sb.Append(fileName);
                sb.Append("\" width=\"" + thumbWidth + "\" height=\"" + thumbHeight + "\"/>");
            }
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
        /// We want to replace only the label part of operands that have formulas, e.g. just the
        /// word "foo" in "foo+1".  The string substitution feels a little flimsy, but this is
        /// much simpler than reformatting the operand from scratch.  (We don't want to linkify
        /// the entire operand text, but that would be misleading because clicking on the link
        /// jumps to the label "foo", not the address "foo+1".)
        /// </remarks>
        /// <param name="index">Display line index.</param>
        /// <param name="operand">Full text of formatted operand.</param>
        private string GetLinkOperand(int index, string operand) {
            LineListGen.Line line = mCodeLineList[index];
            if (line.FileOffset < 0) {
                // EQU directive - we shouldn't be here.
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

            // Symbol refs are weak.  If the symbol doesn't exist, the value will have been
            // formatted as hex.  We can't simply check to see if the formatted operand
            // contains the symbol, because we could false-positive on the use of symbols
            // whose label is a valid hex value, e.g. "ABCD = $ABCD".
            //
            // We currently exclude references to local variables, since those require special
            // handling.
            if (!mProject.SymbolTable.TryGetNonVariableValue(attr.DataDescriptor.SymbolRef.Label,
                    out Symbol sym)) {
                return null;
            }
            string htmlId = sym.Label;              // HTML id, usually just the label itself
            string dispText = sym.Label;            // displayed text we're substituting
            if (sym.IsNonUnique) {
                // Normally we just substitute an HTML blob for a label string like "foo", but
                // now we're replacing "@foo" (since we want '@' to be included in the anchor)
                // with a link to the uniquified label (which looks like "Symbol§000175").
                //
                // HTML 5 allows just about any character in an anchor, though avoiding characters
                // used in CSS selectors is recommended.  The '§' char turns into "%C2%A7" when
                // you copy the link (UTF-8 encoding), which is not ideal.  We don't want to use
                // a valid label char like '_', since that would require using a different
                // LABEL_LINK_PREFIX to ensure uniqueness, and we want to avoid characters that
                // have a special meaning to CSS or in URLs.
                htmlId = sym.Label.Replace(Symbol.UNIQUE_TAG_CHAR, NU_LINK_CHAR);
                dispText = sym.GenerateDisplayLabel(mFormatter);
            }
            string linkified =
                "<a href=\"#" + LABEL_LINK_PREFIX + htmlId + "\">" + dispText + "</a>";
            // If our attempt to recreate the display text went wrong, the Replace() operation
            // will just leave the original string intact.
            return TextUtil.EscapeHTML(operand).Replace(dispText, linkified);
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
                    sb.Append("  <tr><th>Label</th><th>Value</th></tr>");
                }
                sb.Append("  <tr>");
                sb.Append("<td><a href=\"#" + LABEL_LINK_PREFIX + sym.Label + "\">" +
                    sym.Label + "</a></td>");
                sb.Append("<td><code>");
                if (sym.Value != Address.NON_ADDR) {
                    sb.Append(mFormatter.FormatHexValue(sym.Value, 2));
                } else {
                    sb.Append(Address.NON_ADDR_STR);
                }
                sb.Append("</code></td>");
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
