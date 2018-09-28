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
using System.IO;
using System.Text;
using System.Windows.Forms;

using Asm65;
using CommonUtil;

namespace SourceGen.AppForms {
    /// <summary>
    /// Edit project properties.
    /// 
    /// Changes are made locally, and pushed to NewProps when OK or Apply is clicked.  When
    /// the dialog exits, if NewProps is non-null, the caller should apply those changes
    /// regardless of the dialog's return value.
    /// </summary>
    public partial class EditProjectProperties : Form {
        /// <summary>
        /// Working set.  Used internally to hold state.
        /// </summary>
        private ProjectProperties WorkProps { get; set; }

        /// <summary>
        /// New set.  Updated when Apply or OK is hit.  This will be null if no changes have
        /// been applied.
        /// </summary>
        public ProjectProperties NewProps { get; private set; }

        /// <summary>
        /// Format object to use when formatting addresses and constants.
        /// </summary>
        public Formatter NumFormatter { get; set; }

        /// <summary>
        /// Dirty flag.  Ideally this would just be "WorkProps != OldProps", but it doesn't
        /// seem worthwhile to maintain an equality operator.
        /// </summary>
        private bool mDirty;

        /// <summary>
        /// Project directory, if one has been established; otherwise empty.
        /// </summary>
        private string mProjectDir;


        public EditProjectProperties(string projectDir) {
            InitializeComponent();

            mProjectDir = projectDir;
        }

        /// <summary>
        /// Sets the initial state from an existing ProjectProperties object.  This must be
        /// called, and must be called before the dialog is shown.
        /// </summary>
        /// <param name="props">Object to clone.</param>
        public void SetInitialProps(ProjectProperties props) {
            WorkProps = new ProjectProperties(props);
        }

        private void EditProperties_Load(object sender, EventArgs e) {
            // Configure CPU chooser.  This must match the order of strings in the designer.
            switch (WorkProps.CpuType) {
                case CpuDef.CpuType.Cpu6502:
                    cpuComboBox.SelectedIndex = 0;
                    break;
                case CpuDef.CpuType.Cpu65C02:
                    cpuComboBox.SelectedIndex = 1;
                    break;
                case CpuDef.CpuType.Cpu65816:
                    cpuComboBox.SelectedIndex = 2;
                    break;
                default:
                    Debug.Assert(false);
                    cpuComboBox.SelectedIndex = 0;
                    break;
            }

            undocInstrCheckBox.Checked = WorkProps.IncludeUndocumentedInstr;
            analyzeUncategorizedCheckBox.Checked =
                WorkProps.AnalysisParams.AnalyzeUncategorizedData;
            seekAltTargetCheckBox.Checked =
                WorkProps.AnalysisParams.SeekNearbyTargets;

            int matchLen = WorkProps.AnalysisParams.MinCharsForString;
            int selIndex;
            if (matchLen == DataAnalysis.MIN_CHARS_FOR_STRING_DISABLED) {
                selIndex = 0;       // disabled
            } else {
                selIndex = matchLen - 2;
            }
            if (selIndex < 0 || selIndex >= minStringCharsComboBox.Items.Count) {
                Debug.Assert(false, "bad MinCharsForString " + matchLen);
                selIndex = 0;
            }
            minStringCharsComboBox.SelectedIndex = selIndex;

            LoadProjectSymbols();
            LoadPlatformSymbolFiles();
            LoadExtensionScriptNames();

            // Various callbacks will have fired while configuring controls.  Reset to "clean".
            mDirty = false;
            UpdateControls();
        }

        private void okButton_Click(object sender, EventArgs e) {
            NewProps = new ProjectProperties(WorkProps);
        }

        private void applyButton_Click(object sender, EventArgs e) {
            NewProps = new ProjectProperties(WorkProps);
            mDirty = false;
            UpdateControls();
        }

