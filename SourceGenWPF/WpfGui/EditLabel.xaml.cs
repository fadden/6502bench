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
using System.Windows.Media;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Edit a label.
    /// </summary>
    public partial class EditLabel : Window, INotifyPropertyChanged {
        /// <summary>
        /// Symbol object.  When the dialog completes successfully,
        /// this will have the new symbol, or null if the user deleted the label.
        /// </summary>
        public Symbol LabelSym { get; private set; }

        /// <summary>
        /// Address we are editing the label for.
        /// </summary>
        private int mAddress;

        /// <summary>
        /// Reference to DisasmProject's SymbolTable.
        /// </summary>
        private SymbolTable mSymbolTable;

        // Dialog label text color, saved off at dialog load time.
        private Brush mDefaultLabelColor;

        /// <summary>
        /// Set to true when input is valid.  Controls whether the OK button is enabled.
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
        /// Property backing the text in the text entry box.
        /// </summary>
        public string LabelText {
            get { return mLabelText; }
            set {
                mLabelText = value;
                LabelTextBox_TextChanged();
                OnPropertyChanged();
            }
        }
        string mLabelText;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditLabel(Window owner, Symbol origSym, int address, SymbolTable symbolTable) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            LabelSym = origSym;
            mAddress = address;
            mSymbolTable = symbolTable;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mDefaultLabelColor = maxLengthLabel.Foreground;

            if (LabelSym == null) {
                LabelText = string.Empty;
                radioButtonLocal.IsChecked = true;
            } else {
                LabelText = LabelSym.Label;
                switch (LabelSym.SymbolType) {
                    case Symbol.Type.LocalOrGlobalAddr:
                        radioButtonLocal.IsChecked = true;
                        break;
                    case Symbol.Type.GlobalAddr:
                        radioButtonGlobal.IsChecked = true;
                        break;
                    case Symbol.Type.GlobalAddrExport:
                        radioButtonExport.IsChecked = true;
                        break;
                    default:
                        Debug.Assert(false);    // WTF
                        radioButtonLocal.IsChecked = true;
                        break;
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            labelTextBox.SelectAll();
            labelTextBox.Focus();
        }

        private void LabelTextBox_TextChanged() {
            string str = LabelText;
            bool valid = true;

            if (str.Length == 1 || str.Length > Asm65.Label.MAX_LABEL_LEN) {
                valid = false;
                maxLengthLabel.Foreground = Brushes.Red;
            } else {
                maxLengthLabel.Foreground = mDefaultLabelColor;
            }

            // Regex never matches on strings of length 0 or 1, but we don't want
            // to complain about that since we're already doing that above.
            // TODO(maybe): Ideally this wouldn't light up if the only problem was a
            //   non-alpha first character, since the next test will call that out.
            if (str.Length > 1) {
                if (!Asm65.Label.ValidateLabel(str)) {
                    valid = false;
                    validCharsLabel.Foreground = Brushes.Red;
                } else {
                    validCharsLabel.Foreground = mDefaultLabelColor;
                }
            } else {
                validCharsLabel.Foreground = mDefaultLabelColor;
            }

            if (str.Length > 0 &&
                    !((str[0] >= 'A' && str[0] <= 'Z') || (str[0] >= 'a' && str[0] <= 'z') ||
                      str[0] == '_')) {
                // This should have been caught by the regex.  We just want to set the
                // color on the "first character must be" instruction text.
                Debug.Assert(!valid);
                firstLetterLabel.Foreground = Brushes.Red;
            } else {
                firstLetterLabel.Foreground = mDefaultLabelColor;
            }

            // Refuse to continue if the label already exists.  The only exception is if
            // it's the same symbol, and it's user-defined.  (If they're trying to edit an
            // auto label, we want to force them to change the name.)
            //
            // NOTE: if label matching is case-insensitive, we want to allow a situation
            // where a label is being renamed from "FOO" to "Foo".  We should be able to
            // test for object equality on the Symbol to determine if we're renaming a
            // symbol to itself.
            if (valid && mSymbolTable.TryGetValue(str, out Symbol sym) &&
                    (sym != LabelSym || LabelSym.SymbolSource != Symbol.Source.User)) {
                valid = false;
                notDuplicateLabel.Foreground = Brushes.Red;
            } else {
                notDuplicateLabel.Foreground = mDefaultLabelColor;
            }

            IsValid = valid;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(LabelText)) {
                LabelSym = null;
            } else {
                Symbol.Type symbolType;
                if (radioButtonLocal.IsChecked == true) {
                    symbolType = Symbol.Type.LocalOrGlobalAddr;
                } else if (radioButtonGlobal.IsChecked == true) {
                    symbolType = Symbol.Type.GlobalAddr;
                } else if (radioButtonExport.IsChecked == true) {
                    symbolType = Symbol.Type.GlobalAddrExport;
                } else {
                    Debug.Assert(false);        // WTF
                    symbolType = Symbol.Type.LocalOrGlobalAddr;
                }
                LabelSym = new Symbol(LabelText, mAddress, Symbol.Source.User, symbolType);
            }
            DialogResult = true;
        }
    }
}
