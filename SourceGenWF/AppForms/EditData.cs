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

using CommonUtil;

namespace SourceGenWF.AppForms {
    public partial class EditData : Form {
        /// <summary>
        /// Result set that describes the formatting to perform.  Not all regions will have
        /// the same format, e.g. the "mixed ASCII" mode will alternate strings and bytes
        /// (rather than a dedicated "mixed ASCII" format type).
        /// </summary>
        public SortedList<int, FormatDescriptor> Results { get; private set; }

        /// <summary>
        /// Selected offsets.  An otherwise contiguous range of offsets can be broken up
        /// by user-specified labels and address discontinuities, so this needs to be
        /// processed by range.
        /// </summary>
        public TypedRangeSet Selection { private get; set; }

        /// <summary>
        /// FormatDescriptor from the first offset.  May be null if the offset doesn't
        /// have a format descriptor specified.  This will be used to configure the
        /// dialog controls if the format is suited to the selection.  The goal is to
        /// make single-item editing work as expected.
        /// </summary>
        public FormatDescriptor FirstFormatDescriptor { private get; set; }

        /// <summary>
        /// Raw file data.
        /// </summary>
        private byte[] mFileData;

        /// <summary>
        /// Symbol table to use when resolving symbolic values.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Formatter to use when displaying addresses and hex values.
        /// </summary>
        private Asm65.Formatter mFormatter;

        /// <summary>
        /// Set this during initial control configuration, so we know to ignore the CheckedChanged
        /// events.
        /// </summary>
        private bool mIsInitialSetup;

        /// <summary>
        /// Set to true if, during the initial setup, the format defined by FirstFormatDescriptor
        /// was unavailable.
        /// </summary>
        private bool mPreferredFormatUnavailable;


        public EditData(byte[] fileData, SymbolTable symbolTable, Asm65.Formatter formatter) {
            InitializeComponent();

            mFileData = fileData;
            mSymbolTable = symbolTable;
            mFormatter = formatter;

            //Results = new List<Result>();
        }

        private void EditData_Load(object sender, EventArgs e) {
            DateTime startWhen = DateTime.Now;

            mIsInitialSetup = true;

            // Determine which of the various options is suitable for the selected offsets.
            // Disable any radio buttons that won't work.
            AnalyzeRanges();

            // Configure the dialog from the FormatDescriptor, if one is available.
            Debug.WriteLine("First FD: " + FirstFormatDescriptor);
            SetControlsFromDescriptor(FirstFormatDescriptor);

            if (mPreferredFormatUnavailable) {
                // This can happen when e.g. a bunch of stuff is formatted as null-terminated
                // strings.  We don't recognize a lone zero as a string, but we allow it if
                // it's next to a bunch of others.  If you come back later and try to format
                // just that one byte, you end up here.
                // TODO(maybe): make it more obvious what's going on?
                Debug.WriteLine("NOTE: preferred format unavailable");
            }

            mIsInitialSetup = false;
            UpdateControls();

            Debug.WriteLine("EditData dialog load time: " +
                (DateTime.Now - startWhen).TotalMilliseconds + " ms");
        }

