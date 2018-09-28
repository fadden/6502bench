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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SourceGen.AppForms {
    public partial class EditLongComment : Form {
        /// <summary>
        /// Get or set the multi-line comment object.  On exit, will be set to null if
        /// the user wants to delete the comment.
        /// </summary>
        public MultiLineComment LongComment { get; set; }

        private Asm65.Formatter mFormatter;


        public EditLongComment(Asm65.Formatter formatter) {
            InitializeComponent();

            mFormatter = formatter;
            LongComment = new MultiLineComment(string.Empty);
        }

        private void EditLongComment_Load(object sender, EventArgs e) {
            Debug.Assert(LongComment != null);
            entryTextBox.Text = LongComment.Text;
            boxModeCheckBox.Checked = LongComment.BoxMode;

            maxWidthComboBox.SelectedIndex = 0;
            for (int i = 0; i < maxWidthComboBox.Items.Count; i++) {
                string item = (string) maxWidthComboBox.Items[i];
                if (int.Parse(item) == LongComment.MaxWidth) {
                    maxWidthComboBox.SelectedIndex = i;
                    break;
                }
            }

            FormatInput();
        }

        // Handle Ctrl+Enter.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == (Keys.Control | Keys.Enter)) {
                DialogResult = DialogResult.OK;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void EditLongComment_FormClosing(object sender, FormClosingEventArgs e) {
            if (string.IsNullOrEmpty(entryTextBox.Text)) {
                LongComment = null;
            } else {
                LongComment = CreateMLC();
            }
        }

        private void entryTextBox_TextChanged(object sender, EventArgs e) {
            FormatInput();
        }

        private void maxWidthComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            FormatInput();
        }

        private void boxModeCheckBox_CheckedChanged(object sender, EventArgs e) {
            FormatInput();
        }

        /// <summary>
        /// Creates a MultiLineComment from the current state of the dialog.
        /// </summary>
        /// <returns>New MultiLineComment object.  Returns null if the dialog is still
        ///   in the process of initializing.</returns>
        private MultiLineComment CreateMLC() {
            if (maxWidthComboBox.SelectedItem == null) {
                return null;    // still initializing
            }
            return new MultiLineComment(entryTextBox.Text, boxModeCheckBox.Checked,
                int.Parse((string)maxWidthComboBox.SelectedItem));
        }

        /// <summary>
        /// Formats entryTextBox.Text into displayTextBox.Text.
        /// </summary>
        private void FormatInput() {
            MultiLineComment mlc = CreateMLC();
            if (mlc == null) {
                return;
            }
            List<string> lines = mlc.FormatText(mFormatter, string.Empty);

            StringBuilder sb = new StringBuilder(entryTextBox.Text.Length + lines.Count * 2);
            //sb.AppendFormat("### got {0} lines\r\n", lines.Count);
            bool first = true;
            foreach (string line in lines) {
                if (first) {
                    first = false;
                } else {
                    sb.Append("\r\n");
                }
                sb.Append(line);
            }

            displayTextBox.Text = sb.ToString();
        }

        private void EditLongComment_HelpButtonClicked(object sender, System.ComponentModel.CancelEventArgs e) {
            HelpAccess.ShowHelp(HelpAccess.Topic.EditLongComment);
        }
    }
}
