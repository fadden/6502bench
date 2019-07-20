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

namespace SourceGen.WpfGui {
    /// <summary>
    /// Find text dialog.
    /// </summary>
    public partial class FindBox : Window, INotifyPropertyChanged {
        /// <summary>
        /// Text to find.  On success, holds the string searched for.  This is bound to the
        /// text field.
        /// </summary>
        public string TextToFind {
            get { return mTextToFind; }
            set {
                mTextToFind = value;
                OnPropertyChanged();
            }
        }
        private string mTextToFind;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.  Pass in the last string searched for, to use as the initial value.
        /// </summary>
        public FindBox(Window owner, string findStr) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            Debug.Assert(findStr != null);
            TextToFind = findStr;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            findTextBox.Focus();
            findTextBox.SelectAll();
        }

        /// <summary>
        /// Handles key events, looking for the escape key.
        /// </summary>
        /// <remarks>
        /// Required because we don't have a "cancel" button.  Thanks:
        /// https://stackoverflow.com/a/419615/294248
        /// </remarks>
        private void Window_KeyEventHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }
}
