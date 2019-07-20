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
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using Asm65;

namespace SourceGenWF.AppForms {
    public partial class EditOperand : Form {
        /// <summary>
        /// In/out.  May be null on entry if the offset doesn't have a format descriptor
        /// specified.  Will be null on exit if "default" is selected.
        /// </summary>
        public FormatDescriptor FormatDescriptor { get; set; }

        public enum SymbolShortcutAction {
            None = 0, CreateLabelInstead, CreateLabelAlso, CreateProjectSymbolAlso
        }

        /// <summary>
        /// Remember the last option we used.
        /// </summary>
        private static SymbolShortcutAction sLastAction = SymbolShortcutAction.None;

        /// <summary>
        /// On OK dialog exit, specifies that an additional action should be taken.
        /// </summary>
        public SymbolShortcutAction ShortcutAction { get; private set; }

        /// <summary>
        /// Additional argument, meaning dependent on ShortcutAction.  This will either be
        /// the target label offset or the project symbol value.
        /// </summary>
        public int ShortcutArg { get; private set; }

        /// <summary>
        /// Width of full instruction, including opcode.
        /// </summary>
        private int mInstructionLength;

        /// <summary>
        /// Number of hexadecimal digits to show in the preview.  Sometimes you want
        /// to force this to be longer or shorter than InstructionLength would indicate,
        /// e.g. "BRA $1000" has a 1-byte operand.
        /// </summary>
        private int mPreviewHexDigits;

        /// <summary>
        /// Operand value, extracted from file data.  For a relative branch, this will be
        /// an address instead.  Only used for preview window.
        /// </summary>
        private int mOperandValue;

        /// <summary>
        /// Is the operand an immediate value?  If so, we enable the symbol part selection.
        /// </summary>
        private bool mIsExtendedImmediate;

        /// <summary>
        /// Is the operand a PC relative offset?
        /// </summary>
        private bool mIsPcRelative;

        /// <summary>
        /// Special handling for block move instructions (MVN/MVP).
        /// </summary>
        private bool mIsBlockMove;

        /// <summary>
        /// If set, show a '#' in the preview indow.
        /// </summary>
        private bool mShowHashPrefix;

        ///// <summary>
        ///// Symbol table to use when resolving symbolic values.
        ///// </summary>
        //private SymbolTable SymbolTable { get; set; }

        /// <summary>
        /// Project reference.
        /// </summary>
        private DisasmProject mProject;

        /// <summary>
        /// Formatter to use when displaying addresses and hex values.
        /// </summary>
        private Formatter mFormatter;

        /// <summary>
        /// Copy of operand Anattribs.
        /// </summary>
        private Anattrib mAttr;

        /// <summary>
        /// Set this during initial control configuration, so we know to ignore the CheckedChanged
        /// events.
        /// </summary>
        private bool mIsInitialSetup;

        /// <summary>
        /// Set to true if the user has entered a symbol that matches an auto-generated symbol.
        /// </summary>
        private bool mIsSymbolAuto;


        public EditOperand(int offset, DisasmProject project, Asm65.Formatter formatter) {
            InitializeComponent();

            mProject = project;
            mFormatter = formatter;

            // Configure the appearance.
            mAttr = mProject.GetAnattrib(offset);
            OpDef op = mProject.CpuDef.GetOpDef(mProject.FileData[offset]);
            mInstructionLength = mAttr.Length;
            mPreviewHexDigits = (mAttr.Length - 1) * 2;
            if (mAttr.OperandAddress >= 0) {
                // Use this as the operand value when available.  This lets us present
                // relative branch instructions in the expected form.
                mOperandValue = mAttr.OperandAddress;

                if (op.AddrMode == OpDef.AddressMode.PCRel) {
                    mPreviewHexDigits = 4;
                    mIsPcRelative = true;
                } else if (op.AddrMode == OpDef.AddressMode.PCRelLong ||
                        op.AddrMode == OpDef.AddressMode.StackPCRelLong) {
                    mIsPcRelative = true;
                }
            } else {
                int opVal = op.GetOperand(mProject.FileData, offset, mAttr.StatusFlags);
                mOperandValue = opVal;
                if (op.AddrMode == OpDef.AddressMode.BlockMove) {
                    // MVN and MVP screw things up by having two operands in one instruction.
                    // We deal with this by passing in the value from the second byte
                    // (source bank) as the value, and applying the chosen format to both bytes.
                    mIsBlockMove = true;
                    mOperandValue = opVal >> 8;
                    mPreviewHexDigits = 2;
                }
            }
            mIsExtendedImmediate = op.IsExtendedImmediate;   // Imm, PEA, MVN/MVP
            mShowHashPrefix = op.IsImmediate;                // just Imm
        }

