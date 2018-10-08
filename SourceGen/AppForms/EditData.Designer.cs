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
    partial class EditData {
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
            this.selectFormatLabel = new System.Windows.Forms.Label();
            this.rawDataSectionLabel = new System.Windows.Forms.Label();
            this.radioSingleBytes = new System.Windows.Forms.RadioButton();
            this.radio16BitBig = new System.Windows.Forms.RadioButton();
            this.radio16BitLittle = new System.Windows.Forms.RadioButton();
            this.radio24BitLittle = new System.Windows.Forms.RadioButton();
            this.radio32BitLittle = new System.Windows.Forms.RadioButton();
            this.radioSimpleDataBinary = new System.Windows.Forms.RadioButton();
            this.radioSimpleDataDecimal = new System.Windows.Forms.RadioButton();
            this.radioSimpleDataHex = new System.Windows.Forms.RadioButton();
            this.radioDenseHex = new System.Windows.Forms.RadioButton();
            this.radioFill = new System.Windows.Forms.RadioButton();
            this.horizontalLine1 = new System.Windows.Forms.Label();
            this.horizontalLine2 = new System.Windows.Forms.Label();
            this.stringSectionLabel = new System.Windows.Forms.Label();
            this.horizontalLine3 = new System.Windows.Forms.Label();
            this.radioStringMixed = new System.Windows.Forms.RadioButton();
            this.radioStringMixedReverse = new System.Windows.Forms.RadioButton();
            this.radioStringNullTerm = new System.Windows.Forms.RadioButton();
            this.radioStringLen8 = new System.Windows.Forms.RadioButton();
            this.radioStringLen16 = new System.Windows.Forms.RadioButton();
            this.radioStringDci = new System.Windows.Forms.RadioButton();
            this.symbolEntryTextBox = new System.Windows.Forms.TextBox();
            this.symbolPartPanel = new System.Windows.Forms.Panel();
            this.radioSymbolPartBank = new System.Windows.Forms.RadioButton();
            this.radioSymbolPartHigh = new System.Windows.Forms.RadioButton();
            this.radioSymbolPartLow = new System.Windows.Forms.RadioButton();
            this.radioSimpleDataAscii = new System.Windows.Forms.RadioButton();
            this.radioDefaultFormat = new System.Windows.Forms.RadioButton();
            this.simpleDisplayAsGroupBox = new System.Windows.Forms.GroupBox();
            this.radioSimpleDataAddress = new System.Windows.Forms.RadioButton();
            this.radioSimpleDataSymbolic = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.symbolPartPanel.SuspendLayout();
            this.simpleDisplayAsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(438, 461);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 23;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(357, 461);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 22;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // selectFormatLabel
            // 
            this.selectFormatLabel.AutoSize = true;
            this.selectFormatLabel.Location = new System.Drawing.Point(12, 9);
            this.selectFormatLabel.Name = "selectFormatLabel";
            this.selectFormatLabel.Size = new System.Drawing.Size(253, 13);
            this.selectFormatLabel.TabIndex = 0;
            this.selectFormatLabel.Text = "Select data format ({0} bytes selected in {1} groups):";
            // 
            // rawDataSectionLabel
            // 
            this.rawDataSectionLabel.AutoSize = true;
            this.rawDataSectionLabel.Location = new System.Drawing.Point(12, 63);
            this.rawDataSectionLabel.Name = "rawDataSectionLabel";
            this.rawDataSectionLabel.Size = new System.Drawing.Size(64, 13);
            this.rawDataSectionLabel.TabIndex = 2;
            this.rawDataSectionLabel.Text = "Simple Data";
            // 
            // radioSingleBytes
            // 
            this.radioSingleBytes.AutoSize = true;
            this.radioSingleBytes.Location = new System.Drawing.Point(14, 89);
            this.radioSingleBytes.Name = "radioSingleBytes";
            this.radioSingleBytes.Size = new System.Drawing.Size(82, 17);
            this.radioSingleBytes.TabIndex = 4;
            this.radioSingleBytes.TabStop = true;
            this.radioSingleBytes.Text = "Single &bytes";
            this.radioSingleBytes.UseVisualStyleBackColor = true;
            this.radioSingleBytes.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radio16BitBig
            // 
            this.radio16BitBig.AutoSize = true;
            this.radio16BitBig.Location = new System.Drawing.Point(14, 135);
            this.radio16BitBig.Name = "radio16BitBig";
            this.radio16BitBig.Size = new System.Drawing.Size(137, 17);
            this.radio16BitBig.TabIndex = 6;
            this.radio16BitBig.TabStop = true;
            this.radio16BitBig.Text = "16-bit words, big-endian";
            this.radio16BitBig.UseVisualStyleBackColor = true;
            this.radio16BitBig.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radio16BitLittle
            // 
            this.radio16BitLittle.AutoSize = true;
            this.radio16BitLittle.Location = new System.Drawing.Point(14, 112);
            this.radio16BitLittle.Name = "radio16BitLittle";
            this.radio16BitLittle.Size = new System.Drawing.Size(141, 17);
            this.radio16BitLittle.TabIndex = 5;
            this.radio16BitLittle.TabStop = true;
            this.radio16BitLittle.Text = "16-bit words, little-endian";
            this.radio16BitLittle.UseVisualStyleBackColor = true;
            this.radio16BitLittle.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radio24BitLittle
            // 
            this.radio24BitLittle.AutoSize = true;
            this.radio24BitLittle.Location = new System.Drawing.Point(14, 158);
            this.radio24BitLittle.Name = "radio24BitLittle";
            this.radio24BitLittle.Size = new System.Drawing.Size(141, 17);
            this.radio24BitLittle.TabIndex = 7;
            this.radio24BitLittle.TabStop = true;
            this.radio24BitLittle.Text = "24-bit words, little-endian";
            this.radio24BitLittle.UseVisualStyleBackColor = true;
            this.radio24BitLittle.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radio32BitLittle
            // 
            this.radio32BitLittle.AutoSize = true;
            this.radio32BitLittle.Location = new System.Drawing.Point(14, 181);
            this.radio32BitLittle.Name = "radio32BitLittle";
            this.radio32BitLittle.Size = new System.Drawing.Size(141, 17);
            this.radio32BitLittle.TabIndex = 8;
            this.radio32BitLittle.TabStop = true;
            this.radio32BitLittle.Text = "32-bit words, little-endian";
            this.radio32BitLittle.UseVisualStyleBackColor = true;
            this.radio32BitLittle.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioSimpleDataBinary
            // 
            this.radioSimpleDataBinary.AutoSize = true;
            this.radioSimpleDataBinary.Location = new System.Drawing.Point(6, 64);
            this.radioSimpleDataBinary.Name = "radioSimpleDataBinary";
            this.radioSimpleDataBinary.Size = new System.Drawing.Size(54, 17);
            this.radioSimpleDataBinary.TabIndex = 4;
            this.radioSimpleDataBinary.TabStop = true;
            this.radioSimpleDataBinary.Text = "Binary";
            this.radioSimpleDataBinary.UseVisualStyleBackColor = true;
            this.radioSimpleDataBinary.CheckedChanged += new System.EventHandler(this.SimpleDisplay_CheckedChanged);
            // 
            // radioSimpleDataDecimal
            // 
            this.radioSimpleDataDecimal.AutoSize = true;
            this.radioSimpleDataDecimal.Location = new System.Drawing.Point(6, 41);
            this.radioSimpleDataDecimal.Name = "radioSimpleDataDecimal";
            this.radioSimpleDataDecimal.Size = new System.Drawing.Size(63, 17);
            this.radioSimpleDataDecimal.TabIndex = 3;
            this.radioSimpleDataDecimal.TabStop = true;
            this.radioSimpleDataDecimal.Text = "Decimal";
            this.radioSimpleDataDecimal.UseVisualStyleBackColor = true;
            this.radioSimpleDataDecimal.CheckedChanged += new System.EventHandler(this.SimpleDisplay_CheckedChanged);
            // 
            // radioSimpleDataHex
            // 
            this.radioSimpleDataHex.AutoSize = true;
            this.radioSimpleDataHex.Location = new System.Drawing.Point(6, 18);
            this.radioSimpleDataHex.Name = "radioSimpleDataHex";
            this.radioSimpleDataHex.Size = new System.Drawing.Size(44, 17);
            this.radioSimpleDataHex.TabIndex = 2;
            this.radioSimpleDataHex.TabStop = true;
            this.radioSimpleDataHex.Text = "Hex";
            this.radioSimpleDataHex.UseVisualStyleBackColor = true;
            this.radioSimpleDataHex.CheckedChanged += new System.EventHandler(this.SimpleDisplay_CheckedChanged);
            // 
            // radioDenseHex
            // 
            this.radioDenseHex.AutoSize = true;
            this.radioDenseHex.Location = new System.Drawing.Point(14, 247);
            this.radioDenseHex.Name = "radioDenseHex";
            this.radioDenseHex.Size = new System.Drawing.Size(130, 17);
            this.radioDenseHex.TabIndex = 11;
            this.radioDenseHex.TabStop = true;
            this.radioDenseHex.Text = "Densely-&packed bytes";
            this.radioDenseHex.UseVisualStyleBackColor = true;
            this.radioDenseHex.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioFill
            // 
            this.radioFill.AutoSize = true;
            this.radioFill.Location = new System.Drawing.Point(14, 270);
            this.radioFill.Name = "radioFill";
            this.radioFill.Size = new System.Drawing.Size(88, 17);
            this.radioFill.TabIndex = 12;
            this.radioFill.TabStop = true;
            this.radioFill.Text = "&Fill with value";
            this.radioFill.UseVisualStyleBackColor = true;
            this.radioFill.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // horizontalLine1
            // 
            this.horizontalLine1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.horizontalLine1.Location = new System.Drawing.Point(12, 81);
            this.horizontalLine1.Name = "horizontalLine1";
            this.horizontalLine1.Size = new System.Drawing.Size(500, 2);
            this.horizontalLine1.TabIndex = 3;
            // 
            // horizontalLine2
            // 
            this.horizontalLine2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.horizontalLine2.Location = new System.Drawing.Point(12, 239);
            this.horizontalLine2.Name = "horizontalLine2";
            this.horizontalLine2.Size = new System.Drawing.Size(320, 2);
            this.horizontalLine2.TabIndex = 10;
            // 
            // stringSectionLabel
            // 
            this.stringSectionLabel.AutoSize = true;
            this.stringSectionLabel.Location = new System.Drawing.Point(12, 308);
            this.stringSectionLabel.Name = "stringSectionLabel";
            this.stringSectionLabel.Size = new System.Drawing.Size(34, 13);
            this.stringSectionLabel.TabIndex = 13;
            this.stringSectionLabel.Text = "String";
            // 
            // horizontalLine3
            // 
            this.horizontalLine3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.horizontalLine3.Location = new System.Drawing.Point(12, 326);
            this.horizontalLine3.Name = "horizontalLine3";
            this.horizontalLine3.Size = new System.Drawing.Size(320, 2);
            this.horizontalLine3.TabIndex = 14;
            // 
            // radioStringMixed
            // 
            this.radioStringMixed.AutoSize = true;
            this.radioStringMixed.Location = new System.Drawing.Point(14, 334);
            this.radioStringMixed.Name = "radioStringMixed";
            this.radioStringMixed.Size = new System.Drawing.Size(257, 17);
            this.radioStringMixed.TabIndex = 15;
            this.radioStringMixed.TabStop = true;
            this.radioStringMixed.Text = "Mixed ASCII ({0} bytes) and non-ASCII ({1} bytes)";
            this.radioStringMixed.UseVisualStyleBackColor = true;
            this.radioStringMixed.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioStringMixedReverse
            // 
            this.radioStringMixedReverse.AutoSize = true;
            this.radioStringMixedReverse.Location = new System.Drawing.Point(14, 357);
            this.radioStringMixedReverse.Name = "radioStringMixedReverse";
            this.radioStringMixedReverse.Size = new System.Drawing.Size(275, 17);
            this.radioStringMixedReverse.TabIndex = 16;
            this.radioStringMixedReverse.TabStop = true;
            this.radioStringMixedReverse.Text = "Reversed ASCII ({0} bytes) and non-ASCII ({1} bytes)";
            this.radioStringMixedReverse.UseVisualStyleBackColor = true;
            this.radioStringMixedReverse.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioStringNullTerm
            // 
            this.radioStringNullTerm.AutoSize = true;
            this.radioStringNullTerm.Location = new System.Drawing.Point(14, 381);
            this.radioStringNullTerm.Name = "radioStringNullTerm";
            this.radioStringNullTerm.Size = new System.Drawing.Size(151, 17);
            this.radioStringNullTerm.TabIndex = 17;
            this.radioStringNullTerm.TabStop = true;
            this.radioStringNullTerm.Text = "Null-terminated strings ({0})";
            this.radioStringNullTerm.UseVisualStyleBackColor = true;
            this.radioStringNullTerm.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioStringLen8
            // 
            this.radioStringLen8.AutoSize = true;
            this.radioStringLen8.Location = new System.Drawing.Point(14, 405);
            this.radioStringLen8.Name = "radioStringLen8";
            this.radioStringLen8.Size = new System.Drawing.Size(197, 17);
            this.radioStringLen8.TabIndex = 18;
            this.radioStringLen8.TabStop = true;
            this.radioStringLen8.Text = "Strings prefixed with 8-bit length ({0})";
            this.radioStringLen8.UseVisualStyleBackColor = true;
            this.radioStringLen8.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioStringLen16
            // 
            this.radioStringLen16.AutoSize = true;
            this.radioStringLen16.Location = new System.Drawing.Point(14, 429);
            this.radioStringLen16.Name = "radioStringLen16";
            this.radioStringLen16.Size = new System.Drawing.Size(203, 17);
            this.radioStringLen16.TabIndex = 19;
            this.radioStringLen16.TabStop = true;
            this.radioStringLen16.Text = "Strings prefixed with 16-bit length ({0})";
            this.radioStringLen16.UseVisualStyleBackColor = true;
            this.radioStringLen16.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // radioStringDci
            // 
            this.radioStringDci.AutoSize = true;
            this.radioStringDci.Location = new System.Drawing.Point(14, 453);
            this.radioStringDci.Name = "radioStringDci";
            this.radioStringDci.Size = new System.Drawing.Size(170, 17);
            this.radioStringDci.TabIndex = 20;
            this.radioStringDci.TabStop = true;
            this.radioStringDci.Text = "Dextral character inverted ({0})";
            this.radioStringDci.UseVisualStyleBackColor = true;
            this.radioStringDci.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // symbolEntryTextBox
            // 
            this.symbolEntryTextBox.Location = new System.Drawing.Point(108, 62);
            this.symbolEntryTextBox.Name = "symbolEntryTextBox";
            this.symbolEntryTextBox.Size = new System.Drawing.Size(200, 20);
            this.symbolEntryTextBox.TabIndex = 1;
            this.symbolEntryTextBox.TextChanged += new System.EventHandler(this.symbolEntryTextBox_TextChanged);
            // 
            // symbolPartPanel
            // 
            this.symbolPartPanel.Controls.Add(this.radioSymbolPartBank);
            this.symbolPartPanel.Controls.Add(this.radioSymbolPartHigh);
            this.symbolPartPanel.Controls.Add(this.radioSymbolPartLow);
            this.symbolPartPanel.Location = new System.Drawing.Point(108, 86);
            this.symbolPartPanel.Name = "symbolPartPanel";
            this.symbolPartPanel.Size = new System.Drawing.Size(200, 20);
            this.symbolPartPanel.TabIndex = 27;
            // 
            // radioSymbolPartBank
            // 
            this.radioSymbolPartBank.AutoSize = true;
            this.radioSymbolPartBank.Location = new System.Drawing.Point(103, 1);
            this.radioSymbolPartBank.Name = "radioSymbolPartBank";
            this.radioSymbolPartBank.Size = new System.Drawing.Size(50, 17);
            this.radioSymbolPartBank.TabIndex = 2;
            this.radioSymbolPartBank.TabStop = true;
            this.radioSymbolPartBank.Text = "Bank";
            this.radioSymbolPartBank.UseVisualStyleBackColor = true;
            this.radioSymbolPartBank.CheckedChanged += new System.EventHandler(this.PartGroup_CheckedChanged);
            // 
            // radioSymbolPartHigh
            // 
            this.radioSymbolPartHigh.AutoSize = true;
            this.radioSymbolPartHigh.Location = new System.Drawing.Point(52, 1);
            this.radioSymbolPartHigh.Name = "radioSymbolPartHigh";
            this.radioSymbolPartHigh.Size = new System.Drawing.Size(47, 17);
            this.radioSymbolPartHigh.TabIndex = 1;
            this.radioSymbolPartHigh.TabStop = true;
            this.radioSymbolPartHigh.Text = "High";
            this.radioSymbolPartHigh.UseVisualStyleBackColor = true;
            this.radioSymbolPartHigh.CheckedChanged += new System.EventHandler(this.PartGroup_CheckedChanged);
            // 
            // radioSymbolPartLow
            // 
            this.radioSymbolPartLow.AutoSize = true;
            this.radioSymbolPartLow.Location = new System.Drawing.Point(4, 1);
            this.radioSymbolPartLow.Name = "radioSymbolPartLow";
            this.radioSymbolPartLow.Size = new System.Drawing.Size(45, 17);
            this.radioSymbolPartLow.TabIndex = 0;
            this.radioSymbolPartLow.TabStop = true;
            this.radioSymbolPartLow.Text = "Low";
            this.radioSymbolPartLow.UseVisualStyleBackColor = true;
            this.radioSymbolPartLow.CheckedChanged += new System.EventHandler(this.PartGroup_CheckedChanged);
            // 
            // radioSimpleDataAscii
            // 
            this.radioSimpleDataAscii.AutoSize = true;
            this.radioSimpleDataAscii.Location = new System.Drawing.Point(6, 87);
            this.radioSimpleDataAscii.Name = "radioSimpleDataAscii";
            this.radioSimpleDataAscii.Size = new System.Drawing.Size(52, 17);
            this.radioSimpleDataAscii.TabIndex = 5;
            this.radioSimpleDataAscii.TabStop = true;
            this.radioSimpleDataAscii.Text = "ASCII";
            this.radioSimpleDataAscii.UseVisualStyleBackColor = true;
            this.radioSimpleDataAscii.CheckedChanged += new System.EventHandler(this.SimpleDisplay_CheckedChanged);
            // 
            // radioDefaultFormat
            // 
            this.radioDefaultFormat.AutoSize = true;
            this.radioDefaultFormat.Location = new System.Drawing.Point(14, 30);
            this.radioDefaultFormat.Name = "radioDefaultFormat";
            this.radioDefaultFormat.Size = new System.Drawing.Size(59, 17);
            this.radioDefaultFormat.TabIndex = 1;
            this.radioDefaultFormat.TabStop = true;
            this.radioDefaultFormat.Text = "&Default";
            this.radioDefaultFormat.UseVisualStyleBackColor = true;
            this.radioDefaultFormat.CheckedChanged += new System.EventHandler(this.MainGroup_CheckedChanged);
            // 
            // simpleDisplayAsGroupBox
            // 
            this.simpleDisplayAsGroupBox.Controls.Add(this.radioSimpleDataAddress);
            this.simpleDisplayAsGroupBox.Controls.Add(this.radioSimpleDataSymbolic);
            this.simpleDisplayAsGroupBox.Controls.Add(this.radioSimpleDataAscii);
            this.simpleDisplayAsGroupBox.Controls.Add(this.radioSimpleDataHex);
            this.simpleDisplayAsGroupBox.Controls.Add(this.symbolPartPanel);
            this.simpleDisplayAsGroupBox.Controls.Add(this.radioSimpleDataBinary);
            this.simpleDisplayAsGroupBox.Controls.Add(this.symbolEntryTextBox);
            this.simpleDisplayAsGroupBox.Controls.Add(this.radioSimpleDataDecimal);
            this.simpleDisplayAsGroupBox.Location = new System.Drawing.Point(190, 84);
            this.simpleDisplayAsGroupBox.Name = "simpleDisplayAsGroupBox";
            this.simpleDisplayAsGroupBox.Size = new System.Drawing.Size(320, 114);
            this.simpleDisplayAsGroupBox.TabIndex = 24;
            this.simpleDisplayAsGroupBox.TabStop = false;
            this.simpleDisplayAsGroupBox.Text = "Display As...";
            // 
            // radioSimpleDataAddress
            // 
            this.radioSimpleDataAddress.AutoSize = true;
            this.radioSimpleDataAddress.Location = new System.Drawing.Point(89, 18);
            this.radioSimpleDataAddress.Name = "radioSimpleDataAddress";
            this.radioSimpleDataAddress.Size = new System.Drawing.Size(63, 17);
            this.radioSimpleDataAddress.TabIndex = 6;
            this.radioSimpleDataAddress.TabStop = true;
            this.radioSimpleDataAddress.Text = "&Address";
            this.radioSimpleDataAddress.UseVisualStyleBackColor = true;
            // 
            // radioSimpleDataSymbolic
            // 
            this.radioSimpleDataSymbolic.AutoSize = true;
            this.radioSimpleDataSymbolic.Location = new System.Drawing.Point(89, 41);
            this.radioSimpleDataSymbolic.Name = "radioSimpleDataSymbolic";
            this.radioSimpleDataSymbolic.Size = new System.Drawing.Size(115, 17);
            this.radioSimpleDataSymbolic.TabIndex = 7;
            this.radioSimpleDataSymbolic.TabStop = true;
            this.radioSimpleDataSymbolic.Text = "&Symbolic reference";
            this.radioSimpleDataSymbolic.UseVisualStyleBackColor = true;
            this.radioSimpleDataSymbolic.CheckedChanged += new System.EventHandler(this.SimpleDisplay_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 221);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Bulk Data";
            // 
            // EditData
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(525, 496);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.simpleDisplayAsGroupBox);
            this.Controls.Add(this.radioDefaultFormat);
            this.Controls.Add(this.radioStringDci);
            this.Controls.Add(this.radioStringLen16);
            this.Controls.Add(this.radioStringLen8);
            this.Controls.Add(this.radioStringNullTerm);
            this.Controls.Add(this.radioStringMixedReverse);
            this.Controls.Add(this.radioStringMixed);
            this.Controls.Add(this.stringSectionLabel);
            this.Controls.Add(this.horizontalLine1);
            this.Controls.Add(this.horizontalLine2);
            this.Controls.Add(this.horizontalLine3);
            this.Controls.Add(this.radioFill);
            this.Controls.Add(this.radioDenseHex);
            this.Controls.Add(this.radio32BitLittle);
            this.Controls.Add(this.radio24BitLittle);
            this.Controls.Add(this.radio16BitLittle);
            this.Controls.Add(this.radio16BitBig);
            this.Controls.Add(this.radioSingleBytes);
            this.Controls.Add(this.rawDataSectionLabel);
            this.Controls.Add(this.selectFormatLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditData";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Data Format";
            this.Load += new System.EventHandler(this.EditData_Load);
            this.Shown += new System.EventHandler(this.EditData_Shown);
            this.symbolPartPanel.ResumeLayout(false);
            this.symbolPartPanel.PerformLayout();
            this.simpleDisplayAsGroupBox.ResumeLayout(false);
            this.simpleDisplayAsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label selectFormatLabel;
        private System.Windows.Forms.Label rawDataSectionLabel;
        private System.Windows.Forms.RadioButton radioSingleBytes;
        private System.Windows.Forms.RadioButton radio16BitBig;
        private System.Windows.Forms.RadioButton radio16BitLittle;
        private System.Windows.Forms.RadioButton radio24BitLittle;
        private System.Windows.Forms.RadioButton radio32BitLittle;
        private System.Windows.Forms.RadioButton radioSimpleDataBinary;
        private System.Windows.Forms.RadioButton radioSimpleDataDecimal;
        private System.Windows.Forms.RadioButton radioSimpleDataHex;
        private System.Windows.Forms.RadioButton radioDenseHex;
        private System.Windows.Forms.RadioButton radioFill;
        private System.Windows.Forms.Label horizontalLine1;
        private System.Windows.Forms.Label horizontalLine2;
        private System.Windows.Forms.Label stringSectionLabel;
        private System.Windows.Forms.Label horizontalLine3;
        private System.Windows.Forms.RadioButton radioStringMixed;
        private System.Windows.Forms.RadioButton radioStringMixedReverse;
        private System.Windows.Forms.RadioButton radioStringNullTerm;
        private System.Windows.Forms.RadioButton radioStringLen8;
        private System.Windows.Forms.RadioButton radioStringLen16;
        private System.Windows.Forms.RadioButton radioStringDci;
        private System.Windows.Forms.TextBox symbolEntryTextBox;
        private System.Windows.Forms.Panel symbolPartPanel;
        private System.Windows.Forms.RadioButton radioSymbolPartBank;
        private System.Windows.Forms.RadioButton radioSymbolPartHigh;
        private System.Windows.Forms.RadioButton radioSymbolPartLow;
        private System.Windows.Forms.RadioButton radioSimpleDataAscii;
        private System.Windows.Forms.RadioButton radioDefaultFormat;
        private System.Windows.Forms.GroupBox simpleDisplayAsGroupBox;
        private System.Windows.Forms.RadioButton radioSimpleDataSymbolic;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioSimpleDataAddress;
    }
}