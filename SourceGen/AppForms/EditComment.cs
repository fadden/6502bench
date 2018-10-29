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
using System.Drawing;
using System.Windows.Forms;

namespace SourceGen.AppForms {
    public partial class EditComment : Form {
        /// <summary>
        /// Edited comment string.  Will be empty if the comment is to be deleted.
        /// </summary>
        public string Comment { get; private set; }

        private string mNumCharsFormat;

        private Color mDefaultLabelColor;

        private const int RECOMMENDED_MAX_LENGTH = 52;


        public EditComment(string comment) {
            InitializeComponent();

            // The initial label string is used as the format arg.
            mNumCharsFormat = numCharsLabel.Text;

            // Remember the default color.
            mDefaultLabelColor = asciiOnlyLabel.ForeColor;

            Debug.Assert(comment != null);
            commentTextBox.Text = comment;
        }

        private void EditComment_Load(object sender, EventArgs e) {
            UpdateLengthLabel();
        }

        private void UpdateLengthLabel() {
            numCharsLabel.Text = string.Format(mNumCharsFormat, commentTextBox.Text.Length);
        }

        private void commentTextBox_TextChanged(object sender, EventArgs e) {
            UpdateLengthLabel();

            if (!CommonUtil.TextUtil.IsPrintableAscii(commentTextBox.Text)) {
                asciiOnlyLabel.ForeColor = Color.Red;
            } else {
                asciiOnlyLabel.ForeColor = mDefaultLabelColor;
            }
            if (commentTextBox.Text.Length > RECOMMENDED_MAX_LENGTH) {
                maxLengthLabel.ForeColor = Color.Red;
            } else {
                maxLengthLabel.ForeColor = mDefaultLabelColor;
            }
        }

        private void okButton_Click(object sender, EventArgs e) {
            Comment = commentTextBox.Text;
        }
    }
}