        private void EditData_Shown(object sender, EventArgs e) {
            // Start with the focus in the text box if the initial format allows for a
            // symbolic reference.  This way they can start typing immediately.
            if (simpleDisplayAsGroupBox.Enabled) {
                symbolEntryTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles CheckedChanged event for all radio buttons in main group.  This will
        /// fire twice when a radio button is clicked (once to un-check the old, once
        /// to check the new).
        /// </summary>
        private void MainGroup_CheckedChanged(object sender, EventArgs e) {
            // Enable/disable the style group and the low/high/bank radio group.
            // Update preview window.
            UpdateControls();
        }

        /// <summary>
        /// Handles CheckedChanged event for radio buttons in the simple-data "display as"
        /// group box.
        /// </summary>
        private void SimpleDisplay_CheckedChanged(object sender, EventArgs e) {
            // Enable/disable the low/high/bank radio group.
            UpdateControls();
        }

        /// <summary>
        /// Handles CheckedChanged event for all radio buttons in symbol-part group.
        /// </summary>
        private void PartGroup_CheckedChanged(object sender, EventArgs e) {
            // not currently using a preview window; could add one for single items?
        }

        private void symbolEntryTextBox_TextChanged(object sender, EventArgs e) {
            // Make sure Symbol is checked if they're typing text in.
            Debug.Assert(radioSimpleDataSymbolic.Enabled);
            radioSimpleDataSymbolic.Checked = true;
            // Update OK button based on symbol validity.
            UpdateControls();
        }

        private void okButton_Click(object sender, EventArgs e) {
            CreateDescriptorListFromControls();
            FormatDescriptor.DebugDumpSortedList(Results);
        }

        /// <summary>
        /// Updates all of the controls to reflect the current internal state.
        /// </summary>
        private void UpdateControls() {
            if (mIsInitialSetup) {
                return;
            }

            // Configure the simple data "display as" style box.
            bool wantStyle = false;
            int simpleWidth = -1;
            bool isBigEndian = false;
            if (radioSingleBytes.Checked) {
                wantStyle = true;
                simpleWidth = 1;
            } else if (radio16BitLittle.Checked) {
                wantStyle = true;
                simpleWidth = 2;
            } else if (radio16BitBig.Checked) {
                wantStyle = true;
                simpleWidth = 2;
                isBigEndian = true;
            } else if (radio24BitLittle.Checked) {
                wantStyle = true;
                simpleWidth = 3;
            } else if (radio32BitLittle.Checked) {
                wantStyle = true;
                simpleWidth = 4;
            }
            bool focusOnSymbol = !simpleDisplayAsGroupBox.Enabled && wantStyle;
            simpleDisplayAsGroupBox.Enabled = wantStyle;
            if (wantStyle) {
                // TODO(soon): compute on first need and save results; this is getting called
                //   2x as radio buttons are hit, and might be slow on large data sets
                radioSimpleDataAscii.Enabled = IsRawAsciiCompatible(simpleWidth, isBigEndian);
            }

            // Enable the symbolic reference entry box if the "display as" group is enabled.
            // That way instead of "click 16-bit", "click symbol", "enter symbol", the user
            // can skip the second step.
            symbolEntryTextBox.Enabled = simpleDisplayAsGroupBox.Enabled;
            symbolPartPanel.Enabled = radioSimpleDataSymbolic.Checked;

            // If we just enabled the group box, set the focus on the symbol entry box.  This
            // removes another click from the steps, though it's a bit aggressive if you're
            // trying to arrow your way through the items.
            if (focusOnSymbol) {
                symbolEntryTextBox.Focus();
            }

            bool isOk = true;
            if (radioSimpleDataSymbolic.Checked) {
                // Just check for correct format.  References to non-existent labels are allowed.
                isOk = Asm65.Label.ValidateLabel(symbolEntryTextBox.Text);

                // Actually, let's discourage references to auto-labels.
                if (isOk && mSymbolTable.TryGetValue(symbolEntryTextBox.Text, out Symbol sym)) {
                    isOk = sym.SymbolSource != Symbol.Source.Auto;
                }
            }
            okButton.Enabled = isOk;
        }

        /// <summary>
        /// Analyzes the selection to see which data formatting options are suitable.
        /// Disables radio buttons and updates labels.
        /// 
        /// Call this once, when the dialog is first loaded.
        /// </summary>
        private void AnalyzeRanges() {
            Debug.Assert(Selection.Count != 0);

            string fmt = (Selection.RangeCount == 1) ?
                Properties.Resources.FMT_FORMAT_SINGLE_GROUP :
                Properties.Resources.FMT_FORMAT_MULTIPLE_GROUPS;
            selectFormatLabel.Text = string.Format(fmt, Selection.Count, Selection.RangeCount);

            IEnumerator<TypedRangeSet.TypedRange> iter = Selection.RangeListIterator;

            int mixedAsciiOkCount = 0;
            int mixedAsciiNotCount = 0;
            int nullTermStringCount = 0;
            int len8StringCount = 0;
            int len16StringCount = 0;
            int dciStringCount = 0;
            //int revDciStringCount = 0;

            // For each range, check to see if the data within qualifies for the various
            // options.  If any of them fail to meet the criteria, the option is disabled
            // for all ranges.
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;
                Debug.WriteLine("Testing [" + rng.Low + ", " + rng.High + "]");

                // Start with the easy ones.  Single-byte and dense are always enabled.

                int count = rng.High - rng.Low + 1;
                Debug.Assert(count > 0);
                if ((count & 0x01) != 0) {
                    // not divisible by 2, disallow 16-bit entries
                    radio16BitLittle.Enabled = false;
                    radio16BitBig.Enabled = false;
                }
                if ((count & 0x03) != 0) {
                    // not divisible by 4, disallow 32-bit entries
                    radio32BitLittle.Enabled = false;
                }
                if ((count / 3) * 3 != count) {
                    // not divisible by 3, disallow 24-bit entries
                    radio24BitLittle.Enabled = false;
                }


                // Check for run of bytes (2 or more of the same thing).  Remember that
                // we check this one region at a time, and each region could have different
                // bytes, but so long as the bytes are all the same within a region we're good.
                if (radioFill.Enabled && count > 1 &&
                        DataAnalysis.RecognizeRun(mFileData, rng.Low, rng.High) == count) {
                    // LGTM
                } else {
                    radioFill.Enabled = false;
                }

                // See if there's enough string data to make it worthwhile.  We use an
                // arbitrary threshold of 2+ ASCII characters, and require twice as many
                // ASCII as non-ASCII.  We arbitrarily require the strings to be either
                // high or low ASCII, and treat the other as non-ASCII.  (We could relax
                // this -- we generate separate items for each string and non-ASCII chunk --
                // but I'm trying to hide the option when the buffer doesn't really seem
                // to be holding strings.  Could replace with some sort of minimum string
                // length requirement?)
                if (radioStringMixed.Enabled) {
                    int asciiCount;
                    DataAnalysis.CountAsciiBytes(mFileData, rng.Low, rng.High,
                        out int lowAscii, out int highAscii, out int nonAscii);
                    if (highAscii > lowAscii) {
                        asciiCount = highAscii;
                        nonAscii += lowAscii;
                    } else {
                        asciiCount = lowAscii;
                        nonAscii += highAscii;
                    }

                    if (asciiCount >= 2 && asciiCount >= nonAscii * 2) {
                        // Looks good
                        mixedAsciiOkCount += asciiCount;
                        mixedAsciiNotCount += nonAscii;
                    } else {
                        // Fail
                        radioStringMixed.Enabled = false;
                        radioStringMixedReverse.Enabled = false;
                        mixedAsciiOkCount = mixedAsciiNotCount = -1;
                    }
                }

                // Check for null-terminated strings.  Zero-length strings are allowed, but
                // not counted -- we want to have some actual character data.  Individual
                // strings need to be entirely high-ASCII or low-ASCII, but not all strings
                // in a region have to be the same.
                if (radioStringNullTerm.Enabled) {
                    int strCount = DataAnalysis.RecognizeNullTerminatedStrings(mFileData,
                        rng.Low, rng.High);
                    if (strCount > 0) {
                        nullTermStringCount += strCount;
                    } else {
                        radioStringNullTerm.Enabled = false;
                        nullTermStringCount = -1;
                    }
                }

                // Check for strings prefixed with an 8-bit length.
                if (radioStringLen8.Enabled) {
                    int strCount = DataAnalysis.RecognizeLen8Strings(mFileData, rng.Low, rng.High);
                    if (strCount > 0) {
                        len8StringCount += strCount;
                    } else {
                        radioStringLen8.Enabled = false;
                        len8StringCount = -1;
                    }
                }

                // Check for strings prefixed with a 16-bit length.
                if (radioStringLen16.Enabled) {
                    int strCount = DataAnalysis.RecognizeLen16Strings(mFileData, rng.Low, rng.High);
                    if (strCount > 0) {
                        len16StringCount += strCount;
                    } else {
                        radioStringLen16.Enabled = false;
                        len16StringCount = -1;
                    }
                }

                // Check for DCI strings.  All strings within a single range must have the
                // same "polarity", e.g. low ASCII terminated by high ASCII.
                if (radioStringDci.Enabled) {
                    int strCount = DataAnalysis.RecognizeDciStrings(mFileData, rng.Low, rng.High);
                    if (strCount > 0) {
                        dciStringCount += strCount;
                    } else {
                        radioStringDci.Enabled = false;
                        dciStringCount = -1;
                    }
                }

                //// Check for reverse DCI strings.  All strings within a single range must have the
                //// same "polarity", e.g. low ASCII terminated by high ASCII.
                //if (radioStringDciReverse.Enabled) {
                //    int strCount = DataAnalysis.RecognizeReverseDciStrings(mFileData,
                //            rng.Low, rng.High);
                //    if (strCount > 0) {
                //        revDciStringCount += strCount;
                //    } else {
                //        radioStringDciReverse.Enabled = false;
                //        revDciStringCount = -1;
                //    }
                //}
            }

            // Update the dialog with string and character counts, summed across all regions.

            if (mixedAsciiOkCount > 0) {
                Debug.Assert(radioStringMixed.Enabled);
                radioStringMixed.Text = string.Format(radioStringMixed.Text,
                    mixedAsciiOkCount, mixedAsciiNotCount);
                radioStringMixedReverse.Text = string.Format(radioStringMixedReverse.Text,
                    mixedAsciiOkCount, mixedAsciiNotCount);
            } else {
                Debug.Assert(!radioStringMixed.Enabled);
                radioStringMixed.Text = string.Format(radioStringMixed.Text, "xx", "xx");
                radioStringMixedReverse.Text = string.Format(radioStringMixedReverse.Text,
                    "xx", "xx");
            }

            if (nullTermStringCount > 0) {
                Debug.Assert(radioStringNullTerm.Enabled);
                radioStringNullTerm.Text = string.Format(radioStringNullTerm.Text, nullTermStringCount);
            } else {
                Debug.Assert(!radioStringNullTerm.Enabled);
                radioStringNullTerm.Text = string.Format(radioStringNullTerm.Text, "xx");
            }

            if (len8StringCount > 0) {
                Debug.Assert(radioStringLen8.Enabled);
                radioStringLen8.Text = string.Format(radioStringLen8.Text, len8StringCount);
            } else {
                Debug.Assert(!radioStringLen8.Enabled);
                radioStringLen8.Text = string.Format(radioStringLen8.Text, "xx");
            }

            if (len16StringCount > 0) {
                Debug.Assert(radioStringLen16.Enabled);
                radioStringLen16.Text = string.Format(radioStringLen16.Text, len16StringCount);
            } else {
                Debug.Assert(!radioStringLen16.Enabled);
                radioStringLen16.Text = string.Format(radioStringLen16.Text, "xx");
            }

            if (dciStringCount > 0) {
                Debug.Assert(radioStringDci.Enabled);
                radioStringDci.Text = string.Format(radioStringDci.Text, dciStringCount);
            } else {
                Debug.Assert(!radioStringDci.Enabled);
                radioStringDci.Text = string.Format(radioStringDci.Text, "xx");
            }

            //if (revDciStringCount > 0) {
            //    Debug.Assert(radioStringDciReverse.Enabled);
            //    radioStringDciReverse.Text =
            //        string.Format(radioStringDciReverse.Text, revDciStringCount);
            //} else {
            //    Debug.Assert(!radioStringDciReverse.Enabled);
            //    radioStringDciReverse.Text = string.Format(radioStringDciReverse.Text, "xx");
            //}
        }

        /// <summary>
        /// Determines whether the data in the buffer can be represented as ASCII values.
        /// Using ".DD1 'A'" for 0x41 is obvious, but we also allow ".DD2 'A'" for
        /// 0x41 0x00.  16-bit character constants are more likely as intermediate
        /// operands, but could be found in data areas.
        /// 
        /// High and low ASCII are allowed, and may be freely mixed.
        /// 
        /// Testing explicitly is probably excessive, and possibly counter-productive if
        /// the user is trying to flag an area that is a mix of ASCII and non-ASCII and
        /// just wants hex for the rest, but we'll give it a try.
        /// </summary>
        /// <param name="wordWidth">Number of bytes per character.</param>
        /// <param name="isBigEndian">Word endian-ness.</param>
        /// <returns>True if data in all regions can be represented as high or low ASCII.</returns>
        private bool IsRawAsciiCompatible(int wordWidth, bool isBigEndian) {
            IEnumerator<TypedRangeSet.TypedRange> iter = Selection.RangeListIterator;
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;
                Debug.Assert(((rng.High - rng.Low + 1) / wordWidth) * wordWidth ==
                    rng.High - rng.Low + 1);
                for (int i = rng.Low; i <= rng.High; i += wordWidth) {
                    int val = RawData.GetWord(mFileData, rng.Low, wordWidth, isBigEndian);
                    if (val < 0x20 || (val >= 0x7f && val < 0xa0) || val >= 0xff) {
                        // bad value, fail
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Configures the dialog controls based on the provided format descriptor.  If
        /// the desired options are unavailable, a suitable default is selected instead.
        /// </summary>
        /// <param name="dfd">FormatDescriptor to use.</param>
        private void SetControlsFromDescriptor(FormatDescriptor dfd) {
            Debug.Assert(mIsInitialSetup);
            radioSimpleDataHex.Checked = true;
            radioSymbolPartLow.Checked = true;

            if (dfd == null) {
                radioDefaultFormat.Checked = true;
                return;
            }

            RadioButton preferredFormat;

            switch (dfd.FormatType) {
                case FormatDescriptor.Type.NumericLE:
                case FormatDescriptor.Type.NumericBE:
                    switch (dfd.Length) {
                        case 1:
                            preferredFormat = radioSingleBytes;
                            break;
                        case 2:
                            preferredFormat =
                                (dfd.FormatType == FormatDescriptor.Type.NumericLE ?
                                    radio16BitLittle : radio16BitBig);
                            break;
                        case 3:
                            preferredFormat = radio24BitLittle;
                            break;
                        case 4:
                            preferredFormat = radio32BitLittle;
                            break;
                        default:
                            Debug.Assert(false);
                            preferredFormat = radioDefaultFormat;
                            break;
                    }
                    if (preferredFormat.Enabled) {
                        switch (dfd.FormatSubType) {
                            case FormatDescriptor.SubType.None:
                            case FormatDescriptor.SubType.Hex:
                                radioSimpleDataHex.Checked = true;
                                break;
                            case FormatDescriptor.SubType.Decimal:
                                radioSimpleDataDecimal.Checked = true;
                                break;
                            case FormatDescriptor.SubType.Binary:
                                radioSimpleDataBinary.Checked = true;
                                break;
                            case FormatDescriptor.SubType.Ascii:
                                radioSimpleDataAscii.Checked = true;
                                break;
                            case FormatDescriptor.SubType.Address:
                                radioSimpleDataAddress.Checked = true;
                                break;
                            case FormatDescriptor.SubType.Symbol:
                                radioSimpleDataSymbolic.Checked = true;
                                switch (dfd.SymbolRef.ValuePart) {
                                    case WeakSymbolRef.Part.Low:
                                        radioSymbolPartLow.Checked = true;
                                        break;
                                    case WeakSymbolRef.Part.High:
                                        radioSymbolPartHigh.Checked = true;
                                        break;
                                    case WeakSymbolRef.Part.Bank:
                                        radioSymbolPartBank.Checked = true;
                                        break;
                                    default:
                                        Debug.Assert(false);
                                        break;
                                }
                                Debug.Assert(dfd.HasSymbol);
                                symbolEntryTextBox.Text = dfd.SymbolRef.Label;
                                break;
                            default:
                                Debug.Assert(false);
                                break;
                        }
                    } else {
                        // preferred format not enabled; leave Hex/Low checked
                    }
                    break;
                case FormatDescriptor.Type.String:
                    switch (dfd.FormatSubType) {
                        case FormatDescriptor.SubType.None:
                            preferredFormat = radioStringMixed;
                            break;
                        case FormatDescriptor.SubType.Reverse:
                            preferredFormat = radioStringMixedReverse;
                            break;
                        case FormatDescriptor.SubType.CString:
                            preferredFormat = radioStringNullTerm;
                            break;
                        case FormatDescriptor.SubType.L8String:
                            preferredFormat = radioStringLen8;
                            break;
                        case FormatDescriptor.SubType.L16String:
                            preferredFormat = radioStringLen16;
                            break;
                        case FormatDescriptor.SubType.Dci:
                            preferredFormat = radioStringDci;
                            break;
                        case FormatDescriptor.SubType.DciReverse:
                            preferredFormat = radioDefaultFormat;
                            break;
                        default:
                            Debug.Assert(false);
                            preferredFormat = radioDefaultFormat;
                            break;
                    }
                    break;
                case FormatDescriptor.Type.Dense:
                    preferredFormat = radioDenseHex;
                    break;
                case FormatDescriptor.Type.Fill:
                    preferredFormat = radioFill;
                    break;
                default:
                    // Should not be here.
                    Debug.Assert(false);
                    preferredFormat = radioDefaultFormat;
                    break;
            }

            if (preferredFormat.Enabled) {
                preferredFormat.Checked = true;
            } else {
                mPreferredFormatUnavailable = true;
                radioDefaultFormat.Checked = true;
            }
        }

        /// <summary>
        /// Creates a list of FormatDescriptors, based on the current control configuration.
        /// 
        /// The entries in the list are guaranteed to be sorted by start address and not
        /// overlap.
        /// 
        /// We assume that whatever the control gives us is correct, e.g. it's not going
        /// to tell us to put a buffer full of zeroes into a DCI string.
        /// </summary>
        /// <returns>Result list.</returns>
        private void CreateDescriptorListFromControls() {
            FormatDescriptor.Type type = FormatDescriptor.Type.Default;
            FormatDescriptor.SubType subType = FormatDescriptor.SubType.None;
            WeakSymbolRef symbolRef = null;
            int chunkLength = -1;

            // Decode the "display as" panel, if it's relevant.
            if (radioSimpleDataHex.Enabled) {
                if (radioSimpleDataHex.Checked) {
                    subType = FormatDescriptor.SubType.Hex;
                } else if (radioSimpleDataDecimal.Checked) {
                    subType = FormatDescriptor.SubType.Decimal;
                } else if (radioSimpleDataBinary.Checked) {
                    subType = FormatDescriptor.SubType.Binary;
                } else if (radioSimpleDataAscii.Checked) {
                    subType = FormatDescriptor.SubType.Ascii;
                } else if (radioSimpleDataAddress.Checked) {
                    subType = FormatDescriptor.SubType.Address;
                } else if (radioSimpleDataSymbolic.Checked) {
                    WeakSymbolRef.Part part;
                    if (radioSymbolPartLow.Checked) {
                        part = WeakSymbolRef.Part.Low;
                    } else if (radioSymbolPartHigh.Checked) {
                        part = WeakSymbolRef.Part.High;
                    } else if (radioSymbolPartBank.Checked) {
                        part = WeakSymbolRef.Part.Bank;
                    } else {
                        Debug.Assert(false);
                        part = WeakSymbolRef.Part.Low;
                    }
                    subType = FormatDescriptor.SubType.Symbol;
                    symbolRef = new WeakSymbolRef(symbolEntryTextBox.Text, part);
                } else {
                    Debug.Assert(false);
                }
            } else {
                subType = 0;        // set later, or doesn't matter
            }

            // Decode the main format.
            if (radioDefaultFormat.Checked) {
                // Default/None; note this would create a multi-byte Default format, which isn't
                // really allowed.  What we actually want to do is remove the explicit formatting
                // from all spanned offsets, so we use a dedicated type for that.
                type = FormatDescriptor.Type.REMOVE;
            } else if (radioSingleBytes.Checked) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 1;
            } else if (radio16BitLittle.Checked) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 2;
            } else if (radio16BitBig.Checked) {
                type = FormatDescriptor.Type.NumericBE;
                chunkLength = 2;
            } else if (radio24BitLittle.Checked) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 3;
            } else if (radio32BitLittle.Checked) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 4;
            } else if (radioDenseHex.Checked) {
                type = FormatDescriptor.Type.Dense;
            } else if (radioFill.Checked) {
                type = FormatDescriptor.Type.Fill;
            } else if (radioStringMixed.Checked) {
                type = FormatDescriptor.Type.String;
            } else if (radioStringMixedReverse.Checked) {
                type = FormatDescriptor.Type.String;
                subType = FormatDescriptor.SubType.Reverse;
            } else if (radioStringNullTerm.Checked) {
                type = FormatDescriptor.Type.String;
                subType = FormatDescriptor.SubType.CString;
            } else if (radioStringLen8.Checked) {
                type = FormatDescriptor.Type.String;
                subType = FormatDescriptor.SubType.L8String;
            } else if (radioStringLen16.Checked) {
                type = FormatDescriptor.Type.String;
                subType = FormatDescriptor.SubType.L16String;
            } else if (radioStringDci.Checked) {
                type = FormatDescriptor.Type.String;
                subType = FormatDescriptor.SubType.Dci;
            //} else if (radioStringDciReverse.Checked) {
            //    type = FormatDescriptor.Type.String;
            //    subType = FormatDescriptor.SubType.DciReverse;
            } else {
                Debug.Assert(false);
                // default/none
            }


            Results = new SortedList<int, FormatDescriptor>();

            IEnumerator<TypedRangeSet.TypedRange> iter = Selection.RangeListIterator;
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;

                if (type == FormatDescriptor.Type.String) {
                    // We want to create one FormatDescriptor object per string.  That way
                    // each string gets its own line.
                    if ((subType == FormatDescriptor.SubType.None ||
                            subType == FormatDescriptor.SubType.Reverse)) {
                        CreateMixedStringEntries(rng.Low, rng.High, subType);
                    } else if (subType == FormatDescriptor.SubType.CString) {
                        CreateCStringEntries(rng.Low, rng.High, subType);
                    } else if (subType == FormatDescriptor.SubType.L8String ||
                            subType == FormatDescriptor.SubType.L16String) {
                        CreateLengthStringEntries(rng.Low, rng.High, subType);
                    } else if (subType == FormatDescriptor.SubType.Dci ||
                            subType == FormatDescriptor.SubType.DciReverse) {
                        CreateDciStringEntries(rng.Low, rng.High, subType);
                    } else {
                        Debug.Assert(false);
                        CreateMixedStringEntries(rng.Low, rng.High, subType);   // shrug
                    }
                } else {
                    CreateSimpleEntries(type, subType, chunkLength, symbolRef, rng.Low, rng.High);
                }
            }
        }

