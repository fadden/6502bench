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

using Asm65;

namespace SourceGenWF.AppForms {
    public partial class EditStatusFlags : Form {
        /// <summary>
        /// In/out status flag value.
        /// </summary>
        public StatusFlags FlagValue { get; private set; }

        /// <summary>
        /// Set this if the CPU has an emulation flag (65802/65816).  If this isn't
        /// set, the M, X, and E flag buttons will be disabled.
        /// </summary>
        private bool mHasEmuFlag;


        public EditStatusFlags(StatusFlags flagValue, bool hasEmuFlag) {
            InitializeComponent();

            FlagValue = flagValue;
            mHasEmuFlag = hasEmuFlag;
        }

        private void EditStatusFlags_Load(object sender, EventArgs e) {
            if (!mHasEmuFlag) {
                panelM.Enabled = false;
                panelX.Enabled = false;
                panelE.Enabled = false;

                // I'm not going to force the M/X/E flags to have a particular value based
                // on the CPU definition.  The flags aren't used for non-65802/65816, so
                // the values are irrelevant.  If somebody is switching between CPUs I think
                // it'd be weird to force the values during editing but leave any non-edited
                // values alone.  If they want to switch to 65816, set M/X/E, and then switch
                // back, they're welcome to do so.
            }

            SetCheckedButtons();
        }

        /// <summary>
        /// Calls SetChecked() for each flag.
        /// </summary>
        private void SetCheckedButtons() {
            SetChecked(FlagValue.N, radioNDefault, radioNZero, radioNOne, radioNIndeterminate);
            SetChecked(FlagValue.V, radioVDefault, radioVZero, radioVOne, radioVIndeterminate);
            SetChecked(FlagValue.M, radioMDefault, radioMZero, radioMOne, radioMIndeterminate);
            SetChecked(FlagValue.X, radioXDefault, radioXZero, radioXOne, radioXIndeterminate);
            SetChecked(FlagValue.D, radioDDefault, radioDZero, radioDOne, radioDIndeterminate);
            SetChecked(FlagValue.I, radioIDefault, radioIZero, radioIOne, radioIIndeterminate);
            SetChecked(FlagValue.Z, radioZDefault, radioZZero, radioZOne, radioZIndeterminate);
            SetChecked(FlagValue.C, radioCDefault, radioCZero, radioCOne, radioCIndeterminate);
            SetChecked(FlagValue.E, radioEDefault, radioEZero, radioEOne, radioEIndeterminate);
        }

        /// <summary>
        /// Sets the "checked" flag on the appropriate radio button.
        /// </summary>
        private void SetChecked(int value, RadioButton def, RadioButton zero, RadioButton one,
                RadioButton indeterminate) {
            switch (value) {
                case TriState16.UNSPECIFIED:
                    def.Checked = true;
                    break;
                case TriState16.INDETERMINATE:
                    indeterminate.Checked = true;
                    break;
                case 0:
                    zero.Checked = true;
                    break;
                case 1:
                    one.Checked = true;
                    break;
                default:
                    throw new Exception("Unexpected value " + value);
            }
        }

        private void okButton_Click(object sender, EventArgs e) {
            StatusFlags flags = new StatusFlags();

            flags.N = GetChecked(radioNDefault, radioNZero, radioNOne, radioNIndeterminate);
            flags.V = GetChecked(radioVDefault, radioVZero, radioVOne, radioVIndeterminate);
            flags.M = GetChecked(radioMDefault, radioMZero, radioMOne, radioMIndeterminate);
            flags.X = GetChecked(radioXDefault, radioXZero, radioXOne, radioXIndeterminate);
            flags.D = GetChecked(radioDDefault, radioDZero, radioDOne, radioDIndeterminate);
            flags.I = GetChecked(radioIDefault, radioIZero, radioIOne, radioIIndeterminate);
            flags.Z = GetChecked(radioZDefault, radioZZero, radioZOne, radioZIndeterminate);
            flags.C = GetChecked(radioCDefault, radioCZero, radioCOne, radioCIndeterminate);
            flags.E = GetChecked(radioEDefault, radioEZero, radioEOne, radioEIndeterminate);

            //// If they're setting emulation mode, also set M/X to 1.  This is implicitly
            //// true, but things are a bit clearer if we make it explicit.
            //if (flags.E == 1) {
            //    flags.M = flags.X = 1;
            //}

            FlagValue = flags;
        }

        /// <summary>
        /// Identifies the checked radio button and returns the appropriate TriState16 value.
        /// </summary>
        private int GetChecked(RadioButton def, RadioButton zero, RadioButton one,
                RadioButton indeterminate) {
            if (zero.Checked) {
                return 0;
            } else if (one.Checked) {
                return 1;
            } else if (indeterminate.Checked) {
                return TriState16.INDETERMINATE;
            } else if (def.Checked) {
                return TriState16.UNSPECIFIED;
            } else {
                throw new Exception("No radio button selected");
            }
        }

        private void resetButton_Click(object sender, EventArgs e) {
            FlagValue = new StatusFlags();
            SetCheckedButtons();
        }
    }
}
