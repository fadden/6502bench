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

using Asm65;

namespace SourceGen.WpfGui {
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
        /// Unique tag, for non-unique label creation.  (Currently using offset.)
        /// </summary>
        private int mUniqueTag;

        /// <summary>
        /// Address we are editing the label for.  This is needed when creating the Symbol result.
        /// </summary>
        private int mAddress;

        /// <summary>
        /// Reference to DisasmProject's SymbolTable.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Label formatter.
        /// </summary>
        private Formatter mFormatter;

        private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;

        /// <summary>
        /// Recursion guard.
        /// </summary>
        private bool mInUpdateControls;

        public string NonUniqueButtonLabel { get; private set; }

        /// <summary>
        /// Set to true when input is valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        /// <summary>
        /// Property backing the text in the text entry box.
        /// </summary>
        public string LabelText {
            get { return mLabelText; }
            set { mLabelText = value; OnPropertyChanged(); UpdateControls(); }
        }
        string mLabelText;

        public string EnterLabelText {
            get { return mEnterLabelText; }
            set { mEnterLabelText = value; OnPropertyChanged(); }
        }
        string mEnterLabelText;

        // Radio buttons.
        public bool mIsNonUniqueChecked, mIsNonUniqueEnabled;
        public bool IsNonUniqueChecked {
            get { return mIsNonUniqueChecked; }
            set { mIsNonUniqueChecked = value; OnPropertyChanged(); UpdateControls(); }
        }
        public bool IsNonUniqueEnabled {
            get { return mIsNonUniqueEnabled; }
            set { mIsNonUniqueEnabled = value; OnPropertyChanged(); }
        }

        public bool mIsLocalChecked, mIsLocalEnabled;
        public bool IsLocalChecked {
            get { return mIsLocalChecked; }
            set { mIsLocalChecked = value; OnPropertyChanged(); UpdateControls(); }
        }
        public bool IsLocalEnabled {
            get { return mIsLocalEnabled; }
            set { mIsLocalEnabled = value; OnPropertyChanged(); }
        }

        public bool mIsGlobalChecked, mIsGlobalEnabled;
        public bool IsGlobalChecked {
            get { return mIsGlobalChecked; }
            set { mIsGlobalChecked = value; OnPropertyChanged(); UpdateControls(); }
        }
        public bool IsGlobalEnabled {
            get { return mIsGlobalEnabled; }
            set { mIsGlobalEnabled = value; OnPropertyChanged(); }
        }

        public bool mIsExportedChecked, mIsExportedEnabled;
        public bool IsExportedChecked {
            get { return mIsExportedChecked; }
            set { mIsExportedChecked = value; OnPropertyChanged(); UpdateControls(); }
        }
        public bool IsExportedEnabled {
            get { return mIsExportedEnabled; }
            set { mIsExportedEnabled = value; OnPropertyChanged(); }
        }

