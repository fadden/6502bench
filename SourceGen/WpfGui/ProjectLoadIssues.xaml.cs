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

namespace SourceGen.WpfGui {
    /// <summary>
    /// Display errors and warnings generated while attempting to open a project.
    /// </summary>
    public partial class ProjectLoadIssues : Window, INotifyPropertyChanged {
        public bool WantReadOnly {
            get { return mWantReadOnly; }
            set { mWantReadOnly = value; OnPropertyChanged(); }
        }
        private bool mWantReadOnly;

        public bool ShowSaveWarning {
            get { return mShowItemWarning; }
            set { mShowItemWarning = value; OnPropertyChanged(); }
        }
        private bool mShowItemWarning;

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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public ProjectLoadIssues(Window owner, string msgs, Buttons allowedButtons) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mMessages = msgs;
            mAllowedButtons = allowedButtons;
        }

        private void ProjectLoadIssues_Loaded(object sender, RoutedEventArgs e) {
            messageTextBox.Text = mMessages;

            if (mAllowedButtons == Buttons.Cancel) {
                // Continue not allowed
                okButton.IsEnabled = false;

                // No point warning them about invalid data if they can't continue.
                ShowSaveWarning = false;
            } else if (mAllowedButtons == Buttons.Continue) {
                // Cancel not allowed.
                cancelButton.IsEnabled = false;

                // Problem is outside the scope of the project (e.g. bad platform symbol file),
                // so saving the project won't change anything.
                ShowSaveWarning = false;
            } else {
                Debug.Assert(mAllowedButtons == Buttons.ContinueOrCancel);
                ShowSaveWarning = true;
                WantReadOnly = true;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
