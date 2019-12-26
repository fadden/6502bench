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
using System.Windows.Controls;

using Asm65;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Edit Address dialog.
    /// </summary>
    public partial class EditAddress : Window, INotifyPropertyChanged {
        /// <summary>
        /// Address typed by user. Only valid after the dialog returns OK.  Will be set to
        /// AddressMap.NO_ENTRY_ADDR if the user is attempting to delete the address.
        /// </summary>
        public int NewAddress { get; private set; }

        /// <summary>
        /// Offset being edited.
        /// </summary>
        private int mFirstOffset;

        /// <summary>
        /// Offset after the end of the selection, or -1 if only one line is selected.
        /// </summary>
        private int mNextOffset;

        /// <summary>
        /// Address after the end of the selection, or -1 if only one line is selected.
        /// </summary>
        private int mNextAddress;

        /// <summary>
        /// Maximum allowed address value.
        /// </summary>
        private int mMaxAddressValue;

        /// <summary>
        /// What the address would be if there were no addresses set after the initial one.
        /// </summary>
        private int mBaseAddr;

        /// <summary>
        /// Text formatter.
        /// </summary>
        private Formatter mFormatter;

        public string FirstOffsetStr {
            get { return mFormatter.FormatOffset24(mFirstOffset); }
        }
        public string NextOffsetStr {
            get { return mFormatter.FormatOffset24(mNextOffset); }
        }
        public string NextAddressStr {
            get { return '$' + mFormatter.FormatAddress(mNextAddress, mNextAddress > 0xffff); }
        }
        public string BytesSelectedStr {
            get {
                int count = mNextOffset - mFirstOffset;
                return count.ToString() + " (" + mFormatter.FormatHexValue(count, 2) + ")";
            }
        }

        /// <summary>
        /// Address input TextBox.
        /// </summary>
        public string AddressText {
            get { return mAddressText; }
            set { mAddressText = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mAddressText;

        /// <summary>
        /// Set to true when input is valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        public Visibility NextAddressVis {
            get { return mNextAddressVis; }
            set { mNextAddressVis = value; OnPropertyChanged(); }
        }
        public Visibility mNextAddressVis = Visibility.Collapsed;

        public string LoadAddressText {
            get { return mLoadAddressText; }
            set { mLoadAddressText = value; OnPropertyChanged(); }
        }
        public string mLoadAddressText = string.Empty;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="firstOffset">Offset at top of selection.</param>
        /// <param name="nextOffset">Offset past bottom of selection, or -1 if only one
        ///   line is selected.</param>
        /// <param name="project">Project reference.</param>
        /// <param name="formatter">Text formatter object.</param>
        public EditAddress(Window owner, int firstOffset, int nextOffset, int nextAddr,
                DisasmProject project, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mFirstOffset = firstOffset;
            mNextOffset = nextOffset;
            mNextAddress = nextAddr;
            mFormatter = formatter;
            mMaxAddressValue = project.CpuDef.MaxAddressValue;

            // Compute load address, i.e. where the byte would have been placed if the entire
            // file were loaded at the address of the first address map entry.  We assume
            // offsets wrap at the bank boundary.
            int fileStartAddr = project.AddrMap.OffsetToAddress(0);
            mBaseAddr = ((fileStartAddr + firstOffset) & 0xffff) | (fileStartAddr & 0xff0000);

            int firstAddr = project.GetAnattrib(firstOffset).Address;
            Debug.Assert(project.AddrMap.OffsetToAddress(firstOffset) == firstAddr);

            AddressText = Asm65.Address.AddressToString(firstAddr, false);

            LoadAddressText = '$' + mFormatter.FormatAddress(mBaseAddr, mBaseAddr > 0xffff);

            if (nextOffset >= 0) {
                NextAddressVis = Visibility.Visible;
            }

            NewAddress = -2;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            addrTextBox.SelectAll();
            addrTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            if (AddressText.Length == 0) {
                NewAddress = CommonUtil.AddressMap.NO_ENTRY_ADDR;
            } else {
                bool ok = Asm65.Address.ParseAddress(AddressText, mMaxAddressValue, out int addr);
                Debug.Assert(ok);
                NewAddress = addr;
            }
            DialogResult = true;
        }

        /// <summary>
        /// Handles a TextChanged event on the address text box.
        /// </summary>
        /// <remarks>
        /// Must have UpdateSourceTrigger=PropertyChanged set for this to work.  The default
        /// for TextBox is LostFocus.
        /// </remarks>
        private void UpdateControls() {
            IsValid = (AddressText.Length == 0) ||
                Asm65.Address.ParseAddress(AddressText, mMaxAddressValue, out int unused);
        }
    }


    // This might be better with validation rules, but it's sort of awkward to pass parameters
    // (like MaxAddressValue) in.
    // https://social.technet.microsoft.com/wiki/contents/articles/31422.wpf-passing-a-data-bound-value-to-a-validation-rule.aspx
    //
    // Speaking of awkward, updating the OK button's IsEnable value through validation
    // requires MultiDataTrigger.


    //public class AddressValidationRule : ValidationRule {
    //    public int MaxAddress { get; set; }

    //    public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
    //        string text = value.ToString();
    //        Debug.WriteLine("VALIDATE " + text);
    //        if ((text.Length == 0) ||
    //            Asm65.Address.ParseAddress(text, MaxAddress, out int unused)) {
    //            return new ValidationResult(true, null);
    //        } else {
    //            return new ValidationResult(false, "Invalid address");
    //        }
    //    }
    //}
}