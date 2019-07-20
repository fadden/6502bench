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
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Asm65;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Go to line dialog.
    /// </summary>
    public partial class GotoBox : Window, INotifyPropertyChanged {
        /// <summary>
        /// On success, this will hold the target offset.
        /// </summary>
        public int TargetOffset { get; private set; }

        /// <summary>
        /// Reference to project.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Reference to formatter.  This determines how values are displayed.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Set to true when input is valid.  Controls whether the "Go" button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set {
                mIsValid = value;
                OnPropertyChanged();
            }
        }
        private bool mIsValid;

        public string OffsetValueStr {
            get { return mOffsetValueStr; }
            set {
                mOffsetValueStr = value;
                OnPropertyChanged();
            }
        }
        private string mOffsetValueStr;

        public string AddressValueStr {
            get { return mAddressValueStr; }
            set {
                mAddressValueStr = value;
                OnPropertyChanged();
            }
        }
        private string mAddressValueStr;

        public string LabelValueStr {
            get { return mLabelValueStr; }
            set {
                mLabelValueStr = value;
                OnPropertyChanged();
            }
        }
        private string mLabelValueStr;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public GotoBox(Window owner, DisasmProject proj, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = proj;
            mFormatter = formatter;
            TargetOffset = -1;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            UpdateDisplay();
            targetTextBox.Focus();
        }

        // Catch ESC key.
        private void Window_KeyEventHandler(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                DialogResult = false;
            }
        }

        private void GoButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void TargetTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            ProcessInput();
            UpdateDisplay();
            IsValid = (TargetOffset >= 0);
        }

        private void ProcessInput() {
            TargetOffset = -1;

            string input = targetTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input)) {
                return;
            }
            if (input[0] == '+') {
                // this can only be an offset; convert as hexadecimal number
                try {
                    TargetOffset = Convert.ToInt32(input.Substring(1), 16);
                } catch (Exception) {
                }
                return;
            }

            // Try it as a label.  If they give the label a hex name (e.g. "A001") they
            // can prefix it with '$' to disambiguate the address.
            int labelOffset = mProject.FindLabelOffsetByName(input);
            if (labelOffset >= 0) {
                TargetOffset = labelOffset;
            } else if (Address.ParseAddress(input, 1 << 24, out int addr)) {
                // could be a valid address; check against address map
                int offset = mProject.AddrMap.AddressToOffset(0, addr);
                if (offset >= 0) {
                    TargetOffset = offset;
                }
            }
        }

        private void UpdateDisplay() {
            string offsetStr = string.Empty;
            string addressStr = string.Empty;
            string labelStr = string.Empty;

            if (TargetOffset >= 0) {
                offsetStr = mFormatter.FormatOffset24(TargetOffset);
                int addr = mProject.GetAnattrib(TargetOffset).Address;
                addressStr = mFormatter.FormatAddress(addr, addr > 0xffff);
                Symbol sym = mProject.GetAnattrib(TargetOffset).Symbol;
                if (sym != null) {
                    labelStr = sym.Label;
                }
            }

            OffsetValueStr = offsetStr;
            AddressValueStr = addressStr;
            LabelValueStr = labelStr;
        }
    }
}
