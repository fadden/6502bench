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
using System.Windows.Controls;
using System.Windows.Media;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Comment editor.
    /// </summary>
    public partial class EditComment : Window, INotifyPropertyChanged {
        private const int RECOMMENDED_MAX_LENGTH = 52;

        /// <summary>
        /// Edited comment string.  Will be empty if the comment is to be deleted.
        /// </summary>
        public string CommentText {
            get { return mCommentText; }
            set {
                mCommentText = value;
                OnPropertyChanged();
            }
        }
        private string mCommentText;

        private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditComment(Window owner, string comment) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            CommentText = comment;
        }

        public void Window_ContentRendered(object sender, EventArgs e) {
            commentTextBox.SelectAll();
            commentTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void CommentTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (!CommonUtil.TextUtil.IsPrintableAscii(commentTextBox.Text)) {
                asciiOnlyLabel.Foreground = Brushes.Red;
            } else {
                asciiOnlyLabel.Foreground = mDefaultLabelColor;
            }
            if (commentTextBox.Text.Length > RECOMMENDED_MAX_LENGTH) {
                maxLengthLabel.Foreground = Brushes.Red;
            } else {
                maxLengthLabel.Foreground = mDefaultLabelColor;
            }
        }
    }
}
