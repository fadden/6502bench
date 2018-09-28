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
namespace SourceGen.Tests {
    partial class GenTestRunner {
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
            this.closeButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.progressRichTextBox = new System.Windows.Forms.RichTextBox();
            this.outputSelectComboBox = new System.Windows.Forms.ComboBox();
            this.outputTextBox = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.retainOutputCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(537, 406);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 23);
            this.closeButton.TabIndex = 4;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(13, 13);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 23);
            this.runButton.TabIndex = 0;
            this.runButton.Text = "Run Test";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // progressRichTextBox
            // 
            this.progressRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressRichTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.progressRichTextBox.Location = new System.Drawing.Point(13, 43);
            this.progressRichTextBox.Name = "progressRichTextBox";
            this.progressRichTextBox.ReadOnly = true;
            this.progressRichTextBox.Size = new System.Drawing.Size(599, 130);
            this.progressRichTextBox.TabIndex = 1;
            this.progressRichTextBox.Text = "";
            // 
            // outputSelectComboBox
            // 
            this.outputSelectComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.outputSelectComboBox.FormattingEnabled = true;
            this.outputSelectComboBox.Location = new System.Drawing.Point(13, 201);
            this.outputSelectComboBox.Name = "outputSelectComboBox";
            this.outputSelectComboBox.Size = new System.Drawing.Size(300, 21);
            this.outputSelectComboBox.TabIndex = 2;
            this.outputSelectComboBox.SelectedIndexChanged += new System.EventHandler(this.outputSelectComboBox_SelectedIndexChanged);
            // 
            // outputTextBox
            // 
            this.outputTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.outputTextBox.Location = new System.Drawing.Point(13, 229);
            this.outputTextBox.MaxLength = 0;
            this.outputTextBox.Multiline = true;
            this.outputTextBox.Name = "outputTextBox";
            this.outputTextBox.ReadOnly = true;
            this.outputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.outputTextBox.Size = new System.Drawing.Size(599, 171);
            this.outputTextBox.TabIndex = 3;
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // retainOutputCheckBox
            // 
            this.retainOutputCheckBox.AutoSize = true;
            this.retainOutputCheckBox.Location = new System.Drawing.Point(116, 17);
            this.retainOutputCheckBox.Name = "retainOutputCheckBox";
            this.retainOutputCheckBox.Size = new System.Drawing.Size(90, 17);
            this.retainOutputCheckBox.TabIndex = 5;
            this.retainOutputCheckBox.Text = "Retain output";
            this.retainOutputCheckBox.UseVisualStyleBackColor = true;
            // 
            // GenTestRunner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.retainOutputCheckBox);
            this.Controls.Add(this.outputTextBox);
            this.Controls.Add(this.outputSelectComboBox);
            this.Controls.Add(this.progressRichTextBox);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.closeButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 480);
            this.Name = "GenTestRunner";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Source Generation Test";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GenTestRunner_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.RichTextBox progressRichTextBox;
        private System.Windows.Forms.ComboBox outputSelectComboBox;
        private System.Windows.Forms.TextBox outputTextBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.CheckBox retainOutputCheckBox;
    }
}