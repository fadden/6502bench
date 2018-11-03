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

namespace SourceGen.AppForms {
    /// <summary>
    /// Prompt the user before discarding changes.
    /// 
    /// Dialog result will be:
    ///   DialogResult.Yes: save before continuing
    ///   DialogResult.No: don't save before continuing
    ///   DialogResult.Cancel: don't continue
    /// </summary>
    public partial class DiscardChanges : Form {
        public DiscardChanges() {
            InitializeComponent();
        }

        private void DiscardChanges_Load(object sender, EventArgs e) {
            // Make this the default.
            cancelButton.Select();
        }

        private void saveButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void dontSaveButton_Click(object sender, EventArgs e) {
            DialogResult = DialogResult.No;
            Close();
        }
    }
}
