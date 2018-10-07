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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

using Asm65;
using CommonUtil;
using CommonWinForms;

namespace SourceGen.AppForms {
    public partial class FormatSplitAddress : Form {
        /// <summary>
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

        public bool WantCodeHints {
            get {
                return addCodeHintCheckBox.Checked;
            }
        }

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
        /// Set to prevent controls from going nuts while initializing.
        /// </summary>
        private bool mInitializing;

        /// <summary>
        /// Set to true when valid output is available.
        /// </summary>
        private bool mOutputReady;


        public FormatSplitAddress(DisasmProject project, TypedRangeSet selection,
                Formatter formatter) {
            InitializeComponent();

            mProject = project;
            mFormatter = formatter;
            mSelection = selection;

            mOutputReady = false;
        }

        private void FormatSplitAddress_Load(object sender, EventArgs e) {
            mInitializing = true;

            string fmt = selectionInfoLabel.Text;
            selectionInfoLabel.Text = string.Format(fmt, mSelection.Count, mSelection.RangeCount);

            width16Radio.Checked = true;
            lowFirstPartRadio.Checked = true;
            highSecondPartRadio.Checked = true;
            bankNthPartRadio.Checked = true;

            incompatibleSelectionLabel.Visible = invalidConstantLabel.Visible = false;

            if (mProject.CpuDef.HasAddr16) {
                // Disable the 24-bit option.  Having 16-bit selected will disable the rest.
                width24Radio.Enabled = false;
            }

            outputPreviewListView.SetDoubleBuffered(true);

            mInitializing = false;
            UpdateControls();
        }

        private void okButton_Click(object sender, EventArgs e) { }

        private void UpdateControls() {
            if (mInitializing) {
                return;
            }
            mInitializing = true;   // no re-entry

            lowThirdPartRadio.Enabled = width24Radio.Checked;
            highThirdPartRadio.Enabled = width24Radio.Checked;
            bankByteGroupBox.Enabled = width24Radio.Checked;

            lowSecondPartRadio.Enabled = true;

            // If the user selects "constant" for high byte or bank byte, then there is no
            // 3rd part available for low/high, so we need to turn those back off.
            if (width24Radio.Checked) {
                bool haveThree = !(highConstantRadio.Checked || bankConstantRadio.Checked);
                lowThirdPartRadio.Enabled = haveThree;
                highThirdPartRadio.Enabled = haveThree;

                // If "constant" is selected for high byte *and* bank byte, then there's no
                // 2nd part available for low.
                if (highConstantRadio.Checked && bankConstantRadio.Checked) {
                    lowSecondPartRadio.Enabled = false;
                }
            } else {
                // For 16-bit address, if high byte is constant, then there's no second
                // part for the low byte.
                if (highConstantRadio.Checked) {
                    lowSecondPartRadio.Enabled = false;
                }
            }

            // Was a now-invalidated radio button selected before?
            if (!lowThirdPartRadio.Enabled && lowThirdPartRadio.Checked) {
                // low now invalid, switch to whatever high isn't using
                if (highFirstPartRadio.Checked) {
                    lowSecondPartRadio.Checked = true;
                } else {
                    lowFirstPartRadio.Checked = true;
                }
            }
            if (!highThirdPartRadio.Enabled && highThirdPartRadio.Checked) {
                // high now invalid, switch to whatever low isn't using
                if (lowFirstPartRadio.Checked) {
                    highSecondPartRadio.Checked = true;
                } else {
                    highFirstPartRadio.Checked = true;
                }
            }
            if (!lowSecondPartRadio.Enabled && lowSecondPartRadio.Checked) {
                // Should only happen when high part is constant.
                Debug.Assert(highFirstPartRadio.Checked == false);
                lowFirstPartRadio.Checked = true;
            }

            mInitializing = false;
            UpdatePreview();

            okButton.Enabled = mOutputReady;
        }

        private void widthRadio_CheckedChanged(object sender, EventArgs e) {
            UpdateControls();
        }

        private void pushRtsCheckBox_CheckedChanged(object sender, EventArgs e) {
            UpdateControls();
        }

