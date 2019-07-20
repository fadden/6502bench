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
namespace SourceGenWF.Tools {
    partial class HexDumpViewer {
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
            this.hexDumpListView = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.charConvComboBox = new System.Windows.Forms.ComboBox();
            this.charConvLabel = new System.Windows.Forms.Label();
            this.topMostCheckBox = new System.Windows.Forms.CheckBox();
            this.asciiOnlyCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // hexDumpListView
            // 
            this.hexDumpListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hexDumpListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.hexDumpListView.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexDumpListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.hexDumpListView.HideSelection = false;
            this.hexDumpListView.Location = new System.Drawing.Point(13, 13);
            this.hexDumpListView.Name = "hexDumpListView";
            this.hexDumpListView.Size = new System.Drawing.Size(477, 514);
            this.hexDumpListView.TabIndex = 0;
            this.hexDumpListView.UseCompatibleStateImageBehavior = false;
            this.hexDumpListView.View = System.Windows.Forms.View.Details;
            this.hexDumpListView.VirtualMode = true;
            this.hexDumpListView.CacheVirtualItems += new System.Windows.Forms.CacheVirtualItemsEventHandler(this.hexDumpListView_CacheVirtualItems);
            this.hexDumpListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.hexDumpListView_RetrieveVirtualItem);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Offset   0  1  2  3  4  5  6  7  8  9  A  B  C  D  E  F  ASCII";
            this.columnHeader1.Width = 455;
            // 
            // charConvComboBox
            // 
            this.charConvComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.charConvComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.charConvComboBox.FormattingEnabled = true;
            this.charConvComboBox.Items.AddRange(new object[] {
            "Basic ASCII",
            "High/Low ASCII"});
            this.charConvComboBox.Location = new System.Drawing.Point(130, 533);
            this.charConvComboBox.Name = "charConvComboBox";
            this.charConvComboBox.Size = new System.Drawing.Size(136, 21);
            this.charConvComboBox.TabIndex = 1;
            this.charConvComboBox.SelectedIndexChanged += new System.EventHandler(this.charConvComboBox_SelectedIndexChanged);
            // 
            // charConvLabel
            // 
            this.charConvLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.charConvLabel.AutoSize = true;
            this.charConvLabel.Location = new System.Drawing.Point(13, 536);
            this.charConvLabel.Name = "charConvLabel";
            this.charConvLabel.Size = new System.Drawing.Size(111, 13);
            this.charConvLabel.TabIndex = 2;
            this.charConvLabel.Text = "Character conversion:";
            // 
            // topMostCheckBox
            // 
            this.topMostCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.topMostCheckBox.AutoSize = true;
            this.topMostCheckBox.Location = new System.Drawing.Point(398, 535);
            this.topMostCheckBox.Name = "topMostCheckBox";
            this.topMostCheckBox.Size = new System.Drawing.Size(92, 17);
            this.topMostCheckBox.TabIndex = 3;
            this.topMostCheckBox.Text = "Always on top";
            this.topMostCheckBox.UseVisualStyleBackColor = true;
            this.topMostCheckBox.CheckedChanged += new System.EventHandler(this.topMostCheckBox_CheckedChanged);
            // 
            // asciiOnlyCheckBox
            // 
            this.asciiOnlyCheckBox.AutoSize = true;
            this.asciiOnlyCheckBox.Location = new System.Drawing.Point(284, 535);
            this.asciiOnlyCheckBox.Name = "asciiOnlyCheckBox";
            this.asciiOnlyCheckBox.Size = new System.Drawing.Size(104, 17);
            this.asciiOnlyCheckBox.TabIndex = 4;
            this.asciiOnlyCheckBox.Text = "ASCII-only dump";
            this.asciiOnlyCheckBox.UseVisualStyleBackColor = true;
            this.asciiOnlyCheckBox.CheckedChanged += new System.EventHandler(this.asciiOnlyCheckBox_CheckedChanged);
            // 
            // HexDumpViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 561);
            this.Controls.Add(this.asciiOnlyCheckBox);
            this.Controls.Add(this.topMostCheckBox);
            this.Controls.Add(this.charConvLabel);
            this.Controls.Add(this.charConvComboBox);
            this.Controls.Add(this.hexDumpListView);
            this.MinimumSize = new System.Drawing.Size(518, 180);
            this.Name = "HexDumpViewer";
            this.Text = "Hex Dump Viewer";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HexDumpViewer_FormClosed);
            this.Load += new System.EventHandler(this.HexDumpViewer_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView hexDumpListView;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ComboBox charConvComboBox;
        private System.Windows.Forms.Label charConvLabel;
        private System.Windows.Forms.CheckBox topMostCheckBox;
        private System.Windows.Forms.CheckBox asciiOnlyCheckBox;
    }
}