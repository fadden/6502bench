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
        /// Result; will be set non-null on OK.
        /// </summary>
        public DefSymbol NewSym { get; private set; }

        /// <summary>
        /// Set to true when all fields are valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        public string Label {
            get { return mLabel; }
            set { mLabel = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mLabel;

        public string Value {
            get { return mValue; }
            set { mValue = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mValue;

        public string VarWidth {
            get { return mWidth; }
            set { mWidth = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mWidth;

        public string WidthLimitLabel {
            get { return mWidthLimitLabel; }
            set { mWidthLimitLabel = value; OnPropertyChanged(); }
        }
        private string mWidthLimitLabel;

        public string Comment {
            get { return mComment; }
            set { mComment = value; OnPropertyChanged(); }
        }
        private string mComment;

        public bool IsAddress {
            get { return mIsAddress; }
            set { mIsAddress = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mIsAddress;

        public bool IsConstant {
            get { return mIsConstant; }
            set { mIsConstant = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mIsConstant;

        public string ConstantLabel {
            get { return mConstantLabel; }
            set { mConstantLabel = value; OnPropertyChanged(); }
        }
        private string mConstantLabel;

        public bool IsReadChecked {
            get { return mIsReadChecked; }
            set { mIsReadChecked = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mIsReadChecked;

        public bool IsWriteChecked {
            get { return mIsWriteChecked; }
            set { mIsWriteChecked = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mIsWriteChecked;

        public bool ReadOnlyValueAndType {
            get { return mReadOnlyValueAndType; }
            set { mReadOnlyValueAndType = value; OnPropertyChanged(); }
        }
        public bool NotReadOnlyValueAndType {
            get { return !mReadOnlyValueAndType; }
        }
        private bool mReadOnlyValueAndType;

        /// <summary>
        /// Set to true if we should create a Variable rather than a project symbol.
        /// </summary>
        public bool IsVariable {
            get { return mIsVariable; }
            set { mIsVariable = value; OnPropertyChanged(); }
        }
        public bool IsNotVariable {
            get { return !mIsVariable; }
        }
        private bool mIsVariable;

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        private Formatter mNumFormatter;

        /// <summary>
        /// Old symbol value.  May be null.
        /// </summary>
        private DefSymbol mOldSym;

        /// <summary>
        /// List of existing symbols, for uniqueness check.  The list will not be modified.
        /// </summary>
        private SortedList<string, DefSymbol> mDefSymbolList;

        /// <summary>
        /// Full symbol table, for extended uniqueness check.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Set to true if the width value is optional.
        /// </summary>
        private bool mIsWidthOptional;

        // Saved off at dialog load time.
        private Brush mDefaultLabelColor;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor, for editing a project or platform symbol.
        /// </summary>
        public EditDefSymbol(Window owner, Formatter formatter,
                SortedList<string, DefSymbol> defList, DefSymbol defSym,
                SymbolTable symbolTable)
            : this(owner, formatter, defList, defSym, symbolTable, false, false) { }

        /// <summary>
        /// Constructor, for editing a local variable, or editing a project symbol with
        /// the value field locked.
        /// </summary>
        /// <remarks>
        /// TODO(someday): disable the "constant" radio button unless CPU=65816.
        /// </remarks>
        public EditDefSymbol(Window owner, Formatter formatter,
                SortedList<string, DefSymbol> defList, DefSymbol defSym,
                SymbolTable symbolTable, bool isVariable, bool lockValueAndType) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mNumFormatter = formatter;
            mDefSymbolList = defList;
            mOldSym = defSym;
            mSymbolTable = symbolTable;
            IsVariable = isVariable;
            mReadOnlyValueAndType = lockValueAndType;

            Label = Value = VarWidth = Comment = string.Empty;

            int maxWidth;
            if (isVariable) {
                ConstantLabel = (string)FindResource("str_VariableConstant");
                maxWidth = 256;
            } else {
                ConstantLabel = (string)FindResource("str_ProjectConstant");
                maxWidth = 65536;
            }
            mIsWidthOptional = !isVariable;

            string fmt = (string)FindResource("str_WidthLimitFmt");
            WidthLimitLabel = string.Format(fmt, maxWidth);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mDefaultLabelColor = labelNotesLabel.Foreground;

            if (mOldSym != null) {
                Label = mOldSym.Label;
                Value = mNumFormatter.FormatValueInBase(mOldSym.Value,
                    mOldSym.DataDescriptor.NumBase);
                if (mOldSym.HasWidth) {
                    VarWidth = mOldSym.DataDescriptor.Length.ToString();
                }
                Comment = mOldSym.Comment;

                if (mOldSym.IsConstant) {
                    IsConstant = true;
                } else {
                    IsAddress = true;
                }

                if (mOldSym.Direction == DefSymbol.DirectionFlags.Read) {
                    IsReadChecked = true;
                } else if (mOldSym.Direction == DefSymbol.DirectionFlags.Write) {
                    IsWriteChecked = true;
                } else {
                    IsReadChecked = IsWriteChecked = true;
                }
            } else {
                IsAddress = IsReadChecked = IsWriteChecked = true;
            }

            UpdateControls();
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            labelTextBox.SelectAll();
            labelTextBox.Focus();
        }

        /// <summary>
        /// Validates input and updates controls appropriately.
        /// </summary>
        private void UpdateControls() {
            if (!IsLoaded) {
                return;
            }

            // Label must be valid and not already exist in the table we're editing.  (For project
            // symbols, it's okay if an identical label exists elsewhere.)
            bool labelValid = Asm65.Label.ValidateLabel(Label);
            bool labelUnique;

            // NOTE: should be using Asm65.Label.LABEL_COMPARER?
            if (mDefSymbolList.TryGetValue(Label, out DefSymbol existing)) {
                // It's okay if it's the same object.
                labelUnique = (existing == mOldSym);
            } else {
                labelUnique = true;
            }

            // For local variables, do a secondary uniqueness check across the full symbol table.
            if (labelUnique && mSymbolTable != null) {
                labelUnique = !mSymbolTable.TryGetValue(Label, out Symbol sym);

                // It's okay if this and the other are both variables.
                if (!labelUnique && IsVariable && sym.IsVariable) {
                    labelUnique = true;
                }
            }

            // Value must be blank, meaning "erase any earlier definition", or valid value.
            // (Hmm... don't currently have a way to specify "no symbol" in DefSymbol.)
            //if (!string.IsNullOrEmpty(valueTextBox.Text)) {
            bool valueValid = ParseValue(out int thisValue, out int unused2);
            //} else {
            //    valueValid = true;
            //}

            bool widthValid = true;
            int thisWidth = -1;
            if (IsConstant && !IsVariable) {
                // width field is ignored
            } else if (string.IsNullOrEmpty(VarWidth)) {
                // blank field is okay if the width is optional
                widthValid = mIsWidthOptional;
            } else if (!Asm65.Number.TryParseInt(VarWidth, out thisWidth, out int unusedBase) ||
                    thisWidth < DefSymbol.MIN_WIDTH || thisWidth > DefSymbol.MAX_WIDTH ||
                    (IsVariable && thisWidth > 256)) {
                // All widths must be between 1 and 65536.  For a variable, the full thing must
                // fit on zero page without wrapping.  We test for 256 here so that we highlight
                // the "bad width" label, rather than the "it doesn't fit on the page" label.
                widthValid = false;
            }

            bool valueRangeValid = true;
            if (IsVariable && valueValid && widthValid) {
                // $ff with width 1 is okay, $ff with width 2 is not
                if (thisValue < 0 || thisValue + thisWidth > 256) {
                    valueRangeValid = false;
                }
            } else if (IsAddress && valueValid) {
                // limit to positive 24-bit integers; use a long for value+width so we
                // don't get fooled by overflow
                long lvalue = thisValue;
                if (thisWidth > 0) {
                    lvalue += thisWidth - 1;
                }
                if (thisValue < 0 || lvalue > 0x00ffffff) {
                    valueRangeValid = false;
                }
            }

            Symbol.Type symbolType = IsConstant ? Symbol.Type.Constant : Symbol.Type.ExternalAddr;

            // For a variable, the value must also be unique within the table.  Values have
            // width, so we need to check for overlap.
            bool valueUniqueValid = true;
            if (IsVariable && valueValid && widthValid) {
                foreach (KeyValuePair<string, DefSymbol> kvp in mDefSymbolList) {
                    if (kvp.Value != mOldSym &&
                            DefSymbol.CheckOverlap(kvp.Value, thisValue, thisWidth, symbolType)) {
                        valueUniqueValid = false;
                        break;
                    }
                }
            }

            bool rwValid = true;
            if (!IsVariable && IsAddress) {
                rwValid = IsReadChecked || IsWriteChecked;
            }

            labelNotesLabel.Foreground = labelValid ? mDefaultLabelColor : Brushes.Red;
            labelUniqueLabel.Foreground = projectLabelUniqueLabel.Foreground =
                labelUnique ? mDefaultLabelColor : Brushes.Red;
            valueNotesLabel.Foreground = valueValid ? mDefaultLabelColor : Brushes.Red;
            addrValueRangeLabel.Foreground = valueRangeValid ? mDefaultLabelColor : Brushes.Red;
            varValueRangeLabel.Foreground = valueRangeValid ? mDefaultLabelColor : Brushes.Red;
            varValueUniqueLabel.Foreground = valueUniqueValid ? mDefaultLabelColor : Brushes.Red;
            widthNotesLabel.Foreground = widthValid ? mDefaultLabelColor : Brushes.Red;
            checkReadWriteLabel.Foreground = rwValid ? mDefaultLabelColor : Brushes.Red;

            IsValid = labelValid && labelUnique && valueValid && valueRangeValid &&
                valueUniqueValid && widthValid && rwValid;
        }

        private bool ParseValue(out int value, out int numBase) {
            string str = Value;
            if (str.IndexOf('/') >= 0) {
                // treat as address
                numBase = 16;
                return Asm65.Address.ParseAddress(str, (1 << 24) - 1, out value);
            } else {
                return Asm65.Number.TryParseInt(str, out value, out numBase);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            ParseValue(out int value, out int numBase);
            FormatDescriptor.SubType subType = FormatDescriptor.GetSubTypeForBase(numBase);
            int width = -1;
            if (IsConstant && !IsVariable) {
                // width field is ignored, don't bother parsing
            } else if (!string.IsNullOrEmpty(VarWidth)) {
                bool ok = Asm65.Number.TryParseInt(VarWidth, out width, out int unusedNumBase);
                Debug.Assert(ok);
            }

            DefSymbol.DirectionFlags direction;
            if (IsReadChecked && IsWriteChecked) {
                direction = DefSymbol.DirectionFlags.ReadWrite;
            } else if (IsReadChecked) {
                direction = DefSymbol.DirectionFlags.Read;
            } else if (IsWriteChecked) {
                direction = DefSymbol.DirectionFlags.Write;
            } else {
                Debug.Assert(false);
                direction = DefSymbol.DirectionFlags.None;
            }

            NewSym = new DefSymbol(Label, value,
                IsVariable ? Symbol.Source.Variable : Symbol.Source.Project,
                IsConstant ? Symbol.Type.Constant : Symbol.Type.ExternalAddr,
                subType, width, width > 0, Comment, direction, null, string.Empty);

            DialogResult = true;
        }
    }
}