        /// <summary>
        /// Creates one or more FormatDescriptor entries for the specified range, adding them
        /// to the Results list.
        /// 
        /// This will either create one entry that spans the entire range (for e.g. strings
        /// and bulk data), or create equal-sized chunks.
        /// </summary>
        /// <param name="type">Region data type.</param>
        /// <param name="subType">Region data sub-type.</param>
        /// <param name="chunkLength">Length of a chunk, or -1 for full buffer.</param>
        /// <param name="symbolRef">Symbol reference, or null if not applicable.</param>
        /// <param name="low">Offset of first byte in range.</param>
        /// <param name="high">Offset of last byte in range.</param>
        private void CreateSimpleEntries(FormatDescriptor.Type type,
                FormatDescriptor.SubType subType, int chunkLength,
                WeakSymbolRef symbolRef, int low, int high) {

            if (chunkLength == -1) {
                chunkLength = (high - low) + 1;
            }
            Debug.Assert(((high - low + 1) / chunkLength) * chunkLength == high - low + 1);

            // Either we have one chunk, or we have multiple chunks with the same type and
            // length.  Either way, we only need to create the descriptor once.  (This is
            // safe because FormatDescriptor instances are immutable.)
            //
            // Because certain details, like the fill byte and high-vs-low ASCII, are pulled
            // out of the data stream at format time, we don't have to dig for them now.
            FormatDescriptor dfd;
            if (subType == FormatDescriptor.SubType.Symbol) {
                dfd = FormatDescriptor.Create(chunkLength, symbolRef,
                    type == FormatDescriptor.Type.NumericBE);
            } else {
                dfd = FormatDescriptor.Create(chunkLength, type, subType);
            }

            while (low <= high) {
                Results.Add(low, dfd);
                low += chunkLength;
            }
        }

