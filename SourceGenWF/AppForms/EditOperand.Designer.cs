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
    partial class EditOperand {
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
            this.mainRadioPanel = new System.Windows.Forms.Panel();
            this.symbolPartPanel = new System.Windows.Forms.Panel();
            this.radioButtonBank = new System.Windows.Forms.RadioButton();
            this.radioButtonHigh = new System.Windows.Forms.RadioButton();
            this.radioButtonLow = new System.Windows.Forms.RadioButton();
            this.symbolTextBox = new System.Windows.Forms.TextBox();
            this.radioButtonSymbol = new System.Windows.Forms.RadioButton();
            this.radioButtonAscii = new System.Windows.Forms.RadioButton();
            this.radioButtonBinary = new System.Windows.Forms.RadioButton();
            this.radioButtonDecimal = new System.Windows.Forms.RadioButton();
            this.radioButtonHex = new System.Windows.Forms.RadioButton();
            this.radioButtonDefault = new System.Windows.Forms.RadioButton();
            this.selectFormatLabel = new System.Windows.Forms.Label();
            this.previewLabel = new System.Windows.Forms.Label();
            this.previewTextBox = new System.Windows.Forms.TextBox();
            this.symbolShortcutsGroupBox = new System.Windows.Forms.GroupBox();
            this.operandAndProjRadioButton = new System.Windows.Forms.RadioButton();
            this.operandAndLabelRadioButton = new System.Windows.Forms.RadioButton();
            this.labelInsteadRadioButton = new System.Windows.Forms.RadioButton();
            this.operandOnlyRadioButton = new System.Windows.Forms.RadioButton();
            this.mainRadioPanel.SuspendLayout();
            this.symbolPartPanel.SuspendLayout();
            this.symbolShortcutsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(199, 377);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(118, 377);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // mainRadioPanel
            // 
            this.mainRadioPanel.Controls.Add(this.symbolPartPanel);
            this.mainRadioPanel.Controls.Add(this.symbolTextBox);
            this.mainRadioPanel.Controls.Add(this.radioButtonSymbol);
            this.mainRadioPanel.Controls.Add(this.radioButtonAscii);
            this.mainRadioPanel.Controls.Add(this.radioButtonBinary);
            this.mainRadioPanel.Controls.Add(this.radioButtonDecimal);
            this.mainRadioPanel.Controls.Add(this.radioButtonHex);
            this.mainRadioPanel.Controls.Add(this.radioButtonDefault);
            this.mainRadioPanel.Location = new System.Drawing.Point(12, 29);
            this.mainRadioPanel.Name = "mainRadioPanel";
            this.mainRadioPanel.Size = new System.Drawing.Size(261, 173);
            this.mainRadioPanel.TabIndex = 0;
            // 
            // symbolPartPanel
            // 
            this.symbolPartPanel.Controls.Add(this.radioButtonBank);
            this.symbolPartPanel.Controls.Add(this.radioButtonHigh);
            this.symbolPartPanel.Controls.Add(this.radioButtonLow);
            this.symbolPartPanel.Location = new System.Drawing.Point(69, 150);
            this.symbolPartPanel.Name = "symbolPartPanel";
            this.symbolPartPanel.Size = new System.Drawing.Size(154, 18);
            this.symbolPartPanel.TabIndex = 7;
            // 
            // radioButtonBank
            // 
            this.radioButtonBank.AutoSize = true;
            this.radioButtonBank.Location = new System.Drawing.Point(100, 0);
            this.radioButtonBank.Name = "radioButtonBank";
            this.radioButtonBank.Size = new System.Drawing.Size(50, 17);
            this.radioButtonBank.TabIndex = 2;
            this.radioButtonBank.TabStop = true;
            this.radioButtonBank.Text = "&Bank";
            this.radioButtonBank.UseVisualStyleBackColor = true;
            this.radioButtonBank.CheckedChanged += new System.EventHandler(this.PartGroup_CheckedChanged);
            // 
            // radioButtonHigh
            // 
            this.radioButtonHigh.AutoSize = true;
            this.radioButtonHigh.Location = new System.Drawing.Point(50, 0);
            this.radioButtonHigh.Name = "radioButtonHigh";
            this.radioButtonHigh.Size = new System.Drawing.Size(47, 17);
            this.radioButtonHigh.TabIndex = 1;
            this.radioButtonHigh.TabStop = true;
            this.radioButtonHigh.Text = "&High";
            this.radioButtonHigh.UseVisualStyleBackColor = true;
            this.radioButtonHigh.CheckedChanged += new System.EventHandler(this.PartGroup_CheckedChanged);
            // 
            // radioButtonLow
            // 
            this.radioButtonLow.AutoSize = true;
            this.radioButtonLow.Location = new System.Drawing.Point(0, 0);
            this.radioButtonLow.Name = "radioButtonLow";
            this.radioButtonLow.Size = new System.Drawing.Size(45, 17);
            this.radioButtonLow.TabIndex = 0;
            this.radioButtonLow.TabStop = true;
            this.radioButtonLow.Text = "&Low";
            this.radioButtonLow.UseVisualStyleBackColor = true;
            this.radioButtonLow.CheckedChanged += new System.EventHandler(this.PartGroup_CheckedChanged);
            // 
            // symbolTextBox
            // 
            this.symbolTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.symbolTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.symbolTextBox.Location = new System.Drawing.Point(69, 124);
            this.symbolTextBox.MaxLength = 128;
            this.symbolTextBox.Name = "symbolTextBox";
            this.symbolTextBox.Size = new System.Drawing.Size(179, 20);
            this.symbolTextBox.TabIndex = 6;
            this.symbolTextBox.TextChanged += new System.EventHandler(this.symbolTextBox_TextChanged);
            // 
            // radioButtonSymbol
            // 
            this.radioButtonSymbol.AutoSize = true;
            this.radioButtonSymbol.Location = new System.Drawing.Point(4, 124);
            this.radioButtonSymbol.Name = "radioButtonSymbol";
            this.radioButtonSymbol.Size = new System.Drawing.Size(59, 17);
            this.radioButtonSymbol.TabIndex = 5;
            this.radioButtonSymbol.TabStop = true;
            this.radioButtonSymbol.Text = "&Symbol";
            this.radioButtonSymbol.UseVisualStyleBackColor = true;
            this.radioButtonSymbol.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioButtonAscii
            // 
            this.radioButtonAscii.AutoSize = true;
            this.radioButtonAscii.Location = new System.Drawing.Point(4, 100);
            this.radioButtonAscii.Name = "radioButtonAscii";
            this.radioButtonAscii.Size = new System.Drawing.Size(100, 17);
            this.radioButtonAscii.TabIndex = 4;
            this.radioButtonAscii.TabStop = true;
            this.radioButtonAscii.Text = "&ASCII character";
            this.radioButtonAscii.UseVisualStyleBackColor = true;
            this.radioButtonAscii.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioButtonBinary
            // 
            this.radioButtonBinary.AutoSize = true;
            this.radioButtonBinary.Location = new System.Drawing.Point(4, 76);
            this.radioButtonBinary.Name = "radioButtonBinary";
            this.radioButtonBinary.Size = new System.Drawing.Size(54, 17);
            this.radioButtonBinary.TabIndex = 3;
            this.radioButtonBinary.TabStop = true;
            this.radioButtonBinary.Text = "&Binary";
            this.radioButtonBinary.UseVisualStyleBackColor = true;
            this.radioButtonBinary.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioButtonDecimal
            // 
            this.radioButtonDecimal.AutoSize = true;
            this.radioButtonDecimal.Location = new System.Drawing.Point(4, 52);
            this.radioButtonDecimal.Name = "radioButtonDecimal";
            this.radioButtonDecimal.Size = new System.Drawing.Size(63, 17);
            this.radioButtonDecimal.TabIndex = 2;
            this.radioButtonDecimal.TabStop = true;
            this.radioButtonDecimal.Text = "&Decimal";
            this.radioButtonDecimal.UseVisualStyleBackColor = true;
            this.radioButtonDecimal.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioButtonHex
            // 
            this.radioButtonHex.AutoSize = true;
            this.radioButtonHex.Location = new System.Drawing.Point(4, 28);
            this.radioButtonHex.Name = "radioButtonHex";
            this.radioButtonHex.Size = new System.Drawing.Size(86, 17);
            this.radioButtonHex.TabIndex = 1;
            this.radioButtonHex.TabStop = true;
            this.radioButtonHex.Text = "&Hexadecimal";
            this.radioButtonHex.UseVisualStyleBackColor = true;
            this.radioButtonHex.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioButtonDefault
            // 
            this.radioButtonDefault.AutoSize = true;
            this.radioButtonDefault.Location = new System.Drawing.Point(4, 4);
            this.radioButtonDefault.Name = "radioButtonDefault";
            this.radioButtonDefault.Size = new System.Drawing.Size(59, 17);
            this.radioButtonDefault.TabIndex = 0;
            this.radioButtonDefault.TabStop = true;
            this.radioButtonDefault.Text = "&Default";
            this.radioButtonDefault.UseVisualStyleBackColor = true;
            this.radioButtonDefault.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // selectFormatLabel
            // 
            this.selectFormatLabel.AutoSize = true;
            this.selectFormatLabel.Location = new System.Drawing.Point(13, 13);
            this.selectFormatLabel.Name = "selectFormatLabel";
            this.selectFormatLabel.Size = new System.Drawing.Size(114, 13);
            this.selectFormatLabel.TabIndex = 0;
            this.selectFormatLabel.Text = "Select operand format:";
            // 
            // previewLabel
            // 
            this.previewLabel.AutoSize = true;
            this.previewLabel.Location = new System.Drawing.Point(11, 210);
            this.previewLabel.Name = "previewLabel";
            this.previewLabel.Size = new System.Drawing.Size(48, 13);
            this.previewLabel.TabIndex = 1;
            this.previewLabel.Text = "Preview:";
            // 
            // previewTextBox
            // 
            this.previewTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.previewTextBox.Enabled = false;
            this.previewTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.previewTextBox.Location = new System.Drawing.Point(66, 208);
            this.previewTextBox.Name = "previewTextBox";
            this.previewTextBox.Size = new System.Drawing.Size(207, 20);
            this.previewTextBox.TabIndex = 2;
            // 
            // symbolShortcutsGroupBox
            // 
            this.symbolShortcutsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.symbolShortcutsGroupBox.Controls.Add(this.operandAndProjRadioButton);
            this.symbolShortcutsGroupBox.Controls.Add(this.operandAndLabelRadioButton);
            this.symbolShortcutsGroupBox.Controls.Add(this.labelInsteadRadioButton);
            this.symbolShortcutsGroupBox.Controls.Add(this.operandOnlyRadioButton);
            this.symbolShortcutsGroupBox.Location = new System.Drawing.Point(13, 248);
            this.symbolShortcutsGroupBox.Name = "symbolShortcutsGroupBox";
            this.symbolShortcutsGroupBox.Size = new System.Drawing.Size(261, 118);
            this.symbolShortcutsGroupBox.TabIndex = 5;
            this.symbolShortcutsGroupBox.TabStop = false;
            this.symbolShortcutsGroupBox.Text = "Symbol Shortcuts";
            // 
            // operandAndProjRadioButton
            // 
            this.operandAndProjRadioButton.AutoSize = true;
            this.operandAndProjRadioButton.Location = new System.Drawing.Point(7, 92);
            this.operandAndProjRadioButton.Name = "operandAndProjRadioButton";
            this.operandAndProjRadioButton.Size = new System.Drawing.Size(212, 17);
            this.operandAndProjRadioButton.TabIndex = 3;
            this.operandAndProjRadioButton.TabStop = true;
            this.operandAndProjRadioButton.Text = "Set operand AND create &project symbol";
            this.operandAndProjRadioButton.UseVisualStyleBackColor = true;
            // 
            // operandAndLabelRadioButton
            // 
            this.operandAndLabelRadioButton.AutoSize = true;
            this.operandAndLabelRadioButton.Location = new System.Drawing.Point(7, 68);
            this.operandAndLabelRadioButton.Name = "operandAndLabelRadioButton";
            this.operandAndLabelRadioButton.Size = new System.Drawing.Size(249, 17);
            this.operandAndLabelRadioButton.TabIndex = 2;
            this.operandAndLabelRadioButton.TabStop = true;
            this.operandAndLabelRadioButton.Text = "Set &operand AND create label at target address";
            this.operandAndLabelRadioButton.UseVisualStyleBackColor = true;
            // 
            // labelInsteadRadioButton
            // 
            this.labelInsteadRadioButton.AutoSize = true;
            this.labelInsteadRadioButton.Location = new System.Drawing.Point(7, 44);
            this.labelInsteadRadioButton.Name = "labelInsteadRadioButton";
            this.labelInsteadRadioButton.Size = new System.Drawing.Size(200, 17);
            this.labelInsteadRadioButton.TabIndex = 1;
            this.labelInsteadRadioButton.TabStop = true;
            this.labelInsteadRadioButton.Text = "&Create label at target address instead";
            this.labelInsteadRadioButton.UseVisualStyleBackColor = true;
            // 
            // operandOnlyRadioButton
            // 
            this.operandOnlyRadioButton.AutoSize = true;
            this.operandOnlyRadioButton.Location = new System.Drawing.Point(7, 20);
            this.operandOnlyRadioButton.Name = "operandOnlyRadioButton";
            this.operandOnlyRadioButton.Size = new System.Drawing.Size(162, 17);
            this.operandOnlyRadioButton.TabIndex = 0;
            this.operandOnlyRadioButton.TabStop = true;
            this.operandOnlyRadioButton.Text = "&Just set the operand (default)";
            this.operandOnlyRadioButton.UseVisualStyleBackColor = true;
            // 
            // EditOperand
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(286, 412);
            this.Controls.Add(this.symbolShortcutsGroupBox);
            this.Controls.Add(this.previewTextBox);
            this.Controls.Add(this.previewLabel);
            this.Controls.Add(this.selectFormatLabel);
            this.Controls.Add(this.mainRadioPanel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditOperand";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Instruction Operand";
            this.Load += new System.EventHandler(this.EditOperand_Load);
            this.Shown += new System.EventHandler(this.EditOperand_Shown);
            this.mainRadioPanel.ResumeLayout(false);
            this.mainRadioPanel.PerformLayout();
            this.symbolPartPanel.ResumeLayout(false);
            this.symbolPartPanel.PerformLayout();
            this.symbolShortcutsGroupBox.ResumeLayout(false);
            this.symbolShortcutsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Panel mainRadioPanel;
        private System.Windows.Forms.TextBox symbolTextBox;
        private System.Windows.Forms.RadioButton radioButtonSymbol;
        private System.Windows.Forms.RadioButton radioButtonAscii;
        private System.Windows.Forms.RadioButton radioButtonBinary;
        private System.Windows.Forms.RadioButton radioButtonDecimal;
        private System.Windows.Forms.RadioButton radioButtonHex;
        private System.Windows.Forms.RadioButton radioButtonDefault;
        private System.Windows.Forms.Label selectFormatLabel;
        private System.Windows.Forms.Label previewLabel;
        private System.Windows.Forms.TextBox previewTextBox;
        private System.Windows.Forms.Panel symbolPartPanel;
        private System.Windows.Forms.RadioButton radioButtonBank;
        private System.Windows.Forms.RadioButton radioButtonHigh;
        private System.Windows.Forms.RadioButton radioButtonLow;
        private System.Windows.Forms.GroupBox symbolShortcutsGroupBox;
        private System.Windows.Forms.RadioButton operandAndProjRadioButton;
        private System.Windows.Forms.RadioButton operandAndLabelRadioButton;
        private System.Windows.Forms.RadioButton labelInsteadRadioButton;
        private System.Windows.Forms.RadioButton operandOnlyRadioButton;
    }
}