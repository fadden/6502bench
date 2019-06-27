/*
 * Copyright 2019 faddenSoft
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
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using CommonUtil;

using AssemblerInfo = SourceGenWPF.AsmGen.AssemblerInfo;
using AssemblerConfig = SourceGenWPF.AsmGen.AssemblerConfig;
using ExpressionMode = Asm65.Formatter.FormatConfig.ExpressionMode;

namespace SourceGenWPF.WpfGui {
    /// <summary>
    /// Application settings dialog.
    /// </summary>
    public partial class EditAppSettings : Window, INotifyPropertyChanged {
        /// <summary>
        /// Reference to main window.  Needed for examination of the code list font and
        /// column widths.
        /// </summary>
        private MainWindow mMainWin;

        /// <summary>
        /// Reference to main controller.  Needed to push settings out when Apply/OK is clicked.
        /// </summary>
        private MainController mMainCtrl;

        /// <summary>
        /// Copy of settings that we make changes to.  On "Apply" or "OK", this is pushed
        /// into the global settings object, and applied to the ProjectView.
        /// </summary>
        private AppSettings mSettings;

        /// <summary>
        /// Dirty flag, set when anything in mSettings changes.  Determines whether or not
        /// the Apply button is enabled.
        /// </summary>
        public bool IsDirty {
            get { return mIsDirty; }
            set {
                mIsDirty = value;
                OnPropertyChanged();
            }
        }
        private bool mIsDirty;

        /// <summary>
        /// Tab page enumeration.
        /// </summary>
        public enum Tab {
            Unknown = -1,
            CodeView = 0,
            AsmConfig = 1,
            DisplayFormat = 2,
            PseudoOp = 3
        }

        /// <summary>
        /// Tab to show when dialog is first opened.
        /// </summary>
        private Tab mInitialTab;

        /// <summary>
        /// Assembler to initially select in combo boxes.
        /// </summary>
        private AssemblerInfo.Id mInitialAsmId;

        public List<AssemblerInfo> AssemblerList { get; private set; }


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditAppSettings(Window owner, MainWindow mainWin, MainController mainCtrl,
                Tab initialTab, AssemblerInfo.Id initialAsmId) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mMainWin = mainWin;
            mMainCtrl = mainCtrl;
            mInitialTab = initialTab;
            mInitialAsmId = initialAsmId;

            // Make a work copy, so we can discard changes if the user cancels out of the dialog.
            // Column width changes don't actively update settings, so grab them.
            mMainWin.CaptureColumnWidths();
            mSettings = AppSettings.Global.GetCopy();

            // Put column-width buttons in an array.
            mColButtons = new Button[] {
                showCol0, showCol1, showCol2, showCol3, showCol4,
                showCol5, showCol6, showCol7, showCol8 };
            Debug.Assert(NUM_COLUMNS == 9);

            // Extract format strings from column-width button labels.
            for (int i = 0; i < NUM_COLUMNS; i++) {
                //mColButtons[i].Click += ColumnVisibilityButtonClick;
                mColumnFormats[i] = (string) mColButtons[i].Content;
            }

#if false
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
#endif

            // Create an assembler list for the assembler-config combo box and the two
            // "quick set" combo boxes.
            AssemblerList = new List<AssemblerInfo>();
            IEnumerator<AssemblerInfo> iter = AssemblerInfo.GetInfoEnumerator();
            while (iter.MoveNext()) {
                AssemblerList.Add(iter.Current);
            }
            // Can't set the selected item yet.

#if false
            expressionStyleComboBox.DisplayMember = "Name";
            foreach (ExpressionStyleItem esi in sExpStyleItems) {
                expressionStyleComboBox.Items.Add(esi);
            }
#endif
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Loaded_CodeView();
            Loaded_AsmConfig();

#if false

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
            ExpressionMode mode;
            if (!Enum.TryParse<ExpressionMode>(exprMode, out mode)) {
                mode = ExpressionMode.Common;
            }
            SetExpressionStyle(mode);
#endif

            switch (mInitialTab) {
                case Tab.CodeView:
                    tabControl.SelectedItem = codeViewTab;
                    break;
                case Tab.AsmConfig:
                    tabControl.SelectedItem = asmConfigTab;
                    break;
                case Tab.DisplayFormat:
                    tabControl.SelectedItem = displayFormatTab;
                    break;
                case Tab.PseudoOp:
                    tabControl.SelectedItem = pseudoOpTab;
                    break;
                case Tab.Unknown:
                    break;
                default:
                    Debug.Assert(false);
                    break;

            }

            // The various control initializers probably triggered events.  Reset the dirty flag.
            IsDirty = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            ApplySettings();
            DialogResult = true;
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e) {
            ApplySettings();
        }

        private void ApplySettings() {
#if false
            PseudoOp.PseudoOpNames opNames = ExportPseudoOpNames();
            string pseudoCereal = opNames.Serialize();
            mSettings.SetString(AppSettings.FMT_PSEUDO_OP_NAMES, pseudoCereal);
#endif

            mMainCtrl.SetAppSettings(mSettings);
            AsmGen.AssemblerVersionCache.QueryVersions();
            IsDirty = false;
        }


        #region Code View

        private void Loaded_CodeView() {
            // Column widths.  We called CaptureColumnWidths() during init, so this
            // should always be a valid serialized string.
            string widthStr = mSettings.GetString(AppSettings.CDLV_COL_WIDTHS, null);
            Debug.Assert(!string.IsNullOrEmpty(widthStr));
            int[] widths = TextUtil.DeserializeIntArray(widthStr);
            Debug.Assert(widths != null);
            for (int i = 0; i < NUM_COLUMNS; i++) {
                SetShowHideButton(i, widths[i]);
            }

            // Set the string.  Note this shows what is currently set in the pending settings
            // object, *not* what's currently being rendered (until you hit Apply).
            string fontFamilyName = mSettings.GetString(AppSettings.CDLV_FONT_FAMILY, "BROKEN");
            int fontSize = mSettings.GetInt(AppSettings.CDLV_FONT_SIZE, 2);
            codeListFontDesc.Text = string.Format(Res.Strings.FONT_DESCRIPTOR_FMT,
                fontSize, fontFamilyName);

            // Upper-case formatting.
            UpperHexValues = mSettings.GetBool(AppSettings.FMT_UPPER_HEX_DIGITS, false);
            UpperOpcodes = mSettings.GetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, false);
            UpperPseudoOps = mSettings.GetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, false);
            UpperOperandA = mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_A, false);
            UpperOperandS = mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_S, false);
            UpperOperandXY = mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_XY, false);

            int clipIndex = mSettings.GetEnum(AppSettings.CLIP_LINE_FORMAT,
                typeof(MainController.ClipLineFormat), 0);
            if (clipIndex >= 0 && clipIndex < clipboardFormatComboBox.Items.Count) {
                // NOTE: this couples the ClipLineFormat enum to the XAML.
                clipboardFormatComboBox.SelectedIndex = clipIndex;
            }

            SpacesBetweenBytes = mSettings.GetBool(AppSettings.FMT_SPACES_BETWEEN_BYTES, false);
            EnableDebugMenu = mSettings.GetBool(AppSettings.DEBUG_MENU_ENABLED, false);
        }

        // Map buttons to column show/hide buttons.
        private const int NUM_COLUMNS = (int)MainController.CodeListColumn.COUNT;
        private string[] mColumnFormats = new string[NUM_COLUMNS];
        private Button[] mColButtons;

        /// <summary>
        /// Updates the text on a show/hide column button.
        /// </summary>
        /// <param name="index">Column index.</param>
        /// <param name="width">New width.</param>
        private void SetShowHideButton(int index, int width) {
            Button button = mColButtons[index];
            string fmt = mColumnFormats[index];
            string show = Res.Strings.SHOW_COL;
            string hide = Res.Strings.HIDE_COL;
            button.Content = string.Format(fmt, (width == 0) ? show : hide);
        }

        private void ColumnVisibilityButton_Click(object sender, RoutedEventArgs e) {
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
            int[] widths = TextUtil.DeserializeIntArray(widthStr);
            if (widths[index] == 0) {
                // Expand to default width.  The default width changes when the font
                // changes, so it's best to just reacquire the default width set as needed.
                int[] defaultWidths = mMainWin.GetDefaultCodeListColumnWidths();
                widths[index] = defaultWidths[index];
            } else {
                widths[index] = 0;
            }
            widthStr = TextUtil.SerializeIntArray(widths);
            mSettings.SetString(AppSettings.CDLV_COL_WIDTHS, widthStr);
            SetShowHideButton(index, widths[index]);

            IsDirty = true;
        }

        private void SelectFontButton_Click(object sender, RoutedEventArgs e) {
            FontPicker dlg = new FontPicker(this,
                mSettings.GetString(AppSettings.CDLV_FONT_FAMILY, string.Empty),
                mSettings.GetInt(AppSettings.CDLV_FONT_SIZE, 12));
            if (dlg.ShowDialog() == true) {
                string familyName = dlg.SelectedFamily.ToString();
                mSettings.SetString(AppSettings.CDLV_FONT_FAMILY, familyName);
                mSettings.SetInt(AppSettings.CDLV_FONT_SIZE, dlg.SelectedSize);

                codeListFontDesc.Text = string.Format(Res.Strings.FONT_DESCRIPTOR_FMT,
                    dlg.SelectedSize, familyName);
                IsDirty = true;
            }
        }

        private bool mUpperHexValues;
        public bool UpperHexValues {
            get { return mUpperHexValues; }
            set {
                mUpperHexValues = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_UPPER_HEX_DIGITS, value);
                IsDirty = true;
            }
        }
        private bool mUpperOpcodes;
        public bool UpperOpcodes {
            get { return mUpperOpcodes; }
            set {
                mUpperOpcodes = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, value);
                IsDirty = true;
            }
        }
        private bool mUpperPseudoOps;
        public bool UpperPseudoOps {
            get { return mUpperPseudoOps; }
            set {
                mUpperPseudoOps = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, value);
                IsDirty = true;
            }
        }
        private bool mUpperOperandA;
        public bool UpperOperandA {
            get { return mUpperOperandA; }
            set {
                mUpperOperandA = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_A, value);
                IsDirty = true;
            }
        }
        private bool mUpperOperandS;
        public bool UpperOperandS {
            get { return mUpperOperandS; }
            set {
                mUpperOperandS = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_S, value);
                IsDirty = true;
            }
        }
        private bool mUpperOperandXY;
        public bool UpperOperandXY {
            get { return mUpperOperandXY; }
            set {
                mUpperOperandXY = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_XY, value);
                IsDirty = true;
            }
        }

        private void AllLower_Click(object sender, RoutedEventArgs e) {
            UpperHexValues = UpperOpcodes = UpperPseudoOps =
                UpperOperandA = UpperOperandS = UpperOperandXY = false;
        }

        private void AllUpper_Click(object sender, RoutedEventArgs e) {
            UpperHexValues = UpperOpcodes = UpperPseudoOps =
                UpperOperandA = UpperOperandS = UpperOperandXY = true;
        }

        private void ClipboardFormatComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            // NOTE: again, this ties the combo box index to the enum value
            mSettings.SetEnum(AppSettings.CLIP_LINE_FORMAT, typeof(MainController.ClipLineFormat),
                clipboardFormatComboBox.SelectedIndex);
            IsDirty = true;
        }

        private bool mSpacesBetweenBytes;
        public bool SpacesBetweenBytes {
            get { return mSpacesBetweenBytes; }
            set {
                mSpacesBetweenBytes = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.FMT_SPACES_BETWEEN_BYTES, value);
                IsDirty = true;
            }
        }

        private bool mEnableDebugMenu;
        public bool EnableDebugMenu {
            get { return mEnableDebugMenu; }
            set {
                mEnableDebugMenu = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.DEBUG_MENU_ENABLED, value);
                IsDirty = true;
            }
        }

        #endregion Code View

        #region Asm Config

        private bool mShowCycleCounts;
        public bool ShowCycleCounts {
            get { return mShowCycleCounts; }
            set {
                mShowCycleCounts = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, value);
                IsDirty = true;
            }
        }
        private bool mLongLabelNewLine;
        public bool LongLabelNewLine {
            get { return mLongLabelNewLine; }
            set {
                mLongLabelNewLine = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, value);
                IsDirty = true;
            }
        }
        private bool mAddIdentComment;
        public bool AddIdentComment {
            get { return mAddIdentComment; }
            set {
                mAddIdentComment = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, value);
                IsDirty = true;
            }
        }
        private bool mDisableLabelLocalization;
        public bool DisableLabelLocalization {
            get { return mDisableLabelLocalization; }
            set {
                mDisableLabelLocalization = value;
                OnPropertyChanged();
                mSettings.SetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, value);
                IsDirty = true;
            }
        }

        private void Loaded_AsmConfig() {
            asmConfigComboBox.SelectedItem = AssemblerInfo.GetAssemblerInfo(mInitialAsmId);
            if (asmConfigComboBox.SelectedIndex < 0) {
                Debug.Assert(mInitialAsmId == AssemblerInfo.Id.Unknown);
                asmConfigComboBox.SelectedIndex = 0;
            }

            ShowCycleCounts =
                mSettings.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, false);
            LongLabelNewLine =
                mSettings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false);
            AddIdentComment =
                mSettings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false);
            DisableLabelLocalization =
                mSettings.GetBool(AppSettings.SRCGEN_DISABLE_LABEL_LOCALIZATION, false);
        }

        /// <summary>
        /// Populates the UI elements from the asm config item in the settings.  If that doesn't
        /// exist, use the default config.
        /// </summary>
        private void PopulateAsmConfigItems() {
            AssemblerInfo info = (AssemblerInfo)asmConfigComboBox.SelectedItem;

            AssemblerConfig config = AssemblerConfig.GetConfig(mSettings, info.AssemblerId);
            if (config == null) {
                AsmGen.IAssembler asm = AssemblerInfo.GetAssembler(info.AssemblerId);
                config = asm.GetDefaultConfig();
            }

            asmExePathTextBox.Text = config.ExecutablePath;
            asmLabelColWidthTextBox.Text = config.ColumnWidths[0].ToString();
            asmOpcodeColWidthTextBox.Text = config.ColumnWidths[1].ToString();
            asmOperandColWidthTextBox.Text = config.ColumnWidths[2].ToString();
            asmCommentColWidthTextBox.Text = config.ColumnWidths[3].ToString();
        }

        private void AsmConfigComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // They're switching to a different asm config.  Changing the boxes will cause
            // the dirty flag to be raised, which isn't right, so we save/restore it.
            bool oldDirty = IsDirty;
            PopulateAsmConfigItems();
            IsDirty = oldDirty;
        }

        /// <summary>
        /// Checks whether the character typed into a column width entry field is allowed.
        /// This is only useful for screening the character set, not the field contents.
        /// </summary>
        /// <remarks>
        /// This just screens the character.  Doesn't handle selection operations like pasting.
        /// Also, doesn't fire when you hit the space bar.  This is only slightly better than
        /// useless, but since we don't otherwise give an indication of wrongness it's nice
        /// to have.
        ///
        /// Another approach is to bind Text to an integer property.  This enables the validation
        /// mechanism, which puts a red box around the field when it contains bad things, but
        /// only after focus leaves the field.
        ///
        /// See also https://stackoverflow.com/q/1268552/294248
        /// </remarks>
        private void CheckWidthInput(object sender, TextCompositionEventArgs e) {
            // Set e.Handled to true if the character is invalid.
            char ch = e.Text[0];
            e.Handled = (ch < '0' || ch > '9');
        }

        /// <summary>
        /// Updates the assembler config settings whenever one of the text fields is edited.
        /// </summary>
        /// <remarks>
        /// This fires 4x every time the combo box selection changes, as the new fields are
        /// populated.  That should work out correctly.
        /// </remarks>
        private void AsmLabelColWidthTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            AssemblerInfo asm = (AssemblerInfo)asmConfigComboBox.SelectedItem;
            AssemblerConfig.SetConfig(mSettings, asm.AssemblerId, GetAsmConfigFromUi());
            IsDirty = true;
        }

        /// <summary>
        /// Extracts the asm configuration items (exe path, column widths) from the UI.
        /// </summary>
        /// <returns></returns>
        private AssemblerConfig GetAsmConfigFromUi() {
            const int MIN_WIDTH = 1;
            const int MAX_WIDTH = 200;


            int[] widths = new int[4];
            for (int i = 0; i < widths.Length; i++) {
                widths[i] = MIN_WIDTH;
            }

            int result;
            if (int.TryParse(asmLabelColWidthTextBox.Text, out result) && result >= MIN_WIDTH &&
                    result <= MAX_WIDTH) {
                widths[0] = result;
            }
            if (int.TryParse(asmOpcodeColWidthTextBox.Text, out result) && result >= MIN_WIDTH &&
                    result <= MAX_WIDTH) {
                widths[1] = result;
            }
            if (int.TryParse(asmOperandColWidthTextBox.Text, out result) && result >= MIN_WIDTH &&
                    result <= MAX_WIDTH) {
                widths[2] = result;
            }
            if (int.TryParse(asmCommentColWidthTextBox.Text, out result) && result >= MIN_WIDTH &&
                    result <= MAX_WIDTH) {
                widths[3] = result;
            }

            return new AssemblerConfig(asmExePathTextBox.Text, widths);
        }

        private void AsmExeBrowseButton_Click(object sender, RoutedEventArgs e) {
            AssemblerInfo asmInfo = (AssemblerInfo)asmConfigComboBox.SelectedItem;

            // Figure out what we're looking for.  For example, cc65 needs "cl65".
            AsmGen.IAssembler asm = AssemblerInfo.GetAssembler(asmInfo.AssemblerId);
            asm.GetExeIdentifiers(out string humanName, out string exeName);

            // Ask the user to find it.
            string pathName = BrowseForExecutable(humanName, exeName);
            if (pathName != null) {
                asmExePathTextBox.Text = pathName;
                AssemblerConfig.SetConfig(mSettings, asmInfo.AssemblerId, GetAsmConfigFromUi());
                IsDirty = true;
            }

        }

        /// <summary>
        /// Creates a file dialog to search for a specific executable.
        /// </summary>
        /// <param name="prefix">Human-readable filter string for UI.</param>
        /// <param name="name">Filename of executable.</param>
        /// <returns>Path of executable, or null if dialog was canceled.</returns>
        private string BrowseForExecutable(string prefix, string name) {
            string pathName = null;

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                name += ".exe";
            }

            OpenFileDialog dlg = new OpenFileDialog() {
                FileName = name,
                Filter = prefix + "|" + name,
                RestoreDirectory = true
            };
            if (dlg.ShowDialog() == true) {
                pathName = dlg.FileName;
            }

            return pathName;
        }

        #endregion AsmConfig

        #region Display Format

        /// <summary>
        /// Holds an item for the expression style selection combo box.
        /// </summary>
        private struct ExpressionStyleItem {
            // Enumerated mode.
            public ExpressionMode ExpMode { get; private set; }

            // Human-readable name for display.
            public string Name { get; private set; }

            public ExpressionStyleItem(ExpressionMode expMode, string name) {
                ExpMode = expMode;
                Name = name;
            }
        }
        private static ExpressionStyleItem[] sExpStyleItems = new ExpressionStyleItem[] {
            new ExpressionStyleItem(ExpressionMode.Common, "Common"),
            new ExpressionStyleItem(ExpressionMode.Cc65, "cc65"),
            new ExpressionStyleItem(ExpressionMode.Merlin, "Merlin"),
        };

        #endregion Display Format

        #region PseudoOp

        /// <summary>
        /// Map pseudo-op text entry fields to PseudoOpName properties.
        /// </summary>
        private class TextBoxPropertyMap {
            public TextBox TextBox { get; private set; }
            public PropertyInfo PropInfo { get; private set; }

            public TextBoxPropertyMap(TextBox textBox, string propName) {
                TextBox = textBox;
                PropInfo = typeof(PseudoOp.PseudoOpNames).GetProperty(propName);
            }
        }
        private TextBoxPropertyMap[] mPseudoNameMap;

        #endregion PseudoOp
    }
}
