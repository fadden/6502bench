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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using CommonWPF;

namespace SourceGen.WpfGui {
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
            set { mUserInput = value; OnPropertyChanged(); }
        }
        private string mUserInput;

        public string CustomColorStr {
            get { return mCustomColorStr; }
            set {
                mCustomColorStr = value;
                OnPropertyChanged();
                UpdateCustomColor();
                if (mIsInitialized) {
                    Debug.WriteLine("SET CUSTOM");
                    IsCustomChecked = true;
                }
            }
        }
        private string mCustomColorStr;

        public bool IsCustomChecked {
            get { return mIsCustomChecked; }
            set { mIsCustomChecked = value; OnPropertyChanged(); }
        }
        private bool mIsCustomChecked;

        public Brush CustomColorBrush {
            get { return mCustomColorBrush; }
            set { mCustomColorBrush = value; OnPropertyChanged(); }
        }
        private Brush mCustomColorBrush = Brushes.Transparent;

        // This is static so it carries over between edits.
        private static Color mCustomColor = Helper.ZeroColor;

        // Highlight color palette.  Unless the user has funky theme, the color will be
        // replacing a white background, and will be overlaid with black text, so should
        // be on the lighter end of the spectrum.
        private enum ColorList {
            None = 0, Green, Blue, Yellow, Pink, Orange
        }
        private static Color[] sColors = new Color[] {
            CommonWPF.Helper.ZeroColor,     // no highlight
            Colors.LightGreen,
            Colors.LightBlue,
            Colors.Yellow, //LightGoldenrodYellow,
            Colors.LightPink,
            Colors.Orange
        };
        private RadioButton[] mColorButtons;

        private bool mIsInitialized = false;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditNote(Window owner, MultiLineComment note) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            if (note == null) {
                Note = new MultiLineComment(string.Empty);
            } else {
                Note = note;
            }

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
                bool found = false;
                Color curColor = Note.BackgroundColor;
                for (int i = 0; i < sColors.Length; i++) {
                    if (sColors[i].Equals(curColor)) {
                        mColorButtons[i].IsChecked = true;
                        found = true;
                        break;
                    }
                }

                if (!found) {
                    IsCustomChecked = true;
                    mCustomColor = curColor;
                }
            }

            if (mCustomColor.A == 0xff) {
                CustomColorStr = string.Format("#{0:X2}{1:X2}{2:X2}",
                    mCustomColor.R, mCustomColor.G, mCustomColor.B);
            } else {
                CustomColorStr = string.Format("#{0:X2}{1:X2}{2:X2}{3:X2}",
                    mCustomColor.A, mCustomColor.R, mCustomColor.G, mCustomColor.B);
            }

            inputTextBox.Focus();
            mIsInitialized = true;
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
                return;
            }

            if (string.IsNullOrEmpty(UserInput)) {
                Note = null;
            } else {
                Color bkgndColor = Colors.Fuchsia;  // make it obvious if we screw up
                if (IsCustomChecked) {
                    if (Validation.GetHasError(customColorBox)) {
                        bkgndColor = Helper.ZeroColor;
                    } else {
                        bkgndColor = mCustomColor;
                    }
                } else {
                    for (int i = 0; i < mColorButtons.Length; i++) {
                        if (mColorButtons[i].IsChecked == true) {
                            bkgndColor = sColors[i];
                            break;
                        }
                    }
                }
                Note = new MultiLineComment(UserInput, bkgndColor);
            }
        }

        private void UpdateCustomColor() {
            Color cl;
            try {
                cl = (Color)ColorConverter.ConvertFromString(CustomColorStr);
            } catch (FormatException ex) {
                // no dice
                Debug.WriteLine("Unable to convert color '" + CustomColorStr + "': " + ex.Message);
                CustomColorBrush = Brushes.Transparent;
                mCustomColor = Helper.ZeroColor;
                return;
            }

            mCustomColor = cl;
            CustomColorBrush = new SolidColorBrush(cl);
        }
    }

    #region Validation rules

    public class ColorRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string strValue = Convert.ToString(value);
            if (strValue == null) {
                return new ValidationResult(false, "could not convert to string");
            }
            if (string.IsNullOrEmpty(strValue)) {
                return new ValidationResult(false, "empty color string");
            }

            try {
                ColorConverter.ConvertFromString(strValue);
                return ValidationResult.ValidResult;
            } catch (Exception) {
                return new ValidationResult(false, "invalid color value");
            }
        }
    }

    #endregion Validation rules
}
