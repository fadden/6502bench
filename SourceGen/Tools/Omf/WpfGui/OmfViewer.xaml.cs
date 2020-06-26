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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Asm65;

namespace SourceGen.Tools.Omf.WpfGui {
    /// <summary>
    /// Apple IIgs OMF file viewer.
    /// </summary>
    public partial class OmfViewer : Window, INotifyPropertyChanged {
        private byte[] mFileData;
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
                get { return OmfSeg.FileLength; }
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

        private string mMessageStrings;
        public string MessageStrings {
            get { return mMessageStrings; }
            set { mMessageStrings = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="pathName">Path to file on disk.  Only used for display.</param>
        /// <param name="data">File contents.</param>
        public OmfViewer(Window owner, string pathName, byte[] data, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            PathName = pathName;
            mFileData = data;
            mFormatter = formatter;

            mOmfFile = new OmfFile(data);
            mOmfFile.Analyze();

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
    }
}
