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
    partial class EditAppSettings {
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
            this.applyButton = new System.Windows.Forms.Button();
            this.settingsTabControl = new System.Windows.Forms.TabControl();
            this.codeViewTabPage = new System.Windows.Forms.TabPage();
            this.clipboardGroupBox = new System.Windows.Forms.GroupBox();
            this.clipboardFormatLabel = new System.Windows.Forms.Label();
            this.clipboardFormatComboBox = new System.Windows.Forms.ComboBox();
            this.enableDebugCheckBox = new System.Windows.Forms.CheckBox();
            this.upperCaseGroupBox = new System.Windows.Forms.GroupBox();
            this.upperAllUpperButton = new System.Windows.Forms.Button();
            this.upperAllLowerButton = new System.Windows.Forms.Button();
            this.upperXYCheckBox = new System.Windows.Forms.CheckBox();
            this.upperSCheckBox = new System.Windows.Forms.CheckBox();
            this.upperACheckBox = new System.Windows.Forms.CheckBox();
            this.upperPseudoOpCheckBox = new System.Windows.Forms.CheckBox();
            this.upperOpcodeCheckBox = new System.Windows.Forms.CheckBox();
            this.upperHexCheckBox = new System.Windows.Forms.CheckBox();
            this.codeViewFontGroupBox = new System.Windows.Forms.GroupBox();
            this.currentFontLabel = new System.Windows.Forms.Label();
            this.selectFontButton = new System.Windows.Forms.Button();
            this.currentFontDisplayLabel = new System.Windows.Forms.Label();
            this.columnVisGroup = new System.Windows.Forms.GroupBox();
            this.showCol0 = new System.Windows.Forms.Button();
            this.showCol8 = new System.Windows.Forms.Button();
            this.showCol1 = new System.Windows.Forms.Button();
            this.showCol7 = new System.Windows.Forms.Button();
            this.showCol2 = new System.Windows.Forms.Button();
            this.showCol6 = new System.Windows.Forms.Button();
            this.showCol3 = new System.Windows.Forms.Button();
            this.showCol5 = new System.Windows.Forms.Button();
            this.showCol4 = new System.Windows.Forms.Button();
            this.asmConfigTabPage = new System.Windows.Forms.TabPage();
            this.configureAsmGroupBox = new System.Windows.Forms.GroupBox();
            this.asmColWidthIdLabel = new System.Windows.Forms.Label();
            this.asmConfigAssemblerLabel = new System.Windows.Forms.Label();
            this.asmCommentColWidthTextBox = new System.Windows.Forms.TextBox();
            this.asmOperandColWidthTextBox = new System.Windows.Forms.TextBox();
            this.asmOpcodeColWidthTextBox = new System.Windows.Forms.TextBox();
            this.asmLabelColWidthTextBox = new System.Windows.Forms.TextBox();
            this.asmColWidthLabel = new System.Windows.Forms.Label();
            this.asmExeLabel = new System.Windows.Forms.Label();
            this.asmConfigComboBox = new System.Windows.Forms.ComboBox();
            this.asmExePathTextBox = new System.Windows.Forms.TextBox();
            this.asmExeBrowseButton = new System.Windows.Forms.Button();
            this.showCycleCountsCheckBox = new System.Windows.Forms.CheckBox();
            this.configAsmGenLabel = new System.Windows.Forms.Label();
            this.longLabelNewLineCheckBox = new System.Windows.Forms.CheckBox();
            this.showAsmIdentCheckBox = new System.Windows.Forms.CheckBox();
            this.disableLabelLocalizationCheckBox = new System.Windows.Forms.CheckBox();
            this.displayFormatTabPage = new System.Windows.Forms.TabPage();
            this.fmtExplanationLabel = new System.Windows.Forms.Label();
            this.quickDisplayFormatGroup = new System.Windows.Forms.GroupBox();
            this.quickFmtMerlin32Button = new System.Windows.Forms.Button();
            this.quickFmtCc65Button = new System.Windows.Forms.Button();
            this.quickFmtDefaultButton = new System.Windows.Forms.Button();
            this.useMerlinExpressions = new System.Windows.Forms.CheckBox();
            this.operandWidthGroupBox = new System.Windows.Forms.GroupBox();
            this.disambPrefix24TextBox = new System.Windows.Forms.TextBox();
            this.disambPrefix16TextBox = new System.Windows.Forms.TextBox();
            this.disambPrefix24Label = new System.Windows.Forms.Label();
            this.disambPrefix16Label = new System.Windows.Forms.Label();
            this.disambOperandPrefixLabel = new System.Windows.Forms.Label();
            this.disambSuffix24Label = new System.Windows.Forms.Label();
            this.disambSuffix16Label = new System.Windows.Forms.Label();
            this.disambOpcodeSuffixLabel = new System.Windows.Forms.Label();
            this.disambSuffix24TextBox = new System.Windows.Forms.TextBox();
            this.disambSuffix16TextBox = new System.Windows.Forms.TextBox();
            this.pseudoOpTabPage = new System.Windows.Forms.TabPage();
            this.quickPseudoSetGroup = new System.Windows.Forms.GroupBox();
            this.quickPseudoMerlin32 = new System.Windows.Forms.Button();
            this.quickPseudoCc65Button = new System.Windows.Forms.Button();
            this.quickPseudoDefaultButton = new System.Windows.Forms.Button();
            this.strDciHiTextBox = new System.Windows.Forms.TextBox();
            this.strDciHiLabel = new System.Windows.Forms.Label();
            this.strDciTextBox = new System.Windows.Forms.TextBox();
            this.strDciLabel = new System.Windows.Forms.Label();
            this.strLen16HiTextBox = new System.Windows.Forms.TextBox();
            this.strLen16HiLabel = new System.Windows.Forms.Label();
            this.strLen16TextBox = new System.Windows.Forms.TextBox();
            this.strLen16Label = new System.Windows.Forms.Label();
            this.strReverseHiTextBox = new System.Windows.Forms.TextBox();
            this.strReverseHiLabel = new System.Windows.Forms.Label();
            this.strReverseTextBox = new System.Windows.Forms.TextBox();
            this.strReverseLabel = new System.Windows.Forms.Label();
            this.strNullTermHiTextBox = new System.Windows.Forms.TextBox();
            this.strNullTermHiLabel = new System.Windows.Forms.Label();
            this.strNullTermTextBox = new System.Windows.Forms.TextBox();
            this.strNullTermLabel = new System.Windows.Forms.Label();
            this.strLen8HiTextBox = new System.Windows.Forms.TextBox();
            this.strLen8HiLabel = new System.Windows.Forms.Label();
            this.strLen8TextBox = new System.Windows.Forms.TextBox();
            this.strLen8Label = new System.Windows.Forms.Label();
            this.strGenericHiTextBox = new System.Windows.Forms.TextBox();
            this.strGenericHiLabel = new System.Windows.Forms.Label();
            this.strGenericTextBox = new System.Windows.Forms.TextBox();
            this.strGenericLabel = new System.Windows.Forms.Label();
            this.popExplanationLabel = new System.Windows.Forms.Label();
            this.denseTextBox = new System.Windows.Forms.TextBox();
            this.denseLabel = new System.Windows.Forms.Label();
            this.fillTextBox = new System.Windows.Forms.TextBox();
            this.fillLabel = new System.Windows.Forms.Label();
            this.defineBigData2TextBox = new System.Windows.Forms.TextBox();
            this.defineBigData2Label = new System.Windows.Forms.Label();
            this.defineData4TextBox = new System.Windows.Forms.TextBox();
            this.defineData4Label = new System.Windows.Forms.Label();
            this.defineData3TextBox = new System.Windows.Forms.TextBox();
            this.defineData3Label = new System.Windows.Forms.Label();
            this.defineData2TextBox = new System.Windows.Forms.TextBox();
            this.defineData2Label = new System.Windows.Forms.Label();
            this.regWidthDirectiveTextBox = new System.Windows.Forms.TextBox();
            this.regWidthDirectiveLabel = new System.Windows.Forms.Label();
            this.orgDirectiveTextBox = new System.Windows.Forms.TextBox();
            this.orgDirectiveLabel = new System.Windows.Forms.Label();
            this.defineData1TextBox = new System.Windows.Forms.TextBox();
            this.defineData1Label = new System.Windows.Forms.Label();
            this.equDirectiveTextBox = new System.Windows.Forms.TextBox();
            this.equDirectiveLabel = new System.Windows.Forms.Label();
            this.settingsTabControl.SuspendLayout();
            this.codeViewTabPage.SuspendLayout();
            this.clipboardGroupBox.SuspendLayout();
            this.upperCaseGroupBox.SuspendLayout();
            this.codeViewFontGroupBox.SuspendLayout();
            this.columnVisGroup.SuspendLayout();
            this.asmConfigTabPage.SuspendLayout();
            this.configureAsmGroupBox.SuspendLayout();
            this.displayFormatTabPage.SuspendLayout();
            this.quickDisplayFormatGroup.SuspendLayout();
            this.operandWidthGroupBox.SuspendLayout();
            this.pseudoOpTabPage.SuspendLayout();
            this.quickPseudoSetGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(537, 406);
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
            this.okButton.Location = new System.Drawing.Point(456, 406);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(354, 406);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 1;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // settingsTabControl
            // 
            this.settingsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.settingsTabControl.Controls.Add(this.codeViewTabPage);
            this.settingsTabControl.Controls.Add(this.asmConfigTabPage);
            this.settingsTabControl.Controls.Add(this.displayFormatTabPage);
            this.settingsTabControl.Controls.Add(this.pseudoOpTabPage);
            this.settingsTabControl.Location = new System.Drawing.Point(2, 2);
            this.settingsTabControl.Name = "settingsTabControl";
            this.settingsTabControl.SelectedIndex = 0;
            this.settingsTabControl.Size = new System.Drawing.Size(622, 398);
            this.settingsTabControl.TabIndex = 0;
            // 
            // codeViewTabPage
            // 
            this.codeViewTabPage.Controls.Add(this.clipboardGroupBox);
            this.codeViewTabPage.Controls.Add(this.enableDebugCheckBox);
            this.codeViewTabPage.Controls.Add(this.upperCaseGroupBox);
            this.codeViewTabPage.Controls.Add(this.codeViewFontGroupBox);
            this.codeViewTabPage.Controls.Add(this.columnVisGroup);
            this.codeViewTabPage.Location = new System.Drawing.Point(4, 22);
            this.codeViewTabPage.Name = "codeViewTabPage";
            this.codeViewTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.codeViewTabPage.Size = new System.Drawing.Size(614, 372);
            this.codeViewTabPage.TabIndex = 0;
            this.codeViewTabPage.Text = "Code View";
            this.codeViewTabPage.UseVisualStyleBackColor = true;
            // 
            // clipboardGroupBox
            // 
            this.clipboardGroupBox.Controls.Add(this.clipboardFormatLabel);
            this.clipboardGroupBox.Controls.Add(this.clipboardFormatComboBox);
            this.clipboardGroupBox.Location = new System.Drawing.Point(406, 6);
            this.clipboardGroupBox.Name = "clipboardGroupBox";
            this.clipboardGroupBox.Size = new System.Drawing.Size(200, 89);
            this.clipboardGroupBox.TabIndex = 4;
            this.clipboardGroupBox.TabStop = false;
            this.clipboardGroupBox.Text = "Clipboard";
            // 
            // clipboardFormatLabel
            // 
            this.clipboardFormatLabel.AutoSize = true;
            this.clipboardFormatLabel.Location = new System.Drawing.Point(6, 22);
            this.clipboardFormatLabel.Name = "clipboardFormatLabel";
            this.clipboardFormatLabel.Size = new System.Drawing.Size(174, 13);
            this.clipboardFormatLabel.TabIndex = 1;
            this.clipboardFormatLabel.Text = "Format for lines copied to clipboard:";
            // 
            // clipboardFormatComboBox
            // 
            this.clipboardFormatComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.clipboardFormatComboBox.FormattingEnabled = true;
            this.clipboardFormatComboBox.Items.AddRange(new object[] {
            "Assembler Source",
            "Disassembly"});
            this.clipboardFormatComboBox.Location = new System.Drawing.Point(6, 49);
            this.clipboardFormatComboBox.Name = "clipboardFormatComboBox";
            this.clipboardFormatComboBox.Size = new System.Drawing.Size(188, 21);
            this.clipboardFormatComboBox.TabIndex = 0;
            this.clipboardFormatComboBox.SelectedIndexChanged += new System.EventHandler(this.clipboardFormatComboBox_SelectedIndexChanged);
            // 
            // enableDebugCheckBox
            // 
            this.enableDebugCheckBox.AutoSize = true;
            this.enableDebugCheckBox.Location = new System.Drawing.Point(12, 349);
            this.enableDebugCheckBox.Name = "enableDebugCheckBox";
            this.enableDebugCheckBox.Size = new System.Drawing.Size(129, 17);
            this.enableDebugCheckBox.TabIndex = 3;
            this.enableDebugCheckBox.Text = "Enable DEBUG menu";
            this.enableDebugCheckBox.UseVisualStyleBackColor = true;
            this.enableDebugCheckBox.CheckedChanged += new System.EventHandler(this.enableDebugCheckBox_CheckedChanged);
            // 
            // upperCaseGroupBox
            // 
            this.upperCaseGroupBox.Controls.Add(this.upperAllUpperButton);
            this.upperCaseGroupBox.Controls.Add(this.upperAllLowerButton);
            this.upperCaseGroupBox.Controls.Add(this.upperXYCheckBox);
            this.upperCaseGroupBox.Controls.Add(this.upperSCheckBox);
            this.upperCaseGroupBox.Controls.Add(this.upperACheckBox);
            this.upperCaseGroupBox.Controls.Add(this.upperPseudoOpCheckBox);
            this.upperCaseGroupBox.Controls.Add(this.upperOpcodeCheckBox);
            this.upperCaseGroupBox.Controls.Add(this.upperHexCheckBox);
            this.upperCaseGroupBox.Location = new System.Drawing.Point(162, 101);
            this.upperCaseGroupBox.Name = "upperCaseGroupBox";
            this.upperCaseGroupBox.Size = new System.Drawing.Size(224, 197);
            this.upperCaseGroupBox.TabIndex = 0;
            this.upperCaseGroupBox.TabStop = false;
            this.upperCaseGroupBox.Text = "Upper Case Display";
            // 
            // upperAllUpperButton
            // 
            this.upperAllUpperButton.Location = new System.Drawing.Point(88, 164);
            this.upperAllUpperButton.Name = "upperAllUpperButton";
            this.upperAllUpperButton.Size = new System.Drawing.Size(75, 23);
            this.upperAllUpperButton.TabIndex = 7;
            this.upperAllUpperButton.Text = "All Upper";
            this.upperAllUpperButton.UseVisualStyleBackColor = true;
            this.upperAllUpperButton.Click += new System.EventHandler(this.upperAllUpperButton_Click);
            // 
            // upperAllLowerButton
            // 
            this.upperAllLowerButton.Location = new System.Drawing.Point(7, 164);
            this.upperAllLowerButton.Name = "upperAllLowerButton";
            this.upperAllLowerButton.Size = new System.Drawing.Size(75, 23);
            this.upperAllLowerButton.TabIndex = 6;
            this.upperAllLowerButton.Text = "All Lower";
            this.upperAllLowerButton.UseVisualStyleBackColor = true;
            this.upperAllLowerButton.Click += new System.EventHandler(this.upperAllLowerButton_Click);
            // 
            // upperXYCheckBox
            // 
            this.upperXYCheckBox.AutoSize = true;
            this.upperXYCheckBox.Location = new System.Drawing.Point(8, 139);
            this.upperXYCheckBox.Name = "upperXYCheckBox";
            this.upperXYCheckBox.Size = new System.Drawing.Size(89, 17);
            this.upperXYCheckBox.TabIndex = 5;
            this.upperXYCheckBox.Text = "Operand X/Y";
            this.upperXYCheckBox.UseVisualStyleBackColor = true;
            this.upperXYCheckBox.CheckedChanged += new System.EventHandler(this.upperXYCheckBox_CheckedChanged);
            // 
            // upperSCheckBox
            // 
            this.upperSCheckBox.AutoSize = true;
            this.upperSCheckBox.Location = new System.Drawing.Point(8, 115);
            this.upperSCheckBox.Name = "upperSCheckBox";
            this.upperSCheckBox.Size = new System.Drawing.Size(77, 17);
            this.upperSCheckBox.TabIndex = 4;
            this.upperSCheckBox.Text = "Operand S";
            this.upperSCheckBox.UseVisualStyleBackColor = true;
            this.upperSCheckBox.CheckedChanged += new System.EventHandler(this.upperSCheckBox_CheckedChanged);
            // 
            // upperACheckBox
            // 
            this.upperACheckBox.AutoSize = true;
            this.upperACheckBox.Location = new System.Drawing.Point(8, 91);
            this.upperACheckBox.Name = "upperACheckBox";
            this.upperACheckBox.Size = new System.Drawing.Size(77, 17);
            this.upperACheckBox.TabIndex = 3;
            this.upperACheckBox.Text = "Operand A";
            this.upperACheckBox.UseVisualStyleBackColor = true;
            this.upperACheckBox.CheckedChanged += new System.EventHandler(this.upperACheckBox_CheckedChanged);
            // 
            // upperPseudoOpCheckBox
            // 
            this.upperPseudoOpCheckBox.AutoSize = true;
            this.upperPseudoOpCheckBox.Location = new System.Drawing.Point(8, 67);
            this.upperPseudoOpCheckBox.Name = "upperPseudoOpCheckBox";
            this.upperPseudoOpCheckBox.Size = new System.Drawing.Size(106, 17);
            this.upperPseudoOpCheckBox.TabIndex = 2;
            this.upperPseudoOpCheckBox.Text = "Pseudo-opcodes";
            this.upperPseudoOpCheckBox.UseVisualStyleBackColor = true;
            this.upperPseudoOpCheckBox.CheckedChanged += new System.EventHandler(this.upperPseudoOpCheckBox_CheckedChanged);
            // 
            // upperOpcodeCheckBox
            // 
            this.upperOpcodeCheckBox.AutoSize = true;
            this.upperOpcodeCheckBox.Location = new System.Drawing.Point(8, 44);
            this.upperOpcodeCheckBox.Name = "upperOpcodeCheckBox";
            this.upperOpcodeCheckBox.Size = new System.Drawing.Size(69, 17);
            this.upperOpcodeCheckBox.TabIndex = 1;
            this.upperOpcodeCheckBox.Text = "Opcodes";
            this.upperOpcodeCheckBox.UseVisualStyleBackColor = true;
            this.upperOpcodeCheckBox.CheckedChanged += new System.EventHandler(this.upperOpcodeCheckBox_CheckedChanged);
            // 
            // upperHexCheckBox
            // 
            this.upperHexCheckBox.AutoSize = true;
            this.upperHexCheckBox.Location = new System.Drawing.Point(8, 20);
            this.upperHexCheckBox.Name = "upperHexCheckBox";
            this.upperHexCheckBox.Size = new System.Drawing.Size(121, 17);
            this.upperHexCheckBox.TabIndex = 0;
            this.upperHexCheckBox.Text = "Hexadecimal values";
            this.upperHexCheckBox.UseVisualStyleBackColor = true;
            this.upperHexCheckBox.CheckedChanged += new System.EventHandler(this.upperHexCheckBox_CheckedChanged);
            // 
            // codeViewFontGroupBox
            // 
            this.codeViewFontGroupBox.Controls.Add(this.currentFontLabel);
            this.codeViewFontGroupBox.Controls.Add(this.selectFontButton);
            this.codeViewFontGroupBox.Controls.Add(this.currentFontDisplayLabel);
            this.codeViewFontGroupBox.Location = new System.Drawing.Point(162, 6);
            this.codeViewFontGroupBox.Name = "codeViewFontGroupBox";
            this.codeViewFontGroupBox.Size = new System.Drawing.Size(224, 89);
            this.codeViewFontGroupBox.TabIndex = 2;
            this.codeViewFontGroupBox.TabStop = false;
            this.codeViewFontGroupBox.Text = "Code List Font";
            // 
            // currentFontLabel
            // 
            this.currentFontLabel.AutoSize = true;
            this.currentFontLabel.Location = new System.Drawing.Point(7, 18);
            this.currentFontLabel.Name = "currentFontLabel";
            this.currentFontLabel.Size = new System.Drawing.Size(65, 13);
            this.currentFontLabel.TabIndex = 0;
            this.currentFontLabel.Text = "Current font:";
            // 
            // selectFontButton
            // 
            this.selectFontButton.Location = new System.Drawing.Point(6, 60);
            this.selectFontButton.Name = "selectFontButton";
            this.selectFontButton.Size = new System.Drawing.Size(125, 23);
            this.selectFontButton.TabIndex = 2;
            this.selectFontButton.Text = "Select Font...";
            this.selectFontButton.UseVisualStyleBackColor = true;
            this.selectFontButton.Click += new System.EventHandler(this.selectFontButton_Click);
            // 
            // currentFontDisplayLabel
            // 
            this.currentFontDisplayLabel.AutoSize = true;
            this.currentFontDisplayLabel.Location = new System.Drawing.Point(7, 33);
            this.currentFontDisplayLabel.Name = "currentFontDisplayLabel";
            this.currentFontDisplayLabel.Size = new System.Drawing.Size(181, 13);
            this.currentFontDisplayLabel.TabIndex = 1;
            this.currentFontDisplayLabel.Text = "Constantia, 14.25pt, style=Bold, Italic";
            // 
            // columnVisGroup
            // 
            this.columnVisGroup.Controls.Add(this.showCol0);
            this.columnVisGroup.Controls.Add(this.showCol8);
            this.columnVisGroup.Controls.Add(this.showCol1);
            this.columnVisGroup.Controls.Add(this.showCol7);
            this.columnVisGroup.Controls.Add(this.showCol2);
            this.columnVisGroup.Controls.Add(this.showCol6);
            this.columnVisGroup.Controls.Add(this.showCol3);
            this.columnVisGroup.Controls.Add(this.showCol5);
            this.columnVisGroup.Controls.Add(this.showCol4);
            this.columnVisGroup.Location = new System.Drawing.Point(6, 6);
            this.columnVisGroup.Name = "columnVisGroup";
            this.columnVisGroup.Size = new System.Drawing.Size(122, 292);
            this.columnVisGroup.TabIndex = 0;
            this.columnVisGroup.TabStop = false;
            this.columnVisGroup.Text = "Column Visibility";
            // 
            // showCol0
            // 
            this.showCol0.Location = new System.Drawing.Point(6, 19);
            this.showCol0.Name = "showCol0";
            this.showCol0.Size = new System.Drawing.Size(110, 23);
            this.showCol0.TabIndex = 0;
            this.showCol0.Text = "{0} Offset";
            this.showCol0.UseVisualStyleBackColor = true;
            // 
            // showCol8
            // 
            this.showCol8.Location = new System.Drawing.Point(6, 259);
            this.showCol8.Name = "showCol8";
            this.showCol8.Size = new System.Drawing.Size(110, 23);
            this.showCol8.TabIndex = 8;
            this.showCol8.Text = "{0} Comment";
            this.showCol8.UseVisualStyleBackColor = true;
            // 
            // showCol1
            // 
            this.showCol1.Location = new System.Drawing.Point(6, 49);
            this.showCol1.Name = "showCol1";
            this.showCol1.Size = new System.Drawing.Size(110, 23);
            this.showCol1.TabIndex = 1;
            this.showCol1.Text = "{0} Address";
            this.showCol1.UseVisualStyleBackColor = true;
            // 
            // showCol7
            // 
            this.showCol7.Location = new System.Drawing.Point(6, 229);
            this.showCol7.Name = "showCol7";
            this.showCol7.Size = new System.Drawing.Size(110, 23);
            this.showCol7.TabIndex = 7;
            this.showCol7.Text = "{0} Operand";
            this.showCol7.UseVisualStyleBackColor = true;
            // 
            // showCol2
            // 
            this.showCol2.Location = new System.Drawing.Point(6, 79);
            this.showCol2.Name = "showCol2";
            this.showCol2.Size = new System.Drawing.Size(110, 23);
            this.showCol2.TabIndex = 2;
            this.showCol2.Text = "{0} Bytes";
            this.showCol2.UseVisualStyleBackColor = true;
            // 
            // showCol6
            // 
            this.showCol6.Location = new System.Drawing.Point(6, 199);
            this.showCol6.Name = "showCol6";
            this.showCol6.Size = new System.Drawing.Size(110, 23);
            this.showCol6.TabIndex = 6;
            this.showCol6.Text = "{0} Opcode";
            this.showCol6.UseVisualStyleBackColor = true;
            // 
            // showCol3
            // 
            this.showCol3.Location = new System.Drawing.Point(6, 109);
            this.showCol3.Name = "showCol3";
            this.showCol3.Size = new System.Drawing.Size(110, 23);
            this.showCol3.TabIndex = 3;
            this.showCol3.Text = "{0} Flags";
            this.showCol3.UseVisualStyleBackColor = true;
            // 
            // showCol5
            // 
            this.showCol5.Location = new System.Drawing.Point(6, 169);
            this.showCol5.Name = "showCol5";
            this.showCol5.Size = new System.Drawing.Size(110, 23);
            this.showCol5.TabIndex = 5;
            this.showCol5.Text = "{0} Label";
            this.showCol5.UseVisualStyleBackColor = true;
            // 
            // showCol4
            // 
            this.showCol4.Location = new System.Drawing.Point(6, 139);
            this.showCol4.Name = "showCol4";
            this.showCol4.Size = new System.Drawing.Size(110, 23);
            this.showCol4.TabIndex = 4;
            this.showCol4.Text = "{0} Attributes";
            this.showCol4.UseVisualStyleBackColor = true;
            // 
            // asmConfigTabPage
            // 
            this.asmConfigTabPage.Controls.Add(this.configureAsmGroupBox);
            this.asmConfigTabPage.Controls.Add(this.showCycleCountsCheckBox);
            this.asmConfigTabPage.Controls.Add(this.configAsmGenLabel);
            this.asmConfigTabPage.Controls.Add(this.longLabelNewLineCheckBox);
            this.asmConfigTabPage.Controls.Add(this.showAsmIdentCheckBox);
            this.asmConfigTabPage.Controls.Add(this.disableLabelLocalizationCheckBox);
            this.asmConfigTabPage.Location = new System.Drawing.Point(4, 22);
            this.asmConfigTabPage.Name = "asmConfigTabPage";
            this.asmConfigTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.asmConfigTabPage.Size = new System.Drawing.Size(614, 372);
            this.asmConfigTabPage.TabIndex = 1;
            this.asmConfigTabPage.Text = "Asm Config";
            this.asmConfigTabPage.UseVisualStyleBackColor = true;
            // 
            // configureAsmGroupBox
            // 
            this.configureAsmGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.configureAsmGroupBox.Controls.Add(this.asmColWidthIdLabel);
            this.configureAsmGroupBox.Controls.Add(this.asmConfigAssemblerLabel);
            this.configureAsmGroupBox.Controls.Add(this.asmCommentColWidthTextBox);
            this.configureAsmGroupBox.Controls.Add(this.asmOperandColWidthTextBox);
            this.configureAsmGroupBox.Controls.Add(this.asmOpcodeColWidthTextBox);
            this.configureAsmGroupBox.Controls.Add(this.asmLabelColWidthTextBox);
            this.configureAsmGroupBox.Controls.Add(this.asmColWidthLabel);
            this.configureAsmGroupBox.Controls.Add(this.asmExeLabel);
            this.configureAsmGroupBox.Controls.Add(this.asmConfigComboBox);
            this.configureAsmGroupBox.Controls.Add(this.asmExePathTextBox);
            this.configureAsmGroupBox.Controls.Add(this.asmExeBrowseButton);
            this.configureAsmGroupBox.Location = new System.Drawing.Point(6, 6);
            this.configureAsmGroupBox.Name = "configureAsmGroupBox";
            this.configureAsmGroupBox.Size = new System.Drawing.Size(602, 121);
            this.configureAsmGroupBox.TabIndex = 0;
            this.configureAsmGroupBox.TabStop = false;
            this.configureAsmGroupBox.Text = "Assembler Configuration";
            // 
            // asmColWidthIdLabel
            // 
            this.asmColWidthIdLabel.AutoSize = true;
            this.asmColWidthIdLabel.Location = new System.Drawing.Point(371, 88);
            this.asmColWidthIdLabel.Name = "asmColWidthIdLabel";
            this.asmColWidthIdLabel.Size = new System.Drawing.Size(171, 13);
            this.asmColWidthIdLabel.TabIndex = 10;
            this.asmColWidthIdLabel.Text = "(label, opcode, operand, comment)";
            // 
            // asmConfigAssemblerLabel
            // 
            this.asmConfigAssemblerLabel.AutoSize = true;
            this.asmConfigAssemblerLabel.Location = new System.Drawing.Point(27, 22);
            this.asmConfigAssemblerLabel.Name = "asmConfigAssemblerLabel";
            this.asmConfigAssemblerLabel.Size = new System.Drawing.Size(58, 13);
            this.asmConfigAssemblerLabel.TabIndex = 0;
            this.asmConfigAssemblerLabel.Text = "Assembler:";
            // 
            // asmCommentColWidthTextBox
            // 
            this.asmCommentColWidthTextBox.Location = new System.Drawing.Point(301, 85);
            this.asmCommentColWidthTextBox.Name = "asmCommentColWidthTextBox";
            this.asmCommentColWidthTextBox.Size = new System.Drawing.Size(64, 20);
            this.asmCommentColWidthTextBox.TabIndex = 9;
            this.asmCommentColWidthTextBox.TextChanged += new System.EventHandler(this.AsmConfig_TextChanged);
            // 
            // asmOperandColWidthTextBox
            // 
            this.asmOperandColWidthTextBox.Location = new System.Drawing.Point(231, 85);
            this.asmOperandColWidthTextBox.Name = "asmOperandColWidthTextBox";
            this.asmOperandColWidthTextBox.Size = new System.Drawing.Size(64, 20);
            this.asmOperandColWidthTextBox.TabIndex = 8;
            this.asmOperandColWidthTextBox.TextChanged += new System.EventHandler(this.AsmConfig_TextChanged);
            // 
            // asmOpcodeColWidthTextBox
            // 
            this.asmOpcodeColWidthTextBox.Location = new System.Drawing.Point(161, 85);
            this.asmOpcodeColWidthTextBox.Name = "asmOpcodeColWidthTextBox";
            this.asmOpcodeColWidthTextBox.Size = new System.Drawing.Size(64, 20);
            this.asmOpcodeColWidthTextBox.TabIndex = 7;
            this.asmOpcodeColWidthTextBox.TextChanged += new System.EventHandler(this.AsmConfig_TextChanged);
            // 
            // asmLabelColWidthTextBox
            // 
            this.asmLabelColWidthTextBox.Location = new System.Drawing.Point(91, 85);
            this.asmLabelColWidthTextBox.Name = "asmLabelColWidthTextBox";
            this.asmLabelColWidthTextBox.Size = new System.Drawing.Size(64, 20);
            this.asmLabelColWidthTextBox.TabIndex = 6;
            this.asmLabelColWidthTextBox.TextChanged += new System.EventHandler(this.AsmConfig_TextChanged);
            // 
            // asmColWidthLabel
            // 
            this.asmColWidthLabel.AutoSize = true;
            this.asmColWidthLabel.Location = new System.Drawing.Point(7, 88);
            this.asmColWidthLabel.Name = "asmColWidthLabel";
            this.asmColWidthLabel.Size = new System.Drawing.Size(78, 13);
            this.asmColWidthLabel.TabIndex = 5;
            this.asmColWidthLabel.Text = "Column widths:";
            // 
            // asmExeLabel
            // 
            this.asmExeLabel.AutoSize = true;
            this.asmExeLabel.Location = new System.Drawing.Point(22, 61);
            this.asmExeLabel.Name = "asmExeLabel";
            this.asmExeLabel.Size = new System.Drawing.Size(63, 13);
            this.asmExeLabel.TabIndex = 2;
            this.asmExeLabel.Text = "Executable:";
            // 
            // asmConfigComboBox
            // 
            this.asmConfigComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.asmConfigComboBox.FormattingEnabled = true;
            this.asmConfigComboBox.Items.AddRange(new object[] {
            "cc65",
            "Merlin32"});
            this.asmConfigComboBox.Location = new System.Drawing.Point(91, 19);
            this.asmConfigComboBox.Name = "asmConfigComboBox";
            this.asmConfigComboBox.Size = new System.Drawing.Size(154, 21);
            this.asmConfigComboBox.TabIndex = 1;
            this.asmConfigComboBox.SelectedIndexChanged += new System.EventHandler(this.asmConfigComboBox_SelectedIndexChanged);
            // 
            // asmExePathTextBox
            // 
            this.asmExePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.asmExePathTextBox.Location = new System.Drawing.Point(91, 58);
            this.asmExePathTextBox.Name = "asmExePathTextBox";
            this.asmExePathTextBox.Size = new System.Drawing.Size(424, 20);
            this.asmExePathTextBox.TabIndex = 3;
            this.asmExePathTextBox.Text = "C:\\something";
            this.asmExePathTextBox.TextChanged += new System.EventHandler(this.AsmConfig_TextChanged);
            // 
            // asmExeBrowseButton
            // 
            this.asmExeBrowseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.asmExeBrowseButton.Location = new System.Drawing.Point(521, 56);
            this.asmExeBrowseButton.Name = "asmExeBrowseButton";
            this.asmExeBrowseButton.Size = new System.Drawing.Size(75, 23);
            this.asmExeBrowseButton.TabIndex = 4;
            this.asmExeBrowseButton.Text = "Browse...";
            this.asmExeBrowseButton.UseVisualStyleBackColor = true;
            this.asmExeBrowseButton.Click += new System.EventHandler(this.asmExeBrowseButton_Click);
            // 
            // showCycleCountsCheckBox
            // 
            this.showCycleCountsCheckBox.AutoSize = true;
            this.showCycleCountsCheckBox.Location = new System.Drawing.Point(6, 162);
            this.showCycleCountsCheckBox.Name = "showCycleCountsCheckBox";
            this.showCycleCountsCheckBox.Size = new System.Drawing.Size(116, 17);
            this.showCycleCountsCheckBox.TabIndex = 2;
            this.showCycleCountsCheckBox.Text = "Show cycle counts";
            this.showCycleCountsCheckBox.UseVisualStyleBackColor = true;
            this.showCycleCountsCheckBox.CheckedChanged += new System.EventHandler(this.showCycleCountsCheckBox_CheckedChanged);
            // 
            // configAsmGenLabel
            // 
            this.configAsmGenLabel.AutoSize = true;
            this.configAsmGenLabel.Location = new System.Drawing.Point(3, 143);
            this.configAsmGenLabel.Name = "configAsmGenLabel";
            this.configAsmGenLabel.Size = new System.Drawing.Size(127, 13);
            this.configAsmGenLabel.TabIndex = 1;
            this.configAsmGenLabel.Text = "General code generation:";
            // 
            // longLabelNewLineCheckBox
            // 
            this.longLabelNewLineCheckBox.AutoSize = true;
            this.longLabelNewLineCheckBox.Location = new System.Drawing.Point(6, 185);
            this.longLabelNewLineCheckBox.Name = "longLabelNewLineCheckBox";
            this.longLabelNewLineCheckBox.Size = new System.Drawing.Size(173, 17);
            this.longLabelNewLineCheckBox.TabIndex = 3;
            this.longLabelNewLineCheckBox.Text = "Put long labels on separate line";
            this.longLabelNewLineCheckBox.UseVisualStyleBackColor = true;
            // 
            // showAsmIdentCheckBox
            // 
            this.showAsmIdentCheckBox.AutoSize = true;
            this.showAsmIdentCheckBox.Location = new System.Drawing.Point(6, 208);
            this.showAsmIdentCheckBox.Name = "showAsmIdentCheckBox";
            this.showAsmIdentCheckBox.Size = new System.Drawing.Size(154, 17);
            this.showAsmIdentCheckBox.TabIndex = 4;
            this.showAsmIdentCheckBox.Text = "Identify assembler in output";
            this.showAsmIdentCheckBox.UseVisualStyleBackColor = true;
            this.showAsmIdentCheckBox.CheckedChanged += new System.EventHandler(this.showAsmIdentCheckBox_CheckedChanged);
            // 
            // disableLabelLocalizationCheckBox
            // 
            this.disableLabelLocalizationCheckBox.AutoSize = true;
            this.disableLabelLocalizationCheckBox.Location = new System.Drawing.Point(6, 231);
            this.disableLabelLocalizationCheckBox.Name = "disableLabelLocalizationCheckBox";
            this.disableLabelLocalizationCheckBox.Size = new System.Drawing.Size(141, 17);
            this.disableLabelLocalizationCheckBox.TabIndex = 5;
            this.disableLabelLocalizationCheckBox.Text = "Disable label localization";
            this.disableLabelLocalizationCheckBox.UseVisualStyleBackColor = true;
            this.disableLabelLocalizationCheckBox.CheckedChanged += new System.EventHandler(this.disableLabelLocalizationCheckBox_CheckedChanged);
            // 
            // displayFormatTabPage
            // 
            this.displayFormatTabPage.Controls.Add(this.fmtExplanationLabel);
            this.displayFormatTabPage.Controls.Add(this.quickDisplayFormatGroup);
            this.displayFormatTabPage.Controls.Add(this.useMerlinExpressions);
            this.displayFormatTabPage.Controls.Add(this.operandWidthGroupBox);
            this.displayFormatTabPage.Location = new System.Drawing.Point(4, 22);
            this.displayFormatTabPage.Name = "displayFormatTabPage";
            this.displayFormatTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.displayFormatTabPage.Size = new System.Drawing.Size(614, 372);
            this.displayFormatTabPage.TabIndex = 3;
            this.displayFormatTabPage.Text = "Display Format";
            this.displayFormatTabPage.UseVisualStyleBackColor = true;
            // 
            // fmtExplanationLabel
            // 
            this.fmtExplanationLabel.AutoSize = true;
            this.fmtExplanationLabel.Location = new System.Drawing.Point(7, 7);
            this.fmtExplanationLabel.Name = "fmtExplanationLabel";
            this.fmtExplanationLabel.Size = new System.Drawing.Size(374, 13);
            this.fmtExplanationLabel.TabIndex = 52;
            this.fmtExplanationLabel.Text = "Configure display format options. This does not affect source code generation.";
            // 
            // quickDisplayFormatGroup
            // 
            this.quickDisplayFormatGroup.Controls.Add(this.quickFmtMerlin32Button);
            this.quickDisplayFormatGroup.Controls.Add(this.quickFmtCc65Button);
            this.quickDisplayFormatGroup.Controls.Add(this.quickFmtDefaultButton);
            this.quickDisplayFormatGroup.Location = new System.Drawing.Point(348, 291);
            this.quickDisplayFormatGroup.Name = "quickDisplayFormatGroup";
            this.quickDisplayFormatGroup.Size = new System.Drawing.Size(258, 75);
            this.quickDisplayFormatGroup.TabIndex = 51;
            this.quickDisplayFormatGroup.TabStop = false;
            this.quickDisplayFormatGroup.Text = "Quick Set";
            // 
            // quickFmtMerlin32Button
            // 
            this.quickFmtMerlin32Button.Location = new System.Drawing.Point(173, 30);
            this.quickFmtMerlin32Button.Name = "quickFmtMerlin32Button";
            this.quickFmtMerlin32Button.Size = new System.Drawing.Size(75, 23);
            this.quickFmtMerlin32Button.TabIndex = 2;
            this.quickFmtMerlin32Button.Text = "Merlin 32";
            this.quickFmtMerlin32Button.UseVisualStyleBackColor = true;
            this.quickFmtMerlin32Button.Click += new System.EventHandler(this.quickFmtMerlin32Button_Click);
            // 
            // quickFmtCc65Button
            // 
            this.quickFmtCc65Button.Location = new System.Drawing.Point(92, 30);
            this.quickFmtCc65Button.Name = "quickFmtCc65Button";
            this.quickFmtCc65Button.Size = new System.Drawing.Size(75, 23);
            this.quickFmtCc65Button.TabIndex = 1;
            this.quickFmtCc65Button.Text = "cc65";
            this.quickFmtCc65Button.UseVisualStyleBackColor = true;
            this.quickFmtCc65Button.Click += new System.EventHandler(this.quickFmtCc65Button_Click);
            // 
            // quickFmtDefaultButton
            // 
            this.quickFmtDefaultButton.Location = new System.Drawing.Point(11, 30);
            this.quickFmtDefaultButton.Name = "quickFmtDefaultButton";
            this.quickFmtDefaultButton.Size = new System.Drawing.Size(75, 23);
            this.quickFmtDefaultButton.TabIndex = 0;
            this.quickFmtDefaultButton.Text = "Default";
            this.quickFmtDefaultButton.UseVisualStyleBackColor = true;
            this.quickFmtDefaultButton.Click += new System.EventHandler(this.quickFmtDefaultButton_Click);
            // 
            // useMerlinExpressions
            // 
            this.useMerlinExpressions.AutoSize = true;
            this.useMerlinExpressions.Location = new System.Drawing.Point(6, 153);
            this.useMerlinExpressions.Name = "useMerlinExpressions";
            this.useMerlinExpressions.Size = new System.Drawing.Size(158, 17);
            this.useMerlinExpressions.TabIndex = 49;
            this.useMerlinExpressions.Text = "Use Merlin-style expressions";
            this.useMerlinExpressions.UseVisualStyleBackColor = true;
            this.useMerlinExpressions.CheckedChanged += new System.EventHandler(this.shiftAfterAdjustCheckBox_CheckedChanged);
            // 
            // operandWidthGroupBox
            // 
            this.operandWidthGroupBox.Controls.Add(this.disambPrefix24TextBox);
            this.operandWidthGroupBox.Controls.Add(this.disambPrefix16TextBox);
            this.operandWidthGroupBox.Controls.Add(this.disambPrefix24Label);
            this.operandWidthGroupBox.Controls.Add(this.disambPrefix16Label);
            this.operandWidthGroupBox.Controls.Add(this.disambOperandPrefixLabel);
            this.operandWidthGroupBox.Controls.Add(this.disambSuffix24Label);
            this.operandWidthGroupBox.Controls.Add(this.disambSuffix16Label);
            this.operandWidthGroupBox.Controls.Add(this.disambOpcodeSuffixLabel);
            this.operandWidthGroupBox.Controls.Add(this.disambSuffix24TextBox);
            this.operandWidthGroupBox.Controls.Add(this.disambSuffix16TextBox);
            this.operandWidthGroupBox.Location = new System.Drawing.Point(6, 38);
            this.operandWidthGroupBox.Name = "operandWidthGroupBox";
            this.operandWidthGroupBox.Size = new System.Drawing.Size(295, 98);
            this.operandWidthGroupBox.TabIndex = 48;
            this.operandWidthGroupBox.TabStop = false;
            this.operandWidthGroupBox.Text = "Operand Width Disambiguator";
            // 
            // disambPrefix24TextBox
            // 
            this.disambPrefix24TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disambPrefix24TextBox.Location = new System.Drawing.Point(220, 64);
            this.disambPrefix24TextBox.MaxLength = 8;
            this.disambPrefix24TextBox.Name = "disambPrefix24TextBox";
            this.disambPrefix24TextBox.Size = new System.Drawing.Size(62, 20);
            this.disambPrefix24TextBox.TabIndex = 13;
            this.disambPrefix24TextBox.Text = ".placeho";
            this.disambPrefix24TextBox.TextChanged += new System.EventHandler(this.WidthDisamControlChanged);
            // 
            // disambPrefix16TextBox
            // 
            this.disambPrefix16TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disambPrefix16TextBox.Location = new System.Drawing.Point(220, 38);
            this.disambPrefix16TextBox.MaxLength = 8;
            this.disambPrefix16TextBox.Name = "disambPrefix16TextBox";
            this.disambPrefix16TextBox.Size = new System.Drawing.Size(62, 20);
            this.disambPrefix16TextBox.TabIndex = 12;
            this.disambPrefix16TextBox.Text = ".placeho";
            this.disambPrefix16TextBox.TextChanged += new System.EventHandler(this.WidthDisamControlChanged);
            // 
            // disambPrefix24Label
            // 
            this.disambPrefix24Label.AutoSize = true;
            this.disambPrefix24Label.Location = new System.Drawing.Point(171, 66);
            this.disambPrefix24Label.Name = "disambPrefix24Label";
            this.disambPrefix24Label.Size = new System.Drawing.Size(36, 13);
            this.disambPrefix24Label.TabIndex = 11;
            this.disambPrefix24Label.Text = "24 bit:";
            // 
            // disambPrefix16Label
            // 
            this.disambPrefix16Label.AutoSize = true;
            this.disambPrefix16Label.Location = new System.Drawing.Point(171, 40);
            this.disambPrefix16Label.Name = "disambPrefix16Label";
            this.disambPrefix16Label.Size = new System.Drawing.Size(36, 13);
            this.disambPrefix16Label.TabIndex = 10;
            this.disambPrefix16Label.Text = "16 bit:";
            // 
            // disambOperandPrefixLabel
            // 
            this.disambOperandPrefixLabel.AutoSize = true;
            this.disambOperandPrefixLabel.Location = new System.Drawing.Point(171, 20);
            this.disambOperandPrefixLabel.Name = "disambOperandPrefixLabel";
            this.disambOperandPrefixLabel.Size = new System.Drawing.Size(79, 13);
            this.disambOperandPrefixLabel.TabIndex = 9;
            this.disambOperandPrefixLabel.Text = "Operand prefix:";
            // 
            // disambSuffix24Label
            // 
            this.disambSuffix24Label.AutoSize = true;
            this.disambSuffix24Label.Location = new System.Drawing.Point(10, 66);
            this.disambSuffix24Label.Name = "disambSuffix24Label";
            this.disambSuffix24Label.Size = new System.Drawing.Size(36, 13);
            this.disambSuffix24Label.TabIndex = 8;
            this.disambSuffix24Label.Text = "24 bit:";
            // 
            // disambSuffix16Label
            // 
            this.disambSuffix16Label.AutoSize = true;
            this.disambSuffix16Label.Location = new System.Drawing.Point(10, 40);
            this.disambSuffix16Label.Name = "disambSuffix16Label";
            this.disambSuffix16Label.Size = new System.Drawing.Size(36, 13);
            this.disambSuffix16Label.TabIndex = 7;
            this.disambSuffix16Label.Text = "16 bit:";
            // 
            // disambOpcodeSuffixLabel
            // 
            this.disambOpcodeSuffixLabel.AutoSize = true;
            this.disambOpcodeSuffixLabel.Location = new System.Drawing.Point(7, 20);
            this.disambOpcodeSuffixLabel.Name = "disambOpcodeSuffixLabel";
            this.disambOpcodeSuffixLabel.Size = new System.Drawing.Size(75, 13);
            this.disambOpcodeSuffixLabel.TabIndex = 6;
            this.disambOpcodeSuffixLabel.Text = "Opcode suffix:";
            // 
            // disambSuffix24TextBox
            // 
            this.disambSuffix24TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disambSuffix24TextBox.Location = new System.Drawing.Point(62, 64);
            this.disambSuffix24TextBox.MaxLength = 8;
            this.disambSuffix24TextBox.Name = "disambSuffix24TextBox";
            this.disambSuffix24TextBox.Size = new System.Drawing.Size(62, 20);
            this.disambSuffix24TextBox.TabIndex = 5;
            this.disambSuffix24TextBox.Text = ".placeho";
            this.disambSuffix24TextBox.TextChanged += new System.EventHandler(this.WidthDisamControlChanged);
            // 
            // disambSuffix16TextBox
            // 
            this.disambSuffix16TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.disambSuffix16TextBox.Location = new System.Drawing.Point(62, 38);
            this.disambSuffix16TextBox.MaxLength = 8;
            this.disambSuffix16TextBox.Name = "disambSuffix16TextBox";
            this.disambSuffix16TextBox.Size = new System.Drawing.Size(62, 20);
            this.disambSuffix16TextBox.TabIndex = 3;
            this.disambSuffix16TextBox.Text = ".placeho";
            this.disambSuffix16TextBox.TextChanged += new System.EventHandler(this.WidthDisamControlChanged);
            // 
            // pseudoOpTabPage
            // 
            this.pseudoOpTabPage.Controls.Add(this.quickPseudoSetGroup);
            this.pseudoOpTabPage.Controls.Add(this.strDciHiTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strDciHiLabel);
            this.pseudoOpTabPage.Controls.Add(this.strDciTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strDciLabel);
            this.pseudoOpTabPage.Controls.Add(this.strLen16HiTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strLen16HiLabel);
            this.pseudoOpTabPage.Controls.Add(this.strLen16TextBox);
            this.pseudoOpTabPage.Controls.Add(this.strLen16Label);
            this.pseudoOpTabPage.Controls.Add(this.strReverseHiTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strReverseHiLabel);
            this.pseudoOpTabPage.Controls.Add(this.strReverseTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strReverseLabel);
            this.pseudoOpTabPage.Controls.Add(this.strNullTermHiTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strNullTermHiLabel);
            this.pseudoOpTabPage.Controls.Add(this.strNullTermTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strNullTermLabel);
            this.pseudoOpTabPage.Controls.Add(this.strLen8HiTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strLen8HiLabel);
            this.pseudoOpTabPage.Controls.Add(this.strLen8TextBox);
            this.pseudoOpTabPage.Controls.Add(this.strLen8Label);
            this.pseudoOpTabPage.Controls.Add(this.strGenericHiTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strGenericHiLabel);
            this.pseudoOpTabPage.Controls.Add(this.strGenericTextBox);
            this.pseudoOpTabPage.Controls.Add(this.strGenericLabel);
            this.pseudoOpTabPage.Controls.Add(this.popExplanationLabel);
            this.pseudoOpTabPage.Controls.Add(this.denseTextBox);
            this.pseudoOpTabPage.Controls.Add(this.denseLabel);
            this.pseudoOpTabPage.Controls.Add(this.fillTextBox);
            this.pseudoOpTabPage.Controls.Add(this.fillLabel);
            this.pseudoOpTabPage.Controls.Add(this.defineBigData2TextBox);
            this.pseudoOpTabPage.Controls.Add(this.defineBigData2Label);
            this.pseudoOpTabPage.Controls.Add(this.defineData4TextBox);
            this.pseudoOpTabPage.Controls.Add(this.defineData4Label);
            this.pseudoOpTabPage.Controls.Add(this.defineData3TextBox);
            this.pseudoOpTabPage.Controls.Add(this.defineData3Label);
            this.pseudoOpTabPage.Controls.Add(this.defineData2TextBox);
            this.pseudoOpTabPage.Controls.Add(this.defineData2Label);
            this.pseudoOpTabPage.Controls.Add(this.regWidthDirectiveTextBox);
            this.pseudoOpTabPage.Controls.Add(this.regWidthDirectiveLabel);
            this.pseudoOpTabPage.Controls.Add(this.orgDirectiveTextBox);
            this.pseudoOpTabPage.Controls.Add(this.orgDirectiveLabel);
            this.pseudoOpTabPage.Controls.Add(this.defineData1TextBox);
            this.pseudoOpTabPage.Controls.Add(this.defineData1Label);
            this.pseudoOpTabPage.Controls.Add(this.equDirectiveTextBox);
            this.pseudoOpTabPage.Controls.Add(this.equDirectiveLabel);
            this.pseudoOpTabPage.Location = new System.Drawing.Point(4, 22);
            this.pseudoOpTabPage.Name = "pseudoOpTabPage";
            this.pseudoOpTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.pseudoOpTabPage.Size = new System.Drawing.Size(614, 372);
            this.pseudoOpTabPage.TabIndex = 2;
            this.pseudoOpTabPage.Text = "Pseudo-Op";
            this.pseudoOpTabPage.UseVisualStyleBackColor = true;
            // 
            // quickPseudoSetGroup
            // 
            this.quickPseudoSetGroup.Controls.Add(this.quickPseudoMerlin32);
            this.quickPseudoSetGroup.Controls.Add(this.quickPseudoCc65Button);
            this.quickPseudoSetGroup.Controls.Add(this.quickPseudoDefaultButton);
            this.quickPseudoSetGroup.Location = new System.Drawing.Point(348, 291);
            this.quickPseudoSetGroup.Name = "quickPseudoSetGroup";
            this.quickPseudoSetGroup.Size = new System.Drawing.Size(258, 75);
            this.quickPseudoSetGroup.TabIndex = 46;
            this.quickPseudoSetGroup.TabStop = false;
            this.quickPseudoSetGroup.Text = "Quick Set";
            // 
            // quickPseudoMerlin32
            // 
            this.quickPseudoMerlin32.Location = new System.Drawing.Point(173, 30);
            this.quickPseudoMerlin32.Name = "quickPseudoMerlin32";
            this.quickPseudoMerlin32.Size = new System.Drawing.Size(75, 23);
            this.quickPseudoMerlin32.TabIndex = 2;
            this.quickPseudoMerlin32.Text = "Merlin 32";
            this.quickPseudoMerlin32.UseVisualStyleBackColor = true;
            this.quickPseudoMerlin32.Click += new System.EventHandler(this.quickPseudoMerlin32_Click);
            // 
            // quickPseudoCc65Button
            // 
            this.quickPseudoCc65Button.Location = new System.Drawing.Point(92, 30);
            this.quickPseudoCc65Button.Name = "quickPseudoCc65Button";
            this.quickPseudoCc65Button.Size = new System.Drawing.Size(75, 23);
            this.quickPseudoCc65Button.TabIndex = 1;
            this.quickPseudoCc65Button.Text = "cc65";
            this.quickPseudoCc65Button.UseVisualStyleBackColor = true;
            this.quickPseudoCc65Button.Click += new System.EventHandler(this.quickPseudoCc65Button_Click);
            // 
            // quickPseudoDefaultButton
            // 
            this.quickPseudoDefaultButton.Location = new System.Drawing.Point(11, 30);
            this.quickPseudoDefaultButton.Name = "quickPseudoDefaultButton";
            this.quickPseudoDefaultButton.Size = new System.Drawing.Size(75, 23);
            this.quickPseudoDefaultButton.TabIndex = 0;
            this.quickPseudoDefaultButton.Text = "Default";
            this.quickPseudoDefaultButton.UseVisualStyleBackColor = true;
            this.quickPseudoDefaultButton.Click += new System.EventHandler(this.quickPseudoDefaultButton_Click);
            // 
            // strDciHiTextBox
            // 
            this.strDciHiTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strDciHiTextBox.Location = new System.Drawing.Point(546, 230);
            this.strDciHiTextBox.MaxLength = 8;
            this.strDciHiTextBox.Name = "strDciHiTextBox";
            this.strDciHiTextBox.Size = new System.Drawing.Size(62, 20);
            this.strDciHiTextBox.TabIndex = 44;
            this.strDciHiTextBox.Text = ".placeho";
            this.strDciHiTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strDciHiLabel
            // 
            this.strDciHiLabel.Location = new System.Drawing.Point(463, 232);
            this.strDciHiLabel.Name = "strDciHiLabel";
            this.strDciHiLabel.Size = new System.Drawing.Size(79, 23);
            this.strDciHiLabel.TabIndex = 43;
            this.strDciHiLabel.Text = "DCI/hi:";
            this.strDciHiLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strDciTextBox
            // 
            this.strDciTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strDciTextBox.Location = new System.Drawing.Point(394, 230);
            this.strDciTextBox.MaxLength = 8;
            this.strDciTextBox.Name = "strDciTextBox";
            this.strDciTextBox.Size = new System.Drawing.Size(62, 20);
            this.strDciTextBox.TabIndex = 42;
            this.strDciTextBox.Text = ".placeho";
            this.strDciTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strDciLabel
            // 
            this.strDciLabel.Location = new System.Drawing.Point(309, 232);
            this.strDciLabel.Name = "strDciLabel";
            this.strDciLabel.Size = new System.Drawing.Size(82, 23);
            this.strDciLabel.TabIndex = 41;
            this.strDciLabel.Text = "DCI:";
            this.strDciLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strLen16HiTextBox
            // 
            this.strLen16HiTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strLen16HiTextBox.Location = new System.Drawing.Point(546, 192);
            this.strLen16HiTextBox.MaxLength = 8;
            this.strLen16HiTextBox.Name = "strLen16HiTextBox";
            this.strLen16HiTextBox.Size = new System.Drawing.Size(62, 20);
            this.strLen16HiTextBox.TabIndex = 36;
            this.strLen16HiTextBox.Text = ".placeho";
            this.strLen16HiTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strLen16HiLabel
            // 
            this.strLen16HiLabel.Location = new System.Drawing.Point(463, 194);
            this.strLen16HiLabel.Name = "strLen16HiLabel";
            this.strLen16HiLabel.Size = new System.Drawing.Size(79, 23);
            this.strLen16HiLabel.TabIndex = 35;
            this.strLen16HiLabel.Text = "2-byte len/hi:";
            this.strLen16HiLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strLen16TextBox
            // 
            this.strLen16TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strLen16TextBox.Location = new System.Drawing.Point(394, 192);
            this.strLen16TextBox.MaxLength = 8;
            this.strLen16TextBox.Name = "strLen16TextBox";
            this.strLen16TextBox.Size = new System.Drawing.Size(62, 20);
            this.strLen16TextBox.TabIndex = 34;
            this.strLen16TextBox.Text = ".placeho";
            this.strLen16TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strLen16Label
            // 
            this.strLen16Label.Location = new System.Drawing.Point(309, 194);
            this.strLen16Label.Name = "strLen16Label";
            this.strLen16Label.Size = new System.Drawing.Size(82, 23);
            this.strLen16Label.TabIndex = 33;
            this.strLen16Label.Text = "2-byte len:";
            this.strLen16Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strReverseHiTextBox
            // 
            this.strReverseHiTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strReverseHiTextBox.Location = new System.Drawing.Point(546, 154);
            this.strReverseHiTextBox.MaxLength = 8;
            this.strReverseHiTextBox.Name = "strReverseHiTextBox";
            this.strReverseHiTextBox.Size = new System.Drawing.Size(62, 20);
            this.strReverseHiTextBox.TabIndex = 28;
            this.strReverseHiTextBox.Text = ".placeho";
            this.strReverseHiTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strReverseHiLabel
            // 
            this.strReverseHiLabel.Location = new System.Drawing.Point(463, 156);
            this.strReverseHiLabel.Name = "strReverseHiLabel";
            this.strReverseHiLabel.Size = new System.Drawing.Size(79, 23);
            this.strReverseHiLabel.TabIndex = 27;
            this.strReverseHiLabel.Text = "Reverse/hi:";
            this.strReverseHiLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strReverseTextBox
            // 
            this.strReverseTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strReverseTextBox.Location = new System.Drawing.Point(394, 154);
            this.strReverseTextBox.MaxLength = 8;
            this.strReverseTextBox.Name = "strReverseTextBox";
            this.strReverseTextBox.Size = new System.Drawing.Size(62, 20);
            this.strReverseTextBox.TabIndex = 26;
            this.strReverseTextBox.Text = ".placeho";
            this.strReverseTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strReverseLabel
            // 
            this.strReverseLabel.Location = new System.Drawing.Point(309, 156);
            this.strReverseLabel.Name = "strReverseLabel";
            this.strReverseLabel.Size = new System.Drawing.Size(82, 23);
            this.strReverseLabel.TabIndex = 25;
            this.strReverseLabel.Text = "Reverse:";
            this.strReverseLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strNullTermHiTextBox
            // 
            this.strNullTermHiTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strNullTermHiTextBox.Location = new System.Drawing.Point(244, 230);
            this.strNullTermHiTextBox.MaxLength = 8;
            this.strNullTermHiTextBox.Name = "strNullTermHiTextBox";
            this.strNullTermHiTextBox.Size = new System.Drawing.Size(62, 20);
            this.strNullTermHiTextBox.TabIndex = 40;
            this.strNullTermHiTextBox.Text = ".placeho";
            this.strNullTermHiTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strNullTermHiLabel
            // 
            this.strNullTermHiLabel.Location = new System.Drawing.Point(160, 232);
            this.strNullTermHiLabel.Name = "strNullTermHiLabel";
            this.strNullTermHiLabel.Size = new System.Drawing.Size(80, 23);
            this.strNullTermHiLabel.TabIndex = 39;
            this.strNullTermHiLabel.Text = "Null term/hi:";
            this.strNullTermHiLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strNullTermTextBox
            // 
            this.strNullTermTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strNullTermTextBox.Location = new System.Drawing.Point(92, 230);
            this.strNullTermTextBox.MaxLength = 8;
            this.strNullTermTextBox.Name = "strNullTermTextBox";
            this.strNullTermTextBox.Size = new System.Drawing.Size(62, 20);
            this.strNullTermTextBox.TabIndex = 38;
            this.strNullTermTextBox.Text = ".placeho";
            this.strNullTermTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strNullTermLabel
            // 
            this.strNullTermLabel.Location = new System.Drawing.Point(3, 232);
            this.strNullTermLabel.Name = "strNullTermLabel";
            this.strNullTermLabel.Size = new System.Drawing.Size(85, 23);
            this.strNullTermLabel.TabIndex = 37;
            this.strNullTermLabel.Text = "Null term string:";
            this.strNullTermLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strLen8HiTextBox
            // 
            this.strLen8HiTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strLen8HiTextBox.Location = new System.Drawing.Point(244, 192);
            this.strLen8HiTextBox.MaxLength = 8;
            this.strLen8HiTextBox.Name = "strLen8HiTextBox";
            this.strLen8HiTextBox.Size = new System.Drawing.Size(62, 20);
            this.strLen8HiTextBox.TabIndex = 32;
            this.strLen8HiTextBox.Text = ".placeho";
            this.strLen8HiTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strLen8HiLabel
            // 
            this.strLen8HiLabel.Location = new System.Drawing.Point(160, 194);
            this.strLen8HiLabel.Name = "strLen8HiLabel";
            this.strLen8HiLabel.Size = new System.Drawing.Size(80, 23);
            this.strLen8HiLabel.TabIndex = 31;
            this.strLen8HiLabel.Text = "1-byte len/hi:";
            this.strLen8HiLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strLen8TextBox
            // 
            this.strLen8TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strLen8TextBox.Location = new System.Drawing.Point(92, 192);
            this.strLen8TextBox.MaxLength = 8;
            this.strLen8TextBox.Name = "strLen8TextBox";
            this.strLen8TextBox.Size = new System.Drawing.Size(62, 20);
            this.strLen8TextBox.TabIndex = 30;
            this.strLen8TextBox.Text = ".placeho";
            this.strLen8TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strLen8Label
            // 
            this.strLen8Label.Location = new System.Drawing.Point(7, 194);
            this.strLen8Label.Name = "strLen8Label";
            this.strLen8Label.Size = new System.Drawing.Size(81, 23);
            this.strLen8Label.TabIndex = 29;
            this.strLen8Label.Text = "1-byte len:";
            this.strLen8Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strGenericHiTextBox
            // 
            this.strGenericHiTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strGenericHiTextBox.Location = new System.Drawing.Point(244, 154);
            this.strGenericHiTextBox.MaxLength = 8;
            this.strGenericHiTextBox.Name = "strGenericHiTextBox";
            this.strGenericHiTextBox.Size = new System.Drawing.Size(62, 20);
            this.strGenericHiTextBox.TabIndex = 24;
            this.strGenericHiTextBox.Text = ".placeho";
            this.strGenericHiTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strGenericHiLabel
            // 
            this.strGenericHiLabel.Location = new System.Drawing.Point(160, 156);
            this.strGenericHiLabel.Name = "strGenericHiLabel";
            this.strGenericHiLabel.Size = new System.Drawing.Size(80, 23);
            this.strGenericHiLabel.TabIndex = 23;
            this.strGenericHiLabel.Text = "Generic/hi:";
            this.strGenericHiLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // strGenericTextBox
            // 
            this.strGenericTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.strGenericTextBox.Location = new System.Drawing.Point(92, 154);
            this.strGenericTextBox.MaxLength = 8;
            this.strGenericTextBox.Name = "strGenericTextBox";
            this.strGenericTextBox.Size = new System.Drawing.Size(62, 20);
            this.strGenericTextBox.TabIndex = 22;
            this.strGenericTextBox.Text = ".placeho";
            this.strGenericTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // strGenericLabel
            // 
            this.strGenericLabel.Location = new System.Drawing.Point(3, 156);
            this.strGenericLabel.Name = "strGenericLabel";
            this.strGenericLabel.Size = new System.Drawing.Size(85, 23);
            this.strGenericLabel.TabIndex = 21;
            this.strGenericLabel.Text = "Generic string:";
            this.strGenericLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // popExplanationLabel
            // 
            this.popExplanationLabel.AutoSize = true;
            this.popExplanationLabel.Location = new System.Drawing.Point(7, 7);
            this.popExplanationLabel.Name = "popExplanationLabel";
            this.popExplanationLabel.Size = new System.Drawing.Size(541, 13);
            this.popExplanationLabel.TabIndex = 0;
            this.popExplanationLabel.Text = "Select pseudo-op names for display. This does not affect source code generation. " +
    "Blank entries get default value.";
            // 
            // denseTextBox
            // 
            this.denseTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.denseTextBox.Location = new System.Drawing.Point(546, 116);
            this.denseTextBox.MaxLength = 8;
            this.denseTextBox.Name = "denseTextBox";
            this.denseTextBox.Size = new System.Drawing.Size(62, 20);
            this.denseTextBox.TabIndex = 20;
            this.denseTextBox.Text = ".placeho";
            this.denseTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // denseLabel
            // 
            this.denseLabel.Location = new System.Drawing.Point(460, 118);
            this.denseLabel.Name = "denseLabel";
            this.denseLabel.Size = new System.Drawing.Size(82, 23);
            this.denseLabel.TabIndex = 19;
            this.denseLabel.Text = "Bulk data:";
            this.denseLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // fillTextBox
            // 
            this.fillTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.fillTextBox.Location = new System.Drawing.Point(395, 116);
            this.fillTextBox.MaxLength = 8;
            this.fillTextBox.Name = "fillTextBox";
            this.fillTextBox.Size = new System.Drawing.Size(62, 20);
            this.fillTextBox.TabIndex = 18;
            this.fillTextBox.Text = ".placeho";
            this.fillTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // fillLabel
            // 
            this.fillLabel.Location = new System.Drawing.Point(309, 118);
            this.fillLabel.Name = "fillLabel";
            this.fillLabel.Size = new System.Drawing.Size(82, 23);
            this.fillLabel.TabIndex = 17;
            this.fillLabel.Text = "Fill:";
            this.fillLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // defineBigData2TextBox
            // 
            this.defineBigData2TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defineBigData2TextBox.Location = new System.Drawing.Point(92, 116);
            this.defineBigData2TextBox.MaxLength = 8;
            this.defineBigData2TextBox.Name = "defineBigData2TextBox";
            this.defineBigData2TextBox.Size = new System.Drawing.Size(62, 20);
            this.defineBigData2TextBox.TabIndex = 16;
            this.defineBigData2TextBox.Text = ".placeho";
            this.defineBigData2TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // defineBigData2Label
            // 
            this.defineBigData2Label.Location = new System.Drawing.Point(3, 113);
            this.defineBigData2Label.Name = "defineBigData2Label";
            this.defineBigData2Label.Size = new System.Drawing.Size(85, 32);
            this.defineBigData2Label.TabIndex = 15;
            this.defineBigData2Label.Text = "Big-endian data, two bytes:";
            this.defineBigData2Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // defineData4TextBox
            // 
            this.defineData4TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defineData4TextBox.Location = new System.Drawing.Point(546, 78);
            this.defineData4TextBox.MaxLength = 8;
            this.defineData4TextBox.Name = "defineData4TextBox";
            this.defineData4TextBox.Size = new System.Drawing.Size(62, 20);
            this.defineData4TextBox.TabIndex = 14;
            this.defineData4TextBox.Text = ".placeho";
            this.defineData4TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // defineData4Label
            // 
            this.defineData4Label.Location = new System.Drawing.Point(463, 80);
            this.defineData4Label.Name = "defineData4Label";
            this.defineData4Label.Size = new System.Drawing.Size(79, 23);
            this.defineData4Label.TabIndex = 13;
            this.defineData4Label.Text = "Four bytes:";
            this.defineData4Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // defineData3TextBox
            // 
            this.defineData3TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defineData3TextBox.Location = new System.Drawing.Point(395, 78);
            this.defineData3TextBox.MaxLength = 8;
            this.defineData3TextBox.Name = "defineData3TextBox";
            this.defineData3TextBox.Size = new System.Drawing.Size(62, 20);
            this.defineData3TextBox.TabIndex = 12;
            this.defineData3TextBox.Text = ".placeho";
            this.defineData3TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // defineData3Label
            // 
            this.defineData3Label.Location = new System.Drawing.Point(309, 80);
            this.defineData3Label.Name = "defineData3Label";
            this.defineData3Label.Size = new System.Drawing.Size(82, 23);
            this.defineData3Label.TabIndex = 11;
            this.defineData3Label.Text = "Three bytes:";
            this.defineData3Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // defineData2TextBox
            // 
            this.defineData2TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defineData2TextBox.Location = new System.Drawing.Point(244, 78);
            this.defineData2TextBox.MaxLength = 8;
            this.defineData2TextBox.Name = "defineData2TextBox";
            this.defineData2TextBox.Size = new System.Drawing.Size(62, 20);
            this.defineData2TextBox.TabIndex = 10;
            this.defineData2TextBox.Text = ".placeho";
            this.defineData2TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // defineData2Label
            // 
            this.defineData2Label.Location = new System.Drawing.Point(160, 80);
            this.defineData2Label.Name = "defineData2Label";
            this.defineData2Label.Size = new System.Drawing.Size(80, 23);
            this.defineData2Label.TabIndex = 9;
            this.defineData2Label.Text = "Two bytes:";
            this.defineData2Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // regWidthDirectiveTextBox
            // 
            this.regWidthDirectiveTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.regWidthDirectiveTextBox.Location = new System.Drawing.Point(395, 40);
            this.regWidthDirectiveTextBox.MaxLength = 8;
            this.regWidthDirectiveTextBox.Name = "regWidthDirectiveTextBox";
            this.regWidthDirectiveTextBox.Size = new System.Drawing.Size(62, 20);
            this.regWidthDirectiveTextBox.TabIndex = 6;
            this.regWidthDirectiveTextBox.Text = ".placeho";
            this.regWidthDirectiveTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // regWidthDirectiveLabel
            // 
            this.regWidthDirectiveLabel.Location = new System.Drawing.Point(312, 42);
            this.regWidthDirectiveLabel.Name = "regWidthDirectiveLabel";
            this.regWidthDirectiveLabel.Size = new System.Drawing.Size(79, 23);
            this.regWidthDirectiveLabel.TabIndex = 5;
            this.regWidthDirectiveLabel.Text = "Reg width:";
            this.regWidthDirectiveLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // orgDirectiveTextBox
            // 
            this.orgDirectiveTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.orgDirectiveTextBox.Location = new System.Drawing.Point(244, 40);
            this.orgDirectiveTextBox.MaxLength = 8;
            this.orgDirectiveTextBox.Name = "orgDirectiveTextBox";
            this.orgDirectiveTextBox.Size = new System.Drawing.Size(62, 20);
            this.orgDirectiveTextBox.TabIndex = 4;
            this.orgDirectiveTextBox.Text = ".placeho";
            this.orgDirectiveTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // orgDirectiveLabel
            // 
            this.orgDirectiveLabel.Location = new System.Drawing.Point(160, 42);
            this.orgDirectiveLabel.Name = "orgDirectiveLabel";
            this.orgDirectiveLabel.Size = new System.Drawing.Size(80, 23);
            this.orgDirectiveLabel.TabIndex = 3;
            this.orgDirectiveLabel.Text = "Origin:";
            this.orgDirectiveLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // defineData1TextBox
            // 
            this.defineData1TextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.defineData1TextBox.Location = new System.Drawing.Point(92, 78);
            this.defineData1TextBox.MaxLength = 8;
            this.defineData1TextBox.Name = "defineData1TextBox";
            this.defineData1TextBox.Size = new System.Drawing.Size(62, 20);
            this.defineData1TextBox.TabIndex = 8;
            this.defineData1TextBox.Text = ".placeho";
            this.defineData1TextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // defineData1Label
            // 
            this.defineData1Label.Location = new System.Drawing.Point(3, 74);
            this.defineData1Label.Name = "defineData1Label";
            this.defineData1Label.Size = new System.Drawing.Size(85, 32);
            this.defineData1Label.TabIndex = 7;
            this.defineData1Label.Text = "Little-endian data, one byte:";
            this.defineData1Label.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // equDirectiveTextBox
            // 
            this.equDirectiveTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.equDirectiveTextBox.Location = new System.Drawing.Point(92, 40);
            this.equDirectiveTextBox.MaxLength = 8;
            this.equDirectiveTextBox.Name = "equDirectiveTextBox";
            this.equDirectiveTextBox.Size = new System.Drawing.Size(62, 20);
            this.equDirectiveTextBox.TabIndex = 2;
            this.equDirectiveTextBox.Text = ".placeho";
            this.equDirectiveTextBox.TextChanged += new System.EventHandler(this.PseudoOpTextChanged);
            // 
            // equDirectiveLabel
            // 
            this.equDirectiveLabel.Location = new System.Drawing.Point(3, 42);
            this.equDirectiveLabel.Name = "equDirectiveLabel";
            this.equDirectiveLabel.Size = new System.Drawing.Size(85, 23);
            this.equDirectiveLabel.TabIndex = 1;
            this.equDirectiveLabel.Text = "Equate:";
            this.equDirectiveLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // EditAppSettings
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(624, 441);
            this.Controls.Add(this.settingsTabControl);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditAppSettings";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Settings";
            this.Load += new System.EventHandler(this.EditAppSettings_Load);
            this.settingsTabControl.ResumeLayout(false);
            this.codeViewTabPage.ResumeLayout(false);
            this.codeViewTabPage.PerformLayout();
            this.clipboardGroupBox.ResumeLayout(false);
            this.clipboardGroupBox.PerformLayout();
            this.upperCaseGroupBox.ResumeLayout(false);
            this.upperCaseGroupBox.PerformLayout();
            this.codeViewFontGroupBox.ResumeLayout(false);
            this.codeViewFontGroupBox.PerformLayout();
            this.columnVisGroup.ResumeLayout(false);
            this.asmConfigTabPage.ResumeLayout(false);
            this.asmConfigTabPage.PerformLayout();
            this.configureAsmGroupBox.ResumeLayout(false);
            this.configureAsmGroupBox.PerformLayout();
            this.displayFormatTabPage.ResumeLayout(false);
            this.displayFormatTabPage.PerformLayout();
            this.quickDisplayFormatGroup.ResumeLayout(false);
            this.operandWidthGroupBox.ResumeLayout(false);
            this.operandWidthGroupBox.PerformLayout();
            this.pseudoOpTabPage.ResumeLayout(false);
            this.pseudoOpTabPage.PerformLayout();
            this.quickPseudoSetGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.TabControl settingsTabControl;
        private System.Windows.Forms.TabPage codeViewTabPage;
        private System.Windows.Forms.TabPage asmConfigTabPage;
        private System.Windows.Forms.TabPage pseudoOpTabPage;
        private System.Windows.Forms.GroupBox columnVisGroup;
        private System.Windows.Forms.Button showCol0;
        private System.Windows.Forms.Button showCol8;
        private System.Windows.Forms.Button showCol1;
        private System.Windows.Forms.Button showCol7;
        private System.Windows.Forms.Button showCol2;
        private System.Windows.Forms.Button showCol6;
        private System.Windows.Forms.Button showCol3;
        private System.Windows.Forms.Button showCol5;
        private System.Windows.Forms.Button showCol4;
        private System.Windows.Forms.TextBox defineData1TextBox;
        private System.Windows.Forms.Label defineData1Label;
        private System.Windows.Forms.TextBox equDirectiveTextBox;
        private System.Windows.Forms.Label equDirectiveLabel;
        private System.Windows.Forms.TextBox denseTextBox;
        private System.Windows.Forms.Label denseLabel;
        private System.Windows.Forms.TextBox fillTextBox;
        private System.Windows.Forms.Label fillLabel;
        private System.Windows.Forms.TextBox defineBigData2TextBox;
        private System.Windows.Forms.Label defineBigData2Label;
        private System.Windows.Forms.TextBox defineData4TextBox;
        private System.Windows.Forms.Label defineData4Label;
        private System.Windows.Forms.TextBox defineData3TextBox;
        private System.Windows.Forms.Label defineData3Label;
        private System.Windows.Forms.TextBox defineData2TextBox;
        private System.Windows.Forms.Label defineData2Label;
        private System.Windows.Forms.TextBox regWidthDirectiveTextBox;
        private System.Windows.Forms.Label regWidthDirectiveLabel;
        private System.Windows.Forms.TextBox orgDirectiveTextBox;
        private System.Windows.Forms.Label orgDirectiveLabel;
        private System.Windows.Forms.Label popExplanationLabel;
        private System.Windows.Forms.TextBox strLen8HiTextBox;
        private System.Windows.Forms.Label strLen8HiLabel;
        private System.Windows.Forms.TextBox strLen8TextBox;
        private System.Windows.Forms.Label strLen8Label;
        private System.Windows.Forms.TextBox strGenericHiTextBox;
        private System.Windows.Forms.Label strGenericHiLabel;
        private System.Windows.Forms.TextBox strGenericTextBox;
        private System.Windows.Forms.Label strGenericLabel;
        private System.Windows.Forms.TextBox strNullTermHiTextBox;
        private System.Windows.Forms.Label strNullTermHiLabel;
        private System.Windows.Forms.TextBox strNullTermTextBox;
        private System.Windows.Forms.Label strNullTermLabel;
        private System.Windows.Forms.TextBox strDciHiTextBox;
        private System.Windows.Forms.Label strDciHiLabel;
        private System.Windows.Forms.TextBox strDciTextBox;
        private System.Windows.Forms.Label strDciLabel;
        private System.Windows.Forms.TextBox strLen16HiTextBox;
        private System.Windows.Forms.Label strLen16HiLabel;
        private System.Windows.Forms.TextBox strLen16TextBox;
        private System.Windows.Forms.Label strLen16Label;
        private System.Windows.Forms.TextBox strReverseHiTextBox;
        private System.Windows.Forms.Label strReverseHiLabel;
        private System.Windows.Forms.TextBox strReverseTextBox;
        private System.Windows.Forms.Label strReverseLabel;
        private System.Windows.Forms.TextBox asmExePathTextBox;
        private System.Windows.Forms.Button asmExeBrowseButton;
        private System.Windows.Forms.GroupBox codeViewFontGroupBox;
        private System.Windows.Forms.Label currentFontLabel;
        private System.Windows.Forms.Button selectFontButton;
        private System.Windows.Forms.Label currentFontDisplayLabel;
        private System.Windows.Forms.GroupBox quickPseudoSetGroup;
        private System.Windows.Forms.Button quickPseudoMerlin32;
        private System.Windows.Forms.Button quickPseudoCc65Button;
        private System.Windows.Forms.Button quickPseudoDefaultButton;
        private System.Windows.Forms.GroupBox upperCaseGroupBox;
        private System.Windows.Forms.CheckBox upperOpcodeCheckBox;
        private System.Windows.Forms.CheckBox upperHexCheckBox;
        private System.Windows.Forms.Button upperAllUpperButton;
        private System.Windows.Forms.Button upperAllLowerButton;
        private System.Windows.Forms.CheckBox upperXYCheckBox;
        private System.Windows.Forms.CheckBox upperSCheckBox;
        private System.Windows.Forms.CheckBox upperACheckBox;
        private System.Windows.Forms.CheckBox upperPseudoOpCheckBox;
        private System.Windows.Forms.CheckBox disableLabelLocalizationCheckBox;
        private System.Windows.Forms.CheckBox enableDebugCheckBox;
        private System.Windows.Forms.CheckBox showAsmIdentCheckBox;
        private System.Windows.Forms.TabPage displayFormatTabPage;
        private System.Windows.Forms.CheckBox useMerlinExpressions;
        private System.Windows.Forms.GroupBox operandWidthGroupBox;
        private System.Windows.Forms.TextBox disambPrefix24TextBox;
        private System.Windows.Forms.TextBox disambPrefix16TextBox;
        private System.Windows.Forms.Label disambPrefix24Label;
        private System.Windows.Forms.Label disambPrefix16Label;
        private System.Windows.Forms.Label disambOperandPrefixLabel;
        private System.Windows.Forms.Label disambSuffix24Label;
        private System.Windows.Forms.Label disambSuffix16Label;
        private System.Windows.Forms.Label disambOpcodeSuffixLabel;
        private System.Windows.Forms.TextBox disambSuffix24TextBox;
        private System.Windows.Forms.TextBox disambSuffix16TextBox;
        private System.Windows.Forms.GroupBox quickDisplayFormatGroup;
        private System.Windows.Forms.Button quickFmtMerlin32Button;
        private System.Windows.Forms.Button quickFmtCc65Button;
        private System.Windows.Forms.Button quickFmtDefaultButton;
        private System.Windows.Forms.Label configAsmGenLabel;
        private System.Windows.Forms.CheckBox longLabelNewLineCheckBox;
        private System.Windows.Forms.Label fmtExplanationLabel;
        private System.Windows.Forms.CheckBox showCycleCountsCheckBox;
        private System.Windows.Forms.GroupBox clipboardGroupBox;
        private System.Windows.Forms.ComboBox clipboardFormatComboBox;
        private System.Windows.Forms.Label clipboardFormatLabel;
        private System.Windows.Forms.ComboBox asmConfigComboBox;
        private System.Windows.Forms.GroupBox configureAsmGroupBox;
        private System.Windows.Forms.Label asmExeLabel;
        private System.Windows.Forms.Label asmColWidthLabel;
        private System.Windows.Forms.TextBox asmCommentColWidthTextBox;
        private System.Windows.Forms.TextBox asmOperandColWidthTextBox;
        private System.Windows.Forms.TextBox asmOpcodeColWidthTextBox;
        private System.Windows.Forms.TextBox asmLabelColWidthTextBox;
        private System.Windows.Forms.Label asmConfigAssemblerLabel;
        private System.Windows.Forms.Label asmColWidthIdLabel;
    }
}