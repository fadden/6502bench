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
using System.Windows.Documents;
using System.Windows.Media;
using Asm65;
using CommonUtil;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Edit Address Region dialog.
    /// </summary>
    public partial class EditAddress : Window, INotifyPropertyChanged {
        /// <summary>
        /// Updated address map entry.  Will be null if we want to delete the existing entry.
        /// </summary>
        public AddressMap.AddressMapEntry ResultEntry { get; private set; }

        /// <summary>
        /// Dialog header.
        /// </summary>
        public string OperationStr {
            get { return mOperationStr; }
            set { mOperationStr = value; OnPropertyChanged(); }
        }
        private string mOperationStr;

        /// <summary>
        /// Initial address.  (Does not change.)
        /// </summary>
        public string RegionAddressStr {
            get { return "$" + mFormatter.FormatAddress(mRegionAddress, mShowBank); }
        }
        private int mRegionAddress;

        /// <summary>
        /// Offset of first selected byte.  (Does not change.)
        /// </summary>
        public string RegionStartOffsetStr {
            get { return mFormatter.FormatOffset24(mRegionStartOffset); }
        }
        private int mRegionStartOffset;

        /// <summary>
        /// Offset of last selected byte.  (Does not change.)
        /// </summary>
        public string RegionEndOffsetStr {
            get { return mFormatter.FormatOffset24(mRegionEndOffset); }
        }
        private int mRegionEndOffset;

        public string RegionLengthStr {
            get {
                int count = mRegionEndOffset - mRegionStartOffset + 1;
                return FormatLength(count);
            }
        }

        /// <summary>
        /// Set to true to show the offset/length stats for a current region.
        /// </summary>
        public bool ShowExistingRegion {
            get { return mShowExistingRegion; }
            set { mShowExistingRegion = value; OnPropertyChanged(); }
        }
        private bool mShowExistingRegion;

        public bool ShowOption1 {
            get { return mShowOption1; }
            set { mShowOption1 = value; OnPropertyChanged(); }
        }
        private bool mShowOption1;

        public bool ShowOption2 {
            get { return mShowOption2; }
            set { mShowOption2 = value; OnPropertyChanged(); }
        }
        private bool mShowOption2;

        public bool EnableOption1 {
            get { return mEnableOption1; }
            set { mEnableOption1 = value; OnPropertyChanged(); }
        }
        private bool mEnableOption1;

        public bool EnableOption2 {
            get { return mEnableOption2; }
            set { mEnableOption2 = value; OnPropertyChanged(); }
        }
        private bool mEnableOption2;

        public bool CheckOption1 {
            get { return mCheckOption1; }
            set { mCheckOption1 = value; OnPropertyChanged(); }
        }
        private bool mCheckOption1;

        public bool CheckOption2 {
            get { return mCheckOption2; }
            set { mCheckOption2 = value; OnPropertyChanged(); }
        }
        private bool mCheckOption2;

        /// <summary>
        /// Address at which a pre-label would be placed.  This is determined by the parent
        /// region, so its value is fixed.
        /// </summary>
        private int mPreLabelAddress;
        public string PreLabelAddressStr {
            get {
                if (mPreLabelAddress == Address.NON_ADDR) {
                    return Address.NON_ADDR_STR;
                } else {
                    return "$" + mFormatter.FormatAddress(mPreLabelAddress, mShowBank);
                }
            }
        }
        private bool mShowBank;

        public bool ShowErrorMessage {
            get { return mShowErrorMessage; }
            set { mShowErrorMessage = value; OnPropertyChanged(); }
        }
        private bool mShowErrorMessage;

        public string ErrorMessageStr {
            get { return mErrorMessageStr; }
            set { mErrorMessageStr = value; OnPropertyChanged(); }
        }
        private string mErrorMessageStr;

        /// <summary>
        /// Address input TextBox.
        /// </summary>
        public string AddressText {
            get { return mAddressText; }
            set { mAddressText = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mAddressText;

        public bool UseRelativeAddressing {
            get { return mUseRelativeAddressing; }
            set { mUseRelativeAddressing = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mUseRelativeAddressing;

        public bool DisallowInwardRes {
            get { return mDisallowInwardRes; }
            set { mDisallowInwardRes = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mDisallowInwardRes;

        public bool DisallowOutwardRes {
            get { return mDisallowOutwardRes; }
            set { mDisallowOutwardRes = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mDisallowOutwardRes;

        /// <summary>
        /// Pre-label input TextBox.
        /// </summary>
        public string PreLabelText {
            get { return mPreLabelText; }
            set { mPreLabelText = value; OnPropertyChanged(); UpdateControls(); }
        }
        private string mPreLabelText;

        /// <summary>
        /// Set to true when input is valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        /// <summary>
        /// Set to true unless there are no valid options (e.g. invalid new region).
        /// </summary>
        public bool EnableAttributeControls {
            get { return mEnableAttributeControls; }
            set { mEnableAttributeControls = value; OnPropertyChanged(); }
        }
        private bool mEnableAttributeControls;

        /// <summary>
        /// Set to true if the region has a floating end point.
        /// </summary>
        public bool IsFloating {
            get { return mIsFloating; }
            set { mIsFloating = value; OnPropertyChanged(); }
        }
        private bool mIsFloating;

        /// <summary>
        /// Set to true if the region is not new, and thus can be deleted.
        /// </summary>
        public bool CanDeleteRegion {
            get { return mCanDeleteRegion; }
            set { mCanDeleteRegion = value; OnPropertyChanged(); }
        }
        private bool mCanDeleteRegion;


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Non-error color for labels.
        private Brush mDefaultLabelColor = SystemColors.WindowTextBrush;

        /// <summary>
        /// Initial value for pre-label.
        /// </summary>
        private string mOrigPreLabel;

        /// <summary>
        /// True if parent region is non-addressable.
        /// </summary>
        private bool mParentNonAddr;

        /// <summary>
        /// Result for option #1.
        /// </summary>
        private AddressMap.AddressMapEntry mResultEntry1;

        /// <summary>
        /// Result for option #2.
        /// </summary>
        private AddressMap.AddressMapEntry mResultEntry2;

        /// <summary>
        /// Maximum allowed address value, based on CPU type.
        /// </summary>
        private int mMaxAddressValue;

        /// <summary>
        /// Reference to project.  We need the address map and symbol table.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Reference to text formatter.
        /// </summary>
        private Formatter mFormatter;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="curRegion">Current region; will be null for new entries.</param>
        /// <param name="newEntry">Prototype entry to create.</param>
        /// <param name="selectionLen">Length, in bytes, of the selection.</param>
        /// <param name="isSingleLine">True if the selection is a single line.</param>
        /// <param name="project">Project reference.</param>
        /// <param name="formatter">Text formatter object.</param>
        public EditAddress(Window owner, AddressMap.AddressRegion curRegion,
                AddressMap.AddressMapEntry newEntry, int selectionLen, bool isSingleLine,
                DisasmProject project, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            Debug.Assert((curRegion == null) ^ (newEntry == null));     // exactly one must be true

            mProject = project;
            mMaxAddressValue = project.CpuDef.MaxAddressValue;
            mShowBank = !project.CpuDef.HasAddr16;
            mFormatter = formatter;

            Configure(curRegion, newEntry, selectionLen, isSingleLine);
            UpdateControls();
        }

        private void Configure(AddressMap.AddressRegion curRegion,
                AddressMap.AddressMapEntry newEntry, int selectionLen, bool isSingleLine) {
            Debug.WriteLine("Configuring AR: reg=" + curRegion + " newEnt=" + newEntry +
                " selLen=" + selectionLen + " isSingle=" + isSingleLine);

            ShowOption1 = ShowOption2 = true;
            EnableOption1 = EnableOption2 = true;
            CheckOption1 = true;
            EnableAttributeControls = true;

            string option1Summ;
            string option1Msg;
            string option2Summ;
            string option2Msg;

            if (curRegion != null) {
                // Editing an existing region.
                CanDeleteRegion = true;
                ShowExistingRegion = true;
                mOrigPreLabel = curRegion.PreLabel;
                mParentNonAddr = (curRegion.PreLabelAddress == Address.NON_ADDR);

                if (curRegion.Address == Address.NON_ADDR) {
                    AddressText = Address.NON_ADDR_STR;
                } else {
                    AddressText = Asm65.Address.AddressToString(curRegion.Address, false);
                }
                PreLabelText = curRegion.PreLabel;
                DisallowInwardRes = curRegion.DisallowInward;
                DisallowOutwardRes = curRegion.DisallowOutward;
                UseRelativeAddressing = curRegion.IsRelative;

                OperationStr = (string)FindResource("str_HdrEdit");
                mRegionAddress = curRegion.Address;
                mRegionStartOffset = curRegion.Offset;
                mRegionEndOffset = curRegion.Offset + curRegion.ActualLength - 1;
                mPreLabelAddress = curRegion.PreLabelAddress;

                if (isSingleLine) {
                    // Only thing selected was arstart/arend.  First action is to edit
                    // the region properties, second action is to convert floating end
                    // to fixed.
                    mResultEntry1 = new AddressMap.AddressMapEntry(curRegion.Offset,
                        curRegion.Length, curRegion.Address, curRegion.PreLabel,
                        curRegion.DisallowInward, curRegion.DisallowOutward,
                        curRegion.IsRelative);
                    option1Summ = (string)FindResource("str_OptEditAsIsSummary");
                    option1Msg = (string)FindResource("str_OptEditAsIs");

                    if (curRegion.IsFloating) {
                        option2Summ = (string)FindResource("str_OptEditAndFixSummary");
                        option2Msg = (string)FindResource("str_OptEditAndFix");
                        mResultEntry2 = new AddressMap.AddressMapEntry(curRegion.Offset,
                            curRegion.ActualLength, curRegion.Address, curRegion.PreLabel,
                            curRegion.DisallowInward, curRegion.DisallowOutward,
                            curRegion.IsRelative);
                    } else {
                        option2Summ = string.Empty;
                        option2Msg = (string)FindResource("str_EditFixedAlreadyFixed");
                        mResultEntry2 = null;
                        EnableOption2 = false;  // show it, but disabled
                    }

                } else {
                    // Selection started with arstart and included multiple lines.  First
                    // action is to resize region.  Second action is edit without resize.
                    // If resize is illegal (e.g. new region exactly overlaps another),
                    // first action is disabled.
                    mResultEntry1 = new AddressMap.AddressMapEntry(curRegion.Offset,
                        selectionLen, curRegion.Address, curRegion.PreLabel,
                        curRegion.DisallowInward, curRegion.DisallowOutward, curRegion.IsRelative);
                    mResultEntry2 = new AddressMap.AddressMapEntry(curRegion.Offset,
                        curRegion.Length, curRegion.Address, curRegion.PreLabel,
                        curRegion.DisallowInward, curRegion.DisallowOutward, curRegion.IsRelative);

                    option1Summ = (string)FindResource("str_OptResizeSummary");
                    string fmt = (string)FindResource("str_OptResize");
                    option1Msg = string.Format(fmt,
                        mFormatter.FormatOffset24(curRegion.Offset + selectionLen - 1),
                        FormatLength(selectionLen));
                    option2Summ = (string)FindResource("str_OptEditAsIsSummary");
                    option2Msg = (string)FindResource("str_OptEditAsIs");

                    Debug.Assert(selectionLen > 0);
                    AddressMap.AddResult ares;
                    TryCreateRegion(curRegion, curRegion.Offset, selectionLen,
                        curRegion.Address, out ares);
                    if (ares != AddressMap.AddResult.Okay) {
                        // Can't resize the new region, so disable that option (still visible).
                        option1Summ = string.Empty;
                        string fmta = (string)FindResource("str_OptResizeFail");
                        option1Msg = string.Format(fmta, GetErrorString(ares));
                        EnableOption1 = false;
                        CheckOption2 = true;
                    }

                    if (curRegion.ActualLength == selectionLen) {
                        // The selection size matches the region's length, which means they
                        // have the entire region selected, so "resize" and "edit" do the same
                        // thing.  No real need to disable the resize option, but we can default
                        // to "edit only" to emphasize that there's no actual change.
                        CheckOption2 = true;
                    }
                }

            } else {
                // Creating a new region.  Prototype entry specifies offset, length, and address.
                // First action is to create a fixed-length region, second action is to create
                // a floating region.  Default changes for single-item selections.
                CanDeleteRegion = false;
                ShowExistingRegion = false;
                mOrigPreLabel = string.Empty;

                if (newEntry.Address == Address.NON_ADDR) {
                    AddressText = Address.NON_ADDR_STR;
                } else {
                    AddressText = Asm65.Address.AddressToString(newEntry.Address, false);
                }
                PreLabelText = string.Empty;
                DisallowInwardRes = false;
                DisallowOutwardRes = false;
                UseRelativeAddressing = false;

                OperationStr = (string)FindResource("str_HdrCreate");

                AddressMap.AddResult ares1;
                AddressMap.AddressRegion newRegion1 = TryCreateRegion(null, newEntry.Offset,
                    newEntry.Length, newEntry.Address, out ares1);
                AddressMap.AddResult ares2;
                AddressMap.AddressRegion newRegion2 = TryCreateRegion(null, newEntry.Offset,
                    AddressMap.FLOATING_LEN, newEntry.Address, out ares2);

                if (isSingleLine) {
                    // For single-line selection, create a floating region by default.
                    CheckOption2 = true;
                }

                // If it failed, report the error.  Most common reason will be a start offset
                // that overlaps an existing region.  You can create a fixed region inside
                // a fixed region with the same start offset, but can't create a float there.
                if (ares1 == AddressMap.AddResult.Okay) {
                    mResultEntry1 = new AddressMap.AddressMapEntry(newEntry.Offset,
                        newRegion1.ActualLength, newEntry.Address,
                        string.Empty, false, false, false);

                    option1Summ = (string)FindResource("str_CreateFixedSummary");
                    string fmt = (string)FindResource("str_CreateFixed");
                    option1Msg = string.Format(fmt,
                        mFormatter.FormatOffset24(newEntry.Offset),
                        FormatLength(newRegion1.ActualLength));
                    mPreLabelAddress = newRegion1.PreLabelAddress;

                    mParentNonAddr = (newRegion1.PreLabelAddress == Address.NON_ADDR);
                } else {
                    option1Summ = string.Empty;
                    if (ares1 == AddressMap.AddResult.StraddleExisting) {
                        option1Msg = (string)FindResource("str_CreateFixedFailStraddle");
                    } else {
                        option1Msg = (string)FindResource("str_CreateFixedFail");
                    }
                    CheckOption2 = true;
                    EnableOption1 = false;
                }
                if (ares2 == AddressMap.AddResult.Okay) {
                    mResultEntry2 = new AddressMap.AddressMapEntry(newEntry.Offset,
                        AddressMap.FLOATING_LEN, newEntry.Address,
                        string.Empty, false, false, false);

                    option2Summ = (string)FindResource("str_CreateFloatingSummary");
                    string fmt = (string)FindResource("str_CreateFloating");
                    option2Msg = string.Format(fmt,
                        mFormatter.FormatOffset24(newEntry.Offset),
                        FormatLength(newRegion2.ActualLength));
                    mPreLabelAddress = newRegion2.PreLabelAddress;

                    mParentNonAddr = (newRegion2.PreLabelAddress == Address.NON_ADDR);
                } else {
                    option2Summ = string.Empty;
                    option2Msg = (string)FindResource("str_CreateFloatingFail");
                    CheckOption1 = true;
                    CheckOption2 = false;   // required for some reason
                    EnableOption2 = false;
                }
                if (ares1 != AddressMap.AddResult.Okay && ares2 != AddressMap.AddResult.Okay) {
                    // Unable to create region here.  Explain why not.
                    EnableAttributeControls = false;
                    CheckOption1 = CheckOption2 = false;
                    mPreLabelAddress = Address.NON_ADDR;

                    SetErrorString(ares1);
                }
            }

            TextBlock tb1 = option1TextBlock;
            tb1.Inlines.Clear();
            if (!string.IsNullOrEmpty(option1Summ)) {
                tb1.Inlines.Add(new Run(option1Summ + " ") { FontWeight = FontWeights.Bold });
            }
            tb1.Inlines.Add(option1Msg);

            TextBlock tb2 = option2TextBlock;
            tb2.Inlines.Clear();
            if (!string.IsNullOrEmpty(option2Summ)) {
                tb2.Inlines.Add(new Run(option2Summ + " ") { FontWeight = FontWeights.Bold });
            }
            tb2.Inlines.Add(option2Msg);
        }

        private string FormatLength(int len) {
            return len + " (" + mFormatter.FormatHexValue(len, 2) + ")";
        }

        private AddressMap.AddressRegion TryCreateRegion(AddressMap.AddressRegion delRegion,
                int offset, int length, int addr, out AddressMap.AddResult result) {
            AddressMap tmpMap = mProject.AddrMap.Clone();

            if (delRegion != null && !tmpMap.RemoveEntry(delRegion.Offset, delRegion.Length)) {
                Debug.Assert(false, "Failed to remove existing region");
                result = AddressMap.AddResult.InternalError;
                return null;
            }

            result = tmpMap.AddEntry(offset, length, addr);
            if (result != AddressMap.AddResult.Okay) {
                return null;
            }
            AddressMap.AddressRegion newRegion = tmpMap.FindRegion(offset, length);
            if (newRegion == null) {
                // Shouldn't happen.
                Debug.Assert(false, "Failed to find region we just created");
                result = AddressMap.AddResult.InternalError;
                return null;
            }
            return newRegion;
        }

        private string GetErrorString(AddressMap.AddResult result) {
            string rsrc;
            switch (result) {
                case AddressMap.AddResult.InternalError:
                    rsrc = "str_ErrInternal";
                    break;
                case AddressMap.AddResult.InvalidValue:
                    rsrc = "str_ErrInvalidValue";
                    break;
                case AddressMap.AddResult.OverlapExisting:
                    rsrc = "str_ErrOverlapExisting";
                    break;
                case AddressMap.AddResult.OverlapFloating:
                    rsrc = "str_ErrOverlapFloating";
                    break;
                case AddressMap.AddResult.StraddleExisting:
                    rsrc = "str_ErrStraddleExisting";
                    break;
                default:
                    Debug.Assert(false);
                    rsrc = "str_ErrInternal";
                    break;
            }
            return(string)FindResource(rsrc);   // throws exception on failure
        }

        private void SetErrorString(AddressMap.AddResult result) {
            ErrorMessageStr = GetErrorString(result);
            ShowErrorMessage = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            addrTextBox.SelectAll();
            addrTextBox.Focus();
        }

        /// <summary>
        /// Handles a TextChanged event on the address text box.
        /// </summary>
        /// <remarks>
        /// Must have UpdateSourceTrigger=PropertyChanged set for this to work.  The default
        /// for TextBox is LostFocus.
        /// </remarks>
        private void UpdateControls() {
            bool addrOkay = ParseAddress(out int unused);
            if (addrOkay) {
                enterAddressLabel.Foreground = mDefaultLabelColor;
            } else {
                enterAddressLabel.Foreground = Brushes.Red;
            }

            bool preLabelOkay = PreLabelTextChanged();

            IsValid = EnableAttributeControls && addrOkay && preLabelOkay;
        }

        /// <summary>
        /// Validates pre-label.
        /// </summary>
        private bool PreLabelTextChanged() {
            string label = PreLabelText;

            parentNonAddrLabel.Foreground = mDefaultLabelColor;
            if (string.IsNullOrEmpty(label)) {
                return true;
            }

            if (mParentNonAddr) {
                parentNonAddrLabel.Foreground = Brushes.Blue;
            }

            // Check syntax.  We don't care about the details.
            bool isValid = Asm65.Label.ValidateLabelDetail(label, out bool unused1,
                out bool unused2);
            if (!isValid) {
                validSyntaxLabel.Foreground = Brushes.Red;
                return false;
            } else {
                validSyntaxLabel.Foreground = mDefaultLabelColor;
            }

            // Check for duplicates.
            notDuplicateLabel.Foreground = mDefaultLabelColor;
            if (label == mOrigPreLabel) {
                return true;
            }
            if (mProject.SymbolTable.TryGetValue(label, out Symbol sym)) {
                notDuplicateLabel.Foreground = Brushes.Red;
                return false;
            }
            return true;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            bool ok = ParseAddress(out int addr);
            Debug.Assert(ok);

            AddressMap.AddressMapEntry baseEntry;
            if (CheckOption1) {
                baseEntry = mResultEntry1;
            } else {
                baseEntry = mResultEntry2;
            }

            // Combine base entry with pre-label string and relative addressing checkbox.
            ResultEntry = new AddressMap.AddressMapEntry(baseEntry.Offset,
                baseEntry.Length, addr, PreLabelText,
                DisallowInwardRes, DisallowOutwardRes, UseRelativeAddressing);
            Debug.WriteLine("Dialog result: " + ResultEntry);
            DialogResult = true;
        }

        /// <summary>
        /// Parses the address out of the AddressText text box.
        /// </summary>
        /// <param name="addr">Receives the parsed address.  Will be NON_ADDR for "NA".</param>
        /// <returns>True if the string parsed successfully.</returns>
        private bool ParseAddress(out int addr) {
            // "NA" for non-addressable?
            string upper = AddressText.ToUpper();
            if (upper == Address.NON_ADDR_STR) {
                addr = Address.NON_ADDR;
                return true;
            }
            // Parse numerically.
            return Asm65.Address.ParseAddress(AddressText, mMaxAddressValue, out addr);
        }

        private void DeleteRegion_Click(object sender, RoutedEventArgs e) {
            ResultEntry = null;
            DialogResult = true;
        }
    }
}
