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
namespace SourceGenWF.AppForms {
    partial class EditLongComment {
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
            this.entryTextBox = new System.Windows.Forms.TextBox();
            this.textEntryLabel = new System.Windows.Forms.Label();
            this.sampleOutputLabel = new System.Windows.Forms.Label();
            this.displayTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.maximumWidthLabel = new System.Windows.Forms.Label();
            this.maxWidthComboBox = new System.Windows.Forms.ComboBox();
            this.boxModeCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // entryTextBox
            // 
            this.entryTextBox.AcceptsReturn = true;
            this.entryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.entryTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.entryTextBox.Location = new System.Drawing.Point(12, 29);
            this.entryTextBox.Multiline = true;
            this.entryTextBox.Name = "entryTextBox";
            this.entryTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.entryTextBox.Size = new System.Drawing.Size(509, 150);
            this.entryTextBox.TabIndex = 0;
            this.entryTextBox.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            this.entryTextBox.TextChanged += new System.EventHandler(this.entryTextBox_TextChanged);
            // 
            // textEntryLabel
            // 
            this.textEntryLabel.AutoSize = true;
            this.textEntryLabel.Location = new System.Drawing.Point(13, 12);
            this.textEntryLabel.Name = "textEntryLabel";
            this.textEntryLabel.Size = new System.Drawing.Size(101, 13);
            this.textEntryLabel.TabIndex = 1;
            this.textEntryLabel.Text = "Enter comment text:";
            // 
            // sampleOutputLabel
            // 
            this.sampleOutputLabel.AutoSize = true;
            this.sampleOutputLabel.Location = new System.Drawing.Point(12, 240);
            this.sampleOutputLabel.Name = "sampleOutputLabel";
            this.sampleOutputLabel.Size = new System.Drawing.Size(88, 13);
            this.sampleOutputLabel.TabIndex = 5;
            this.sampleOutputLabel.Text = "Expected output:";
            // 
            // displayTextBox
            // 
            this.displayTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.displayTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.displayTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.displayTextBox.Location = new System.Drawing.Point(12, 258);
            this.displayTextBox.Multiline = true;
            this.displayTextBox.Name = "displayTextBox";
            this.displayTextBox.ReadOnly = true;
            this.displayTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.displayTextBox.Size = new System.Drawing.Size(509, 163);
            this.displayTextBox.TabIndex = 6;
            this.displayTextBox.Text = "01234567890123456789012345678901234567890123456789012345678901234567890123456789";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(365, 438);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(446, 438);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // maximumWidthLabel
            // 
            this.maximumWidthLabel.AutoSize = true;
            this.maximumWidthLabel.Location = new System.Drawing.Point(12, 198);
            this.maximumWidthLabel.Name = "maximumWidthLabel";
            this.maximumWidthLabel.Size = new System.Drawing.Size(101, 13);
            this.maximumWidthLabel.TabIndex = 2;
            this.maximumWidthLabel.Text = "Maximum line width:";
            // 
            // maxWidthComboBox
            // 
            this.maxWidthComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.maxWidthComboBox.FormattingEnabled = true;
            this.maxWidthComboBox.Items.AddRange(new object[] {
            "30",
            "40",
            "64",
            "80"});
            this.maxWidthComboBox.Location = new System.Drawing.Point(119, 195);
            this.maxWidthComboBox.Name = "maxWidthComboBox";
            this.maxWidthComboBox.Size = new System.Drawing.Size(74, 21);
            this.maxWidthComboBox.TabIndex = 3;
            this.maxWidthComboBox.SelectedIndexChanged += new System.EventHandler(this.maxWidthComboBox_SelectedIndexChanged);
            // 
            // boxModeCheckBox
            // 
            this.boxModeCheckBox.AutoSize = true;
            this.boxModeCheckBox.Location = new System.Drawing.Point(264, 197);
            this.boxModeCheckBox.Name = "boxModeCheckBox";
            this.boxModeCheckBox.Size = new System.Drawing.Size(92, 17);
            this.boxModeCheckBox.TabIndex = 4;
            this.boxModeCheckBox.Text = "Render in box";
            this.boxModeCheckBox.UseVisualStyleBackColor = true;
            this.boxModeCheckBox.CheckedChanged += new System.EventHandler(this.boxModeCheckBox_CheckedChanged);
            // 
            // EditLongComment
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(534, 473);
            this.Controls.Add(this.boxModeCheckBox);
            this.Controls.Add(this.maxWidthComboBox);
            this.Controls.Add(this.maximumWidthLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.displayTextBox);
            this.Controls.Add(this.sampleOutputLabel);
            this.Controls.Add(this.textEntryLabel);
            this.Controls.Add(this.entryTextBox);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(550, 512);
            this.Name = "EditLongComment";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Long Comment";
            this.HelpButtonClicked += new System.ComponentModel.CancelEventHandler(this.EditLongComment_HelpButtonClicked);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditLongComment_FormClosing);
            this.Load += new System.EventHandler(this.EditLongComment_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox entryTextBox;
        private System.Windows.Forms.Label textEntryLabel;
        private System.Windows.Forms.Label sampleOutputLabel;
        private System.Windows.Forms.TextBox displayTextBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label maximumWidthLabel;
        private System.Windows.Forms.ComboBox maxWidthComboBox;
        private System.Windows.Forms.CheckBox boxModeCheckBox;
    }
}