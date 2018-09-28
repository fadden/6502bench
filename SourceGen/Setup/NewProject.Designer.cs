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
namespace SourceGen.Setup {
    partial class NewProject {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewProject));
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.targetLabel = new System.Windows.Forms.Label();
            this.targetSystemTree = new System.Windows.Forms.TreeView();
            this.systemDescr = new System.Windows.Forms.TextBox();
            this.detailsLabel = new System.Windows.Forms.Label();
            this.selectFileButton = new System.Windows.Forms.Button();
            this.selectFileGroup = new System.Windows.Forms.GroupBox();
            this.dataFileDetailsLabel = new System.Windows.Forms.Label();
            this.selectedFileText = new System.Windows.Forms.TextBox();
            this.selectFileGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            resources.ApplyResources(this.cancelButton, "cancelButton");
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            resources.ApplyResources(this.okButton, "okButton");
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Name = "okButton";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // targetLabel
            // 
            resources.ApplyResources(this.targetLabel, "targetLabel");
            this.targetLabel.Name = "targetLabel";
            // 
            // targetSystemTree
            // 
            resources.ApplyResources(this.targetSystemTree, "targetSystemTree");
            this.targetSystemTree.FullRowSelect = true;
            this.targetSystemTree.HideSelection = false;
            this.targetSystemTree.Name = "targetSystemTree";
            this.targetSystemTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.targetSystemTree_AfterSelect);
            // 
            // systemDescr
            // 
            resources.ApplyResources(this.systemDescr, "systemDescr");
            this.systemDescr.Name = "systemDescr";
            this.systemDescr.ReadOnly = true;
            // 
            // detailsLabel
            // 
            resources.ApplyResources(this.detailsLabel, "detailsLabel");
            this.detailsLabel.Name = "detailsLabel";
            // 
            // selectFileButton
            // 
            resources.ApplyResources(this.selectFileButton, "selectFileButton");
            this.selectFileButton.Name = "selectFileButton";
            this.selectFileButton.UseVisualStyleBackColor = true;
            this.selectFileButton.Click += new System.EventHandler(this.selectFileButton_Click);
            // 
            // selectFileGroup
            // 
            resources.ApplyResources(this.selectFileGroup, "selectFileGroup");
            this.selectFileGroup.Controls.Add(this.dataFileDetailsLabel);
            this.selectFileGroup.Controls.Add(this.selectedFileText);
            this.selectFileGroup.Controls.Add(this.selectFileButton);
            this.selectFileGroup.Name = "selectFileGroup";
            this.selectFileGroup.TabStop = false;
            // 
            // dataFileDetailsLabel
            // 
            resources.ApplyResources(this.dataFileDetailsLabel, "dataFileDetailsLabel");
            this.dataFileDetailsLabel.Name = "dataFileDetailsLabel";
            // 
            // selectedFileText
            // 
            resources.ApplyResources(this.selectedFileText, "selectedFileText");
            this.selectedFileText.Name = "selectedFileText";
            this.selectedFileText.ReadOnly = true;
            // 
            // NewProject
            // 
            this.AcceptButton = this.okButton;
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.Controls.Add(this.selectFileGroup);
            this.Controls.Add(this.detailsLabel);
            this.Controls.Add(this.systemDescr);
            this.Controls.Add(this.targetSystemTree);
            this.Controls.Add(this.targetLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewProject";
            this.ShowInTaskbar = false;
            this.Load += new System.EventHandler(this.NewProject_Load);
            this.selectFileGroup.ResumeLayout(false);
            this.selectFileGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label targetLabel;
        private System.Windows.Forms.TreeView targetSystemTree;
        private System.Windows.Forms.TextBox systemDescr;
        private System.Windows.Forms.Label detailsLabel;
        private System.Windows.Forms.Button selectFileButton;
        private System.Windows.Forms.GroupBox selectFileGroup;
        private System.Windows.Forms.TextBox selectedFileText;
        private System.Windows.Forms.Label dataFileDetailsLabel;
    }
}