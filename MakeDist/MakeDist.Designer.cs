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
namespace MakeDist {
    partial class MakeDist {
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
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.distributionTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.releaseDistRadio = new System.Windows.Forms.RadioButton();
            this.debugDistRadio = new System.Windows.Forms.RadioButton();
            this.includeTestsCheckBox = new System.Windows.Forms.CheckBox();
            this.goButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.distributionTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.Location = new System.Drawing.Point(13, 13);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(371, 35);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "This program gathers up all the files needed for a 6502bench distribution. A full" +
    " debug or release build should be performed before running this.";
            // 
            // distributionTypeGroupBox
            // 
            this.distributionTypeGroupBox.Controls.Add(this.releaseDistRadio);
            this.distributionTypeGroupBox.Controls.Add(this.debugDistRadio);
            this.distributionTypeGroupBox.Location = new System.Drawing.Point(16, 51);
            this.distributionTypeGroupBox.Name = "distributionTypeGroupBox";
            this.distributionTypeGroupBox.Size = new System.Drawing.Size(120, 67);
            this.distributionTypeGroupBox.TabIndex = 1;
            this.distributionTypeGroupBox.TabStop = false;
            this.distributionTypeGroupBox.Text = "Distribution Type";
            // 
            // releaseDistRadio
            // 
            this.releaseDistRadio.AutoSize = true;
            this.releaseDistRadio.Location = new System.Drawing.Point(7, 20);
            this.releaseDistRadio.Name = "releaseDistRadio";
            this.releaseDistRadio.Size = new System.Drawing.Size(64, 17);
            this.releaseDistRadio.TabIndex = 0;
            this.releaseDistRadio.TabStop = true;
            this.releaseDistRadio.Text = "Release";
            this.releaseDistRadio.UseVisualStyleBackColor = true;
            // 
            // debugDistRadio
            // 
            this.debugDistRadio.AutoSize = true;
            this.debugDistRadio.Location = new System.Drawing.Point(7, 43);
            this.debugDistRadio.Name = "debugDistRadio";
            this.debugDistRadio.Size = new System.Drawing.Size(57, 17);
            this.debugDistRadio.TabIndex = 1;
            this.debugDistRadio.TabStop = true;
            this.debugDistRadio.Text = "Debug";
            this.debugDistRadio.UseVisualStyleBackColor = true;
            // 
            // includeTestsCheckBox
            // 
            this.includeTestsCheckBox.AutoSize = true;
            this.includeTestsCheckBox.Location = new System.Drawing.Point(16, 125);
            this.includeTestsCheckBox.Name = "includeTestsCheckBox";
            this.includeTestsCheckBox.Size = new System.Drawing.Size(153, 17);
            this.includeTestsCheckBox.TabIndex = 2;
            this.includeTestsCheckBox.Text = "Include regression test files";
            this.includeTestsCheckBox.UseVisualStyleBackColor = true;
            // 
            // goButton
            // 
            this.goButton.Location = new System.Drawing.Point(192, 59);
            this.goButton.Name = "goButton";
            this.goButton.Size = new System.Drawing.Size(88, 47);
            this.goButton.TabIndex = 3;
            this.goButton.Text = "BUILD";
            this.goButton.UseVisualStyleBackColor = true;
            this.goButton.Click += new System.EventHandler(this.goButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(309, 120);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Close";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // MakeDist
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(396, 155);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.goButton);
            this.Controls.Add(this.includeTestsCheckBox);
            this.Controls.Add(this.distributionTypeGroupBox);
            this.Controls.Add(this.descriptionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MakeDist";
            this.Text = "6502bench Distribution Maker";
            this.Load += new System.EventHandler(this.MakeDist_Load);
            this.distributionTypeGroupBox.ResumeLayout(false);
            this.distributionTypeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.GroupBox distributionTypeGroupBox;
        private System.Windows.Forms.RadioButton releaseDistRadio;
        private System.Windows.Forms.RadioButton debugDistRadio;
        private System.Windows.Forms.CheckBox includeTestsCheckBox;
        private System.Windows.Forms.Button goButton;
        private System.Windows.Forms.Button cancelButton;
    }
}