        private void UpdateControls() {
            //
            // General tab
            //
            applyButton.Enabled = mDirty;

            const string FLAGS = "CZIDXMVNE";   // flags, in order low to high, plus emu bit
            const string VALUES = "-?01";
            StringBuilder sb = new StringBuilder(27);
            StatusFlags flags = WorkProps.EntryFlags;
            for (int i = 0; i < 9; i++) {
                // Want to show P reg flags (first 8) in conventional high-to-low order.
                int idx = (7 - i) + (i == 8 ? 9 : 0);
                int val = flags.GetBit((StatusFlags.FlagBits)idx);
                sb.Append(FLAGS[idx]);
                sb.Append(VALUES[val + 2]);
                sb.Append(' ');
            }

            currentFlagsLabel.Text = sb.ToString();

            //
            // Project symbols tab
            //
            int symSelCount = projectSymbolsListView.SelectedIndices.Count;
            removeSymbolButton.Enabled = (symSelCount == 1);
            editSymbolButton.Enabled = (symSelCount == 1);

            //
            // Platform symbol files tab
            //
            int fileSelCount = symbolFilesListBox.SelectedIndices.Count;
            symbolFileRemoveButton.Enabled = (fileSelCount != 0);
            symbolFileUpButton.Enabled = (fileSelCount == 1 &&
                symbolFilesListBox.SelectedIndices[0] != 0);
            symbolFileDownButton.Enabled = (fileSelCount == 1 &&
                symbolFilesListBox.SelectedIndices[0] != symbolFilesListBox.Items.Count - 1);

            //
            // Extension Scripts tab
            //
            fileSelCount = extensionScriptsListBox.SelectedIndices.Count;
            extensionScriptRemoveButton.Enabled = (fileSelCount != 0);
        }


        #region General

        /// <summary>
        /// Converts the CPU combo box selection to a CpuType enum value.
        /// </summary>
        /// <param name="sel">Selection index.</param>
        /// <returns>CPU type.</returns>
        private CpuDef.CpuType CpuSelectionToCpuType(int sel) {
            switch (sel) {
                case 0:     return CpuDef.CpuType.Cpu6502;
                case 1:     return CpuDef.CpuType.Cpu65C02;
                case 2:     return CpuDef.CpuType.Cpu65816;
                default:
                    Debug.Assert(false);
                    return CpuDef.CpuType.Cpu6502;
            }
        }

        private void cpuComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            CpuDef.CpuType cpuType = CpuSelectionToCpuType(cpuComboBox.SelectedIndex);
            if (WorkProps.CpuType != cpuType) {
                WorkProps.CpuType = cpuType;
                mDirty = true;
                UpdateControls();
            }
        }

        private void undocInstrCheckBox_CheckedChanged(object sender, EventArgs e) {
            if (WorkProps.IncludeUndocumentedInstr != undocInstrCheckBox.Checked) {
                WorkProps.IncludeUndocumentedInstr = undocInstrCheckBox.Checked;
                mDirty = true;
                UpdateControls();
            }
        }

        private void changeFlagButton_Click(object sender, EventArgs e) {
            EditStatusFlags dlg = new EditStatusFlags();
            dlg.FlagValue = WorkProps.EntryFlags;

            CpuDef cpuDef = CpuDef.GetBestMatch(WorkProps.CpuType,
                WorkProps.IncludeUndocumentedInstr);
            dlg.HasEmuFlag = cpuDef.HasEmuFlag;

            dlg.ShowDialog();
            if (dlg.DialogResult == DialogResult.OK) {
                if (WorkProps.EntryFlags != dlg.FlagValue) {
                    // Flags changed.
                    WorkProps.EntryFlags = dlg.FlagValue;
                    mDirty = true;
                    UpdateControls();
                }
            }

            dlg.Dispose();
        }

        private void analyzeUncategorizedCheckBox_CheckedChanged(object sender, EventArgs e) {
            WorkProps.AnalysisParams.AnalyzeUncategorizedData =
                analyzeUncategorizedCheckBox.Checked;
            mDirty = true;
            UpdateControls();
        }

        private void seekAltTargetCheckBox_CheckedChanged(object sender, EventArgs e) {
            WorkProps.AnalysisParams.SeekNearbyTargets =
                seekAltTargetCheckBox.Checked;
            mDirty = true;
            UpdateControls();
        }

        private void minStringCharsComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            int index = minStringCharsComboBox.SelectedIndex;
            int newVal;
            if (index == 0) {
                newVal = DataAnalysis.MIN_CHARS_FOR_STRING_DISABLED;
            } else {
                newVal = index + 2;
            }

