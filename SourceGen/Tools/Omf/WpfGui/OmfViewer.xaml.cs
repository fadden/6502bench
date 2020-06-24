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
using System.Windows.Input;

namespace SourceGen.Tools.Omf.WpfGui {
    /// <summary>
    /// Apple IIgs OMF file viewer.
    /// </summary>
    public partial class OmfViewer : Window, INotifyPropertyChanged {
        private string mPathName;
        private byte[] mFileData;

        //private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class SegmentListItem {
            public int SegNum { get; private set; }

            // TODO: take OMFSegment obj
            public SegmentListItem(int segNum) {
                SegNum = segNum;
            }
        }

        public List<SegmentListItem> SegmentListItems { get; private set; } = new List<SegmentListItem>();


        public OmfViewer(Window owner, string pathName, byte[] data) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mPathName = pathName;
            mFileData = data;

            SegmentListItems.Add(new SegmentListItem(123));
            SegmentListItems.Add(new SegmentListItem(456));
        }

        private void SegmentList_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            Debug.WriteLine("DCLICK");
        }
    }
}
