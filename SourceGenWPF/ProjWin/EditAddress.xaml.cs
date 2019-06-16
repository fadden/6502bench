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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace SourceGenWPF.ProjWin {
    /// <summary>
    /// Edit Address dialog.
    /// </summary>
    public partial class EditAddress : Window {
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


        public EditAddress(Window owner, int initialAddr, int maxAddressValue) {
            // Set the property before initializing the window -- we don't have a property
            // change notifier.
            Address = -2;
            mMaxAddressValue = maxAddressValue;
            AddressText = Asm65.Address.AddressToString(initialAddr, false);

            this.DataContext = this;
            InitializeComponent();
            Owner = owner;
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
                UpdateOkEnabled();
            }
        }

        private void UpdateOkEnabled() {
            string text = AddressText;
            okButton.IsEnabled = (text.Length == 0) ||
                Asm65.Address.ParseAddress(text, mMaxAddressValue, out int unused);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            addrTextBox.SelectAll();
            addrTextBox.Focus();
        }
    }


    // I briefly played with the validation rules.  However, they're primarily designed for
    // form entry, which means they fire when focus leaves the text box.  [note: not sure if
    // this would change with UpdateSourceTrigger=PropertyChanged]  I want the OK button to be
    // kept constantly up to date as the user types, so this didn't really work.  It's also a
    // lot bigger and uglier than just handling an event.
    //
    // It's also sort of awkward to pass parameters, like MaxAddressValue, into the
    // validation rule.
    // https://social.technet.microsoft.com/wiki/contents/articles/31422.wpf-passing-a-data-bound-value-to-a-validation-rule.aspx
    //
    // Speaking of awkward, updating the OK button enable/disable through validation
    // is possible via MultiDataTrigger.


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