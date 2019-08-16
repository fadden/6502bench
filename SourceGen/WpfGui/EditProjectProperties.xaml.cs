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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

using Asm65;
using CommonUtil;
using CommonWPF;
using TextScanMode = SourceGen.ProjectProperties.AnalysisParameters.TextScanMode;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Project properties dialog.
    /// </summary>
    public partial class EditProjectProperties : Window, INotifyPropertyChanged {
        /// <summary>
        /// New set.  Updated when Apply or OK is hit.  This will be null if no changes have
        /// been applied.
        /// </summary>
        public ProjectProperties NewProps { get; private set; }

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Working set.  Used internally to hold state.
        /// </summary>
        private ProjectProperties mWorkProps;

        /// <summary>
        /// Dirty flag.  Determines whether or not the Apply button is enabled.
        /// </summary>
        /// <remarks>
        /// Ideally this would just be "WorkProps != OldProps", but it doesn't
        /// seem worthwhile to maintain an equality operator.
        /// </remarks>
        public bool IsDirty {
            get { return mIsDirty; }
            set {
                mIsDirty = value;
                OnPropertyChanged();
            }
        }
        private bool mIsDirty;

        /// <summary>
        /// Project directory, if one has been established; otherwise empty.
        /// </summary>
        private string mProjectDir;


        /// <summary>
        /// Constructor.  Initial state is configured from an existing ProjectProperties object.
        /// </summary>
        /// <param name="props">Property holder to clone.</param>
        /// <param name="projectDir">Project directory, if known.</param>
        /// <param name="formatter">Text formatter.</param>
        public EditProjectProperties(Window owner, ProjectProperties props, string projectDir,
                Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mWorkProps = new ProjectProperties(props);
            mProjectDir = projectDir;
            mFormatter = formatter;

            // Construct arrays used as item sources for combo boxes.
            CpuItems = new CpuItem[] {
                new CpuItem((string)FindResource("str_6502"), CpuDef.CpuType.Cpu6502),
                new CpuItem((string)FindResource("str_65C02"), CpuDef.CpuType.Cpu65C02),
                new CpuItem((string)FindResource("str_65816"), CpuDef.CpuType.Cpu65816),
            };
            DefaultTextScanModeItems = new DefaultTextScanMode[] {
                new DefaultTextScanMode(Res.Strings.SCAN_LOW_ASCII,
                    TextScanMode.LowAscii),
                new DefaultTextScanMode(Res.Strings.SCAN_LOW_HIGH_ASCII,
                    TextScanMode.LowHighAscii),
                new DefaultTextScanMode(Res.Strings.SCAN_C64_PETSCII,
                    TextScanMode.C64Petscii),
                new DefaultTextScanMode(Res.Strings.SCAN_C64_SCREEN_CODE,
                    TextScanMode.C64ScreenCode),
            };
            MinCharsItems = new MinCharsItem[] {
                new MinCharsItem((string)FindResource("str_DisableStringScan"),
                    DataAnalysis.MIN_CHARS_FOR_STRING_DISABLED),
                new MinCharsItem("3", 3),
                new MinCharsItem("4", 4),
                new MinCharsItem("5", 5),
                new MinCharsItem("6", 6),
                new MinCharsItem("7", 7),
                new MinCharsItem("8", 8),
                new MinCharsItem("9", 9),
                new MinCharsItem("10", 10),
            };
            AutoLabelItems = new AutoLabelItem[] {
                new AutoLabelItem((string)FindResource("str_AutoLabelSimple"),
                    AutoLabel.Style.Simple),
                new AutoLabelItem((string)FindResource("str_AutoLabelAnnotated"),
                    AutoLabel.Style.Annotated),
                new AutoLabelItem((string)FindResource("str_AutoLabelFullyAnnotated"),
                    AutoLabel.Style.FullyAnnotated),
            };
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            Loaded_General();

            LoadProjectSymbols();
            LoadPlatformSymbolFiles();
            LoadExtensionScriptNames();

            UpdateControls();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e) {
            NewProps = new ProjectProperties(mWorkProps);
            IsDirty = false;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            // TODO(maybe): ideally we'd return false here if nothing has changed from the
            // state things were in when the dialog first opened.  Checking IsDirty is
            // insufficient.  Might be best to just let the UndoableChange stuff figure out
            // that nothing changed.
            NewProps = new ProjectProperties(mWorkProps);
            DialogResult = true;

            GridView view = (GridView)projectSymbolsListView.View;
            foreach (GridViewColumn header in view.Columns) {
                Debug.WriteLine("WIDTH " + header.ActualWidth);
            }
        }

        private void UpdateControls() {
            //
            // Project symbols tab
            //
            // Enable or disable the edit/remove buttons based on how many items are selected.
            // (We're currently configured for single-select, so this is really just a != 0 test.)
            int symSelCount = projectSymbolsListView.SelectedItems.Count;
            removeSymbolButton.IsEnabled = (symSelCount == 1);
            editSymbolButton.IsEnabled = (symSelCount == 1);

            //
            // Platform symbol files tab
            //
            int fileSelCount = symbolFilesListBox.SelectedItems.Count;
            symbolFileRemoveButton.IsEnabled = (fileSelCount != 0);
            symbolFileUpButton.IsEnabled = (fileSelCount == 1 &&
                symbolFilesListBox.SelectedIndex != 0);
            symbolFileDownButton.IsEnabled = (fileSelCount == 1 &&
                symbolFilesListBox.SelectedIndex != symbolFilesListBox.Items.Count - 1);

            //
            // Extension Scripts tab
            //
            fileSelCount = extensionScriptsListBox.SelectedItems.Count;
            extensionScriptRemoveButton.IsEnabled = (fileSelCount != 0);
        }

        /// <summary>
        /// Handles a change in the selection of any of the lists.
        /// </summary>
        private void List_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // Enable/disable buttons as the selection changes.
            UpdateControls();
        }


        #region General

        // CPU combo box items
        public class CpuItem {
            public string Name { get; private set; }
            public CpuDef.CpuType Type { get; private set; }

            public CpuItem(string name, CpuDef.CpuType type) {
                Name = name;
                Type = type;
            }
        }
        public CpuItem[] CpuItems { get; private set; }

        // Default text encoding combo box items
        public class DefaultTextScanMode {
            public string Name { get; private set; }
            public TextScanMode Mode { get; private set; }

            public DefaultTextScanMode(string name, TextScanMode mode) {
                Name = name;
                Mode = mode;
            }
        }
        public DefaultTextScanMode[] DefaultTextScanModeItems { get; private set; }

        // Min chars for string combo box items
        public class MinCharsItem {
            public string Name { get; private set; }
            public int Value { get; private set; }

            public MinCharsItem(string name, int value) {
                Name = name;
                Value = value;
            }
        }
        public MinCharsItem[] MinCharsItems { get; private set; }

        // Auto-label style combo box items
        public class AutoLabelItem {
            public string Name { get; private set; }
            public AutoLabel.Style Style { get; private set; }

            public AutoLabelItem(string name, AutoLabel.Style style) {
                Name = name;
                Style = style;
            }
        }
        public AutoLabelItem[] AutoLabelItems { get; private set; }

        // properties for checkboxes

        public bool IncludeUndocumentedInstr {
            get { return mWorkProps.IncludeUndocumentedInstr; }
            set {
                mWorkProps.IncludeUndocumentedInstr = value;
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool AnalyzeUncategorizedData {
            get { return mWorkProps.AnalysisParams.AnalyzeUncategorizedData; }
            set {
                mWorkProps.AnalysisParams.AnalyzeUncategorizedData = value;
                OnPropertyChanged();
                IsDirty = true;
            }
        }
        public bool SeekNearbyTargets {
            get { return mWorkProps.AnalysisParams.SeekNearbyTargets; }
            set {
                mWorkProps.AnalysisParams.SeekNearbyTargets = value;
                OnPropertyChanged();
                IsDirty = true;
            }
        }

        private void Loaded_General() {
            for (int i = 0; i < CpuItems.Length; i++) {
                if (CpuItems[i].Type == mWorkProps.CpuType) {
                    cpuComboBox.SelectedItem = CpuItems[i];
                    break;
                }
            }
            if (cpuComboBox.SelectedItem == null) {
                cpuComboBox.SelectedIndex = 0;                  // 6502
            }

            for (int i = 0; i < DefaultTextScanModeItems.Length; i++) {
                if (DefaultTextScanModeItems[i].Mode ==
                        mWorkProps.AnalysisParams.DefaultTextScanMode) {
                    defaultTextEncComboBox.SelectedItem = DefaultTextScanModeItems[i];
                    break;
                }
            }
            if (defaultTextEncComboBox.SelectedItem == null) {
                defaultTextEncComboBox.SelectedIndex = 1;       // low+high ASCII
            }

            for (int i = 0; i < MinCharsItems.Length; i++) {
                if (MinCharsItems[i].Value == mWorkProps.AnalysisParams.MinCharsForString) {
                    minStringCharsComboBox.SelectedItem = MinCharsItems[i];
                    break;
                }
            }
            if (minStringCharsComboBox.SelectedItem == null) {
                minStringCharsComboBox.SelectedIndex = 2;       // 4
            }

            for (int i = 0; i < AutoLabelItems.Length; i++) {
                if (AutoLabelItems[i].Style == mWorkProps.AutoLabelStyle) {
                    autoLabelStyleComboBox.SelectedItem = AutoLabelItems[i];
                    break;
                }
            }
            if (autoLabelStyleComboBox.SelectedItem == null) {
                autoLabelStyleComboBox.SelectedIndex = 0;       // simple
            }

            UpdateEntryFlags();
            IsDirty = false;
        }

        private void UpdateEntryFlags() {
            const string FLAGS = "CZIDXMVNE";   // flags, in order low to high, plus emu bit
            const string VALUES = "-?01";
            StringBuilder sb = new StringBuilder(27);
            StatusFlags flags = mWorkProps.EntryFlags;
            for (int i = 0; i < 9; i++) {
                // Want to show P reg flags (first 8) in conventional high-to-low order.
                int idx = (7 - i) + (i == 8 ? 9 : 0);
                int val = flags.GetBit((StatusFlags.FlagBits)idx);
                sb.Append(FLAGS[idx]);
                sb.Append(VALUES[val + 2]);
                sb.Append(' ');
            }

            currentFlagsText.Text = sb.ToString();
        }


        private void CpuComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            CpuItem item = (CpuItem)cpuComboBox.SelectedItem;
            mWorkProps.CpuType = item.Type;
            IsDirty = true;
        }

        private void DefaultTextEncComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            DefaultTextScanMode item =
                (DefaultTextScanMode)defaultTextEncComboBox.SelectedItem;
            mWorkProps.AnalysisParams.DefaultTextScanMode = item.Mode;
            IsDirty = true;
        }

        private void MinStringCharsComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            MinCharsItem item = (MinCharsItem)minStringCharsComboBox.SelectedItem;
            mWorkProps.AnalysisParams.MinCharsForString = item.Value;
            IsDirty = true;
        }

        private void AutoLabelStyleComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            AutoLabelItem item = (AutoLabelItem)autoLabelStyleComboBox.SelectedItem;
            mWorkProps.AutoLabelStyle = item.Style;
            IsDirty = true;
        }

        private void ChangeFlagButton_Click(object sender, RoutedEventArgs e) {
            CpuDef cpuDef = CpuDef.GetBestMatch(mWorkProps.CpuType,
                mWorkProps.IncludeUndocumentedInstr);
            EditStatusFlags dlg =
                new EditStatusFlags(this, mWorkProps.EntryFlags, cpuDef.HasEmuFlag);

            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                if (mWorkProps.EntryFlags != dlg.FlagValue) {
                    // Flags changed.
                    mWorkProps.EntryFlags = dlg.FlagValue;
                    UpdateEntryFlags();
                    IsDirty = true;
                }
            }
        }

        #endregion General

        #region Project Symbols

        // Item for the project symbol list view.
        public class FormattedSymbol {
            public string Label { get; private set; }
            public string Value { get; private set; }
            public string Type { get; private set; }
            public string Comment { get; private set; }

            public FormattedSymbol(string label, string value, string type, string comment) {
                Label = label;
                Value = value;
                Type = type;
                Comment = comment;
            }
        }
        public ObservableCollection<FormattedSymbol> ProjectSymbols { get; private set; } =
            new ObservableCollection<FormattedSymbol>();

        /// <summary>
        /// Prepares the project symbols ListView.
        /// </summary>
        private void LoadProjectSymbols() {
            ProjectSymbols.Clear();

            foreach (KeyValuePair<string, DefSymbol> kvp in mWorkProps.ProjectSyms) {
                DefSymbol defSym = kvp.Value;
                string typeStr;
                if (defSym.SymbolType == Symbol.Type.Constant) {
                    typeStr = Res.Strings.ABBREV_CONSTANT;
                } else {
                    typeStr = Res.Strings.ABBREV_ADDRESS;
                }

                FormattedSymbol fsym = new FormattedSymbol(
                    defSym.Label,
                    mFormatter.FormatValueInBase(defSym.Value, defSym.DataDescriptor.NumBase),
                    typeStr,
                    defSym.Comment);

                ProjectSymbols.Add(fsym);
            }
        }

        private void NewSymbolButton_Click(object sender, RoutedEventArgs e) {
            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter, mWorkProps.ProjectSyms);
            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                Debug.WriteLine("ADD: " + dlg.DefSym);
                mWorkProps.ProjectSyms[dlg.DefSym.Label] = dlg.DefSym;
                IsDirty = true;

                // Reload the contents.  This loses the selection, but that shouldn't be an
                // issue when adding new symbols.  To do this incrementally we'd need to add
                // the symbol at the correct sorted position.
                LoadProjectSymbols();
                UpdateControls();
            }
        }

        private void EditSymbolButton_Click(object sender, EventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(projectSymbolsListView.SelectedItems.Count == 1);
            FormattedSymbol item = (FormattedSymbol)projectSymbolsListView.SelectedItems[0];
            DefSymbol defSym = mWorkProps.ProjectSyms[item.Label];
            DoEditSymbol(defSym);
        }

        private void ProjectSymbolsListView_MouseDoubleClick(object sender,
                MouseButtonEventArgs e) {
            ListViewItem lvi = projectSymbolsListView.GetClickedItem(e);
            if (lvi == null) {
                return;
            }
            FormattedSymbol item = (FormattedSymbol)lvi.Content;
            DefSymbol defSym = mWorkProps.ProjectSyms[item.Label];
            DoEditSymbol(defSym);
        }

        private void DoEditSymbol(DefSymbol defSym) {
            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter, mWorkProps.ProjectSyms);
            dlg.DefSym = defSym;
            dlg.ShowDialog();
            if (dlg.DialogResult == true) {
                // Label might have changed, so remove old before adding new.
                mWorkProps.ProjectSyms.Remove(defSym.Label);
                mWorkProps.ProjectSyms[dlg.DefSym.Label] = dlg.DefSym;
                IsDirty = true;
                LoadProjectSymbols();
                UpdateControls();
            }
        }

        private void RemoveSymbolButton_Click(object sender, RoutedEventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(projectSymbolsListView.SelectedItems.Count == 1);

            int selectionIndex = projectSymbolsListView.SelectedIndex;
            FormattedSymbol item = (FormattedSymbol)projectSymbolsListView.SelectedItems[0];
            DefSymbol defSym = mWorkProps.ProjectSyms[item.Label];
            mWorkProps.ProjectSyms.Remove(defSym.Label);
            IsDirty = true;
            LoadProjectSymbols();
            UpdateControls();

            // Restore selection to the item that used to come after the one we just deleted,
            // so you can hit "Remove" repeatedly to delete multiple items.
            int newCount = projectSymbolsListView.Items.Count;
            if (selectionIndex >= newCount) {
                selectionIndex = newCount - 1;
            }
            if (selectionIndex >= 0) {
                projectSymbolsListView.SelectedIndex = selectionIndex;
                removeSymbolButton.Focus();
            }
        }

        private void ImportSymbolsButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = ProjectFile.FILENAME_FILTER + "|" + Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }
            string projPathName = Path.GetFullPath(fileDlg.FileName);

            DisasmProject newProject = new DisasmProject();
            if (!ProjectFile.DeserializeFromFile(projPathName, newProject,
                    out FileLoadReport report)) {
                // Unable to open project file.  Report error and bail.
                ProjectLoadIssues dlg = new ProjectLoadIssues(this, report.Format(),
                    ProjectLoadIssues.Buttons.Cancel);
                dlg.ShowDialog();
                // ignore dlg.DialogResult
                return;
            }

            // Import all user labels that were marked as "global export".  These become
            // external-address project symbols.
            int foundCount = 0;
            foreach (KeyValuePair<int, Symbol> kvp in newProject.UserLabels) {
                if (kvp.Value.SymbolType == Symbol.Type.GlobalAddrExport) {
                    Symbol sym = kvp.Value;
                    DefSymbol defSym = new DefSymbol(sym.Label, sym.Value, Symbol.Source.Project,
                        Symbol.Type.ExternalAddr, FormatDescriptor.SubType.None,
                        string.Empty, string.Empty);
                    mWorkProps.ProjectSyms[defSym.Label] = defSym;
                    foundCount++;
                }
            }
            if (foundCount != 0) {
                IsDirty = true;
                LoadProjectSymbols();
                UpdateControls();
            }

            newProject.Cleanup();

            // Tell the user we did something.  Might be nice to tell them how many weren't
            // already present.
            string msg;
            if (foundCount == 0) {
                msg = Res.Strings.SYMBOL_IMPORT_NONE;
            } else {
                msg = string.Format(Res.Strings.SYMBOL_IMPORT_GOOD_FMT, foundCount);
            }
            MessageBox.Show(msg, Res.Strings.SYMBOL_IMPORT_CAPTION,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion Project Symbols

        #region Platform symbol files

        public ObservableCollection<string> PlatformSymbolIdentifiers { get; private set; } =
            new ObservableCollection<string>();

        /// <summary>
        /// Loads the platform symbol file names into the list control.
        /// </summary>
        private void LoadPlatformSymbolFiles() {
            PlatformSymbolIdentifiers.Clear();
            foreach (string fileName in mWorkProps.PlatformSymbolFileIdentifiers) {
                PlatformSymbolIdentifiers.Add(fileName);
            }
        }

        private void AddSymbolFilesButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = PlatformSymbols.FILENAME_FILTER,
                Multiselect = true,
                InitialDirectory = RuntimeDataAccess.GetDirectory(),
                RestoreDirectory = true     // doesn't seem to work?
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }

            foreach (string pathName in fileDlg.FileNames) {
                // I'm assuming the full names got the Path.GetFullPath() canonicalization and
                // don't need further processing.  Also, I'm assuming that all files live in
                // the same directory, so if one is in an invalid location then they all are.
                ExternalFile ef = ExternalFile.CreateFromPath(pathName, mProjectDir);
                if (ef == null) {
                    // Files not found in runtime or project directory.
                    string projDir = mProjectDir;
                    if (string.IsNullOrEmpty(projDir)) {
                        projDir = Res.Strings.UNSET;
                    }
                    string msg = string.Format(Res.Strings.EXTERNAL_FILE_BAD_DIR_FMT,
                            RuntimeDataAccess.GetDirectory(), projDir, pathName);
                    MessageBox.Show(msg, Res.Strings.EXTERNAL_FILE_BAD_DIR_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string ident = ef.Identifier;

                if (mWorkProps.PlatformSymbolFileIdentifiers.Contains(ident)) {
                    Debug.WriteLine("Already present: " + ident);
                    continue;
                }

                Debug.WriteLine("Adding symbol file: " + ident);
                mWorkProps.PlatformSymbolFileIdentifiers.Add(ident);
                IsDirty = true;
            }

            if (IsDirty) {
                LoadPlatformSymbolFiles();
                UpdateControls();
            }
        }

        private void SymbolFileUpButton_Click(object sender, EventArgs e) {
            Debug.Assert(symbolFilesListBox.SelectedItems.Count == 1);
            int selIndex = symbolFilesListBox.SelectedIndex;
            Debug.Assert(selIndex > 0);

            MoveSingleItem(selIndex, symbolFilesListBox.SelectedItem, -1);
        }

        private void SymbolFileDownButton_Click(object sender, EventArgs e) {
            Debug.Assert(symbolFilesListBox.SelectedItems.Count == 1);
            int selIndex = symbolFilesListBox.SelectedIndex;
            Debug.Assert(selIndex < symbolFilesListBox.Items.Count - 1);

            MoveSingleItem(selIndex, symbolFilesListBox.SelectedItem, +1);
        }

        private void MoveSingleItem(int selIndex, object selectedItem, int adj) {
            string selected = (string)symbolFilesListBox.SelectedItem;
            PlatformSymbolIdentifiers.Remove(selected);
            PlatformSymbolIdentifiers.Insert(selIndex + adj, selected);
            symbolFilesListBox.SelectedIndex = selIndex + adj;

            // do the same operation in the file name list
            string str = mWorkProps.PlatformSymbolFileIdentifiers[selIndex];
            mWorkProps.PlatformSymbolFileIdentifiers.RemoveAt(selIndex);
            mWorkProps.PlatformSymbolFileIdentifiers.Insert(selIndex + adj, str);

            IsDirty = true;
            UpdateControls();
        }

        private void SymbolFileRemoveButton_Click(object sender, EventArgs e) {
            Debug.Assert(symbolFilesListBox.SelectedItems.Count > 0);
            for (int i = symbolFilesListBox.SelectedItems.Count - 1; i >= 0; i--) {
                string selItem = (string)symbolFilesListBox.SelectedItems[i];
                PlatformSymbolIdentifiers.Remove(selItem);
                mWorkProps.PlatformSymbolFileIdentifiers.Remove(selItem);
            }

            IsDirty = true;
            UpdateControls();
        }

        #endregion Platform symbol files

        #region Extension scripts

        public ObservableCollection<string> ExtensionScriptIdentifiers { get; private set; } =
            new ObservableCollection<string>();

        /// <summary>
        /// Loads the extension script file names into the list control.
        /// </summary>
        private void LoadExtensionScriptNames() {
            ExtensionScriptIdentifiers.Clear();

            foreach (string fileName in mWorkProps.ExtensionScriptFileIdentifiers) {
                ExtensionScriptIdentifiers.Add(fileName);
            }
        }

        private void AddExtensionScriptsButton_Click(object sender, EventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = Sandbox.ScriptManager.FILENAME_FILTER,
                Multiselect = true,
                InitialDirectory = RuntimeDataAccess.GetDirectory(),
                RestoreDirectory = true     // doesn't seem to work?
            };
            if (fileDlg.ShowDialog() != true) {
                return;
            }

            foreach (string pathName in fileDlg.FileNames) {
                // I'm assuming the full names got the Path.GetFullPath() canonicalization and
                // don't need further processing.  Also, I'm assuming that all files live in
                // the same directory, so if one is in an invalid location then they all are.
                ExternalFile ef = ExternalFile.CreateFromPath(pathName, mProjectDir);
                if (ef == null) {
                    // Files not found in runtime or project directory.
                    string projDir = mProjectDir;
                    if (string.IsNullOrEmpty(projDir)) {
                        projDir = Res.Strings.UNSET;
                    }
                    string msg = string.Format(Res.Strings.EXTERNAL_FILE_BAD_DIR_FMT,
                            RuntimeDataAccess.GetDirectory(), projDir, pathName);
                    MessageBox.Show(this, msg, Res.Strings.EXTERNAL_FILE_BAD_DIR_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string ident = ef.Identifier;

                if (mWorkProps.ExtensionScriptFileIdentifiers.Contains(ident)) {
                    Debug.WriteLine("Already present: " + ident);
                    continue;
                }

                Debug.WriteLine("Adding extension script: " + ident);
                mWorkProps.ExtensionScriptFileIdentifiers.Add(ident);
                IsDirty = true;
            }

            if (IsDirty) {
                LoadExtensionScriptNames();
                UpdateControls();
            }
        }

        private void ExtensionScriptRemoveButton_Click(object sender, EventArgs e) {
            Debug.Assert(extensionScriptsListBox.SelectedItems.Count > 0);
            for (int i = extensionScriptsListBox.SelectedItems.Count - 1; i >= 0; i--) {
                string selItem = (string)extensionScriptsListBox.SelectedItems[i];
                ExtensionScriptIdentifiers.Remove(selItem);
                mWorkProps.ExtensionScriptFileIdentifiers.Remove(selItem);
            }

            IsDirty = true;
            UpdateControls();
        }

        #endregion Extension scripts
    }
}