        private void lowByte_CheckedChanged(object sender, EventArgs e) {
            // If we conflict with the high byte, change the high byte.
            if (lowFirstPartRadio.Checked && highFirstPartRadio.Checked) {
                highSecondPartRadio.Checked = true;
            } else if (lowSecondPartRadio.Checked && highSecondPartRadio.Checked) {
                highFirstPartRadio.Checked = true;
            } else if (lowThirdPartRadio.Checked && highThirdPartRadio.Checked) {
                highFirstPartRadio.Checked = true;
            }
            UpdateControls();
        }

        private void highByte_CheckedChanged(object sender, EventArgs e) {
            // If we conflict with the low byte, change the low byte.
            if (lowFirstPartRadio.Checked && highFirstPartRadio.Checked) {
                lowSecondPartRadio.Checked = true;
            } else if (lowSecondPartRadio.Checked && highSecondPartRadio.Checked) {
                lowFirstPartRadio.Checked = true;
            } else if (lowThirdPartRadio.Checked && highThirdPartRadio.Checked) {
                lowFirstPartRadio.Checked = true;
            }
            UpdateControls();
        }

        private void bankByte_CheckedChanged(object sender, EventArgs e) {
            UpdateControls();
        }

        private void highConstantTextBox_TextChanged(object sender, EventArgs e) {
            highConstantRadio.Checked = true;
            UpdateControls();
        }

        private void bankConstantTextBox_TextChanged(object sender, EventArgs e) {
            bankConstantRadio.Checked = true;
            UpdateControls();
        }

        private void UpdatePreview() {
            mOutputReady = false;

            int minDiv;

            if (width16Radio.Checked) {
                if (highConstantRadio.Checked) {
                    minDiv = 1;
                } else {
                    minDiv = 2;
                }
            } else {
                if (highConstantRadio.Checked) {
                    if (bankConstantRadio.Checked) {
                        minDiv = 1;
                    } else {
                        minDiv = 2;
                    }
                } else {
                    if (bankConstantRadio.Checked) {
                        minDiv = 2;
                    } else {
                        minDiv = 3;
                    }
                }
            }

            incompatibleSelectionLabel.Visible = invalidConstantLabel.Visible = false;

            try {
                // Start by clearing the previous contents of the list.  If something goes
                // wrong, we want to show the error messages on an empty list.
                outputPreviewListView.BeginUpdate();
                outputPreviewListView.Items.Clear();

                if ((mSelection.Count % minDiv) != 0) {
                    incompatibleSelectionLabel.Visible = true;
                    return;
                }

                int highConstant = -1;
                if (highConstantRadio.Checked) {
                    if (!Number.TryParseInt(highConstantTextBox.Text, out highConstant,
                            out int unused) || (highConstant != (byte) highConstant)) {
                        invalidConstantLabel.Visible = true;
                        return;
                    }
                }

                int bankConstant = -1;
                if (bankConstantRadio.Enabled && bankConstantRadio.Checked) {
                    if (!Number.TryParseInt(bankConstantTextBox.Text, out bankConstant,
                            out int unused) || (bankConstant != (byte) bankConstant)) {
                        invalidConstantLabel.Visible = true;
                        return;
                    }
                }

                // Looks valid, generate format list.
                GenerateFormats(minDiv, highConstant, bankConstant);
            } finally {
                outputPreviewListView.EndUpdate();
            }
        }

