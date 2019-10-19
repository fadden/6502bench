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
using System.Windows.Controls;

using Asm65;
using CommonUtil;
using TextScanMode = SourceGen.ProjectProperties.AnalysisParameters.TextScanMode;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Data operand editor.
    /// </summary>
    public partial class EditDataOperand : Window, INotifyPropertyChanged {
        /// <summary>
        /// Result set that describes the formatting to perform.  Not all regions will have
        /// the same format, e.g. the "mixed ASCII" mode will alternate strings and bytes
        /// (rather than a dedicated "mixed ASCII" format type).
        /// </summary>
        public SortedList<int, FormatDescriptor> Results { get; private set; }

        /// <summary>
        /// Set to true when input is valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set {
                mIsValid = value;
                OnPropertyChanged();
            }
        }
        private bool mIsValid;

        /// <summary>
        /// Selected offsets.  An otherwise contiguous range of offsets can be broken up
        /// by user-specified labels and address discontinuities, so this needs to be
        /// processed by range.
        /// </summary>
        private TypedRangeSet mSelection;

        /// <summary>
        /// FormatDescriptor from the first offset.  May be null if the offset doesn't
        /// have a format descriptor specified.  This will be used to configure the
        /// dialog controls if the format is suited to the selection.  The goal is to
        /// make single-item editing work as expected.
        /// </summary>
        public FormatDescriptor mFirstFormatDescriptor;

        /// <summary>
        /// Raw file data.
        /// </summary>
        private byte[] mFileData;

        /// <summary>
        /// Symbol table to use when resolving symbolic values.
        /// </summary>
        private SymbolTable mSymbolTable;

        /// <summary>
        /// Map of offsets to addresses.
        /// </summary>
        private AddressMap mAddrMap;

        /// <summary>
        /// Formatter to use when displaying addresses and hex values.
        /// </summary>
        private Asm65.Formatter mFormatter;

        /// <summary>
        /// Set to true if, during the initial setup, the format defined by FirstFormatDescriptor
        /// was unavailable.
        /// </summary>
        private bool mPreferredFormatUnavailable;

        /// <summary>
        /// Text encoding combo box item.  We use the same TextScanMode enum that the
        /// uncategorized data analyzer uses.
        /// </summary>
        public class StringEncodingItem {
            public string Name { get; private set; }
            public TextScanMode Mode { get; private set; }

            public StringEncodingItem(string name, TextScanMode mode) {
                Name = name;
                Mode = mode;
            }
        }
        public StringEncodingItem[] StringEncodingItems { get; private set; }

        public class JunkAlignmentItem {
            public string Description { get; private set; }
            public FormatDescriptor.SubType FormatSubType { get; private set; }

            public JunkAlignmentItem(string descr, FormatDescriptor.SubType subFmt) {
                Description = descr;
                FormatSubType = subFmt;
            }
        }
        public List<JunkAlignmentItem> JunkAlignmentItems { get; private set; }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public EditDataOperand(Window owner, DisasmProject project,
                Asm65.Formatter formatter, TypedRangeSet trs, FormatDescriptor firstDesc) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mFileData = project.FileData;
            mSymbolTable = project.SymbolTable;
            mAddrMap = project.AddrMap;
            mFormatter = formatter;
            mSelection = trs;
            mFirstFormatDescriptor = firstDesc;

            StringEncodingItems = new StringEncodingItem[] {
                new StringEncodingItem(Res.Strings.SCAN_LOW_ASCII,
                    TextScanMode.LowAscii),
                new StringEncodingItem(Res.Strings.SCAN_LOW_HIGH_ASCII,
                    TextScanMode.LowHighAscii),
                new StringEncodingItem(Res.Strings.SCAN_C64_PETSCII,
                    TextScanMode.C64Petscii),
                new StringEncodingItem(Res.Strings.SCAN_C64_SCREEN_CODE,
                    TextScanMode.C64ScreenCode),
            };

            GetMinMaxAlignment(out FormatDescriptor.SubType min, out FormatDescriptor.SubType max);
            //Debug.WriteLine("ALIGN: min=" + min + " max=" + max);
            Debug.Assert(min == FormatDescriptor.SubType.None ^     // both or neither are None
                         max != FormatDescriptor.SubType.None);

            int junkSel = 0;
            string noAlign = (string)FindResource("str_AlignmentNone");
            string alignFmt = (string)FindResource("str_AlignmentItemFmt");
            JunkAlignmentItems = new List<JunkAlignmentItem>();
            JunkAlignmentItems.Add(new JunkAlignmentItem(noAlign, FormatDescriptor.SubType.None));
            if (min != FormatDescriptor.SubType.None) {
                int index = 1;
                // We assume the enum values are consecutive and ascending.
                FormatDescriptor.SubType end = (FormatDescriptor.SubType)(((int)max) + 1);
                while (min != end) {
                    int pwr = FormatDescriptor.AlignmentToPower(min);
                    string endStr = mFormatter.FormatHexValue(1 << pwr, 4);
                    JunkAlignmentItems.Add(new JunkAlignmentItem(
                        string.Format(alignFmt, 1 << pwr, endStr), min));

                    // See if this matches previous value.
                    if (mFirstFormatDescriptor != null &&
                            mFirstFormatDescriptor.FormatType == FormatDescriptor.Type.Junk &&
                            mFirstFormatDescriptor.FormatSubType == min) {
                        junkSel = index;
                    }

                    // Advance.
                    min = (FormatDescriptor.SubType)(((int)min) + 1);
                    index++;
                }
            }
            junkAlignComboBox.SelectedIndex = junkSel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            DateTime startWhen = DateTime.Now;

            // Determine which of the various options is suitable for the selected offsets.
            // Disable any radio buttons that won't work.
            AnalyzeRanges();

            // This gets invoked a bit later, from the "selection changed" callback.
            //AnalyzeStringRanges(TextScanMode.LowHighAscii);

            // Configure the dialog from the FormatDescriptor, if one is available.
            Debug.WriteLine("First FD: " + mFirstFormatDescriptor);
            SetControlsFromDescriptor(mFirstFormatDescriptor);

            if (mPreferredFormatUnavailable) {
                // This can happen when e.g. a bunch of stuff is formatted as null-terminated
                // strings.  We don't recognize a lone zero as a string, but we allow it if
                // it's next to a bunch of others.  If you come back later and try to format
                // just that one byte, you end up here.
                // TODO(maybe): make it more obvious what's going on?
                Debug.WriteLine("NOTE: preferred format unavailable");
            }

            UpdateControls();

            Debug.WriteLine("EditData dialog load time: " +
                (DateTime.Now - startWhen).TotalMilliseconds + " ms");
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            // Start with the focus in the text box if the initial format allows for a
            // symbolic reference.  This way they can start typing immediately.
            if (simpleDisplayAsGroupBox.IsEnabled) {
                symbolEntryTextBox.Focus();
            }
        }

        /// <summary>
        /// Handles Checked event for all buttons in Main group.
        /// </summary>
        private void MainGroup_CheckedChanged(object sender, EventArgs e) {
            // Enable/disable the style group and the low/high/bank radio group.
            // Update preview window.
            UpdateControls();
        }

        /// <summary>
        /// Handles Checked event for radio buttons in the Display group.
        /// group box.
        /// </summary>
        private void SimpleDisplay_CheckedChanged(object sender, EventArgs e) {
            // Enable/disable the low/high/bank radio group.
            UpdateControls();
        }

        private void SymbolEntryTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            // Make sure Symbol is checked if they're typing text in.
            //Debug.Assert(radioSimpleDataSymbolic.IsEnabled);
            radioSimpleDataSymbolic.IsChecked = true;
            // Update OK button based on symbol validity.
            UpdateControls();
        }

        /// <summary>
        /// Sets the string encoding combo box to an item that matches the specified mode.  If
        /// the mode can't be found, an arbitrary entry will be chosen.
        /// </summary>
        private void SetStringEncoding(TextScanMode mode) {
            StringEncodingItem choice = null;
            foreach (StringEncodingItem item in StringEncodingItems) {
                if (item.Mode == mode) {
                    choice = item;
                    break;
                }
            }
            if (choice == null) {
                choice = StringEncodingItems[1];
            }
            stringEncodingComboBox.SelectedItem = choice;
        }

        private void StringEncodingComboBox_SelectionChanged(object sender,
                SelectionChangedEventArgs e) {
            if (!IsLoaded) {
                return;
            }
            StringEncodingItem item = (StringEncodingItem)stringEncodingComboBox.SelectedItem;
            AnalyzeStringRanges(item.Mode);
            UpdateControls();

            AppSettings.Global.SetEnum(AppSettings.OPED_DEFAULT_STRING_ENCODING,
                typeof(TextScanMode), (int)item.Mode);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            CreateDescriptorListFromControls();
            FormatDescriptor.DebugDumpSortedList(Results);
            DialogResult = true;
        }

        /// <summary>
        /// Updates all of the controls to reflect the current internal state.
        /// </summary>
        private void UpdateControls() {
            if (!IsLoaded) {
                return;
            }

            // Configure the simple data "display as" style box.
            bool wantStyle = false;
            int simpleWidth = -1;
            bool isBigEndian = false;
            if (radioSingleBytes.IsChecked == true) {
                wantStyle = true;
                simpleWidth = 1;
            } else if (radio16BitLittle.IsChecked == true) {
                wantStyle = true;
                simpleWidth = 2;
            } else if (radio16BitBig.IsChecked == true) {
                wantStyle = true;
                simpleWidth = 2;
                isBigEndian = true;
            } else if (radio24BitLittle.IsChecked == true) {
                wantStyle = true;
                simpleWidth = 3;
            } else if (radio32BitLittle.IsChecked == true) {
                wantStyle = true;
                simpleWidth = 4;
            }
            bool focusOnSymbol = !simpleDisplayAsGroupBox.IsEnabled && wantStyle;
            simpleDisplayAsGroupBox.IsEnabled = wantStyle;
            if (wantStyle) {
                // Because this covers multiple items in a data area, we allow the
                // "extended" set, which includes some control characters.
                radioSimpleDataAscii.IsEnabled = IsCompatibleWithCharSet(simpleWidth,
                    isBigEndian, CharEncoding.IsExtendedLowOrHighAscii);
                radioSimpleDataPetscii.IsEnabled = IsCompatibleWithCharSet(simpleWidth,
                    isBigEndian, CharEncoding.IsExtendedC64Petscii);
                radioSimpleDataScreenCode.IsEnabled = IsCompatibleWithCharSet(simpleWidth,
                    isBigEndian, CharEncoding.IsExtendedC64ScreenCode);
            }

            // Enable the symbolic reference entry box if the "display as" group is enabled.
            // That way instead of "click 16-bit", "click symbol", "enter symbol", the user
            // can skip the second step.
            symbolEntryTextBox.IsEnabled = simpleDisplayAsGroupBox.IsEnabled;

            // Part panel is enabled when Symbol is checked.  (Now handled in XAML.)
            //symbolPartPanel.IsEnabled = (radioSimpleDataSymbolic.IsChecked == true);

            // If we just enabled the group box, set the focus on the symbol entry box.  This
            // removes another click from the steps, though it's a bit aggressive if you're
            // trying to arrow your way through the items.
            if (focusOnSymbol) {
                symbolEntryTextBox.Focus();
            }

            // Disable the alignment pop-up unless Junk is selected.
            junkAlignComboBox.IsEnabled = (radioJunk.IsChecked == true);

            bool isOk = true;
            if (radioSimpleDataSymbolic.IsChecked == true) {
                // Just check for correct format.  References to non-existent labels are allowed.
                isOk = Asm65.Label.ValidateLabel(symbolEntryTextBox.Text);

                // Actually, let's discourage references to auto-labels.
                if (isOk && mSymbolTable.TryGetValue(symbolEntryTextBox.Text, out Symbol sym)) {
                    isOk = sym.SymbolSource != Symbol.Source.Auto;
                }
            }
            IsValid = isOk;
        }

        #region Setup

        /// <summary>
        /// Determines the minimum and maximum alignment values, based on the sizes of the
        /// regions and the address they end on.
        /// </summary>
        /// <param name="min">Minimum allowed format, or None.</param>
        /// <param name="max">Maximum allowed format, or None.</param>
        private void GetMinMaxAlignment(out FormatDescriptor.SubType min,
                out FormatDescriptor.SubType max) {
            min = max = FormatDescriptor.SubType.None;

            int maxLenPow = -1;
            int minAlignPow = 65535;

            IEnumerator<TypedRangeSet.TypedRange> iter = mSelection.RangeListIterator;
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;
                int length = rng.High - rng.Low + 1;
                Debug.Assert(length > 0);

                // The goal is to find an instruction that fills an entire region with zeroes
                // or junk bytes for the sole purpose of ending at a specific boundary.
                //
                // If we have a 100-byte region that ends at address $103f (inclusive), it
                // can't be the result of an assembler alignment directive.  "align $40" would
                // have stopped at $1000, "align $80" would have continued on to $107f.
                //
                // Alignment of junk whose last byte $103f could be due to Align2, Align4 (1-3
                // bytes at $103d/e/f), Align8 (1-7 bytes at $1039-f), and so on, up to Align64.
                // The size of the buffer determines the minimum value, the end address
                // determines the maximum.
                //
                // Bear in mind that assembler alignment directives will do nothing if the
                // address is already aligned: Align256 at $1000 generates no output.  So we
                // cannot use Align8 on a buffer of length 8.

                // Count the trailing 1 bits in the address.  This gets us the power of 2
                // alignment value.  Note alignPow will be zero if the last byte is stored at
                // an even address.
                int endAddress = mAddrMap.OffsetToAddress(rng.High) & 0x0000ffff;
                int alignPow = BitTwiddle.CountTrailingZeroes(~endAddress);

                // Round length up to next highest power of 2, and compute Log2().  Unfortunately
                // .NET Standard 2.0 doesn't have Math.Log2().  Note we want the next-highest
                // even if it's already a power of 2.
                int lenRound = BitTwiddle.NextHighestPowerOf2(length);
                int lenPow = BitTwiddle.CountTrailingZeroes(lenRound);
                Debug.Assert(lenPow > 0);   // length==1 -> lenRound=2 --> lenPow=1

                // Want the biggest minimum value and the smallest maximum value.
                if (maxLenPow < lenPow) {
                    maxLenPow = lenPow;
                }
                if (minAlignPow > alignPow) {
                    minAlignPow = alignPow;
                }

                if (maxLenPow > minAlignPow) {
                    return;
                }
            }

            min = FormatDescriptor.PowerToAlignment(maxLenPow);
            max = FormatDescriptor.PowerToAlignment(minAlignPow);
        }

        /// <summary>
        /// Analyzes the selection to see which data formatting options are suitable.
        /// Disables radio buttons and updates labels.
        /// 
        /// Call this once, when the dialog is first loaded.
        /// </summary>
        private void AnalyzeRanges() {
            Debug.Assert(mSelection.Count != 0);

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
            selectFormatLabel.Text = infoStr;

            IEnumerator<TypedRangeSet.TypedRange> iter = mSelection.RangeListIterator;

            // For each range, check to see if the data within qualifies for the various
            // options.  If any of them fail to meet the criteria, the option is disabled
            // for all ranges.
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;
                Debug.WriteLine("Testing [" + rng.Low + ", " + rng.High + "]");

                // Note single-byte and dense are always enabled.

                int count = rng.High - rng.Low + 1;
                Debug.Assert(count > 0);
                if ((count & 0x01) != 0) {
                    // not divisible by 2, disallow 16-bit entries
                    radio16BitLittle.IsEnabled = false;
                    radio16BitBig.IsEnabled = false;
                }
                if ((count & 0x03) != 0) {
                    // not divisible by 4, disallow 32-bit entries
                    radio32BitLittle.IsEnabled = false;
                }
                if ((count / 3) * 3 != count) {
                    // not divisible by 3, disallow 24-bit entries
                    radio24BitLittle.IsEnabled = false;
                }


                // Check for run of bytes (2 or more of the same thing).  Remember that
                // we check this one region at a time, and each region could have different
                // bytes, but so long as the bytes are all the same within a region we're good.
                if (radioFill.IsEnabled && count > 1 &&
                        DataAnalysis.RecognizeRun(mFileData, rng.Low, rng.High) == count) {
                    // LGTM
                } else {
                    radioFill.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Analyzes the selection to see which string formatting options are suitable.
        /// Disables radio buttons and updates labels.
        /// 
        /// Call this when the character encoding selection changes.
        /// </summary>
        private void AnalyzeStringRanges(TextScanMode scanMode) {
            Debug.WriteLine("Analyzing string ranges");
            Debug.Assert(IsLoaded);

            int mixedCharOkCount = 0;
            int mixedCharNotCount = 0;
            int nullTermStringCount = 0;
            int len8StringCount = 0;
            int len16StringCount = 0;
            int dciStringCount = 0;

            CharEncoding.InclusionTest charTest;
            switch (scanMode) {
                case TextScanMode.LowAscii:
                    charTest = CharEncoding.IsExtendedAscii;
                    break;
                case TextScanMode.LowHighAscii:
                    charTest = CharEncoding.IsExtendedLowOrHighAscii;
                    break;
                case TextScanMode.C64Petscii:
                    charTest = CharEncoding.IsExtendedC64Petscii;
                    break;
                case TextScanMode.C64ScreenCode:
                    charTest = CharEncoding.IsExtendedC64ScreenCode;
                    break;
                default:
                    Debug.Assert(false);
                    charTest = CharEncoding.IsExtendedAscii;
                    break;
            }

            radioStringMixed.IsEnabled = true;
            radioStringMixedReverse.IsEnabled = true;
            radioStringNullTerm.IsEnabled = (scanMode != TextScanMode.C64ScreenCode);
            radioStringLen8.IsEnabled = true;
            radioStringLen16.IsEnabled = true;
            radioStringDci.IsEnabled = true;

            IEnumerator<TypedRangeSet.TypedRange> iter = mSelection.RangeListIterator;
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;
                Debug.WriteLine("Testing [" + rng.Low + ", " + rng.High + "]");

                // See if there's enough string data to make it worthwhile.  We use an
                // arbitrary threshold of 2+ printable characters, and require twice as many
                // printable as non-printable.
                if (radioStringMixed.IsEnabled) {
                    if (scanMode == TextScanMode.LowHighAscii) {
                        // We use a special test that counts low, high, and non-ASCII.
                        // Whichever form of ASCII has the highest count is the winner, and
                        // the loser is counted as non-ASCII.
                        int asciiCount;
                        DataAnalysis.CountHighLowBytes(mFileData, rng.Low, rng.High, charTest,
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
                            mixedCharOkCount += asciiCount;
                            mixedCharNotCount += nonAscii;
                        } else {
                            // Fail
                            radioStringMixed.IsEnabled = false;
                            radioStringMixedReverse.IsEnabled = false;
                            mixedCharOkCount = mixedCharNotCount = -1;
                        }
                    } else {
                        int matchCount = DataAnalysis.CountCharacterBytes(mFileData,
                            rng.Low, rng.High, charTest);
                        int missCount = (rng.High - rng.Low + 1) - matchCount;
                        if (matchCount >= 2 && matchCount >= missCount * 2) {
                            mixedCharOkCount += matchCount;
                            mixedCharNotCount += missCount;
                        } else {
                            // Fail
                            radioStringMixed.IsEnabled = false;
                            radioStringMixedReverse.IsEnabled = false;
                            mixedCharOkCount = mixedCharNotCount = -1;
                        }
                    }
                }

                // Check for null-terminated strings.  Zero-length strings are allowed, but
                // not counted -- we want to have some actual character data.  Individual
                // ASCII strings need to be entirely high-ASCII or low-ASCII, but not all strings
                // in a region have to be the same.
                if (radioStringNullTerm.IsEnabled) {
                    int strCount = DataAnalysis.RecognizeNullTerminatedStrings(mFileData,
                        rng.Low, rng.High, charTest, scanMode == TextScanMode.LowHighAscii);
                    if (strCount > 0) {
                        nullTermStringCount += strCount;
                    } else {
                        radioStringNullTerm.IsEnabled = false;
                        nullTermStringCount = -1;
                    }
                }

                // Check for strings prefixed with an 8-bit length.
                if (radioStringLen8.IsEnabled) {
                    int strCount = DataAnalysis.RecognizeLen8Strings(mFileData, rng.Low, rng.High,
                        charTest, scanMode == TextScanMode.LowHighAscii);
                    if (strCount > 0) {
                        len8StringCount += strCount;
                    } else {
                        radioStringLen8.IsEnabled = false;
                        len8StringCount = -1;
                    }
                }

                // Check for strings prefixed with a 16-bit length.
                if (radioStringLen16.IsEnabled) {
                    int strCount = DataAnalysis.RecognizeLen16Strings(mFileData, rng.Low, rng.High,
                        charTest, scanMode == TextScanMode.LowHighAscii);
                    if (strCount > 0) {
                        len16StringCount += strCount;
                    } else {
                        radioStringLen16.IsEnabled = false;
                        len16StringCount = -1;
                    }
                }

                // Check for DCI strings.  All strings within the entire range must have the
                // same "polarity", e.g. low ASCII terminated by high ASCII.
                if (radioStringDci.IsEnabled) {
                    int strCount = DataAnalysis.RecognizeDciStrings(mFileData, rng.Low, rng.High,
                        charTest);
                    if (strCount > 0) {
                        dciStringCount += strCount;
                    } else {
                        radioStringDci.IsEnabled = false;
                        dciStringCount = -1;
                    }
                }
            }

            // Update the dialog with string and character counts, summed across all regions.

            string fmt;
            const string UNSUP_STR = "xx";
            fmt = (string)FindResource("str_StringMixed");
            string revfmt = (string)FindResource("str_StringMixedReverse");
            if (mixedCharOkCount > 0) {
                Debug.Assert(radioStringMixed.IsEnabled);
                radioStringMixed.Content = string.Format(fmt,
                    mixedCharOkCount, mixedCharNotCount);
                radioStringMixedReverse.Content = string.Format(revfmt,
                    mixedCharOkCount, mixedCharNotCount);
            } else {
                Debug.Assert(!radioStringMixed.IsEnabled);
                radioStringMixed.Content = string.Format(fmt, UNSUP_STR, UNSUP_STR);
                radioStringMixedReverse.Content = string.Format(revfmt, UNSUP_STR, UNSUP_STR);
            }

            fmt = (string)FindResource("str_StringNullTerm");
            if (nullTermStringCount > 0) {
                Debug.Assert(radioStringNullTerm.IsEnabled);
                radioStringNullTerm.Content = string.Format(fmt, nullTermStringCount);
            } else {
                Debug.Assert(!radioStringNullTerm.IsEnabled);
                radioStringNullTerm.Content = string.Format(fmt, UNSUP_STR);
            }

            fmt = (string)FindResource("str_StringLen8");
            if (len8StringCount > 0) {
                Debug.Assert(radioStringLen8.IsEnabled);
                radioStringLen8.Content = string.Format(fmt, len8StringCount);
            } else {
                Debug.Assert(!radioStringLen8.IsEnabled);
                radioStringLen8.Content = string.Format(fmt, UNSUP_STR);
            }

            fmt = (string)FindResource("str_StringLen16");
            if (len16StringCount > 0) {
                Debug.Assert(radioStringLen16.IsEnabled);
                radioStringLen16.Content = string.Format(fmt, len16StringCount);
            } else {
                Debug.Assert(!radioStringLen16.IsEnabled);
                radioStringLen16.Content = string.Format(fmt, UNSUP_STR);
            }

            fmt = (string)FindResource("str_StringDci");
            if (dciStringCount > 0) {
                Debug.Assert(radioStringDci.IsEnabled);
                radioStringDci.Content = string.Format(fmt, dciStringCount);
            } else {
                Debug.Assert(!radioStringDci.IsEnabled);
                radioStringDci.Content = string.Format(fmt, UNSUP_STR);
            }

            // If this invalidated the selected item, reset to Default.
            if ((radioStringMixed.IsChecked == true && !radioStringMixed.IsEnabled) ||
                (radioStringMixedReverse.IsChecked == true && !radioStringMixedReverse.IsEnabled) ||
                (radioStringNullTerm.IsChecked == true && !radioStringNullTerm.IsEnabled) ||
                (radioStringLen8.IsChecked == true && !radioStringLen8.IsEnabled) ||
                (radioStringLen8.IsChecked == true && !radioStringLen8.IsEnabled) ||
                (radioStringDci.IsChecked == true && !radioStringDci.IsEnabled)) {

                Debug.WriteLine("Previous selection invalidated");
                radioDefaultFormat.IsChecked = true;
            }
        }

        /// <summary>
        /// Determines whether the data in the buffer can be represented as character values.
        /// Using ".DD1 'A'" for 0x41 is obvious, but we also allow ".DD2 'A'" for
        /// 0x41 0x00.  16-bit character constants are more likely as intermediate
        /// operands, but could be found in data areas.
        /// </summary>
        /// <param name="wordWidth">Number of bytes per character.</param>
        /// <param name="isBigEndian">Word endian-ness.</param>
        /// <param name="charTest">Character test delegate.</param>
        /// <returns>True if data in all regions can be represented as a character.</returns>
        private bool IsCompatibleWithCharSet(int wordWidth, bool isBigEndian,
                CharEncoding.InclusionTest charTest) {
            IEnumerator<TypedRangeSet.TypedRange> iter = mSelection.RangeListIterator;
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;
                Debug.Assert(((rng.High - rng.Low + 1) / wordWidth) * wordWidth ==
                    rng.High - rng.Low + 1);
                for (int i = rng.Low; i <= rng.High; i += wordWidth) {
                    int val = RawData.GetWord(mFileData, i, wordWidth, isBigEndian);
                    if (val != (byte)val || !charTest((byte)val)) {
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
        ///
        /// Call from the Loaded event.
        /// </summary>
        /// <param name="dfd">FormatDescriptor to use.</param>
        private void SetControlsFromDescriptor(FormatDescriptor dfd) {
            radioSimpleDataHex.IsChecked = true;
            radioSymbolPartLow.IsChecked = true;

            // Get the previous mode selected in the combo box.  If the format descriptor
            // doesn't specify a string, we'll use this.
            TextScanMode textMode = (TextScanMode)AppSettings.Global.GetEnum(
                AppSettings.OPED_DEFAULT_STRING_ENCODING, typeof(TextScanMode),
                (int)TextScanMode.LowHighAscii);

            if (dfd == null) {
                radioDefaultFormat.IsChecked = true;
                SetStringEncoding(textMode);
                return;
            }

            if (dfd.IsString) {
                textMode = TextScanModeFromDescriptor(dfd);
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
                    if (preferredFormat.IsEnabled) {
                        switch (dfd.FormatSubType) {
                            case FormatDescriptor.SubType.None:
                            case FormatDescriptor.SubType.Hex:
                                radioSimpleDataHex.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.Decimal:
                                radioSimpleDataDecimal.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.Binary:
                                radioSimpleDataBinary.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.Ascii:
                            case FormatDescriptor.SubType.HighAscii:
                                radioSimpleDataAscii.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.C64Petscii:
                                radioSimpleDataPetscii.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.C64Screen:
                                radioSimpleDataScreenCode.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.Address:
                                radioSimpleDataAddress.IsChecked = true;
                                break;
                            case FormatDescriptor.SubType.Symbol:
                                radioSimpleDataSymbolic.IsChecked = true;
                                switch (dfd.SymbolRef.ValuePart) {
                                    case WeakSymbolRef.Part.Low:
                                        radioSymbolPartLow.IsChecked = true;
                                        break;
                                    case WeakSymbolRef.Part.High:
                                        radioSymbolPartHigh.IsChecked = true;
                                        break;
                                    case WeakSymbolRef.Part.Bank:
                                        radioSymbolPartBank.IsChecked = true;
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
                case FormatDescriptor.Type.StringGeneric:
                    preferredFormat = radioStringMixed;
                    break;
                case FormatDescriptor.Type.StringReverse:
                    preferredFormat = radioStringMixedReverse;
                    break;
                case FormatDescriptor.Type.StringNullTerm:
                    preferredFormat = radioStringNullTerm;
                    break;
                case FormatDescriptor.Type.StringL8:
                    preferredFormat = radioStringLen8;
                    break;
                case FormatDescriptor.Type.StringL16:
                    preferredFormat = radioStringLen16;
                    break;
                case FormatDescriptor.Type.StringDci:
                    preferredFormat = radioStringDci;
                    break;
                case FormatDescriptor.Type.Dense:
                    preferredFormat = radioDenseHex;
                    break;
                case FormatDescriptor.Type.Fill:
                    preferredFormat = radioFill;
                    break;
                case FormatDescriptor.Type.Junk:
                    preferredFormat = radioJunk;
                    break;
                default:
                    // Should not be here.
                    Debug.Assert(false);
                    preferredFormat = radioDefaultFormat;
                    break;
            }

            if (preferredFormat.IsEnabled) {
                preferredFormat.IsChecked = true;
            } else {
                mPreferredFormatUnavailable = true;
                radioDefaultFormat.IsChecked = true;
            }

            SetStringEncoding(textMode);
        }

        private TextScanMode TextScanModeFromDescriptor(FormatDescriptor dfd) {
            Debug.Assert(dfd.IsString);
            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.Ascii:
                case FormatDescriptor.SubType.HighAscii:
                    return TextScanMode.LowHighAscii;
                case FormatDescriptor.SubType.C64Petscii:
                    return TextScanMode.C64Petscii;
                case FormatDescriptor.SubType.C64Screen:
                    return TextScanMode.C64ScreenCode;
                default:
                    Debug.Assert(false);
                    return TextScanMode.LowHighAscii;
            }
        }

        #endregion Setup

        #region FormatDescriptor creation

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

            FormatDescriptor.SubType charSubType;
            CharEncoding.InclusionTest charTest;
            StringEncodingItem item = (StringEncodingItem)stringEncodingComboBox.SelectedItem;
            switch (item.Mode) {
                case TextScanMode.LowAscii:
                    charSubType = FormatDescriptor.SubType.Ascii;
                    charTest = CharEncoding.IsExtendedAscii;
                    break;
                case TextScanMode.LowHighAscii:
                    charSubType = FormatDescriptor.SubType.ASCII_GENERIC;
                    charTest = CharEncoding.IsExtendedLowOrHighAscii;
                    break;
                case TextScanMode.C64Petscii:
                    charSubType = FormatDescriptor.SubType.C64Petscii;
                    charTest = CharEncoding.IsExtendedC64Petscii;
                    break;
                case TextScanMode.C64ScreenCode:
                    charSubType = FormatDescriptor.SubType.C64Screen;
                    charTest = CharEncoding.IsExtendedC64ScreenCode;
                    break;
                default:
                    Debug.Assert(false);
                    charSubType = FormatDescriptor.SubType.ASCII_GENERIC;
                    charTest = CharEncoding.IsExtendedLowOrHighAscii;
                    break;
            }

            // Decode the "display as" panel, if it's relevant.
            if (radioSimpleDataHex.IsEnabled) {
                if (radioSimpleDataHex.IsChecked == true) {
                    subType = FormatDescriptor.SubType.Hex;
                } else if (radioSimpleDataDecimal.IsChecked == true) {
                    subType = FormatDescriptor.SubType.Decimal;
                } else if (radioSimpleDataBinary.IsChecked == true) {
                    subType = FormatDescriptor.SubType.Binary;
                } else if (radioSimpleDataAscii.IsChecked == true) {
                    subType = FormatDescriptor.SubType.ASCII_GENERIC;
                } else if (radioSimpleDataPetscii.IsChecked == true) {
                    subType = FormatDescriptor.SubType.C64Petscii;
                } else if (radioSimpleDataScreenCode.IsChecked == true) {
                    subType = FormatDescriptor.SubType.C64Screen;
                } else if (radioSimpleDataAddress.IsChecked == true) {
                    subType = FormatDescriptor.SubType.Address;
                } else if (radioSimpleDataSymbolic.IsChecked == true) {
                    WeakSymbolRef.Part part;
                    if (radioSymbolPartLow.IsChecked == true) {
                        part = WeakSymbolRef.Part.Low;
                    } else if (radioSymbolPartHigh.IsChecked == true) {
                        part = WeakSymbolRef.Part.High;
                    } else if (radioSymbolPartBank.IsChecked == true) {
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
            if (radioDefaultFormat.IsChecked == true) {
                // Default/None; note this would create a multi-byte Default format, which isn't
                // really allowed.  What we actually want to do is remove the explicit formatting
                // from all spanned offsets, so we use a dedicated type for that.
                type = FormatDescriptor.Type.REMOVE;
            } else if (radioSingleBytes.IsChecked == true) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 1;
            } else if (radio16BitLittle.IsChecked == true) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 2;
            } else if (radio16BitBig.IsChecked == true) {
                type = FormatDescriptor.Type.NumericBE;
                chunkLength = 2;
            } else if (radio24BitLittle.IsChecked == true) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 3;
            } else if (radio32BitLittle.IsChecked == true) {
                type = FormatDescriptor.Type.NumericLE;
                chunkLength = 4;
            } else if (radioDenseHex.IsChecked == true) {
                type = FormatDescriptor.Type.Dense;
            } else if (radioFill.IsChecked == true) {
                type = FormatDescriptor.Type.Fill;
            } else if (radioJunk.IsChecked == true) {
                type = FormatDescriptor.Type.Junk;
                JunkAlignmentItem comboItem = (JunkAlignmentItem)junkAlignComboBox.SelectedItem;
                subType = comboItem.FormatSubType;
            } else if (radioStringMixed.IsChecked == true) {
                type = FormatDescriptor.Type.StringGeneric;
                subType = charSubType;
            } else if (radioStringMixedReverse.IsChecked == true) {
                type = FormatDescriptor.Type.StringReverse;
                subType = charSubType;
            } else if (radioStringNullTerm.IsChecked == true) {
                type = FormatDescriptor.Type.StringNullTerm;
                subType = charSubType;
            } else if (radioStringLen8.IsChecked == true) {
                type = FormatDescriptor.Type.StringL8;
                subType = charSubType;
            } else if (radioStringLen16.IsChecked == true) {
                type = FormatDescriptor.Type.StringL16;
                subType = charSubType;
            } else if (radioStringDci.IsChecked == true) {
                type = FormatDescriptor.Type.StringDci;
                subType = charSubType;
            } else {
                Debug.Assert(false);
                // default/none
            }


            Results = new SortedList<int, FormatDescriptor>();

            IEnumerator<TypedRangeSet.TypedRange> iter = mSelection.RangeListIterator;
            while (iter.MoveNext()) {
                TypedRangeSet.TypedRange rng = iter.Current;

                switch (type) {
                    case FormatDescriptor.Type.StringGeneric:
                        CreateMixedStringEntries(rng.Low, rng.High, type, subType, charTest);
                        break;
                    case FormatDescriptor.Type.StringReverse:
                        CreateMixedStringEntries(rng.Low, rng.High, type, subType, charTest);
                        break;
                    case FormatDescriptor.Type.StringNullTerm:
                        CreateCStringEntries(rng.Low, rng.High, type, subType);
                        break;
                    case FormatDescriptor.Type.StringL8:
                    case FormatDescriptor.Type.StringL16:
                        CreateLengthStringEntries(rng.Low, rng.High, type, subType);
                        break;
                    case FormatDescriptor.Type.StringDci:
                        CreateDciStringEntries(rng.Low, rng.High, type, subType);
                        break;
                    default:
                        CreateSimpleEntries(type, subType, chunkLength, symbolRef,
                            rng.Low, rng.High);
                        break;
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
            // The one exception to this is ASCII values for non-string data, because we have
            // to dig the low vs. high value out of the data itself.
            FormatDescriptor dfd;
            if (subType == FormatDescriptor.SubType.Symbol) {
                dfd = FormatDescriptor.Create(chunkLength, symbolRef,
                    type == FormatDescriptor.Type.NumericBE);
            } else {
                dfd = FormatDescriptor.Create(chunkLength, type, subType);
            }
            while (low <= high) {
                if (subType == FormatDescriptor.SubType.ASCII_GENERIC) {
                    // should not be REMOVE with a meaningful subtype
                    Debug.Assert(dfd.IsNumeric);
                    int val = RawData.GetWord(mFileData, low, dfd.Length,
                        type == FormatDescriptor.Type.NumericBE);
                    FormatDescriptor.SubType actualSubType = (val > 0x7f) ?
                        FormatDescriptor.SubType.HighAscii : FormatDescriptor.SubType.Ascii;
                    if (actualSubType != dfd.FormatSubType) {
                        // replace the descriptor
                        dfd = FormatDescriptor.Create(chunkLength, type, actualSubType);
                    }
                }

                Results.Add(low, dfd);
                low += chunkLength;
            }
        }

        /// <summary>
        /// Creates one or more FormatDescriptor entries for the specified range, adding them
        /// to the Results list.  Runs of character data are output as generic strings, while any
        /// non-character data is output as individual bytes.
        /// </summary>
        /// <remarks>
        /// This is the only string create function that accepts a mix of valid and invalid
        /// characters.
        /// </remarks>
        /// <param name="low">Offset of first byte in range.</param>
        /// <param name="high">Offset of last byte in range.</param>
        /// <param name="type">String type (Generic or Reverse).</param>
        /// <param name="subType">String sub-type.</param>
        /// <param name="charTest">Character test delegate.</param>
        private void CreateMixedStringEntries(int low, int high, FormatDescriptor.Type type,
                FormatDescriptor.SubType subType, CharEncoding.InclusionTest charTest) {
            int stringStart = -1;
            int cur;

            if (subType == FormatDescriptor.SubType.ASCII_GENERIC) {
                int highBit = 0;
                for (cur = low; cur <= high; cur++) {
                    byte val = mFileData[cur];
                    if (charTest(val)) {
                        // is ASCII
                        if (stringStart >= 0) {
                            // was in a string
                            if (highBit != (val & 0x80)) {
                                // end of string due to high bit flip, output
                                CreateGenericStringOrByte(stringStart, cur - stringStart,
                                    type, subType);
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
                            CreateGenericStringOrByte(stringStart, cur - stringStart,
                                type, subType);
                            stringStart = -1;
                        }
                        // output as single byte
                        CreateByteFD(cur, FormatDescriptor.SubType.Hex);
                    }
                }
            } else {
                for (cur = low; cur <= high; cur++) {
                    byte val = mFileData[cur];
                    if (charTest(val)) {
                        // is character
                        if (stringStart < 0) {
                            // mark this as the start of the string
                            stringStart = cur;
                        }
                    } else {
                        // not character
                        if (stringStart >= 0) {
                            // was in a string, output it
                            CreateGenericStringOrByte(stringStart, cur - stringStart,
                                type, subType);
                            stringStart = -1;
                        }
                        // output as single byte
                        CreateByteFD(cur, FormatDescriptor.SubType.Hex);
                    }
                }

            }
            if (stringStart >= 0) {
                // close out the string
                CreateGenericStringOrByte(stringStart, cur - stringStart, type, subType);
            }
        }

        private FormatDescriptor.SubType ResolveAsciiGeneric(int offset,
                FormatDescriptor.SubType subType) {
            if (subType == FormatDescriptor.SubType.ASCII_GENERIC) {
                if ((mFileData[offset] & 0x80) != 0) {
                    subType = FormatDescriptor.SubType.HighAscii;
                } else {
                    subType = FormatDescriptor.SubType.Ascii;
                }
            }
            return subType;
        }

        /// <summary>
        /// Creates a format descriptor for character data.  If the data is only one byte long,
        /// a single-byte character item is emitted instead.
        /// </summary>
        /// <param name="offset">Offset of first byte.</param>
        /// <param name="length">Length of string.</param>
        /// <param name="type">String type (Generic or Reverse).</param>
        /// <param name="subType">String sub-type.  If set to ASCII_GENERIC, this will
        ///   refine the sub-type.</param>
        private void CreateGenericStringOrByte(int offset, int length,
                FormatDescriptor.Type type, FormatDescriptor.SubType subType) {
            Debug.Assert(length > 0);
            subType = ResolveAsciiGeneric(offset, subType);
            if (length == 1) {
                // Single byte, output as single char rather than 1-byte string.  We use the
                // same encoding as the rest of the string.
                CreateByteFD(offset, subType);
            } else {
                FormatDescriptor dfd;
                dfd = FormatDescriptor.Create(length, type, subType);
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
        private void CreateCStringEntries(int low, int high, FormatDescriptor.Type type,
                FormatDescriptor.SubType subType) {
            int startOffset = low;
            for (int i = low; i <= high; i++) {
                if (mFileData[i] == 0x00) {
                    // End of string.  Zero-length strings are allowed.
                    FormatDescriptor dfd = FormatDescriptor.Create(
                        i - startOffset + 1, type, ResolveAsciiGeneric(startOffset, subType));
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
        private void CreateLengthStringEntries(int low, int high, FormatDescriptor.Type type,
                FormatDescriptor.SubType subType) {
            int i;
            for (i = low; i <= high;) {
                int length = mFileData[i];
                if (type == FormatDescriptor.Type.StringL16) {
                    length |= mFileData[i + 1] << 8;
                    length += 2;
                } else {
                    length++;
                }
                // Zero-length strings are allowed.
                FormatDescriptor dfd = FormatDescriptor.Create(length, type,
                    ResolveAsciiGeneric(i, subType));
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
        private void CreateDciStringEntries(int low, int high, FormatDescriptor.Type type,
                FormatDescriptor.SubType subType) {
            int end, endMask;

            end = high + 1;

            // Zero-length strings aren't a thing for DCI.  The analyzer requires that all
            // strings in a region have the same polarity, so just grab the last byte.
            endMask = mFileData[end - 1] & 0x80;

            int stringStart = low;
            for (int i = low; i != end; i++) {
                byte val = mFileData[i];
                if ((val & 0x80) == endMask) {
                    // found the end of a string
                    int length = (i - stringStart) + 1;
                    FormatDescriptor dfd = FormatDescriptor.Create(length, type,
                        ResolveAsciiGeneric(stringStart, subType));
                    Results.Add(stringStart, dfd);
                    stringStart = i + 1;
                }
            }

            Debug.Assert(stringStart == end);
        }

        #endregion FormatDescriptor creation
    }
}
