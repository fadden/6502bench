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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using CommonUtil;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Export selection dialog.
    /// </summary>
    public partial class Export : Window, INotifyPropertyChanged {
        /// <summary>
        /// Indicates which type of file the user wants to generate.
        /// </summary>
        public enum GenerateFileType {
            Unknown = 0,
            Text,
            Html
        }

        /// <summary>
        /// Result: type of file to generate.
        /// </summary>
        public GenerateFileType GenType { get; private set; }

        /// <summary>
        /// Result: full pathname of output file.
        /// </summary>
        public string PathName { get; private set; }

        /// <summary>
        /// Result: flags indicating which of the optional columns should be shown.
        /// </summary>
        public Exporter.ActiveColumnFlags ColFlags { get; private set; }

        private bool mIncludeNotes;
        public bool IncludeNotes {
            get { return mIncludeNotes; }
            set { mIncludeNotes = value; OnPropertyChanged(); }
        }

        private bool mShowOffset;
        public bool ShowOffset {
            get { return mShowOffset; }
            set { mShowOffset = value; OnPropertyChanged(); }
        }

        private bool mShowAddress;
        public bool ShowAddress {
            get { return mShowAddress; }
            set { mShowAddress = value; OnPropertyChanged(); }
        }

        private bool mShowBytes;
        public bool ShowBytes {
            get { return mShowBytes; }
            set { mShowBytes = value; OnPropertyChanged(); }
        }

        private bool mShowFlags;
        public bool ShowFlags {
            get { return mShowFlags; }
            set { mShowFlags = value; OnPropertyChanged(); }
        }

        private bool mShowAttrs;
        public bool ShowAttr {
            get { return mShowAttrs; }
            set { mShowAttrs = value; OnPropertyChanged(); }
        }

        private bool mSelectionOnly;
        public bool SelectionOnly {
            get { return mSelectionOnly; }
            set { mSelectionOnly = value; OnPropertyChanged(); }
        }

        private bool mLongLabelNewLine;
        public bool LongLabelNewLine {
            get { return mLongLabelNewLine; }
            set { mLongLabelNewLine = value; OnPropertyChanged(); }
        }

        //
        // Numeric input fields, bound directly to TextBox.Text.  These rely on a TextChanged
        // field to update the IsValid flag, because the "set" method is only called when the
        // field contains a valid integer.
        //
        private int mAsmLabelColWidth;
        public int AsmLabelColWidth {
            get { return mAsmLabelColWidth; }
            set {
                if (mAsmLabelColWidth != value) {
                    mAsmLabelColWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        private int mAsmOpcodeColWidth;
        public int AsmOpcodeColWidth {
            get { return mAsmOpcodeColWidth; }
            set {
                if (mAsmOpcodeColWidth != value) {
                    mAsmOpcodeColWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        private int mAsmOperandColWidth;
        public int AsmOperandColWidth {
            get { return mAsmOperandColWidth; }
            set {
                if (mAsmOperandColWidth != value) {
                    mAsmOperandColWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        private int mAsmCommentColWidth;
        public int AsmCommentColWidth {
            get { return mAsmCommentColWidth; }
            set {
                if (mAsmCommentColWidth != value) {
                    mAsmCommentColWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool mTextModePlain;
        public bool TextModePlain {
            get { return mTextModePlain; }
            set { mTextModePlain = value; OnPropertyChanged(); }
        }

        private bool mTextModeCsv;
        public bool TextModeCsv {
            get { return mTextModeCsv; }
            set { mTextModeCsv = value; OnPropertyChanged(); }
        }

        private bool mOverwriteCss;
        public bool OverwriteCss {
            get { return mOverwriteCss; }
            set { mOverwriteCss = value; OnPropertyChanged(); }
        }

        private bool mGenerateImageFiles;
        public bool GenerateImageFiles {
            get { return mGenerateImageFiles; }
            set { mGenerateImageFiles = value; OnPropertyChanged(); }
        }

        private enum TextMode {
            Unknown = 0,
            PlainText,
            Csv
        }

        /// <summary>
        /// Valid flag, used to enable the "generate" buttons.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Filename of project file.
        /// </summary>
        string mProjectFileName;


        public Export(Window owner, string projectFileName) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProjectFileName = projectFileName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            IncludeNotes = AppSettings.Global.GetBool(AppSettings.EXPORT_INCLUDE_NOTES, false);
            ShowOffset = AppSettings.Global.GetBool(AppSettings.EXPORT_SHOW_OFFSET, false);
            ShowAddress = AppSettings.Global.GetBool(AppSettings.EXPORT_SHOW_ADDR, false);
            ShowBytes = AppSettings.Global.GetBool(AppSettings.EXPORT_SHOW_BYTES, false);
            ShowFlags = AppSettings.Global.GetBool(AppSettings.EXPORT_SHOW_FLAGS, false);
            ShowAttr = AppSettings.Global.GetBool(AppSettings.EXPORT_SHOW_ATTR, false);
            SelectionOnly = AppSettings.Global.GetBool(AppSettings.EXPORT_SELECTION_ONLY, false);
            LongLabelNewLine =
                AppSettings.Global.GetBool(AppSettings.EXPORT_LONG_LABEL_NEW_LINE, false);
            GenerateImageFiles =
                AppSettings.Global.GetBool(AppSettings.EXPORT_GENERATE_IMAGE_FILES, true);

            int[] colWidths = new int[] { 16, 8, 18, 100 };   // wide output
            string colStr = AppSettings.Global.GetString(AppSettings.EXPORT_COL_WIDTHS, null);
            try {
                if (!string.IsNullOrEmpty(colStr)) {
                    colWidths = TextUtil.DeserializeIntArray(colStr);
                    if (colWidths.Length != 4) {
                        throw new InvalidOperationException("Bad length " + colWidths.Length);
                    }
                }
            } finally {
                AsmLabelColWidth = colWidths[0];
                AsmOpcodeColWidth = colWidths[1];
                AsmOperandColWidth = colWidths[2];
                AsmCommentColWidth = colWidths[3];
            }

            TextMode mode = AppSettings.Global.GetEnum(AppSettings.EXPORT_TEXT_MODE,
                TextMode.PlainText);
            if (mode == TextMode.PlainText) {
                TextModePlain = true;
            } else {
                TextModeCsv = true;
            }
        }

        /// <summary>
        /// Saves the settings to the global settings object.
        /// </summary>
        private void SaveSettings() {
            AppSettings.Global.SetBool(AppSettings.EXPORT_INCLUDE_NOTES, IncludeNotes);
            AppSettings.Global.SetBool(AppSettings.EXPORT_SHOW_OFFSET, ShowOffset);
            AppSettings.Global.SetBool(AppSettings.EXPORT_SHOW_ADDR, ShowAddress);
            AppSettings.Global.SetBool(AppSettings.EXPORT_SHOW_BYTES, ShowBytes);
            AppSettings.Global.SetBool(AppSettings.EXPORT_SHOW_FLAGS, ShowFlags);
            AppSettings.Global.SetBool(AppSettings.EXPORT_SHOW_ATTR, ShowAttr);
            AppSettings.Global.SetBool(AppSettings.EXPORT_SELECTION_ONLY, SelectionOnly);
            AppSettings.Global.SetBool(AppSettings.EXPORT_LONG_LABEL_NEW_LINE, LongLabelNewLine);
            AppSettings.Global.SetBool(AppSettings.EXPORT_GENERATE_IMAGE_FILES,GenerateImageFiles);
            int[] colWidths = new int[] {
                AsmLabelColWidth, AsmOpcodeColWidth, AsmOperandColWidth, AsmCommentColWidth
            };
            string cereal = TextUtil.SerializeIntArray(colWidths);
            AppSettings.Global.SetString(AppSettings.EXPORT_COL_WIDTHS, cereal);
            // OverwriteCss is not saved, since there's generally no reason to replace it.
            // Forcing the user to check it every time is essentially the same as popping
            // up an "are you sure you want to overwrite" dialog, but less annoying for the
            // common case.

            TextMode mode;
            if (TextModePlain) {
                mode = TextMode.PlainText;
            } else {
                mode = TextMode.Csv;
            }
            AppSettings.Global.SetEnum(AppSettings.EXPORT_TEXT_MODE, mode);
        }

        /// <summary>
        /// Updates the state of the UI.
        /// </summary>
        private void UpdateControls() {
            bool isValid = true;

            isValid &= !Validation.GetHasError(asmLabelColWidthTextBox);
            isValid &= !Validation.GetHasError(asmOpcodeColWidthTextBox);
            isValid &= !Validation.GetHasError(asmOperandColWidthTextBox);
            isValid &= !Validation.GetHasError(asmCommentColWidthTextBox);

            IsValid = isValid;
        }

        /// <summary>
        /// Called whenever something is typed in one of the column width entry boxes.
        /// </summary>
        /// <remarks>
        /// We need this because we're using validated int fields rather than strings.  The
        /// "set" call doesn't fire if the user types garbage.
        /// </remarks>
        private void AsmColWidthTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateControls();
        }

        private void GenerateHtmlButton_Click(object sender, RoutedEventArgs e) {
            GenType = GenerateFileType.Html;
            Finish(Res.Strings.FILE_FILTER_HTML, ".html");
        }

        private void GenerateTextButton_Click(object sender, RoutedEventArgs e) {
            GenType = GenerateFileType.Text;
            if (TextModeCsv) {
                Finish(Res.Strings.FILE_FILTER_CSV, ".csv");
            } else {
                Finish(Res.Strings.FILE_FILTER_TEXT, ".txt");
            }
        }

        /// <summary>
        /// Handles a click on one of the "generate" buttons.
        /// </summary>
        private void Finish(string fileFilter, string fileExt) {
            Debug.Assert(mProjectFileName == Path.GetFileName(mProjectFileName));
            string initialName = Path.GetFileNameWithoutExtension(mProjectFileName) + fileExt;
            if (GenType == GenerateFileType.Html) {
                // Can't link to a file with an unescaped '#' -- the browser will think
                // it's an anchor.
                initialName = initialName.Replace('#', '_');
            }

            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = fileFilter + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1,
                ValidateNames = true,
                AddExtension = true,    // doesn't add extension if non-ext file exists
                FileName = initialName
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            PathName = Path.GetFullPath(fileDlg.FileName);

            ColFlags = Exporter.ActiveColumnFlags.None;
            if (ShowOffset) {
                ColFlags |= Exporter.ActiveColumnFlags.Offset;
            }
            if (ShowAddress) {
                ColFlags |= Exporter.ActiveColumnFlags.Address;
            }
            if (ShowBytes) {
                ColFlags |= Exporter.ActiveColumnFlags.Bytes;
            }
            if (ShowFlags) {
                ColFlags |= Exporter.ActiveColumnFlags.Flags;
            }
            if (ShowAttr) {
                ColFlags |= Exporter.ActiveColumnFlags.Attr;
            }

            SaveSettings();
            DialogResult = true;
        }
    }
}
