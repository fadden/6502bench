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
    partial class EditProjectProperties {
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
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
            "0123456789AB",
            "%00000000",
            "Const",
            "This is a test to gauge column widths"}, -1);
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.generalTab = new System.Windows.Forms.TabPage();
            this.analysisGroupBox = new System.Windows.Forms.GroupBox();
            this.seekAltTargetCheckBox = new System.Windows.Forms.CheckBox();
            this.minStringCharsComboBox = new System.Windows.Forms.ComboBox();
            this.minCharsForStringLabel = new System.Windows.Forms.Label();
            this.analyzeUncategorizedCheckBox = new System.Windows.Forms.CheckBox();
            this.entryFlagsGroupBox = new System.Windows.Forms.GroupBox();
            this.flagsLabel = new System.Windows.Forms.Label();
            this.currentFlagsLabel = new System.Windows.Forms.Label();
            this.changeFlagButton = new System.Windows.Forms.Button();
            this.cpuGroupBox = new System.Windows.Forms.GroupBox();
            this.undocInstrCheckBox = new System.Windows.Forms.CheckBox();
            this.cpuComboBox = new System.Windows.Forms.ComboBox();
            this.symbolsTab = new System.Windows.Forms.TabPage();
            this.importSymbolsButton = new System.Windows.Forms.Button();
            this.editSymbolButton = new System.Windows.Forms.Button();
            this.removeSymbolButton = new System.Windows.Forms.Button();
            this.newSymbolButton = new System.Windows.Forms.Button();
            this.symbolsDefinedLabel = new System.Windows.Forms.Label();
            this.projectSymbolsListView = new System.Windows.Forms.ListView();
            this.nameColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.valueColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.typeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.commentColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.symbolFilesTab = new System.Windows.Forms.TabPage();
            this.symbolFileDownButton = new System.Windows.Forms.Button();
            this.symbolFileUpButton = new System.Windows.Forms.Button();
            this.addSymbolFilesButton = new System.Windows.Forms.Button();
            this.symbolFileRemoveButton = new System.Windows.Forms.Button();
            this.symbolFilesListBox = new System.Windows.Forms.ListBox();
            this.configuredFilesLabel = new System.Windows.Forms.Label();
            this.extensionScriptsTab = new System.Windows.Forms.TabPage();
            this.extensionScriptRemoveButton = new System.Windows.Forms.Button();
            this.addExtensionScriptsButton = new System.Windows.Forms.Button();
            this.extensionScriptsListBox = new System.Windows.Forms.ListBox();
            this.configuredScriptsLabel = new System.Windows.Forms.Label();
            this.labelUndoRedoNote = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.applyButton = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.generalTab.SuspendLayout();
            this.analysisGroupBox.SuspendLayout();
            this.entryFlagsGroupBox.SuspendLayout();
            this.cpuGroupBox.SuspendLayout();
            this.symbolsTab.SuspendLayout();
            this.symbolFilesTab.SuspendLayout();
            this.extensionScriptsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.generalTab);
            this.tabControl1.Controls.Add(this.symbolsTab);
            this.tabControl1.Controls.Add(this.symbolFilesTab);
            this.tabControl1.Controls.Add(this.extensionScriptsTab);
            this.tabControl1.Location = new System.Drawing.Point(2, 2);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(622, 318);
            this.tabControl1.TabIndex = 0;
            // 
            // generalTab
            // 
            this.generalTab.Controls.Add(this.analysisGroupBox);
            this.generalTab.Controls.Add(this.entryFlagsGroupBox);
            this.generalTab.Controls.Add(this.cpuGroupBox);
            this.generalTab.Location = new System.Drawing.Point(4, 22);
            this.generalTab.Name = "generalTab";
            this.generalTab.Padding = new System.Windows.Forms.Padding(3);
            this.generalTab.Size = new System.Drawing.Size(614, 292);
            this.generalTab.TabIndex = 0;
            this.generalTab.Text = "General";
            this.generalTab.UseVisualStyleBackColor = true;
            // 
            // analysisGroupBox
            // 
            this.analysisGroupBox.Controls.Add(this.seekAltTargetCheckBox);
            this.analysisGroupBox.Controls.Add(this.minStringCharsComboBox);
            this.analysisGroupBox.Controls.Add(this.minCharsForStringLabel);
            this.analysisGroupBox.Controls.Add(this.analyzeUncategorizedCheckBox);
            this.analysisGroupBox.Location = new System.Drawing.Point(225, 7);
            this.analysisGroupBox.Name = "analysisGroupBox";
            this.analysisGroupBox.Size = new System.Drawing.Size(204, 163);
            this.analysisGroupBox.TabIndex = 2;
            this.analysisGroupBox.TabStop = false;
            this.analysisGroupBox.Text = "Analysis Parameters";
            // 
            // seekAltTargetCheckBox
            // 
            this.seekAltTargetCheckBox.AutoSize = true;
            this.seekAltTargetCheckBox.Location = new System.Drawing.Point(7, 45);
            this.seekAltTargetCheckBox.Name = "seekAltTargetCheckBox";
            this.seekAltTargetCheckBox.Size = new System.Drawing.Size(130, 17);
            this.seekAltTargetCheckBox.TabIndex = 3;
            this.seekAltTargetCheckBox.Text = "Seek alternate targets";
            this.seekAltTargetCheckBox.UseVisualStyleBackColor = true;
            this.seekAltTargetCheckBox.CheckedChanged += new System.EventHandler(this.seekAltTargetCheckBox_CheckedChanged);
            // 
            // minStringCharsComboBox
            // 
            this.minStringCharsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.minStringCharsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.minStringCharsComboBox.FormattingEnabled = true;
            this.minStringCharsComboBox.Items.AddRange(new object[] {
            "None (disabled)",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.minStringCharsComboBox.Location = new System.Drawing.Point(7, 88);
            this.minStringCharsComboBox.Name = "minStringCharsComboBox";
            this.minStringCharsComboBox.Size = new System.Drawing.Size(191, 21);
            this.minStringCharsComboBox.TabIndex = 2;
            this.minStringCharsComboBox.SelectedIndexChanged += new System.EventHandler(this.minStringCharsComboBox_SelectedIndexChanged);
            // 
            // minCharsForStringLabel
            // 
            this.minCharsForStringLabel.AutoSize = true;
            this.minCharsForStringLabel.Location = new System.Drawing.Point(7, 71);
            this.minCharsForStringLabel.Name = "minCharsForStringLabel";
            this.minCharsForStringLabel.Size = new System.Drawing.Size(147, 13);
            this.minCharsForStringLabel.TabIndex = 1;
            this.minCharsForStringLabel.Text = "Minimum characters for string:";
            // 
            // analyzeUncategorizedCheckBox
            // 
            this.analyzeUncategorizedCheckBox.AutoSize = true;
            this.analyzeUncategorizedCheckBox.Location = new System.Drawing.Point(7, 21);
            this.analyzeUncategorizedCheckBox.Name = "analyzeUncategorizedCheckBox";
            this.analyzeUncategorizedCheckBox.Size = new System.Drawing.Size(157, 17);
            this.analyzeUncategorizedCheckBox.TabIndex = 0;
            this.analyzeUncategorizedCheckBox.Text = "Analyze uncategorized data";
            this.analyzeUncategorizedCheckBox.UseVisualStyleBackColor = true;
            this.analyzeUncategorizedCheckBox.CheckedChanged += new System.EventHandler(this.analyzeUncategorizedCheckBox_CheckedChanged);
            // 
            // entryFlagsGroupBox
            // 
            this.entryFlagsGroupBox.Controls.Add(this.flagsLabel);
            this.entryFlagsGroupBox.Controls.Add(this.currentFlagsLabel);
            this.entryFlagsGroupBox.Controls.Add(this.changeFlagButton);
            this.entryFlagsGroupBox.Location = new System.Drawing.Point(7, 92);
            this.entryFlagsGroupBox.Name = "entryFlagsGroupBox";
            this.entryFlagsGroupBox.Size = new System.Drawing.Size(204, 78);
            this.entryFlagsGroupBox.TabIndex = 1;
            this.entryFlagsGroupBox.TabStop = false;
            this.entryFlagsGroupBox.Text = "Entry Flags";
            // 
            // flagsLabel
            // 
            this.flagsLabel.AutoSize = true;
            this.flagsLabel.Location = new System.Drawing.Point(7, 20);
            this.flagsLabel.Name = "flagsLabel";
            this.flagsLabel.Size = new System.Drawing.Size(35, 13);
            this.flagsLabel.TabIndex = 0;
            this.flagsLabel.Text = "Flags:";
            // 
            // currentFlagsLabel
            // 
            this.currentFlagsLabel.AutoSize = true;
            this.currentFlagsLabel.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.currentFlagsLabel.Location = new System.Drawing.Point(38, 21);
            this.currentFlagsLabel.Name = "currentFlagsLabel";
            this.currentFlagsLabel.Size = new System.Drawing.Size(163, 13);
            this.currentFlagsLabel.TabIndex = 1;
            this.currentFlagsLabel.Text = "N- V- M- X- D- I- Z- C- E-";
            // 
            // changeFlagButton
            // 
            this.changeFlagButton.Location = new System.Drawing.Point(10, 42);
            this.changeFlagButton.Name = "changeFlagButton";
            this.changeFlagButton.Size = new System.Drawing.Size(91, 23);
            this.changeFlagButton.TabIndex = 2;
            this.changeFlagButton.Text = "Change";
            this.changeFlagButton.UseVisualStyleBackColor = true;
            this.changeFlagButton.Click += new System.EventHandler(this.changeFlagButton_Click);
            // 
            // cpuGroupBox
            // 
            this.cpuGroupBox.Controls.Add(this.undocInstrCheckBox);
            this.cpuGroupBox.Controls.Add(this.cpuComboBox);
            this.cpuGroupBox.Location = new System.Drawing.Point(7, 7);
            this.cpuGroupBox.Name = "cpuGroupBox";
            this.cpuGroupBox.Size = new System.Drawing.Size(204, 78);
            this.cpuGroupBox.TabIndex = 0;
            this.cpuGroupBox.TabStop = false;
            this.cpuGroupBox.Text = "CPU";
            // 
            // undocInstrCheckBox
            // 
            this.undocInstrCheckBox.AutoSize = true;
            this.undocInstrCheckBox.Location = new System.Drawing.Point(7, 47);
            this.undocInstrCheckBox.Name = "undocInstrCheckBox";
            this.undocInstrCheckBox.Size = new System.Drawing.Size(189, 17);
            this.undocInstrCheckBox.TabIndex = 1;
            this.undocInstrCheckBox.Text = "Enable undocumented instructions";
            this.undocInstrCheckBox.UseVisualStyleBackColor = true;
            this.undocInstrCheckBox.CheckedChanged += new System.EventHandler(this.undocInstrCheckBox_CheckedChanged);
            // 
            // cpuComboBox
            // 
            this.cpuComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cpuComboBox.FormattingEnabled = true;
            this.cpuComboBox.Items.AddRange(new object[] {
            "MOS 6502",
            "WDC W65C02S",
            "WDC W65C816S"});
            this.cpuComboBox.Location = new System.Drawing.Point(6, 19);
            this.cpuComboBox.Name = "cpuComboBox";
            this.cpuComboBox.Size = new System.Drawing.Size(190, 21);
            this.cpuComboBox.TabIndex = 0;
            this.cpuComboBox.SelectedIndexChanged += new System.EventHandler(this.cpuComboBox_SelectedIndexChanged);
            // 
            // symbolsTab
            // 
            this.symbolsTab.Controls.Add(this.importSymbolsButton);
            this.symbolsTab.Controls.Add(this.editSymbolButton);
            this.symbolsTab.Controls.Add(this.removeSymbolButton);
            this.symbolsTab.Controls.Add(this.newSymbolButton);
            this.symbolsTab.Controls.Add(this.symbolsDefinedLabel);
            this.symbolsTab.Controls.Add(this.projectSymbolsListView);
            this.symbolsTab.Location = new System.Drawing.Point(4, 22);
            this.symbolsTab.Name = "symbolsTab";
            this.symbolsTab.Padding = new System.Windows.Forms.Padding(3);
            this.symbolsTab.Size = new System.Drawing.Size(614, 292);
            this.symbolsTab.TabIndex = 1;
            this.symbolsTab.Text = "Project Symbols";
            this.symbolsTab.UseVisualStyleBackColor = true;
            // 
            // importSymbolsButton
            // 
            this.importSymbolsButton.Location = new System.Drawing.Point(506, 158);
            this.importSymbolsButton.Name = "importSymbolsButton";
            this.importSymbolsButton.Size = new System.Drawing.Size(102, 23);
            this.importSymbolsButton.TabIndex = 5;
            this.importSymbolsButton.Text = "&Import...";
            this.importSymbolsButton.UseVisualStyleBackColor = true;
            this.importSymbolsButton.Click += new System.EventHandler(this.importSymbolsButton_Click);
            // 
            // editSymbolButton
            // 
            this.editSymbolButton.Location = new System.Drawing.Point(506, 52);
            this.editSymbolButton.Name = "editSymbolButton";
            this.editSymbolButton.Size = new System.Drawing.Size(102, 23);
            this.editSymbolButton.TabIndex = 3;
            this.editSymbolButton.Text = "&Edit Symbol...";
            this.editSymbolButton.UseVisualStyleBackColor = true;
            this.editSymbolButton.Click += new System.EventHandler(this.editSymbolButton_Click);
            // 
            // removeSymbolButton
            // 
            this.removeSymbolButton.Location = new System.Drawing.Point(506, 81);
            this.removeSymbolButton.Name = "removeSymbolButton";
            this.removeSymbolButton.Size = new System.Drawing.Size(102, 23);
            this.removeSymbolButton.TabIndex = 4;
            this.removeSymbolButton.Text = "&Remove";
            this.removeSymbolButton.UseVisualStyleBackColor = true;
            this.removeSymbolButton.Click += new System.EventHandler(this.removeSymbolButton_Click);
            // 
            // newSymbolButton
            // 
            this.newSymbolButton.Location = new System.Drawing.Point(506, 23);
            this.newSymbolButton.Name = "newSymbolButton";
            this.newSymbolButton.Size = new System.Drawing.Size(102, 23);
            this.newSymbolButton.TabIndex = 2;
            this.newSymbolButton.Text = "&New Symbol...";
            this.newSymbolButton.UseVisualStyleBackColor = true;
            this.newSymbolButton.Click += new System.EventHandler(this.newSymbolButton_Click);
            // 
            // symbolsDefinedLabel
            // 
            this.symbolsDefinedLabel.AutoSize = true;
            this.symbolsDefinedLabel.Location = new System.Drawing.Point(7, 7);
            this.symbolsDefinedLabel.Name = "symbolsDefinedLabel";
            this.symbolsDefinedLabel.Size = new System.Drawing.Size(133, 13);
            this.symbolsDefinedLabel.TabIndex = 0;
            this.symbolsDefinedLabel.Text = "Symbols defined in project:";
            // 
            // projectSymbolsListView
            // 
            this.projectSymbolsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumnHeader,
            this.valueColumnHeader,
            this.typeColumnHeader,
            this.commentColumnHeader});
            this.projectSymbolsListView.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.projectSymbolsListView.FullRowSelect = true;
            this.projectSymbolsListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.projectSymbolsListView.HideSelection = false;
            this.projectSymbolsListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1});
            this.projectSymbolsListView.Location = new System.Drawing.Point(11, 23);
            this.projectSymbolsListView.MultiSelect = false;
            this.projectSymbolsListView.Name = "projectSymbolsListView";
            this.projectSymbolsListView.Size = new System.Drawing.Size(489, 259);
            this.projectSymbolsListView.TabIndex = 1;
            this.projectSymbolsListView.UseCompatibleStateImageBehavior = false;
            this.projectSymbolsListView.View = System.Windows.Forms.View.Details;
            this.projectSymbolsListView.SelectedIndexChanged += new System.EventHandler(this.projectSymbolsListView_SelectedIndexChanged);
            this.projectSymbolsListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.projectSymbolsListView_MouseDoubleClick);
            // 
            // nameColumnHeader
            // 
            this.nameColumnHeader.Text = "Name";
            this.nameColumnHeader.Width = 109;
            // 
            // valueColumnHeader
            // 
            this.valueColumnHeader.Text = "Value";
            this.valueColumnHeader.Width = 69;
            // 
            // typeColumnHeader
            // 
            this.typeColumnHeader.Text = "Type";
            this.typeColumnHeader.Width = 42;
            // 
            // commentColumnHeader
            // 
            this.commentColumnHeader.Text = "Comment";
            this.commentColumnHeader.Width = 264;
            // 
            // symbolFilesTab
            // 
            this.symbolFilesTab.Controls.Add(this.symbolFileDownButton);
            this.symbolFilesTab.Controls.Add(this.symbolFileUpButton);
            this.symbolFilesTab.Controls.Add(this.addSymbolFilesButton);
            this.symbolFilesTab.Controls.Add(this.symbolFileRemoveButton);
            this.symbolFilesTab.Controls.Add(this.symbolFilesListBox);
            this.symbolFilesTab.Controls.Add(this.configuredFilesLabel);
            this.symbolFilesTab.Location = new System.Drawing.Point(4, 22);
            this.symbolFilesTab.Name = "symbolFilesTab";
            this.symbolFilesTab.Padding = new System.Windows.Forms.Padding(3);
            this.symbolFilesTab.Size = new System.Drawing.Size(614, 292);
            this.symbolFilesTab.TabIndex = 2;
            this.symbolFilesTab.Text = "Symbol Files";
            this.symbolFilesTab.UseVisualStyleBackColor = true;
            // 
            // symbolFileDownButton
            // 
            this.symbolFileDownButton.Location = new System.Drawing.Point(319, 57);
            this.symbolFileDownButton.Name = "symbolFileDownButton";
            this.symbolFileDownButton.Size = new System.Drawing.Size(75, 23);
            this.symbolFileDownButton.TabIndex = 4;
            this.symbolFileDownButton.Text = "Down";
            this.symbolFileDownButton.UseVisualStyleBackColor = true;
            this.symbolFileDownButton.Click += new System.EventHandler(this.symbolFileDownButton_Click);
            // 
            // symbolFileUpButton
            // 
            this.symbolFileUpButton.Location = new System.Drawing.Point(319, 28);
            this.symbolFileUpButton.Name = "symbolFileUpButton";
            this.symbolFileUpButton.Size = new System.Drawing.Size(75, 23);
            this.symbolFileUpButton.TabIndex = 3;
            this.symbolFileUpButton.Text = "Up";
            this.symbolFileUpButton.UseVisualStyleBackColor = true;
            this.symbolFileUpButton.Click += new System.EventHandler(this.symbolFileUpButton_Click);
            // 
            // addSymbolFilesButton
            // 
            this.addSymbolFilesButton.Location = new System.Drawing.Point(8, 249);
            this.addSymbolFilesButton.Name = "addSymbolFilesButton";
            this.addSymbolFilesButton.Size = new System.Drawing.Size(134, 23);
            this.addSymbolFilesButton.TabIndex = 2;
            this.addSymbolFilesButton.Text = "Add Symbol Files...";
            this.addSymbolFilesButton.UseVisualStyleBackColor = true;
            this.addSymbolFilesButton.Click += new System.EventHandler(this.addSymbolFilesButton_Click);
            // 
            // symbolFileRemoveButton
            // 
            this.symbolFileRemoveButton.Location = new System.Drawing.Point(319, 97);
            this.symbolFileRemoveButton.Name = "symbolFileRemoveButton";
            this.symbolFileRemoveButton.Size = new System.Drawing.Size(75, 23);
            this.symbolFileRemoveButton.TabIndex = 5;
            this.symbolFileRemoveButton.Text = "Remove";
            this.symbolFileRemoveButton.UseVisualStyleBackColor = true;
            this.symbolFileRemoveButton.Click += new System.EventHandler(this.symbolFileRemoveButton_Click);
            // 
            // symbolFilesListBox
            // 
            this.symbolFilesListBox.FormattingEnabled = true;
            this.symbolFilesListBox.Location = new System.Drawing.Point(8, 28);
            this.symbolFilesListBox.Name = "symbolFilesListBox";
            this.symbolFilesListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.symbolFilesListBox.Size = new System.Drawing.Size(305, 212);
            this.symbolFilesListBox.TabIndex = 1;
            this.symbolFilesListBox.SelectedIndexChanged += new System.EventHandler(this.symbolFilesListBox_SelectedIndexChanged);
            // 
            // configuredFilesLabel
            // 
            this.configuredFilesLabel.AutoSize = true;
            this.configuredFilesLabel.Location = new System.Drawing.Point(7, 7);
            this.configuredFilesLabel.Name = "configuredFilesLabel";
            this.configuredFilesLabel.Size = new System.Drawing.Size(160, 13);
            this.configuredFilesLabel.TabIndex = 0;
            this.configuredFilesLabel.Text = "Currently configured symbol files:";
            // 
            // extensionScriptsTab
            // 
            this.extensionScriptsTab.Controls.Add(this.extensionScriptRemoveButton);
            this.extensionScriptsTab.Controls.Add(this.addExtensionScriptsButton);
            this.extensionScriptsTab.Controls.Add(this.extensionScriptsListBox);
            this.extensionScriptsTab.Controls.Add(this.configuredScriptsLabel);
            this.extensionScriptsTab.Location = new System.Drawing.Point(4, 22);
            this.extensionScriptsTab.Name = "extensionScriptsTab";
            this.extensionScriptsTab.Padding = new System.Windows.Forms.Padding(3);
            this.extensionScriptsTab.Size = new System.Drawing.Size(614, 292);
            this.extensionScriptsTab.TabIndex = 3;
            this.extensionScriptsTab.Text = "Extension Scripts";
            this.extensionScriptsTab.UseVisualStyleBackColor = true;
            // 
            // extensionScriptRemoveButton
            // 
            this.extensionScriptRemoveButton.Location = new System.Drawing.Point(320, 28);
            this.extensionScriptRemoveButton.Name = "extensionScriptRemoveButton";
            this.extensionScriptRemoveButton.Size = new System.Drawing.Size(75, 23);
            this.extensionScriptRemoveButton.TabIndex = 3;
            this.extensionScriptRemoveButton.Text = "Remove";
            this.extensionScriptRemoveButton.UseVisualStyleBackColor = true;
            this.extensionScriptRemoveButton.Click += new System.EventHandler(this.extensionScriptRemoveButton_Click);
            // 
            // addExtensionScriptsButton
            // 
            this.addExtensionScriptsButton.Location = new System.Drawing.Point(8, 249);
            this.addExtensionScriptsButton.Name = "addExtensionScriptsButton";
            this.addExtensionScriptsButton.Size = new System.Drawing.Size(134, 23);
            this.addExtensionScriptsButton.TabIndex = 2;
            this.addExtensionScriptsButton.Text = "Add Scripts...";
            this.addExtensionScriptsButton.UseVisualStyleBackColor = true;
            this.addExtensionScriptsButton.Click += new System.EventHandler(this.addExtensionScriptsButton_Click);
            // 
            // extensionScriptsListBox
            // 
            this.extensionScriptsListBox.FormattingEnabled = true;
            this.extensionScriptsListBox.Location = new System.Drawing.Point(8, 28);
            this.extensionScriptsListBox.Name = "extensionScriptsListBox";
            this.extensionScriptsListBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.extensionScriptsListBox.Size = new System.Drawing.Size(305, 212);
            this.extensionScriptsListBox.TabIndex = 1;
            this.extensionScriptsListBox.SelectedIndexChanged += new System.EventHandler(this.extensionScriptsListBox_SelectedIndexChanged);
            // 
            // configuredScriptsLabel
            // 
            this.configuredScriptsLabel.AutoSize = true;
            this.configuredScriptsLabel.Location = new System.Drawing.Point(7, 7);
            this.configuredScriptsLabel.Name = "configuredScriptsLabel";
            this.configuredScriptsLabel.Size = new System.Drawing.Size(185, 13);
            this.configuredScriptsLabel.TabIndex = 0;
            this.configuredScriptsLabel.Text = "Currently configured extension scripts:";
            // 
            // labelUndoRedoNote
            // 
            this.labelUndoRedoNote.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelUndoRedoNote.AutoSize = true;
            this.labelUndoRedoNote.Location = new System.Drawing.Point(12, 331);
            this.labelUndoRedoNote.Name = "labelUndoRedoNote";
            this.labelUndoRedoNote.Size = new System.Drawing.Size(248, 13);
            this.labelUndoRedoNote.TabIndex = 1;
            this.labelUndoRedoNote.Text = "NOTE: changes are added to the undo/redo buffer";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(537, 326);
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
            this.okButton.Location = new System.Drawing.Point(456, 326);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // applyButton
            // 
            this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.applyButton.Location = new System.Drawing.Point(354, 326);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(75, 23);
            this.applyButton.TabIndex = 2;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
            // 
            // EditProjectProperties
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(624, 361);
            this.Controls.Add(this.applyButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.labelUndoRedoNote);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditProjectProperties";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Project Properties";
            this.Load += new System.EventHandler(this.EditProperties_Load);
            this.tabControl1.ResumeLayout(false);
            this.generalTab.ResumeLayout(false);
            this.analysisGroupBox.ResumeLayout(false);
            this.analysisGroupBox.PerformLayout();
            this.entryFlagsGroupBox.ResumeLayout(false);
            this.entryFlagsGroupBox.PerformLayout();
            this.cpuGroupBox.ResumeLayout(false);
            this.cpuGroupBox.PerformLayout();
            this.symbolsTab.ResumeLayout(false);
            this.symbolsTab.PerformLayout();
            this.symbolFilesTab.ResumeLayout(false);
            this.symbolFilesTab.PerformLayout();
            this.extensionScriptsTab.ResumeLayout(false);
            this.extensionScriptsTab.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage generalTab;
        private System.Windows.Forms.ComboBox cpuComboBox;
        private System.Windows.Forms.TabPage symbolsTab;
        private System.Windows.Forms.Label labelUndoRedoNote;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.Label currentFlagsLabel;
        private System.Windows.Forms.Button changeFlagButton;
        private System.Windows.Forms.Label symbolsDefinedLabel;
        private System.Windows.Forms.ListView projectSymbolsListView;
        private System.Windows.Forms.ColumnHeader nameColumnHeader;
        private System.Windows.Forms.ColumnHeader valueColumnHeader;
        private System.Windows.Forms.ColumnHeader typeColumnHeader;
        private System.Windows.Forms.TabPage symbolFilesTab;
        private System.Windows.Forms.Button addSymbolFilesButton;
        private System.Windows.Forms.Button symbolFileRemoveButton;
        private System.Windows.Forms.ListBox symbolFilesListBox;
        private System.Windows.Forms.Label configuredFilesLabel;
        private System.Windows.Forms.Button removeSymbolButton;
        private System.Windows.Forms.Button newSymbolButton;
        private System.Windows.Forms.ColumnHeader commentColumnHeader;
        private System.Windows.Forms.GroupBox entryFlagsGroupBox;
        private System.Windows.Forms.Label flagsLabel;
        private System.Windows.Forms.GroupBox cpuGroupBox;
        private System.Windows.Forms.CheckBox undocInstrCheckBox;
        private System.Windows.Forms.Button editSymbolButton;
        private System.Windows.Forms.Button symbolFileDownButton;
        private System.Windows.Forms.Button symbolFileUpButton;
        private System.Windows.Forms.GroupBox analysisGroupBox;
        private System.Windows.Forms.ComboBox minStringCharsComboBox;
        private System.Windows.Forms.Label minCharsForStringLabel;
        private System.Windows.Forms.CheckBox analyzeUncategorizedCheckBox;
        private System.Windows.Forms.TabPage extensionScriptsTab;
        private System.Windows.Forms.Button extensionScriptRemoveButton;
        private System.Windows.Forms.Button addExtensionScriptsButton;
        private System.Windows.Forms.ListBox extensionScriptsListBox;
        private System.Windows.Forms.Label configuredScriptsLabel;
        private System.Windows.Forms.Button importSymbolsButton;
        private System.Windows.Forms.CheckBox seekAltTargetCheckBox;
    }
}