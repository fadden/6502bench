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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using CommonUtil;

namespace SourceGenWPF.AsmGen.WpfGui {
    /// <summary>
    /// Code generation and assembler execution dialog.
    /// </summary>
    public partial class GenAndAsm : Window {
        private const int PREVIEW_BUF_SIZE = 64 * 1024;     // 64KB should be enough for preview
        private static string NO_PREVIEW_FILES = "<" + Res.Strings.NO_FILES_AVAILABLE + ">";

        /// <summary>
        /// Holds data for the preview combo box.
        /// </summary>
        private class ComboPath {
            public string FileName { get; private set; }
            public string PathName { get; private set; }
            public ComboPath(string pathName) {
                PathName = pathName;
                if (string.IsNullOrEmpty(pathName)) {
                    FileName = NO_PREVIEW_FILES;
                } else {
                    FileName = Path.GetFileName(pathName);
                }
            }
            public override string ToString() {
                return FileName;
            }
        }

        /// <summary>
        /// Project with data.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Directory where generated files and assembler output will go.
        /// </summary>
        private string mWorkDirectory;

        /// <summary>
        /// Base file name.  For example, if this is "GenFile", we might generate
        /// "GenFile_Cc65.S".
        /// </summary>
        private string mBaseFileName;

        /// <summary>
        /// Currently-selected assembler ID.
        /// </summary>
        private AssemblerInfo.Id mSelectedAssemblerId;

        /// <summary>
        /// Results from last source generation.
        /// </summary>
        private List<string> mGenerationResults;

        /// <summary>
        /// Holds an item for the pick-your-assembler combox box.
        /// </summary>
        private class AsmComboItem {
            public AssemblerInfo.Id AssemblerId { get; private set; }
            public string Name { get; private set; }
            public AssemblerVersion AsmVersion { get; private set; }

