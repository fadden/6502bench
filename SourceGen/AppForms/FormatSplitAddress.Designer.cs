namespace SourceGen.AppForms {
    partial class FormatSplitAddress {
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
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new string[] {
            "12/3456",
            "+123456",
            "(+) T_123456"}, -1);
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.selectionInfoLabel = new System.Windows.Forms.Label();
            this.addressCharacteristicsGroup = new System.Windows.Forms.GroupBox();
            this.pushRtsCheckBox = new System.Windows.Forms.CheckBox();
            this.width24Radio = new System.Windows.Forms.RadioButton();
            this.width16Radio = new System.Windows.Forms.RadioButton();
            this.lowByteGroupBox = new System.Windows.Forms.GroupBox();
            this.lowThirdPartRadio = new System.Windows.Forms.RadioButton();
            this.lowSecondPartRadio = new System.Windows.Forms.RadioButton();
            this.lowFirstPartRadio = new System.Windows.Forms.RadioButton();
            this.highByteGroupBox = new System.Windows.Forms.GroupBox();
            this.highConstantTextBox = new System.Windows.Forms.TextBox();
            this.highConstantRadio = new System.Windows.Forms.RadioButton();
            this.highThirdPartRadio = new System.Windows.Forms.RadioButton();
            this.highSecondPartRadio = new System.Windows.Forms.RadioButton();
            this.highFirstPartRadio = new System.Windows.Forms.RadioButton();
            this.bankByteGroupBox = new System.Windows.Forms.GroupBox();
            this.bankConstantTextBox = new System.Windows.Forms.TextBox();
            this.bankNthPartRadio = new System.Windows.Forms.RadioButton();
            this.bankConstantRadio = new System.Windows.Forms.RadioButton();
            this.outputPreviewListView = new System.Windows.Forms.ListView();
            this.addrColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.symbolColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.outputPreviewGroupBox = new System.Windows.Forms.GroupBox();
            this.invalidConstantLabel = new System.Windows.Forms.Label();
            this.incompatibleSelectionLabel = new System.Windows.Forms.Label();
            this.addCodeHintCheckBox = new System.Windows.Forms.CheckBox();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.offsetColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.addressCharacteristicsGroup.SuspendLayout();
            this.lowByteGroupBox.SuspendLayout();
            this.highByteGroupBox.SuspendLayout();
            this.bankByteGroupBox.SuspendLayout();
            this.outputPreviewGroupBox.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(461, 461);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(380, 461);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // selectionInfoLabel
            // 
            this.selectionInfoLabel.AutoSize = true;
            this.selectionInfoLabel.Location = new System.Drawing.Point(13, 13);
            this.selectionInfoLabel.Name = "selectionInfoLabel";
            this.selectionInfoLabel.Size = new System.Drawing.Size(213, 13);
            this.selectionInfoLabel.TabIndex = 0;
            this.selectionInfoLabel.Text = "There are {0} bytes selected, in {1} group(s)";
            // 
            // addressCharacteristicsGroup
            // 
            this.addressCharacteristicsGroup.Controls.Add(this.pushRtsCheckBox);
            this.addressCharacteristicsGroup.Controls.Add(this.width24Radio);
            this.addressCharacteristicsGroup.Controls.Add(this.width16Radio);
            this.addressCharacteristicsGroup.Location = new System.Drawing.Point(16, 40);
            this.addressCharacteristicsGroup.Name = "addressCharacteristicsGroup";
            this.addressCharacteristicsGroup.Size = new System.Drawing.Size(204, 90);
            this.addressCharacteristicsGroup.TabIndex = 1;
            this.addressCharacteristicsGroup.TabStop = false;
            this.addressCharacteristicsGroup.Text = "Address Characteristics";
            // 
            // pushRtsCheckBox
            // 
            this.pushRtsCheckBox.AutoSize = true;
            this.pushRtsCheckBox.Location = new System.Drawing.Point(7, 68);
            this.pushRtsCheckBox.Name = "pushRtsCheckBox";
            this.pushRtsCheckBox.Size = new System.Drawing.Size(167, 17);
            this.pushRtsCheckBox.TabIndex = 2;
            this.pushRtsCheckBox.Text = "Push for RTS/RTL (target - 1)";
            this.pushRtsCheckBox.UseVisualStyleBackColor = true;
            this.pushRtsCheckBox.CheckedChanged += new System.EventHandler(this.pushRtsCheckBox_CheckedChanged);
            // 
            // width24Radio
            // 
            this.width24Radio.AutoSize = true;
            this.width24Radio.Location = new System.Drawing.Point(7, 44);
            this.width24Radio.Name = "width24Radio";
            this.width24Radio.Size = new System.Drawing.Size(51, 17);
            this.width24Radio.TabIndex = 1;
            this.width24Radio.TabStop = true;
            this.width24Radio.Text = "24-bit";
            this.width24Radio.UseVisualStyleBackColor = true;
            this.width24Radio.CheckedChanged += new System.EventHandler(this.widthRadio_CheckedChanged);
            // 
            // width16Radio
            // 
            this.width16Radio.AutoSize = true;
            this.width16Radio.Location = new System.Drawing.Point(7, 20);
            this.width16Radio.Name = "width16Radio";
            this.width16Radio.Size = new System.Drawing.Size(51, 17);
            this.width16Radio.TabIndex = 0;
            this.width16Radio.TabStop = true;
            this.width16Radio.Text = "16-bit";
            this.width16Radio.UseVisualStyleBackColor = true;
            this.width16Radio.CheckedChanged += new System.EventHandler(this.widthRadio_CheckedChanged);
            // 
            // lowByteGroupBox
            // 
            this.lowByteGroupBox.Controls.Add(this.lowThirdPartRadio);
            this.lowByteGroupBox.Controls.Add(this.lowSecondPartRadio);
            this.lowByteGroupBox.Controls.Add(this.lowFirstPartRadio);
            this.lowByteGroupBox.Location = new System.Drawing.Point(13, 136);
            this.lowByteGroupBox.Name = "lowByteGroupBox";
            this.lowByteGroupBox.Size = new System.Drawing.Size(207, 94);
            this.lowByteGroupBox.TabIndex = 2;
            this.lowByteGroupBox.TabStop = false;
            this.lowByteGroupBox.Text = "Low Byte";
            // 
            // lowThirdPartRadio
            // 
            this.lowThirdPartRadio.AutoSize = true;
            this.lowThirdPartRadio.Location = new System.Drawing.Point(10, 68);
            this.lowThirdPartRadio.Name = "lowThirdPartRadio";
            this.lowThirdPartRadio.Size = new System.Drawing.Size(127, 17);
            this.lowThirdPartRadio.TabIndex = 2;
            this.lowThirdPartRadio.TabStop = true;
            this.lowThirdPartRadio.Text = "Third part of selection";
            this.lowThirdPartRadio.UseVisualStyleBackColor = true;
            this.lowThirdPartRadio.CheckedChanged += new System.EventHandler(this.lowByte_CheckedChanged);
            // 
            // lowSecondPartRadio
            // 
            this.lowSecondPartRadio.AutoSize = true;
            this.lowSecondPartRadio.Location = new System.Drawing.Point(10, 44);
            this.lowSecondPartRadio.Name = "lowSecondPartRadio";
            this.lowSecondPartRadio.Size = new System.Drawing.Size(140, 17);
            this.lowSecondPartRadio.TabIndex = 1;
            this.lowSecondPartRadio.TabStop = true;
            this.lowSecondPartRadio.Text = "Second part of selection";
            this.lowSecondPartRadio.UseVisualStyleBackColor = true;
            this.lowSecondPartRadio.CheckedChanged += new System.EventHandler(this.lowByte_CheckedChanged);
            // 
            // lowFirstPartRadio
            // 
            this.lowFirstPartRadio.AutoSize = true;
            this.lowFirstPartRadio.Location = new System.Drawing.Point(10, 20);
            this.lowFirstPartRadio.Name = "lowFirstPartRadio";
            this.lowFirstPartRadio.Size = new System.Drawing.Size(122, 17);
            this.lowFirstPartRadio.TabIndex = 0;
            this.lowFirstPartRadio.TabStop = true;
            this.lowFirstPartRadio.Text = "First part of selection";
            this.lowFirstPartRadio.UseVisualStyleBackColor = true;
            this.lowFirstPartRadio.CheckedChanged += new System.EventHandler(this.lowByte_CheckedChanged);
            // 
            // highByteGroupBox
            // 
            this.highByteGroupBox.Controls.Add(this.highConstantTextBox);
            this.highByteGroupBox.Controls.Add(this.highConstantRadio);
            this.highByteGroupBox.Controls.Add(this.highThirdPartRadio);
            this.highByteGroupBox.Controls.Add(this.highSecondPartRadio);
            this.highByteGroupBox.Controls.Add(this.highFirstPartRadio);
            this.highByteGroupBox.Location = new System.Drawing.Point(13, 236);
            this.highByteGroupBox.Name = "highByteGroupBox";
            this.highByteGroupBox.Size = new System.Drawing.Size(207, 120);
            this.highByteGroupBox.TabIndex = 3;
            this.highByteGroupBox.TabStop = false;
            this.highByteGroupBox.Text = "High Byte";
            // 
            // highConstantTextBox
            // 
            this.highConstantTextBox.Location = new System.Drawing.Point(87, 91);
            this.highConstantTextBox.MaxLength = 10;
            this.highConstantTextBox.Name = "highConstantTextBox";
            this.highConstantTextBox.Size = new System.Drawing.Size(93, 20);
            this.highConstantTextBox.TabIndex = 4;
            this.highConstantTextBox.TextChanged += new System.EventHandler(this.highConstantTextBox_TextChanged);
            // 
            // highConstantRadio
            // 
            this.highConstantRadio.AutoSize = true;
            this.highConstantRadio.Location = new System.Drawing.Point(10, 92);
            this.highConstantRadio.Name = "highConstantRadio";
            this.highConstantRadio.Size = new System.Drawing.Size(70, 17);
            this.highConstantRadio.TabIndex = 3;
            this.highConstantRadio.TabStop = true;
            this.highConstantRadio.Text = "Constant:";
            this.highConstantRadio.UseVisualStyleBackColor = true;
            this.highConstantRadio.CheckedChanged += new System.EventHandler(this.highByte_CheckedChanged);
            // 
            // highThirdPartRadio
            // 
            this.highThirdPartRadio.AutoSize = true;
            this.highThirdPartRadio.Location = new System.Drawing.Point(10, 68);
            this.highThirdPartRadio.Name = "highThirdPartRadio";
            this.highThirdPartRadio.Size = new System.Drawing.Size(127, 17);
            this.highThirdPartRadio.TabIndex = 2;
            this.highThirdPartRadio.TabStop = true;
            this.highThirdPartRadio.Text = "Third part of selection";
            this.highThirdPartRadio.UseVisualStyleBackColor = true;
            this.highThirdPartRadio.CheckedChanged += new System.EventHandler(this.highByte_CheckedChanged);
            // 
            // highSecondPartRadio
            // 
            this.highSecondPartRadio.AutoSize = true;
            this.highSecondPartRadio.Location = new System.Drawing.Point(10, 44);
            this.highSecondPartRadio.Name = "highSecondPartRadio";
            this.highSecondPartRadio.Size = new System.Drawing.Size(140, 17);
            this.highSecondPartRadio.TabIndex = 1;
            this.highSecondPartRadio.TabStop = true;
            this.highSecondPartRadio.Text = "Second part of selection";
            this.highSecondPartRadio.UseVisualStyleBackColor = true;
            this.highSecondPartRadio.CheckedChanged += new System.EventHandler(this.highByte_CheckedChanged);
            // 
            // highFirstPartRadio
            // 
            this.highFirstPartRadio.AutoSize = true;
            this.highFirstPartRadio.Location = new System.Drawing.Point(10, 20);
            this.highFirstPartRadio.Name = "highFirstPartRadio";
            this.highFirstPartRadio.Size = new System.Drawing.Size(122, 17);
            this.highFirstPartRadio.TabIndex = 0;
            this.highFirstPartRadio.TabStop = true;
            this.highFirstPartRadio.Text = "First part of selection";
            this.highFirstPartRadio.UseVisualStyleBackColor = true;
            this.highFirstPartRadio.CheckedChanged += new System.EventHandler(this.highByte_CheckedChanged);
            // 
            // bankByteGroupBox
            // 
            this.bankByteGroupBox.Controls.Add(this.bankConstantTextBox);
            this.bankByteGroupBox.Controls.Add(this.bankNthPartRadio);
            this.bankByteGroupBox.Controls.Add(this.bankConstantRadio);
            this.bankByteGroupBox.Location = new System.Drawing.Point(13, 362);
            this.bankByteGroupBox.Name = "bankByteGroupBox";
            this.bankByteGroupBox.Size = new System.Drawing.Size(207, 70);
            this.bankByteGroupBox.TabIndex = 4;
            this.bankByteGroupBox.TabStop = false;
            this.bankByteGroupBox.Text = "Bank Byte";
            // 
            // bankConstantTextBox
            // 
            this.bankConstantTextBox.Location = new System.Drawing.Point(87, 41);
            this.bankConstantTextBox.MaxLength = 10;
            this.bankConstantTextBox.Name = "bankConstantTextBox";
            this.bankConstantTextBox.Size = new System.Drawing.Size(93, 20);
            this.bankConstantTextBox.TabIndex = 2;
            this.bankConstantTextBox.TextChanged += new System.EventHandler(this.bankConstantTextBox_TextChanged);
            // 
            // bankNthPartRadio
            // 
            this.bankNthPartRadio.AutoSize = true;
            this.bankNthPartRadio.Location = new System.Drawing.Point(10, 19);
            this.bankNthPartRadio.Name = "bankNthPartRadio";
            this.bankNthPartRadio.Size = new System.Drawing.Size(120, 17);
            this.bankNthPartRadio.TabIndex = 0;
            this.bankNthPartRadio.TabStop = true;
            this.bankNthPartRadio.Text = "Nth part of selection";
            this.bankNthPartRadio.UseVisualStyleBackColor = true;
            this.bankNthPartRadio.CheckedChanged += new System.EventHandler(this.bankByte_CheckedChanged);
            // 
            // bankConstantRadio
            // 
            this.bankConstantRadio.AutoSize = true;
            this.bankConstantRadio.Location = new System.Drawing.Point(10, 42);
            this.bankConstantRadio.Name = "bankConstantRadio";
            this.bankConstantRadio.Size = new System.Drawing.Size(70, 17);
            this.bankConstantRadio.TabIndex = 1;
            this.bankConstantRadio.TabStop = true;
            this.bankConstantRadio.Text = "Constant:";
            this.bankConstantRadio.UseVisualStyleBackColor = true;
            this.bankConstantRadio.CheckedChanged += new System.EventHandler(this.bankByte_CheckedChanged);
            // 
            // outputPreviewListView
            // 
            this.outputPreviewListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPreviewListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.addrColumnHeader,
            this.offsetColumnHeader,
            this.symbolColumnHeader});
            this.outputPreviewListView.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outputPreviewListView.FullRowSelect = true;
            this.outputPreviewListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.outputPreviewListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem5});
            this.outputPreviewListView.Location = new System.Drawing.Point(6, 19);
            this.outputPreviewListView.MultiSelect = false;
            this.outputPreviewListView.Name = "outputPreviewListView";
            this.outputPreviewListView.Size = new System.Drawing.Size(290, 369);
            this.outputPreviewListView.TabIndex = 0;
            this.outputPreviewListView.UseCompatibleStateImageBehavior = false;
            this.outputPreviewListView.View = System.Windows.Forms.View.Details;
            // 
            // addrColumnHeader
            // 
            this.addrColumnHeader.Text = "Addr";
            this.addrColumnHeader.Width = 52;
            // 
            // symbolColumnHeader
            // 
            this.symbolColumnHeader.Text = "Symbol";
            this.symbolColumnHeader.Width = 174;
            // 
            // outputPreviewGroupBox
            // 
            this.outputPreviewGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.outputPreviewGroupBox.Controls.Add(this.invalidConstantLabel);
            this.outputPreviewGroupBox.Controls.Add(this.incompatibleSelectionLabel);
            this.outputPreviewGroupBox.Controls.Add(this.outputPreviewListView);
            this.outputPreviewGroupBox.Location = new System.Drawing.Point(234, 40);
            this.outputPreviewGroupBox.Name = "outputPreviewGroupBox";
            this.outputPreviewGroupBox.Size = new System.Drawing.Size(302, 393);
            this.outputPreviewGroupBox.TabIndex = 6;
            this.outputPreviewGroupBox.TabStop = false;
            this.outputPreviewGroupBox.Text = "Generated Addresses";
            // 
            // invalidConstantLabel
            // 
            this.invalidConstantLabel.AutoSize = true;
            this.invalidConstantLabel.BackColor = System.Drawing.SystemColors.Window;
            this.invalidConstantLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.invalidConstantLabel.ForeColor = System.Drawing.Color.Red;
            this.invalidConstantLabel.Location = new System.Drawing.Point(101, 200);
            this.invalidConstantLabel.Name = "invalidConstantLabel";
            this.invalidConstantLabel.Size = new System.Drawing.Size(100, 16);
            this.invalidConstantLabel.TabIndex = 2;
            this.invalidConstantLabel.Text = "Invalid constant";
            // 
            // incompatibleSelectionLabel
            // 
            this.incompatibleSelectionLabel.AutoSize = true;
            this.incompatibleSelectionLabel.BackColor = System.Drawing.SystemColors.Window;
            this.incompatibleSelectionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.incompatibleSelectionLabel.ForeColor = System.Drawing.Color.Red;
            this.incompatibleSelectionLabel.Location = new System.Drawing.Point(43, 177);
            this.incompatibleSelectionLabel.Name = "incompatibleSelectionLabel";
            this.incompatibleSelectionLabel.Size = new System.Drawing.Size(216, 16);
            this.incompatibleSelectionLabel.TabIndex = 1;
            this.incompatibleSelectionLabel.Text = "Options incompatible with selection";
            // 
            // addCodeHintCheckBox
            // 
            this.addCodeHintCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.addCodeHintCheckBox.AutoSize = true;
            this.addCodeHintCheckBox.Location = new System.Drawing.Point(10, 19);
            this.addCodeHintCheckBox.Name = "addCodeHintCheckBox";
            this.addCodeHintCheckBox.Size = new System.Drawing.Size(165, 17);
            this.addCodeHintCheckBox.TabIndex = 0;
            this.addCodeHintCheckBox.Text = "Add code entry hint if needed";
            this.addCodeHintCheckBox.UseVisualStyleBackColor = true;
            // 
            // optionsGroupBox
            // 
            this.optionsGroupBox.Controls.Add(this.addCodeHintCheckBox);
            this.optionsGroupBox.Location = new System.Drawing.Point(13, 439);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(207, 43);
            this.optionsGroupBox.TabIndex = 5;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            // 
            // offsetColumnHeader
            // 
            this.offsetColumnHeader.Text = "Offset";
            this.offsetColumnHeader.Width = 55;
            // 
            // FormatSplitAddress
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(548, 496);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.outputPreviewGroupBox);
            this.Controls.Add(this.bankByteGroupBox);
            this.Controls.Add(this.highByteGroupBox);
            this.Controls.Add(this.lowByteGroupBox);
            this.Controls.Add(this.addressCharacteristicsGroup);
            this.Controls.Add(this.selectionInfoLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormatSplitAddress";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Format Split-Address Table";
            this.Load += new System.EventHandler(this.FormatSplitAddress_Load);
            this.addressCharacteristicsGroup.ResumeLayout(false);
            this.addressCharacteristicsGroup.PerformLayout();
            this.lowByteGroupBox.ResumeLayout(false);
            this.lowByteGroupBox.PerformLayout();
            this.highByteGroupBox.ResumeLayout(false);
            this.highByteGroupBox.PerformLayout();
            this.bankByteGroupBox.ResumeLayout(false);
            this.bankByteGroupBox.PerformLayout();
            this.outputPreviewGroupBox.ResumeLayout(false);
            this.outputPreviewGroupBox.PerformLayout();
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label selectionInfoLabel;
        private System.Windows.Forms.GroupBox addressCharacteristicsGroup;
        private System.Windows.Forms.RadioButton width16Radio;
        private System.Windows.Forms.RadioButton width24Radio;
        private System.Windows.Forms.CheckBox pushRtsCheckBox;
        private System.Windows.Forms.GroupBox lowByteGroupBox;
        private System.Windows.Forms.RadioButton lowFirstPartRadio;
        private System.Windows.Forms.RadioButton lowSecondPartRadio;
        private System.Windows.Forms.RadioButton lowThirdPartRadio;
        private System.Windows.Forms.GroupBox highByteGroupBox;
        private System.Windows.Forms.RadioButton highFirstPartRadio;
        private System.Windows.Forms.RadioButton highSecondPartRadio;
        private System.Windows.Forms.RadioButton highThirdPartRadio;
        private System.Windows.Forms.RadioButton highConstantRadio;
        private System.Windows.Forms.TextBox highConstantTextBox;
        private System.Windows.Forms.GroupBox bankByteGroupBox;
        private System.Windows.Forms.ListView outputPreviewListView;
        private System.Windows.Forms.ColumnHeader addrColumnHeader;
        private System.Windows.Forms.ColumnHeader symbolColumnHeader;
        private System.Windows.Forms.RadioButton bankConstantRadio;
        private System.Windows.Forms.RadioButton bankNthPartRadio;
        private System.Windows.Forms.TextBox bankConstantTextBox;
        private System.Windows.Forms.GroupBox outputPreviewGroupBox;
        private System.Windows.Forms.CheckBox addCodeHintCheckBox;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.Label incompatibleSelectionLabel;
        private System.Windows.Forms.Label invalidConstantLabel;
        private System.Windows.Forms.ColumnHeader offsetColumnHeader;
    }
}