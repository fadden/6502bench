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
    /// <summary>
    /// Display errors and warnings generated while attempting to open a project.
    /// </summary>
    public partial class ProjectLoadIssues : Form {
        /// <summary>
        /// Multi-line message for text box.
        /// </summary>
        private string mMessages;

        /// <summary>
        /// Which buttons are enabled.
        /// </summary>
        private Buttons mAllowedButtons;
        public enum Buttons {
            Unknown = 0, Continue, Cancel, ContinueOrCancel
        }


        public ProjectLoadIssues(string msgs, Buttons allowedButtons) {
            InitializeComponent();

            mMessages = msgs;
            mAllowedButtons = allowedButtons;
        }

        private void ProjectLoadIssues_Load(object sender, EventArgs e) {
            messageTextBox.Text = mMessages;

            if (mAllowedButtons == Buttons.Cancel) {
                // Continue not allowed
                okButton.Enabled = false;

                // No point warning them about invalid data if they can't continue.
                invalidDiscardLabel.Visible = false;
            }
            if (mAllowedButtons == Buttons.Continue) {
                // Cancel not allowed.
                cancelButton.Enabled = false;

                // They're stuck with the problem.
                invalidDiscardLabel.Visible = false;
            }
        }
    }
}
