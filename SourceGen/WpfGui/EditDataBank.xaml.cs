/*
 * Copyright 2020 faddenSoft
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
using Asm65;
using CommonUtil;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Edit Data Bank dialog.
    /// </summary>
    public partial class EditDataBank : Window, INotifyPropertyChanged {
        private const string PROG_BANK_STR = "K";

        public CodeAnalysis.DbrValue Result { get; private set; }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DisasmProject mProject;
        private Formatter mFormatter;
        private bool mSettingComboBox;

        private string mDataBankStr;
        public string DataBankStr {
            get { return mDataBankStr; }
            set { mDataBankStr = value; OnPropertyChanged(); UpdateControls(); }
        }

        private bool mIsValid;
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }

        public class BankLabelItem : IComparable<BankLabelItem> {
            //public CodeAnalysis.DbrValue Bank { get; private set; }
            public byte Bank { get; private set; }
            public string Label { get; private set; }

            public BankLabelItem(byte bank, string label) {
                Bank = bank;
                Label = label;
            }

            public int CompareTo(BankLabelItem other) {
                return Bank - other.Bank;
            }
        }
        public List<BankLabelItem> BankLabels { get; private set; } = new List<BankLabelItem>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="proj">Disassembly project.</param>
        /// <param name="formatter">Text formatter.</param>
        /// <param name="curValue">Current value, or null if none set.</param>
        public EditDataBank(Window owner, DisasmProject proj, Formatter formatter,
                CodeAnalysis.DbrValue curValue) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = proj;
            mFormatter = formatter;

            PopulateComboBox();
            DataBankStr = DbrValueToString(curValue);   // sets combo box
            IsValid = true;
        }

        private void PopulateComboBox() {
            // Entry #0 is always the "other" option.
            string otherStr = (string)FindResource("str_OtherBank");
            BankLabels.Add(new BankLabelItem(0, otherStr));

            bool[] done = new bool[256];
            foreach (AddressMap.AddressMapEntry ent in mProject.AddrMap) {
                byte bank = (byte)(ent.Address >> 16);
                if (done[bank]) {
                    continue;
                }
                done[bank] = true;

                Anattrib attr = mProject.GetAnattrib(ent.Offset);
                string label = (attr.Symbol != null) ? attr.Symbol.Label : string.Empty;
                BankLabels.Add(new BankLabelItem(bank,
                    mFormatter.FormatHexValue(bank, 2) + " " + label));
            }

            BankLabels.Sort();
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            bankValueBox.SelectAll();
            bankValueBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Result = StringToDbrValue(DataBankStr);
            DialogResult = true;
        }

        private void UpdateControls() {
            CodeAnalysis.DbrValue dbrVal = StringToDbrValue(DataBankStr);
            IsValid = (string.IsNullOrEmpty(DataBankStr) || dbrVal != null);
            SetComboBoxSelection(dbrVal);
        }

        /// <summary>
        /// Sets the selected item in the combo box based on the value in the text edit box.
        /// </summary>
        private void SetComboBoxSelection(CodeAnalysis.DbrValue dbrVal) {
            mSettingComboBox = true;        // recursion guard

            //CodeAnalysis.DbrValue dbrVal = StringToDbrValue(DataBankStr);
            int index = 0;
            if (dbrVal != null && !dbrVal.FollowPbr) {
                // skip first entry
                for (int i = 1; i < BankLabels.Count; i++) {
                    if (BankLabels[i].Bank == dbrVal.Bank) {
                        index = i;
                        break;
                    }
                }
            }
            bankCombo.SelectedIndex = index;

            mSettingComboBox = false;
        }

        /// <summary>
        /// Reacts to a combo box selection change.
        /// </summary>
        private void bankCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (mSettingComboBox) {
                // Selection changed programatically, don't update text edit box.
                return;
            }
            if (bankCombo.SelectedIndex == 0) {
                // "other", don't update text edit box
                return;
            }
            BankLabelItem item = (BankLabelItem)bankCombo.SelectedItem;

            DataBankStr = mFormatter.FormatHexValue(item.Bank, 2);
        }

        /// <summary>
        /// Converts a DBR value string to a value.
        /// </summary>
        /// <param name="valueStr">String to convert.</param>
        /// <returns>DBR value, or null if invalid.</returns>
        private static CodeAnalysis.DbrValue StringToDbrValue(string valueStr) {
            valueStr = valueStr.Trim();
            if (string.IsNullOrEmpty(valueStr)) {
                return null;
            } else if (valueStr.Equals(PROG_BANK_STR,
                    StringComparison.InvariantCultureIgnoreCase)) {
                return new CodeAnalysis.DbrValue(true, 0, CodeAnalysis.DbrValue.Source.User);
            } else {
                if (!Number.TryParseIntHex(valueStr, out int val)) {
                    Debug.WriteLine("Unable to parse '" + valueStr + "' as hex value");
                    return null;
                }
                if (val != (byte)val) {
                    Debug.WriteLine("Val " + val + " out of range of byte");
                    return null;
                }

                return new CodeAnalysis.DbrValue(false, (byte)val,
                    CodeAnalysis.DbrValue.Source.User);
            }
        }

        private string DbrValueToString(CodeAnalysis.DbrValue value) {
            if (value == null) {
                return string.Empty;
            } else if (value.FollowPbr) {
                return PROG_BANK_STR;
            } else {
                return mFormatter.FormatHexValue(value.Bank, 2);
            }
        }
    }
}
