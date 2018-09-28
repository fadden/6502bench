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
    partial class EditNote {
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
            this.headerLabel = new System.Windows.Forms.Label();
            this.noteTextBox = new System.Windows.Forms.TextBox();
            this.colorGroupBox = new System.Windows.Forms.GroupBox();
            this.colorOrangeRadio = new System.Windows.Forms.RadioButton();
            this.colorPinkRadio = new System.Windows.Forms.RadioButton();
            this.colorYellowRadio = new System.Windows.Forms.RadioButton();
            this.colorBlueRadio = new System.Windows.Forms.RadioButton();
            this.colorGreenRadio = new System.Windows.Forms.RadioButton();
            this.colorDefaultRadio = new System.Windows.Forms.RadioButton();
            this.colorGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(552, 225);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(471, 225);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // headerLabel
            // 
            this.headerLabel.AutoSize = true;
            this.headerLabel.Location = new System.Drawing.Point(13, 13);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Size = new System.Drawing.Size(59, 13);
            this.headerLabel.TabIndex = 0;
            this.headerLabel.Text = "Enter note:";
            // 
            // noteTextBox
            // 
            this.noteTextBox.AcceptsReturn = true;
            this.noteTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.noteTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.noteTextBox.Location = new System.Drawing.Point(13, 33);
            this.noteTextBox.Multiline = true;
            this.noteTextBox.Name = "noteTextBox";
            this.noteTextBox.Size = new System.Drawing.Size(614, 113);
            this.noteTextBox.TabIndex = 1;
            // 
            // colorGroupBox
            // 
            this.colorGroupBox.Controls.Add(this.colorOrangeRadio);
            this.colorGroupBox.Controls.Add(this.colorPinkRadio);
            this.colorGroupBox.Controls.Add(this.colorYellowRadio);
            this.colorGroupBox.Controls.Add(this.colorBlueRadio);
            this.colorGroupBox.Controls.Add(this.colorGreenRadio);
            this.colorGroupBox.Controls.Add(this.colorDefaultRadio);
            this.colorGroupBox.Location = new System.Drawing.Point(13, 153);
            this.colorGroupBox.Name = "colorGroupBox";
            this.colorGroupBox.Size = new System.Drawing.Size(182, 95);
            this.colorGroupBox.TabIndex = 4;
            this.colorGroupBox.TabStop = false;
            this.colorGroupBox.Text = "Select Highlight Color";
            // 
            // colorOrangeRadio
            // 
            this.colorOrangeRadio.AutoSize = true;
            this.colorOrangeRadio.Location = new System.Drawing.Point(98, 68);
            this.colorOrangeRadio.Name = "colorOrangeRadio";
            this.colorOrangeRadio.Size = new System.Drawing.Size(60, 17);
            this.colorOrangeRadio.TabIndex = 5;
            this.colorOrangeRadio.TabStop = true;
            this.colorOrangeRadio.Text = "&Orange";
            this.colorOrangeRadio.UseVisualStyleBackColor = true;
            // 
            // colorPinkRadio
            // 
            this.colorPinkRadio.AutoSize = true;
            this.colorPinkRadio.Location = new System.Drawing.Point(98, 44);
            this.colorPinkRadio.Name = "colorPinkRadio";
            this.colorPinkRadio.Size = new System.Drawing.Size(46, 17);
            this.colorPinkRadio.TabIndex = 4;
            this.colorPinkRadio.TabStop = true;
            this.colorPinkRadio.Text = "&Pink";
            this.colorPinkRadio.UseVisualStyleBackColor = true;
            // 
            // colorYellowRadio
            // 
            this.colorYellowRadio.AutoSize = true;
            this.colorYellowRadio.Location = new System.Drawing.Point(98, 20);
            this.colorYellowRadio.Name = "colorYellowRadio";
            this.colorYellowRadio.Size = new System.Drawing.Size(56, 17);
            this.colorYellowRadio.TabIndex = 3;
            this.colorYellowRadio.TabStop = true;
            this.colorYellowRadio.Text = "&Yellow";
            this.colorYellowRadio.UseVisualStyleBackColor = true;
            // 
            // colorBlueRadio
            // 
            this.colorBlueRadio.AutoSize = true;
            this.colorBlueRadio.Location = new System.Drawing.Point(7, 68);
            this.colorBlueRadio.Name = "colorBlueRadio";
            this.colorBlueRadio.Size = new System.Drawing.Size(46, 17);
            this.colorBlueRadio.TabIndex = 2;
            this.colorBlueRadio.TabStop = true;
            this.colorBlueRadio.Text = "&Blue";
            this.colorBlueRadio.UseVisualStyleBackColor = true;
            // 
            // colorGreenRadio
            // 
            this.colorGreenRadio.AutoSize = true;
            this.colorGreenRadio.Location = new System.Drawing.Point(7, 44);
            this.colorGreenRadio.Name = "colorGreenRadio";
            this.colorGreenRadio.Size = new System.Drawing.Size(54, 17);
            this.colorGreenRadio.TabIndex = 1;
            this.colorGreenRadio.TabStop = true;
            this.colorGreenRadio.Text = "&Green";
            this.colorGreenRadio.UseVisualStyleBackColor = true;
            // 
            // colorDefaultRadio
            // 
            this.colorDefaultRadio.AutoSize = true;
            this.colorDefaultRadio.Location = new System.Drawing.Point(7, 20);
            this.colorDefaultRadio.Name = "colorDefaultRadio";
            this.colorDefaultRadio.Size = new System.Drawing.Size(51, 17);
            this.colorDefaultRadio.TabIndex = 0;
            this.colorDefaultRadio.TabStop = true;
            this.colorDefaultRadio.Text = "&None";
            this.colorDefaultRadio.UseVisualStyleBackColor = true;
            // 
            // EditNote
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(639, 260);
            this.Controls.Add(this.colorGroupBox);
            this.Controls.Add(this.noteTextBox);
            this.Controls.Add(this.headerLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditNote";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Note";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.EditNote_FormClosing);
            this.Load += new System.EventHandler(this.EditNote_Load);
            this.colorGroupBox.ResumeLayout(false);
            this.colorGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label headerLabel;
        private System.Windows.Forms.TextBox noteTextBox;
        private System.Windows.Forms.GroupBox colorGroupBox;
        private System.Windows.Forms.RadioButton colorOrangeRadio;
        private System.Windows.Forms.RadioButton colorPinkRadio;
        private System.Windows.Forms.RadioButton colorYellowRadio;
        private System.Windows.Forms.RadioButton colorBlueRadio;
        private System.Windows.Forms.RadioButton colorGreenRadio;
        private System.Windows.Forms.RadioButton colorDefaultRadio;
    }
}