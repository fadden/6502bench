/*
 * Copyright 2018 faddenSoft
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
using System.Windows.Forms;

namespace SourceGen.AppForms {
    public partial class EditAddress : Form {

        /// <summary>
        /// Address typed by user. Only valid after the dialog returns OK.  Will be set to -1
        /// if the user is attempting to delete the address.
        /// </summary>
        public int Address { get; private set; }

        /// <summary>
        /// Maximum allowed address value.
        /// </summary>
        private int mMaxAddressValue;


        public EditAddress(int initialAddr, int maxAddressValue) {
            InitializeComponent();

            Address = -2;
            mMaxAddressValue = maxAddressValue;
            addressTextBox.Text = Asm65.Address.AddressToString(initialAddr, false);
        }

        private void EditAddress_Load(object sender, EventArgs e) {
            addressTextBox.SelectAll();
        }

        /// <summary>
        /// Handles a click on the OK button by setting the Address property to the
        /// decoded value from the text field.
        /// </summary>
        private void okButton_Click(object sender, EventArgs e) {
            if (addressTextBox.Text.Length == 0) {
                Address = -1;
            } else {
                Asm65.Address.ParseAddress(addressTextBox.Text, mMaxAddressValue, out int addr);
                Address = addr;
            }
        }

        /// <summary>
        /// Updates the OK button whenever the text changes.  This works for all change sources,
        /// including programmatic.
        /// </summary>
        private void addressTextBox_TextChanged(object sender, EventArgs e) {
            UpdateOkEnabled();
        }

        /// <summary>
        /// Enables or disables the OK button depending on whether the current input is
        /// valid.  We allow valid addresses and an empty box.
        /// </summary>
        private void UpdateOkEnabled() {
            string text = addressTextBox.Text;
            okButton.Enabled = (text.Length == 0) ||
                Asm65.Address.ParseAddress(text, mMaxAddressValue, out int unused);
        }
    }
}
