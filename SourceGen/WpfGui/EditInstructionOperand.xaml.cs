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
using System.Text;
using System.Windows;

using Asm65;

namespace SourceGen.WpfGui {
    /// <summary>
    /// Instruction operand editor.
    /// </summary>
    public partial class EditInstructionOperand : Window, INotifyPropertyChanged {
        /// <summary>
        /// Updated format descriptor.  Will be null if the user selected "default".
        /// </summary>
        public FormatDescriptor FormatDescriptorResult { get; private set; }

        /// <summary>
        /// Updated local variable table.  Will be null if no changes were made.
        /// </summary>
        public LocalVariableTable LocalVariableResult { get; private set; }

        /// <summary>
        /// Offset of the local variable table we updated in LocalVariableResult.
        /// </summary>
        public int LocalVariableTableOffsetResult { get; private set; }

        /// <summary>
        /// Offset of label that was edited.  A non-negative value here indicates that an
        /// edit has been made.
        /// </summary>
        public int SymbolEditOffsetResult { get; private set; }

        /// <summary>
        /// Edited project property, or null if no changes were made.
        /// </summary>
        public DefSymbol ProjectPropertyResult { get; private set; }

        /// <summary>
        /// The project property that was modified, or null if none.
        /// </summary>
        public DefSymbol PrevProjectPropertyResult { get; private set; }

        /// <summary>
        /// Updated label.
        /// </summary>
        public Symbol SymbolEditResult { get; private set; }

        private readonly string SYMBOL_NOT_USED;
        private readonly string SYMBOL_UNKNOWN;
        private readonly string SYMBOL_INVALID;

        private readonly string CREATE_LOCAL_VARIABLE;
        private readonly string EDIT_LOCAL_VARIABLE;
        private readonly string LV_MATCH_FOUND_ADDRESS;
        private readonly string LV_MATCH_FOUND_CONSTANT;

        private readonly string CREATE_LABEL;
        private readonly string EDIT_LABEL;
        private readonly string CREATE_PROJECT_SYMBOL;
        private readonly string EDIT_PROJECT_SYMBOL;
        private readonly string CURRENT_LABEL;
        private readonly string CURRENT_LABEL_ADJUSTED_FMT;

        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Offset of instruction being edited.
        /// </summary>
        private int mOffset;

        /// <summary>
        /// Format object.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Operation definition, from file data.
        /// </summary>
        private OpDef mOpDef;

        /// <summary>
        /// Status flags at the point where the instruction is defined.  This tells us whether
        /// an operand is 8-bit or 16-bit.
        /// </summary>
        private StatusFlags mOpStatusFlags;

        /// <summary>
        /// Operand value, extracted from file data.  For a relative branch, this will be
        /// an address instead.
        /// </summary>
        private int mOperandValue;

        /// <summary>
        /// True when the input is valid.  Controls whether the OK button is enabled.
        /// </summary>
        public bool IsValid {
            get { return mIsValid; }
            set { mIsValid = value; OnPropertyChanged(); }
        }
        private bool mIsValid;