        private void GenerateFormats(int div, int highConst, int bankConst) {
            SortedList<int, FormatDescriptor> newDfds = new SortedList<int, FormatDescriptor>();
            Dictionary<int, Symbol> newLabels = new Dictionary<int, Symbol>();
            List<int> targetOffsets = new List<int>();

            // Identify the offset where each set of data starts.
            int span = mSelection.Count / div;
            int lowOff, highOff, bankOff;

            if (lowFirstPartRadio.Checked) {
                lowOff = 0;
            } else if (lowSecondPartRadio.Checked) {
                lowOff = span;
            } else if (lowThirdPartRadio.Checked) {
                lowOff = span * 2;
            } else {
                Debug.Assert(false);
                lowOff = -1;
            }
            if (highFirstPartRadio.Checked) {
                highOff = 0;
            } else if (highSecondPartRadio.Checked) {
                highOff = span;
            } else if (highThirdPartRadio.Checked) {
                highOff = span * 2;
            } else {
                highOff = -1;   // use constant
            }
            if (width24Radio.Checked) {
                if (bankNthPartRadio.Checked) {
                    // Use whichever part isn't being used by the other two.
                    if (lowOff != 0 && highOff != 0) {
                        bankOff = 0;
                    } else if (lowOff != span && highOff != span) {
                        bankOff = span;
                    } else {
                        Debug.Assert(lowOff != span * 2 && highOff != span * 2);
                        bankOff = span * 2;
                    }
                } else {
                    bankOff = -1;   // use constant
                }
            } else {
                bankOff = -1;       // use constant
                bankConst = 0;      // always bank 0
            }

            Debug.WriteLine("Extract from low=" + lowOff + " high=" + highOff +
                " bank=" + bankOff);

            // The TypedRangeSet doesn't have an index operation, so copy the values into
            // an array.
            int[] offsets = new int[mSelection.Count];
            int index = 0;
            foreach (TypedRangeSet.Tuple tup in mSelection) {
                offsets[index++] = tup.Value;
            }

            int adj = 0;
            if (pushRtsCheckBox.Checked) {
                adj = 1;
            }

            // Walk through the file data, generating addresses as we go.
            byte[] fileData = mProject.FileData;
            for (int i = 0; i < span; i++) {
                byte low, high, bank;

                low = fileData[offsets[lowOff + i]];
                if (highOff >= 0) {
                    high = fileData[offsets[highOff + i]];
                } else {
                    high = (byte) highConst;
                }
                if (bankOff >= 0) {
                    bank = fileData[offsets[bankOff + i]];
                } else {
                    bank = (byte) bankConst;
                }

                int addr = ((bank << 16) | (high << 8) | low) + adj;

                int targetOffset = mProject.AddrMap.AddressToOffset(offsets[0], addr);
                if (targetOffset < 0) {
                    // Address not within file bounds.
                    // TODO(maybe): look for matching platform/project symbols
                    AddPreviewItem(addr, -1, Properties.Resources.INVALID_ADDRESS);
                } else {
                    // Note the same target offset may appear more than once.
                    targetOffsets.Add(targetOffset);

                    // If there's a user-defined label there already, use it.  Otherwise, we'll
                    // need to generate one.
                    string targetLabel;
                    if (mProject.UserLabels.TryGetValue(targetOffset, out Symbol sym)) {
                        AddPreviewItem(addr, targetOffset, sym.Label);
                        targetLabel = sym.Label;
                    } else {
                        AddPreviewItem(addr, targetOffset, "(+)");
                        // Generate a symbol that's unique vs. the symbol table.  We don't need
                        // it to be unique vs. the labels we're generating here, because we
                        // won't generate identical labels for different addresses, and we do
                        // want to generate a single label if more than one table entry refers
                        // to the same target.
                        Symbol tmpSym = SymbolTable.GenerateUniqueForAddress(addr,
                            mProject.SymbolTable, "T");
                        // tmpSym was returned as an auto-label, make it a user label instead
                        tmpSym = new Symbol(tmpSym.Label, tmpSym.Value, Symbol.Source.User,
                            Symbol.Type.LocalOrGlobalAddr);
                        newLabels[targetOffset] = tmpSym;       // overwrites previous
                        targetLabel = tmpSym.Label;
                    }

                    // Now we need to create format descriptors for the addresses where we
                    // extracted the low, high, and bank values.
                    newDfds.Add(offsets[lowOff + i], FormatDescriptor.Create(1,
                        new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.Low), false));
                    if (highOff >= 0) {
                        newDfds.Add(offsets[highOff + i], FormatDescriptor.Create(1,
                            new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.High), false));
                    }
                    if (bankOff >= 0) {
                        newDfds.Add(offsets[bankOff + i], FormatDescriptor.Create(1,
                            new WeakSymbolRef(targetLabel, WeakSymbolRef.Part.Bank), false));
                    }
                }
            }

            NewFormatDescriptors = newDfds;
            NewUserLabels = newLabels;
            AllTargetOffsets = targetOffsets;

            // Don't show ready if all addresses are invalid.
            mOutputReady = (AllTargetOffsets.Count > 0);
        }

        private void AddPreviewItem(int addr, int offset, string label) {
            ListViewItem lvi = new ListViewItem(mFormatter.FormatAddress(addr,
                !mProject.CpuDef.HasAddr16));
            if (offset >= 0) {
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi,
                    mFormatter.FormatOffset24(offset)));
            } else {
                lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, "---"));
            }
            lvi.SubItems.Add(new ListViewItem.ListViewSubItem(lvi, label));
            outputPreviewListView.Items.Add(lvi);
        }
    }
}
