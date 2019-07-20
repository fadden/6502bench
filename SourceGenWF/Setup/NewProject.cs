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
using System.Windows.Forms;

namespace SourceGenWF.Setup {
    /// <summary>
    /// "New Project" form.
    /// </summary>
    public partial class NewProject : Form {
        private SystemDefSet mSystemDefs;

        public SystemDef SystemDef {
            get { return ((MyTreeNode)this.targetSystemTree.SelectedNode).SystemDef; }
        }
        public string DataFileName {
            get { return selectedFileText.Text; }
        }

        public NewProject() {
            InitializeComponent();
        }

        private void NewProject_Load(object sender, EventArgs e) {
            this.dataFileDetailsLabel.Text = string.Empty;
            LoadSystemDefSet();
        }

        /// <summary>
        /// Initializes the "system definition" tree.
        /// </summary>
        private void LoadSystemDefSet() {
            TreeView tv = this.targetSystemTree;
            tv.BeginUpdate();

            string prevSelSystem = AppSettings.Global.GetString(AppSettings.NEWP_SELECTED_SYSTEM,
                "[nothing!selected]");

            TreeNode selNode = PopulateNodes(tv, prevSelSystem);

            tv.ExpandAll();

            TreeNode firstNode = tv.Nodes[0];
            if (firstNode != null) {
                // Control seems to scroll to bottom while being populated.  Scroll back
                // to top, then scroll down as needed.
                firstNode.EnsureVisible();

                if (selNode != null) {
                    tv.SelectedNode = selNode;
                    selNode.EnsureVisible();
                } else if (firstNode.Nodes.Count > 0) {
                    // Just grab the first node.
                    tv.SelectedNode = firstNode.Nodes[0];
                }
            }

            tv.Focus();
            tv.EndUpdate();

            Debug.WriteLine("selected is " + tv.SelectedNode);
        }

        /// <summary>
        /// Populates the tree view nodes with the contents of the data file.
        /// </summary>
        /// <param name="tv">TreeView to add items to</param>
        /// <param name="prevSelSystem">Name of previously-selected system.</param>
        /// <returns>The node that matches prevSelSystem, or null if not found.</returns>
        private TreeNode PopulateNodes(TreeView tv, string prevSelSystem) {
            TreeNode selNode = null;

            string sysDefsPath = RuntimeDataAccess.GetPathName("SystemDefs.json");
            if (sysDefsPath == null) {
                TreeNode errNode = new MyTreeNode(
                    SourceGenWF.Properties.Resources.ERR_LOAD_CONFIG_FILE, false, null);
                tv.Nodes.Add(errNode);
                return null;
            }

            try {
                mSystemDefs = SystemDefSet.ReadFile(sysDefsPath);
            } catch (Exception ex) {
                Debug.WriteLine("Failed loading system def set: " + ex);
                TreeNode errNode = new MyTreeNode(
                    SourceGenWF.Properties.Resources.ERR_LOAD_CONFIG_FILE, false, null);
                tv.Nodes.Add(errNode);
                return null;
            }

            if (mSystemDefs.Defs == null || mSystemDefs.Defs.Length == 0) {
                Debug.WriteLine("Empty def set found");
                TreeNode errNode = new MyTreeNode(
                    SourceGenWF.Properties.Resources.ERR_LOAD_CONFIG_FILE, false, null);
                tv.Nodes.Add(errNode);
                return null;
            }

            var groups = new Dictionary<string, MyTreeNode>();
            foreach (SystemDef sd in mSystemDefs.Defs) {
                if (!groups.TryGetValue(sd.GroupName, out MyTreeNode groupNode)) {
                    groupNode = new MyTreeNode(sd.GroupName, false, null);
                    groups[sd.GroupName] = groupNode;
                    tv.Nodes.Add(groupNode);
                }

                bool isValid = sd.Validate();
                string treeName = isValid ? sd.Name :
                    sd.Name + SourceGenWF.Properties.Resources.ERR_INVALID_SYSDEF;

                MyTreeNode newNode = new MyTreeNode(treeName, isValid, sd);
                groupNode.Nodes.Add(newNode);

                if (isValid && sd.Name == prevSelSystem) {
                    selNode = newNode;
                }
            }

            return selNode;
        }

        /// <summary>
        /// Updates the enabled state of the OK button based on the state of the other
        /// controls.
        /// </summary>
        private void UpdateOKEnabled() {
            MyTreeNode myNode = (MyTreeNode)this.targetSystemTree.SelectedNode;
            this.okButton.Enabled = myNode.IsSelectable &&
                !string.IsNullOrEmpty(selectedFileText.Text);
        }

        private void targetSystemTree_AfterSelect(object sender, TreeViewEventArgs ev) {
            Debug.WriteLine("AfterSel: " + this.targetSystemTree.SelectedNode);

            MyTreeNode myNode = (MyTreeNode)this.targetSystemTree.SelectedNode;

            if (!myNode.IsSelectable) {
                this.systemDescr.Text = string.Empty;
            } else {
                this.systemDescr.Text = myNode.SystemDef.GetSummaryString();
            }

            UpdateOKEnabled();
        }

        private void okButton_Click(object sender, EventArgs e) {
            Debug.WriteLine("OK: " + this.targetSystemTree.SelectedNode);

            AppSettings.Global.SetString(AppSettings.NEWP_SELECTED_SYSTEM,
                this.targetSystemTree.SelectedNode.Text);
        }

        private void selectFileButton_Click(object sender, EventArgs e) {
            OpenFileDialog fileDlg = new OpenFileDialog() {
                Filter = Properties.Resources.FILE_FILTER_ALL,
                FilterIndex = 1
            };
            if (fileDlg.ShowDialog() == DialogResult.OK) {
                FileInfo fi = new FileInfo(fileDlg.FileName);

                if (fi.Length > DisasmProject.MAX_DATA_FILE_SIZE) {
                    string msg = string.Format(Properties.Resources.OPEN_DATA_TOO_LARGE,
                            fi.Length / 1024, DisasmProject.MAX_DATA_FILE_SIZE / 1024);
                    MessageBox.Show(this, msg, Properties.Resources.OPEN_DATA_FAIL_CAPTION,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                selectedFileText.Text = fileDlg.FileName;

                this.dataFileDetailsLabel.Text =
                    string.Format(Properties.Resources.FMT_FILE_INFO, fi.Length / 1024.0);
            }

            UpdateOKEnabled();
        }
    }

    /// <summary>
    /// TreeView node, with benefits.
    /// </summary>
    class MyTreeNode : TreeNode {
        public bool IsSelectable { get; set; }
        public SystemDef SystemDef { get; set; }

        public MyTreeNode(string text, bool isSelectable, SystemDef systemDef)
                : base(text) {
            IsSelectable = isSelectable;
            SystemDef = systemDef;
        }

        public override string ToString() {
            return base.ToString() + " (isSel=" + IsSelectable + ")";
        }
    }
}
