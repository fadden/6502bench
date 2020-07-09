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

        private AddressMap mAddrMap;
        private Formatter mFormatter;

        private string mDataBankStr;
        public string DataBankStr {
            get { return mDataBankStr; }
            set { mDataBankStr = value; OnPropertyChanged(); }
        }

        private bool mIsValid;
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }

        public class BankLabel {
            public CodeAnalysis.DbrValue Bank { get; private set; }
            public string Label { get; private set; }

            public BankLabel(CodeAnalysis.DbrValue bank, string label) {
                Bank = bank;
                Label = label;
            }
        }
        public List<BankLabel> BankLabels { get; private set; } = new List<BankLabel>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        public EditDataBank(Window owner, AddressMap addrMap, Formatter formatter,
                CodeAnalysis.DbrValue curValue) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mAddrMap = addrMap;
            mFormatter = formatter;

            if (curValue == CodeAnalysis.DbrValue.ProgramBankReg) {
                DataBankStr = PROG_BANK_STR;
            } else if (curValue == CodeAnalysis.DbrValue.Unknown) {
                DataBankStr = string.Empty;
            } else if ((int)curValue >= 0 && (int)curValue <= 255) {
                // Format as address rather than hexvalue so we don't get leading '$'.
                DataBankStr = formatter.FormatAddress((int)curValue, false);
            } else {
                Debug.Assert(false, "invalid DBR value " + curValue);
                DataBankStr = string.Empty;
            }

            // TODO: combo box
            BankLabels.Add(new BankLabel((CodeAnalysis.DbrValue)1, "(other)"));
            BankLabels.Add(new BankLabel((CodeAnalysis.DbrValue)1, "$02 FirstBankLabel"));
            BankLabels.Add(new BankLabel((CodeAnalysis.DbrValue)1, "$88 FancyBank"));
            bankCombo.SelectedIndex = 0;

            IsValid = true;     // TODO: validate
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            bankValueBox.SelectAll();
            bankValueBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Result = GetValue(DataBankStr);
            DialogResult = true;
        }

        /// <summary>
        /// Converts a DBR value string to a value.
        /// </summary>
        /// <param name="valueStr">String to convert.</param>
        /// <returns>DBR value.</returns>
        private static CodeAnalysis.DbrValue GetValue(string valueStr) {
            if (valueStr == PROG_BANK_STR) {
                return CodeAnalysis.DbrValue.ProgramBankReg;
            } else {
                // Try to parse as 1- or 2-digit hex value.
                try {
                    int val = Convert.ToInt32(valueStr, 16);
                    if (val < 0 || val > 255) {
                        // invalid value
                        return CodeAnalysis.DbrValue.Unknown;
                    }
                    return (CodeAnalysis.DbrValue)val;
                } catch (Exception ex) {
                    Debug.WriteLine("Result parse failed: " + ex.Message);
                    return CodeAnalysis.DbrValue.Unknown;
                }
            }
        }
    }
}
