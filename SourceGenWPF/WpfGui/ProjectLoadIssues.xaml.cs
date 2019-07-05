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
using System.Windows;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Display errors and warnings generated while attempting to open a project.
    /// </summary>
    public partial class ProjectLoadIssues : Window {
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


        public ProjectLoadIssues(Window owner, string msgs, Buttons allowedButtons) {
            InitializeComponent();
            Owner = owner;

            mMessages = msgs;
            mAllowedButtons = allowedButtons;
        }

        private void ProjectLoadIssues_Loaded(object sender, RoutedEventArgs e) {
            messageTextBox.Text = mMessages;

            if (mAllowedButtons == Buttons.Cancel) {
                // Continue not allowed
                okButton.IsEnabled = false;

                // No point warning them about invalid data if they can't continue.
                invalidDiscardLabel.Visibility = Visibility.Hidden;
            }
            if (mAllowedButtons == Buttons.Continue) {
                // Cancel not allowed.
                cancelButton.IsEnabled = false;

                // They're stuck with the problem.
                invalidDiscardLabel.Visibility = Visibility.Hidden;
            }
        }
    }
}