            if (newVal != WorkProps.AnalysisParams.MinCharsForString) {
                WorkProps.AnalysisParams.MinCharsForString = newVal;
                mDirty = true;
                UpdateControls();
            }
        }

        #endregion General


        #region Project Symbols

        private ListViewItem.ListViewSubItem[] mSymbolSubArray =
            new ListViewItem.ListViewSubItem[3];

        /// <summary>
        /// Loads the project symbols into the ListView.
        /// </summary>
        private void LoadProjectSymbols() {
            // The set should be small enough that we don't need to worry about updating
            // the item list incrementally.
            //Debug.WriteLine("LPS loading " + WorkProps.ProjectSyms.Count + " project symbols");
            projectSymbolsListView.BeginUpdate();
            projectSymbolsListView.Items.Clear();
            foreach (KeyValuePair<string, DefSymbol> kvp in WorkProps.ProjectSyms) {
                DefSymbol defSym = kvp.Value;
                string typeStr;
                if (defSym.SymbolType == Symbol.Type.Constant) {
                    typeStr = Properties.Resources.ABBREV_CONSTANT;
                } else {
                    typeStr = Properties.Resources.ABBREV_ADDRESS;
                }

                ListViewItem lvi = new ListViewItem();
                lvi.Text = defSym.Label;
                mSymbolSubArray[0] = new ListViewItem.ListViewSubItem(lvi,
                    NumFormatter.FormatValueInBase(defSym.Value, defSym.DataDescriptor.NumBase));
                mSymbolSubArray[1] = new ListViewItem.ListViewSubItem(lvi, typeStr);
                mSymbolSubArray[2] = new ListViewItem.ListViewSubItem(lvi, defSym.Comment);
                lvi.SubItems.AddRange(mSymbolSubArray);

                projectSymbolsListView.Items.Add(lvi);
            }
            projectSymbolsListView.EndUpdate();
        }


        private void projectSymbolsListView_SelectedIndexChanged(object sender, EventArgs e) {
            // Need to enable/disable the edit+remove buttons depending on the number of
            // selected items.
            UpdateControls();
        }

        private void newSymbolButton_Click(object sender, EventArgs e) {
            EditDefSymbol dlg = new EditDefSymbol(NumFormatter, WorkProps.ProjectSyms);
            dlg.ShowDialog();
            if (dlg.DialogResult == DialogResult.OK) {
                Debug.WriteLine("ADD: " + dlg.DefSym);
                WorkProps.ProjectSyms[dlg.DefSym.Label] = dlg.DefSym;
                mDirty = true;
                LoadProjectSymbols();
                UpdateControls();
            }
            dlg.Dispose();
        }

        private void editSymbolButton_Click(object sender, EventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(projectSymbolsListView.SelectedItems.Count == 1);
            ListViewItem item = projectSymbolsListView.SelectedItems[0];
            DefSymbol defSym = WorkProps.ProjectSyms[item.Text];
            DoEditSymbol(defSym);
        }

        private void projectSymbolsListView_MouseDoubleClick(object sender, MouseEventArgs e) {
            ListViewHitTestInfo info = projectSymbolsListView.HitTest(e.X, e.Y);
            DefSymbol defSym = WorkProps.ProjectSyms[info.Item.Text];
            DoEditSymbol(defSym);
        }

        private void DoEditSymbol(DefSymbol defSym) {
            EditDefSymbol dlg = new EditDefSymbol(NumFormatter, WorkProps.ProjectSyms);
            dlg.DefSym = defSym;
            dlg.ShowDialog();
            if (dlg.DialogResult == DialogResult.OK) {
                // Label might have changed, so remove old before adding new.
                WorkProps.ProjectSyms.Remove(defSym.Label);
                WorkProps.ProjectSyms[dlg.DefSym.Label] = dlg.DefSym;
                mDirty = true;
                LoadProjectSymbols();
                UpdateControls();
            }
            dlg.Dispose();
        }

        private void removeSymbolButton_Click(object sender, EventArgs e) {
            // Single-select list view, button dimmed when no selection.
            Debug.Assert(projectSymbolsListView.SelectedItems.Count == 1);

            int selectionIndex = projectSymbolsListView.SelectedIndices[0];
            ListViewItem item = projectSymbolsListView.SelectedItems[0];
            DefSymbol defSym = WorkProps.ProjectSyms[item.Text];
            WorkProps.ProjectSyms.Remove(defSym.Label);
            mDirty = true;
            LoadProjectSymbols();
            UpdateControls();

            // Restore selection, so you can hit "Remove" repeatedly to delete
            // multiple items.
            int newCount = projectSymbolsListView.Items.Count;
            if (selectionIndex >= newCount) {
                selectionIndex = newCount - 1;
            }
            if (selectionIndex >= 0) {
                projectSymbolsListView.SelectedIndices.Add(selectionIndex);
                removeSymbolButton.Focus();
            }
        }

        #endregion Project Symbols


        #region Platform symbol files

        /// <summary>
        /// Loads the platform symbol file names into the list control.
        /// </summary>
        private void LoadPlatformSymbolFiles() {
            symbolFilesListBox.BeginUpdate();
            symbolFilesListBox.Items.Clear();

            foreach (string fileName in WorkProps.PlatformSymbolFileIdentifiers) {
                symbolFilesListBox.Items.Add(fileName);
            }

            symbolFilesListBox.EndUpdate();
        }

        private void symbolFilesListBox_SelectedIndexChanged(object sender, EventArgs e) {
            // Enable/disable buttons as the selection changes.
            UpdateControls();
        }

        private void addSymbolFilesButton_Click(object sender, EventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = PlatformSymbols.FILENAME_FILTER;
            fileDlg.Multiselect = true;
            fileDlg.InitialDirectory = RuntimeDataAccess.GetDirectory();
            fileDlg.RestoreDirectory = true;    // doesn't seem to work?
            if (fileDlg.ShowDialog() != DialogResult.OK) {
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
                        projDir = Properties.Resources.UNSET;
                    }
                    string msg = string.Format(Properties.Resources.EXTERNAL_FILE_BAD_DIR,
                            RuntimeDataAccess.GetDirectory(), projDir, pathName);
                    MessageBox.Show(msg, Properties.Resources.EXTERNAL_FILE_BAD_DIR_CAPTION,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string ident = ef.Identifier;

                if (WorkProps.PlatformSymbolFileIdentifiers.Contains(ident)) {
                    Debug.WriteLine("Already present: " + ident);
                    continue;
                }

                Debug.WriteLine("Adding symbol file: " + ident);
                WorkProps.PlatformSymbolFileIdentifiers.Add(ident);
                mDirty = true;
            }

            if (mDirty) {
                LoadPlatformSymbolFiles();
                UpdateControls();
            }
        }

        private void symbolFileUpButton_Click(object sender, EventArgs e) {
            Debug.Assert(symbolFilesListBox.SelectedIndices.Count == 1);
            int selIndex = symbolFilesListBox.SelectedIndices[0];
            Debug.Assert(selIndex > 0);

            MoveSingleItem(selIndex, symbolFilesListBox.SelectedItem, -1);
        }

        private void symbolFileDownButton_Click(object sender, EventArgs e) {
            Debug.Assert(symbolFilesListBox.SelectedIndices.Count == 1);
            int selIndex = symbolFilesListBox.SelectedIndices[0];
            Debug.Assert(selIndex < symbolFilesListBox.Items.Count - 1);

            MoveSingleItem(selIndex, symbolFilesListBox.SelectedItem, +1);
        }

        private void MoveSingleItem(int selIndex, object selectedItem, int adj) {
            object selected = symbolFilesListBox.SelectedItem;
            symbolFilesListBox.Items.Remove(selected);
            symbolFilesListBox.Items.Insert(selIndex + adj, selected);
            symbolFilesListBox.SetSelected(selIndex + adj, true);

            // do the same operation in the file name list
            string str = WorkProps.PlatformSymbolFileIdentifiers[selIndex];
            WorkProps.PlatformSymbolFileIdentifiers.RemoveAt(selIndex);
            WorkProps.PlatformSymbolFileIdentifiers.Insert(selIndex + adj, str);

            mDirty = true;
            UpdateControls();
        }

        private void symbolFileRemoveButton_Click(object sender, EventArgs e) {
            Debug.Assert(symbolFilesListBox.SelectedIndices.Count > 0);
            for (int i = symbolFilesListBox.SelectedIndices.Count - 1; i >= 0; i--) {
                int index = symbolFilesListBox.SelectedIndices[i];
                symbolFilesListBox.Items.RemoveAt(index);
                WorkProps.PlatformSymbolFileIdentifiers.RemoveAt(index);
            }

            mDirty = true;
            UpdateControls();
        }

        private void importSymbolsButton_Click(object sender, EventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog();

            fileDlg.Filter = ProjectFile.FILENAME_FILTER + "|" +
                Properties.Resources.FILE_FILTER_ALL;
            fileDlg.FilterIndex = 1;
            if (fileDlg.ShowDialog() != DialogResult.OK) {
                return;
            }
            string projPathName = Path.GetFullPath(fileDlg.FileName);

            DisasmProject newProject = new DisasmProject();
            if (!ProjectFile.DeserializeFromFile(projPathName, newProject,
                    out FileLoadReport report)) {
                ProjectLoadIssues dlg = new ProjectLoadIssues();
                dlg.Messages = report.Format();
                dlg.CanContinue = false;
                dlg.ShowDialog();
                // ignore dlg.DialogResult
                dlg.Dispose();
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
                    WorkProps.ProjectSyms[defSym.Label] = defSym;
                    foundCount++;
                }
            }
            if (foundCount != 0) {
                mDirty = true;
                LoadProjectSymbols();
                UpdateControls();
            }

            newProject.Cleanup();

            // Tell the user we did something.  Might be nice to tell them how many weren't
            // already present.
            string msg;
            if (foundCount == 0) {
                msg = Properties.Resources.SYMBOL_IMPORT_NONE;
            } else {
                msg = string.Format(Properties.Resources.SYMBOL_IMPORT_GOOD, foundCount);
            }
            MessageBox.Show(msg, Properties.Resources.SYMBOL_IMPORT_CAPTION,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion Platform symbol files


        #region Extension scripts

        /// <summary>
        /// Loads the extension script file names into the list control.
        /// </summary>
        private void LoadExtensionScriptNames() {
            extensionScriptsListBox.BeginUpdate();
            extensionScriptsListBox.Items.Clear();

            foreach (string fileName in WorkProps.ExtensionScriptFileIdentifiers) {
                extensionScriptsListBox.Items.Add(fileName);
            }

            extensionScriptsListBox.EndUpdate();
        }

        private void extensionScriptsListBox_SelectedIndexChanged(object sender, EventArgs e) {
            // Enable/disable buttons as the selection changes.
            UpdateControls();
        }

        private void addExtensionScriptsButton_Click(object sender, EventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = Sandbox.ScriptManager.FILENAME_FILTER;
            fileDlg.Multiselect = true;
            fileDlg.InitialDirectory = RuntimeDataAccess.GetDirectory();
            fileDlg.RestoreDirectory = true;    // doesn't seem to work?
            if (fileDlg.ShowDialog() != DialogResult.OK) {
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
                        projDir = Properties.Resources.UNSET;
                    }
                    string msg = string.Format(Properties.Resources.EXTERNAL_FILE_BAD_DIR,
                            RuntimeDataAccess.GetDirectory(), projDir, pathName);
                    MessageBox.Show(msg, Properties.Resources.EXTERNAL_FILE_BAD_DIR_CAPTION,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string ident = ef.Identifier;

                if (WorkProps.ExtensionScriptFileIdentifiers.Contains(ident)) {
                    Debug.WriteLine("Already present: " + ident);
                    continue;
                }

                Debug.WriteLine("Adding extension script: " + ident);
                WorkProps.ExtensionScriptFileIdentifiers.Add(ident);
                mDirty = true;
            }

            if (mDirty) {
                LoadExtensionScriptNames();
                UpdateControls();
            }
        }

        private void extensionScriptRemoveButton_Click(object sender, EventArgs e) {
            Debug.Assert(extensionScriptsListBox.SelectedIndices.Count > 0);
            for (int i = extensionScriptsListBox.SelectedIndices.Count - 1; i >= 0; i--) {
                int index = extensionScriptsListBox.SelectedIndices[i];
                extensionScriptsListBox.Items.RemoveAt(index);
                WorkProps.ExtensionScriptFileIdentifiers.RemoveAt(index);
            }

            mDirty = true;
            UpdateControls();
        }

        #endregion Extension scripts
    }
}
