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
    /// Edit Address dialog.
    /// </summary>
    public partial class EditAddress : Window, INotifyPropertyChanged {
        /// <summary>
        /// Address typed by user. Only valid after the dialog returns OK.  Will be set to -1
        /// if the user is attempting to delete the address.
        /// </summary>
        public int Address { get; private set; }

        /// <summary>
        /// Maximum allowed address value.
        /// </summary>
        private int mMaxAddressValue;

        /// <summary>
        /// Bound two-way property.
        /// </summary>
        public string AddressText { get; set; }

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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditAddress(Window owner, int initialAddr, int maxAddressValue) {
            // Set the property before initializing the window -- we don't have a property
            // change notifier.
            Address = -2;
            mMaxAddressValue = maxAddressValue;
            AddressText = Asm65.Address.AddressToString(initialAddr, false);

            InitializeComponent();
            Owner = owner;
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            addrTextBox.SelectAll();
            addrTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (AddressText.Length == 0) {
                Address = -1;
            } else {
                Asm65.Address.ParseAddress(AddressText, mMaxAddressValue, out int addr);
                Address = addr;
            }
            DialogResult = true;
        }

        /// <summary>
        /// Handles a TextChanged event on the address text box.
        /// </summary>
        /// <remarks>
        /// Must have UpdateSourceTrigger=PropertyChanged set for this to work.  The default
        /// for TextBox is LostFocus.
        /// </remarks>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (IsLoaded) {
                string text = AddressText;
                IsValid = (text.Length == 0) ||
                    Asm65.Address.ParseAddress(text, mMaxAddressValue, out int unused);
            }
        }
    }


    // This might be better with validation rules, but it's sort of awkward to pass parameters
    // (like MaxAddressValue) in.
    // https://social.technet.microsoft.com/wiki/contents/articles/31422.wpf-passing-a-data-bound-value-to-a-validation-rule.aspx
    //
    // Speaking of awkward, updating the OK button's IsEnable value through validation
    // requires MultiDataTrigger.


    //public class AddressValidationRule : ValidationRule {
    //    public int MaxAddress { get; set; }

    //    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
    //        string text = value.ToString();
    //        Debug.WriteLine("VALIDATE " + text);
    //        if ((text.Length == 0) ||
    //            Asm65.Address.ParseAddress(text, MaxAddress, out int unused)) {
    //            return new ValidationResult(true, null);
    //        } else {
    //            return new ValidationResult(false, "Invalid address");
    //        }
    //    }
    //}
}