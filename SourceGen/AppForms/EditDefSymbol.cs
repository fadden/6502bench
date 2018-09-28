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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using Asm65;

namespace SourceGen.AppForms {
    public partial class EditDefSymbol : Form {
        /// <summary>
        /// Set to previous value before calling; may be null if creating a new symbol.
        /// Will be set to new value on OK result.
        /// </summary>
        public DefSymbol DefSym { get; set; }

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        private Formatter NumFormatter { get; set; }

        /// <summary>
        /// List of existing symbols, for uniqueness check.  The list will not be modified.
        /// </summary>
        private SortedList<String, DefSymbol> DefSymbolList { get; set; }

        // Saved off at dialog load time.
        private Color mDefaultLabelColor;


        public EditDefSymbol(Formatter formatter, SortedList<String, DefSymbol> defList) {
            InitializeComponent();

            NumFormatter = formatter;
            DefSymbolList = defList;
        }

        private void EditDefSymbol_Load(object sender, EventArgs e) {
            mDefaultLabelColor = labelNotesLabel.ForeColor;

            if (DefSym != null) {
                labelTextBox.Text = DefSym.Label;
                valueTextBox.Text = NumFormatter.FormatValueInBase(DefSym.Value,
                    DefSym.DataDescriptor.NumBase);
                commentTextBox.Text = DefSym.Comment;

                if (DefSym.SymbolType == Symbol.Type.Constant) {
                    constantRadioButton.Checked = true;
                } else {
                    addressRadioButton.Checked = true;
                }
            } else {
                addressRadioButton.Checked = true;
            }

            UpdateControls();
        }

        private void UpdateControls() {
            bool labelValid, labelUnique, valueValid;

            // Label must be valid and not already exist in project symbol list.  (It's okay
            // if it exists elsewhere.)
            labelValid = Asm65.Label.ValidateLabel(labelTextBox.Text);

            if (DefSymbolList.TryGetValue(labelTextBox.Text, out DefSymbol existing)) {
                // It's okay if it's the same object.
                labelUnique = (existing == DefSym);
            } else {
                labelUnique = true;
            }

            // Value must be blank, meaning "erase any earlier definition", or valid value.
            // (Hmm... don't currently have a way to specify "no symbol" in DefSymbol.)
            //if (!string.IsNullOrEmpty(valueTextBox.Text)) {
                valueValid = ParseValue(out int unused1, out int unused2);
            //} else {
            //    valueValid = true;
            //}

            labelNotesLabel.ForeColor = labelValid ? mDefaultLabelColor : Color.Red;
            labelUniqueLabel.ForeColor = labelUnique ? mDefaultLabelColor : Color.Red;
            valueNotesLabel.ForeColor = valueValid ? mDefaultLabelColor : Color.Red;

            okButton.Enabled = labelValid && labelUnique && valueValid;
        }

        private bool ParseValue(out int value, out int numBase) {
            string str = valueTextBox.Text;
            if (str.IndexOf('/') >= 0) {
                // treat as address
                numBase = 16;
                return Asm65.Address.ParseAddress(str, (1 << 24) - 1, out value);
            } else {
                return Asm65.Number.TryParseInt(str, out value, out numBase);
            }
        }

        private void okButton_Click(object sender, EventArgs e) {
            bool isConstant = constantRadioButton.Checked;

            ParseValue(out int value, out int numBase);
            FormatDescriptor.SubType subType = FormatDescriptor.GetSubTypeForBase(numBase);
            DefSym = new DefSymbol(labelTextBox.Text, value, Symbol.Source.Project,
                isConstant ? Symbol.Type.Constant : Symbol.Type.ExternalAddr,
                subType, commentTextBox.Text, string.Empty);
        }

        private void labelTextBox_TextChanged(object sender, EventArgs e) {
            UpdateControls();
        }

        private void valueTextBox_TextChanged(object sender, EventArgs e) {
            UpdateControls();
        }
    }
}
