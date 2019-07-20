/*
 * Copyright 2018 faddenSoft
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
using System.Text;
using System.Windows.Forms;

using Asm65;
using CommonWinForms;

namespace SourceGenWF.Tools {
    /// <summary>
    /// Display a hex dump.
    /// </summary>
    public partial class HexDumpViewer : Form {
        /// <summary>
        /// Maximum length of data we will display.
        /// </summary>
        public const int MAX_LENGTH = 1 << 24;

        /// <summary>
        /// Character conversion mode.  The enum must match the items in the combo box.
        /// </summary>
        private enum CharConvMode {
            PlainAscii = 0,
            HighLowAscii
        }

        /// <summary>
        /// Data to display.  We currently require that the entire file fit in memory,
        /// which is reasonable because we impose a 2^24 (16MB) limit.
        /// </summary>
        private byte[] mData;

        /// <summary>
        /// Data formatter object.
        /// 
        /// There's currently no way to update this after the dialog is opened, which means
        /// we won't track changes to hex case preference.  I'm okay with that.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// If true, don't include non-ASCII characters in text area.
        /// </summary>
        private bool mAsciiOnlyDump;

        /// <summary>
        /// Subscribe to this to be notified when the dialog closes.
        /// </summary>
        public event WindowClosing OnWindowClosing;
        public delegate void WindowClosing(object sender, EventArgs e);


        public HexDumpViewer(byte[] data, Formatter formatter) {
            InitializeComponent();

            hexDumpListView.SetDoubleBuffered(true);

            Debug.Assert(data.Length <= MAX_LENGTH);
            mData = data;
            mFormatter = formatter;

            hexDumpListView.VirtualListSize = (mData.Length + 15) / 16;
        }

        private void HexDumpViewer_Load(object sender, EventArgs e) {
            topMostCheckBox.Checked = TopMost;

            // Configure ASCII-only mode.  Note this causes the CheckedChange callback to
            // fire, which sets the field and replaces the formatter.
            bool asciiOnly = AppSettings.Global.GetBool(AppSettings.HEXD_ASCII_ONLY, false);
            asciiOnlyCheckBox.Checked = asciiOnly;

            // Just save and restore the combo box index.  This might come up wrong after
            // an upgrade that shuffles the options, but doing it right isn't worth the effort.
            int charConv = AppSettings.Global.GetInt(AppSettings.HEXD_CHAR_CONV, 0);
            if (charConv >= 0 && charConv <= charConvComboBox.Items.Count) {
                charConvComboBox.SelectedIndex = charConv;
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == (Keys.Control | Keys.A)) {
                hexDumpListView.SelectAll();
                return true;
            } else if (keyData == (Keys.Control | Keys.C)) {
                CopySelectionToClipboard();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void asciiOnlyCheckBox_CheckedChanged(object sender, EventArgs e) {
            mAsciiOnlyDump = asciiOnlyCheckBox.Checked;
            AppSettings.Global.SetBool(AppSettings.HEXD_ASCII_ONLY, mAsciiOnlyDump);

            ReplaceFormatter();
            InvalidateListView();
        }

        private void topMostCheckBox_CheckedChanged(object sender, EventArgs e) {
            TopMost = topMostCheckBox.Checked;
        }

        private void charConvComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            Debug.WriteLine("charConvCombBox selected: " + charConvComboBox.SelectedIndex);
            AppSettings.Global.SetInt(AppSettings.HEXD_CHAR_CONV, charConvComboBox.SelectedIndex);

            ReplaceFormatter();
            InvalidateListView();
        }

        private void HexDumpViewer_FormClosed(object sender, FormClosedEventArgs e) {
            if (OnWindowClosing != null) {
                OnWindowClosing(this, e);
            }
        }

        /// <summary>
        /// Replaces the Formatter with a new one, using the current dialog configuration.
        /// </summary>
        private void ReplaceFormatter() {
            Formatter.FormatConfig config = mFormatter.Config;

            config.mHexDumpAsciiOnly = mAsciiOnlyDump;

            CharConvMode mode = (CharConvMode)charConvComboBox.SelectedIndex;
            switch (mode) {
                case CharConvMode.PlainAscii:
                    config.mHexDumpCharConvMode = Formatter.FormatConfig.CharConvMode.PlainAscii;
                    break;
                case CharConvMode.HighLowAscii:
                    config.mHexDumpCharConvMode = Formatter.FormatConfig.CharConvMode.HighLowAscii;
                    break;
                case (CharConvMode)(-1):
                    // this happens during dialog init, before combo box is configured
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }

            mFormatter = new Formatter(config);
        }

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

        /// <summary>
        /// Sets the scroll position to show the specified range.
        /// </summary>
        /// <param name="startOffset">First offset to show.</param>
        /// <param name="endOffset">Last offset to show.</param>
        public void ShowOffsetRange(int startOffset, int endOffset) {
            Debug.WriteLine("HexDumpViewer: show +" + startOffset.ToString("x6") + " - +" +
                endOffset.ToString("x6"));
            int startLine = startOffset / 16;
            int endLine = endOffset / 16;

            // TODO(someday): instead of selecting the lines, highlight the individual
            //   bytes.  This requires an owner-drawn ListView.
            hexDumpListView.SelectedIndices.Clear();
            for (int i = startLine; i <= endLine; i++) {
                hexDumpListView.SelectedIndices.Add(i);
            }
            hexDumpListView.EnsureVisible(endLine);
            hexDumpListView.EnsureVisible(startLine);
        }


        #region Virtual List View
        // Using a virtual list view means we're allocating objects frequently when the
        // list is being scrolled, but get to avoid massive trauma when opening a large file,
        // and don't have to turn a 16MB file into a 74+MB string collection.

        /// <summary>
        /// Cache of previously-constructed ListViewItems.  The ListView will request items
        /// continuously as they are moused-over, so this is fairly important.
        /// </summary>
        private ListViewItem[] mItemCache;
        private int mItemCacheFirst;

        private void hexDumpListView_RetrieveVirtualItem(object sender,
                RetrieveVirtualItemEventArgs e) {
            // Is item cached?
            if (mItemCache != null && e.ItemIndex >= mItemCacheFirst &&
                    e.ItemIndex < mItemCacheFirst + mItemCache.Length) {
                // Yes, return existing item.
                e.Item = mItemCache[e.ItemIndex - mItemCacheFirst];
            } else {
                // No, create item.
                e.Item = CreateListViewItem(e.ItemIndex);
            }
        }

        private void hexDumpListView_CacheVirtualItems(object sender,
                CacheVirtualItemsEventArgs e) {
            if (mItemCache != null && e.StartIndex >= mItemCacheFirst &&
                    e.EndIndex <= mItemCacheFirst + mItemCache.Length) {
                // Already have this span cached.
                return;
            }

            // Discard old cache, create new one, populate it.
            mItemCacheFirst = e.StartIndex;
            int len = e.EndIndex - e.StartIndex + 1;        // end is inclusive
            mItemCache = new ListViewItem[len];
            for (int i = 0; i < len; i++) {
                mItemCache[i] = CreateListViewItem(e.StartIndex + i);
            }
        }

        private ListViewItem CreateListViewItem(int index) {
            string fmtd = mFormatter.FormatHexDump(mData, index * 16);
            return new ListViewItem(fmtd);
        }

        /// <summary>
        /// Invalidates the contents of the list view, forcing a redraw.  Useful when
        /// the desired output format changes.
        /// </summary>
        private void InvalidateListView() {
            hexDumpListView.BeginUpdate();
            mItemCache = null;
            mItemCacheFirst = -1;
            hexDumpListView.EndUpdate();
        }

        #endregion Virtual List View
    }
}
