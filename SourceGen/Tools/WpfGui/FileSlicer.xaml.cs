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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using Asm65;
using System.Text;
using System.Windows.Media;

namespace SourceGen.Tools.WpfGui {
    /// <summary>
    /// File slicer tool.
    /// </summary>
    public partial class FileSlicer : Window, INotifyPropertyChanged {
        /// <summary>
        /// Path to file to slice.
        /// </summary>
        private string mPathName;

        /// <summary>
        /// Text formatter.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Length of file to slice.
        /// </summary>
        private long mFileLength;

        /// <summary>
        /// Open file.
        /// </summary>
        private FileStream mFileStream;

        private bool mIsSaveEnabled;
        public bool IsSaveEnabled {
            get { return mIsSaveEnabled; }
            set { mIsSaveEnabled = value; OnPropertyChanged(); }
        }

        public string FileLengthStr {
            get { return FormatDecAndHex(mFileLength); }
        }

        // Start/length entry fields and dec+hex display.
        private string mSliceStart;
        public string SliceStart {
            get { return mSliceStart; }
            set { mSliceStart = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mSliceStartDesc;
        public string SliceStartDesc {
            get { return mSliceStartDesc; }
            set { mSliceStartDesc = value; OnPropertyChanged(); }
        }
        private string mSliceLength;
        public string SliceLength {
            get { return mSliceLength; }
            set { mSliceLength = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mSliceLengthDesc;
        public string SliceLengthDesc {
            get { return mSliceLengthDesc; }
            set { mSliceLengthDesc = value; OnPropertyChanged(); }
        }

        private string mStartHexDump;
        public string StartHexDump {
            get { return mStartHexDump; }
            set { mStartHexDump = value; OnPropertyChanged(); }
        }
        private string mEndHexDump;
        public string EndHexDump {
            get { return mEndHexDump; }
            set { mEndHexDump = value; OnPropertyChanged(); }
        }

        // Text turns red on error.
        private Brush mSliceStartBrush;
        public Brush SliceStartBrush {
            get { return mSliceStartBrush; }
            set { mSliceStartBrush = value; OnPropertyChanged(); }
        }
        private Brush mSliceLengthBrush;
        public Brush SliceLengthBrush {
            get { return mSliceLengthBrush; }
            set { mSliceLengthBrush = value; OnPropertyChanged(); }
        }

        private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;
        private Brush mErrorLabelColor = Brushes.Red;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public FileSlicer(Window owner, string pathName, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mPathName = pathName;
            mFormatter = formatter;

            mFileLength = new FileInfo(pathName).Length;
            mFileStream = new FileStream(pathName, FileMode.Open, FileAccess.Read);

            mSliceStart = mSliceLength = string.Empty;
            UpdateControls();
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            mFileStream.Close();
        }

        /// <summary>
        /// Formats a value in decimal and hex.
        /// </summary>
        /// <param name="val">Value to format.</param>
        /// <returns>Formatted string.</returns>
        private string FormatDecAndHex(long val) {
            StringBuilder sb = new StringBuilder();
            sb.Append(val.ToString());
            sb.Append(" (");
            sb.Append(mFormatter.FormatHexValue((int)val, 4));
            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Updates the state of the controls after something changed.
        /// </summary>
        private void UpdateControls() {
            ParseStartLength(out bool isStartValid, out long sliceStart,
                    out bool isLengthValid, out long sliceLength);

            SliceStartDesc = FormatDecAndHex(sliceStart);
            SliceLengthDesc = FormatDecAndHex(sliceLength) +
                (string)FindResource("str_LastByteAt") +
                mFormatter.FormatOffset24((int)(sliceStart + sliceLength - 1));

            if (isStartValid && isLengthValid) {
                // anchor is first byte in slice
                StartHexDump = CreateSplitHexDump(0, sliceStart, sliceStart + sliceLength - 1);
                // anchor is first byte after slice (may be off end)
                EndHexDump = CreateSplitHexDump(sliceStart, sliceStart + sliceLength,
                    mFileLength - 1);
            } else {
                StartHexDump = EndHexDump = string.Empty;
            }

            SliceStartBrush = isStartValid ? mDefaultLabelColor : mErrorLabelColor;
            SliceLengthBrush = isLengthValid ? mDefaultLabelColor : mErrorLabelColor;

            IsSaveEnabled = isStartValid && isLengthValid;
        }

        private void ParseStartLength(out bool startOk, out long sliceStart,
                out bool lengthOk, out long sliceLength) {
            startOk = lengthOk = true;

            if (string.IsNullOrEmpty(SliceStart)) {
                sliceStart = 0;
            } else if (Number.TryParseLong(SliceStart.Trim(), out sliceStart, out int unused1)) {
                if (sliceStart < 0 || sliceStart >= mFileLength) {
                    startOk = false;
                }
            } else {
                startOk = false;
            }

            if (string.IsNullOrEmpty(SliceLength)) {
                sliceLength = mFileLength - sliceStart;
                if (sliceLength < 0) {
                    sliceLength = 0;
                }
            } else if (Number.TryParseLong(SliceLength.Trim(), out sliceLength, out int unused2)) {
                if (sliceLength <= 0 || sliceLength > mFileLength ||
                        sliceStart + sliceLength > mFileLength) {
                    lengthOk = false;
                }
            } else {
                lengthOk = false;
            }
        }

        /// <summary>
        /// Creates a hex dump with up to 5 lines.  Two lines before the anchor point, then
        /// a gap, then two lines that start with the anchor.
        /// </summary>
        /// <param name="minFirst">Earliest position we're allowed to include.</param>
        /// <param name="anchorPos">Anchor point.</param>
        /// <param name="maxLast">Last position we're allowed to show (inclusive end).</param>
        /// <returns>Multi-line formatted string.</returns>
        private string CreateSplitHexDump(long minFirst, long anchorPos, long maxLast) {
            const long AND_16_MASK = ~0x0f;

            Debug.Assert(minFirst <= anchorPos && anchorPos <= maxLast + 1 && minFirst <= maxLast);
            Debug.Assert(minFirst >= 0);
            Debug.Assert(maxLast < mFileLength);

            StringBuilder sb = new StringBuilder(5 * 64);
            byte[] dataBuf = new byte[32];

            // We show two lines of hex dump before the anchor, so we need up to 32 bytes.
            long firstPos = Math.Max(anchorPos - 32, minFirst);
            firstPos = (firstPos + 15) & AND_16_MASK;
            long chunkLen = anchorPos - firstPos;

            mFileStream.Seek(firstPos, SeekOrigin.Begin);
            int actual = mFileStream.Read(dataBuf, 0, dataBuf.Length);
            Debug.Assert(chunkLen <= actual);

            if (chunkLen <= 0) {
                // no pre-anchor data
                sb.AppendLine(string.Empty);
                sb.AppendLine(string.Empty);
            } else {
                long pos = firstPos;
                long lineLen = 16 - (pos & 0x0f);
                if (lineLen >= chunkLen) {
                    // top part fits on a single line; do it on the next one
                    lineLen = chunkLen;
                    sb.AppendLine(string.Empty);
                } else {
                    mFormatter.FormatHexDump(dataBuf, (int)(pos - firstPos), (int)pos,
                        (int)lineLen, sb);
                    sb.Append("\r\n");
                    //sb.AppendLine(pos.ToString("x4") + ": " + lineLen);
                    pos += lineLen;
                    lineLen = chunkLen - lineLen;
                }

                mFormatter.FormatHexDump(dataBuf, (int)(pos - firstPos), (int)pos,
                    (int)lineLen, sb);
                sb.Append("\r\n");
                //sb.AppendLine(pos.ToString("x4") + ": " + lineLen);
            }

            sb.AppendLine("------");

            // We show two lines of hex dump with the anchor, so we need up to 31 bytes
            // following it.
            //
            // NOTE: anchorPos is inclusive and represents the first byte contained in the
            // range.  If we're doing the hex dump for the end of the file, the value can be
            // one greater than maxLast if the pre-anchor range runs to EOF.
            long lastPos = ((anchorPos + 32) & AND_16_MASK) - 1;    // inclusive end
            lastPos = Math.Min(lastPos, maxLast);
            chunkLen = lastPos - anchorPos + 1;

            mFileStream.Seek(anchorPos, SeekOrigin.Begin);
            actual = mFileStream.Read(dataBuf, 0, dataBuf.Length);
            Debug.Assert(chunkLen <= actual);

            if (chunkLen <= 0) {
                // anchor not visible
                sb.AppendLine(string.Empty);
                sb.AppendLine(string.Empty);
            } else {
                long pos = anchorPos;
                long lineLen = 16 - (pos & 0x0f);
                if (lineLen > chunkLen) {
                    lineLen = chunkLen;
                }
                mFormatter.FormatHexDump(dataBuf, (int)(pos - anchorPos), (int)pos,
                    (int)lineLen, sb);
                sb.Append("\r\n");
                //sb.AppendLine(pos.ToString("x4") + ": " + lineLen);
                pos += lineLen;
                lineLen = chunkLen - lineLen;

                if (lineLen <= 0) {
                    sb.AppendLine(string.Empty);
                } else {
                    mFormatter.FormatHexDump(dataBuf, (int)(pos - anchorPos), (int)pos,
                        (int)lineLen, sb);
                    sb.Append("\r\n");
                    //sb.AppendLine(pos.ToString("x4") + ": " + lineLen);
                }
            }

            return sb.ToString();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog fileDlg = new SaveFileDialog() {
                Filter = Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 0,
                ValidateNames = true,
                FileName = "slice.bin"
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            string pathName = Path.GetFullPath(fileDlg.FileName);
            Debug.WriteLine("OUTPUT TO " + pathName);

            try {
                ParseStartLength(out bool startOk, out long sliceStart,
                    out bool lengthOk, out long sliceLength);
                if (!(startOk && lengthOk)) {
                    throw new Exception("Internal error: start/length invalid");
                }

                using (FileStream outStream = new FileStream(pathName, FileMode.Create)) {
                    mFileStream.Seek(sliceStart, SeekOrigin.Begin);
                    CopyStreamToStream(mFileStream, outStream, sliceLength);
                }
            } catch (Exception ex) {
                string ecaption = (string)FindResource("str_FileAccessFailedCaption");
                string efmt = (string)FindResource("str_FileAccessFailedFmt");
                MessageBox.Show(string.Format(efmt, ex.Message), ecaption,
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string caption = (string)FindResource("str_SuccessCaption");
            string msg = (string)FindResource("str_SuccessMsg");
            MessageBox.Show(msg, caption, MessageBoxButton.OK, MessageBoxImage.None);
        }

        private static void CopyStreamToStream(Stream inStream, Stream outStream, long length) {
            byte[] buffer = new byte[256 * 1024];

            while (length > 0) {
                int getLen = (int)Math.Min(length, buffer.Length);

                int actual = inStream.Read(buffer, 0, getLen);
                if (actual != getLen) {
                    throw new IOException("Read failed: requested " + getLen + ", got " + actual);
                }
                outStream.Write(buffer, 0, getLen);

                length -= getLen;
            }
        }
    }
}
