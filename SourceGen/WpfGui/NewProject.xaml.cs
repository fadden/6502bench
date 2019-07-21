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
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace SourceGen.WpfGui {
    /// <summary>
    /// New project creation dialog.
    /// </summary>
    public partial class NewProject : Window {
        private SystemDefSet mSystemDefs;

        public SystemDef SystemDef {
            get { return (SystemDef)((TreeViewItem)targetSystemTree.SelectedItem).Tag; }
        }
        public string DataFileName {
            get { return selectedFileText.Text; }
        }


        public NewProject(Window owner, SystemDefSet systemDefs) {
            InitializeComponent();
            Owner = owner;

            dataFileDetails.Text = string.Empty;
            selectedFileText.Text = string.Empty;
            mSystemDefs = systemDefs;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            LoadSystemDefSet();
        }

        /// <summary>
        /// Initializes the "system definition" tree.
        /// </summary>
        private void LoadSystemDefSet() {
            string prevSelSystem = AppSettings.Global.GetString(AppSettings.NEWP_SELECTED_SYSTEM,
                "[nothing!selected]");

            TreeViewItem selItem = PopulateNodes(prevSelSystem);
            if (selItem != null) {
                selItem.IsSelected = true;
                selItem.BringIntoView();
            }

            targetSystemTree.Focus();

            Debug.WriteLine("selected is " + targetSystemTree.SelectedItem);
        }

        /// <summary>
        /// Populates the tree view nodes with the contents of the data file.
        /// </summary>
        /// <param name="tv">TreeView to add items to</param>
        /// <param name="prevSelSystem">Name of previously-selected system.</param>
        /// <returns>The node that matches prevSelSystem, or the first leaf node if no node
        ///   matches, or null if no leaf nodes are found.</returns>
        private TreeViewItem PopulateNodes(string prevSelSystem) {
            TreeViewItem selItem = null;

            TreeView tv = targetSystemTree;
            tv.Items.Clear();

            if (mSystemDefs.Defs == null || mSystemDefs.Defs.Length == 0) {
                Debug.WriteLine("Empty def set found");
                TreeViewItem errItem = new TreeViewItem();
                errItem.Header = Res.Strings.ERR_LOAD_CONFIG_FILE;
                tv.Items.Add(errItem);
                return null;
            }

            var groups = new Dictionary<string, TreeViewItem>();
            foreach (SystemDef sd in mSystemDefs.Defs) {
                if (!groups.TryGetValue(sd.GroupName, out TreeViewItem groupItem)) {
                    groupItem = new TreeViewItem();
                    groupItem.Header = sd.GroupName;
                    groupItem.IsExpanded = true;
                    groups[sd.GroupName] = groupItem;
                    tv.Items.Add(groupItem);
                }

                bool isValid = sd.Validate();
                string treeName = isValid ? sd.Name :
                    sd.Name + Res.Strings.ERR_INVALID_SYSDEF;

                TreeViewItem newItem = new TreeViewItem();
                newItem.Header = treeName;
                newItem.IsEnabled = isValid;
                newItem.Tag = sd;
                groupItem.Items.Add(newItem);

                if ((isValid && sd.Name == prevSelSystem) || selItem == null) {
                    selItem = newItem;
                }
            }

            return selItem;
        }

        private void TargetSystemTree_SelectedItemChanged(object sender,
            RoutedPropertyChangedEventArgs<object> e) {

            Debug.WriteLine("Now selected: " + targetSystemTree.SelectedItem);
            SystemDef sd = (SystemDef)((TreeViewItem)targetSystemTree.SelectedItem).Tag;

            if (sd == null) {
                systemDescr.Text = string.Empty;
            } else {
                systemDescr.Text = sd.GetSummaryString();
            }

            UpdateOKEnabled();
        }

        /// <summary>
        /// Updates the enabled state of the OK button based on the state of the other
        /// controls.
        /// </summary>
        private void UpdateOKEnabled() {
            TreeViewItem item = (TreeViewItem)targetSystemTree.SelectedItem;
            okButton.IsEnabled = (item.Tag != null) &&
                !string.IsNullOrEmpty(selectedFileText.Text);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            Debug.WriteLine("OK: " + targetSystemTree.SelectedItem);

            SystemDef sd = (SystemDef)((TreeViewItem)targetSystemTree.SelectedItem).Tag;
            AppSettings.Global.SetString(AppSettings.NEWP_SELECTED_SYSTEM, sd.Name);
            DialogResult = true;
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = Res.Strings.FILE_FILTER_ALL,
                FilterIndex = 1
            };
            if (fileDlg.ShowDialog() == true) {
                FileInfo fi = new FileInfo(fileDlg.FileName);

                if (fi.Length > DisasmProject.MAX_DATA_FILE_SIZE) {
                    string msg = string.Format(Res.Strings.OPEN_DATA_TOO_LARGE_FMT,
                            fi.Length / 1024, DisasmProject.MAX_DATA_FILE_SIZE / 1024);
                    MessageBox.Show(msg, Res.Strings.OPEN_DATA_FAIL_CAPTION,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                selectedFileText.Text = fileDlg.FileName;

                this.dataFileDetails.Text =
                    string.Format(Res.Strings.FILE_INFO_FMT, fi.Length / 1024.0);
            }

            UpdateOKEnabled();
        }
    }
}
