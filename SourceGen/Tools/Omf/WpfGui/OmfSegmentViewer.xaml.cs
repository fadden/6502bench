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
using System.Runtime.CompilerServices;
using System.Windows;

using Asm65;

namespace SourceGen.Tools.Omf.WpfGui {
    /// <summary>
    /// Apple IIgs OMF segment viewer.
    /// </summary>
    public partial class OmfSegmentViewer : Window, INotifyPropertyChanged {
        //private OmfFile mOmfFile;
        private OmfSegment mOmfSeg;
        private Formatter mFormatter;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string mFileOffsetLenStr;
        public string FileOffsetLenStr {
            get { return mFileOffsetLenStr; }
            set { mFileOffsetLenStr = value; OnPropertyChanged(); }
        }

        private string mRecordHeaderStr;
        public string RecordHeaderStr {
            get { return mRecordHeaderStr; }
            set { mRecordHeaderStr = value; OnPropertyChanged(); }
        }

        public class HeaderItem {
            public string Name { get; private set; }
            public string Value { get; private set; }
            public string Note { get; private set; }

            public HeaderItem(string name, string value, string note) {
                Name = name;
                Value = value;
                Note = note;
            }
        }
        public List<HeaderItem> HeaderItems { get; private set; } = new List<HeaderItem>();

        public List<OmfRecord> RecordItems { get; private set; }

        public class RelocItem {
            public string Offset { get; private set; }
            public string Reference { get; private set; }
            public string Width { get; private set; }
            public string Shift { get; private set; }
        }
        public List<RelocItem> RelocItems { get; private set; } = new List<RelocItem>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="omfFile">OMF file object.</param>
        /// <param name="omfSeg">Segment to view.  Must be part of omfFile.</param>
        /// <param name="formatter">Text formatter.</param>
        public OmfSegmentViewer(Window owner, OmfFile omfFile, OmfSegment omfSeg,
                Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            //mOmfFile = omfFile;
            mOmfSeg = omfSeg;
            mFormatter = formatter;

            string fmt = (string)FindResource("str_FileOffsetLenFmt");
            FileOffsetLenStr = string.Format(fmt,
                mFormatter.FormatOffset24(omfSeg.FileOffset),
                omfSeg.FileLength,
                mFormatter.FormatHexValue(omfSeg.FileLength, 4));

            GenerateHeaderItems();

            RecordItems = omfSeg.Records;

            fmt = (string)FindResource("str_RecordHeaderFmt");
            RecordHeaderStr = string.Format(fmt, RecordItems.Count);
        }

        private void GenerateHeaderItems() {
            foreach (OmfSegment.NameValueNote nvn in mOmfSeg.RawValues) {
                string value;
                if (nvn.Value is int) {
                    int byteWidth = nvn.Width;
                    if (byteWidth > 3) {
                        byteWidth = 3;
                    }
                    value = mFormatter.FormatHexValue((int)nvn.Value, byteWidth * 2);
                } else {
                    value = nvn.Value.ToString();
                }
                HeaderItems.Add(new HeaderItem(nvn.Name, value, nvn.Note));
            }
        }
    }
}