            public AsmComboItem(AssemblerInfo info, AssemblerVersion version) {
                AssemblerId = info.AssemblerId;
                Name = info.Name;
                AsmVersion = version;
            }
            // This determines what the combo box shows.
            public override string ToString() {
                if (AsmVersion == null) {
                    return Name + " " + Res.Strings.ASM_LATEST_VERSION;
                } else {
                    return Name + " v" + AsmVersion.VersionStr;
                }
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="project">Project reference.</param>
        /// <param name="projectPathName">Full path to the project file.</param>
        public GenAndAsm(Window owner, DisasmProject project, string projectPathName) {
            InitializeComponent();
            Owner = owner;

            mProject = project;
            mWorkDirectory = Path.GetDirectoryName(projectPathName);
            mBaseFileName = Path.GetFileNameWithoutExtension(projectPathName);

            workDirectoryTextBox.Text = mWorkDirectory;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            // Try to select the previously-used asm format.
            string defaultAsm =
                AppSettings.Global.GetString(AppSettings.SRCGEN_DEFAULT_ASM, string.Empty);
            PopulateAssemblerComboBox(defaultAsm);

            ResetElements();
        }

        /// <summary>
        /// Populates the assembler combo box.  Attempts to match the defaultAsm arg with
        /// the entries to configure the initial value.
        /// </summary>
        private void PopulateAssemblerComboBox(string defaultAsm) {
            //assemblerComboBox.DisplayMember = "Name";   // show this property

            assemblerComboBox.Items.Clear();
            IEnumerator<AssemblerInfo> iter = AssemblerInfo.GetInfoEnumerator();
            bool foundMatch = false;
            while (iter.MoveNext()) {
                AssemblerInfo info = iter.Current;
                AssemblerVersion version = AssemblerVersionCache.GetVersion(info.AssemblerId);
                AsmComboItem item = new AsmComboItem(info, version);
                assemblerComboBox.Items.Add(item);
                if (item.AssemblerId.ToString() == defaultAsm) {
                    Debug.WriteLine("matched current " + defaultAsm);
                    assemblerComboBox.SelectedItem = item;
                    foundMatch = true;
                }
            }
            if (!foundMatch) {
                // Need to do this or box will show empty.
                assemblerComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Updates the selected assembler as the combo box selection changes.  This is
        /// expected to be called during the window load event, to initialize the field.
        /// </summary>
        private void AssemblerComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            AsmComboItem sel = (AsmComboItem)assemblerComboBox.SelectedItem;
            if (sel == null) {
                // this happens on Items.Clear()
                return;
            }
            if (mSelectedAssemblerId != sel.AssemblerId) {
                // Selection changed, discard window contents.
                mSelectedAssemblerId = sel.AssemblerId;
                AppSettings.Global.SetString(AppSettings.SRCGEN_DEFAULT_ASM,
                    mSelectedAssemblerId.ToString());
                ResetElements();
            }
        }

        /// <summary>
        /// Loads the appropriate preview file when the combo box selection changes.
        /// </summary>
        private void PreviewFileComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            ComboPath cpath = (ComboPath)previewFileComboBox.SelectedItem;
            if (cpath == null || string.IsNullOrEmpty(cpath.PathName)) {
                // nothing to do
                return;
            }

            LoadPreviewFile(cpath.PathName);
        }

        /// <summary>
        /// Resets all of the active elements to the initial state, before any source code
        /// was generated.
        /// </summary>
        private void ResetElements() {
            mGenerationResults = null;
            previewFileComboBox.Items.Clear();
            previewFileComboBox.Items.Add(new ComboPath(null));
            previewFileComboBox.SelectedIndex = 0;

            previewTextBox.Text = string.Empty;

            cmdOutputTextBox.Text = string.Empty;

            UpdateAssemblerControls();
        }

        /// <summary>
        /// Updates the controls in the lower (assembler) half of the dialog.
        /// </summary>
        private void UpdateAssemblerControls() {
            bool asmConf = IsAssemblerConfigured();
            //Debug.WriteLine("ID=" + mSelectedAssemblerId + " asmConf=" + asmConf);
            asmNotConfiguredText.Visibility = asmConf ? Visibility.Hidden : Visibility.Visible;
            if (mGenerationResults == null || !asmConf) {
                runAssemblerButton.IsEnabled = false;
            } else {
                runAssemblerButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Returns true if the selected cross-assembler executable has been configured.
        /// </summary>
        private bool IsAssemblerConfigured() {
            AssemblerConfig config =
                AssemblerConfig.GetConfig(AppSettings.Global, mSelectedAssemblerId);
            return config != null && !string.IsNullOrEmpty(config.ExecutablePath);
        }

        private void AssemblerSettingsButton_Click(object sender, RoutedEventArgs e) {
            // Pop open the app settings dialog, with the appropriate tab selected.
#if false
            mMainCtrl.ShowAppSettings(AppForms.EditAppSettings.Tab.AsmConfig,
                mSelectedAssemblerId);
#endif

            // Update the controls based on whether or not the assembler is now available.
            UpdateAssemblerControls();
            AsmComboItem item = (AsmComboItem)assemblerComboBox.SelectedItem;
            Debug.Assert(item != null);
            PopulateAssemblerComboBox(item.AssemblerId.ToString());
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e) {
#if false
            IGenerator gen = AssemblerInfo.GetGenerator(mSelectedAssemblerId);
            if (gen == null) {
                Debug.WriteLine("Unable to get generator for " + mSelectedAssemblerId);
                return;
            }
            gen.Configure(mProject, mWorkDirectory, mBaseFileName,
                AssemblerVersionCache.GetVersion(mSelectedAssemblerId), AppSettings.Global);

            GeneratorProgress dlg = new GeneratorProgress(gen);
            dlg.ShowDialog();
            Debug.WriteLine("Dialog returned: " + dlg.DialogResult);

            List<string> pathNames = dlg.Results;
            dlg.Dispose();

            if (pathNames == null) {
                // errors already reported
                return;
            }

            ResetElements();
            mGenerationResults = pathNames;
            previewFileComboBox.Items.Clear();
            foreach (string str in pathNames) {
                previewFileComboBox.Items.Add(new ComboPath(str));
            }
            previewFileComboBox.SelectedIndex = 0;      // should trigger update

            UpdateAssemblerControls();
#else
            Debug.WriteLine("GENERATE");
#endif
        }

        private void LoadPreviewFile(string pathName) {
            Debug.WriteLine("LOAD " + pathName);

            try {
                using (StreamReader sr = new StreamReader(pathName, Encoding.UTF8)) {
                    char[] bigbuf = new char[PREVIEW_BUF_SIZE];
                    int actual = sr.Read(bigbuf, 0, bigbuf.Length);
                    string str = TextUtil.CharArrayToLineNumberedString(bigbuf);
                    if (actual < PREVIEW_BUF_SIZE) {
                        previewTextBox.Text = str;
                    } else {
                        previewTextBox.Text = str + "\r\n" +
                            Res.Strings.ERR_TOO_LARGE_FOR_PREVIEW;
                    }
                }
            } catch (Exception ex) {
                previewTextBox.Text = ex.ToString();
            }
        }

        private void RunAssemblerButton_Click(object sender, RoutedEventArgs e) {
#if false
            IAssembler asm = AssemblerInfo.GetAssembler(mSelectedAssemblerId);
            if (asm == null) {
                Debug.WriteLine("Unable to get assembler for " + mSelectedAssemblerId);
                return;
            }

            asm.Configure(mGenerationResults, mWorkDirectory);
            AssemblerProgress dlg = new AssemblerProgress(asm);
            dlg.ShowDialog();
            Debug.WriteLine("Dialog returned: " + dlg.DialogResult);
            if (dlg.DialogResult != DialogResult.OK) {
                // Cancelled, or failed to even run the assembler.
                return;
            }

            AssemblerResults results = dlg.Results;
            if (results == null) {
                Debug.WriteLine("Dialog returned OK, but no assembler results found");
                Debug.Assert(false);
                return;
            }

            StringBuilder sb =
                new StringBuilder(results.Stdout.Length + results.Stderr.Length + 200);
            sb.Append(results.CommandLine);
            sb.Append("\r\n");
            sb.AppendFormat("ExitCode={0} - ", results.ExitCode);
            if (results.ExitCode == 0) {
                FileInfo fi = new FileInfo(results.OutputPathName);
                if (!fi.Exists) {
                    MessageBox.Show(this, Properties.Resources.ASM_OUTPUT_NOT_FOUND,
                        Properties.Resources.ASM_MISMATCH_CAPTION,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    sb.Append(Properties.Resources.ASM_MATCH_FAILURE);
                } else if (!CommonUtil.FileUtil.CompareBinaryFile(mProject.FileData,
                        results.OutputPathName, out int offset, out byte fileVal)) {
                    if (fi.Length != mProject.FileData.Length &&
                            offset == fi.Length || offset == mProject.FileData.Length) {
                        // The files matched up to the point where one ended.
                        string msg = string.Format(Properties.Resources.ASM_MISMATCH_LENGTH_FMT,
                            fi.Length, mProject.FileData.Length);
                        MessageBox.Show(this, msg, Properties.Resources.ASM_MISMATCH_CAPTION,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        sb.Append(msg);
                    } else {
                        string msg = string.Format(Properties.Resources.ASM_MISMATCH_DATA_FMT,
                            offset, fileVal, mProject.FileData[offset]);
                        MessageBox.Show(this, msg, Properties.Resources.ASM_MISMATCH_CAPTION,
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        sb.Append(msg);
                    }
                } else {
                    sb.Append(Properties.Resources.ASM_MATCH_SUCCESS);
                }
            }
            sb.Append("\r\n\r\n");

            if (results.Stdout != null && results.Stdout.Length > 2) {
                sb.Append("----- stdout -----\r\n");
                sb.Append(results.Stdout);
                sb.Append("\r\n");
            }
            if (results.Stderr != null && results.Stderr.Length > 2) {
                sb.Append("----- stderr -----\r\n");
                sb.Append(results.Stderr);
                sb.Append("\r\n");
            }

            cmdOutputTextBox.Text = sb.ToString();
            cmdOutputTextBox.BackColor = SystemColors.Window;
#else
            Debug.WriteLine("ASSEMBLE");
#endif
        }
    }
}
