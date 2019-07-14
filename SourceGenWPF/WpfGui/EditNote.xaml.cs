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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Note editor.
    /// </summary>
    public partial class EditNote : Window, INotifyPropertyChanged {
        /// <summary>
        /// Note result object.  Will be set to null if the note was deleted.
        /// </summary>
        public MultiLineComment Note { get; private set; }

        /// <summary>
        /// Text input by user.  This is bound to the input TextBox.
        /// </summary>
        public string UserInput {
            get { return mUserInput; }
            set {
                mUserInput = value;
                OnPropertyChanged();
            }
        }
        private string mUserInput;

        // Highlight color palette.  Unless the user has funky theme, the color will be
        // replacing a white background, and will be overlaid with black text, so should
        // be on the lighter end of the spectrum.
        private enum ColorList {
            None = 0, Green, Blue, Yellow, Pink, Orange
        }
        private static Color[] sColors = new Color[] {
            Color.FromArgb(0, 0, 0, 0),     // no highlight
            Colors.LightGreen,
            Colors.LightBlue,
            Colors.Yellow, //LightGoldenrodYellow,
            Colors.LightPink,
            Colors.Orange
        };
        private RadioButton[] mColorButtons;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditNote(Window owner, MultiLineComment note) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            Note = note;

            mColorButtons = new RadioButton[] {
                colorDefaultRadio,
                colorGreenRadio,
                colorBlueRadio,
                colorYellowRadio,
                colorPinkRadio,
                colorOrangeRadio
            };
            Debug.Assert(mColorButtons.Length == sColors.Length);
        }

        public void Window_Loaded(object sender, RoutedEventArgs e) {
            UserInput = Note.Text;

            // Configure radio buttons.
            colorDefaultRadio.IsChecked = true;
            if (Note != null) {
                Color curColor = Note.BackgroundColor;
                for (int i = 0; i < sColors.Length; i++) {
                    if (sColors[i].Equals(curColor)) {
                        mColorButtons[i].IsChecked = true;
                        break;
                    }
                }
            }

        }

        // Handle Ctrl+Enter as a way to close the dialog, since plain Enter just
        // moves to a new line.
        private void CloseCmd_Executed(object sender, ExecutedRoutedEventArgs e) {
            DialogResult = true;
        }
        public void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void Window_Closing(object sender, CancelEventArgs e) {
            if (DialogResult != true) {
                Debug.WriteLine("Skip it");
                return;
            }

            if (string.IsNullOrEmpty(UserInput)) {
                Note = null;
            } else {
                Color bkgndColor = Colors.Fuchsia;
                for (int i = 0; i < mColorButtons.Length; i++) {
                    if (mColorButtons[i].IsChecked == true) {
                        bkgndColor = sColors[i];
                        break;
                    }
                }
                Note = new MultiLineComment(UserInput, bkgndColor);
            }
        }
    }
}
