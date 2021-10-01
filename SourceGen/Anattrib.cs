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
using System.Diagnostics;
using System.Text;

using Asm65;

namespace SourceGen {
    /// <summary>
    /// Analyzer attribute holder.  Contains the output of the instruction and data analyzers.
    /// Every byte in the input file has one of these associated with it.
    /// </summary>
    /// <remarks>
    /// (Yes, it's a mutable struct.  Yes, that fact has bitten me a few times.  The array
    /// of these may have millions of elements, so the reduction in overhead seems worthwhile.)
    /// </remarks>
    public struct Anattrib {
        [FlagsAttribute]
        private enum AttribFlags {
            InstrStart = 1 << 0,        // byte is first of an instruction
            Instruction = 1 << 1,       // byte is part of an instruction or inline data
            InlineData = 1 << 2,        // byte is inline data
            Data = 1 << 3,              // byte is data

            UsesDataBankReg = 1 << 4,   // operand value should be merged with DBR

            EntryPoint = 1 << 8,        // external code branches here
            BranchTarget = 1 << 9,      // internal code branches here
            ExternalBranch = 1 << 10,   // this abs/rel branch lands outside input file

            NoContinue = 1 << 12,       // execution does not continue to following instruction
            NoContinueScript = 1 << 13, // no-continue flag set by extension script

            Visited = 1 << 16,          // has the analyzer visited this byte?
            Changed = 1 << 17,          // set/cleared as the analyzer works

            ATagged = 1 << 18,          // was this byte affected by an analyzer tag?
            AddrRegionChange = 1 << 19, // is this byte in a different address region from prev?
            NonAddressable = 1 << 20,   // is this byte in a non-addressable range?
        }

        // Flags indicating what type of data is here.  Use the following Is* properties
        // to set/clear.
        private AttribFlags mAttribFlags;

