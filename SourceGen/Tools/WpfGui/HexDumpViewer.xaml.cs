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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using Asm65;

namespace SourceGen.Tools.WpfGui {
    /// <summary>
    /// Hex dump viewer.
    /// </summary>
    public partial class HexDumpViewer : Window, INotifyPropertyChanged {
        /// <summary>
        /// Maximum length of data we will display.
        /// </summary>
        public const int MAX_LENGTH = 1 << 24;

        /// <summary>
        /// ItemsSource for list.
        /// </summary>
        public VirtualHexDump HexDumpLines { get; private set; }

        /// <summary>
        /// Formatter that handles the actual string formatting.
        ///
        /// There's currently no way to update this after the dialog is opened, which means
        /// we won't track changes to hex case preference if the app settings are updated.
        /// I'm okay with that.
        /// </summary>
        private Formatter mFormatter;


        /// <summary>
        /// If true, don't include non-ASCII characters in text area.  (Without this we might
        /// use Unicode bullets or other glyphs for unprintable text.)  Bound to a CheckBox.
        /// </summary>
        public bool AsciiOnlyDump {
            get { return mAsciiOnlyDump; }
            set {
                mAsciiOnlyDump = value;
                OnPropertyChanged();
                ReplaceFormatter();
            }
        }
        private bool mAsciiOnlyDump;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Character conversion modes.  These determine how we interpret bytes for the
        /// ASCII portion of the dump.
        /// </summary>
        public enum CharConvMode {
            Unknown = 0,
            PlainAscii,
            HighLowAscii
        }

        /// <summary>
        /// Character conversion combo box item.
        /// </summary>
        public class CharConvItem {
            public string Name { get; private set; }
            public CharConvMode Mode { get; private set; }

            public CharConvItem(string name, CharConvMode mode) {
                Name = name;
                Mode = mode;
            }
        }
        public CharConvItem[] CharConvItems { get; private set; }


        public HexDumpViewer(Window owner, byte[] data, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            Debug.Assert(data.Length <= MAX_LENGTH);

            HexDumpLines = new VirtualHexDump(data, formatter);
            mFormatter = formatter;

            CharConvItems = new CharConvItem[] {
                new CharConvItem((string)FindResource("str_PlainAscii"),
                    CharConvMode.PlainAscii),
                new CharConvItem((string)FindResource("str_HighLowAscii"),
                    CharConvMode.HighLowAscii),
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Restore ASCII-only setting.
            AsciiOnlyDump = AppSettings.Global.GetBool(AppSettings.HEXD_ASCII_ONLY, false);

            // Restore conv mode setting.
            CharConvMode mode = (CharConvMode)AppSettings.Global.GetEnum(
                AppSettings.HEXD_CHAR_CONV, typeof(CharConvMode), (int)CharConvMode.PlainAscii);
            int index = 0;
            for (int i = 0; i < CharConvItems.Length; i++) {
                if (CharConvItems[i].Mode == mode) {
                    index = i;
                    break;
                }
            }
            charConvComboBox.SelectedIndex = index;
        }

        //private void Window_Closing(object sender, EventArgs e) {
        //    Debug.WriteLine("Window width: " + ActualWidth);
        //    Debug.WriteLine("Column width: " + hexDumpData.Columns[0].ActualWidth);
        //}

        /// <summary>
        /// Sets the filename associated with the data.  This is for display purposes only.
        /// </summary>
        public void SetFileName(string fileName) {
            Title = fileName;
        }

        private void CharConvComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            ReplaceFormatter();
        }

        private void ReplaceFormatter() {
            Formatter.FormatConfig config = mFormatter.Config;

            CharConvItem item = (CharConvItem)charConvComboBox.SelectedItem;
            if (item == null) {
                // initializing
                return;
            }

            switch (item.Mode) {
                case CharConvMode.PlainAscii:
                    config.mHexDumpCharConvMode = Formatter.FormatConfig.CharConvMode.PlainAscii;
                    break;
                case CharConvMode.HighLowAscii:
                    config.mHexDumpCharConvMode = Formatter.FormatConfig.CharConvMode.HighLowAscii;
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            config.mHexDumpAsciiOnly = AsciiOnlyDump;

            // Keep app settings up to date.
            AppSettings.Global.SetBool(AppSettings.HEXD_ASCII_ONLY, mAsciiOnlyDump);
            AppSettings.Global.SetEnum(AppSettings.HEXD_CHAR_CONV, typeof(CharConvMode),
                (int)item.Mode);

            mFormatter = new Formatter(config);
            HexDumpLines.Reformat(mFormatter);
        }

        /// <summary>
        /// Sets the scroll position and selection to show the specified range.
        /// </summary>
        /// <param name="startOffset">First offset to show.</param>
        /// <param name="endOffset">Last offset to show.</param>
        public void ShowOffsetRange(int startOffset, int endOffset) {
            Debug.WriteLine("HexDumpViewer: show +" + startOffset.ToString("x6") + " - +" +
                endOffset.ToString("x6"));
            int startLine = startOffset / 16;
            int endLine = endOffset / 16;

            hexDumpData.SelectedItems.Clear();
            for (int i = startLine; i <= endLine; i++) {
                hexDumpData.SelectedItems.Add(HexDumpLines[i]);
            }

            // Make sure it's visible.
            hexDumpData.ScrollIntoView(HexDumpLines[endLine]);
            hexDumpData.ScrollIntoView(HexDumpLines[startLine]);
            hexDumpData.Focus();
        }

#if false   // DataGrid provides this automatically
        /// <summary>
        /// Generates a string for every selected line, then copies the full thing to the
        /// clipboard.
        /// </summary>
        private void CopySelectionToClipboard() {
            ListView.SelectedIndexCollection indices = hexDumpListView.SelectedIndices;
            if (indices.Count == 0) {
                Debug.WriteLine("Nothing selected");
                return;
            }

            // Try to make the initial allocation big enough to hold the full thing.
            // Each line is currently 73 bytes, plus we throw in a CRLF.  Doesn't have to
            // be exact.  With a 16MB max file size we're creating a ~75MB string for the
            // clipboard, which .NET and Win10-64 seem to be able to handle.
            StringBuilder sb = new StringBuilder(indices.Count * (73 + 2));

            try {
                Application.UseWaitCursor = true;
                Cursor.Current = Cursors.WaitCursor;

                foreach (int index in indices) {
                    mFormatter.FormatHexDump(mData, index * 16, sb);
                    sb.Append("\r\n");
                }
            } finally {
                Application.UseWaitCursor = false;
                Cursor.Current = Cursors.Arrow;
            }

            Clipboard.SetText(sb.ToString(), TextDataFormat.Text);
        }
#endif
    }
}
