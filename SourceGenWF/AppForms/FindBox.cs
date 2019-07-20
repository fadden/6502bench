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
using System.Windows.Forms;

namespace SourceGenWF.AppForms {
    public partial class FindBox : Form {
        /// <summary>
        /// Text to find.  On success, holds the string searched for.
        /// </summary>
        public string TextToFind { get; private set; }


        public FindBox(string findStr) {
            InitializeComponent();

            TextToFind = findStr;
        }

        private void FindBox_Load(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(TextToFind)) {
                findTextBox.Text = TextToFind;
                findTextBox.SelectAll();
            }
        }

        // Without a "cancel" button, the escape key does nothing.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.Escape) {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void okButton_Click(object sender, EventArgs e) {
            TextToFind = findTextBox.Text;
        }
    }
}
