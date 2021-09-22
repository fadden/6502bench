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
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;

using Asm65;
using CommonUtil;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Edit Address Region dialog.
    /// </summary>
    public partial class EditAddress : Window, INotifyPropertyChanged {
        /// <summary>
        /// Updated address map entry.  Will be null if we want to delete the entry.
        /// </summary>
        public AddressMap.AddressMapEntry NewEntry { get; private set; }


        /// <summary>
        /// Offset being edited.
        /// </summary>
        private int mRegionStartOffset;
        public string RegionStartOffsetStr {
            get { return mFormatter.FormatOffset24(mRegionStartOffset); }
        }

        /// <summary>
        /// Offset after the end of the selection, or -1 if only one line is selected.
        /// </summary>
        private int mRegionEndOffset;
        public string RegionEndOffsetStr {
            get { return mFormatter.FormatOffset24(mRegionEndOffset); }
        }

        public string RegionLengthStr {
            get {
                int count = mRegionEndOffset - mRegionStartOffset;
                return count.ToString() + " (" + mFormatter.FormatHexValue(count, 2) + ")";
            }
        }

        /// <summary>
        /// Address at which a pre-label would be placed.  This is determined by the parent
        /// region, so its value is fixed.
        /// </summary>
        private int mPreLabelAddress;
        public string PreLabelAddressStr {
            get { return mFormatter.FormatOffset24(mRegionEndOffset); }
        }

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
        /// Set to true when requested region is valid.  Everything but the cancel button is
        /// disabled if not.
        /// </summary>
        public bool IsRegionValid {
            get { return mIsRegionValid; }
            set { mIsRegionValid = value; OnPropertyChanged(); }
        }
        private bool mIsRegionValid;

        /// <summary>
        /// Determines whether the "(floating)" message appears next to the length.
        /// </summary>
        public Visibility FloatTextVis {
            get { return mFloatTextVis; }
        }
        private Visibility mFloatTextVis;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private AddressMap.AddressRegion mNewRegion;

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
        /// <param name="entry">Map entry definition.  This may be an existing entry, or values
        ///   representing the selection.</param>
        /// <param name="newLength">Length of region.  Only used if we're resizing an
        ///   existing region.</param>
        /// <param name="project">Project reference.</param>
        /// <param name="formatter">Text formatter object.</param>
        public EditAddress(Window owner, AddressMap.AddressMapEntry entry, bool isNew,
                int newLength, DisasmProject project, Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mMaxAddressValue = project.CpuDef.MaxAddressValue;
            mFormatter = formatter;

            Configure(entry, isNew, newLength);
        }

        private void Configure(AddressMap.AddressMapEntry entry, bool isNew, int newLength) {
            mRegionStartOffset = mRegionEndOffset = entry.Offset;
            mPreLabelAddress = 0;
            IsRegionValid = false;

            // The passed-in region could have Length=FLOATING_LEN, so we need to resolve
            // that now.  We also need to figure out if it's valid.  The easiest way to do
            // that is to clone the address map, add the region to it, and see how the values
            // resolve.  This also gets us an address for the pre-label.
            List<AddressMap.AddressMapEntry> entries;
            int spanLength;
            entries = mProject.AddrMap.GetEntryList(out spanLength);
            AddressMap tmpMap = new AddressMap(spanLength, entries);
            if (!isNew) {
                // Remove the old entry.
                if (!tmpMap.RemoveEntry(entry.Offset, entry.Length)) {
                    // Shouldn't happen.
                    Debug.Assert(false);
                    // TODO(org): some sort of failure indicator
                    return;
                }
            }

            // Add the new / replacement entry.
            AddressMap.AddResult result = tmpMap.AddEntry(entry);
            if (result != AddressMap.AddResult.Okay) {
                // TODO(org): various things with failures
                Debug.Assert(false);    // remove
            } else {
                // Find it in the region tree.
                mNewRegion = tmpMap.FindRegion(entry.Offset, entry.Length);
                if (mNewRegion == null) {
                    // Shouldn't happen.
                    Debug.Assert(false);
                    // TODO(org): some sort of failure indicator
                    return;
                } else {
                    // Set offset / length values based on what we got.
                    IsRegionValid = true;
                    mRegionStartOffset = mNewRegion.Offset;
                    mRegionEndOffset = mNewRegion.Offset + mNewRegion.ActualLength;
                    mPreLabelAddress = mNewRegion.PreLabelAddress;
                    mFloatTextVis = mNewRegion.IsFloating ? Visibility.Visible : Visibility.Hidden;
                    // Init editable stuff.
                    AddressText = Asm65.Address.AddressToString(mNewRegion.Address, false);
                    PreLabelText = mNewRegion.PreLabel;
                    UseRelativeAddressing = mNewRegion.IsRelative;
                }
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            addrTextBox.SelectAll();
            addrTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            bool ok = ParseAddress(out int addr);
            Debug.Assert(ok);
            if (addr == AddressMap.INVALID_ADDR) {
                // field was blank, want to delete the entry
                NewEntry = null;
            } else {
                NewEntry = new AddressMap.AddressMapEntry(mNewRegion.Offset,
                    mNewRegion.Length, addr, PreLabelText,
                    UseRelativeAddressing);
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
            IsValid = IsRegionValid && ParseAddress(out int unused);
            // TODO(org): check pre-label syntax
        }

        private const string NON_ADDR_STR = "NA";

        /// <summary>
        /// Parses the address out of the AddressText text box.
        /// </summary>
        /// <param name="addr">Receives the parsed address.  Will be NON_ADDR for "NA", and
        ///   INVALID_ADDR if blank.</param>
        /// <returns>True if the string parsed successfully.</returns>
        private bool ParseAddress(out int addr) {
            // Left blank?
            if (AddressText.Length == 0) {
                addr = AddressMap.INVALID_ADDR;
                return true;
            }
            // "NA" for non-addressable?
            string upper = AddressText.ToUpper();
            if (upper == NON_ADDR_STR) {
                addr = AddressMap.NON_ADDR;
                return true;
            }
            // Parse numerically.
            return Asm65.Address.ParseAddress(AddressText, mMaxAddressValue, out addr);
        }
    }
}