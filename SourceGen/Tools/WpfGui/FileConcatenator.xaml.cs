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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;

using CommonUtil;
using System.Windows.Controls;

namespace SourceGen.Tools.WpfGui {
    /// <summary>
    /// File concatenation tool.
    /// </summary>
    public partial class FileConcatenator : Window, INotifyPropertyChanged {
        //
        // Every action causes a selection change, so we don't explicitly call an "update
        // controls" function.
        //

        public bool IsRemoveEnabled {
            get { return mIsRemoveEnabled; }
            set { mIsRemoveEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsRemoveEnabled;

        public bool IsUpEnabled {
            get { return mIsUpEnabled; }
            set { mIsUpEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsUpEnabled;

        public bool IsDownEnabled {
            get { return mIsDownEnabled; }
            set { mIsDownEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsDownEnabled;

        public bool IsSaveEnabled {
            get { return mIsSaveEnabled; }
            set { mIsSaveEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsSaveEnabled;


        /// <summary>
        /// Item for main list.
        /// </summary>
        public class ConcatItem {
            public string PathName { get; private set; }
            public string FileName { get; private set; }
            public long Length { get; private set; }
            public string Crc32 { get; private set; }

            public ConcatItem(string pathName, long length, uint crc32) {
                PathName = pathName;
                Length = length;
                Crc32 = crc32.ToString("x8");

                FileName = Path.GetFileName(pathName);
            }
        }

        public ObservableCollection<ConcatItem> ConcatItems { get; private set; } =
            new ObservableCollection<ConcatItem>();

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        public FileConcatenator(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            // all "enabled" flags are initially false
        }

        private void ConcatGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            bool isItemSelected = (concatGrid.SelectedItem != null);
            IsRemoveEnabled = isItemSelected;
            IsUpEnabled = isItemSelected && concatGrid.SelectedIndex != 0;
            IsDownEnabled = isItemSelected && concatGrid.SelectedIndex != ConcatItems.Count - 1;

            IsSaveEnabled = ConcatItems.Count != 0;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 0,
                Multiselect = true,
                Title = (string)FindResource("str_SelectFileTitle")
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }

            try {
                foreach (string pathName in fileDlg.FileNames) {
                    // The names in FileNames appear to be fully qualified.
                    long length = new FileInfo(pathName).Length;
                    uint crc32 = CRC32.OnWholeFile(pathName);
                    ConcatItems.Add(new ConcatItem(pathName, length, crc32));
                    concatGrid.SelectedIndex = ConcatItems.Count - 1;
                }
            } catch (Exception ex) {
                string caption = (string)FindResource("str_FileAccessFailedCaption");
                string fmt = (string)FindResource("str_FileAccessFailedFmt");
                MessageBox.Show(string.Format(fmt, ex.Message), caption,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e) {
            int index = concatGrid.SelectedIndex;
            ConcatItems.RemoveAt(index);

            // Keep selection at same index, unless we just removed the item at the end.
            if (index == ConcatItems.Count) {
                index--;
            }
            if (index >= 0) {
                concatGrid.SelectedIndex = index;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e) {
            ConcatItem item = (ConcatItem)concatGrid.SelectedItem;
            int index = concatGrid.SelectedIndex;
            Debug.Assert(index > 0);
            ConcatItems.RemoveAt(index);
            ConcatItems.Insert(index - 1, item);
            concatGrid.SelectedIndex = index - 1;
        }

        private void DownButton_Click(object sender, RoutedEventArgs e) {
            ConcatItem item = (ConcatItem)concatGrid.SelectedItem;
            int index = concatGrid.SelectedIndex;
            Debug.Assert(index >= 0 && index < ConcatItems.Count - 1);
            ConcatItems.RemoveAt(index);
            ConcatItems.Insert(index + 1, item);
            concatGrid.SelectedIndex = index + 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 0,
                ValidateNames = true,
                FileName = "concat.bin"
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            string pathName = Path.GetFullPath(fileDlg.FileName);
            Debug.WriteLine("OUTPUT TO " + pathName);

            // Create the new file, and copy each file to it in order.  We compute a total
            // CRC for the file contents as we go.  (It's possible to combine the CRCs we already
            // have with some math; see e.g. https://stackoverflow.com/a/44061990/294248 .)
            uint totalCrc;
            long totalLen;
            try {
                using (FileStream stream = new FileStream(pathName, FileMode.Create)) {
                    totalCrc = CopyAllFiles(stream);
                }
                totalLen = new FileInfo(pathName).Length;
            } catch (Exception ex) {
                string ecaption = (string)FindResource("str_FileAccessFailedCaption");
                string efmt = (string)FindResource("str_FileAccessFailedFmt");
                MessageBox.Show(string.Format(efmt, ex.Message), ecaption,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // TODO(someday): make all data available on the clipboard

            string caption = (string)FindResource("str_SuccessCaption");
            string fmt = (string)FindResource("str_SuccessMsgFmt");
            MessageBox.Show(string.Format(fmt, totalLen, "0x" + totalCrc.ToString("x8")),
                caption, MessageBoxButton.OK, MessageBoxImage.None);
            DialogResult = true;
        }

        private uint CopyAllFiles(Stream outStream) {
            byte[] buffer = new byte[256 * 1024];
            uint crc32 = 0;

            foreach (ConcatItem item in ConcatItems) {
                CopyFileToStreamWithCrc32(item.PathName, outStream, buffer, ref crc32);
            }

            return crc32;
        }

        /// <summary>
        /// Copies a file on disk to a Stream, computing a CRC-32 on the data.  Throws an
        /// exception on file error.
        /// </summary>
        /// <param name="inputPath">Full path to input file.</param>
        /// <param name="outStream">Output data stream.</param>
        /// <param name="buffer">Data buffer, to avoid repeated allocations.</param>
        /// <param name="crc">CRC-32, initially zero.</param>
        public static void CopyFileToStreamWithCrc32(string inputPath, Stream outStream,
                byte[] buffer, ref uint crc) {
            using (FileStream inStream = new FileStream(inputPath, FileMode.Open,
                    FileAccess.Read)) {
                while (true) {
                    int actual = inStream.Read(buffer, 0, buffer.Length);
                    if (actual == 0) {
                        // end of stream
                        return;
                    }

                    crc = CRC32.OnBuffer(crc, buffer, 0, actual);

                    outStream.Write(buffer, 0, actual);
                }
            }
        }
    }
}
