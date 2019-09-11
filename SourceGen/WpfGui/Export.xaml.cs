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

namespace SourceGen.WpfGui {
    /// <summary>
    /// Export selection dialog.
    /// </summary>
    public partial class Export : Window, INotifyPropertyChanged {
        //
        // Numeric input fields, bound directly to TextBox.Text.  These rely on a TextChanged
        // field to update the IsValid flag, because the "set" method is only called when the
        // field contains a valid integer.
        //
        private int mAsmLabelColWidth;
        public int AsmLabelColWidth {
            get { return mAsmLabelColWidth; }
            set {
                if (mAsmLabelColWidth != value) {
                    mAsmLabelColWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        private int mAsmOpcodeColWidth;
        public int AsmOpcodeColWidth {
            get { return mAsmOpcodeColWidth; }
            set {
                if (mAsmOpcodeColWidth != value) {
                    mAsmOpcodeColWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        private int mAsmOperandColWidth;
        public int AsmOperandColWidth {
            get { return mAsmOperandColWidth; }
            set {
                if (mAsmOperandColWidth != value) {
                    mAsmOperandColWidth = value;
                    OnPropertyChanged();
                }
            }
        }
        private int mAsmCommentColWidth;
        public int AsmCommentColWidth {
            get { return mAsmCommentColWidth; }
            set {
                if (mAsmCommentColWidth != value) {
                    mAsmCommentColWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Valid flag, used to enable the "generate" buttons.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Export(Window owner) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // TODO
        }

        private void UpdateControls() {
            bool isValid = true;

            isValid &= !Validation.GetHasError(asmLabelColWidthTextBox);
            isValid &= !Validation.GetHasError(asmOpcodeColWidthTextBox);
            isValid &= !Validation.GetHasError(asmOperandColWidthTextBox);
            isValid &= !Validation.GetHasError(asmCommentColWidthTextBox);

            IsValid = isValid;
        }

        private void AsmColWidthTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateControls();
        }
    }
}
