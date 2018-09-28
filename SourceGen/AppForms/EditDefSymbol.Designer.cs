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
    partial class EditDefSymbol {
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
            this.labelLabel = new System.Windows.Forms.Label();
            this.labelTextBox = new System.Windows.Forms.TextBox();
            this.valueTextBox = new System.Windows.Forms.TextBox();
            this.labelNotesLabel = new System.Windows.Forms.Label();
            this.valueLabel = new System.Windows.Forms.Label();
            this.valueNotesLabel = new System.Windows.Forms.Label();
            this.commentLabel = new System.Windows.Forms.Label();
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.commentNotesLabel = new System.Windows.Forms.Label();
            this.symbolTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.constantRadioButton = new System.Windows.Forms.RadioButton();
            this.addressRadioButton = new System.Windows.Forms.RadioButton();
            this.labelUniqueLabel = new System.Windows.Forms.Label();
            this.symbolTypeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(203, 253);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(122, 253);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // labelLabel
            // 
            this.labelLabel.AutoSize = true;
            this.labelLabel.Location = new System.Drawing.Point(14, 16);
            this.labelLabel.Name = "labelLabel";
            this.labelLabel.Size = new System.Drawing.Size(36, 13);
            this.labelLabel.TabIndex = 6;
            this.labelLabel.Text = "Label:";
            // 
            // labelTextBox
            // 
            this.labelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTextBox.Location = new System.Drawing.Point(77, 14);
            this.labelTextBox.Name = "labelTextBox";
            this.labelTextBox.Size = new System.Drawing.Size(200, 20);
            this.labelTextBox.TabIndex = 0;
            this.labelTextBox.TextChanged += new System.EventHandler(this.labelTextBox_TextChanged);
            // 
            // valueTextBox
            // 
            this.valueTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.valueTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.valueTextBox.Location = new System.Drawing.Point(77, 81);
            this.valueTextBox.Name = "valueTextBox";
            this.valueTextBox.Size = new System.Drawing.Size(200, 20);
            this.valueTextBox.TabIndex = 1;
            this.valueTextBox.TextChanged += new System.EventHandler(this.valueTextBox_TextChanged);
            // 
            // labelNotesLabel
            // 
            this.labelNotesLabel.AutoSize = true;
            this.labelNotesLabel.Location = new System.Drawing.Point(74, 37);
            this.labelNotesLabel.Name = "labelNotesLabel";
            this.labelNotesLabel.Size = new System.Drawing.Size(187, 13);
            this.labelNotesLabel.TabIndex = 9;
            this.labelNotesLabel.Text = "• 2+ alphanumerics, starting with letter";
            // 
            // valueLabel
            // 
            this.valueLabel.AutoSize = true;
            this.valueLabel.Location = new System.Drawing.Point(14, 83);
            this.valueLabel.Name = "valueLabel";
            this.valueLabel.Size = new System.Drawing.Size(37, 13);
            this.valueLabel.TabIndex = 7;
            this.valueLabel.Text = "Value:";
            // 
            // valueNotesLabel
            // 
            this.valueNotesLabel.AutoSize = true;
            this.valueNotesLabel.Location = new System.Drawing.Point(74, 104);
            this.valueNotesLabel.Name = "valueNotesLabel";
            this.valueNotesLabel.Size = new System.Drawing.Size(155, 13);
            this.valueNotesLabel.TabIndex = 11;
            this.valueNotesLabel.Text = "• Decimal, hex ($), or binary (%)";
            // 
            // commentLabel
            // 
            this.commentLabel.AutoSize = true;
            this.commentLabel.Location = new System.Drawing.Point(17, 137);
            this.commentLabel.Name = "commentLabel";
            this.commentLabel.Size = new System.Drawing.Size(54, 13);
            this.commentLabel.TabIndex = 8;
            this.commentLabel.Text = "Comment:";
            // 
            // commentTextBox
            // 
            this.commentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commentTextBox.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.commentTextBox.Location = new System.Drawing.Point(77, 134);
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.Size = new System.Drawing.Size(200, 20);
            this.commentTextBox.TabIndex = 2;
            // 
            // commentNotesLabel
            // 
            this.commentNotesLabel.AutoSize = true;
            this.commentNotesLabel.Location = new System.Drawing.Point(74, 157);
            this.commentNotesLabel.Name = "commentNotesLabel";
            this.commentNotesLabel.Size = new System.Drawing.Size(55, 13);
            this.commentNotesLabel.TabIndex = 12;
            this.commentNotesLabel.Text = "• Optional";
            // 
            // symbolTypeGroupBox
            // 
            this.symbolTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.symbolTypeGroupBox.Controls.Add(this.constantRadioButton);
            this.symbolTypeGroupBox.Controls.Add(this.addressRadioButton);
            this.symbolTypeGroupBox.Location = new System.Drawing.Point(13, 190);
            this.symbolTypeGroupBox.Name = "symbolTypeGroupBox";
            this.symbolTypeGroupBox.Size = new System.Drawing.Size(264, 48);
            this.symbolTypeGroupBox.TabIndex = 3;
            this.symbolTypeGroupBox.TabStop = false;
            this.symbolTypeGroupBox.Text = "Symbol Type";
            // 
            // constantRadioButton
            // 
            this.constantRadioButton.AutoSize = true;
            this.constantRadioButton.Location = new System.Drawing.Point(99, 20);
            this.constantRadioButton.Name = "constantRadioButton";
            this.constantRadioButton.Size = new System.Drawing.Size(67, 17);
            this.constantRadioButton.TabIndex = 1;
            this.constantRadioButton.TabStop = true;
            this.constantRadioButton.Text = "Constant";
            this.constantRadioButton.UseVisualStyleBackColor = true;
            // 
            // addressRadioButton
            // 
            this.addressRadioButton.AutoSize = true;
            this.addressRadioButton.Location = new System.Drawing.Point(6, 20);
            this.addressRadioButton.Name = "addressRadioButton";
            this.addressRadioButton.Size = new System.Drawing.Size(63, 17);
            this.addressRadioButton.TabIndex = 0;
            this.addressRadioButton.TabStop = true;
            this.addressRadioButton.Text = "Address";
            this.addressRadioButton.UseVisualStyleBackColor = true;
            // 
            // labelUniqueLabel
            // 
            this.labelUniqueLabel.AutoSize = true;
            this.labelUniqueLabel.Location = new System.Drawing.Point(74, 53);
            this.labelUniqueLabel.Name = "labelUniqueLabel";
            this.labelUniqueLabel.Size = new System.Drawing.Size(160, 13);
            this.labelUniqueLabel.TabIndex = 10;
            this.labelUniqueLabel.Text = "• Unique among project symbols";
            // 
            // EditDefSymbol
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(290, 288);
            this.Controls.Add(this.labelUniqueLabel);
            this.Controls.Add(this.symbolTypeGroupBox);
            this.Controls.Add(this.commentNotesLabel);
            this.Controls.Add(this.commentTextBox);
            this.Controls.Add(this.commentLabel);
            this.Controls.Add(this.valueNotesLabel);
            this.Controls.Add(this.valueLabel);
            this.Controls.Add(this.labelNotesLabel);
            this.Controls.Add(this.valueTextBox);
            this.Controls.Add(this.labelTextBox);
            this.Controls.Add(this.labelLabel);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditDefSymbol";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Symbol";
            this.Load += new System.EventHandler(this.EditDefSymbol_Load);
            this.symbolTypeGroupBox.ResumeLayout(false);
            this.symbolTypeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label labelLabel;
        private System.Windows.Forms.TextBox labelTextBox;
        private System.Windows.Forms.TextBox valueTextBox;
        private System.Windows.Forms.Label labelNotesLabel;
        private System.Windows.Forms.Label valueLabel;
        private System.Windows.Forms.Label valueNotesLabel;
        private System.Windows.Forms.Label commentLabel;
        private System.Windows.Forms.TextBox commentTextBox;
        private System.Windows.Forms.Label commentNotesLabel;
        private System.Windows.Forms.GroupBox symbolTypeGroupBox;
        private System.Windows.Forms.RadioButton constantRadioButton;
        private System.Windows.Forms.RadioButton addressRadioButton;
        private System.Windows.Forms.Label labelUniqueLabel;
    }
}