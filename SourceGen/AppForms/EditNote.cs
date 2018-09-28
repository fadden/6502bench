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
    public partial class EditNote : Form {
        /// <summary>
        /// Get or set the note object.  On exit, will be set to null if the user wants
        /// to delete the note.
        /// </summary>
        public MultiLineComment Note { get; set; }

        // Highlight color palette.  Unless the user has funky theme, the color will be
        // replacing a white background, and will be overlaid with black text, so should
        // be on the lighter end of the spectrum.
        private enum ColorList {
            None = 0, Green, Blue, Yellow, Pink, Orange
        }
        private static Color[] sColors = new Color[] {
            Color.FromArgb(0),          // None
            Color.LightGreen,
            Color.LightBlue,
            Color.Yellow, //LightGoldenrodYellow,
            Color.LightPink,
            Color.Orange
        };
        private RadioButton[] mColorButtons;


        public EditNote() {
            InitializeComponent();
            Note = new MultiLineComment(string.Empty);
        }

        private void EditNote_Load(object sender, EventArgs e) {
            noteTextBox.Text = Note.Text;

            mColorButtons = new RadioButton[] {
                colorDefaultRadio,
                colorGreenRadio,
                colorBlueRadio,
                colorYellowRadio,
                colorPinkRadio,
                colorOrangeRadio
            };
            Debug.Assert(mColorButtons.Length == sColors.Length);

            // Configure radio buttons.
            colorDefaultRadio.Checked = true;
            if (Note != null) {
                Color curColor = Note.BackgroundColor;
                for (int i = 0; i < sColors.Length; i++) {
                    // Can't just compare colors, because the sColors entries are "known" and
                    // have some additional properties set.  Comparing the RGB values works.
                    if (sColors[i].ToArgb() == curColor.ToArgb()) {
                        mColorButtons[i].Checked = true;
                        break;
                    }
                }
            }
        }

        // Handle Ctrl+Enter.
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            if (keyData == (Keys.Control | Keys.Enter)) {
                DialogResult = DialogResult.OK;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void EditNote_FormClosing(object sender, FormClosingEventArgs e) {
            if (string.IsNullOrEmpty(noteTextBox.Text)) {
                Note = null;
            } else {
                Color bkgndColor = Color.Fuchsia;
                for (int i = 0; i < mColorButtons.Length; i++) {
                    if (mColorButtons[i].Checked) {
                        bkgndColor = sColors[i];
                        break;
                    }
                }
                Note = new MultiLineComment(noteTextBox.Text, bkgndColor);
            }
        }
    }
}