        public bool IsInstructionStart {
            get {
                return (mAttribFlags & AttribFlags.InstrStart) != 0;
            }
            set {
                IsInstruction = value;
                if (value) {
                    mAttribFlags |= AttribFlags.InstrStart;
                } else {
                    mAttribFlags &= ~AttribFlags.InstrStart;
                }
            }
        }
        public bool IsInstruction {
            get {
                return (mAttribFlags & AttribFlags.Instruction) != 0;
            }
            set {
                Debug.Assert(value == false ||
                    (mAttribFlags & (AttribFlags.InlineData | AttribFlags.Data)) == 0);
                if (value) {
                    mAttribFlags |= AttribFlags.Instruction;
                } else {
                    mAttribFlags &= ~AttribFlags.Instruction;
                }
            }
        }
        public bool IsInlineData {
            get {
                return (mAttribFlags & AttribFlags.InlineData) != 0;
            }
            set {
                Debug.Assert(value == false ||
                    (mAttribFlags & (AttribFlags.Instruction | AttribFlags.Data)) == 0);
                if (value) {
                    mAttribFlags |= AttribFlags.InlineData;
                } else {
                    mAttribFlags &= ~AttribFlags.InlineData;
                }
            }
        }
        public bool IsData {
            get {
                return (mAttribFlags & AttribFlags.Data) != 0;
            }
            set {
                Debug.Assert(value == false ||
                    (mAttribFlags & (AttribFlags.InlineData | AttribFlags.Instruction)) == 0);
                if (value) {
                    mAttribFlags |= AttribFlags.Data;
                } else {
                    mAttribFlags &= ~AttribFlags.Data;
                }
            }
        }
        public bool IsStart {
            get {
                return IsInstructionStart || IsDataStart || IsInlineDataStart;
            }
        }
        public bool UsesDataBankReg {
            get {
                return (mAttribFlags & AttribFlags.UsesDataBankReg) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.UsesDataBankReg;
                } else {
                    mAttribFlags &= ~AttribFlags.UsesDataBankReg;
                }
            }
        }
        public bool IsEntryPoint {
            get {
                return (mAttribFlags & AttribFlags.EntryPoint) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.EntryPoint;
                } else {
                    mAttribFlags &= ~AttribFlags.EntryPoint;
                }
            }
        }
        public bool IsBranchTarget {
            get {
                return (mAttribFlags & AttribFlags.BranchTarget) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.BranchTarget;
                } else {
                    mAttribFlags &= ~AttribFlags.BranchTarget;
                }
            }
        }
        public bool IsExternalBranch {
            get {
                return (mAttribFlags & AttribFlags.ExternalBranch) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.ExternalBranch;
                } else {
                    mAttribFlags &= ~AttribFlags.ExternalBranch;
                }
            }
        }
        public bool NoContinue {
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.NoContinue;
                } else {
                    mAttribFlags &= ~AttribFlags.NoContinue;
                }
            }
        }
        public bool NoContinueScript {
            get {
                return (mAttribFlags & AttribFlags.NoContinueScript) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.NoContinueScript;
                } else {
                    mAttribFlags &= ~AttribFlags.NoContinueScript;
                }
            }
        }
        public bool DoesNotContinue {
            get {
                return (mAttribFlags & AttribFlags.NoContinue) != 0 ||
                       (mAttribFlags & AttribFlags.NoContinueScript) != 0;
            }
        }
        public bool DoesNotBranch {
            get {
                return (BranchTaken == OpDef.BranchTaken.Never);
            }
        }
        public bool IsVisited {
            get {
                return (mAttribFlags & AttribFlags.Visited) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.Visited;
                } else {
                    mAttribFlags &= ~AttribFlags.Visited;
                }
            }
        }
        public bool IsChanged {
            get {
                return (mAttribFlags & AttribFlags.Changed) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.Changed;
                } else {
                    mAttribFlags &= ~AttribFlags.Changed;
                }
            }
        }
        public bool HasAnalyzerTag {
            get {
                return (mAttribFlags & AttribFlags.ATagged) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.ATagged;
                } else {
                    mAttribFlags &= ~AttribFlags.ATagged;
                }
            }
        }
        public bool IsAddrRegionChange {
            get {
                return (mAttribFlags & AttribFlags.AddrRegionChange) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.AddrRegionChange;
                } else {
                    mAttribFlags &= ~AttribFlags.AddrRegionChange;
                }
            }
        }
        public bool IsNonAddressable {
            get {
                return (mAttribFlags & AttribFlags.NonAddressable) != 0;
            }
            set {
                if (value) {
                    mAttribFlags |= AttribFlags.NonAddressable;
                } else {
                    mAttribFlags &= ~AttribFlags.NonAddressable;
                }
            }
        }

        public bool IsDataStart {
            get {
                return IsData && DataDescriptor != null;
            }
        }
        public bool IsInlineDataStart {
            get {
                return IsInlineData && DataDescriptor != null;
            }
        }
        public bool IsUntyped {
            get {
                return !IsInstruction && !IsData && !IsInlineData;
            }
        }

        /// <summary>
        /// Get the target memory address for this byte.
        /// </summary>
        public int Address { get; set; }

        /// <summary>
        /// Instructions: length of the instruction (for InstrStart).  If a FormatDescriptor is
        ///   assigned, the length must match, or the dfd will be ignored.
        /// Inline data: FormatDescriptor length, or zero if no descriptor is defined.
        /// Data: FormatDescriptor length, or zero if no descriptor is defined.
        /// 
        /// This field should only be set by CodeAnalysis methods, although the "get" value
        /// can be changed for data/inline-data by setting the DataDescriptor field.
        /// </summary>
        public int Length {
            get {
                // For data we don't even use the field; this ensures that we're always
                // using the FormatDescriptor's length.
                if (IsData || IsInlineData) {
                    Debug.Assert(mLength == 0);
                    if (DataDescriptor != null) {
                        return DataDescriptor.Length;
                    } else {
                        return 0;
                    }
                }
                return mLength;
            }
            set {
                Debug.Assert(!IsData);
                mLength = value;
            }
        }
        private int mLength;

        /// <summary>
        /// Instructions only: processor status flags.
        /// 
        /// Note this returns a copy of a struct, so modifications to the returned value
        /// (including calls to Merge and Apply) are not permanent.
        /// </summary>
        public StatusFlags StatusFlags {
            get { return mStatusFlags; }
            set { mStatusFlags = value; }
        }
        private StatusFlags mStatusFlags;

        public void MergeStatusFlags(StatusFlags other) {
            mStatusFlags.Merge(other);
        }
        public void ApplyStatusFlags(StatusFlags other) {
            mStatusFlags.Apply(other);
        }

        /// <summary>
        /// Branch instructions only: outcome of branch.
        /// </summary>
        public OpDef.BranchTaken BranchTaken { get; set; }

        /// <summary>
        /// Instructions only: decoded operand address value.  Will be -1 if not
        /// yet computed or not applicable.  For a relative branch instruction,
        /// this will have the absolute branch target address.  On the 65816, this
        /// will be a 24-bit address.
        /// </summary>
        public int OperandAddress {
            get { return mOperandAddressSet ? mOperandAddress : -1; }
            set {
                Debug.Assert(mOperandAddress >= -1);
                mOperandAddress = value;
                mOperandAddressSet = (value >= 0);
            }
        }
        private int mOperandAddress;
        private bool mOperandAddressSet;

        /// <summary>
        /// Instructions only: offset referenced by OperandAddress.  Will be -1 if not
        /// yet computed, not applicable, or if OperandAddress refers to a location
        /// outside the scope of the file.
        /// </summary>
        public int OperandOffset {
            get { return mOperandOffsetSet ? mOperandOffset : -1; }
            set {
                Debug.Assert(mOperandOffset >= -1);
                mOperandOffset = value;
                mOperandOffsetSet = (value >= 0);
            }
        }
        private int mOperandOffset;
        private bool mOperandOffsetSet;

        /// <summary>
        /// Instructions only: is OperandOffset a direct target offset?  (This is used when
        /// tracing jump instructions, to know if we should add the offset to the scan list.
        /// It's determined by the opcode, e.g. "JMP addr" -> true, "JMP (addr,X)" -> false.)
        /// </summary>
        public bool IsOperandOffsetDirect { get; set; }

        /// <summary>
        /// Symbol defined as the label for this offset.  All offsets that are instruction
        /// or data target offsets will have one of these defined.  Users can define additional
        /// symbols as well.
        /// 
        /// Will be null if no label is defined for this offset.
        /// </summary>
        public Symbol Symbol { get; set; }

        /// <summary>
        /// Format descriptor for operands and data items.  Will be null if no descriptor
        /// is defined for this offset.
        /// </summary>
        public FormatDescriptor DataDescriptor { get; set; }

        /// <summary>
        /// Is this an instruction with an operand (i.e. not impl/acc)?
        /// </summary>
        public bool IsInstructionWithOperand {
            get {
                if (!IsInstructionStart) {
                    return false;
                }
                return Length != 1;
            }
        }

        /// <summary>
        /// Returns a fixed-width string with indicators for items of interest.
        /// </summary>
        public string ToAttrString() {
            StringBuilder sb = new StringBuilder(5);
            char blank = '.';
            sb.Append(IsEntryPoint ? '@' : blank);
            sb.Append(HasAnalyzerTag ? 'T' : blank);
            sb.Append(DoesNotBranch ? '!' : blank);
            sb.Append(DoesNotContinue ? '#' : blank);
            sb.Append(IsBranchTarget ? '>' : blank);
            return sb.ToString();
        }

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            if (IsInstruction) {
                sb.Append("Inst");
            } else if (IsData) {
                sb.Append("Data");
            } else if (IsInlineData) {
                sb.Append("Inli");
            }
            if (IsStart) {
                sb.Append("Start");
            }
            sb.Append(" len=");
            sb.Append(Length);
            return sb.ToString();
        }
    }
}
