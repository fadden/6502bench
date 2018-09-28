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
    partial class GotoBox {
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
            this.instructionLabel = new System.Windows.Forms.Label();
            this.targetTextBox = new System.Windows.Forms.TextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.offsetLabel = new System.Windows.Forms.Label();
            this.addressLabel = new System.Windows.Forms.Label();
            this.labelLabel = new System.Windows.Forms.Label();
            this.addressValueLabel = new System.Windows.Forms.Label();
            this.offsetValueLabel = new System.Windows.Forms.Label();
            this.labelValueLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // instructionLabel
            // 
            this.instructionLabel.AutoSize = true;
            this.instructionLabel.Location = new System.Drawing.Point(13, 13);
            this.instructionLabel.Name = "instructionLabel";
            this.instructionLabel.Size = new System.Drawing.Size(215, 52);
            this.instructionLabel.TabIndex = 0;
            this.instructionLabel.Text = "Enter target location as one of:\r\n • Hex file offset (with \'+\', e.g. +500)\r\n • He" +
    "x address (e.g. 1000, $1000, 00/1000)\r\n • Label (case-sensitive)\r\n";
            // 
            // targetTextBox
            // 
            this.targetTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.targetTextBox.Location = new System.Drawing.Point(13, 78);
            this.targetTextBox.Name = "targetTextBox";
            this.targetTextBox.Size = new System.Drawing.Size(215, 20);
            this.targetTextBox.TabIndex = 1;
            this.targetTextBox.TextChanged += new System.EventHandler(this.targetTextBox_TextChanged);
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(240, 76);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "Go";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // offsetLabel
            // 
            this.offsetLabel.AutoSize = true;
            this.offsetLabel.Location = new System.Drawing.Point(13, 111);
            this.offsetLabel.Name = "offsetLabel";
            this.offsetLabel.Size = new System.Drawing.Size(38, 13);
            this.offsetLabel.TabIndex = 3;
            this.offsetLabel.Text = "Offset:";
            // 
            // addressLabel
            // 
            this.addressLabel.AutoSize = true;
            this.addressLabel.Location = new System.Drawing.Point(12, 129);
            this.addressLabel.Name = "addressLabel";
            this.addressLabel.Size = new System.Drawing.Size(48, 13);
            this.addressLabel.TabIndex = 4;
            this.addressLabel.Text = "Address:";
            // 
            // labelLabel
            // 
            this.labelLabel.AutoSize = true;
            this.labelLabel.Location = new System.Drawing.Point(12, 147);
            this.labelLabel.Name = "labelLabel";
            this.labelLabel.Size = new System.Drawing.Size(36, 13);
            this.labelLabel.TabIndex = 5;
            this.labelLabel.Text = "Label:";
            // 
            // addressValueLabel
            // 
            this.addressValueLabel.AutoSize = true;
            this.addressValueLabel.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.addressValueLabel.Location = new System.Drawing.Point(66, 129);
            this.addressValueLabel.Name = "addressValueLabel";
            this.addressValueLabel.Size = new System.Drawing.Size(49, 13);
            this.addressValueLabel.TabIndex = 6;
            this.addressValueLabel.Text = "01/2345";
            // 
            // offsetValueLabel
            // 
            this.offsetValueLabel.AutoSize = true;
            this.offsetValueLabel.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.offsetValueLabel.Location = new System.Drawing.Point(66, 111);
            this.offsetValueLabel.Name = "offsetValueLabel";
            this.offsetValueLabel.Size = new System.Drawing.Size(37, 13);
            this.offsetValueLabel.TabIndex = 7;
            this.offsetValueLabel.Text = "+1234";
            // 
            // labelValueLabel
            // 
            this.labelValueLabel.AutoSize = true;
            this.labelValueLabel.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelValueLabel.Location = new System.Drawing.Point(66, 147);
            this.labelValueLabel.Name = "labelValueLabel";
            this.labelValueLabel.Size = new System.Drawing.Size(37, 13);
            this.labelValueLabel.TabIndex = 8;
            this.labelValueLabel.Text = "FUBAR";
            // 
            // GotoBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 171);
            this.Controls.Add(this.labelValueLabel);
            this.Controls.Add(this.offsetValueLabel);
            this.Controls.Add(this.addressValueLabel);
            this.Controls.Add(this.labelLabel);
            this.Controls.Add(this.addressLabel);
            this.Controls.Add(this.offsetLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.targetTextBox);
            this.Controls.Add(this.instructionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GotoBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Go To Line";
            this.Load += new System.EventHandler(this.GotoBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label instructionLabel;
        private System.Windows.Forms.TextBox targetTextBox;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label offsetLabel;
        private System.Windows.Forms.Label addressLabel;
        private System.Windows.Forms.Label labelLabel;
        private System.Windows.Forms.Label addressValueLabel;
        private System.Windows.Forms.Label offsetValueLabel;
        private System.Windows.Forms.Label labelValueLabel;
    }
}