        /// <summary>
        /// Creates one or more FormatDescriptor entries for the specified range, adding them
        /// to the Results list.
        /// </summary>
        /// <param name="low">Offset of first byte in range.</param>
        /// <param name="high">Offset of last byte in range.</param>
        /// <param name="subType">String sub-type.</param>
        private void CreateMixedStringEntries(int low, int high,
                FormatDescriptor.SubType subType) {
            int stringStart = -1;
            int highBit = 0;
            int cur;
            for (cur = low; cur <= high; cur++) {
                byte val = mFileData[cur];
                if (CommonUtil.TextUtil.IsHiLoAscii(val)) {
                    // is ASCII
                    if (stringStart >= 0) {
                        // was in a string
                        if (highBit != (val & 0x80)) {
                            // end of string due to high bit flip, output
                            CreateStringOrByte(stringStart, cur - stringStart, subType);
                            // start a new string
                            stringStart = cur;
                        } else {
                            // still in string, keep going
                        }
                    } else {
                        // wasn't in a string, start one
                        stringStart = cur;
                    }
                    highBit = val & 0x80;
                } else {
                    // not ASCII
                    if (stringStart >= 0) {
                        // was in a string, output it
                        CreateStringOrByte(stringStart, cur - stringStart, subType);
                        stringStart = -1;
                    }
                    // output as single byte
                    CreateByteFD(cur, FormatDescriptor.SubType.Hex);
                }
            }
            if (stringStart >= 0) {
                // close out the string
                CreateStringOrByte(stringStart, cur - stringStart, subType);
            }
        }