        /// <summary>
        /// Set when our load-time initialization is complete.
        /// </summary>
        private bool mLoadDone;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Parent window.</param>
        /// <param name="project">Project reference.</param>
        /// <param name="offset">File offset of instruction start.</param>
        /// <param name="formatter">Formatter object, for preview window.</param>
        public EditInstructionOperand(Window owner, DisasmProject project, int offset,
                Formatter formatter) {
            InitializeComponent();
            Owner = owner;
            DataContext = this;

            mProject = project;
            mOffset = offset;
            mFormatter = formatter;

            SYMBOL_NOT_USED = (string)FindResource("str_SymbolNotUsed");
            SYMBOL_INVALID = (string)FindResource("str_SymbolNotValid");
            SYMBOL_UNKNOWN = (string)FindResource("str_SymbolUnknown");

            CREATE_LOCAL_VARIABLE = (string)FindResource("str_CreateLocalVariable");
            EDIT_LOCAL_VARIABLE = (string)FindResource("str_EditLocalVariable");
            LV_MATCH_FOUND_ADDRESS = (string)FindResource("str_LvMatchFoundAddress");
            LV_MATCH_FOUND_CONSTANT = (string)FindResource("str_LvMatchFoundConstant");

            CREATE_LABEL = (string)FindResource("str_CreateLabel");
            EDIT_LABEL = (string)FindResource("str_EditLabel");
            CREATE_PROJECT_SYMBOL = (string)FindResource("str_CreateProjectSymbol");
            EDIT_PROJECT_SYMBOL = (string)FindResource("str_EditProjectSymbol");
            CURRENT_LABEL = (string)FindResource("str_CurrentLabel");
            CURRENT_LABEL_ADJUSTED_FMT = (string)FindResource("str_CurrentLabelAdjustedFmt");

            Debug.Assert(offset >= 0 && offset < project.FileDataLength);
            mOpDef = project.CpuDef.GetOpDef(project.FileData[offset]);
            Anattrib attr = project.GetAnattrib(offset);
            mOpStatusFlags = attr.StatusFlags;
            Debug.Assert(offset + mOpDef.GetLength(mOpStatusFlags) <= project.FileDataLength);

            if (attr.OperandAddress >= 0) {
                // Use this as the operand value when available.  This lets us present
                // relative branch instructions in the expected form.
                mOperandValue = attr.OperandAddress;
            } else {
                // For BlockMove this will have both parts.
                mOperandValue = mOpDef.GetOperand(project.FileData, offset, attr.StatusFlags);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            BasicFormat_Loaded();
            NumericReferences_Loaded();
            LocalVariables_Loaded();
            mLoadDone = true;
        }

        private void Window_ContentRendered(object sender, EventArgs e) {
            UpdateControls();
            symbolTextBox.SelectAll();
            symbolTextBox.Focus();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e) {
            FormatDescriptorResult = CreateDescriptorFromControls();

            // Export the updated local variable table if we made changes.
            if (mEditedLvTable != null) {
                LocalVariableTable lvt = mProject.LvTables[mLvTableOffset];
                if (mEditedLvTable != lvt) {
                    LocalVariableResult = mEditedLvTable;
                    LocalVariableTableOffsetResult = mLvTableOffset;

                    Debug.WriteLine("NEW TABLE:");
                    mEditedLvTable.DebugDump(mLvTableOffset);
                } else {
                    Debug.WriteLine("No change to LvTable, not exporting");
                }
            }

            if (mLabelHasBeenEdited) {
                SymbolEditOffsetResult = mEditedLabelOffset;
                SymbolEditResult = mEditedLabel;
            }

            ProjectPropertyResult = mEditedProjectSymbol;

            DialogResult = true;
        }

        /// <summary>
        /// Looks up the symbol in the symbol table.  If not found there, it checks for a
        /// match against the existing or edited project symbol.
        /// </summary>
        private bool LookupSymbol(string label, out Symbol sym) {
            if (mProject.SymbolTable.TryGetValue(label, out sym)) {
                return true;
            }
            if (mEditedProjectSymbol != null && label.Equals(mEditedProjectSymbol.Label)) {
                sym = mEditedProjectSymbol;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the state of the UI controls as the user interacts with the dialog.
        /// </summary>
        private void UpdateControls() {
            if (!mLoadDone) {
                return;
            }

            // Parts panel IsEnabled depends directly on formatSymbolButton.IsChecked.
            IsValid = true;
            IsSymbolAuto = false;
            IsSymbolVar = false;
            IsPartPanelEnabled = false;
            SymbolValueDecimal = string.Empty;
            if (FormatSymbol) {
                IsPartPanelEnabled = mOpDef.IsExtendedImmediate;

                if (!Asm65.Label.ValidateLabel(SymbolLabel)) {
                    SymbolValueHex = SYMBOL_INVALID;
                    IsValid = false;
                } else if (LookupSymbol(SymbolLabel, out Symbol sym)) {
                    if (sym.SymbolSource == Symbol.Source.Auto) {
                        // We try to block references to auto labels, but it's possible to get
                        // around it because FormatDescriptors are weak references (replace auto
                        // label with user label, reference non-existent auto label, remove user
                        // label).  We could try harder, but currently not necessary.
                        //
                        // Referencing an auto label is unwise because we use weak references
                        // by name, and auto labels can appear, disappear, or be renamed.
                        IsValid = false;
                        IsSymbolAuto = true;
                    } else if (sym.SymbolSource == Symbol.Source.Variable) {
                        // Local variables can be de-duplicated and uniquified, so referring to
                        // them by name doesn't make sense.  The numeric operand formatter will
                        // disregard attempts to use them in this way.
                        IsValid = false;
                        IsSymbolVar = true;
                    }

                    SymbolValueHex = mFormatter.FormatHexValue(sym.Value, 4);
                    SymbolValueDecimal = mFormatter.FormatDecimalValue(sym.Value);
                } else {
                    // Valid but unknown symbol.  This is fine -- symbols don't have to exist.
                    SymbolValueHex = SYMBOL_UNKNOWN;
                }
            } else {
                SymbolValueHex = SYMBOL_NOT_USED;
            }

            UpdatePreview();
            UpdateCopyToOperand();
        }

        /// <summary>
        /// Updates the contents of the preview text box.
        /// </summary>
        private void UpdatePreview() {
            // Generate a descriptor from the controls.  This isn't strictly necessary, but it
            // gets all of the data in one small package.
            FormatDescriptor dfd = CreateDescriptorFromControls();

            if (dfd == null) {
                // Showing the right thing for the default format is surprisingly hard.  There
                // are a bunch of complicated steps that are performed in sequence, including
                // the "nearby label" lookups, the elision of hidden symbols, and other
                // obscure bits that may get tweaked from time to time.  These things are not
                // easy to factor out because we're slicing the data at a different angle: the
                // initial pass walks the entire file looking for one thing at a point before
                // analysis has completed, while here we're trying to mimic all of the
                // steps for a single offset, after analysis has finished.  It's a lot of work
                // to show text that they'll see as soon as they hit "OK".
                PreviewText = string.Empty;
                return;
            }

            StringBuilder sb = new StringBuilder(16);

            // Show the opcode.  Don't bother trying to figure out width disambiguation here.
            sb.Append(mFormatter.FormatOpcode(mOpDef, OpDef.WidthDisambiguation.None));
            sb.Append(' ');

            bool showHashPrefix = mOpDef.IsImmediate ||
                mOpDef.AddrMode == OpDef.AddressMode.BlockMove;
            if (showHashPrefix) {
                sb.Append('#');
            }

            Anattrib attr = mProject.GetAnattrib(mOffset);
            int previewHexDigits = (attr.Length - 1) * 2;
            int operandValue = mOperandValue;
            bool isPcRelative = false;
            bool isBlockMove = false;
            if (attr.OperandAddress >= 0) {
                if (mOpDef.AddrMode == OpDef.AddressMode.PCRel) {
                    previewHexDigits = 4;   // show branches as $xxxx even when on zero page
                    isPcRelative = true;
                } else if (mOpDef.AddrMode == OpDef.AddressMode.PCRelLong ||
                        mOpDef.AddrMode == OpDef.AddressMode.StackPCRelLong) {
                    isPcRelative = true;
                }
            } else {
                if (mOpDef.AddrMode == OpDef.AddressMode.BlockMove) {
                    // MVN and MVP screw things up by having two operands in one instruction.
                    // We deal with this by passing in the value from the second byte
                    // (source bank) as the value, and applying the chosen format to both bytes.
                    isBlockMove = true;
                    operandValue = mOperandValue >> 8;
                    previewHexDigits = 2;
                }
            }

            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.Hex:
                    sb.Append(mFormatter.FormatHexValue(operandValue, previewHexDigits));
                    break;
                case FormatDescriptor.SubType.Decimal:
                    sb.Append(mFormatter.FormatDecimalValue(operandValue));
                    break;
                case FormatDescriptor.SubType.Binary:
                    sb.Append(mFormatter.FormatBinaryValue(operandValue, 8));
                    break;
                case FormatDescriptor.SubType.Ascii:
                case FormatDescriptor.SubType.HighAscii:
                case FormatDescriptor.SubType.C64Petscii:
                case FormatDescriptor.SubType.C64Screen:
                    CharEncoding.Encoding enc = PseudoOp.SubTypeToEnc(dfd.FormatSubType);
                    sb.Append(mFormatter.FormatCharacterValue(operandValue, enc));
                    break;
                case FormatDescriptor.SubType.Symbol:
                    if (LookupSymbol(dfd.SymbolRef.Label, out Symbol sym)) {
                        // Block move is a little weird.  "MVN label1,label2" is supposed to use
                        // the bank byte, while "MVN #const1,#const2" uses the entire symbol.
                        // The easiest thing to do is require the user to specify the "bank"
                        // part for 24-bit symbols, and always generate this as an immediate.
                        //
                        // MVN/MVP are also the only instructions with two operands, something
                        // we don't really handle.
                        // TODO(someday): allow a different symbol for each part of the operand.

                        // Hack to make relative branches look right in the preview window.
                        // Otherwise they show up like "<LABEL" because they appear to be
                        // only 8 bits.
                        int operandLen = dfd.Length - 1;
                        if (operandLen == 1 && isPcRelative) {
                            operandLen = 2;
                        }

                        // Set the operand length to 1 for block move so we use the part
                        // operators (<, >, ^) rather than bit-shifting.
                        if (isBlockMove) {
                            operandLen = 1;
                        }

                        PseudoOp.FormatNumericOpFlags flags;
                        if (isPcRelative) {
                            flags = PseudoOp.FormatNumericOpFlags.IsPcRel;
                        } else if (showHashPrefix) {
                            flags = PseudoOp.FormatNumericOpFlags.HasHashPrefix;
                        } else {
                            flags = PseudoOp.FormatNumericOpFlags.None;
                        }
                        string str = PseudoOp.FormatNumericOperand(mFormatter,
                            mProject.SymbolTable, null, dfd,
                            operandValue, operandLen, flags);
                        sb.Append(str);

                        if (sym.SymbolSource == Symbol.Source.Auto) {
                            mIsSymbolAuto = true;
                        }
                    } else {
                        sb.Append(dfd.SymbolRef.Label + " (?)");
                        Debug.Assert(!string.IsNullOrEmpty(dfd.SymbolRef.Label));
                        //symbolValueLabel.Text = Properties.Resources.MSG_SYMBOL_NOT_FOUND;
                    }
                    break;
                default:
                    Debug.Assert(false);
                    sb.Append("BUG");
                    break;
            }

            if (isBlockMove) {
                sb.Append(",#<dest>");
            }

            PreviewText = sb.ToString();
        }

        #region Basic Format

        public bool FormatDefault {
            get { return mFormatDefault; }
            set { mFormatDefault = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatDefault;

        public bool FormatHex {
            get { return mFormatHex; }
            set { mFormatHex = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatHex;

        public bool FormatDecimal {
            get { return mFormatDecimal; }
            set { mFormatDecimal = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatDecimal;

        public bool FormatBinary {
            get { return mFormatBinary; }
            set { mFormatBinary = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatBinary;

        public bool IsFormatAsciiAllowed {
            get { return mIsFormatAsciiAllowed; }
            set { mIsFormatAsciiAllowed = value; OnPropertyChanged(); }
        }
        private bool mIsFormatAsciiAllowed;

        public bool FormatAscii {
            get { return mFormatAscii; }
            set { mFormatAscii = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatAscii;

        public bool IsFormatPetsciiAllowed {
            get { return mIsFormatPetsciiAllowed; }
            set { mIsFormatPetsciiAllowed = value; OnPropertyChanged(); }
        }
        private bool mIsFormatPetsciiAllowed;

        public bool FormatPetscii {
            get { return mFormatPetscii; }
            set { mFormatPetscii = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatPetscii;

        public bool IsFormatScreenCodeAllowed {
            get { return mIsFormatScreenCodeAllowed; }
            set { mIsFormatScreenCodeAllowed = value; OnPropertyChanged(); }
        }
        private bool mIsFormatScreenCodeAllowed;

        public bool FormatScreenCode {
            get { return mFormatScreenCode; }
            set { mFormatScreenCode = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatScreenCode;

        public bool FormatSymbol {
            get { return mFormatSymbol; }
            set { mFormatSymbol = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatSymbol;

        public string SymbolLabel {
            get { return mSymbolLabel; }
            set {
                mSymbolLabel = value;
                OnPropertyChanged();
                // Set the radio button when the user starts typing.
                if (mLoadDone) {
                    FormatSymbol = true;
                }
                UpdateControls();
            }
        }
        private string mSymbolLabel;

        public bool IsSymbolAuto {
            get { return mIsSymbolAuto; }
            set { mIsSymbolAuto = value; OnPropertyChanged(); }
        }
        private bool mIsSymbolAuto;

        public bool IsSymbolVar{
            get { return mIsSymbolVar; }
            set { mIsSymbolVar = value; OnPropertyChanged(); }
        }
        private bool mIsSymbolVar;

        public string SymbolValueHex {
            get { return mSymbolValueHex; }
            set { mSymbolValueHex = value; OnPropertyChanged(); }
        }
        private string mSymbolValueHex;

        public string SymbolValueDecimal {
            get { return mSymbolValueDecimal; }
            set { mSymbolValueDecimal = value; OnPropertyChanged(); }
        }
        private string mSymbolValueDecimal;

        public bool IsPartPanelEnabled {
            get { return mIsPartPanelEnabled; }
            set { mIsPartPanelEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsPartPanelEnabled;

        public bool FormatPartLow {
            get { return mFormatPartLow; }
            set { mFormatPartLow = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatPartLow;

        public bool FormatPartHigh {
            get { return mFormatPartHigh; }
            set { mFormatPartHigh = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatPartHigh;

        public bool FormatPartBank {
            get { return mFormatPartBank; }
            set { mFormatPartBank = value; OnPropertyChanged(); UpdateControls(); }
        }
        private bool mFormatPartBank;

        public string PreviewText {
            get { return mPreviewText; }
            set { mPreviewText = value; OnPropertyChanged(); }
        }
        private string mPreviewText;

        /// <summary>
        /// Configures the basic formatting options, based on the existing format descriptor.
        /// </summary>
        private void BasicFormat_Loaded() {
            // Can this be represented as a character?  We only allow the printable set
            // here, not the extended set (which includes control characters).
            if (mOperandValue == (byte) mOperandValue) {
                IsFormatAsciiAllowed =
                    CharEncoding.IsPrintableLowOrHighAscii((byte)mOperandValue);
                IsFormatPetsciiAllowed =
                    CharEncoding.IsPrintableC64Petscii((byte)mOperandValue);
                IsFormatScreenCodeAllowed =
                    CharEncoding.IsPrintableC64ScreenCode((byte)mOperandValue);
            } else {
                IsFormatAsciiAllowed = IsFormatPetsciiAllowed = IsFormatScreenCodeAllowed =
                    false;
            }

            SymbolLabel = string.Empty;
            FormatPartLow = true;       // could default to high for MVN/MVP
            FormatDefault = true;       // if nothing better comes along

            // Is there an operand format at this location?  If not, we're done.
            if (!mProject.OperandFormats.TryGetValue(mOffset, out FormatDescriptor dfd)) {
                return;
            }

            // NOTE: it's entirely possible to have a weird format (e.g. string) if the
            // instruction used to be hinted as data.  Handle it gracefully.
            switch (dfd.FormatType) {
                case FormatDescriptor.Type.NumericLE:
                    switch (dfd.FormatSubType) {
                        case FormatDescriptor.SubType.Hex:
                            FormatHex = true;
                            break;
                        case FormatDescriptor.SubType.Decimal:
                            FormatDecimal = true;
                            break;
                        case FormatDescriptor.SubType.Binary:
                            FormatBinary = true;
                            break;
                        case FormatDescriptor.SubType.Ascii:
                        case FormatDescriptor.SubType.HighAscii:
                            if (IsFormatAsciiAllowed) {
                                FormatAscii = true;
                            }
                            break;
                        case FormatDescriptor.SubType.C64Petscii:
                            if (IsFormatPetsciiAllowed) {
                                FormatPetscii = true;
                            }
                            break;
                        case FormatDescriptor.SubType.C64Screen:
                            if (IsFormatScreenCodeAllowed) {
                                FormatScreenCode = true;
                            }
                            break;
                        case FormatDescriptor.SubType.Symbol:
                            Debug.Assert(dfd.HasSymbol);
                            FormatSymbol = true;
                            switch (dfd.SymbolRef.ValuePart) {
                                case WeakSymbolRef.Part.Low:
                                    FormatPartLow = true;
                                    break;
                                case WeakSymbolRef.Part.High:
                                    FormatPartHigh = true;
                                    break;
                                case WeakSymbolRef.Part.Bank:
                                    FormatPartBank = true;
                                    break;
                                default:
                                    Debug.Assert(false);
                                    break;
                            }
                            SymbolLabel = dfd.SymbolRef.Label;
                            break;
                        case FormatDescriptor.SubType.None:
                        default:
                            break;
                    }
                    break;
                case FormatDescriptor.Type.NumericBE:
                case FormatDescriptor.Type.StringGeneric:
                case FormatDescriptor.Type.StringReverse:
                case FormatDescriptor.Type.StringNullTerm:
                case FormatDescriptor.Type.StringL8:
                case FormatDescriptor.Type.StringL16:
                case FormatDescriptor.Type.StringDci:
                case FormatDescriptor.Type.Dense:
                case FormatDescriptor.Type.Fill:
                default:
                    // Unexpected; used to be data?
                    break;
            }

            // In theory, if FormatDefault is still checked, we failed to find a useful match
            // for the format descriptor.  In practice, the radio button checkification stuff
            // happens later.  If we want to tell the user that there's a bad descriptor present,
            // we'll need to track it locally, or test all known radio buttons for True.
        }

        /// <summary>
        /// Creates a FormatDescriptor from the current state of the dialog controls.
        /// </summary>
        /// <returns>New FormatDescriptor.  Will return null if the default format is
        ///   selected, or symbol is selected with an empty label.</returns>
        private FormatDescriptor CreateDescriptorFromControls() {
            int instructionLength = mProject.GetAnattrib(mOffset).Length;

            if (FormatSymbol) {
                if (string.IsNullOrEmpty(SymbolLabel)) {
                    // empty symbol --> default format (intuitive way to delete label reference)
                    return null;
                }
                WeakSymbolRef.Part part;
                if (FormatPartLow) {
                    part = WeakSymbolRef.Part.Low;
                } else if (FormatPartHigh) {
                    part = WeakSymbolRef.Part.High;
                } else if (FormatPartBank) {
                    part = WeakSymbolRef.Part.Bank;
                } else {
                    Debug.Assert(false);
                    part = WeakSymbolRef.Part.Low;
                }
                return FormatDescriptor.Create(instructionLength,
                    new WeakSymbolRef(SymbolLabel, part), false);
            }

            FormatDescriptor.SubType subType;
            if (FormatDefault) {
                return null;
            } else if (FormatHex) {
                subType = FormatDescriptor.SubType.Hex;
            } else if (FormatDecimal) {
                subType = FormatDescriptor.SubType.Decimal;
            } else if (FormatBinary) {
                subType = FormatDescriptor.SubType.Binary;
            } else if (FormatAscii) {
                if (mOperandValue > 0x7f) {
                    subType = FormatDescriptor.SubType.HighAscii;
                } else {
                    subType = FormatDescriptor.SubType.Ascii;
                }
            } else if (FormatPetscii) {
                subType = FormatDescriptor.SubType.C64Petscii;
            } else if (FormatScreenCode) {
                subType = FormatDescriptor.SubType.C64Screen;
            } else {
                Debug.Assert(false);
                subType = FormatDescriptor.SubType.None;
            }

            return FormatDescriptor.Create(instructionLength,
                FormatDescriptor.Type.NumericLE, subType);
        }

        #endregion Basic Format

        #region Numeric References

        public bool ShowNarNotAddress {
            get { return mShowNarNotAddress; }
            set { mShowNarNotAddress = value; OnPropertyChanged(); }
        }
        private bool mShowNarNotAddress;

        public bool ShowNarEditLabel {
            get { return mShowNarEditLabel; }
            set { mShowNarEditLabel = value; OnPropertyChanged(); }
        }
        private bool mShowNarEditLabel;

        public bool ShowNarCurrentLabel {
            get { return mShowNarCurrentLabel; }
            set { mShowNarCurrentLabel = value; OnPropertyChanged(); }
        }
        private bool mShowNarCurrentLabel;

        public string NarLabelOffsetText {
            get { return mNarLabelOffsetText; }
            set { mNarLabelOffsetText = value; OnPropertyChanged(); }
        }
        private string mNarLabelOffsetText;

        public string NarTargetLabel {
            get { return mNarTargetLabel; }
            set { mNarTargetLabel = value; OnPropertyChanged(); }
        }
        private string mNarTargetLabel;

        public string CreateEditLabelText {
            get { return mCreateEditLabelText; }
            set { mCreateEditLabelText = value; OnPropertyChanged(); }
        }
        private string mCreateEditLabelText;

        public bool ShowNarExternalSymbol {
            get { return mShowNarExternalSymbol; }
            set { mShowNarExternalSymbol = value; OnPropertyChanged(); }
        }
        private bool mShowNarExternalSymbol;

        public bool ShowNarPlatformSymbol {
            get { return mShowNarPlatformSymbol; }
            set { mShowNarPlatformSymbol = value; OnPropertyChanged(); }
        }
        private bool mShowNarPlatformSymbol;

        public string NarPlatformSymbol {
            get { return mNarPlatformSymbol; }
            set { mNarPlatformSymbol = value; OnPropertyChanged(); }
        }
        private string mNarPlatformSymbol;

        public bool ShowNarNoProjectMatch {
            get { return mShowNarNoProjectMatch; }
            set { mShowNarNoProjectMatch = value; OnPropertyChanged(); }
        }
        private bool mShowNarNoProjectMatch;

        public bool ShowNarProjectSymbol {
            get { return mShowNarProjectSymbol; }
            set { mShowNarProjectSymbol = value; OnPropertyChanged(); }
        }
        private bool mShowNarProjectSymbol;

        public string NarProjectSymbol {
            get { return mNarProjectSymbol; }
            set { mNarProjectSymbol = value; OnPropertyChanged(); }
        }
        private string mNarProjectSymbol;

        public string CreateEditProjectSymbolText {
            get { return mCreateEditProjectSymbolText; }
            set { mCreateEditProjectSymbolText = value; OnPropertyChanged(); }
        }
        private string mCreateEditProjectSymbolText;

        public bool IsCopyToOperandEnabled {
            get { return mIsCopyToOperandEnabled; }
            set { mIsCopyToOperandEnabled = value; OnPropertyChanged(); }
        }
        private bool mIsCopyToOperandEnabled;

        /// <summary>
        /// Edited label value.  Will be null if the label hasn't been created, or has been
        /// deleted (by entering a blank string in the label edit box).
        /// </summary>
        private Symbol mEditedLabel;

        /// <summary>
        /// Set to true if the label has been edited.
        /// </summary>
        private bool mLabelHasBeenEdited;

        /// <summary>
        /// Address associated with the label (for the Symbol's value).
        /// </summary>
        private int mLabelTargetAddress = -1;

        /// <summary>
        /// Offset of edited label.
        /// </summary>
        private int mEditedLabelOffset = -1;

        /// <summary>
        /// Edited project symbol.  If a symbol already exists, this will be initialized to the
        /// existing value.  Otherwise this will be null.
        /// </summary>
        /// <remarks>
        /// Project symbols can't be deleted from here, so a null reference always means that
        /// there's no symbol and we haven't made an edit.
        /// </remarks>
        private DefSymbol mEditedProjectSymbol;

        /// <summary>
        /// Configures the UI in the local variables box at load time.
        /// </summary>
        private void NumericReferences_Loaded() {
            SymbolEditOffsetResult = -1;

            Anattrib attr = mProject.GetAnattrib(mOffset);

            if (attr.OperandOffset >= 0) {
                // Operand target is inside the file.
                ShowNarEditLabel = true;

                // Seek back to the start of the instruction or data item if the operand points
                // into the middle of one.  This is *not* the same as the "nearby" search,
                // which will traverse multiple items to find a match.
                mEditedLabelOffset =
                    DataAnalysis.GetBaseOperandOffset(mProject, attr.OperandOffset);
                mLabelTargetAddress = mProject.GetAnattrib(mEditedLabelOffset).Address;
                if (mProject.UserLabels.TryGetValue(mEditedLabelOffset, out Symbol sym)) {
                    // Has a label.
                    ShowNarCurrentLabel = true;
                    if (mEditedLabelOffset != attr.OperandOffset) {
                        NarLabelOffsetText = string.Format(CURRENT_LABEL_ADJUSTED_FMT,
                            mFormatter.FormatAdjustment(attr.OperandOffset - mEditedLabelOffset));
                    } else {
                        NarLabelOffsetText = CURRENT_LABEL;
                    }
                    NarTargetLabel = sym.Label;
                    mEditedLabel = sym;
                    CreateEditLabelText = EDIT_LABEL;
                } else {
                    NarLabelOffsetText = CURRENT_LABEL;
                    CreateEditLabelText = CREATE_LABEL;
                }
            } else if (attr.OperandAddress >= 0) {
                ShowNarExternalSymbol = true;

                // There can be multiple symbols with the same value, so we walk through the
                // list and identify the first matching platform and project symbols.  We're
                // only interested in address symbols, not constants.
                Symbol firstPlatform = null;
                Symbol firstProject = null;
                foreach (Symbol sym in mProject.SymbolTable) {
                    if (sym.Value == attr.OperandAddress &&
                            sym.SymbolType != Symbol.Type.Constant) {
                        if (firstPlatform == null && sym.SymbolSource == Symbol.Source.Platform) {
                            firstPlatform = sym;
                        } else if (firstProject == null &&
                                    sym.SymbolSource == Symbol.Source.Project) {
                            firstProject = sym;
                        }

                        if (firstPlatform != null && firstProject != null) {
                            break;
                        }
                    }
                }

                if (firstPlatform != null) {
                    ShowNarPlatformSymbol = true;
                    NarPlatformSymbol = firstPlatform.Label;
                }
                if (firstProject != null) {
                    ShowNarProjectSymbol = true;
                    NarProjectSymbol = firstProject.Label;
                    CreateEditProjectSymbolText = EDIT_PROJECT_SYMBOL;

                    mEditedProjectSymbol = (DefSymbol)firstProject;
                    PrevProjectPropertyResult = mEditedProjectSymbol;
                } else {
                    ShowNarNoProjectMatch = true;
                    CreateEditProjectSymbolText = CREATE_PROJECT_SYMBOL;
                }
            } else {
                // Probably an immediate operand.
                ShowNarNotAddress = true;
            }
        }

        private void UpdateCopyToOperand() {
            IsCopyToOperandEnabled = false;
            if (mEditedProjectSymbol != null) {
                // We have a pre-existing or recently-edited symbol.  See if the current
                // operand configuration already matches.
                if (!FormatSymbol || !mEditedProjectSymbol.Label.Equals(SymbolLabel)) {
                    IsCopyToOperandEnabled = true;
                }
            }
        }

        private void EditLabel_Click(object sender, RoutedEventArgs e) {
            EditLabel dlg = new EditLabel(this, mEditedLabel, mLabelTargetAddress,
                mProject.SymbolTable);
            if (dlg.ShowDialog() != true || mEditedLabel == dlg.LabelSym) {
                Debug.WriteLine("No change to label, ignoring edit");
                return;
            }

            mEditedLabel = dlg.LabelSym;
            mLabelHasBeenEdited = true;

            // Update UI to match current state.
            if (mEditedLabel == null) {
                ShowNarCurrentLabel = false;
                CreateEditLabelText = CREATE_LABEL;
            } else {
                ShowNarCurrentLabel = true;
                CreateEditLabelText = EDIT_LABEL;
                NarTargetLabel = mEditedLabel.Label;
            }

            // Sort of nice to just hit return twice after entering a label, so move the focus
            // to the OK button.
            okButton.Focus();
        }

        private void EditProjectSymbol_Click(object sender, RoutedEventArgs e) {
            DefSymbol origSym = mEditedProjectSymbol;
            if (origSym == null) {
                // Need to start with a symbol so we can set the value field.
                string symName = "SYM";
                if (!string.IsNullOrEmpty(SymbolLabel)) {
                    symName = SymbolLabel;  // may not be valid, but it doesn't have to be
                }
                origSym = new DefSymbol(symName, mOperandValue, Symbol.Source.Project,
                    Symbol.Type.ExternalAddr, FormatDescriptor.SubType.None);
            }

            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter,
                mProject.ProjectProps.ProjectSyms, origSym, null, false, true);
            if (dlg.ShowDialog() != true) {
                return;
            }
            Debug.Assert(dlg.NewSym != null);   // can't delete a symbol from dialog

            if (mEditedProjectSymbol == dlg.NewSym) {
                Debug.WriteLine("No change to project symbol, ignoring edit");
                return;
            }
            mEditedProjectSymbol = dlg.NewSym;
            ShowNarProjectSymbol = true;
            ShowNarNoProjectMatch = false;
            NarProjectSymbol = mEditedProjectSymbol.Label;
            CreateEditProjectSymbolText = EDIT_PROJECT_SYMBOL;

            // The preview and symbol value display will use mEditedProjectSymbol if it's the
            // only place the symbol exists, so we want to keep the other controls updated.
            UpdateControls();

            // Move the focus to the OK button.
            okButton.Focus();
        }

        private void CopyToOperandButton_Click(object sender, RoutedEventArgs e) {
            FormatSymbol = true;
            SymbolLabel = mEditedProjectSymbol.Label;
            IsCopyToOperandEnabled = false;
            // changes to controls will call UpdateControls() for us
        }

        #endregion Numeric References

        #region Local Variables

        public bool ShowLvNotApplicable {
            get { return mShowLvNotApplicable; }
            set { mShowLvNotApplicable = value; OnPropertyChanged(); }
        }
        private bool mShowLvNotApplicable;

        public bool ShowLvTableNotFound {
            get { return mShowLvTableNotFound; }
            set { mShowLvTableNotFound = value; OnPropertyChanged(); }
        }
        private bool mShowLvTableNotFound;

        public bool ShowLvNoMatchFound {
            get { return mShowLvNoMatchFound; }
            set { mShowLvNoMatchFound = value; OnPropertyChanged(); }
        }
        private bool mShowLvNoMatchFound;

        public bool ShowLvMatchFound {
            get { return mShowLvMatchFound; }
            set { mShowLvMatchFound = value; OnPropertyChanged(); }
        }
        private bool mShowLvMatchFound;

        public string LvMatchFoundText {
            get { return mLvMatchFoundText; }
            set { mLvMatchFoundText = value; OnPropertyChanged(); }
        }
        private string mLvMatchFoundText;

        public string LocalVariableLabel {
            get { return mLocalVariableLabel; }
            set { mLocalVariableLabel = value; OnPropertyChanged(); }
        }
        private string mLocalVariableLabel;

        public bool ShowLvCreateEditButton {
            get { return mShowLvCreateEditButton; }
            set { mShowLvCreateEditButton = value; OnPropertyChanged(); }
        }
        private bool mShowLvCreateEditButton;

        public string CreateEditLocalVariableText {
            get { return mCreateEditLocalVariableText; }
            set { mCreateEditLocalVariableText = value; OnPropertyChanged(); }
        }
        private string mCreateEditLocalVariableText;

        /// <summary>
        /// Offset of LocalVariableTable we're going to modify.
        /// </summary>
        private int mLvTableOffset = -1;

        /// <summary>
        /// Local variable value.  If there's already a definition, this will be pre-filled
        /// with the current contents.  Otherwise it will be null.
        /// </summary>
        private DefSymbol mEditedLocalVar;

        /// <summary>
        /// Clone of original table, with local edits.
        /// </summary>
        private LocalVariableTable mEditedLvTable;


        /// <summary>
        /// Configures the UI in the local variables box at load time.
        /// </summary>
        private void LocalVariables_Loaded() {
            if (!mOpDef.IsDirectPageInstruction && !mOpDef.IsStackRelInstruction) {
                ShowLvNotApplicable = true;
                return;
            }

            LvMatchFoundText = mOpDef.IsDirectPageInstruction ?
                LV_MATCH_FOUND_ADDRESS : LV_MATCH_FOUND_CONSTANT;

            LocalVariableLookup lvLookup =
                new LocalVariableLookup(mProject.LvTables, mProject, false);

            // If the operand is already a local variable, use whichever one the
            // analyzer found.
            Anattrib attr = mProject.GetAnattrib(mOffset);
            if (attr.DataDescriptor != null && attr.DataDescriptor.HasSymbol &&
                    attr.DataDescriptor.SymbolRef.IsVariable) {
                // Select the table that defines the local variable that's currently
                // associated with this operand.
                mLvTableOffset = lvLookup.GetDefiningTableOffset(mOffset,
                    attr.DataDescriptor.SymbolRef);
                Debug.Assert(mLvTableOffset >= 0);
                Debug.WriteLine("Symbol " + attr.DataDescriptor.SymbolRef +
                    " from var table at +" + mLvTableOffset.ToString("x6"));
            } else {
                // Operand is not a local variable.  Find the closest table.
                mLvTableOffset = lvLookup.GetNearestTableOffset(mOffset);
                Debug.WriteLine("Closest table is at +" + mLvTableOffset.ToString("x6"));
            }

            if (mLvTableOffset < 0) {
                ShowLvTableNotFound = true;
            } else {
                // Found a table.  Do we have a matching symbol?
                ShowLvCreateEditButton = true;
                mEditedLocalVar = lvLookup.GetSymbol(mOffset, mOperandValue,
                    mOpDef.IsDirectPageInstruction ?
                        Symbol.Type.ExternalAddr : Symbol.Type.Constant);
                if (mEditedLocalVar == null) {
                    ShowLvNoMatchFound = true;
                    CreateEditLocalVariableText = CREATE_LOCAL_VARIABLE;
                } else {
                    ShowLvMatchFound = true;
                    CreateEditLocalVariableText = EDIT_LOCAL_VARIABLE;
                    LocalVariableLabel = mEditedLocalVar.Label;
                }

                // We need to update the symbol table while we work to make the uniqueness
                // check come out right.  Otherwise if you edit, rename FOO to BAR,
                // then edit again, you won't be able to rename BAR back to FOO because
                // it's already in the list and it's not self.
                //
                // We don't need the full LVT, just the list of symbols, but we'll want
                // to hand the modified table to the caller when we exit.
                LocalVariableTable lvt = mProject.LvTables[mLvTableOffset];
                mEditedLvTable = new LocalVariableTable(lvt);
            }
        }

        private void EditLocalVariableButton_Click(object sender, RoutedEventArgs e) {
            Debug.Assert(mOpDef.IsDirectPageInstruction || mOpDef.IsStackRelInstruction);
            Debug.Assert(mLvTableOffset >= 0);

            DefSymbol initialVar = mEditedLocalVar;

            if (initialVar == null) {
                Symbol.Type symType;
                if (mOpDef.IsDirectPageInstruction) {
                    symType = Symbol.Type.ExternalAddr;
                } else {
                    symType = Symbol.Type.Constant;
                }

                // We need to pre-load the value and type, but we can't create a symbol with
                // an empty name.  We don't really need to create something unique since the
                // dialog will handle it.
                initialVar = new DefSymbol("VAR", mOperandValue,
                    Symbol.Source.Variable, symType, FormatDescriptor.SubType.None,
                    string.Empty, string.Empty, 1, true);
            }

            EditDefSymbol dlg = new EditDefSymbol(this, mFormatter,
                mEditedLvTable.GetSortedByLabel(), initialVar, mProject.SymbolTable,
                true, true);
            if (dlg.ShowDialog() == true) {
                if (mEditedLocalVar != dlg.NewSym) {
                    // Integrate result.  Future edits will start with this.
                    // We can't delete a symbol, just create or modify.
                    Debug.Assert(dlg.NewSym != null);
                    mEditedLocalVar = dlg.NewSym;
                    mEditedLvTable.AddOrReplace(dlg.NewSym);
                    LocalVariableLabel = mEditedLocalVar.Label;
                    CreateEditLocalVariableText = EDIT_LOCAL_VARIABLE;
                    ShowLvNoMatchFound = false;
                    ShowLvMatchFound = true;
                } else {
                    Debug.WriteLine("No change to def symbol, ignoring edit");
                }

                okButton.Focus();
            }
        }

        #endregion Local Variables
    }
}
