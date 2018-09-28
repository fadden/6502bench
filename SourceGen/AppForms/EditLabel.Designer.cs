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
    partial class EditLabel {
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
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.instructionLabel = new System.Windows.Forms.Label();
            this.labelTextBox = new System.Windows.Forms.TextBox();
            this.maxLengthLabel = new System.Windows.Forms.Label();
            this.firstLetterLabel = new System.Windows.Forms.Label();
            this.validCharsLabel = new System.Windows.Forms.Label();
            this.notDuplicateLabel = new System.Windows.Forms.Label();
            this.labelTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButtonExport = new System.Windows.Forms.RadioButton();
            this.radioButtonGlobal = new System.Windows.Forms.RadioButton();
            this.radioButtonLocal = new System.Windows.Forms.RadioButton();
            this.labelTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(267, 202);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(186, 202);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // instructionLabel
            // 
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(13, 13);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(60, 13);
            this.instructionLabel.TabIndex = 0;
            this.instructionLabel.Text = "Enter label:";
            // 
            // labelTextBox
            // 
            this.labelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTextBox.Location = new System.Drawing.Point(13, 30);
            this.labelTextBox.Name = "labelTextBox";
            this.labelTextBox.Size = new System.Drawing.Size(325, 20);
            this.labelTextBox.TabIndex = 1;
            this.labelTextBox.TextChanged += new System.EventHandler(this.labelTextBox_TextChanged);
            // 
            // maxLengthLabel
            // 
            this.maxLengthLabel.AutoSize = true;
            this.maxLengthLabel.Location = new System.Drawing.Point(13, 57);
            this.maxLengthLabel.Name = "maxLengthLabel";
            this.maxLengthLabel.Size = new System.Drawing.Size(251, 13);
            this.maxLengthLabel.TabIndex = 2;
            this.maxLengthLabel.Text = "• Must be 2-32 characters long (or blank to remove)";
            // 
            // firstLetterLabel
            // 
            this.firstLetterLabel.AutoSize = true;
            this.firstLetterLabel.Location = new System.Drawing.Point(12, 73);
            this.firstLetterLabel.Name = "firstLetterLabel";
            this.firstLetterLabel.Size = new System.Drawing.Size(187, 13);
            this.firstLetterLabel.TabIndex = 3;
            this.firstLetterLabel.Text = "• Must start with a letter or underscore";
            // 
            // validCharsLabel
            // 
            this.validCharsLabel.AutoSize = true;
            this.validCharsLabel.Location = new System.Drawing.Point(12, 89);
            this.validCharsLabel.Name = "validCharsLabel";
            this.validCharsLabel.Size = new System.Drawing.Size(297, 13);
            this.validCharsLabel.TabIndex = 4;
            this.validCharsLabel.Text = "• Valid characters are ASCII letters, numbers, and underscore";
            // 
            // notDuplicateLabel
            // 
            this.notDuplicateLabel.AutoSize = true;
            this.notDuplicateLabel.Location = new System.Drawing.Point(13, 105);
            this.notDuplicateLabel.Name = "notDuplicateLabel";
            this.notDuplicateLabel.Size = new System.Drawing.Size(217, 13);
            this.notDuplicateLabel.TabIndex = 5;
            this.notDuplicateLabel.Text = "• Must not be a duplicate of an existing label";
            // 
            // labelTypeGroupBox
            // 
            this.labelTypeGroupBox.Controls.Add(this.radioButtonExport);
            this.labelTypeGroupBox.Controls.Add(this.radioButtonGlobal);
            this.labelTypeGroupBox.Controls.Add(this.radioButtonLocal);
            this.labelTypeGroupBox.Location = new System.Drawing.Point(13, 132);
            this.labelTypeGroupBox.Name = "labelTypeGroupBox";
            this.labelTypeGroupBox.Size = new System.Drawing.Size(137, 93);
            this.labelTypeGroupBox.TabIndex = 6;
            this.labelTypeGroupBox.TabStop = false;
            this.labelTypeGroupBox.Text = "Label Type";
            // 
            // radioButtonExport
            // 
            this.radioButtonExport.AutoSize = true;
            this.radioButtonExport.Location = new System.Drawing.Point(7, 67);
            this.radioButtonExport.Name = "radioButtonExport";
            this.radioButtonExport.Size = new System.Drawing.Size(120, 17);
            this.radioButtonExport.TabIndex = 2;
            this.radioButtonExport.TabStop = true;
            this.radioButtonExport.Text = "Global and &exported";
            this.radioButtonExport.UseVisualStyleBackColor = true;
            // 
            // radioButtonGlobal
            // 
            this.radioButtonGlobal.AutoSize = true;
            this.radioButtonGlobal.Location = new System.Drawing.Point(7, 43);
            this.radioButtonGlobal.Name = "radioButtonGlobal";
            this.radioButtonGlobal.Size = new System.Drawing.Size(55, 17);
            this.radioButtonGlobal.TabIndex = 1;
            this.radioButtonGlobal.TabStop = true;
            this.radioButtonGlobal.Text = "&Global";
            this.radioButtonGlobal.UseVisualStyleBackColor = true;
            // 
            // radioButtonLocal
            // 
            this.radioButtonLocal.AutoSize = true;
            this.radioButtonLocal.Location = new System.Drawing.Point(7, 20);
            this.radioButtonLocal.Name = "radioButtonLocal";
            this.radioButtonLocal.Size = new System.Drawing.Size(106, 17);
            this.radioButtonLocal.TabIndex = 0;
            this.radioButtonLocal.TabStop = true;
            this.radioButtonLocal.Text = "&Local (if possible)";
            this.radioButtonLocal.UseVisualStyleBackColor = true;
            // 
            // EditLabel
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(354, 237);
            this.Controls.Add(this.labelTypeGroupBox);
            this.Controls.Add(this.notDuplicateLabel);
            this.Controls.Add(this.validCharsLabel);
            this.Controls.Add(this.firstLetterLabel);
            this.Controls.Add(this.maxLengthLabel);
            this.Controls.Add(this.labelTextBox);
            this.Controls.Add(this.instructionLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditLabel";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Label";
            this.Load += new System.EventHandler(this.EditLabel_Load);
            this.labelTypeGroupBox.ResumeLayout(false);
            this.labelTypeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.TextBox labelTextBox;
        private System.Windows.Forms.Label maxLengthLabel;
        private System.Windows.Forms.Label firstLetterLabel;
        private System.Windows.Forms.Label validCharsLabel;
        private System.Windows.Forms.Label notDuplicateLabel;
        private System.Windows.Forms.GroupBox labelTypeGroupBox;
        private System.Windows.Forms.RadioButton radioButtonLocal;
        private System.Windows.Forms.RadioButton radioButtonGlobal;
        private System.Windows.Forms.RadioButton radioButtonExport;
    }
}