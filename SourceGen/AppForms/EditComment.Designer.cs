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
    partial class EditComment {
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
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.asciiOnlyLabel = new System.Windows.Forms.Label();
            this.maxLengthLabel = new System.Windows.Forms.Label();
            this.numCharsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(426, 105);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(345, 105);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // instructionLabel
            // 
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(13, 13);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(81, 13);
            this.instructionLabel.TabIndex = 0;
            this.instructionLabel.Text = "Enter comment:";
            // 
            // textBox1
            // 
            this.commentTextBox.Location = new System.Drawing.Point(13, 30);
            this.commentTextBox.Name = "textBox1";
            this.commentTextBox.Size = new System.Drawing.Size(488, 20);
            this.commentTextBox.TabIndex = 1;
            this.commentTextBox.TextChanged += new System.EventHandler(this.commentTextBox_TextChanged);
            // 
            // asciiOnlyLabel
            // 
            this.asciiOnlyLabel.AutoSize = true;
            this.asciiOnlyLabel.Location = new System.Drawing.Point(13, 57);
            this.asciiOnlyLabel.Name = "asciiOnlyLabel";
            this.asciiOnlyLabel.Size = new System.Drawing.Size(145, 13);
            this.asciiOnlyLabel.TabIndex = 2;
            this.asciiOnlyLabel.Text = "• ASCII-only is recommended";
            // 
            // maxLengthLabel
            // 
            this.maxLengthLabel.AutoSize = true;
            this.maxLengthLabel.Location = new System.Drawing.Point(13, 74);
            this.maxLengthLabel.Name = "maxLengthLabel";
            this.maxLengthLabel.Size = new System.Drawing.Size(281, 13);
            this.maxLengthLabel.TabIndex = 3;
            this.maxLengthLabel.Text = "• Limit to 52 or fewer characters for nice 80-column output";
            // 
            // numCharsLabel
            // 
            this.numCharsLabel.Location = new System.Drawing.Point(386, 53);
            this.numCharsLabel.Name = "numCharsLabel";
            this.numCharsLabel.Size = new System.Drawing.Size(115, 23);
            this.numCharsLabel.TabIndex = 4;
            this.numCharsLabel.Text = "{0} characters";
            this.numCharsLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // EditComment
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(513, 140);
            this.Controls.Add(this.numCharsLabel);
            this.Controls.Add(this.maxLengthLabel);
            this.Controls.Add(this.asciiOnlyLabel);
            this.Controls.Add(this.commentTextBox);
            this.Controls.Add(this.instructionLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditComment";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Comment";
            this.Load += new System.EventHandler(this.EditComment_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.TextBox commentTextBox;
        private System.Windows.Forms.Label asciiOnlyLabel;
        private System.Windows.Forms.Label maxLengthLabel;
        private System.Windows.Forms.Label numCharsLabel;
    }
}