        public Visibility NonAddrWarningVis { get; private set; }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditLabel(Window owner, Symbol origSym, int address, int uniqueTag,
                SymbolTable symbolTable, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            LabelSym = origSym;
            mAddress = address;
            mUniqueTag = uniqueTag;
            mSymbolTable = symbolTable;
            mFormatter = formatter;

            string fmt = (string)FindResource("str_NonUniqueLocalFmt");
            NonUniqueButtonLabel = string.Format(fmt, mFormatter.NonUniqueLabelPrefix);
            fmt = (string)FindResource("str_EnterLabelFmt");
            EnterLabelText = string.Format(fmt, formatter.FormatAddress(address, address > 0xffff));

            if (mAddress == Address.NON_ADDR) {
                NonAddrWarningVis = Visibility.Visible;
            } else {
                NonAddrWarningVis = Visibility.Collapsed;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            IsNonUniqueEnabled = IsLocalEnabled = IsGlobalEnabled = IsExportedEnabled = true;

            if (LabelSym == null) {
                LabelText = string.Empty;
                IsGlobalChecked = true;
            } else {
                LabelText = LabelSym.GenerateDisplayLabel(mFormatter);
                switch (LabelSym.SymbolType) {
                    case Symbol.Type.NonUniqueLocalAddr:
                        IsNonUniqueChecked = true;
                        break;
                    case Symbol.Type.LocalOrGlobalAddr:
                        if (LabelSym.SymbolSource == Symbol.Source.Auto ||
                                LabelSym.LabelAnno == Symbol.LabelAnnotation.Generated) {
                            // Default to global, otherwise you get different behavior when
                            // adding a new label vs. replacing an auto or generated label.
                            IsGlobalChecked = true;
                        } else {
                            IsLocalChecked = true;
                        }
                        break;
                    case Symbol.Type.GlobalAddr:
                        IsGlobalChecked = true;
                        break;
                    case Symbol.Type.GlobalAddrExport:
                        IsExportedChecked = true;
                        break;
                    default:
                        Debug.Assert(false);    // WTF
                        IsGlobalChecked = true;
                        break;
                }
            }

            UpdateControls();
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            labelTextBox.SelectAll();
            labelTextBox.Focus();
        }

        private void UpdateControls() {
            if (mInUpdateControls) {
                return;
            }
            mInUpdateControls = true;

            LabelTextChanged();

            mInUpdateControls = false;
        }

        private bool mHadNonUniquePrefix = false;

        private void LabelTextChanged() {
            bool isBlank = (LabelText.Length == 0);

            // Strip leading non-unique prefix and the trailing annotation.
            string trimLabel = Symbol.TrimAndValidateLabel(LabelText,
                mFormatter.NonUniqueLabelPrefix, out bool isValid, out bool isLenValid,
                out bool isFirstCharValid, out bool hasNonUniquePrefix,
                out Symbol.LabelAnnotation anno);

            // If they type '@'/':'/'.' at the start of the label, switch the radio button.
            // Alternatively, if they choose a different radio button, remove the prefix.
            // We only want to do this on the first event so we don't wedge the control.
            if (hasNonUniquePrefix && !mHadNonUniquePrefix && !IsNonUniqueChecked) {
                IsNonUniqueChecked = true;
            } else if (hasNonUniquePrefix && mHadNonUniquePrefix && !IsNonUniqueChecked) {
                LabelText = LabelText.Substring(1);
                hasNonUniquePrefix = false;
            }
            mHadNonUniquePrefix = hasNonUniquePrefix;

            if (isBlank || isLenValid) {
                maxLengthLabel.Foreground = mDefaultLabelColor;
            } else {
                maxLengthLabel.Foreground = Brushes.Red;
            }
            if (isBlank || isFirstCharValid) {
                firstLetterLabel.Foreground = mDefaultLabelColor;
            } else {
                firstLetterLabel.Foreground = Brushes.Red;
            }
            if (isBlank || isValid) {
                // TODO(maybe): if the problem is that the label starts with a number, we
                //   shouldn't light up this (which is the "valid chars are" label) as well.
                validCharsLabel.Foreground = mDefaultLabelColor;
            } else {
                validCharsLabel.Foreground = Brushes.Red;
            }

#if false
            if (hasNonUniqueTag) {
                IsNonUniqueChecked = true;
                IsLocalEnabled = IsGlobalEnabled = IsExportedEnabled = false;
            } else {
                IsNonUniqueEnabled = IsLocalEnabled = IsGlobalEnabled = IsExportedEnabled = true;
            }
#endif

            // Refuse to continue if the label already exists and this isn't a non-unique label.
            // The only exception is if it's the same symbol, and it's user-defined.  (If
            // they're trying to edit an auto label, we want to force them to change the name.)
            //
            // NOTE: if label matching is case-insensitive, we want to allow a situation
            // where a label is being renamed from "FOO" to "Foo".  We should be able to
            // test for object equality on the Symbol to determine if we're renaming a
            // symbol to itself.
            if (!IsNonUniqueChecked && isValid &&
                    mSymbolTable.TryGetValue(trimLabel, out Symbol sym) &&
                    (sym != LabelSym || LabelSym.SymbolSource != Symbol.Source.User)) {
                isValid = false;
                notDuplicateLabel.Foreground = Brushes.Red;
            } else if (IsNonUniqueChecked) {
                notDuplicateLabel.Foreground = Brushes.Gray;
            } else {
                notDuplicateLabel.Foreground = mDefaultLabelColor;
            }

            IsValid = isBlank || isValid;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (string.IsNullOrEmpty(LabelText)) {
                LabelSym = null;
            } else {
                Symbol.Type symbolType;
                if (IsNonUniqueChecked) {
                    symbolType = Symbol.Type.NonUniqueLocalAddr;
                } else if (IsLocalChecked == true) {
                    symbolType = Symbol.Type.LocalOrGlobalAddr;
                } else if (IsGlobalChecked == true) {
                    symbolType = Symbol.Type.GlobalAddr;
                } else if (IsExportedChecked == true) {
                    symbolType = Symbol.Type.GlobalAddrExport;
                } else {
                    Debug.Assert(false);        // WTF
                    symbolType = Symbol.Type.GlobalAddr;
                }

                // Parse and strip the annotation and optional non-unique tag.
                string trimLabel = Symbol.TrimAndValidateLabel(LabelText,
                    mFormatter.NonUniqueLabelPrefix, out bool unused1, out bool unused2,
                    out bool unused3, out bool hasNonUniquePrefix,
                    out Symbol.LabelAnnotation anno);

                if (IsNonUniqueChecked) {
                    LabelSym = new Symbol(trimLabel, mAddress, anno, mUniqueTag);
                } else {
                    Debug.Assert(!hasNonUniquePrefix);
                    LabelSym = new Symbol(trimLabel, mAddress, Symbol.Source.User, symbolType,
                        anno);
                }
            }
            DialogResult = true;
        }
    }
}
