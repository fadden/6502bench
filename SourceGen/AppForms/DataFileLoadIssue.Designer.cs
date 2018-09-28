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
namespace SourceGen.AppForms {
    partial class DataFileLoadIssue {
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
            this.problemWithFileLabel = new System.Windows.Forms.Label();
            this.pathNameTextBox = new System.Windows.Forms.TextBox();
            this.problemLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.doYouWantLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // problemWithFileLabel
            // 
            this.problemWithFileLabel.AutoSize = true;
            this.problemWithFileLabel.Location = new System.Drawing.Point(13, 13);
            this.problemWithFileLabel.Name = "problemWithFileLabel";
            this.problemWithFileLabel.Size = new System.Drawing.Size(221, 13);
            this.problemWithFileLabel.TabIndex = 2;
            this.problemWithFileLabel.Text = "There was an error while loading the data file:";
            // 
            // pathNameTextBox
            // 
            this.pathNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathNameTextBox.Location = new System.Drawing.Point(13, 39);
            this.pathNameTextBox.Name = "pathNameTextBox";
            this.pathNameTextBox.ReadOnly = true;
            this.pathNameTextBox.Size = new System.Drawing.Size(488, 20);
            this.pathNameTextBox.TabIndex = 3;
            // 
            // problemLabel
            // 
            this.problemLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.problemLabel.Location = new System.Drawing.Point(13, 73);
            this.problemLabel.Name = "problemLabel";
            this.problemLabel.Size = new System.Drawing.Size(488, 31);
            this.problemLabel.TabIndex = 4;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(428, 113);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(347, 113);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // doYouWantLabel
            // 
            this.doYouWantLabel.AutoSize = true;
            this.doYouWantLabel.Location = new System.Drawing.Point(13, 117);
            this.doYouWantLabel.Name = "doYouWantLabel";
            this.doYouWantLabel.Size = new System.Drawing.Size(175, 13);
            this.doYouWantLabel.TabIndex = 5;
            this.doYouWantLabel.Text = "Do you want to locate the data file?\r\n";
            // 
            // DataFileLoadIssue
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(515, 148);
            this.Controls.Add(this.doYouWantLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.problemLabel);
            this.Controls.Add(this.pathNameTextBox);
            this.Controls.Add(this.problemWithFileLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DataFileLoadIssue";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Data File Load Issue";
            this.Load += new System.EventHandler(this.DataFileLoadIssue_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label problemWithFileLabel;
        private System.Windows.Forms.TextBox pathNameTextBox;
        private System.Windows.Forms.Label problemLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label doYouWantLabel;
    }
}