        private void EditOperand_Load(object sender, EventArgs e) {
            mIsInitialSetup = true;

            // Can this be represented as high or low ASCII?
            radioButtonAscii.Enabled = CommonUtil.TextUtil.IsHiLoAscii(mOperandValue);

            // Configure the dialog from the FormatDescriptor, if one is available.
            SetControlsFromDescriptor(FormatDescriptor);

            // Do this whether or not symbol is checked -- want to have this set when the
            // dialog is initially in default format.
            switch (sLastAction) {
                case SymbolShortcutAction.CreateLabelInstead:
                    labelInsteadRadioButton.Checked = true;
                    break;
                case SymbolShortcutAction.CreateLabelAlso:
                    operandAndLabelRadioButton.Checked = true;
                    break;
                case SymbolShortcutAction.CreateProjectSymbolAlso:
                    operandAndProjRadioButton.Checked = true;
                    break;
                default:
                    operandOnlyRadioButton.Checked = true;
                    break;
            }

            mIsInitialSetup = false;
            UpdateControls();
        }

        private void EditOperand_Shown(object sender, EventArgs e) {
            // Start with the focus in the text box.  This way they can start typing
            // immediately.
            symbolTextBox.Focus();
        }

        private void symbolTextBox_TextChanged(object sender, EventArgs e) {
            // Make sure Symbol is checked if they're typing text in.
            radioButtonSymbol.Checked = true;
            UpdateControls();
        }

        /// <summary>
        /// Handles CheckedChanged event for all radio buttons in main group.
        /// </summary>
        private void MainGroup_CheckedChanged(object sender, EventArgs e) {
            // Enable/disable the low/high/bank radio group.
            // Update preview window.
            UpdateControls();
        }

        /// <summary>
        /// Handles CheckedChanged event for all radio buttons in symbol-part group.
        /// </summary>
        private void PartGroup_CheckedChanged(object sender, EventArgs e) {
            // Update preview window.
            UpdateControls();
        }

        private void okButton_Click(object sender, EventArgs e) {
            FormatDescriptor = CreateDescriptorFromControls();

            //
            // Extract the current shortcut action.  For dialog configuration purposes we
            // want to capture the current state.  For the caller, we force it to "none"
            // if we're not using a symbol format.
            //
            SymbolShortcutAction action = SymbolShortcutAction.None;
            if (labelInsteadRadioButton.Checked) {
                action = SymbolShortcutAction.CreateLabelInstead;
            } else if (operandAndLabelRadioButton.Checked) {
                action = SymbolShortcutAction.CreateLabelAlso;
            } else if (operandAndProjRadioButton.Checked) {
                action = SymbolShortcutAction.CreateProjectSymbolAlso;
            } else if (operandOnlyRadioButton.Checked) {
                action = SymbolShortcutAction.None;
            } else {
                Debug.Assert(false);
                action = SymbolShortcutAction.None;
            }
            sLastAction = action;

            if (radioButtonSymbol.Checked && FormatDescriptor != null) {
                // Only report a shortcut action if they've entered a symbol.  If they
                // checked symbol but left the field blank, they're just trying to delete
                // the format.
                ShortcutAction = action;
            } else {
                ShortcutAction = SymbolShortcutAction.None;
            }
        }

