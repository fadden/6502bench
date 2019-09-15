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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using Asm65;
using CommonUtil;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Split-address table generator.
    /// </summary>
    public partial class FormatSplitAddress : Window, INotifyPropertyChanged {
        /// Format descriptors to apply.
        /// </summary>
        public SortedList<int, FormatDescriptor> NewFormatDescriptors { get; private set; }

        /// <summary>
        /// User labels to apply.
        /// </summary>
        public Dictionary<int, Symbol> NewUserLabels { get; private set; }

        /// <summary>
        /// All target offsets found.  The list may contain redundant entries.
        /// </summary>
        public List<int> AllTargetOffsets { get; private set; }

        /// <summary>
        /// If set, targets are offset by one for RTS/RTL.
        /// </summary>
        public bool IsAdjustedForReturn {
            get { return mIsAdjustedForReturn; }
            set { mIsAdjustedForReturn = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mIsAdjustedForReturn;

        /// <summary>
        /// If set, this is a split-address table, e.g. all of the low bytes are followed
        /// by all of the high bytes.
        /// </summary>
        public bool IsSplitTable {
            get { return mIsSplitTable; }
            set { mIsSplitTable = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mIsSplitTable;

        /// <summary>
        /// If set, caller will add code entry hints to targets.
        /// </summary>
        public bool WantCodeHints {
            get { return mWantCodeHints; }
            set {
                mWantCodeHints = value;
                OnPropertyChanged();
            }
        }
        private bool mWantCodeHints;

        /// <summary>
        /// Set to true to make the "incompatible with selection" message visible.
        /// </summary>
        public bool IncompatibleSelectionVisibility {
            get { return mIncompatibleSelectionVisibility; }
            set {
                mIncompatibleSelectionVisibility = value;
                OnPropertyChanged();
            }
        }
        private bool mIncompatibleSelectionVisibility;

        /// <summary>
        /// Set to true to make the "invalid constant" message visible.
        /// </summary>
        public bool InvalidConstantVisibility {
            get { return mInvalidConstantVisibility; }
            set {
                mInvalidConstantVisibility = value;
                OnPropertyChanged();
            }
        }
        private bool mInvalidConstantVisibility;

        /// <summary>
        /// Set to true when valid output is available.
        /// </summary>
        private bool mOutputReady;

        /// <summary>
        /// Set to true when input is valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        public class OutputPreviewItem {
            public string Addr { get; private set; }
            public string Offset { get; private set; }
            public string Symbol { get; private set; }

            public OutputPreviewItem(string addr, string offset, string symbol) {
                Addr = addr;
                Offset = offset;
                Symbol = symbol;
            }
        }
        public ObservableCollection<OutputPreviewItem> OutputPreviewList { get; private set; }

        /// <summary>
        /// Selected offsets.  An otherwise contiguous range of offsets can be broken up
        /// by user-specified labels and address discontinuities, so this needs to be
        /// processed by range.
        /// </summary>
        private TypedRangeSet mSelection;

        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Formatter to use when displaying addresses and hex values.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Reentrancy block for UpdateControls().
        /// </summary>
        private bool mUpdating;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public FormatSplitAddress(Window owner, DisasmProject project, TypedRangeSet selection,
                Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mFormatter = formatter;
            mSelection = selection;
            IsValid = false;

            OutputPreviewList = new ObservableCollection<OutputPreviewItem>();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            mUpdating = true;

            string fmt, infoStr;
            if (mSelection.RangeCount == 1 && mSelection.Count == 1) {
                infoStr = (string)FindResource("str_SingleByte");
            } else if (mSelection.RangeCount == 1) {
                fmt = (string)FindResource("str_SingleGroup");
                infoStr = string.Format(fmt, mSelection.Count);
            } else {
                fmt = (string)FindResource("str_MultiGroup");
                infoStr = string.Format(fmt, mSelection.Count, mSelection.RangeCount);
            }
            selectionInfoLabel.Text = infoStr;

            width16Radio.IsChecked = true;
            lowFirstPartRadio.IsChecked = true;
            highSecondPartRadio.IsChecked = true;
            bankNthPartRadio.IsChecked = true;

            IncompatibleSelectionVisibility = InvalidConstantVisibility = false;

            if (mProject.CpuDef.HasAddr16) {
                // Disable the 24-bit option.  Having 16-bit selected will disable the rest.
                width24Radio.IsEnabled = false;
            }

            mUpdating = false;
            UpdateControls();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void UpdateControls() {
            if (mUpdating) {
                return;
            }
            mUpdating = true;   // no re-entry

            // handled with XAML bindings
            //lowThirdPartRadio.Enabled = width24Radio.Checked;
            //highThirdPartRadio.Enabled = width24Radio.Checked;
            //bankByteGroupBox.Enabled = width24Radio.Checked;

            lowSecondPartRadio.IsEnabled = true;

            // If the user selects "constant" for high byte or bank byte, then there is no
            // 3rd part available for low/high, so we need to turn those back off.
            if (width24Radio.IsChecked == true) {
                bool haveThree = !(highConstantRadio.IsChecked == true ||
                                   bankConstantRadio.IsChecked == true);
                lowThirdPartRadio.IsEnabled = haveThree;
                highThirdPartRadio.IsEnabled = haveThree;

                // If "constant" is selected for high byte *and* bank byte, then there's no
                // 2nd part available for low.
                if (highConstantRadio.IsChecked == true && bankConstantRadio.IsChecked == true) {
                    lowSecondPartRadio.IsEnabled = false;
                }
            } else {
                // For 16-bit address, if high byte is constant, then there's no second
                // part for the low byte.
                if (highConstantRadio.IsChecked == true) {
                    lowSecondPartRadio.IsEnabled = false;
                }
            }

            // Was a now-invalidated radio button selected before?
            if (!lowThirdPartRadio.IsEnabled && lowThirdPartRadio.IsChecked == true) {
                // low now invalid, switch to whatever high isn't using
                if (highFirstPartRadio.IsChecked == true) {
                    lowSecondPartRadio.IsChecked = true;
                } else {
                    lowFirstPartRadio.IsChecked = true;
                }
            }
            if (width16Radio.IsChecked == true && highThirdPartRadio.IsChecked == true) {
                // high now invalid, switch to whatever low isn't using
                if (lowFirstPartRadio.IsChecked == true) {
                    highSecondPartRadio.IsChecked = true;
                } else {
                    highFirstPartRadio.IsChecked = true;
                }
            }
            if (!lowSecondPartRadio.IsEnabled && lowSecondPartRadio.IsChecked == true) {
                // Should only happen when high part is constant.
                Debug.Assert(highFirstPartRadio.IsChecked == false);
                lowFirstPartRadio.IsChecked = true;
            }

            mUpdating = false;
            UpdatePreview();

            IsValid = mOutputReady;
        }

        private void WidthRadio_CheckedChanged(object sender, RoutedEventArgs e) {
            UpdateControls();
        }

        private void LowByte_CheckedChanged(object sender, RoutedEventArgs e) {
            // If we conflict with the high byte, change the high byte.
            if (lowFirstPartRadio.IsChecked == true && highFirstPartRadio.IsChecked == true) {
                highSecondPartRadio.IsChecked = true;
            } else if (lowSecondPartRadio.IsChecked == true && highSecondPartRadio.IsChecked == true) {
                highFirstPartRadio.IsChecked = true;
            } else if (lowThirdPartRadio.IsChecked == true && highThirdPartRadio.IsChecked == true) {
                highFirstPartRadio.IsChecked = true;
            }
            UpdateControls();
        }

        private void HighByte_CheckedChanged(object sender, RoutedEventArgs e) {
            // If we conflict with the low byte, change the low byte.
            if (lowFirstPartRadio.IsChecked == true && highFirstPartRadio.IsChecked == true) {
                lowSecondPartRadio.IsChecked = true;
            } else if (lowSecondPartRadio.IsChecked == true && highSecondPartRadio.IsChecked == true) {
                lowFirstPartRadio.IsChecked = true;
            } else if (lowThirdPartRadio.IsChecked == true && highThirdPartRadio.IsChecked == true) {
                lowFirstPartRadio.IsChecked = true;
            }
            UpdateControls();
        }

        private void BankByte_CheckedChanged(object sender, EventArgs e) {
            UpdateControls();
        }

        private void HighConstantTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            highConstantRadio.IsChecked = true;
            UpdateControls();
        }

        private void BankConstantTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            bankConstantRadio.IsChecked = true;
            UpdateControls();
        }

        private void UpdatePreview() {
            mOutputReady = false;

            int minDiv;

            if (width16Radio.IsChecked == true) {
                if (highConstantRadio.IsChecked == true) {
                    minDiv = 1;
                } else {
                    minDiv = 2;
                }
            } else {
                if (highConstantRadio.IsChecked == true) {
                    if (bankConstantRadio.IsChecked == true) {
                        minDiv = 1;
                    } else {
                        minDiv = 2;
                    }
                } else {
                    if (bankConstantRadio.IsChecked == true) {
                        minDiv = 2;
                    } else {
                        minDiv = 3;
                    }
                }
            }

            IncompatibleSelectionVisibility = InvalidConstantVisibility = false;

            // Start by clearing the previous contents of the list.  If something goes
            // wrong, we want to show the error messages on an empty list.
            OutputPreviewList.Clear();

            if ((mSelection.Count % minDiv) != 0) {
                IncompatibleSelectionVisibility = true;
                return;
            }

            int highConstant = -1;
            if (highConstantRadio.IsChecked == true) {
                if (!Number.TryParseInt(highConstantTextBox.Text, out highConstant,
                        out int unused) || (highConstant != (byte) highConstant)) {
                    InvalidConstantVisibility = true;
                    return;
                }
            }

            int bankConstant = -1;
            if (bankConstantRadio.IsEnabled && bankConstantRadio.IsChecked == true) {
                if (!Number.TryParseInt(bankConstantTextBox.Text, out bankConstant,
                        out int unused) || (bankConstant != (byte) bankConstant)) {
                    InvalidConstantVisibility = true;
                    return;
                }
            }

            // Looks valid, generate format list.
            GenerateFormats(minDiv, highConstant, bankConstant);
        }

        private void GenerateFormats(int div, int highConst, int bankConst) {
            SortedList<int, FormatDescriptor> newDfds = new SortedList<int, FormatDescriptor>();
            Dictionary<int, Symbol> newLabels = new Dictionary<int, Symbol>();
            List<int> targetOffsets = new List<int>();
            bool isBigEndian;

            // Identify the offset where each set of data starts.
            int span = mSelection.Count / div;
            int lowOff, highOff, bankOff;
            int stride;

            if (lowFirstPartRadio.IsChecked == true) {
                lowOff = 0;
                isBigEndian = false;
            } else if (lowSecondPartRadio.IsChecked == true) {
                lowOff = 1;
                isBigEndian = true;
            } else if (lowThirdPartRadio.IsChecked == true) {
                lowOff = 2;
                isBigEndian = true;
            } else {
                Debug.Assert(false);
                lowOff = -1;
                isBigEndian = false;
            }
            if (highFirstPartRadio.IsChecked == true) {
                highOff = 0;
            } else if (highSecondPartRadio.IsChecked == true) {
                highOff = 1;
            } else if (highThirdPartRadio.IsChecked == true) {
                highOff = 2;
            } else {
                highOff = -1;   // use constant
            }
            if (width24Radio.IsChecked == true) {
                if (bankNthPartRadio.IsChecked == true) {
                    // Use whichever part isn't being used by the other two.
                    if (lowOff != 0 && highOff != 0) {
                        bankOff = 0;
                    } else if (lowOff != 1 && highOff != 1) {
                        bankOff = 1;
                    } else {
                        Debug.Assert(lowOff != 2 && highOff != 2);
                        bankOff = 2;
                    }
                } else {
                    bankOff = -1;   // use constant
                }
            } else {
                bankOff = -1;       // use constant
                bankConst = 0;      // always bank 0
            }

            if (IsSplitTable) {
                // Split table, so stride is 1 and each section start is determined by the span.
                stride = 1;
                lowOff *= span;
                highOff *= span;
                bankOff *= span;
            } else {
                // For non-split table, the stride is the width of each entry.
                stride = 1;
                if (highOff >= 0) {
                    stride++;
                }
                if (bankOff >= 0) {
                    stride++;
                }
            }

            Debug.WriteLine("FormatAddressTable: stride=" + stride + " span=" + span +
                " count=" + mSelection.Count);
            Debug.WriteLine("  low=" + lowOff + " high=" + highOff + " bank=" + bankOff);

            // The TypedRangeSet doesn't have an index operation, so copy the values into
            // an array.
            int[] offsets = new int[mSelection.Count];
            int index = 0;
            foreach (TypedRangeSet.Tuple tup in mSelection) {
                offsets[index++] = tup.Value;
            }

            int adj = 0;
            if (IsAdjustedForReturn) {
                adj = 1;
            }

            // Walk through the file data, generating addresses as we go.
            byte[] fileData = mProject.FileData;
            for (int i = 0; i < span; i++) {
                byte low, high, bank;

                low = fileData[offsets[lowOff + i * stride]];
                if (highOff >= 0) {
                    high = fileData[offsets[highOff + i * stride]];
                } else {
                    high = (byte) highConst;
                }
                if (bankOff >= 0) {
                    bank = fileData[offsets[bankOff + i * stride]];
                } else {
                    bank = (byte) bankConst;
                }

                int addr = ((bank << 16) | (high << 8) | low) + adj;

                int targetOffset = mProject.AddrMap.AddressToOffset(offsets[0], addr);
                if (targetOffset < 0) {
                    // Address not within file bounds.
                    // TODO(maybe): look for matching platform/project symbols
                    AddPreviewItem(addr, -1, Res.Strings.INVALID_ADDRESS);
                } else {
                    // Note the same target offset may appear more than once.
                    targetOffsets.Add(targetOffset);

                    // If there's a user-defined label there already, use it.  Otherwise, we'll
                    // need to generate one.
                    string targetLabel;
                    if (mProject.UserLabels.TryGetValue(targetOffset, out Symbol sym)) {
                        targetLabel = sym.Label;
                        AddPreviewItem(addr, targetOffset, targetLabel);
                    } else {
                        // Generate a symbol that's unique vs. the symbol table.  We don't need
                        // it to be unique vs. the labels we're generating here, because we
                        // won't generate identical labels for different addresses, and we do
                        // want to generate a single label if more than one table entry refers
                        // to the same target.
                        Symbol tmpSym = AutoLabel.GenerateUniqueForAddress(addr,
                            mProject.SymbolTable, "T");
                        // tmpSym was returned as an auto-label, make it a user label instead
                        tmpSym = new Symbol(tmpSym.Label, tmpSym.Value, Symbol.Source.User,
                            Symbol.Type.LocalOrGlobalAddr);
                        newLabels[targetOffset] = tmpSym;       // overwrites previous
                        targetLabel = tmpSym.Label;
                        AddPreviewItem(addr, targetOffset, "(+) " + targetLabel);
                    }

                    if (IsSplitTable) {
                        // Now we need to create format descriptors for the addresses where we
                        // extracted the low, high, and bank values.
                        newDfds.Add(offsets[lowOff + i * stride], FormatDescriptor.Create(1,
                            new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.Low), false));
                        if (highOff >= 0) {
                            newDfds.Add(offsets[highOff + i * stride], FormatDescriptor.Create(1,
                                new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.High), false));
                        }
                        if (bankOff >= 0) {
                            newDfds.Add(offsets[bankOff + i * stride], FormatDescriptor.Create(1,
                                new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.Bank), false));
                        }
                    } else {
                        // Create a single format descriptor that spans all bytes.  Note we
                        // don't want to use lowOff here -- we want to put the format on
                        // whichever byte came first.
                        // TODO(maybe): we don't correctly deal with a "scrambled" non-split
                        //   24-bit table, i.e. low then bank then high.  This is not really
                        //   a thing, but we should either prevent it or punt to single-byte
                        //   like we do for split tables.
                        Debug.Assert(stride >= 1 && stride <= 3);
                        newDfds.Add(offsets[0 + i * stride], FormatDescriptor.Create(stride,
                            new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.Low), isBigEndian));
                    }
                }
            }

            NewFormatDescriptors = newDfds;
            NewUserLabels = newLabels;
            AllTargetOffsets = targetOffsets;

            // Don't show ready if all addresses are invalid.  It's okay if some work and
            // some don't.
            mOutputReady = (AllTargetOffsets.Count > 0);
        }

        private void AddPreviewItem(int addr, int offset, string label) {
            OutputPreviewItem newItem = new OutputPreviewItem(
                mFormatter.FormatAddress(addr, !mProject.CpuDef.HasAddr16),
                (offset >= 0 ? mFormatter.FormatOffset24(offset) : "---"),
                label);
            OutputPreviewList.Add(newItem);
        }
    }
}
