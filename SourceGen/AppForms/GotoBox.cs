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
using System.Windows.Forms;

using Asm65;

namespace SourceGen.AppForms {
    public partial class GotoBox : Form {
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


        public GotoBox(DisasmProject proj, Formatter formatter) {
            InitializeComponent();

            mProject = proj;
            mFormatter = formatter;
            TargetOffset = -1;
        }

        private void GotoBox_Load(object sender, EventArgs e) {
            UpdateDisplay();
        }

        // Without a "cancel" button, the escape key does nothing.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == Keys.Escape) {
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void okButton_Click(object sender, EventArgs e) {
        }

        private void targetTextBox_TextChanged(object sender, EventArgs e) {
            ProcessInput();
            UpdateDisplay();
            okButton.Enabled = (TargetOffset >= 0);
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
            } else if (Address.ParseAddress(input, 1<<24, out int addr)) {
                // could be a valid address
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

            offsetValueLabel.Text = offsetStr;
            addressValueLabel.Text = addressStr;
            labelValueLabel.Text = labelStr;
        }
    }
}
