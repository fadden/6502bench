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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using Asm65;

namespace SourceGen.Tools.Omf.WpfGui {
    /// <summary>
    /// Apple IIgs OMF file viewer.
    /// </summary>
    public partial class OmfViewer : Window, INotifyPropertyChanged {
        private const string OUT_FILE_SUFFIX = "_ld";

        private OmfFile mOmfFile;
        private Formatter mFormatter;

        //private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class SegmentListItem {
            public OmfSegment OmfSeg { get; private set; }

            public int SegNum {
                get { return OmfSeg.SegNum; }
            }
            public string Version {
                get { return OmfSegment.VersionToString(OmfSeg.Version); }
            }
            public string Kind {
                get { return OmfSegment.KindToString(OmfSeg.Kind); }
            }
            public string LoadName {
                get { return OmfSeg.LoadName; }
            }
            public string SegName {
                get { return OmfSeg.SegName; }
            }
            public int Length {
                get { return OmfSeg.Length; }
            }
            public int FileLength {
                get { return OmfSeg.RawFileLength; }
            }

            public SegmentListItem(OmfSegment omfSeg) {
                OmfSeg = omfSeg;
            }
            public override string ToString() {
                return "[SegmentListItem " + OmfSeg + "]";
            }
        }

        public List<SegmentListItem> SegmentListItems { get; private set; } = new List<SegmentListItem>();

        private string mPathName;
        public string PathName {
            get { return mPathName; }
            set { mPathName = value; OnPropertyChanged(); }
        }

        private string mFileSummaryStr;
        public string FileSummaryStr {
            get { return mFileSummaryStr; }
            set { mFileSummaryStr = value; OnPropertyChanged(); }
        }

        private string mMessageStrings;
        public string MessageStrings {
            get { return mMessageStrings; }
            set { mMessageStrings = value; OnPropertyChanged(); }
        }

        public bool IsLoadFile {
            get { return mOmfFile.OmfFileKind == OmfFile.FileKind.Load; }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="pathName">Path to file on disk.  Only used for display.</param>
        /// <param name="data">File contents.</param>
        /// <param name="formatter">Text formatter.</param>
        public OmfViewer(Window owner, string pathName, byte[] data, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            PathName = pathName;
            mFormatter = formatter;

            mOmfFile = new OmfFile(data);
            mOmfFile.Analyze(mFormatter);

            string summary;
            if (mOmfFile.OmfFileKind == OmfFile.FileKind.Foreign) {
                summary = (string)FindResource("str_OmfFileNot");
            } else {
                string fileStr;
                switch (mOmfFile.OmfFileKind) {
                    case OmfFile.FileKind.Indeterminate:
                        fileStr = (string)FindResource("str_OmfFileIndeterminateStr");
                        break;
                    case OmfFile.FileKind.Load:
                        fileStr = (string)FindResource("str_OmfFileLoadStr");
                        break;
                    case OmfFile.FileKind.Object:
                        fileStr = (string)FindResource("str_OmfFileObjectStr");
                        break;
                    case OmfFile.FileKind.Library:
                        fileStr = (string)FindResource("str_OmfFileLibraryStr");
                        break;
                    default:
                        fileStr = (string)FindResource("str_OmfFileUnknownStr");
                        break;
                }

                string fmt;
                if (mOmfFile.SegmentList.Count == 1) {
                    fmt = (string)FindResource("str_OmfFileSummaryFmt");
                } else {
                    fmt = (string)FindResource("str_OmfFileSummaryPlFmt");
                }
                summary = string.Format(fmt, fileStr, mOmfFile.SegmentList.Count);
            }
            FileSummaryStr = summary;

            foreach (OmfSegment omfSeg in mOmfFile.SegmentList) {
                SegmentListItems.Add(new SegmentListItem(omfSeg));
            }
            MessageStrings = string.Join("\r\n", mOmfFile.MessageList.ToArray());
        }

        private void SegmentList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            SegmentListItem item = (SegmentListItem)((DataGrid)sender).SelectedItem;
            OmfSegmentViewer dlg = new OmfSegmentViewer(this, mOmfFile, item.OmfSeg, mFormatter);
            dlg.ShowDialog();
        }

        private void GenerateProject_Click(object sender, RoutedEventArgs e) {
            Loader loader = new Loader(mOmfFile);
            if (!loader.Prepare()) {
                // Unexpected.  If there's a valid reason for this, we need to add details
                // to the error message.
                string msg = (string)FindResource("str_OmfLoaderFail");
                MessageBox.Show(msg, Res.Strings.OPERATION_FAILED);
                return;
            }

            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1,
                ValidateNames = true,
                AddExtension = true,
                FileName = Path.GetFileName(PathName) + OUT_FILE_SUFFIX
            };
            if (fileDlg.ShowDialog() != true) {
                Debug.WriteLine("Save canceled by user");
                return;
            }
            string pathName = Path.GetFullPath(fileDlg.FileName);

            if (!loader.WriteProjectFiles(pathName, pathName + ProjectFile.FILENAME_EXT,
                    out string errorMessage)) {
                MessageBox.Show(Res.Strings.ERR_PROJECT_SAVE_FAIL + ": " + errorMessage,
                    Res.Strings.OPERATION_FAILED,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Success!  Show a message then close the dialog.
            string smsg = (string)FindResource("str_OmfConvertSuccessful");
            MessageBox.Show(smsg, Res.Strings.OPERATION_SUCCEEDED,
                MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
        }
    }
}