        /// <summary>
        /// Updates all of the controls to reflect the current internal state.
        /// </summary>
        private void UpdateControls() {
            if (mIsInitialSetup) {
                return;
            }
            symbolPartPanel.Enabled = radioButtonSymbol.Checked && mIsExtendedImmediate;
            symbolShortcutsGroupBox.Enabled = radioButtonSymbol.Checked;

            SetPreviewText();

            bool isOk = true;
            if (radioButtonSymbol.Checked) {
                // Just check for correct format.  References to non-existent labels are allowed.
                //
                // We try to block references to auto labels, but it's possible to get around it
                // (replace auto label with user label, reference non-existent auto label,
                // remove user label).  We could try harder, but currently not necessary.
                isOk = !mIsSymbolAuto && Asm65.Label.ValidateLabel(symbolTextBox.Text);

                // Allow empty strings as a way to delete the label and return to "default".
                if (string.IsNullOrEmpty(symbolTextBox.Text)) {
                    isOk = true;
                }

                ConfigureSymbolShortcuts();
            }
            okButton.Enabled = isOk;
        }

        /// <summary>
        /// Sets the text displayed in the "preview" text box.
        /// </summary>
        private void SetPreviewText() {
            //symbolValueLabel.Text = string.Empty;
            mIsSymbolAuto = false;

            FormatDescriptor dfd = CreateDescriptorFromControls();
            if (dfd == null) {
                // Default format.  We can't actually know what this look like, so just
                // clear the box.
                previewTextBox.Text = string.Empty;
                return;
            }

            if (dfd.FormatSubType == FormatDescriptor.SubType.Symbol &&
                    string.IsNullOrEmpty(dfd.SymbolRef.Label)) {
                // no label yet, nothing to show
                previewTextBox.Text = string.Empty;
                return;
            }

            StringBuilder preview = new StringBuilder();
            if (mShowHashPrefix) {
                preview.Append('#');
            }

            switch (dfd.FormatSubType) {
                case FormatDescriptor.SubType.Hex:
                    preview.Append(mFormatter.FormatHexValue(mOperandValue, mPreviewHexDigits));
                    break;
                case FormatDescriptor.SubType.Decimal:
                    preview.Append(mFormatter.FormatDecimalValue(mOperandValue));
                    break;
                case FormatDescriptor.SubType.Binary:
                    preview.Append(mFormatter.FormatBinaryValue(mOperandValue, 8));
                    break;
                case FormatDescriptor.SubType.Ascii:
                    preview.Append(mFormatter.FormatAsciiOrHex(mOperandValue));
                    break;
                case FormatDescriptor.SubType.Symbol:
                    if (mProject.SymbolTable.TryGetValue(dfd.SymbolRef.Label, out Symbol sym)) {
                        if (mIsBlockMove) {
                            // For a 24-bit symbol, we grab the high byte.  This is the
                            // expected behavior, according to Eyes & Lichty; see the
                            // explanation of the MVP instruction.  For an 8-bit symbol
                            // the assembler just takes the value.
                            // TODO(someday): allow a different symbol for each part of the
                            // operand.
                            if (sym.Value > 0xff) {
                                radioButtonBank.Checked = true;
                            } else {
                                radioButtonLow.Checked = true;
                            }
                            dfd = CreateDescriptorFromControls();
                        }

                        // Hack to make relative branches look right in the preview window.
                        // Otherwise they show up like "<LABEL" because they appear to be
                        // only 8 bits.
                        int operandLen = dfd.Length - 1;
                        if (operandLen == 1 && mIsPcRelative) {
                            operandLen = 2;
                        }
                        PseudoOp.FormatNumericOpFlags flags;
                        if (mIsPcRelative) {
                            flags = PseudoOp.FormatNumericOpFlags.IsPcRel;
                        } else if (mShowHashPrefix) {
                            flags = PseudoOp.FormatNumericOpFlags.HasHashPrefix;
                        } else {
                            flags = PseudoOp.FormatNumericOpFlags.None;
                        }
                        string str = PseudoOp.FormatNumericOperand(mFormatter,
                            mProject.SymbolTable, null, dfd,
                            mOperandValue, operandLen, flags);
                        preview.Append(str);

                        if (sym.SymbolSource == Symbol.Source.Auto) {
                            mIsSymbolAuto = true;
                        }
                    } else {
                        preview.Append(dfd.SymbolRef.Label + " (?)");
                        Debug.Assert(!string.IsNullOrEmpty(dfd.SymbolRef.Label));
                        //symbolValueLabel.Text = Properties.Resources.MSG_SYMBOL_NOT_FOUND;
                    }
                    break;
                default:
                    Debug.Assert(false);
                    preview.Append("BUG");
                    break;
            }
            previewTextBox.Text = preview.ToString();
        }

