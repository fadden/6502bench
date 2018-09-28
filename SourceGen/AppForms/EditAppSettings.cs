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
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SourceGen.AppForms {
    public partial class EditAppSettings : Form {
        /// <summary>
        /// Tab page enumeration.  Numbers must match page indices in designer.
        /// </summary>
        public enum Tab {
            Unknown = -1,
            CodeList = 0,
            Assembler = 1,
            AsmFormat = 2,
            PseudoOp = 3
        }

        /// <summary>
        /// ProjectView reference.  When the user hits Apply, the object's ApplyAppSettings
        /// method will be invoked.
        /// </summary>
        private ProjectView mProjectView;

        /// <summary>
        /// Copy of settings that we make changes to.  On "Apply" or "OK", this is pushed
        /// into the global settings object, and applied to the ProjectView.
        /// </summary>
        private AppSettings mSettings;

        /// <summary>
        /// Dirty flag, set when anything in mSettings changes.  Don't modify this directly.  Use
        /// the SetDirty() call so that the Apply button's enabled status gets updated.
        /// </summary>
        private bool mDirty;

        /// <summary>
        /// Tab to show when dialog is first opened.
        /// </summary>
        private Tab mInitialTab;

        // Map buttons to column show/hide buttons.
        private const int NUM_COLUMNS = ProjectView.CodeListColumnWidths.NUM_COLUMNS;
        private string[] mColumnFormats = new string[NUM_COLUMNS];
        private Button[] mColButtons;

        // Map pseudo-op text entry fields to PseudoOpName properties.
        private struct TextBoxPropertyMap {
            public TextBox TextBox { get; private set; }
            public PropertyInfo PropInfo { get; private set; }

            public TextBoxPropertyMap(TextBox textBox, string propName) {
                TextBox = textBox;
                PropInfo = typeof(PseudoOp.PseudoOpNames).GetProperty(propName);
            }
        }
        private TextBoxPropertyMap[] mPseudoNameMap;


        public EditAppSettings(ProjectView projectView, Tab initialTab) {
            InitializeComponent();

            mProjectView = projectView;
            mInitialTab = initialTab;

            // Make a work copy, so we can discard changes if the user cancels out of the dialog.
            projectView.SaveCodeListColumnWidths();
            mSettings = AppSettings.Global.GetCopy();

            // Put buttons in an array.
            mColButtons = new Button[] {
                showCol0, showCol1, showCol2, showCol3, showCol4,
                showCol5, showCol6, showCol7, showCol8 };
            Debug.Assert(NUM_COLUMNS == 9);

            // Extract formats from button labels.
            for (int i = 0; i < NUM_COLUMNS; i++) {
                mColButtons[i].Click += ColumnVisibilityButtonClick;
                mColumnFormats[i] = mColButtons[i].Text;
            }

            // Map text boxes to PseudoOpName fields.
            mPseudoNameMap = new TextBoxPropertyMap[] {
                new TextBoxPropertyMap(equDirectiveTextBox, "EquDirective"),
                new TextBoxPropertyMap(orgDirectiveTextBox, "OrgDirective"),
                new TextBoxPropertyMap(regWidthDirectiveTextBox, "RegWidthDirective"),
                new TextBoxPropertyMap(defineData1TextBox, "DefineData1"),
                new TextBoxPropertyMap(defineData2TextBox, "DefineData2"),
                new TextBoxPropertyMap(defineData3TextBox, "DefineData3"),
                new TextBoxPropertyMap(defineData4TextBox, "DefineData4"),
                new TextBoxPropertyMap(defineBigData2TextBox, "DefineBigData2"),
                new TextBoxPropertyMap(fillTextBox, "Fill"),
                new TextBoxPropertyMap(denseTextBox, "Dense"),
                new TextBoxPropertyMap(strGenericTextBox, "StrGeneric"),
                new TextBoxPropertyMap(strGenericHiTextBox, "StrGenericHi"),
                new TextBoxPropertyMap(strReverseTextBox, "StrReverse"),
                new TextBoxPropertyMap(strReverseHiTextBox, "StrReverseHi"),
                new TextBoxPropertyMap(strLen8TextBox, "StrLen8"),
                new TextBoxPropertyMap(strLen8HiTextBox, "StrLen8Hi"),
                new TextBoxPropertyMap(strLen16TextBox, "StrLen16"),
                new TextBoxPropertyMap(strLen16HiTextBox, "StrLen16Hi"),
                new TextBoxPropertyMap(strNullTermTextBox, "StrNullTerm"),
                new TextBoxPropertyMap(strNullTermHiTextBox, "StrNullTermHi"),
                new TextBoxPropertyMap(strDciTextBox, "StrDci"),
                new TextBoxPropertyMap(strDciHiTextBox, "StrDciHi"),
            };
        }

        private void EditAppSettings_Load(object sender, EventArgs e) {
            // Column widths.  We called SaveCodeListColumnWidths() earlier, so this
            // should always be a valid serialized string.
            string widthStr = mSettings.GetString(AppSettings.CDLV_COL_WIDTHS, null);
            Debug.Assert(!string.IsNullOrEmpty(widthStr));
            ProjectView.CodeListColumnWidths widths =
                ProjectView.CodeListColumnWidths.Deserialize(widthStr);
            Debug.Assert(widths != null);
            for (int i = 0; i < NUM_COLUMNS; i++) {
                SetShowHideButton(i, widths.Width[i]);
            }

            // Display localized font string.
            FontConverter cvt = new FontConverter();
            currentFontDisplayLabel.Text = cvt.ConvertToString(mProjectView.CodeListViewFont);

            // Upper-case formatting.
            upperHexCheckBox.Checked = mSettings.GetBool(AppSettings.FMT_UPPER_HEX_DIGITS, false);
            upperOpcodeCheckBox.Checked = mSettings.GetBool(
                AppSettings.FMT_UPPER_OP_MNEMONIC, false);
            upperPseudoOpCheckBox.Checked = mSettings.GetBool(
                AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, false);
            upperACheckBox.Checked = mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_A, false);
            upperSCheckBox.Checked = mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_S, false);
            upperXYCheckBox.Checked = mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_XY, false);

            int clipIndex = mSettings.GetInt(AppSettings.CLIP_LINE_FORMAT, 0);
            if (clipIndex >= 0 && clipIndex < clipboardFormatComboBox.Items.Count) {
                clipboardFormatComboBox.SelectedIndex = clipIndex;
            }

            enableDebugCheckBox.Checked = mSettings.GetBool(AppSettings.DEBUG_MENU_ENABLED, false);

            // Assemblers.
            cc65PathTextBox.Text =
                mSettings.GetString(AppSettings.ASM_CC65_EXECUTABLE, string.Empty);
            merlin32PathTextBox.Text =
                mSettings.GetString(AppSettings.ASM_MERLIN32_EXECUTABLE, string.Empty);
            showAsmIdentCheckBox.Checked =
                mSettings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false);
            disableLabelLocalizationCheckBox.Checked =
                mSettings.GetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, false);
            longLabelNewLineCheckBox.Checked =
                mSettings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);
            showCycleCountsCheckBox.Checked =
                mSettings.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, false);

            // Pseudo ops.
            string opStrCereal = mSettings.GetString(AppSettings.FMT_PSEUDO_OP_NAMES, null);
            if (!string.IsNullOrEmpty(opStrCereal)) {
                PseudoOp.PseudoOpNames opNames = PseudoOp.PseudoOpNames.Deserialize(opStrCereal);
                ImportPseudoOpNames(opNames);
            } else {
                // no data available, populate with blanks
                //PseudoOp.PseudoOpNames opNames = PseudoOp.sDefaultPseudoOpNames;
                ImportPseudoOpNames(new PseudoOp.PseudoOpNames());
            }

            PopulateWidthDisamSettings();

            string exprMode = mSettings.GetString(AppSettings.FMT_EXPRESSION_MODE, string.Empty);
            useMerlinExpressions.Checked =
                (Asm65.Formatter.FormatConfig.ParseExpressionMode(exprMode) ==
                    Asm65.Formatter.FormatConfig.ExpressionMode.Merlin);

            if (mInitialTab != Tab.Unknown) {
                settingsTabControl.SelectTab((int)mInitialTab);
            }

            mDirty = false;
            UpdateControls();
        }

        /// <summary>
        /// Updates controls.
        /// </summary>
        private void UpdateControls() {
            applyButton.Enabled = mDirty;

            clearMerlin32Button.Enabled = !string.IsNullOrEmpty(merlin32PathTextBox.Text);
            clearCc65Button.Enabled = !string.IsNullOrEmpty(cc65PathTextBox.Text);
        }

        /// <summary>
        /// Sets the dirty flag and updates the controls.
        /// </summary>
        /// <param name="dirty">New value for dirty flag.</param>
        private void SetDirty(bool dirty) {
            mDirty = dirty;
            UpdateControls();
        }

        private void okButton_Click(object sender, EventArgs e) {
            ApplySettings();
        }

        private void applyButton_Click(object sender, EventArgs e) {
            ApplySettings();
        }

        private void ApplySettings() {
            PseudoOp.PseudoOpNames opNames = ExportPseudoOpNames();
            string pseudoCereal = opNames.Serialize();
            mSettings.SetString(AppSettings.FMT_PSEUDO_OP_NAMES, pseudoCereal);

            mProjectView.SetAppSettings(mSettings);
            AsmGen.AssemblerVersionCache.QueryVersions();
            SetDirty(false);
        }


        #region Code View

        /// <summary>
        /// Updates the text on a show/hide column button.
        /// </summary>
        /// <param name="index">Column index.</param>
        /// <param name="width">New width.</param>
        private void SetShowHideButton(int index, int width) {
            Button button = mColButtons[index];
            string fmt = mColumnFormats[index];
            string show = Properties.Resources.SHOW_COL;
            string hide = Properties.Resources.HIDE_COL;
            button.Text = string.Format(fmt, (width == 0) ? show : hide);
        }

        /// <summary>
        /// Handler for all show/hide column buttons.
        /// </summary>
        /// <param name="sender">Identifies the button that was clicked.</param>
        /// <param name="e">Stuff.</param>
        private void ColumnVisibilityButtonClick(object sender, EventArgs e) {
            int index = -1;
            for (int i = 0; i < mColButtons.Length; i++) {
                if (sender == mColButtons[i]) {
                    index = i;
                    break;
                }
            }
            Debug.Assert(index != -1);

            string widthStr = mSettings.GetString(AppSettings.CDLV_COL_WIDTHS, null);
            Debug.Assert(!string.IsNullOrEmpty(widthStr));
            ProjectView.CodeListColumnWidths widths =
                ProjectView.CodeListColumnWidths.Deserialize(widthStr);
            if (widths.Width[index] == 0) {
                // Expand to default width.  The default width changes when the font
                // changes, so it's best to just reacquire the default width set as needed.
                ProjectView.CodeListColumnWidths defaultWidths =
                    mProjectView.GetDefaultCodeListColumnWidths();
                widths.Width[index] = defaultWidths.Width[index];
            } else {
                widths.Width[index] = 0;
            }
            widthStr = widths.Serialize();
            mSettings.SetString(AppSettings.CDLV_COL_WIDTHS, widthStr);
            SetShowHideButton(index, widths.Width[index]);

            SetDirty(true);
        }

        private void selectFontButton_Click(object sender, EventArgs e) {
            FontDialog dlg = new FontDialog();
            dlg.Font = mProjectView.CodeListViewFont;
            dlg.ShowEffects = false;
            Debug.WriteLine("Showing font dialog...");
            if (dlg.ShowDialog() != DialogResult.Cancel) {
                FontConverter cvt = new FontConverter();
                // Store invariant string, display localized string.
                mSettings.SetString(AppSettings.CDLV_FONT, cvt.ConvertToInvariantString(dlg.Font));
                currentFontDisplayLabel.Text = cvt.ConvertToString(dlg.Font);
                SetDirty(true);
            }
            Debug.WriteLine("Font dialog done...");
            dlg.Dispose();
        }

        private void upperHexCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.FMT_UPPER_HEX_DIGITS, upperHexCheckBox.Checked);
            SetDirty(true);
        }
        private void upperOpcodeCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, upperOpcodeCheckBox.Checked);
            SetDirty(true);
        }
        private void upperPseudoOpCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC,
                upperPseudoOpCheckBox.Checked);
            SetDirty(true);
        }
        private void upperACheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_A, upperACheckBox.Checked);
            SetDirty(true);
        }
        private void upperSCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_S, upperSCheckBox.Checked);
            SetDirty(true);
        }
        private void upperXYCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_XY, upperXYCheckBox.Checked);
            SetDirty(true);
        }
        private void upperAllLowerButton_Click(object sender, EventArgs e) {
            upperHexCheckBox.Checked =
                upperOpcodeCheckBox.Checked =
                upperPseudoOpCheckBox.Checked =
                upperACheckBox.Checked =
                upperSCheckBox.Checked =
                upperXYCheckBox.Checked = false;
        }
        private void upperAllUpperButton_Click(object sender, EventArgs e) {
            upperHexCheckBox.Checked =
                upperOpcodeCheckBox.Checked =
                upperPseudoOpCheckBox.Checked =
                upperACheckBox.Checked =
                upperSCheckBox.Checked =
                upperXYCheckBox.Checked = true;
        }

        private void clipboardFormatComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            mSettings.SetInt(AppSettings.CLIP_LINE_FORMAT, clipboardFormatComboBox.SelectedIndex);
            SetDirty(true);
        }

        private void enableDebugCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.DEBUG_MENU_ENABLED, enableDebugCheckBox.Checked);
            SetDirty(true);
        }

        #endregion Code View


        #region Asm Config

        private void browseCc65Button_Click(object sender, EventArgs e) {
            string pathName = BrowseForExecutable("cc65 CL", "cl65.exe");
            if (pathName != null) {
                cc65PathTextBox.Text = pathName;
                mSettings.SetString(AppSettings.ASM_CC65_EXECUTABLE, pathName);
            }
        }

        private void cc65PathTextBox_TextChanged(object sender, EventArgs e) {
            mSettings.SetString(AppSettings.ASM_CC65_EXECUTABLE, cc65PathTextBox.Text);
            SetDirty(true);
        }

        private void clearCc65Button_Click(object sender, EventArgs e) {
            cc65PathTextBox.Text = string.Empty;
            mSettings.SetString(AppSettings.ASM_CC65_EXECUTABLE, null);
            SetDirty(true);
        }

        private void browseMerlin32Button_Click(object sender, EventArgs e) {
            string pathName = BrowseForExecutable("Merlin Assembler", "Merlin32.exe");
            if (pathName != null) {
                merlin32PathTextBox.Text = pathName;
                mSettings.SetString(AppSettings.ASM_MERLIN32_EXECUTABLE, pathName);
            }
        }

        private void clearMerlin32Button_Click(object sender, EventArgs e) {
            merlin32PathTextBox.Text = string.Empty;
            mSettings.SetString(AppSettings.ASM_MERLIN32_EXECUTABLE, null);
            SetDirty(true);
        }

        private void merlin32PathTextBox_TextChanged(object sender, EventArgs e) {
            mSettings.SetString(AppSettings.ASM_MERLIN32_EXECUTABLE, merlin32PathTextBox.Text);
            SetDirty(true);
        }

        /// <summary>
        /// Creates a file dialog to search for a specific executable.
        /// </summary>
        /// <param name="prefix">Human-readable filter string for UI.</param>
        /// <param name="name">Filename of executable.</param>
        /// <returns>Path of executable, or null if dialog was canceled.</returns>
        private string BrowseForExecutable(string prefix, string name) {
            string pathName = null;

            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = name;
            dlg.Filter = prefix + "|" + name;
            dlg.RestoreDirectory = true;
            if (dlg.ShowDialog() != DialogResult.Cancel) {
                pathName = dlg.FileName;
                SetDirty(true);
            }
            dlg.Dispose();

            return pathName;
        }

        private void showAsmIdentCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, showAsmIdentCheckBox.Checked);
            SetDirty(true);
        }

        private void disableLabelLocalizationCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION,
                disableLabelLocalizationCheckBox.Checked);
            SetDirty(true);
        }

        private void longLabelNewLineCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE,
                longLabelNewLineCheckBox.Checked);
            SetDirty(true);
        }

        private void showCycleCountsCheckBox_CheckedChanged(object sender, EventArgs e) {
            mSettings.SetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS,
                showCycleCountsCheckBox.Checked);
            SetDirty(true);
        }

        #endregion Asm Config


        #region Display Format

        /// <summary>
        /// Populates the width disambiguation text boxes.
        /// </summary>
        private void PopulateWidthDisamSettings() {
            // Operand width disambiguation.  This is a little tricky -- we have to query all
            // settings then set all controls, or the field-updated callback may interfere
            // with us by changing AppSettings.
            string opcSuffixAbs = mSettings.GetString(AppSettings.FMT_OPCODE_SUFFIX_ABS,
                string.Empty);
            string opcSuffixLong = mSettings.GetString(AppSettings.FMT_OPCODE_SUFFIX_LONG,
                string.Empty);
            string opPrefixAbs = mSettings.GetString(AppSettings.FMT_OPERAND_PREFIX_ABS,
                string.Empty);
            string opPrefixLong = mSettings.GetString(AppSettings.FMT_OPERAND_PREFIX_LONG,
                string.Empty);

            disambSuffix16TextBox.Text = opcSuffixAbs;
            disambSuffix24TextBox.Text = opcSuffixLong;
            disambPrefix16TextBox.Text = opPrefixAbs;
            disambPrefix24TextBox.Text = opPrefixLong;
        }

        /// <summary>
        /// Sets all of the width disambiguation settings.  Used for the quick-set buttons.
        /// </summary>
        private void SetWidthDisamSettings(string opcodeSuffixAbs, string opcodeSuffixLong,
                string operandPrefixAbs, string operandPrefixLong) {
            mSettings.SetString(AppSettings.FMT_OPCODE_SUFFIX_ABS, opcodeSuffixAbs);
            mSettings.SetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, opcodeSuffixLong);
            mSettings.SetString(AppSettings.FMT_OPERAND_PREFIX_ABS, operandPrefixAbs);
            mSettings.SetString(AppSettings.FMT_OPERAND_PREFIX_LONG, operandPrefixLong);
            PopulateWidthDisamSettings();
        }

        // Called when text is typed.
        private void WidthDisamControlChanged(object sender, EventArgs e) {
            ExportWidthDisamSettings();
        }

        /// <summary>
        /// Exports the current state of the width controls to the settings object.
        /// </summary>
        private void ExportWidthDisamSettings() {
            mSettings.SetString(AppSettings.FMT_OPCODE_SUFFIX_ABS, disambSuffix16TextBox.Text);
            mSettings.SetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, disambSuffix24TextBox.Text);
            mSettings.SetString(AppSettings.FMT_OPERAND_PREFIX_ABS, disambPrefix16TextBox.Text);
            mSettings.SetString(AppSettings.FMT_OPERAND_PREFIX_LONG, disambPrefix24TextBox.Text);
            SetDirty(true);

            //Debug.WriteLine("disam: '" +
            //    mSettings.GetString(AppSettings.FMT_OPCODE_SUFFIX_ABS, string.Empty) + "' '" +
            //    mSettings.GetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, string.Empty) + "' '" +
            //    mSettings.GetString(AppSettings.FMT_OPERAND_PREFIX_ABS, string.Empty) + "' '" +
            //    mSettings.GetString(AppSettings.FMT_OPERAND_PREFIX_LONG, string.Empty) + "'");
        }

        private void shiftAfterAdjustCheckBox_CheckedChanged(object sender, EventArgs e) {
            string mode = useMerlinExpressions.Checked ?
                Asm65.Formatter.FormatConfig.ExpressionMode.Merlin.ToString() :
                Asm65.Formatter.FormatConfig.ExpressionMode.Simple.ToString();
            mSettings.SetString(AppSettings.FMT_EXPRESSION_MODE, mode);
            SetDirty(true);
        }

        private void quickFmtDefaultButton_Click(object sender, EventArgs e) {
            SetWidthDisamSettings(null, "l", "a:", "f:");
            useMerlinExpressions.Checked = false;
            // dirty flag set by change callbacks
        }

        private void quickFmtCc65Button_Click(object sender, EventArgs e) {
            SetWidthDisamSettings(null, null, "a:", "f:");
            useMerlinExpressions.Checked = false;
            // dirty flag set by change callbacks
        }

        private void quickFmtMerlin32Button_Click(object sender, EventArgs e) {
            SetWidthDisamSettings(":", "l", null, null);
            useMerlinExpressions.Checked = true;
            // dirty flag set by change callbacks
        }

        #endregion Display Format


        #region Pseudo-Op

        /// <summary>
        /// Imports values from PseudoOpNames struct into text fields.
        /// </summary>
        private void ImportPseudoOpNames(PseudoOp.PseudoOpNames opNames) {
            for (int i = 0; i < mPseudoNameMap.Length; i++) {
                string str = (string)mPseudoNameMap[i].PropInfo.GetValue(opNames);
                mPseudoNameMap[i].TextBox.Text = (str == null) ? string.Empty : str;
            }
        }

        /// <summary>
        /// Exports values from text fields to a PseudoOpNames object.
        /// </summary>
        private PseudoOp.PseudoOpNames ExportPseudoOpNames() {
            PseudoOp.PseudoOpNames opNames = new PseudoOp.PseudoOpNames();
            for (int i = 0; i < mPseudoNameMap.Length; i++) {
                // NOTE: PseudoOpNames must be a class (not a struct) or this will fail.
                // SetValue() would be invoked on a boxed copy that is discarded afterward.
                mPseudoNameMap[i].PropInfo.SetValue(opNames, mPseudoNameMap[i].TextBox.Text);
            }
            return opNames;
        }

        // Invoked when text is changed in any pseudo-op text box.
        private void PseudoOpTextChanged(object sender, EventArgs e) {
            // Just set the dirty flag.  The (somewhat expensive) export will happen
            // on Apply/OK.
            SetDirty(true);
        }

        private void quickPseudoDefaultButton_Click(object sender, EventArgs e) {
            ImportPseudoOpNames(new PseudoOp.PseudoOpNames());
        }

        private void quickPseudoCc65Button_Click(object sender, EventArgs e) {
            ImportPseudoOpNames(new PseudoOp.PseudoOpNames() {
                EquDirective = "=",
                OrgDirective = ".org",
                DefineData1 = ".byte",
                DefineData2 = ".word",
                DefineData3 = ".faraddr",
                DefineData4 = ".dword",
                DefineBigData2 = ".dbyt",
                Fill = ".res",
                StrGeneric = ".byte",
                StrNullTerm = ".asciiz",
            });
        }

        private void quickPseudoMerlin32_Click(object sender, EventArgs e) {
            // Note this doesn't quite match up with the Merlin generator, which uses
            // the same pseudo-op for low/high ASCII but different string delimiters.  We
            // don't change the delimiters for the display list, so we want to tweak the
            // opcode slightly.
            //char hiAscii = '\u21e1';
            char hiAscii = '\u2191';
            ImportPseudoOpNames(new PseudoOp.PseudoOpNames() {
                EquDirective = "equ",
                OrgDirective = "org",
                DefineData1 = "dfb",
                DefineData2 = "dw",
                DefineData3 = "adr",
                DefineData4 = "adrl",
                DefineBigData2 = "ddb",
                Fill = "ds",
                Dense = "hex",
                StrGeneric = "asc",
                StrGenericHi = "asc" + hiAscii,
                StrReverse = "rev",
                StrReverseHi = "rev" + hiAscii,
                StrLen8 = "str",
                StrLen8Hi = "str" + hiAscii,
                StrLen16 = "strl",
                StrLen16Hi = "strl" + hiAscii,
                StrDci = "dci",
                StrDciHi = "dci" + hiAscii,
            });
        }

        #endregion Pseudo-Op
    }
}
