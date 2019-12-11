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
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

using Asm65;
using CommonUtil;

using AssemblerInfo = SourceGen.AsmGen.AssemblerInfo;
using AssemblerConfig = SourceGen.AsmGen.AssemblerConfig;
using ExpressionMode = Asm65.Formatter.FormatConfig.ExpressionMode;
using System.Windows.Input;

namespace SourceGen.WpfGui {
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
            Unknown = 0,
            CodeView,
            TextDelimiters,
            AsmConfig,
            DisplayFormat,
            PseudoOp
        }

        /// <summary>
        /// Tab to show when dialog is first opened.
        /// </summary>
        private Tab mInitialTab;

        /// <summary>
        /// Assembler to initially select in combo boxes.
        /// </summary>
        private AssemblerInfo.Id mInitialAsmId;

        /// <summary>
        /// List of assemblers, for combo boxes.
        /// </summary>
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

            // Create an assembler list for the assembler-config combo box and the two
            // "quick set" combo boxes.
            AssemblerList = new List<AssemblerInfo>();
            IEnumerator<AssemblerInfo> iter = AssemblerInfo.GetInfoEnumerator();
            while (iter.MoveNext()) {
                AssemblerList.Add(iter.Current);
            }
            // Can't set the selected item yet.

            Construct_PseudoOp();
            Construct_DisplayFormat();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Loaded_CodeView();
            Loaded_TextDelimiters();
            Loaded_AsmConfig();
            Loaded_DisplayFormat();
            Loaded_PseudoOp();

            switch (mInitialTab) {
                case Tab.CodeView:
                    tabControl.SelectedItem = codeViewTab;
                    break;
                case Tab.TextDelimiters:
                    tabControl.SelectedItem = textDelimitersTab;
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
            PseudoOp.PseudoOpNames opNames = ExportPseudoOpNames();
            string pseudoCereal = opNames.Serialize();
            mSettings.SetString(AppSettings.FMT_PSEUDO_OP_NAMES, pseudoCereal);

            Formatter.DelimiterSet charSet = ExportDelimiters(mCharDtb);
            string charCereal = charSet.Serialize();
            mSettings.SetString(AppSettings.FMT_CHAR_DELIM, charCereal);
            Formatter.DelimiterSet stringSet = ExportDelimiters(mStringDtb);
            string stringCereal = stringSet.Serialize();
            mSettings.SetString(AppSettings.FMT_STRING_DELIM, stringCereal);

            try {
                // QueryVersions() can sometimes be slow under Win10 (mid 2019), possibly
                // because of the built-in malware detection, so pop up a wait cursor.
                Mouse.OverrideCursor = Cursors.Wait;
                mMainCtrl.SetAppSettings(mSettings);
                AsmGen.AssemblerVersionCache.QueryVersions();
            } finally {
                Mouse.OverrideCursor = null;
            }

            IsDirty = false;
        }


        #region Code View

        /// <summary>
        /// Entries for the clipboard format item combo box.
        /// </summary>
        public class ClipboardFormatItem {
            public string Name { get; private set; }
            public MainController.ClipLineFormat Value { get; private set; }

            public ClipboardFormatItem(string name, MainController.ClipLineFormat value) {
                Name = name;
                Value = value;
            }
        }
        // NOTE: in the current implementation, the array index must match the enum value
        private static ClipboardFormatItem[] sClipboardFormatItems = {
            new ClipboardFormatItem(Res.Strings.CLIPFORMAT_ASSEMBLER_SOURCE,
                MainController.ClipLineFormat.AssemblerSource),
            new ClipboardFormatItem(Res.Strings.CLIPFORMAT_DISASSEMBLY,
                MainController.ClipLineFormat.Disassembly),
            new ClipboardFormatItem(Res.Strings.CLIPFORMAT_ALL_COLUMNS,
                MainController.ClipLineFormat.AllColumns)
        };
        // ItemsSource for combo box
        public ClipboardFormatItem[] ClipboardFormatItems {
            get { return sClipboardFormatItems; }
        }

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

            Debug.Assert(clipboardFormatComboBox.Items.Count == sClipboardFormatItems.Length);
            int clipIndex = mSettings.GetEnum(AppSettings.CLIP_LINE_FORMAT,
                typeof(MainController.ClipLineFormat), 0);
            if (clipIndex >= 0 && clipIndex < sClipboardFormatItems.Length) {
                // require Value == clipIndex because we're lazy and don't want to search
                Debug.Assert((int)sClipboardFormatItems[clipIndex].Value == clipIndex);
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

        public bool UpperHexValues {
            get { return mSettings.GetBool(AppSettings.FMT_UPPER_HEX_DIGITS, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_UPPER_HEX_DIGITS, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool UpperOpcodes {
            get { return mSettings.GetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_UPPER_OP_MNEMONIC, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool UpperPseudoOps {
            get { return mSettings.GetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_UPPER_PSEUDO_OP_MNEMONIC, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool UpperOperandA {
            get { return mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_A, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_A, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool UpperOperandS {
            get { return mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_S, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_S, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool UpperOperandXY {
            get { return mSettings.GetBool(AppSettings.FMT_UPPER_OPERAND_XY, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_UPPER_OPERAND_XY, value);
                OnPropertyChanged();
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
            ClipboardFormatItem item = (ClipboardFormatItem)clipboardFormatComboBox.SelectedItem;
            mSettings.SetEnum(AppSettings.CLIP_LINE_FORMAT, typeof(MainController.ClipLineFormat),
                (int)item.Value);
            IsDirty = true;
        }

        public bool SpacesBetweenBytes {
            get { return mSettings.GetBool(AppSettings.FMT_SPACES_BETWEEN_BYTES, false); }
            set {
                mSettings.SetBool(AppSettings.FMT_SPACES_BETWEEN_BYTES, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }

        public bool DarkColorScheme {
            get { return mSettings.GetBool(AppSettings.SKIN_DARK_COLOR_SCHEME, false); }
            set {
                mSettings.SetBool(AppSettings.SKIN_DARK_COLOR_SCHEME, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }

        public bool EnableDebugMenu {
            get { return mSettings.GetBool(AppSettings.DEBUG_MENU_ENABLED, false); }
            set {
                mSettings.SetBool(AppSettings.DEBUG_MENU_ENABLED, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }

        #endregion Code View

        #region Text Delimiters

        private class DelimiterTextBoxes {
            public CharEncoding.Encoding mEncoding;
            public TextBox mPrefix;
            public TextBox mOpen;
            public TextBox mClose;
            public TextBox mSuffix;

            public DelimiterTextBoxes(CharEncoding.Encoding enc, TextBox prefix, TextBox open,
                    TextBox close, TextBox suffix) {
                mEncoding = enc;
                mPrefix = prefix;
                mOpen = open;
                mClose = close;
                mSuffix = suffix;
            }
        }
        private DelimiterTextBoxes[] mCharDtb;
        private DelimiterTextBoxes[] mStringDtb;

        private void Loaded_TextDelimiters() {
            // Map text boxes to delimiter definitions.
            mCharDtb = new DelimiterTextBoxes[] {
                new DelimiterTextBoxes(CharEncoding.Encoding.Ascii,
                    chrdAsciiPrefix, chrdAsciiOpen, chrdAsciiClose, chrdAsciiSuffix),
                new DelimiterTextBoxes(CharEncoding.Encoding.HighAscii,
                    chrdHighAsciiPrefix, chrdHighAsciiOpen, chrdHighAsciiClose, chrdHighAsciiSuffix),
                new DelimiterTextBoxes(CharEncoding.Encoding.C64Petscii,
                    chrdPetsciiPrefix, chrdPetsciiOpen, chrdPetsciiClose, chrdPetsciiSuffix),
                new DelimiterTextBoxes(CharEncoding.Encoding.C64ScreenCode,
                    chrdScreenCodePrefix, chrdScreenCodeOpen, chrdScreenCodeClose, chrdScreenCodeSuffix),
            };
            mStringDtb = new DelimiterTextBoxes[] {
                new DelimiterTextBoxes(CharEncoding.Encoding.Ascii,
                    strdAsciiPrefix, strdAsciiOpen, strdAsciiClose, strdAsciiSuffix),
                new DelimiterTextBoxes(CharEncoding.Encoding.HighAscii,
                    strdHighAsciiPrefix, strdHighAsciiOpen, strdHighAsciiClose, strdHighAsciiSuffix),
                new DelimiterTextBoxes(CharEncoding.Encoding.C64Petscii,
                    strdPetsciiPrefix, strdPetsciiOpen, strdPetsciiClose, strdPetsciiSuffix),
                new DelimiterTextBoxes(CharEncoding.Encoding.C64ScreenCode,
                    strdScreenCodePrefix, strdScreenCodeOpen, strdScreenCodeClose, strdScreenCodeSuffix),
            };

            string charDelimCereal = mSettings.GetString(AppSettings.FMT_CHAR_DELIM, null);
            Formatter.DelimiterSet chrSet;
            if (!string.IsNullOrEmpty(charDelimCereal)) {
                chrSet = Formatter.DelimiterSet.Deserialize(charDelimCereal);
            } else {
                chrSet = new Formatter.DelimiterSet();
            }
            ImportDelimiters(chrSet, mCharDtb);

            string stringDelimCereal = mSettings.GetString(AppSettings.FMT_STRING_DELIM, null);
            Formatter.DelimiterSet strSet;
            if (!string.IsNullOrEmpty(stringDelimCereal)) {
                strSet = Formatter.DelimiterSet.Deserialize(stringDelimCereal);
            } else {
                strSet = new Formatter.DelimiterSet();
            }
            ImportDelimiters(strSet, mStringDtb);

            // Create text field listeners.  Do this last, so the imports don't set dirty flag.
            foreach (DelimiterTextBoxes boxes in mCharDtb) {
                boxes.mPrefix.TextChanged += DelimiterTextChanged;
                boxes.mOpen.TextChanged += DelimiterTextChanged;
                boxes.mClose.TextChanged += DelimiterTextChanged;
                boxes.mSuffix.TextChanged += DelimiterTextChanged;
            }
            foreach (DelimiterTextBoxes boxes in mStringDtb) {
                boxes.mPrefix.TextChanged += DelimiterTextChanged;
                boxes.mOpen.TextChanged += DelimiterTextChanged;
                boxes.mClose.TextChanged += DelimiterTextChanged;
                boxes.mSuffix.TextChanged += DelimiterTextChanged;
            }
        }

        // Import delimiters from a DelimiterSet to the text fields.
        private void ImportDelimiters(Formatter.DelimiterSet delSet, DelimiterTextBoxes[] boxarr) {
            foreach (DelimiterTextBoxes boxes in boxarr) {
                Formatter.DelimiterDef def = delSet.Get(boxes.mEncoding);
                if (def == null) {
                    def = Formatter.DOUBLE_QUOTE_DELIM;
                }
                boxes.mPrefix.Text = def.Prefix;
                boxes.mOpen.Text = "" + def.OpenDelim;
                boxes.mClose.Text = "" + def.CloseDelim;
                boxes.mSuffix.Text = def.Suffix;
            }
        }

        // Export delimiters from the text fields to a DelimiterSet.
        private Formatter.DelimiterSet ExportDelimiters(DelimiterTextBoxes[] boxarr) {
            Formatter.DelimiterSet delSet = new Formatter.DelimiterSet();
            foreach (DelimiterTextBoxes boxes in boxarr) {
                char open = boxes.mOpen.Text.Length > 0 ? boxes.mOpen.Text[0] : '!';
                char close = boxes.mClose.Text.Length > 0 ? boxes.mClose.Text[0] : '!';
                Formatter.DelimiterDef def = new Formatter.DelimiterDef(
                    boxes.mPrefix.Text, open, close, boxes.mSuffix.Text);
                delSet.Set(boxes.mEncoding, def);
            }
            return delSet;
        }

        // Invoked when text is changed in any delimiter text box.
        private void DelimiterTextChanged(object sender, EventArgs e) {
            IsDirty = true;
        }

        private void ChrDelDefaultsButton_Click(object sender, RoutedEventArgs e) {
            Formatter.DelimiterSet chrDel = Formatter.DelimiterSet.GetDefaultCharDelimiters();
            ImportDelimiters(chrDel, mCharDtb);
        }

        private void StrDelDefaultsButton_Click(object sender, RoutedEventArgs e) {
            Formatter.DelimiterSet strDel = Formatter.DelimiterSet.GetDefaultStringDelimiters();
            ImportDelimiters(strDel, mStringDtb);
        }

        #endregion Text Delimiters

        #region Asm Config

        public const int ASM_COL_MIN_WIDTH = 1;
        public const int ASM_COL_MAX_WIDTH = 200;

        //
        // Numeric input fields, bound directly to TextBox.Text.  This performs the basic
        // validation, but we also do our own so we can cap min/max.  The custom validation
        // rule seems to fire ahead of the field assignment, so if our rule fails we won't
        // try to assign here at all.
        //
        // The validation approach seems to make the most sense here because we don't dim the
        // Apply/OK buttons when invalid input is present.  Instead we just set the width to
        // the minimum value when validation fails.
        //
        // Note that the "set" property is only called when the value changes, which only
        // happens when a valid integer is typed.  If you enter garbage, the dirty flag doesn't
        // get set.  That's just fine for us, but if you need to disable an "is valid" flag
        // on bad input you can't rely on updating it at "set" time.
        //
        // See also https://stackoverflow.com/a/44586784/294248
        //
        private int mAsmLabelColWidth;
        public int AsmLabelColWidth {
            get { return mAsmLabelColWidth; }
            set {
                if (mAsmLabelColWidth != value) {
                    mAsmLabelColWidth = value;
                    OnPropertyChanged();
                    AsmColWidthTextChanged();
                }
            }
        }
        private int mAsmOpcodeColWidth;
        public int AsmOpcodeColWidth {
            get { return mAsmOpcodeColWidth; }
            set {
                if (mAsmOpcodeColWidth != value) {
                    mAsmOpcodeColWidth = value;
                    OnPropertyChanged();
                    AsmColWidthTextChanged();
                }
            }
        }
        private int mAsmOperandColWidth;
        public int AsmOperandColWidth {
            get { return mAsmOperandColWidth; }
            set {
                if (mAsmOperandColWidth != value) {
                    mAsmOperandColWidth = value;
                    OnPropertyChanged();
                    AsmColWidthTextChanged();
                }
            }
        }
        private int mAsmCommentColWidth;
        public int AsmCommentColWidth {
            get { return mAsmCommentColWidth; }
            set {
                if (mAsmCommentColWidth != value) {
                    mAsmCommentColWidth = value;
                    OnPropertyChanged();
                    AsmColWidthTextChanged();
                }
            }
        }

        // checkboxes
        public bool ShowCycleCounts {
            get { return mSettings.GetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, false); }
            set {
                mSettings.SetBool(AppSettings.SRCGEN_SHOW_CYCLE_COUNTS, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool LongLabelNewLine {
            get { return mSettings.GetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, false); }
            set {
                mSettings.SetBool(AppSettings.SRCGEN_LONG_LABEL_NEW_LINE, value);
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool AddIdentComment {
            get { return mSettings.GetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, false); }
            set {
                mSettings.SetBool(AppSettings.SRCGEN_ADD_IDENT_COMMENT, value);
                OnPropertyChanged();
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
        /// Updates the assembler config settings whenever one of the width text fields is edited.
        /// </summary>
        /// <remarks>
        /// This fires 4x every time the combo box selection changes, as the new fields are
        /// populated.  That means we'll have incorrect intermediate states, but should
        /// finish up correctly.
        /// </remarks>
        private void AsmColWidthTextChanged() {
            AssemblerInfo asm = (AssemblerInfo)asmConfigComboBox.SelectedItem;
            if (asm == null) {
                // fires during dialog initialization, before anything is selected
                return;
            }
            AssemblerConfig.SetConfig(mSettings, asm.AssemblerId, GetAsmConfigFromUi());
            IsDirty = true;
        }

        /// <summary>
        /// Extracts the asm configuration items (exe path, column widths) from the UI.
        /// </summary>
        /// <returns></returns>
        private AssemblerConfig GetAsmConfigFromUi() {
            int[] widths = new int[4];

            if (!Validation.GetHasError(asmLabelColWidthTextBox)) {
                widths[0] = AsmLabelColWidth;
            } else {
                widths[0] = ASM_COL_MIN_WIDTH;
            }
            if (!Validation.GetHasError(asmOpcodeColWidthTextBox)) {
                widths[1] = AsmOpcodeColWidth;
            } else {
                widths[1] = ASM_COL_MIN_WIDTH;
            }
            if (!Validation.GetHasError(asmOperandColWidthTextBox)) {
                widths[2] = AsmOperandColWidth;
            } else {
                widths[2] = ASM_COL_MIN_WIDTH;
            }
            if (!Validation.GetHasError(asmCommentColWidthTextBox)) {
                widths[3] = AsmCommentColWidth;
            } else {
                widths[3] = ASM_COL_MIN_WIDTH;
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

        private void AsmExePathTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            if (IsLoaded) {
                // We don't really need to be updating AssemblerConfig every time they type
                // a character, but it's fine.
                AssemblerInfo asmInfo = (AssemblerInfo)asmConfigComboBox.SelectedItem;
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

        public string mOpcodeSuffixAbs;
        public string OpcodeSuffixAbs {
            get { return mOpcodeSuffixAbs; }
            set {
                if (mOpcodeSuffixAbs != value) {
                    mOpcodeSuffixAbs = value;
                    OnPropertyChanged();
                    mSettings.SetString(AppSettings.FMT_OPCODE_SUFFIX_ABS, value);
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }
        public string mOpcodeSuffixLong;
        public string OpcodeSuffixLong {
            get { return mOpcodeSuffixLong; }
            set {
                if (mOpcodeSuffixLong != value) {
                    mOpcodeSuffixLong = value;
                    OnPropertyChanged();
                    mSettings.SetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, value);
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }
        public string mOperandPrefixAbs;
        public string OperandPrefixAbs {
            get { return mOperandPrefixAbs; }
            set {
                if (mOperandPrefixAbs != value) {
                    mOperandPrefixAbs = value;
                    OnPropertyChanged();
                    mSettings.SetString(AppSettings.FMT_OPERAND_PREFIX_ABS, value);
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }
        public string mOperandPrefixLong;
        public string OperandPrefixLong {
            get { return mOperandPrefixLong; }
            set {
                if (mOperandPrefixLong != value) {
                    mOperandPrefixLong = value;
                    OnPropertyChanged();
                    mSettings.SetString(AppSettings.FMT_OPERAND_PREFIX_LONG, value);
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }
        private string mNonUniqueLabelPrefix;
        public string NonUniqueLabelPrefix {
            get { return mNonUniqueLabelPrefix; }
            set {
                if (mNonUniqueLabelPrefix != value) {
                    mNonUniqueLabelPrefix = value;
                    OnPropertyChanged();
                    bool doSave = true;
                    if (value.Length > 0) {
                        char ch = value[0];
                        doSave = !((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') ||
                            ch == '_');
                    }
                    if (doSave) {
                        mSettings.SetString(AppSettings.FMT_NON_UNIQUE_LABEL_PREFIX, value);
                    } else {
                        // TODO(someday): add a validation rule
                        Debug.WriteLine("NOTE: quietly rejecting non-unique prefix '" +
                            value + "'");
                    }
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }
        private string mLocalVarPrefix;
        public string LocalVarPrefix {
            get { return mLocalVarPrefix; }
            set {
                if (mLocalVarPrefix != value) {
                    mLocalVarPrefix = value;
                    OnPropertyChanged();
                    mSettings.SetString(AppSettings.FMT_LOCAL_VARIABLE_PREFIX, value);
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }
        private bool mCommaSeparatedBulkData;
        public bool CommaSeparatedBulkData {
            get { return mCommaSeparatedBulkData; }
            set {
                if (mCommaSeparatedBulkData != value) {
                    mCommaSeparatedBulkData = value;
                    OnPropertyChanged();
                    mSettings.SetBool(AppSettings.FMT_COMMA_SEP_BULK_DATA, value);
                    UpdateDisplayFormatQuickCombo();
                    IsDirty = true;
                }
            }
        }

        // prevent recursion
        private bool mSettingDisplayFmtCombo;

        /// <summary>
        /// Holds an item for the expression style selection combo box.
        /// </summary>
        public class ExpressionStyleItem {
            // Enumerated mode.
            public ExpressionMode ExpMode { get; private set; }

            // Human-readable name for display.
            public string Name { get; private set; }

            public ExpressionStyleItem(ExpressionMode expMode, string name) {
                ExpMode = expMode;
                Name = name;
            }
        }
        private static ExpressionStyleItem[] sExpStyleItems;
        public ExpressionStyleItem[] ExpressionStyleItems {
            get { return sExpStyleItems; }
        }

        private void ConstructExpressionStyleItems() {
            sExpStyleItems = new ExpressionStyleItem[] {
                new ExpressionStyleItem(ExpressionMode.Common,
                    (string)FindResource("str_ExpStyleCommon")),
                new ExpressionStyleItem(ExpressionMode.Cc65,
                    (string)FindResource("str_ExpStyleCc65")),
                new ExpressionStyleItem(ExpressionMode.Merlin,
                    (string)FindResource("str_ExpStyleMerlin")),
            };
        }

        public class DisplayFormatPreset {
            public const int ID_CUSTOM = -2;
            public const int ID_DEFAULT = -1;
            public int Ident { get; private set; }      // positive values are AssemblerInfo.Id
            public string Name { get; private set; }
            public string OpcodeSuffixAbs { get; private set; }
            public string OpcodeSuffixLong { get; private set; }
            public string OperandPrefixAbs { get; private set; }
            public string OperandPrefixLong { get; private set; }
            public string NonUniqueLabelPrefix { get; private set; }
            public string LocalVarPrefix { get; private set; }
            public bool CommaSeparatedBulkData { get; private set; }
            public ExpressionMode ExpressionStyle { get; private set; }

            public DisplayFormatPreset(int id, string name, string opcSuffixAbs,
                    string opcSuffixLong, string operPrefixAbs, string operPrefixLong,
                    string nonUniqueLabelPrefix, string localVarPrefix, bool commaSepBulkData,
                    ExpressionMode expStyle) {
                Ident = id;
                Name = name;
                OpcodeSuffixAbs = opcSuffixAbs;
                OpcodeSuffixLong = opcSuffixLong;
                OperandPrefixAbs = operPrefixAbs;
                OperandPrefixLong = operPrefixLong;
                NonUniqueLabelPrefix = nonUniqueLabelPrefix;
                LocalVarPrefix = localVarPrefix;
                CommaSeparatedBulkData = commaSepBulkData;
                ExpressionStyle = expStyle;
            }
        }
        public DisplayFormatPreset[] DisplayPresets { get; private set; }

        private void ConstructDisplayPresets() {
            // "custom" must be in slot 0
            DisplayPresets = new DisplayFormatPreset[AssemblerList.Count + 2];
            DisplayPresets[0] = new DisplayFormatPreset(DisplayFormatPreset.ID_CUSTOM,
                (string)FindResource("str_PresetCustom"), string.Empty, string.Empty,
                string.Empty, string.Empty, string.Empty, string.Empty, false,
                ExpressionMode.Unknown);
            DisplayPresets[1] = new DisplayFormatPreset(DisplayFormatPreset.ID_DEFAULT,
                (string)FindResource("str_PresetDefault"), string.Empty, "l", "a:", "f:",
                string.Empty, string.Empty, false, ExpressionMode.Common);
            for (int i = 0; i < AssemblerList.Count; i++) {
                AssemblerInfo asmInfo = AssemblerList[i];
                AsmGen.IGenerator gen = AssemblerInfo.GetGenerator(asmInfo.AssemblerId);

                gen.GetDefaultDisplayFormat(out PseudoOp.PseudoOpNames unused,
                    out Asm65.Formatter.FormatConfig formatConfig);

                DisplayPresets[i + 2] = new DisplayFormatPreset((int)asmInfo.AssemblerId,
                    asmInfo.Name, formatConfig.mForceAbsOpcodeSuffix,
                    formatConfig.mForceLongOpcodeSuffix, formatConfig.mForceAbsOperandPrefix,
                    formatConfig.mForceLongOperandPrefix, formatConfig.mNonUniqueLabelPrefix,
                    formatConfig.mLocalVariableLabelPrefix, formatConfig.mCommaSeparatedDense,
                    formatConfig.mExpressionMode);
            }
        }

        private void Construct_DisplayFormat() {
            ConstructExpressionStyleItems();
            ConstructDisplayPresets();
        }

        private void Loaded_DisplayFormat() {
            // Set values from settings.
            OpcodeSuffixAbs =
                mSettings.GetString(AppSettings.FMT_OPCODE_SUFFIX_ABS, string.Empty);
            OpcodeSuffixLong =
                mSettings.GetString(AppSettings.FMT_OPCODE_SUFFIX_LONG, string.Empty);
            OperandPrefixAbs =
                mSettings.GetString(AppSettings.FMT_OPERAND_PREFIX_ABS, string.Empty);
            OperandPrefixLong =
                mSettings.GetString(AppSettings.FMT_OPERAND_PREFIX_LONG, string.Empty);
            NonUniqueLabelPrefix =
                mSettings.GetString(AppSettings.FMT_NON_UNIQUE_LABEL_PREFIX, string.Empty);
            LocalVarPrefix =
                mSettings.GetString(AppSettings.FMT_LOCAL_VARIABLE_PREFIX, string.Empty);
            CommaSeparatedBulkData =
                mSettings.GetBool(AppSettings.FMT_COMMA_SEP_BULK_DATA, false);

            string exprMode = mSettings.GetString(AppSettings.FMT_EXPRESSION_MODE, string.Empty);
            ExpressionMode mode;
            if (!Enum.TryParse<ExpressionMode>(exprMode, out mode)) {
                mode = ExpressionMode.Common;
            }
            SelectExpressionStyle(mode);

            // No need to set this to anything specific.
            UpdateDisplayFormatQuickCombo();
        }

        /// <summary>
        /// Changes the combo box selection to the desired mode.
        /// </summary>
        private void SelectExpressionStyle(ExpressionMode mode) {
            foreach (ExpressionStyleItem esi in expressionStyleComboBox.Items) {
                if (esi.ExpMode == mode) {
                    expressionStyleComboBox.SelectedItem = esi;
                    return;
                }
            }
            Debug.Assert(false, "Expression mode " + mode + " not found");
            expressionStyleComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles a change to the expression style.
        /// </summary>
        private void ExpressionStyleComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            ExpressionStyleItem esi = (ExpressionStyleItem)expressionStyleComboBox.SelectedItem;
            mSettings.SetString(AppSettings.FMT_EXPRESSION_MODE, esi.ExpMode.ToString());
            UpdateDisplayFormatQuickCombo();
            IsDirty = true;
        }

        private void DisplayFmtQuickComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            if (mSettingDisplayFmtCombo) {
                // We shouldn't actually recurse indefinitely because we'll eventually
                // decide that nothing is changing, but I feel better having this here.
                return;
            }

            DisplayFormatPreset preset = (DisplayFormatPreset)displayFmtQuickComboBox.SelectedItem;
            if (preset.Ident == DisplayFormatPreset.ID_CUSTOM) {
                // Not an actual preset.  Leave the combo box set to "Custom".
                return;
            }

            OpcodeSuffixAbs = preset.OpcodeSuffixAbs;
            OpcodeSuffixLong = preset.OpcodeSuffixLong;
            OperandPrefixAbs = preset.OperandPrefixAbs;
            OperandPrefixLong = preset.OperandPrefixLong;
            NonUniqueLabelPrefix = preset.NonUniqueLabelPrefix;
            LocalVarPrefix = preset.LocalVarPrefix;
            CommaSeparatedBulkData = preset.CommaSeparatedBulkData;

            SelectExpressionStyle(preset.ExpressionStyle);
            // dirty flag will be set by change watchers if one or more fields have changed
        }

        private void UpdateDisplayFormatQuickCombo() {
            mSettingDisplayFmtCombo = true;

            ExpressionStyleItem esi = (ExpressionStyleItem)expressionStyleComboBox.SelectedItem;
            ExpressionMode expMode = ExpressionMode.Unknown;
            if (esi != null) {
                expMode = esi.ExpMode;
            }

            // If the current settings match one of the quick sets, update the combo box to
            // match.  Otherwise, set it to "custom".
            displayFmtQuickComboBox.SelectedIndex = 0;
            for (int i = 1; i < DisplayPresets.Length; i++) {
                DisplayFormatPreset preset = DisplayPresets[i];
                if (OpcodeSuffixAbs == preset.OpcodeSuffixAbs &&
                        OpcodeSuffixLong == preset.OpcodeSuffixLong &&
                        OperandPrefixAbs == preset.OperandPrefixAbs &&
                        OperandPrefixLong == preset.OperandPrefixLong &&
                        NonUniqueLabelPrefix == preset.NonUniqueLabelPrefix &&
                        LocalVarPrefix == preset.LocalVarPrefix &&
                        CommaSeparatedBulkData == preset.CommaSeparatedBulkData &&
                        expMode == preset.ExpressionStyle) {
                    // match
                    displayFmtQuickComboBox.SelectedIndex = i;
                    break;
                }
            }

            mSettingDisplayFmtCombo = false;
        }

        #endregion Display Format

        #region PseudoOp

        // recursion preventer
        private bool mSettingPseudoOpCombo;

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

        private void ConstructPseudoOpMap() {
            // Map text boxes to PseudoOpName fields.
            mPseudoNameMap = new TextBoxPropertyMap[] {
                new TextBoxPropertyMap(equDirectiveTextBox, "EquDirective"),
                new TextBoxPropertyMap(varDirectiveTextBox, "VarDirective"),
                new TextBoxPropertyMap(orgDirectiveTextBox, "OrgDirective"),
                new TextBoxPropertyMap(regWidthDirectiveTextBox, "RegWidthDirective"),
                new TextBoxPropertyMap(defineData1TextBox, "DefineData1"),
                new TextBoxPropertyMap(defineData2TextBox, "DefineData2"),
                new TextBoxPropertyMap(defineData3TextBox, "DefineData3"),
                new TextBoxPropertyMap(defineData4TextBox, "DefineData4"),
                new TextBoxPropertyMap(defineBigData2TextBox, "DefineBigData2"),
                new TextBoxPropertyMap(fillTextBox, "Fill"),
                new TextBoxPropertyMap(denseTextBox, "Dense"),
                new TextBoxPropertyMap(junkTextBox, "Junk"),
                new TextBoxPropertyMap(alignTextBox, "Align"),
                new TextBoxPropertyMap(strGenericTextBox, "StrGeneric"),
                new TextBoxPropertyMap(strReverseTextBox, "StrReverse"),
                new TextBoxPropertyMap(strLen8TextBox, "StrLen8"),
                new TextBoxPropertyMap(strLen16TextBox, "StrLen16"),
                new TextBoxPropertyMap(strNullTermTextBox, "StrNullTerm"),
                new TextBoxPropertyMap(strDciTextBox, "StrDci"),
            };
        }

        public class PseudoOpPreset {
            public const int ID_CUSTOM = -2;
            public const int ID_DEFAULT = -1;
            public int Ident { get; private set; }      // positive values are AssemblerInfo.Id
            public string Name { get; private set; }
            public PseudoOp.PseudoOpNames OpNames { get; private set; }

            public PseudoOpPreset(int id, string name, PseudoOp.PseudoOpNames opNames) {
                Ident = id;
                Name = name;
                OpNames = opNames;
            }
        }

        public PseudoOpPreset[] PseudoOpPresets { get; private set; }

        private void ConstructPseudoOpPresets() {
            // "custom" must be in slot 0
            PseudoOpPresets = new PseudoOpPreset[AssemblerList.Count + 2];
            PseudoOpPresets[0] = new PseudoOpPreset(PseudoOpPreset.ID_CUSTOM,
                (string)FindResource("str_PresetCustom"), new PseudoOp.PseudoOpNames());
            PseudoOpPresets[1] = new PseudoOpPreset(PseudoOpPreset.ID_DEFAULT,
                (string)FindResource("str_PresetDefault"), new PseudoOp.PseudoOpNames());
            for (int i = 0; i < AssemblerList.Count; i++) {
                AssemblerInfo asmInfo = AssemblerList[i];
                AsmGen.IGenerator gen = AssemblerInfo.GetGenerator(asmInfo.AssemblerId);

                gen.GetDefaultDisplayFormat(out PseudoOp.PseudoOpNames opNames,
                    out Asm65.Formatter.FormatConfig unused);
                PseudoOpPresets[i + 2] = new PseudoOpPreset((int)asmInfo.AssemblerId,
                    asmInfo.Name, opNames);
            }
        }

        private void Construct_PseudoOp() {
            ConstructPseudoOpMap();
            ConstructPseudoOpPresets();
        }

        private void Loaded_PseudoOp() {
            string opStrCereal = mSettings.GetString(AppSettings.FMT_PSEUDO_OP_NAMES, null);
            if (!string.IsNullOrEmpty(opStrCereal)) {
                PseudoOp.PseudoOpNames opNames = PseudoOp.PseudoOpNames.Deserialize(opStrCereal);
                ImportPseudoOpNames(opNames);
            } else {
                // no data available, populate with blanks
                ImportPseudoOpNames(new PseudoOp.PseudoOpNames());
            }

            UpdatePseudoOpQuickCombo();

            // Create text field listeners.
            foreach (TextBoxPropertyMap pmap in mPseudoNameMap) {
                pmap.TextBox.TextChanged += PseudoOpTextChanged;
            }
        }

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
            Dictionary<string, string> dict = new Dictionary<string, string>();

            for (int i = 0; i < mPseudoNameMap.Length; i++) {
                // Use TrimEnd() to remove invisible trailing spaces, and reduce a string
                // that's nothing but blanks to empty.
                dict[mPseudoNameMap[i].PropInfo.Name] = mPseudoNameMap[i].TextBox.Text.TrimEnd();
            }
            return new PseudoOp.PseudoOpNames(dict);
        }

        // Invoked when text is changed in any pseudo-op text box.
        private void PseudoOpTextChanged(object sender, EventArgs e) {
            // Just set the dirty flag.  The (somewhat expensive) export will happen
            // on Apply/OK.
            UpdatePseudoOpQuickCombo();
            IsDirty = true;
        }

        private void UpdatePseudoOpQuickCombo() {
            mSettingPseudoOpCombo = true;

            PseudoOp.PseudoOpNames curNames = ExportPseudoOpNames();

            // If the current settings match one of the quick sets, update the combo box to
            // match.  Otherwise, set it to "custom".
            pseudoOpQuickComboBox.SelectedIndex = 0;
            for (int i = 1; i < PseudoOpPresets.Length; i++) {
                PseudoOpPreset preset = PseudoOpPresets[i];
                if (preset.OpNames == curNames) {
                    // match
                    pseudoOpQuickComboBox.SelectedIndex = i;
                    break;
                }
            }

            mSettingPseudoOpCombo = false;
        }

        private void PseudoOpQuickComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            if (mSettingPseudoOpCombo) {
                return;
            }

            PseudoOpPreset preset = (PseudoOpPreset)pseudoOpQuickComboBox.SelectedItem;
            if (preset.Ident == PseudoOpPreset.ID_CUSTOM) {
                // Not an actual preset.  Leave the combo box set to "Custom".
                return;
            }

            ImportPseudoOpNames(preset.OpNames);
        }

        #endregion PseudoOp
    }

    #region Validation rules

    /// <summary>
    /// Text entry validation rule for assembler column widths.
    /// </summary>
    public class AsmColWidthRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            // Validating TextBox input, so value should always be a string.  Check anyway.
            string strValue = Convert.ToString(value);
            if (string.IsNullOrEmpty(strValue)) {
                //Debug.WriteLine("VVV not string");
                return new ValidationResult(false, "Could not convert to string");
            }

            if (int.TryParse(strValue, out int result)) {
                if (result >= EditAppSettings.ASM_COL_MIN_WIDTH &&
                        result <= EditAppSettings.ASM_COL_MAX_WIDTH) {
                    return ValidationResult.ValidResult;
                }
                //Debug.WriteLine("VVV out of range: '" + strValue + "' (" + result + ")");
                return new ValidationResult(false, "Column width out of range");
            }

            //Debug.WriteLine("VVV not valid integer: '" + strValue + "'");
            return new ValidationResult(false, "Invalid integer value: '" + strValue + "'");
        }
    }

#if false
    /// <summary>
    /// Text entry validation rule for text string delimiter patterns.
    /// </summary>
    public class StringDelimiterRule : ValidationRule {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            string strValue = Convert.ToString(value);
            int firstHash = strValue.IndexOf('#');
            if (firstHash < 0) {
                return new ValidationResult(false, "Must include exactly one '#'");
            }
            if (strValue.LastIndexOf('#') != firstHash) {
                return new ValidationResult(false, "Found more than one '#'");
            }
            return ValidationResult.ValidResult;
        }
    }
#endif

    #endregion Validation rules
}