        /// <summary>
        /// Configures the buttons in the "symbol shortcuts" group box.  The entire box is
        /// disabled unless "symbol" is selected.  Other options are selectively enabled or
        /// disabled as appropriate for the current input.  If we disable the selection option,
        /// the selection will be reset to default.
        /// </summary>
        private void ConfigureSymbolShortcuts() {
            // operandOnlyRadioButton: always enabled
            // labelInsteadRadioButton: symbol is unknown and operand address has no label
            // operandAndLabelRadioButton: same as labelInstead
            // operandAndProjRadioButton: symbol is unknown and operand address is outside project

            string labelStr = symbolTextBox.Text;
            ShortcutArg = -1;

            // Is this a known symbol?  If so, disable most options and bail.
            if (mProject.SymbolTable.TryGetValue(labelStr, out Symbol sym)) {
                labelInsteadRadioButton.Enabled = operandAndLabelRadioButton.Enabled =
                    operandAndProjRadioButton.Enabled = false;
                operandOnlyRadioButton.Checked = true;
                return;
            }

            if (mAttr.OperandOffset >= 0) {
                // Operand target is inside the file.  Does the target offset already have a label?
                int targetOffset =
                    DataAnalysis.GetBaseOperandOffset(mProject, mAttr.OperandOffset);
                bool hasLabel = mProject.UserLabels.ContainsKey(targetOffset);
                labelInsteadRadioButton.Enabled = operandAndLabelRadioButton.Enabled =
                    !hasLabel;
                operandAndProjRadioButton.Enabled = false;
                ShortcutArg = targetOffset;
            } else if (mAttr.OperandAddress >= 0) {
                // Operand target is outside the file.
                labelInsteadRadioButton.Enabled = operandAndLabelRadioButton.Enabled = false;
                operandAndProjRadioButton.Enabled = true;
                ShortcutArg = mAttr.OperandAddress;
            } else {
                // Probably an immediate operand.
                labelInsteadRadioButton.Enabled = operandAndLabelRadioButton.Enabled =
                    operandAndProjRadioButton.Enabled = false;
            }

            // Select the default option if the currently-selected option is no longer available.
            if ((labelInsteadRadioButton.Checked && !labelInsteadRadioButton.Enabled) ||
                    (operandAndLabelRadioButton.Checked && !operandAndLabelRadioButton.Enabled) ||
                    (operandAndProjRadioButton.Checked && !operandAndProjRadioButton.Enabled)) {
                operandOnlyRadioButton.Checked = true;
            }
        }

