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
using System.Drawing;
using System.Windows.Forms;

namespace SourceGen.AppForms {
    /// <summary>
    /// Display errors and warnings generated while attempting to open a project.
    /// </summary>
    public partial class ProjectLoadIssues : Form {
        /// <summary>
        /// Multi-line message for text box.
        /// </summary>
        public string Messages { get; set; }

        /// <summary>
        /// Enable or disable the Continue button.  Defaults to true.
        /// </summary>
        public bool CanContinue { get; set; }

        /// <summary>
        /// Enable or disable the Cancel button.  Defaults to true.
        /// </summary>
        public bool CanCancel { get; set; }


        public ProjectLoadIssues() {
            InitializeComponent();
            CanContinue = CanCancel = true;
        }

        private void ProjectLoadIssues_Load(object sender, EventArgs e) {
            messageTextBox.Text = Messages;

            if (!CanContinue) {
                okButton.Enabled = false;

                // No point warning them about invalid data if they can't continue.
                invalidDiscardLabel.Visible = false;
            }
            if (!CanCancel) {
                cancelButton.Enabled = false;

                // They're stuck with the problem.
                invalidDiscardLabel.Visible = false;
            }
        }
    }
}
