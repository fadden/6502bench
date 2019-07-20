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
namespace SourceGenWF.AsmGen {
    partial class GenAndAsm {
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.assemblerSettingsButton = new System.Windows.Forms.Button();
            this.previewFileComboBox = new System.Windows.Forms.ComboBox();
            this.previewFileLabel = new System.Windows.Forms.Label();
            this.workDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.outputFileLabel = new System.Windows.Forms.Label();
            this.generateButton = new System.Windows.Forms.Button();
            this.previewTextBox = new System.Windows.Forms.TextBox();
            this.assemblerComboBox = new System.Windows.Forms.ComboBox();
            this.assemblerLabel = new System.Windows.Forms.Label();
            this.configureAsmLinkLabel = new System.Windows.Forms.LinkLabel();
            this.cmdOutputTextBox = new System.Windows.Forms.TextBox();
            this.runAssemblerButton = new System.Windows.Forms.Button();
            this.basePanel = new System.Windows.Forms.Panel();
            this.closeButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.basePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.assemblerSettingsButton);
            this.splitContainer1.Panel1.Controls.Add(this.previewFileComboBox);
            this.splitContainer1.Panel1.Controls.Add(this.previewFileLabel);
            this.splitContainer1.Panel1.Controls.Add(this.workDirectoryTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.outputFileLabel);
            this.splitContainer1.Panel1.Controls.Add(this.generateButton);
            this.splitContainer1.Panel1.Controls.Add(this.previewTextBox);
            this.splitContainer1.Panel1.Controls.Add(this.assemblerComboBox);
            this.splitContainer1.Panel1.Controls.Add(this.assemblerLabel);
            this.splitContainer1.Panel1MinSize = 150;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.configureAsmLinkLabel);
            this.splitContainer1.Panel2.Controls.Add(this.cmdOutputTextBox);
            this.splitContainer1.Panel2.Controls.Add(this.runAssemblerButton);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(784, 612);
            this.splitContainer1.SplitterDistance = 400;
            this.splitContainer1.TabIndex = 0;
            // 
            // assemblerSettingsButton
            // 
            this.assemblerSettingsButton.Location = new System.Drawing.Point(241, 10);
            this.assemblerSettingsButton.Name = "assemblerSettingsButton";
            this.assemblerSettingsButton.Size = new System.Drawing.Size(75, 23);
            this.assemblerSettingsButton.TabIndex = 3;
            this.assemblerSettingsButton.Text = "Settings";
            this.assemblerSettingsButton.UseVisualStyleBackColor = true;
            this.assemblerSettingsButton.Click += new System.EventHandler(this.assemblerSettingsButton_Click);
            // 
            // previewFileComboBox
            // 
            this.previewFileComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.previewFileComboBox.FormattingEnabled = true;
            this.previewFileComboBox.Items.AddRange(new object[] {
            "SampleFile#031234_Merlin32.S"});
            this.previewFileComboBox.Location = new System.Drawing.Point(83, 58);
            this.previewFileComboBox.Name = "previewFileComboBox";
            this.previewFileComboBox.Size = new System.Drawing.Size(262, 21);
            this.previewFileComboBox.TabIndex = 5;
            this.previewFileComboBox.SelectedIndexChanged += new System.EventHandler(this.previewFileComboBox_SelectedIndexChanged);
            // 
            // previewFileLabel
            // 
            this.previewFileLabel.AutoSize = true;
            this.previewFileLabel.Location = new System.Drawing.Point(13, 61);
            this.previewFileLabel.Name = "previewFileLabel";
            this.previewFileLabel.Size = new System.Drawing.Size(64, 13);
            this.previewFileLabel.TabIndex = 4;
            this.previewFileLabel.Text = "Preview file:";
            // 
            // workDirectoryTextBox
            // 
            this.workDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.workDirectoryTextBox.Location = new System.Drawing.Point(445, 59);
            this.workDirectoryTextBox.Name = "workDirectoryTextBox";
            this.workDirectoryTextBox.ReadOnly = true;
            this.workDirectoryTextBox.Size = new System.Drawing.Size(327, 20);
            this.workDirectoryTextBox.TabIndex = 7;
            this.workDirectoryTextBox.Text = "C:\\this\\that\\theother";
            // 
            // outputFileLabel
            // 
            this.outputFileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputFileLabel.AutoSize = true;
            this.outputFileLabel.Location = new System.Drawing.Point(360, 62);
            this.outputFileLabel.Name = "outputFileLabel";
            this.outputFileLabel.Size = new System.Drawing.Size(79, 13);
            this.outputFileLabel.TabIndex = 6;
            this.outputFileLabel.Text = "Work directory:";
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(363, 10);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(94, 23);
            this.generateButton.TabIndex = 0;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // previewTextBox
            // 
            this.previewTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.previewTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previewTextBox.Location = new System.Drawing.Point(13, 85);
            this.previewTextBox.MaxLength = 0;
            this.previewTextBox.Multiline = true;
            this.previewTextBox.Name = "previewTextBox";
            this.previewTextBox.ReadOnly = true;
            this.previewTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.previewTextBox.Size = new System.Drawing.Size(759, 303);
            this.previewTextBox.TabIndex = 8;
            // 
            // assemblerComboBox
            // 
            this.assemblerComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.assemblerComboBox.FormattingEnabled = true;
            this.assemblerComboBox.Items.AddRange(new object[] {
            "Merlin32",
            "CA65"});
            this.assemblerComboBox.Location = new System.Drawing.Point(83, 12);
            this.assemblerComboBox.Name = "assemblerComboBox";
            this.assemblerComboBox.Size = new System.Drawing.Size(152, 21);
            this.assemblerComboBox.TabIndex = 2;
            this.assemblerComboBox.SelectedIndexChanged += new System.EventHandler(this.assemblerComboBox_SelectedIndexChanged);
            // 
            // assemblerLabel
            // 
            this.assemblerLabel.AutoSize = true;
            this.assemblerLabel.Location = new System.Drawing.Point(13, 15);
            this.assemblerLabel.Name = "assemblerLabel";
            this.assemblerLabel.Size = new System.Drawing.Size(58, 13);
            this.assemblerLabel.TabIndex = 1;
            this.assemblerLabel.Text = "Assembler:";
            // 
            // configureAsmLinkLabel
            // 
            this.configureAsmLinkLabel.AutoSize = true;
            this.configureAsmLinkLabel.Location = new System.Drawing.Point(124, 11);
            this.configureAsmLinkLabel.Name = "configureAsmLinkLabel";
            this.configureAsmLinkLabel.Size = new System.Drawing.Size(126, 13);
            this.configureAsmLinkLabel.TabIndex = 1;
            this.configureAsmLinkLabel.TabStop = true;
            this.configureAsmLinkLabel.Text = "Assembler not configured";
            this.configureAsmLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.configureAsmLinkLabel_LinkClicked);
            // 
            // cmdOutputTextBox
            // 
            this.cmdOutputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmdOutputTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.cmdOutputTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmdOutputTextBox.Location = new System.Drawing.Point(10, 36);
            this.cmdOutputTextBox.MaxLength = 0;
            this.cmdOutputTextBox.Multiline = true;
            this.cmdOutputTextBox.Name = "cmdOutputTextBox";
            this.cmdOutputTextBox.ReadOnly = true;
            this.cmdOutputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.cmdOutputTextBox.Size = new System.Drawing.Size(762, 169);
            this.cmdOutputTextBox.TabIndex = 2;
            // 
            // runAssemblerButton
            // 
            this.runAssemblerButton.Location = new System.Drawing.Point(10, 6);
            this.runAssemblerButton.Name = "runAssemblerButton";
            this.runAssemblerButton.Size = new System.Drawing.Size(97, 23);
            this.runAssemblerButton.TabIndex = 0;
            this.runAssemblerButton.Text = "Run Assembler";
            this.runAssemblerButton.UseVisualStyleBackColor = true;
            this.runAssemblerButton.Click += new System.EventHandler(this.runAssemblerButton_Click);
            // 
            // basePanel
            // 
            this.basePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.basePanel.Controls.Add(this.splitContainer1);
            this.basePanel.Location = new System.Drawing.Point(0, 0);
            this.basePanel.Name = "basePanel";
            this.basePanel.Size = new System.Drawing.Size(784, 612);
            this.basePanel.TabIndex = 0;
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(697, 626);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // GenAndAsm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.basePanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "GenAndAsm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate and Assemble";
            this.Load += new System.EventHandler(this.GenAndAsm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.basePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel basePanel;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.TextBox previewTextBox;
        private System.Windows.Forms.ComboBox assemblerComboBox;
        private System.Windows.Forms.Label assemblerLabel;
        private System.Windows.Forms.TextBox cmdOutputTextBox;
        private System.Windows.Forms.Button runAssemblerButton;
        private System.Windows.Forms.TextBox workDirectoryTextBox;
        private System.Windows.Forms.Label outputFileLabel;
        private System.Windows.Forms.ComboBox previewFileComboBox;
        private System.Windows.Forms.Label previewFileLabel;
        private System.Windows.Forms.Button assemblerSettingsButton;
        private System.Windows.Forms.LinkLabel configureAsmLinkLabel;
    }
}