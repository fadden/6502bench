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
using System.Text;
using System.Windows.Forms;

namespace SourceGen.Tools {
    public partial class AsciiChart : Form {
        /// <summary>
        /// Chart mode.  Must match combo box entries.
        /// </summary>
        private enum Mode {
            Standard = 0,
            High = 1
        };

        /// <summary>
        /// Subscribe to this to be notified when the dialog closes.
        /// </summary>
        public event WindowClosing OnWindowClosing;
        public delegate void WindowClosing(object sender);

        public AsciiChart() {
            InitializeComponent();
        }

        private void AsciiChart_Load(object sender, EventArgs e) {
            int mode = AppSettings.Global.GetInt(AppSettings.ASCCH_MODE, 0);
            if (mode >= 0 && mode < modeComboBox.Items.Count) {
                modeComboBox.SelectedIndex = mode;
            }

            DrawContents();
        }

        private void AsciiChart_FormClosed(object sender, FormClosedEventArgs e) {
            if (OnWindowClosing != null) {
                OnWindowClosing(this);
            }
        }

        private void modeComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            AppSettings.Global.SetInt(AppSettings.ASCCH_MODE, modeComboBox.SelectedIndex);

            DrawContents();
        }

        private void DrawContents() {
            const string hdr = "Dec Hex Chr";
            const string div = "  |  ";
            const string eol = "\r\n";

            Mode mode = (Mode) modeComboBox.SelectedIndex;
            StringBuilder sb = new StringBuilder(
                (hdr.Length * 4 + div.Length * 3 + eol.Length) * 32);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(div);
            sb.Append(hdr);
            sb.Append(eol);
            for (int i = 0; i < 32; i++) {
                DrawEntry(mode, i, sb);
                sb.Append(div);
                DrawEntry(mode, i + 32, sb);
                sb.Append(div);
                DrawEntry(mode, i + 64, sb);
                sb.Append(div);
                DrawEntry(mode, i + 96, sb);
                sb.Append(eol);
            }

            chartTextBox.Text = sb.ToString();
            chartTextBox.SelectionStart = sb.Length;
            chartTextBox.SelectionLength = 0;
        }

        private void DrawEntry(Mode mode, int val, StringBuilder sb) {
            // Format is: Dec Hex Chr
            int modVal = (mode == Mode.High) ? val | 0x80 : val;
            sb.AppendFormat("{0,3:D} {1,3:X2} ", modVal, modVal);
            if (val < 0x20) {
                sb.Append('^');
                sb.Append((char)(val + 0x40));
                sb.Append(' ');
            } else if (val == 0x20) {
                sb.Append("' '");
            } else if (val < 0x7f) {
                sb.Append(' ');
                sb.Append((char)val);
                sb.Append(' ');
            } else {
                sb.Append("DEL");
            }
        }
    }
}