        /// <summary>
        /// Configures the dialog controls based on the provided format descriptor.
        /// </summary>
        /// <param name="dfd">FormatDescriptor to use.</param>
        private void SetControlsFromDescriptor(FormatDescriptor dfd) {
            Debug.Assert(mIsInitialSetup);
            radioButtonLow.Checked = true;

            if (dfd == null) {
                radioButtonDefault.Checked = true;
                return;
            }

            // NOTE: it's entirely possible to have a weird format (e.g. string) if the
            // instruction used to be hinted as data.  Handle it gracefully.
            switch (dfd.FormatType) {
                case FormatDescriptor.Type.NumericLE:
                    switch (dfd.FormatSubType) {
                        case FormatDescriptor.SubType.Hex:
                            radioButtonHex.Checked = true;
                            break;
                        case FormatDescriptor.SubType.Decimal:
                            radioButtonDecimal.Checked = true;
                            break;
                        case FormatDescriptor.SubType.Binary:
                            radioButtonBinary.Checked = true;
                            break;
                        case FormatDescriptor.SubType.Ascii:
                            radioButtonAscii.Checked = true;
                            break;
                        case FormatDescriptor.SubType.Symbol:
                            Debug.Assert(dfd.HasSymbol);
                            radioButtonSymbol.Checked = true;
                            switch (dfd.SymbolRef.ValuePart) {
                                case WeakSymbolRef.Part.Low:
                                    radioButtonLow.Checked = true;
                                    break;
                                case WeakSymbolRef.Part.High:
                                    radioButtonHigh.Checked = true;
                                    break;
                                case WeakSymbolRef.Part.Bank:
                                    radioButtonBank.Checked = true;
                                    break;
                                default:
                                    Debug.Assert(false);
                                    break;
                            }
                            symbolTextBox.Text = dfd.SymbolRef.Label;
                            break;
                        case FormatDescriptor.SubType.None:
                        default:
                            // Unexpected; call it hex.
                            radioButtonHex.Checked = true;
                            break;
                    }
                    break;
                case FormatDescriptor.Type.NumericBE:
                case FormatDescriptor.Type.String:
                case FormatDescriptor.Type.Fill:
                default:
                    // Unexpected; used to be data?
                    radioButtonDefault.Checked = true;
                    break;
            }
        }

        /// <summary>
        /// Creates a FormatDescriptor from the current state of the dialog controls.
        /// </summary>
        /// <returns>New FormatDescriptor.</returns>
        private FormatDescriptor CreateDescriptorFromControls() {
            if (radioButtonSymbol.Checked) {
                if (string.IsNullOrEmpty(symbolTextBox.Text)) {
                    // empty symbol --> default format (intuitive way to delete label reference)
                    return null;
                }
                WeakSymbolRef.Part part;
                if (radioButtonLow.Checked) {
                    part = WeakSymbolRef.Part.Low;
                } else if (radioButtonHigh.Checked) {
                    part = WeakSymbolRef.Part.High;
                } else if (radioButtonBank.Checked) {
                    part = WeakSymbolRef.Part.Bank;
                } else {
                    Debug.Assert(false);
                    part = WeakSymbolRef.Part.Low;
                }
                return FormatDescriptor.Create(mInstructionLength,
                    new WeakSymbolRef(symbolTextBox.Text, part), false);
            }

            FormatDescriptor.SubType subType;
            if (radioButtonDefault.Checked) {
                return null;
            } else if (radioButtonHex.Checked) {
                subType = FormatDescriptor.SubType.Hex;
            } else if (radioButtonDecimal.Checked) {
                subType = FormatDescriptor.SubType.Decimal;
            } else if (radioButtonBinary.Checked) {
                subType = FormatDescriptor.SubType.Binary;
            } else if (radioButtonAscii.Checked) {
                subType = FormatDescriptor.SubType.Ascii;
            } else if (radioButtonSymbol.Checked) {
                subType = FormatDescriptor.SubType.Symbol;
            } else {
                Debug.Assert(false);
                subType = FormatDescriptor.SubType.None;
            }

            return FormatDescriptor.Create(mInstructionLength,
                FormatDescriptor.Type.NumericLE, subType);
        }
    }
}
