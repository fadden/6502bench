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
    partial class AboutBox {
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
            this.boardPictureBox = new System.Windows.Forms.PictureBox();
            this.sourceGenLabel = new System.Windows.Forms.Label();
            this.versionLabel = new System.Windows.Forms.Label();
            this.createdLabel = new System.Windows.Forms.Label();
            this.okButton = new System.Windows.Forms.Button();
            this.legalStuffTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.osPlatformLabel = new System.Windows.Forms.Label();
            this.debugEnabledLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.boardPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // boardPictureBox
            // 
            this.boardPictureBox.Location = new System.Drawing.Point(13, 13);
            this.boardPictureBox.Name = "boardPictureBox";
            this.boardPictureBox.Size = new System.Drawing.Size(320, 236);
            this.boardPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.boardPictureBox.TabIndex = 0;
            this.boardPictureBox.TabStop = false;
            // 
            // sourceGenLabel
            // 
            this.sourceGenLabel.AutoSize = true;
            this.sourceGenLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sourceGenLabel.Location = new System.Drawing.Point(340, 13);
            this.sourceGenLabel.Name = "sourceGenLabel";
            this.sourceGenLabel.Size = new System.Drawing.Size(349, 37);
            this.sourceGenLabel.TabIndex = 1;
            this.sourceGenLabel.Text = "6502bench SourceGen";
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.versionLabel.Location = new System.Drawing.Point(340, 60);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(273, 31);
            this.versionLabel.TabIndex = 2;
            this.versionLabel.Text = "Version X.Y.Z Alpha1";
            // 
            // createdLabel
            // 
            this.createdLabel.AutoSize = true;
            this.createdLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.createdLabel.Location = new System.Drawing.Point(407, 142);
            this.createdLabel.Name = "createdLabel";
            this.createdLabel.Size = new System.Drawing.Size(206, 40);
            this.createdLabel.TabIndex = 3;
            this.createdLabel.Text = "Copyright 2018 faddenSoft\r\nCreated by Andy McFadden\r\n";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(617, 526);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            // 
            // legalStuffTextBox
            // 
            this.legalStuffTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.legalStuffTextBox.Location = new System.Drawing.Point(12, 305);
            this.legalStuffTextBox.MaxLength = 0;
            this.legalStuffTextBox.Multiline = true;
            this.legalStuffTextBox.Name = "legalStuffTextBox";
            this.legalStuffTextBox.ReadOnly = true;
            this.legalStuffTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.legalStuffTextBox.Size = new System.Drawing.Size(680, 215);
            this.legalStuffTextBox.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 289);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Legal stuff:";
            // 
            // osPlatformLabel
            // 
            this.osPlatformLabel.AutoSize = true;
            this.osPlatformLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.osPlatformLabel.Location = new System.Drawing.Point(12, 263);
            this.osPlatformLabel.Name = "osPlatformLabel";
            this.osPlatformLabel.Size = new System.Drawing.Size(86, 16);
            this.osPlatformLabel.TabIndex = 6;
            this.osPlatformLabel.Text = "[OS platform]";
            // 
            // debugEnabledLabel
            // 
            this.debugEnabledLabel.AutoSize = true;
            this.debugEnabledLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.debugEnabledLabel.Location = new System.Drawing.Point(340, 233);
            this.debugEnabledLabel.Name = "debugEnabledLabel";
            this.debugEnabledLabel.Size = new System.Drawing.Size(293, 16);
            this.debugEnabledLabel.TabIndex = 7;
            this.debugEnabledLabel.Text = "Assertions and extended validation are enabled";
            this.debugEnabledLabel.Visible = false;
            // 
            // AboutBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(704, 561);
            this.Controls.Add(this.debugEnabledLabel);
            this.Controls.Add(this.osPlatformLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.legalStuffTextBox);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.createdLabel);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.sourceGenLabel);
            this.Controls.Add(this.boardPictureBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AboutBox";
            this.Load += new System.EventHandler(this.AboutBox_Load);
            ((System.ComponentModel.ISupportInitialize)(this.boardPictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox boardPictureBox;
        private System.Windows.Forms.Label sourceGenLabel;
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label createdLabel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TextBox legalStuffTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label osPlatformLabel;
        private System.Windows.Forms.Label debugEnabledLabel;
    }
}