        /// <summary>
        /// Creates a format descriptor for ASCII data.  If the data is only one byte long,
        /// a single-byte ASCII char item is emitted instead.
        /// </summary>
        /// <param name="offset">Offset of first byte.</param>
        /// <param name="length">Length of string.</param>
        /// <param name="subType">String sub-type.</param>
        private void CreateStringOrByte(int offset, int length,
                FormatDescriptor.SubType subType) {
            Debug.Assert(length > 0);
            if (length == 1) {
                // single byte, output as single ASCII char rather than 1-byte string
                CreateByteFD(offset, FormatDescriptor.SubType.Ascii);
            } else {
                FormatDescriptor dfd;
                dfd = FormatDescriptor.Create(length,
                    FormatDescriptor.Type.String, subType);
                Results.Add(offset, dfd);
            }
        }

        /// <summary>
        /// Creates a format descriptor for a single-byte numeric value.
        /// </summary>
        /// <param name="offset">File offset.</param>
        /// <param name="subType">How to format the item.</param>
        private void CreateByteFD(int offset, FormatDescriptor.SubType subType) {
            FormatDescriptor dfd = FormatDescriptor.Create(1,
                FormatDescriptor.Type.NumericLE, subType);
            Results.Add(offset, dfd);
        }

        /// <summary>
        /// Creates one or more FormatDescriptor entries for the specified range, adding them
        /// to the Results list.
        /// </summary>
        /// <param name="low">Offset of first byte in range.</param>
        /// <param name="high">Offset of last byte in range.</param>
        /// <param name="subType">String sub-type.</param>
        private void CreateCStringEntries(int low, int high,
                FormatDescriptor.SubType subType) {
            int startOffset = low;
            for (int i = low; i <= high; i++) {
                if (mFileData[i] == 0x00) {
                    // End of string.  Zero-length strings are allowed.
                    FormatDescriptor dfd = FormatDescriptor.Create(
                        i - startOffset + 1, FormatDescriptor.Type.String, subType);
                    Results.Add(startOffset, dfd);
                    startOffset = i + 1;
                } else {
                    // keep going
                }
            }

            // Earlier analysis guaranteed that the last byte in the buffer is 0x00.
            Debug.Assert(startOffset == high + 1);
        }

