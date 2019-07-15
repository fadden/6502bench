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
using System.Windows.Input;

namespace SourceGenWPF.Tools.WpfGui {
    /// <summary>
    /// Simple text display dialog.  Can be modal or modeless.
    /// </summary>
    public partial class ShowText : Window, INotifyPropertyChanged {
        /// <summary>
        /// Text to display in the window.  May be updated at any time.  Bound to dialog property.
        /// </summary>
        public string DisplayText {
            get { return mDisplayText; }
            set {
                mDisplayText = value;
                OnPropertyChanged();
            }
        }
        private string mDisplayText;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.  Pass in an owner for modal dialogs, or null for modeless.
        /// </summary>
        public ShowText(Window owner, string initialText) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            if (owner == null) {
                // Modeless dialogs can get lost, so show them in the task bar.
                ShowInTaskbar = true;
            }

            DisplayText = initialText;
        }

        // Catch ESC key.
        private void Window_KeyEventHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                Close();
            }
        }
    }
}
