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
#if false
        private bool mAllowLastChar;
#endif

        /// <summary>
        /// Maximum allowed address value.
        /// </summary>
        public int MaxAddressValue { get; set; }

        /// <summary>
        /// Address typed by user. Only valid after the dialog returns OK.  Will be set to -1
        /// if the user is attempting to delete the address.
        /// </summary>
        public int Address { get; private set; }

        public EditAddress() {
            InitializeComponent();
            Address = -2;
            MaxAddressValue = (1 << 24) - 1;

#if false
            // This is probably not all that useful. We're not preventing
            // invalid inputs, e.g. excessively large values or "$/$/$/", by restricting
            // the keys that can be typed.
            textBox1.KeyDown += textBox1_KeyDown;
            textBox1.KeyPress += textBox1_KeyPress;
#endif

            // Update the OK button based on current contents.
            textBox1.TextChanged += textBox1_TextChanged;
        }

        public void SetInitialAddress(int addr) {
            textBox1.Text = Asm65.Address.AddressToString(addr, false);
            textBox1.SelectAll();
        }

        /// <summary>
        /// Handles a click on the OK button by setting the Address property to the
        /// decoded value from the text field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, EventArgs e) {
            if (textBox1.Text.Length == 0) {
                Address = -1;
            } else {
                Asm65.Address.ParseAddress(textBox1.Text, MaxAddressValue, out int addr);
                Address = addr;
            }
        }

        /// <summary>
        /// Enables or disables the OK button depending on whether the current input is
        /// valid.  We allow valid addresses and an empty box.
        /// </summary>
        private void UpdateOkEnabled() {
            string text = textBox1.Text;
            okButton.Enabled = (text.Length == 0) ||
                Asm65.Address.ParseAddress(text, MaxAddressValue, out int unused);
        }

#if false
        /// <summary>
        /// Limits characters to [A-F][a-f][0-9][/].
        /// </summary>
        private void textBox1_KeyDown(object sender, KeyEventArgs e) {
            bool allow = false;
            if (e.KeyCode == Keys.D4 && e.Modifiers == Keys.Shift) {
                allow = true;       // allow '$'; not sure this works on non-US keyboards?
            } else if (e.KeyCode >= Keys.A && e.KeyCode <= Keys.F) {
                allow = !(e.Alt || e.Control);  // allow shift
            } else if ((e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) ||
                    e.KeyCode == Keys.OemQuestion) {
                allow = (e.Modifiers == 0);
            } else if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete) {
                allow = true;
            }

            mAllowLastChar = allow;
            //Debug.WriteLine("DOWN " + e.KeyCode + " allow=" + allow);
        }

        /// <summary>
        /// Rejects invalid characters.
        /// </summary>
        private void textBox1_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) {
            //Debug.WriteLine("PRESS " + e.KeyChar + " : " + mAllowLastChar);
            if (!mAllowLastChar) {
                e.Handled = true;
            }
        }
#endif

        /// <summary>
        /// Updates the OK button whenever the text changes.  This works for all change sources,
        /// including programmatic.
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e) {
            UpdateOkEnabled();
        }
    }
}