        /// <summary>
        /// Creates one or more FormatDescriptor entries for the specified range, adding them
        /// to the Results list.
        /// </summary>
        /// <param name="low">Offset of first byte in range.</param>
        /// <param name="high">Offset of last byte in range.</param>
        /// <param name="subType">String sub-type.</param>
        private void CreateLengthStringEntries(int low, int high,
                FormatDescriptor.SubType subType) {
            int i;
            for (i = low; i <= high;) {
                int length = mFileData[i];
                if (subType == FormatDescriptor.SubType.L16String) {
                    length |= mFileData[i + 1] << 8;
                    length += 2;
                } else {
                    length++;
                }
                // Zero-length strings are allowed.
                FormatDescriptor dfd = FormatDescriptor.Create(length,
                    FormatDescriptor.Type.String, subType);
                Results.Add(i, dfd);
                i += length;
            }

            Debug.Assert(i == high + 1);
        }

        /// <summary>
        /// Creates one or more FormatDescriptor entries for the specified range, adding them
        /// to the Results list.
        /// </summary>
        /// <param name="low">Offset of first byte in range.</param>
        /// <param name="high">Offset of last byte in range.</param>
        /// <param name="subType">String sub-type.</param>
        private void CreateDciStringEntries(int low, int high,
                FormatDescriptor.SubType subType) {
            int start, end, adj, endMask;
            if (subType == FormatDescriptor.SubType.Dci) {
                start = low;
                end = high + 1;
                adj = 1;
            } else if (subType == FormatDescriptor.SubType.DciReverse) {
                start = high;
                end = low - 1;
                adj = -1;
            } else {
                Debug.Assert(false);
                return;
            }

            // Zero-length strings aren't a thing for DCI.  The analyzer requires that all
            // strings in a region have the same polarity, so just grab the last byte.
            endMask = mFileData[end - 1] & 0x80;

            int stringStart = start;
            for (int i = start; i != end; i += adj) {
                byte val = mFileData[i];
                if ((val & 0x80) == endMask) {
                    // found the end of a string
                    int length = (i - stringStart) * adj + 1;
                    FormatDescriptor dfd = FormatDescriptor.Create(length,
                        FormatDescriptor.Type.String, subType);
                    Results.Add(stringStart < i ? stringStart : i, dfd);
                    stringStart = i + adj;
                }
            }

            Debug.Assert(stringStart == end);
        }
    }
}
