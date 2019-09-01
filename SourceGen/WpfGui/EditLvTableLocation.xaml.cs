/*
 * Copyright 2019 faddenSoft
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
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Allows repositioning of a local variable table.
    /// </summary>
    /// <remarks>
    /// This would be better implemented as cut/copy/paste, allowing parts of tables to be
    /// freely duplicated and copied around, but that's a bunch of work for something that
    /// I'm not yet convinced is important.  (It might not be that much more work in and of
    /// itself, but once the action is possible people will expect it to work for comments
    /// and notes, and maybe want to paste operand formatting.)
    /// </remarks>
    public partial class EditLvTableLocation : Window, INotifyPropertyChanged {
        /// <summary>
        /// After a successful move, this will hold the new offset.
        /// </summary>
        public int NewOffset { get; private set; }

        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// File offset at which we are initially positioned.
        /// </summary>
        private int mCurrentOffset;

        // Dialog label text color, saved off at dialog load time.
        private Brush mDefaultLabelColor = Brushes.Black;
        private Brush mErrorLabelColor = Brushes.Red;

        public string OffsetStr {
            get { return mOffsetStr; }
            set { mOffsetStr = value; OnPropertyChanged(); ValidateOffset(); }
        }
        private string mOffsetStr;

        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        public Brush InvalidOffsetBrush {
            get { return mInvalidOffsetBrush; }
            set { mInvalidOffsetBrush = value; OnPropertyChanged(); }
        }
        private Brush mInvalidOffsetBrush;

        public Brush NotInstructionBrush {
            get { return mNotInstructionBrush; }
            set { mNotInstructionBrush = value; OnPropertyChanged(); }
        }
        private Brush mNotInstructionBrush;

        public Brush TableAlreadyPresentBrush {
            get { return mTableAlreadyPresentBrush; }
            set { mTableAlreadyPresentBrush = value; OnPropertyChanged(); }
        }
        private Brush mTableAlreadyPresentBrush;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditLvTableLocation(Window owner, DisasmProject project, int curOffset,
                int initialOffset) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mCurrentOffset = curOffset;

            // curOffset is where the table actually is.  initialOffset reflects changes
            // made on a previous invocation of this dialog.
            OffsetStr = initialOffset.ToString("x6");
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            offsetTextBox.SelectAll();
            offsetTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            NewOffset = Convert.ToInt32(OffsetStr, 16);
            DialogResult = true;
        }

        private void OffsetUp_Click(object sender, RoutedEventArgs e) {
            AdjustOffset(-1);
        }

        private void OffsetDown_Click(object sender, RoutedEventArgs e) {
            AdjustOffset(1);
        }

        /// <summary>
        /// Adjusts the offset up or down enough to get it to the next instruction start that
        /// doesn't already have a local variable table.
        /// </summary>
        /// <param name="adj">Adjustment, should be +1 or -1.</param>
        private void AdjustOffset(int adj) {
            int offset;
            try {
                offset = Convert.ToInt32(OffsetStr, 16);
            } catch (Exception) {
                // string is garbage, just put the initial offset back in
                OffsetStr = mCurrentOffset.ToString("x6");
                return;
            }
            offset += adj;
            while (offset >= 0 && offset < mProject.FileDataLength) {
                if (mProject.GetAnattrib(offset).IsInstructionStart && !HasTableNotSelf(offset)) {
                    // found a winner
                    OffsetStr = offset.ToString("x6");
                    return;
                }
                offset += adj;
            }
        }

        private void ValidateOffset() {
            InvalidOffsetBrush = NotInstructionBrush = TableAlreadyPresentBrush =
                mDefaultLabelColor;

            if (string.IsNullOrEmpty(OffsetStr)) {
                InvalidOffsetBrush = mErrorLabelColor;
                IsValid = false;
                return;
            }

            int offset;
            try {
                offset = Convert.ToInt32(OffsetStr, 16);
            } catch (Exception) {
                InvalidOffsetBrush = mErrorLabelColor;
                IsValid = false;
                return;
            }
            if (offset < 0 || offset >= mProject.FileDataLength) {
                InvalidOffsetBrush = mErrorLabelColor;
                IsValid = false;
                return;
            }

            Anattrib attr = mProject.GetAnattrib(offset);
            if (!attr.IsInstructionStart) {
                NotInstructionBrush = mErrorLabelColor;
                IsValid = false;
                return;
            }
            if (HasTableNotSelf(offset)) {
                TableAlreadyPresentBrush = mErrorLabelColor;
                IsValid = false;
                return;
            }

            IsValid = true;
        }

        /// <summary>
        /// Returns true if the specified offset is the start of an instruction, and doesn't
        /// have a local variable table unless the table is us.
        /// </summary>
        private bool HasTableNotSelf(int offset) {
            return offset != mCurrentOffset &&
                mProject.LvTables.TryGetValue(offset, out LocalVariableTable unused);
        }
    }
}
