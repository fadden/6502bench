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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Asm65;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Symbol edit dialog.
    /// </summary>
    public partial class EditDefSymbol : Window, INotifyPropertyChanged {
        /// <summary>
        /// Set to previous value before calling; may be null if creating a new symbol.
        /// Will be set to new value on OK result.
        /// </summary>
        public DefSymbol DefSym { get; set; }

        /// <summary>
        /// Set to true when all fields are valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set {
                mIsValid = value;
                OnPropertyChanged();
            }
        }
        private bool mIsValid;

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        private Formatter mNumFormatter;

        /// <summary>
        /// List of existing symbols, for uniqueness check.  The list will not be modified.
        /// </summary>
        private SortedList<string, DefSymbol> mDefSymbolList;

        // Saved off at dialog load time.
        private Brush mDefaultLabelColor;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditDefSymbol(Window owner, Formatter formatter,
                SortedList<string, DefSymbol> defList) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mNumFormatter = formatter;
            mDefSymbolList = defList;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mDefaultLabelColor = labelNotesLabel.Foreground;

            if (DefSym != null) {
                labelTextBox.Text = DefSym.Label;
                valueTextBox.Text = mNumFormatter.FormatValueInBase(DefSym.Value,
                    DefSym.DataDescriptor.NumBase);
                commentTextBox.Text = DefSym.Comment;

                if (DefSym.SymbolType == Symbol.Type.Constant) {
                    constantRadioButton.IsChecked = true;
                } else {
                    addressRadioButton.IsChecked = true;
                }
            } else {
                addressRadioButton.IsChecked = true;
            }

            labelTextBox.Focus();
            UpdateControls();
        }

        private void UpdateControls() {
            bool labelValid, labelUnique, valueValid;

            // Label must be valid and not already exist in project symbol list.  (It's okay
            // if it exists elsewhere.)
            labelValid = Asm65.Label.ValidateLabel(labelTextBox.Text);

            if (mDefSymbolList.TryGetValue(labelTextBox.Text, out DefSymbol existing)) {
                // It's okay if it's the same object.
                labelUnique = (existing == DefSym);
            } else {
                labelUnique = true;
            }

            // Value must be blank, meaning "erase any earlier definition", or valid value.
            // (Hmm... don't currently have a way to specify "no symbol" in DefSymbol.)
            //if (!string.IsNullOrEmpty(valueTextBox.Text)) {
            valueValid = ParseValue(out int unused1, out int unused2);
            //} else {
            //    valueValid = true;
            //}

            // TODO(maybe): do this the XAML way, with properties and Styles
            labelNotesLabel.Foreground = labelValid ? mDefaultLabelColor : Brushes.Red;
            labelUniqueLabel.Foreground = labelUnique ? mDefaultLabelColor : Brushes.Red;
            valueNotesLabel.Foreground = valueValid ? mDefaultLabelColor : Brushes.Red;

            IsValid = labelValid && labelUnique && valueValid;
        }

        private bool ParseValue(out int value, out int numBase) {
            string str = valueTextBox.Text;
            if (str.IndexOf('/') >= 0) {
                // treat as address
                numBase = 16;
                return Asm65.Address.ParseAddress(str, (1 << 24) - 1, out value);
            } else {
                return Asm65.Number.TryParseInt(str, out value, out numBase);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            bool isConstant = (constantRadioButton.IsChecked == true);

            ParseValue(out int value, out int numBase);
            FormatDescriptor.SubType subType = FormatDescriptor.GetSubTypeForBase(numBase);
            DefSym = new DefSymbol(labelTextBox.Text, value, Symbol.Source.Project,
                isConstant ? Symbol.Type.Constant : Symbol.Type.ExternalAddr,
                subType, commentTextBox.Text, string.Empty);

            DialogResult = true;
        }

        private void LabelTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateControls();
        }

        private void ValueTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateControls();
        }
    }
}
