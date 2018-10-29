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
using System.Drawing;
using System.Windows.Forms;

namespace SourceGen.AppForms {
    public partial class EditLabel : Form {
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
        private Color mDefaultLabelColor;


        public EditLabel(Symbol origSym, int address, SymbolTable symbolTable) {
            InitializeComponent();

            LabelSym = origSym;
            mAddress = address;
            mSymbolTable = symbolTable;
        }

        private void EditLabel_Load(object sender, EventArgs e) {
            mDefaultLabelColor = maxLengthLabel.ForeColor;

            if (LabelSym == null) {
                labelTextBox.Text = string.Empty;
                radioButtonLocal.Checked = true;
            } else {
                labelTextBox.Text = LabelSym.Label;
                switch (LabelSym.SymbolType) {
                    case Symbol.Type.LocalOrGlobalAddr:
                        radioButtonLocal.Checked = true;
                        break;
                    case Symbol.Type.GlobalAddr:
                        radioButtonGlobal.Checked = true;
                        break;
                    case Symbol.Type.GlobalAddrExport:
                        radioButtonExport.Checked = true;
                        break;
                    default:
                        Debug.Assert(false);    // WTF
                        radioButtonLocal.Checked = true;
                        break;
                }
            }
        }

        private void labelTextBox_TextChanged(object sender, EventArgs e) {
            string str = labelTextBox.Text;
            bool valid = true;

            if (str.Length == 1 || str.Length > Asm65.Label.MAX_LABEL_LEN) {
                valid = false;
                maxLengthLabel.ForeColor = Color.Red;
            } else {
                maxLengthLabel.ForeColor = mDefaultLabelColor;
            }

            // Regex never matches on strings of length 0 or 1, but we don't want
            // to complain about that since we're already doing that above.
            // TODO(maybe): Ideally this wouldn't light up if the only problem was a
            //   non-alpha first character, since the next test will call that out.
            if (str.Length > 1) {
                if (!Asm65.Label.ValidateLabel(str)) {
                    valid = false;
                    validCharsLabel.ForeColor = Color.Red;
                } else {
                    validCharsLabel.ForeColor = mDefaultLabelColor;
                }
            } else {
                validCharsLabel.ForeColor = mDefaultLabelColor;
            }

            if (str.Length > 0 &&
                    !((str[0] >= 'A' && str[0] <= 'Z') || (str[0] >= 'a' && str[0] <= 'z') ||
                      str[0] == '_')) {
                // This should have been caught by the regex.  We just want to set the
                // color on the "first character must be" instruction text.
                Debug.Assert(!valid);
                firstLetterLabel.ForeColor = Color.Red;
            } else {
                firstLetterLabel.ForeColor = mDefaultLabelColor;
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
                notDuplicateLabel.ForeColor = Color.Red;
            } else {
                notDuplicateLabel.ForeColor = mDefaultLabelColor;
            }

            okButton.Enabled = valid;
        }

        private void okButton_Click(object sender, EventArgs e) {
            if (string.IsNullOrEmpty(labelTextBox.Text)) {
                LabelSym = null;
            } else {
                Symbol.Type symbolType;
                if (radioButtonLocal.Checked) {
                    symbolType = Symbol.Type.LocalOrGlobalAddr;
                } else if (radioButtonGlobal.Checked) {
                    symbolType = Symbol.Type.GlobalAddr;
                } else if (radioButtonExport.Checked) {
                    symbolType = Symbol.Type.GlobalAddrExport;
                } else {
                    Debug.Assert(false);        // WTF
                    symbolType = Symbol.Type.LocalOrGlobalAddr;
                }
                LabelSym = new Symbol(labelTextBox.Text, mAddress, Symbol.Source.User, symbolType);
            }
        }
    }
}
