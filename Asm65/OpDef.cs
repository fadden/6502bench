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

namespace Asm65 {
    /// <summary>
    /// Operation code definitions for all 65xx-series CPUs.
    /// 
    /// Instances are immutable.
    /// </summary>
    public class OpDef {
        /// <summary>
        /// Effect an instruction has on code flow.
        /// </summary>
        public enum FlowEffect {
            Unknown = 0,
            Branch,             // JMP, BRA, BRL, ... (jump to new address specified in operand)
            Cont,               // LDA, STA, PHP, NOP, ... (always continue to next instruction)
            NoCont,             // RTS, BRK, ... (jump to new address, not specified in operand)
            CallSubroutine,     // JSR, JSL (jump to new address, and also continue to next)
            ConditionalBranch   // BCC, BEQ, ... (jump to new address and/or continue to next)
        }

        /// <summary>
        /// Effect of executing an instruction on memory.
        /// </summary>
        public enum MemoryEffect {
            Unknown = 0,
            None,               // e.g. TAX, PEA addr, LDA #imm
            Read,               // e.g. LDA addr
            Write,              // e.g. STA addr
            ReadModifyWrite     // e.g. LSR addr
        }

        /// <summary>
        /// Addressing mode.  This uses the same distinctions as Eyes & Lichty, which for
        /// most purposes has some redundancy (e.g. StackRTI is only ever used with RTI, so
        /// in most cases it could be considered Implied).
        /// 
        /// The mode, combined with the processor flags on the 65816, determines the number
        /// of bytes required by the instruction.
        /// </summary>
        public enum AddressMode : byte {
            Unknown = 0,
            Abs,                // OP addr          3
            AbsInd,             // OP (addr)        3 (JMP)
            AbsIndLong,         // OP [addr]        3 (JML)
            AbsIndexX,          // OP addr,X        3
            AbsIndexXInd,       // OP (addr,X)      3 (JMP/JSR)
            AbsIndexXLong,      // OP long,X        4
            AbsIndexY,          // OP addr,Y        3
            AbsLong,            // OP long          4
            Acc,                // OP A             1
            BlockMove,          // OP srcb,dstb     3 (MVN/MVP)
            DP,                 // OP dp            2
            DPInd,              // OP (dp)          2
            DPIndIndexY,        // OP (dp),Y        2
            DPIndIndexYLong,    // OP [dp],Y        2
            DPIndLong,          // OP [dp]          2
            DPIndexX,           // OP dp,X          2
            DPIndexXInd,        // OP (dp,X)        2
            DPIndexY,           // OP dp,Y          2
            Imm,                // OP #const8       2
            ImmLongA,           // OP #const8/16    2 or 3, depending on 'm' flag
            ImmLongXY,          // OP #const8/16    2 or 3, depending on 'x' flag
            Implied,            // OP               1
            PCRel,              // OP label         2 (branch instructions)
            PCRelLong,          // OP label         3 (BRL)
            StackAbs,           // OP addr          3 (PEA)
            StackDPInd,         // OP (dp)          2 (PEI)
            StackInt,           // OP               2 (BRK, COP)
            StackPCRelLong,     // OP label         3 (PER)
            StackPull,          // OP               1 (PLA, PLX, ...)
            StackPush,          // OP               1 (PHA, PHX, ...)
            StackRTI,           // OP               1 (RTI)
            StackRTL,           // OP               1 (RTL)
            StackRTS,           // OP               1 (RTS)
            StackRel,           // OP sr,S          2
            StackRelIndIndexY,  // OP (sr,S),Y      2
            WDM                 // OP               2?
        }

        /// <summary>
        /// Cycle count modifiers.  This is meant to be bitwise-ORed with the base value.
        /// 
        /// This defines things generally, with some modifiers noted as being specific to
        /// certain CPUs.  The actual instruction definitions may choose to include or
        /// exclude bits according to the CPU being defined.
        /// </summary>
        [FlagsAttribute]
        public enum CycleMod {
            // Data from chapter 19 of Eyes & Lichty.
            OneIfM0                 = 1 << 8,   // +1 if M=0 (16-bit mem/accumulator)
            TwoIfM0                 = 1 << 9,   // +2 if M=0 (16-bit mem/accumulator)
            OneIfX0                 = 1 << 10,  // +1 if X=0 (16-bit index regs)
            OneIfDpNonzero          = 1 << 11,  // +1 if DP reg != 0
            OneIfIndexPage          = 1 << 12,  // +1 if indexing crosses page boundary
            OneIfD1                 = 1 << 13,  // +1 if D=1 (decimal mode) ONLY on 65C02
            OneIfBranchTaken        = 1 << 14,  // +1 if conditional branch taken
            OneIfBranchPage         = 1 << 15,  // +1 if branch crosses page UNLESS '816 native
            OneIfE0                 = 1 << 16,  // +1 if 65816/865816 native mode (E=0)
            OneIf65C02              = 1 << 17,  // +1 if 65C02
            MinusOneIfNoPage        = 1 << 18,  // -1 if 65C02 and no page boundary crossed
            BlockMove               = 1 << 19,  // +7 per byte moved
        }

        /// <summary>
        /// Width disambiguation requirement.
        /// </summary>
        public enum WidthDisambiguation : byte {
            None = 0,
            ForceDirect,    // only needed for forward DP label refs in single-pass assemblers
            ForceAbs,
            ForceLong,
            ForceLongMaybe  // add opcode suffix but not operand prefix

            // May want an additional item: "force long if operand suffix specified".  This
            // would let us generate LDAL for assemblers that like to have that made explicit,
            // while avoiding prepending it to operands that are unambiguously long (e.g.
            // $ff1122).  The counter-argument is that the operand prefix is still useful
            // for humans when looking at labels, e.g. "a:FOO" vs. "f:FOO", because the value
            // of the label may not be apparent.
        }


        /// <summary>
        /// Opcode numeric value, e.g. BRK is $00.
        /// </summary>
        public byte Opcode { get; private set; }

        /// <summary>
        /// Addressing mode.  Determines length of instruction (mostly) and decoding of operand.
        /// </summary>
        public AddressMode AddrMode { get; private set; }

        /// <summary>
        /// True if this is an undocumented opcode.
        /// </summary>
        public bool IsUndocumented { get; private set; }

        /// <summary>
        /// Instruction mnemonic, e.g. LDA for Load Accumulator.
        /// </summary>
        public string Mnemonic { get; private set; }

        /// <summary>
        /// Indication of which flags are affected by an instruction.  Unaffected flags
        /// will have the value TriState16.UNSPECIFIED.
        /// 
        /// This is not equivalent to the code flow status flag updater -- this just notes which
        /// flags are modified directly by the instruction.
        /// </summary>
        public StatusFlags FlagsAffected { get; private set; }

        /// <summary>
        /// Effect this instruction has on code flow.
        /// </summary>
        public FlowEffect Effect { get; private set; }

        /// <summary>
        /// Effect this instruction has on memory.
        /// </summary>
        /// <remarks>
        /// We don't consider execution to have a memory effect, so "LDA $1000" is Read but
        /// "JMP $1000" is None.  That's because the instruction itself doesn't access the
        /// memory at $1000, it just changes the program counter to point there.
        /// </remarks>
        public MemoryEffect MemEffect {
            get {
                // If we do this a lot, we should probably just go through and set the
                // mem effect to "none" in all the immediate-mode op definitions.
                if (IsImmediate) {
                    return MemoryEffect.None;
                } else {
                    return BaseMemEffect;
                }
            }
        }
        private MemoryEffect BaseMemEffect { get; set; }

        /// <summary>
        /// Cycles required.  The low 8 bits hold the base cycle count, the remaining bits
        /// are defined by the CycleMod enum.
        /// </summary>
        private int CycDef { get; set; }
        public int Cycles { get { return CycDef & 0xff; } }
        public CycleMod CycleMods { get { return (CycleMod)(CycDef & ~0xff); } }

        /// <summary>
        /// True if the instruction's address mode is a direct page access.
        /// </summary>
        public bool IsDirectPageInstruction {
            get {
                switch (AddrMode) {
                    case AddressMode.DP:
                    case AddressMode.DPInd:
                    case AddressMode.DPIndexX:
                    case AddressMode.DPIndexXInd:
                    case AddressMode.DPIndexY:
                    case AddressMode.DPIndIndexY:
                    case AddressMode.DPIndIndexYLong:
                    case AddressMode.DPIndLong:
                    case AddressMode.StackDPInd:
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// True if the instruction's operand is a stack-relative offset.
        /// </summary>
        public bool IsStackRelInstruction {
            get {
                return AddrMode == AddressMode.StackRel ||
                       AddrMode == AddressMode.StackRelIndIndexY;
            }
        }

        /// <summary>
        /// True if the operand's width is uniquely determined by the opcode mnemonic, even
        /// if the operation supports operands with varying widths.
        /// 
        /// Certain ops (ADC AND CMP EOR LDA ORA SBC STA) can be direct page, absolute, or
        /// absolute long.  "LDA 0" could mean LDA $00 (A5), LDA $0000 (AD), or LDA $000000 (AF).
        /// Similar ambiguities exist for some indexed modes.  Assemblers generally use the
        /// smallest form, but allow a longer form to be selected by modifying the opcode
        /// (e.g. LDA: and LDAL) or operand (e.g. LDA >0).
        /// 
        /// Most operations that access memory require disambiguation for direct page vs.
        /// absolute.  Generally speaking, if the operand's high byte is empty, disambiguation
        /// is required.
        /// 
        /// The JMP/JML and JSR/JSL mnemonics are commonly used (and, in some assemblers,
        /// required to be used) to avoid the ambiguity.  For these instructions, we want
        /// to avoid modifying the mnemonic, so we set this flag.
        /// </summary>
        private bool IsOperandWidthUnambiguous { get; set; }

        /// <summary>
        /// Flag update delegate, used for code flow analysis.
        /// </summary>
        /// <param name="prevFlags">Previous flags value.</param>
        /// <param name="immVal">Immediate mode value, if any.  Value may be 0-255
        ///   for an 8-bit operand, or 0-65535 for a 16-bit operand, or -1 if this is
        ///   not an immediate-mode instruction.</param>
        /// <param name="newFlags">New flags value.  For conditional branches, this is
        ///   the value to use when the branch is not taken.</param>
        /// <param name="branchTakenFlags">For conditional branches, this is the updated
        ///   value when the branch is taken.</param>
        private delegate StatusFlags FlagUpdater(StatusFlags flags, int immVal,
            ref StatusFlags condBranchTakenFlags);
        private FlagUpdater StatusFlagUpdater { get; set; }

        /// <summary>
        /// Nullary constructor.  Most things are left at default values.
        /// </summary>
        public OpDef() {
            StatusFlagUpdater = FlagUpdater_NoChange;
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="src">Object to copy from</param>
        public OpDef(OpDef src) {
            this.Opcode = src.Opcode;
            this.AddrMode = src.AddrMode;
            this.IsUndocumented = src.IsUndocumented;
            this.Mnemonic = src.Mnemonic;
            this.FlagsAffected = src.FlagsAffected;
            this.Effect = src.Effect;
            this.BaseMemEffect = src.BaseMemEffect;
            this.CycDef = src.CycDef;
            this.IsOperandWidthUnambiguous = src.IsOperandWidthUnambiguous;
            this.StatusFlagUpdater = src.StatusFlagUpdater;
        }

        private static StatusFlags FlagsAffected_V =
            new StatusFlags() { V = 1 };
        private static StatusFlags FlagsAffected_D =
            new StatusFlags() { D = 1 };
        private static StatusFlags FlagsAffected_I =
            new StatusFlags() { I = 1 };
        private static StatusFlags FlagsAffected_Z =
            new StatusFlags() { Z = 1 };
        private static StatusFlags FlagsAffected_C =
            new StatusFlags() { C = 1 };
        private static StatusFlags FlagsAffected_NZ =
            new StatusFlags() { N = 1, Z = 1 };
        private static StatusFlags FlagsAffected_NZC =
            new StatusFlags() { N = 1, Z = 1, C = 1 };
        private static StatusFlags FlagsAffected_NVZC =
            new StatusFlags() { N = 1, V = 1, Z = 1, C = 1 };
        private static StatusFlags FlagsAffected_All =
            new StatusFlags() { N = 1, V = 1, M = 1, X = 1, D = 1, I = 1, Z = 1, C = 1 };


        /// <summary>
        /// True if this operation is any type of branch instruction (conditional branch,
        /// unconditional branch/jump, subroutine call).
        /// </summary>
        public bool IsBranchOrSubCall {
            get {
                return Effect == FlowEffect.Branch ||
                    Effect == FlowEffect.ConditionalBranch ||
                    Effect == FlowEffect.CallSubroutine;
            }
        }

        /// <summary>
        /// True if this operation is a subroutine call.
        /// </summary>
        public bool IsSubroutineCall {
            get {
                return Effect == FlowEffect.CallSubroutine;
            }
        }

        /// <summary>
        /// True if the operand is an immediate value, which should be prefixed with '#'.
        /// </summary>
        public bool IsImmediate {
            get {
                return AddrMode == AddressMode.Imm ||
                    AddrMode == AddressMode.ImmLongA ||
                    AddrMode == AddressMode.ImmLongXY;
            }
        }

        /// <summary>
        /// True if the operand is an "extended immediate" value, which includes PEA and MVN/MVP
        /// in addition to Imm/ImmLongA/ImmLongXY.
        /// </summary>
        public bool IsExtendedImmediate {
            get {
                return IsImmediate || AddrMode == AddressMode.StackAbs ||
                    AddrMode == AddressMode.BlockMove;
            }
        }

        /// <summary>
        /// True if the operand's width could be ambiguous.  Generally speaking, assemblers
        /// will use the shortest form possible, so disambiguation is about using a longer
        /// form than may appear to be required.
        /// </summary>
        public bool IsWidthPotentiallyAmbiguous {
            get {
                if (IsOperandWidthUnambiguous) {
                    // JMP has Abs and AbsLong forms, but those are universally distinguished
                    // with unique mnemonics (JMP vs. JML).  We don't want to generate "JMPL"
                    // or "JMP >long".  Ditto for JSR/JSL.
                    return false;
                }
                switch (AddrMode) {
                    case AddressMode.Abs:               // LDA $0000 vs LDA $00
                    case AddressMode.AbsLong:           // LDA $000000 vs LDA $0000/LDA $00
                    case AddressMode.AbsIndexX:         // LDA $0000,X vs LDA $00,X
                    case AddressMode.AbsIndexXLong:     // LDA $000000,X vs LDA $0000,X/LDA $00,X
                        return true;
                    case AddressMode.AbsIndexY:         // LDX $0000,Y vs LDX $00,Y
                        // AbsIndexY is widely used, but DPIndexY is only available for LDX/STX,
                        // and STX doesn't have AbsIndexY.  So this is only ambiguous for LDX.
                        // We want to compare by opcode instance, rather than the byte code
                        // numeric value, to manage different instruction sets.
                        // (This also applies to the undocumented LAX instruction.)
                        if (this == OpLDX_AbsIndexY || this == OpLAX_AbsIndexY) {
                            return true;
                        }
                        return false;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Get a value that indicates what sort of disambiguation is required.  Only call
        /// this if IsWidthPotentiallyAmbiguous is true.
        /// </summary>
        /// <param name="instrWidth">Instruction width, including opcode.</param>
        /// <param name="operandValue">Operand value, extracted from byte stream.</param>
        /// <returns>Width disambiguation value.</returns>
        public static WidthDisambiguation GetWidthDisambiguation(int instrWidth,
                int operandValue) {
            Debug.Assert(instrWidth > 2 && instrWidth <= 4);   // zero-page ops are not ambiguous
            if (instrWidth == 3 && operandValue < 0x100) {
                return WidthDisambiguation.ForceAbs;
            } else if (instrWidth == 4) {
                if (operandValue < 0x10000) {
                    return WidthDisambiguation.ForceLong;
                } else {
                    // The width disambiguator may be helpful for humans when reading labels
                    // whose value is not immediately apparent.  "LDA a:FOO" vs. "LDA f:FOO"
                    // could be nice.
                    return WidthDisambiguation.ForceLongMaybe;
                }
            } else {
                return WidthDisambiguation.None;
            }
        }

        /// <summary>
        /// Returns the full length of the instruction.  Some 65816 operations use
        /// a different number of bytes in 16-bit mode, so we need to know what the
        /// M/X status flags are.
        /// </summary>
        /// <param name="flags">Current status flags.</param>
        /// <returns>Length, in bytes.</returns>
        public int GetLength(StatusFlags flags) {
            switch (AddrMode) {
                case AddressMode.Unknown:
                case AddressMode.Acc:
                case AddressMode.Implied:
                case AddressMode.StackPull:
                case AddressMode.StackPush:
                case AddressMode.StackRTI:
                case AddressMode.StackRTL:
                case AddressMode.StackRTS:
                    return 1;
                case AddressMode.DP:
                case AddressMode.DPIndexX:
                case AddressMode.DPIndexY:
                case AddressMode.DPIndexXInd:
                case AddressMode.DPInd:
                case AddressMode.DPIndLong:
                case AddressMode.DPIndIndexY:
                case AddressMode.DPIndIndexYLong:
                case AddressMode.Imm:
                case AddressMode.PCRel:
                case AddressMode.StackDPInd:
                case AddressMode.StackInt:
                case AddressMode.StackRel:
                case AddressMode.StackRelIndIndexY:
                case AddressMode.WDM:
                    return 2;
                case AddressMode.Abs:
                case AddressMode.AbsIndexX:
                case AddressMode.AbsIndexY:
                case AddressMode.AbsIndexXInd:
                case AddressMode.AbsInd:
                case AddressMode.AbsIndLong:
                case AddressMode.BlockMove:
                case AddressMode.PCRelLong:
                case AddressMode.StackAbs:
                case AddressMode.StackPCRelLong:
                    return 3;

                case AddressMode.AbsLong:
                case AddressMode.AbsIndexXLong:
                    return 4;

                case AddressMode.ImmLongA:
                    bool shortM = flags.ShortM;
                    return shortM ? 2 : 3;
                case AddressMode.ImmLongXY:
                    bool shortX = flags.ShortX;
                    return shortX ? 2 : 3;

                default:
                    Debug.Assert(false, "Unknown address mode " + AddrMode);
                    return -1;
            }
        }


        /// <summary>
        /// Return value from branch evaluation functions.
        /// </summary>
        public enum BranchTaken { Indeterminate = 0, Never, Always };

        /// <summary>
        /// Determines whether a branch is always taken, never taken, or is indeterminate
        /// (i.e. it's taken some of the time but not taken others).
        /// </summary>
        /// <param name="flagVal">Processor status flag value.</param>
        /// <param name="branchVal">Bit value corresponding to branch-taken.</param>
        /// <returns>Branch taken indication.</returns>
        private static BranchTaken FlagToBT(int flagVal, int branchVal) {
            if (flagVal == TriState16.INDETERMINATE) {
                return BranchTaken.Indeterminate;
            } else if (flagVal == TriState16.UNSPECIFIED) {
                // should never happen
                Debug.Assert(false);
                return BranchTaken.Indeterminate;
            } else if (flagVal == branchVal) {
                return BranchTaken.Always;
            } else {
                return BranchTaken.Never;
            }
        }

        /// <summary>
        /// Determines if a conditional branch is always taken, based on the status flags.
        /// </summary>
        /// <param name="op">Conditional branch instruction.</param>
        /// <param name="flags">Processor status flags.</param>
        /// <returns>Whether the branch is sometimes, always, or never taken.</returns>
        public static BranchTaken IsBranchTaken(OpDef op, StatusFlags flags) {
            // Could add a delegate to every OpDef, but that seems silly.
            switch (op.Opcode) {
                case 0x10:      // BPL
                    return FlagToBT(flags.N, 0);
                case 0x30:      // BMI
                    return FlagToBT(flags.N, 1);
                case 0x50:      // BVC
                    return FlagToBT(flags.V, 0);
                case 0x70:      // BVS
                    return FlagToBT(flags.V, 1);
                case 0x90:      // BCC
                    return FlagToBT(flags.C, 0);
                case 0xb0:      // BCS
                    return FlagToBT(flags.C, 1);
                case 0xd0:      // BNE
                    return FlagToBT(flags.Z, 0);
                case 0xf0:      // BEQ
                    return FlagToBT(flags.Z, 1);
                default:
                    // Not a conditional branch.
                    throw new Exception("Not a conditional branch");
            }
        }

        /// <summary>
        /// Get the raw operand value.
        /// </summary>
        /// <param name="data">65xx code.</param>
        /// <param name="offset">Offset of opcode.</param>
        /// <param name="flags">Current status flags.</param>
        /// <returns>Operand value.</returns>
        public int GetOperand(byte[] data, int offset, StatusFlags flags) {
            switch (GetLength(flags)) {
                case 1:
                    return -1;
                case 2:
                    return data[offset + 1];
                case 3:
                    return (data[offset + 2] << 8) | data[offset + 1];
                case 4:
                    return (data[offset + 3] << 16) | (data[offset + 2] << 8) | data[offset + 1];
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Computes the changes the instruction will have on the status flags.
        /// </summary>
        /// <param name="curFlags">Current status flags. This is used as the initial value
        ///   for the return values, and determines whether 65816 immediate operands are
        ///   treated as 8- or 16-bit.</param>
        /// <param name="data">65xx data stream.</param>
        /// <param name="offset">Offset of opcode.</param>
        /// <param name="newFlags">Effect this instruction has on flags.</param>
        /// <param name="condBranchTakenFlags">Effect this conditional branch instruction
        ///   has on flags when the branch is taken.  If this is not a conditional branch,
        ///   the value can be ignored.</param>
        public void ComputeFlagChanges(StatusFlags curFlags, byte[] data, int offset,
                out StatusFlags newFlags, out StatusFlags condBranchTakenFlags) {
            int immVal = -1;

            switch (AddrMode) {
                case AddressMode.Imm:
                case AddressMode.ImmLongA:
                case AddressMode.ImmLongXY:
                    if (GetLength(curFlags) == 2) {
                        // 8-bit operand
                        immVal = data[offset + 1];
                    } else {
                        // 16-bit operand; make as such by negating it
                        // (if it's zero, 8 vs. 16 doesn't matter)
                        immVal = -((data[offset + 2]) << 16 | data[offset + 1]);
                    }
                    break;
                default:
                    break;
            }

            condBranchTakenFlags = curFlags;
            // Invoke the flag update delegate.
            newFlags = StatusFlagUpdater(curFlags, immVal, ref condBranchTakenFlags);

            // TODO(maybe): there are some constraints we can impose: if Z=1 then
            //  N=0, and if N=1 then Z=0.  I'm not sure this is actually useful though.
        }

        private static StatusFlags FlagUpdater_NoChange(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            return flags;
        }
        private static StatusFlags FlagUpdater_Subroutine(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // The correct way to do this is to merge the flags from all code
            // that RTS/RTLs back to here, but that's generally hard to do, especially
            // since this will often be used to call into external code.  The easiest
            // thing to do is scramble CZVN and leave IDXM alone.
            return FlagUpdater_NVZC(flags, immVal, ref condBranchTakenFlags);
        }
        private static StatusFlags FlagUpdater_Z(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            flags.Z = TriState16.INDETERMINATE;
            return flags;
        }
        private static StatusFlags FlagUpdater_C(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            flags.C = TriState16.INDETERMINATE;
            return flags;
        }
        private static StatusFlags FlagUpdater_NZ(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            flags.Z = flags.N = TriState16.INDETERMINATE;
            return flags;
        }
        private static StatusFlags FlagUpdater_NZC(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            flags.C = flags.Z = flags.N = TriState16.INDETERMINATE;
            return flags;
        }
        private static StatusFlags FlagUpdater_NVZC(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            flags.C = flags.Z = flags.V = flags.N = TriState16.INDETERMINATE;
            return flags;
        }
        private static StatusFlags FlagUpdater_LoadImm(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // We can set the N/Z flags based on the value loaded.
            flags.Z = (immVal != 0) ? 0 : 1;
            if (immVal >= 0) {
                // 8-bit operand
                flags.N = (immVal >> 7) & 0x01;
            } else {
                // 16-bit operand
                immVal = -immVal;
                flags.N = (immVal >> 15) & 0x01;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_ANDImm(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // AND #00 --> Z=1, else Z=prev
            // AND #7f --> N=0, else N=prev
            if (immVal == 0) {
                flags.Z = 1;
            }
            bool hiBitClear;
            if (immVal >= 0) {
                // 8-bit operand
                hiBitClear = ((immVal >> 7) & 0x01) == 0;
            } else {
                // 16-bit operand
                immVal = -immVal;
                hiBitClear = ((immVal >> 15) & 0x01) == 0;
            }
            if (hiBitClear) {
                flags.N = 0;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_ORAImm(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // ORA #00 --> Z=prev, else Z=0
            // ORA #80 --> N=1, else N=prev
            if (immVal != 0) {
                flags.Z = 0;
            }
            bool hiBitSet;
            if (immVal >= 0) {
                // 8-bit operand
                hiBitSet = ((immVal >> 7) & 0x01) != 0;
            } else {
                // 16-bit operand
                immVal = -immVal;
                hiBitSet = ((immVal >> 15) & 0x01) != 0;
            }
            if (hiBitSet) {
                flags.N = 1;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_ROL(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // this rotates the N flag into C, so set C=N
            // if carry is one, set Z=0; otherwise set Z/N=indeterminate
            // (if Z=1 we should set Z=C, but this seems rare and I don't entirely trust Z)
            if (flags.C == 1) {
                flags.C = flags.N;
                flags.Z = 0;
                flags.N = TriState16.INDETERMINATE;
            } else {
                flags.C = flags.N;
                flags.Z = flags.N = TriState16.INDETERMINATE;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_ROR(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // if carry is set, set Z=0 and N=1;
            // if carry is clear, set N=0;
            // if carry is clear and Z=1, everything is zero and no flags change
            //  (this seems unlikely, so I'm going to assume we've mis-read a flag and ignore this)
            // otherwise, Z/N/C=indeterminate
            if (flags.C == 1) {
                flags.Z = 0;
                flags.N = 1;
                flags.C = TriState16.INDETERMINATE;
            } else if (flags.C == 0) {
                flags.N = 0;
                flags.Z = flags.C = TriState16.INDETERMINATE;
            } else {
                flags.C = flags.Z = flags.N = TriState16.INDETERMINATE;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_PLP(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // All flags are unknown. The caller may be able to match with a previous
            // PHP to get something less fuzzy.
            flags.C = flags.Z = flags.I = flags.D = flags.X = flags.M = flags.V = flags.N =
                TriState16.INDETERMINATE;
            return flags;
        }
        private static StatusFlags FlagUpdater_REP(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            if ((immVal & (1 << (int)StatusFlags.FlagBits.C)) != 0) {
                flags.C = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.Z)) != 0) {
                flags.Z = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.I)) != 0) {
                flags.I = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.D)) != 0) {
                flags.D = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.X)) != 0) {
                flags.X = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.M)) != 0) {
                flags.M = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.V)) != 0) {
                flags.V = 0;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.N)) != 0) {
                flags.N = 0;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_SEP(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            if ((immVal & (1 << (int)StatusFlags.FlagBits.C)) != 0) {
                flags.C = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.Z)) != 0) {
                flags.Z = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.I)) != 0) {
                flags.I = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.D)) != 0) {
                flags.D = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.X)) != 0) {
                flags.X = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.M)) != 0) {
                flags.M = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.V)) != 0) {
                flags.V = 1;
            }
            if ((immVal & (1 << (int)StatusFlags.FlagBits.N)) != 0) {
                flags.N = 1;
            }
            return flags;
        }
        private static StatusFlags FlagUpdater_XCE(StatusFlags flags, int immVal,
                ref StatusFlags condBranchTakenFlags) {
            // We want the 'E' flag to always have a definite value.  If we don't know
            // what's in the carry flag, guess it's a return to emulation.
            StatusFlags newFlags = flags;
            if (flags.C == 0) {
                // transition to native
                newFlags.E = 0;
                newFlags.X = newFlags.M = 1;
            } else /*C==1 or C==?*/ {
                // transition to emulation
                // The registers will be treated as short by the CPU, ignoring M/X, but
                // the assembler won't generally know that.  Set the flags to definite
                // values here so we generate the necessary directives.
                newFlags.E = 1;
                newFlags.X = newFlags.M = 1;
            }
            newFlags.C = flags.E;
            return newFlags;
        }


        // ======================================================================================
        // Base operation definitions, one per op.
        //

        private static OpDef OpUnknown = new OpDef() {
            Mnemonic = OpName.Unknown,
            Effect = FlowEffect.Cont
        };
        private static OpDef OpADC = new OpDef() {
            Mnemonic = OpName.ADC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NVZC,
            StatusFlagUpdater = FlagUpdater_NVZC
            // We can assert C=0 after ADC #$00, but only if C=0 on entry.  Doesn't seem useful.
        };
        private static OpDef OpAND = new OpDef() {
            Mnemonic = OpName.AND,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ  // special handling for imm
        };
        private static OpDef OpASL = new OpDef() {
            Mnemonic = OpName.ASL,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpBCC = new OpDef() {
            Mnemonic = OpName.BCC,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate(StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.C = 0;
                flags.C = 1;
                return flags;
            }
        };
        private static OpDef OpBCS = new OpDef() {
            Mnemonic = OpName.BCS,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.C = 1;
                flags.C = 0;
                return flags;
            }
        };
        private static OpDef OpBEQ = new OpDef() {
            Mnemonic = OpName.BEQ,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.Z = 1;
                flags.Z = 0;
                return flags;
            }
        };
        private static OpDef OpBIT = new OpDef() {
            Mnemonic = OpName.BIT,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZC,  // special handling for imm
            StatusFlagUpdater = FlagUpdater_NZC // special handling for imm
        };
        private static OpDef OpBMI = new OpDef() {
            Mnemonic = OpName.BMI,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.N = 1;
                flags.N = 0;
                return flags;
            }
        };
        private static OpDef OpBNE = new OpDef() {
            Mnemonic = OpName.BNE,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.Z = 0;
                flags.Z = 1;
                return flags;
            }
        };
        private static OpDef OpBPL = new OpDef() {
            Mnemonic = OpName.BPL,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.N = 0;
                flags.N = 1;
                return flags;
            }
        };
        private static OpDef OpBRA = new OpDef() {
            Mnemonic = OpName.BRA,
            Effect = FlowEffect.Branch,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpBRK = new OpDef() {
            Mnemonic = OpName.BRK,
            Effect = FlowEffect.NoCont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpBRL = new OpDef() {
            Mnemonic = OpName.BRL,
            Effect = FlowEffect.Branch,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpBVC = new OpDef() {
            Mnemonic = OpName.BVC,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.V = 0;
                flags.V = 1;
                return flags;
            }
        };
        private static OpDef OpBVS = new OpDef() {
            Mnemonic = OpName.BVS,
            Effect = FlowEffect.ConditionalBranch,
            BaseMemEffect = MemoryEffect.None,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                condBranchTakenFlags.V = 1;
                flags.V = 0;
                return flags;
            }
        };
        private static OpDef OpCLC = new OpDef() {
            Mnemonic = OpName.CLC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_C,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.C = 0;
                return flags;
            }
        };
        private static OpDef OpCLD = new OpDef() {
            Mnemonic = OpName.CLD,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_D,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.D = 0;
                return flags;
            }
        };
        private static OpDef OpCLI = new OpDef() {
            Mnemonic = OpName.CLI,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_I,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.I = 0;
                return flags;
            }
        };
        private static OpDef OpCLV = new OpDef() {
            Mnemonic = OpName.CLV,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_V,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.V = 0;
                return flags;
            }
        };
        private static OpDef OpCMP = new OpDef() {
            Mnemonic = OpName.CMP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpCOP = new OpDef() {
            Mnemonic = OpName.COP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpCPX = new OpDef() {
            Mnemonic = OpName.CPX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpCPY = new OpDef() {
            Mnemonic = OpName.CPY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpDEC = new OpDef() {
            Mnemonic = OpName.DEC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpDEX = new OpDef() {
            Mnemonic = OpName.DEX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpDEY = new OpDef() {
            Mnemonic = OpName.DEY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpEOR = new OpDef() {
            Mnemonic = OpName.EOR,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpINC = new OpDef() {
            Mnemonic = OpName.INC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpINX = new OpDef() {
            Mnemonic = OpName.INX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpINY = new OpDef() {
            Mnemonic = OpName.INY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpJML = new OpDef() {
            Mnemonic = OpName.JML,      // technically JMP with long operand, but JML is convention
            Effect = FlowEffect.Branch,
            BaseMemEffect = MemoryEffect.None,
            IsOperandWidthUnambiguous = true,
        };
        private static OpDef OpJMP = new OpDef() {
            Mnemonic = OpName.JMP,
            Effect = FlowEffect.Branch,
            BaseMemEffect = MemoryEffect.None,
            IsOperandWidthUnambiguous = true
        };
        private static OpDef OpJSL = new OpDef() {
            Mnemonic = OpName.JSL,      // technically JSR with long operand, but JSL is convention
            Effect = FlowEffect.CallSubroutine,
            BaseMemEffect = MemoryEffect.None,
            IsOperandWidthUnambiguous = true,
            StatusFlagUpdater = FlagUpdater_Subroutine
        };
        private static OpDef OpJSR = new OpDef() {
            Mnemonic = OpName.JSR,
            Effect = FlowEffect.CallSubroutine,
            BaseMemEffect = MemoryEffect.None,
            IsOperandWidthUnambiguous = true,
            StatusFlagUpdater = FlagUpdater_Subroutine
        };
        private static OpDef OpLDA = new OpDef() {
            Mnemonic = OpName.LDA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ  // special handling for imm
        };
        private static OpDef OpLDX = new OpDef() {
            Mnemonic = OpName.LDX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ  // special handling for imm
        };
        private static OpDef OpLDY = new OpDef() {
            Mnemonic = OpName.LDY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ  // special handling for imm
        };
        private static OpDef OpLSR = new OpDef() {
            Mnemonic = OpName.LSR,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpMVN = new OpDef() {
            Mnemonic = OpName.MVN,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None       // not quite right, but what is?
        };
        private static OpDef OpMVP = new OpDef() {
            Mnemonic = OpName.MVP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpNOP = new OpDef() {
            Mnemonic = OpName.NOP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpORA = new OpDef() {
            Mnemonic = OpName.ORA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ  // special handling for imm
        };
        private static OpDef OpPEA = new OpDef() {
            Mnemonic = OpName.PEA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPEI = new OpDef() {
            Mnemonic = OpName.PEI,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read
        };
        private static OpDef OpPER = new OpDef() {
            Mnemonic = OpName.PER,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHA = new OpDef() {
            Mnemonic = OpName.PHA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHB = new OpDef() {
            Mnemonic = OpName.PHB,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHD = new OpDef() {
            Mnemonic = OpName.PHD,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHK = new OpDef() {
            Mnemonic = OpName.PHK,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHP = new OpDef() {
            Mnemonic = OpName.PHP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHX = new OpDef() {
            Mnemonic = OpName.PHX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPHY = new OpDef() {
            Mnemonic = OpName.PHY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpPLA = new OpDef() {
            Mnemonic = OpName.PLA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpPLB = new OpDef() {
            Mnemonic = OpName.PLB,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpPLD = new OpDef() {
            Mnemonic = OpName.PLD,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpPLP = new OpDef() {
            Mnemonic = OpName.PLP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_All,
            StatusFlagUpdater = FlagUpdater_PLP
        };
        private static OpDef OpPLX = new OpDef() {
            Mnemonic = OpName.PLX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpPLY = new OpDef() {
            Mnemonic = OpName.PLY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpREP = new OpDef() {
            Mnemonic = OpName.REP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_All,
            StatusFlagUpdater = FlagUpdater_REP
        };
        private static OpDef OpROL = new OpDef() {
            Mnemonic = OpName.ROL,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_ROL
        };
        private static OpDef OpROR = new OpDef() {
            Mnemonic = OpName.ROR,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_ROR
        };
        private static OpDef OpRTI = new OpDef() {
            Mnemonic = OpName.RTI,
            Effect = FlowEffect.NoCont,
            BaseMemEffect = MemoryEffect.None,
        };
        private static OpDef OpRTL = new OpDef() {
            Mnemonic = OpName.RTL,
            Effect = FlowEffect.NoCont,
            BaseMemEffect = MemoryEffect.None,
        };
        private static OpDef OpRTS = new OpDef() {
            Mnemonic = OpName.RTS,
            Effect = FlowEffect.NoCont,
            BaseMemEffect = MemoryEffect.None,
        };
        private static OpDef OpSBC = new OpDef() {
            Mnemonic = OpName.SBC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NVZC,
            StatusFlagUpdater = FlagUpdater_NVZC
        };
        private static OpDef OpSEC = new OpDef() {
            Mnemonic = OpName.SEC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_C,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.C = 1;
                return flags;
            }
        };
        private static OpDef OpSED = new OpDef() {
            Mnemonic = OpName.SED,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_D,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.D = 1;
                return flags;
            }
        };
        private static OpDef OpSEI = new OpDef() {
            Mnemonic = OpName.SEI,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_I,
            StatusFlagUpdater = delegate (StatusFlags flags, int immVal,
                    ref StatusFlags condBranchTakenFlags) {
                flags.I = 1;
                return flags;
            }
        };
        private static OpDef OpSEP = new OpDef() {
            Mnemonic = OpName.SEP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_All,
            StatusFlagUpdater = FlagUpdater_SEP
        };
        private static OpDef OpSTA = new OpDef() {
            Mnemonic = OpName.STA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write,
        };
        private static OpDef OpSTP = new OpDef() {
            Mnemonic = OpName.STP,
            Effect = FlowEffect.NoCont,
            BaseMemEffect = MemoryEffect.None,
        };
        private static OpDef OpSTX = new OpDef() {
            Mnemonic = OpName.STX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write,
        };
        private static OpDef OpSTY = new OpDef() {
            Mnemonic = OpName.STY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write,
        };
        private static OpDef OpSTZ = new OpDef() {
            Mnemonic = OpName.STZ,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write,
        };
        private static OpDef OpTAX = new OpDef() {
            Mnemonic = OpName.TAX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTAY = new OpDef() {
            Mnemonic = OpName.TAY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTCD = new OpDef() {
            Mnemonic = OpName.TCD,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTCS = new OpDef() {
            Mnemonic = OpName.TCS,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpTDC = new OpDef() {
            Mnemonic = OpName.TDC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTRB = new OpDef() {
            Mnemonic = OpName.TRB,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_Z,
            StatusFlagUpdater = FlagUpdater_Z
        };
        private static OpDef OpTSB = new OpDef() {
            Mnemonic = OpName.TSB,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_Z,
            StatusFlagUpdater = FlagUpdater_Z
        };
        private static OpDef OpTSC = new OpDef() {
            Mnemonic = OpName.TSC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTSX = new OpDef() {
            Mnemonic = OpName.TSX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTXA = new OpDef() {
            Mnemonic = OpName.TXA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTXS = new OpDef() {
            Mnemonic = OpName.TXS,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpTXY = new OpDef() {
            Mnemonic = OpName.TXY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTYA = new OpDef() {
            Mnemonic = OpName.TYA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpTYX = new OpDef() {
            Mnemonic = OpName.TYX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpWAI = new OpDef() {
            Mnemonic = OpName.WAI,
            Effect = FlowEffect.Cont,   // when I=1 (interrupts disabled), continues on interrupt
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpWDM = new OpDef() {
            Mnemonic = OpName.WDM,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpXBA = new OpDef() {
            Mnemonic = OpName.XBA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpXCE = new OpDef() {
            Mnemonic = OpName.XCE,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_C,
            StatusFlagUpdater = FlagUpdater_XCE
        };


        #region 65816 Instructions

        // ======================================================================================
        // Operation + address mode definitions.
        //
        // It's possible for more than one definition to have the same opcode number.  The
        // 6502 only supports 8-bit LDA imm, while the 65816 version can be 8 or 16.  There
        // are minor differences in behavior for some opcodes on different CPUs.
        //
        // Significantly, the "invalid" 6502 ops overlap with official ops defined on later CPUs.
        //

        public static readonly OpDef OpInvalid = new OpDef(OpUnknown) {
            AddrMode = AddressMode.Unknown
        };

        public static readonly OpDef OpBRK_Implied = new OpDef(OpBRK) {     // 1-byte form
            Opcode = 0x00,
            AddrMode = AddressMode.Implied,
            CycDef = 7 | (int)(CycleMod.OneIfE0)
        };
        public static readonly OpDef OpBRK_StackInt = new OpDef(OpBRK) {    // 2-byte form
            Opcode = 0x00,
            AddrMode = AddressMode.StackInt,
            CycDef = 7 | (int)(CycleMod.OneIfE0)
        };
        public static readonly OpDef OpORA_DPIndexXInd = new OpDef(OpORA) {
            Opcode = 0x01,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCOP_StackInt = new OpDef(OpCOP) {
            Opcode = 0x02,
            AddrMode = AddressMode.StackInt,
            CycDef = 7 | (int)(CycleMod.OneIfE0)
        };
        public static readonly OpDef OpORA_StackRel = new OpDef(OpORA) {
            Opcode = 0x03,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpTSB_DP = new OpDef(OpTSB) {
            Opcode = 0x04,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_DP = new OpDef(OpORA) {
            Opcode = 0x05,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpASL_DP = new OpDef(OpASL) {
            Opcode = 0x06,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_DPIndLong = new OpDef(OpORA) {
            Opcode = 0x07,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpPHP_StackPush = new OpDef(OpPHP) {
            Opcode = 0x08,
            AddrMode = AddressMode.StackPush,
            CycDef = 3
        };
        public static readonly OpDef OpORA_ImmLongA = new OpDef(OpORA) {
            Opcode = 0x09,
            AddrMode = AddressMode.ImmLongA,
            StatusFlagUpdater = FlagUpdater_ORAImm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpORA_Imm = new OpDef(OpORA) {
            Opcode = 0x09,
            AddrMode = AddressMode.Imm,
            StatusFlagUpdater = FlagUpdater_ORAImm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpASL_Acc = new OpDef(OpASL) {
            Opcode = 0x0a,
            AddrMode = AddressMode.Acc,
            CycDef = 2
        };
        public static readonly OpDef OpPHD_StackPush = new OpDef(OpPHD) {
            Opcode = 0x0b,
            AddrMode = AddressMode.StackPush,
            CycDef = 4
        };
        public static readonly OpDef OpTSB_Abs = new OpDef(OpTSB) {
            Opcode = 0x0c,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_Abs = new OpDef(OpORA) {
            Opcode = 0x0d,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpASL_Abs = new OpDef(OpASL) {
            Opcode = 0x0e,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_AbsLong = new OpDef(OpORA) {
            Opcode = 0x0f,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBPL_PCRel = new OpDef(OpBPL) {
            Opcode = 0x10,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpORA_DPIndIndexY = new OpDef(OpORA) {
            Opcode = 0x11,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpORA_DPInd = new OpDef(OpORA) {
            Opcode = 0x12,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpORA_StackRelIndIndexY = new OpDef(OpORA) {
            Opcode = 0x13,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpTRB_DP = new OpDef(OpTRB) {
            Opcode = 0x14,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_DPIndexX = new OpDef(OpORA) {
            Opcode = 0x15,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpASL_DPIndexX = new OpDef(OpASL) {
            Opcode = 0x16,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_DPIndIndexYLong = new OpDef(OpORA) {
            Opcode = 0x17,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCLC_Implied = new OpDef(OpCLC) {
            Opcode = 0x18,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpORA_AbsIndexY = new OpDef(OpORA) {
            Opcode = 0x19,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpINC_Acc = new OpDef(OpINC) {
            Opcode = 0x1a,
            AddrMode = AddressMode.Acc,
            CycDef = 2
        };
        public static readonly OpDef OpTCS_Implied = new OpDef(OpTCS) {
            Opcode = 0x1b,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpTRB_Abs = new OpDef(OpTRB) {
            Opcode = 0x1c,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpORA_AbsIndexX = new OpDef(OpORA) {
            Opcode = 0x1d,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpASL_AbsIndexX = new OpDef(OpASL) {
            Opcode = 0x1e,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7 | (int)(CycleMod.TwoIfM0 | CycleMod.MinusOneIfNoPage)
        };
        public static readonly OpDef OpORA_AbsIndexXLong = new OpDef(OpORA) {
            Opcode = 0x1f,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpJSR_Abs = new OpDef(OpJSR) {
            Opcode = 0x20,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpAND_DPIndexXInd = new OpDef(OpAND) {
            Opcode = 0x21,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpJSR_AbsLong = new OpDef(OpJSL) {
            Opcode = 0x22,
            AddrMode = AddressMode.AbsLong,
            CycDef = 8
        };
        public static readonly OpDef OpAND_StackRel = new OpDef(OpAND) {
            Opcode = 0x23,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBIT_DP = new OpDef(OpBIT) {
            Opcode = 0x24,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpAND_DP = new OpDef(OpAND) {
            Opcode = 0x25,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpROL_DP = new OpDef(OpROL) {
            Opcode = 0x26,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpAND_DPIndLong = new OpDef(OpAND) {
            Opcode = 0x27,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpPLP_StackPull = new OpDef(OpPLP) {
            Opcode = 0x28,
            AddrMode = AddressMode.StackPull,
            CycDef = 4
        };
        public static readonly OpDef OpAND_ImmLongA = new OpDef(OpAND) {
            Opcode = 0x29,
            AddrMode = AddressMode.ImmLongA,
            StatusFlagUpdater = FlagUpdater_ANDImm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpAND_Imm = new OpDef(OpAND) {
            Opcode = 0x29,
            AddrMode = AddressMode.Imm,
            StatusFlagUpdater = FlagUpdater_ANDImm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpROL_Acc = new OpDef(OpROL) {
            Opcode = 0x2a,
            AddrMode = AddressMode.Acc,
            CycDef = 2
        };
        public static readonly OpDef OpPLD_StackPull = new OpDef(OpPLD) {
            Opcode = 0x2b,
            AddrMode = AddressMode.StackPull,
            CycDef = 5
        };
        public static readonly OpDef OpBIT_Abs = new OpDef(OpBIT) {
            Opcode = 0x2c,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpAND_Abs = new OpDef(OpAND) {
            Opcode = 0x2d,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpROL_Abs = new OpDef(OpROL) {
            Opcode = 0x2e,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpAND_AbsLong = new OpDef(OpAND) {
            Opcode = 0x2f,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBMI_PCRel = new OpDef(OpBMI) {
            Opcode = 0x30,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpAND_DPIndIndexY = new OpDef(OpAND) {
            Opcode = 0x31,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpAND_DPInd = new OpDef(OpAND) {
            Opcode = 0x32,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpAND_StackRelIndIndexY = new OpDef(OpAND) {
            Opcode = 0x33,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBIT_DPIndexX = new OpDef(OpBIT) {
            Opcode = 0x34,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpAND_DPIndexX = new OpDef(OpAND) {
            Opcode = 0x35,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpROL_DPIndexX = new OpDef(OpROL) {
            Opcode = 0x36,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpAND_DPIndIndexYLong = new OpDef(OpAND) {
            Opcode = 0x37,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpSEC_Implied = new OpDef(OpSEC) {
            Opcode = 0x38,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpAND_AbsIndexY = new OpDef(OpAND) {
            Opcode = 0x39,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpDEC_Acc = new OpDef(OpDEC) {
            Opcode = 0x3a,
            AddrMode = AddressMode.Acc,
            CycDef = 2
        };
        public static readonly OpDef OpTSC_Implied = new OpDef(OpTSC) {
            Opcode = 0x3b,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpBIT_AbsIndexX = new OpDef(OpBIT) {
            Opcode = 0x3c,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpAND_AbsIndexX = new OpDef(OpAND) {
            Opcode = 0x3d,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpROL_AbsIndexX = new OpDef(OpROL) {
            Opcode = 0x3e,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7 | (int)(CycleMod.TwoIfM0 | CycleMod.MinusOneIfNoPage)
        };
        public static readonly OpDef OpAND_AbsIndexXLong = new OpDef(OpAND) {
            Opcode = 0x3f,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpRTI_StackRTI = new OpDef(OpRTI) {
            Opcode = 0x40,
            AddrMode = AddressMode.StackRTI,
            CycDef = 6 | (int)(CycleMod.OneIfE0)
        };
        public static readonly OpDef OpEOR_DPIndexXInd = new OpDef(OpEOR) {
            Opcode = 0x41,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpWDM_WDM = new OpDef(OpWDM) {
            Opcode = 0x42,
            AddrMode = AddressMode.WDM,
            CycDef = 2      // arbitrary; actual time is undefined
        };
        public static readonly OpDef OpEOR_StackRel = new OpDef(OpEOR) {
            Opcode = 0x43,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpMVP_BlockMove = new OpDef(OpMVP) {
            Opcode = 0x44,
            AddrMode = AddressMode.BlockMove,
            CycDef = 7 | (int)(CycleMod.BlockMove)
        };
        public static readonly OpDef OpEOR_DP = new OpDef(OpEOR) {
            Opcode = 0x45,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpLSR_DP = new OpDef(OpLSR) {
            Opcode = 0x46,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpEOR_DPIndLong = new OpDef(OpEOR) {
            Opcode = 0x47,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpPHA_StackPush = new OpDef(OpPHA) {
            Opcode = 0x48,
            AddrMode = AddressMode.StackPush,
            CycDef = 3 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpEOR_ImmLongA = new OpDef(OpEOR) {
            Opcode = 0x49,
            AddrMode = AddressMode.ImmLongA,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpEOR_Imm = new OpDef(OpEOR) {
            Opcode = 0x49,
            AddrMode = AddressMode.Imm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLSR_Acc = new OpDef(OpLSR) {
            Opcode = 0x4a,
            AddrMode = AddressMode.Acc,
            CycDef = 2
        };
        public static readonly OpDef OpPHK_StackPush = new OpDef(OpPHK) {
            Opcode = 0x4b,
            AddrMode = AddressMode.StackPush,
            CycDef = 3
        };
        public static readonly OpDef OpJMP_Abs = new OpDef(OpJMP) {
            Opcode = 0x4c,
            AddrMode = AddressMode.Abs,
            CycDef = 3
        };
        public static readonly OpDef OpEOR_Abs = new OpDef(OpEOR) {
            Opcode = 0x4d,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLSR_Abs = new OpDef(OpLSR) {
            Opcode = 0x4e,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpEOR_AbsLong = new OpDef(OpEOR) {
            Opcode = 0x4f,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBVC_PCRel = new OpDef(OpBVC) {
            Opcode = 0x50,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpEOR_DPIndIndexY = new OpDef(OpEOR) {
            Opcode = 0x51,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpEOR_DPInd = new OpDef(OpEOR) {
            Opcode = 0x52,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpEOR_StackRelIndIndexY = new OpDef(OpEOR) {
            Opcode = 0x53,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpMVN_BlockMove = new OpDef(OpMVN) {
            Opcode = 0x54,
            AddrMode = AddressMode.BlockMove,
            CycDef = 7 | (int)(CycleMod.BlockMove)
        };
        public static readonly OpDef OpEOR_DPIndexX = new OpDef(OpEOR) {
            Opcode = 0x55,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpLSR_DPIndexX = new OpDef(OpLSR) {
            Opcode = 0x56,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpEOR_DPIndIndexYLong = new OpDef(OpEOR) {
            Opcode = 0x57,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCLI_Implied = new OpDef(OpCLI) {
            Opcode = 0x58,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpEOR_AbsIndexY = new OpDef(OpEOR) {
            Opcode = 0x59,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpPHY_StackPush = new OpDef(OpPHY) {
            Opcode = 0x5a,
            AddrMode = AddressMode.StackPush,
            CycDef = 3 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpTCD_Implied = new OpDef(OpTCD) {
            Opcode = 0x5b,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpJMP_AbsLong = new OpDef(OpJML) {
            Opcode = 0x5c,
            AddrMode = AddressMode.AbsLong,
            CycDef = 4
        };
        public static readonly OpDef OpEOR_AbsIndexX = new OpDef(OpEOR) {
            Opcode = 0x5d,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpLSR_AbsIndexX = new OpDef(OpLSR) {
            Opcode = 0x5e,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7 | (int)(CycleMod.TwoIfM0 | CycleMod.MinusOneIfNoPage)
        };
        public static readonly OpDef OpEOR_AbsIndexXLong = new OpDef(OpEOR) {
            Opcode = 0x5f,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpRTS_StackRTS = new OpDef(OpRTS) {
            Opcode = 0x60,
            AddrMode = AddressMode.StackRTS,
            CycDef = 6
        };
        public static readonly OpDef OpADC_DPIndexXInd = new OpDef(OpADC) {
            Opcode = 0x61,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpPER_StackPCRelLong = new OpDef(OpPER) {
            Opcode = 0x62,
            AddrMode = AddressMode.StackPCRelLong,
            CycDef = 6
        };
        public static readonly OpDef OpADC_StackRel = new OpDef(OpADC) {
            Opcode = 0x63,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSTZ_DP = new OpDef(OpSTZ) {
            Opcode = 0x64,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpADC_DP = new OpDef(OpADC) {
            Opcode = 0x65,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpROR_DP = new OpDef(OpROR) {
            Opcode = 0x66,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpADC_DPIndLong = new OpDef(OpADC) {
            Opcode = 0x67,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpPLA_StackPull = new OpDef(OpPLA) {
            Opcode = 0x68,
            AddrMode = AddressMode.StackPull,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpADC_ImmLongA = new OpDef(OpADC) {
            Opcode = 0x69,
            AddrMode = AddressMode.ImmLongA,
            CycDef = 2 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpADC_Imm = new OpDef(OpADC) {
            Opcode = 0x69,
            AddrMode = AddressMode.Imm,
            CycDef = 2 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpROR_Acc = new OpDef(OpROR) {
            Opcode = 0x6a,
            AddrMode = AddressMode.Acc,
            CycDef = 2
        };
        public static readonly OpDef OpRTL_StackRTL = new OpDef(OpRTL) {
            Opcode = 0x6b,
            AddrMode = AddressMode.StackRTL,
            CycDef = 6
        };
        public static readonly OpDef OpJMP_AbsInd = new OpDef(OpJMP) {
            Opcode = 0x6c,
            AddrMode = AddressMode.AbsInd,
            CycDef = 5 | (int)(CycleMod.OneIf65C02)
            // takes one extra cycle on CMOS 6502 if low byte is 0xff?? (it also has a bug)
        };
        public static readonly OpDef OpADC_Abs = new OpDef(OpADC) {
            Opcode = 0x6d,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpROR_Abs = new OpDef(OpROR) {
            Opcode = 0x6e,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpADC_AbsLong = new OpDef(OpADC) {
            Opcode = 0x6f,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpBVS_PCRel = new OpDef(OpBVS) {
            Opcode = 0x70,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpADC_DPIndIndexY = new OpDef(OpADC) {
            Opcode = 0x71,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero |
                CycleMod.OneIfIndexPage | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpADC_DPInd = new OpDef(OpADC) {
            Opcode = 0x72,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpADC_StackRelIndIndexY = new OpDef(OpADC) {
            Opcode = 0x73,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSTZ_DPIndexX = new OpDef(OpSTZ) {
            Opcode = 0x74,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpADC_DPIndexX = new OpDef(OpADC) {
            Opcode = 0x75,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpROR_DPIndexX = new OpDef(OpROR) {
            Opcode = 0x76,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpADC_DPIndIndexYLong = new OpDef(OpADC) {
            Opcode = 0x77,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSEI_Implied = new OpDef(OpSEI) {
            Opcode = 0x78,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpADC_AbsIndexY = new OpDef(OpADC) {
            Opcode = 0x79,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpPLY_StackPull = new OpDef(OpPLY) {
            Opcode = 0x7a,
            AddrMode = AddressMode.StackPull,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpTDC_Implied = new OpDef(OpTDC) {
            Opcode = 0x7b,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpJMP_AbsIndexXInd = new OpDef(OpJMP) {
            Opcode = 0x7c,
            AddrMode = AddressMode.AbsIndexXInd,
            CycDef = 6
        };
        public static readonly OpDef OpADC_AbsIndexX = new OpDef(OpADC) {
            Opcode = 0x7d,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpROR_AbsIndexX = new OpDef(OpROR) {
            Opcode = 0x7e,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7 | (int)(CycleMod.TwoIfM0 | CycleMod.MinusOneIfNoPage)
        };
        public static readonly OpDef OpADC_AbsIndexXLong = new OpDef(OpADC) {
            Opcode = 0x7f,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpBRA_PCRel = new OpDef(OpBRA) {
            Opcode = 0x80,
            AddrMode = AddressMode.PCRel,
            CycDef = 3 | (int)(CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpSTA_DPIndexXInd = new OpDef(OpSTA) {
            Opcode = 0x81,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpBRL_PCRelLong = new OpDef(OpBRL) {
            Opcode = 0x82,
            AddrMode = AddressMode.PCRelLong,
            CycDef = 4
        };
        public static readonly OpDef OpSTA_StackRel = new OpDef(OpSTA) {
            Opcode = 0x83,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpSTY_DP = new OpDef(OpSTY) {
            Opcode = 0x84,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTA_DP = new OpDef(OpSTA) {
            Opcode = 0x85,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpSTX_DP = new OpDef(OpSTX) {
            Opcode = 0x86,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTA_DPIndLong = new OpDef(OpSTA) {
            Opcode = 0x87,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpDEY_Implied = new OpDef(OpDEY) {
            Opcode = 0x88,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpBIT_ImmLongA = new OpDef(OpBIT) {
            Opcode = 0x89,
            AddrMode = AddressMode.ImmLongA,
            FlagsAffected = FlagsAffected_Z,        // special case
            StatusFlagUpdater = FlagUpdater_Z,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBIT_Imm = new OpDef(OpBIT) {
            Opcode = 0x89,
            AddrMode = AddressMode.Imm,
            FlagsAffected = FlagsAffected_Z,        // special case
            StatusFlagUpdater = FlagUpdater_Z,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpTXA_Implied = new OpDef(OpTXA) {
            Opcode = 0x8a,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpPHB_StackPush = new OpDef(OpPHB) {
            Opcode = 0x8b,
            AddrMode = AddressMode.StackPush,
            CycDef = 3
        };
        public static readonly OpDef OpSTY_Abs = new OpDef(OpSTY) {
            Opcode = 0x8c,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTA_Abs = new OpDef(OpSTA) {
            Opcode = 0x8d,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpSTX_Abs = new OpDef(OpSTX) {
            Opcode = 0x8e,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTA_AbsLong = new OpDef(OpSTA) {
            Opcode = 0x8f,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBCC_PCRel = new OpDef(OpBCC) {
            Opcode = 0x90,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpSTA_DPIndIndexY = new OpDef(OpSTA) {
            Opcode = 0x91,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpSTA_DPInd = new OpDef(OpSTA) {
            Opcode = 0x92,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpSTA_StackRelIndIndexY = new OpDef(OpSTA) {
            Opcode = 0x93,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpSTY_DPIndexX = new OpDef(OpSTY) {
            Opcode = 0x94,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTA_DPIndexX = new OpDef(OpSTA) {
            Opcode = 0x95,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpSTX_DPIndexY = new OpDef(OpSTX) {
            Opcode = 0x96,
            AddrMode = AddressMode.DPIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTA_DPIndIndexYLong = new OpDef(OpSTA) {
            Opcode = 0x97,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpTYA_Implied = new OpDef(OpTYA) {
            Opcode = 0x98,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpSTA_AbsIndexY = new OpDef(OpSTA) {
            Opcode = 0x99,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpTXS_Implied = new OpDef(OpTXS) {
            Opcode = 0x9a,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpTXY_Implied = new OpDef(OpTXY) {
            Opcode = 0x9b,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpSTZ_Abs = new OpDef(OpSTZ) {
            Opcode = 0x9c,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpSTA_AbsIndexX = new OpDef(OpSTA) {
            Opcode = 0x9d,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpSTZ_AbsIndexX = new OpDef(OpSTZ) {
            Opcode = 0x9e,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpSTA_AbsIndexXLong = new OpDef(OpSTA) {
            Opcode = 0x9f,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLDY_ImmLongXY = new OpDef(OpLDY) {
            Opcode = 0xa0,
            AddrMode = AddressMode.ImmLongXY,
            StatusFlagUpdater = FlagUpdater_LoadImm,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDY_Imm = new OpDef(OpLDY) {
            Opcode = 0xa0,
            AddrMode = AddressMode.Imm,
            StatusFlagUpdater = FlagUpdater_LoadImm,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_DPIndexXInd = new OpDef(OpLDA) {
            Opcode = 0xa1,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpLDX_ImmLongXY = new OpDef(OpLDX) {
            Opcode = 0xa2,
            AddrMode = AddressMode.ImmLongXY,
            StatusFlagUpdater = FlagUpdater_LoadImm,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDX_Imm = new OpDef(OpLDX) {
            Opcode = 0xa2,
            AddrMode = AddressMode.Imm,
            StatusFlagUpdater = FlagUpdater_LoadImm,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_StackRel = new OpDef(OpLDA) {
            Opcode = 0xa3,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLDY_DP = new OpDef(OpLDY) {
            Opcode = 0xa4,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_DP = new OpDef(OpLDA) {
            Opcode = 0xa5,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpLDX_DP = new OpDef(OpLDX) {
            Opcode = 0xa6,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_DPIndLong = new OpDef(OpLDA) {
            Opcode = 0xa7,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpTAY_Implied = new OpDef(OpTAY) {
            Opcode = 0xa8,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpLDA_ImmLongA = new OpDef(OpLDA) {    // 16-bit CPU
            Opcode = 0xa9,
            AddrMode = AddressMode.ImmLongA,
            StatusFlagUpdater = FlagUpdater_LoadImm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLDA_Imm = new OpDef(OpLDA) {         // 8-bit CPU
            Opcode = 0xa9,
            AddrMode = AddressMode.Imm,
            StatusFlagUpdater = FlagUpdater_LoadImm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpTAX_Implied = new OpDef(OpTAX) {
            Opcode = 0xaa,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpPLB_StackPull = new OpDef(OpPLB) {
            Opcode = 0xab,
            AddrMode = AddressMode.StackPull,
            CycDef = 4
        };
        public static readonly OpDef OpLDY_Abs = new OpDef(OpLDY) {
            Opcode = 0xac,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_Abs = new OpDef(OpLDA) {
            Opcode = 0xad,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLDX_Abs = new OpDef(OpLDX) {
            Opcode = 0xae,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_AbsLong = new OpDef(OpLDA) {
            Opcode = 0xaf,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBCS_PCRel = new OpDef(OpBCS) {
            Opcode = 0xb0,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpLDA_DPIndIndexY = new OpDef(OpLDA) {
            Opcode = 0xb1,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpLDA_DPInd = new OpDef(OpLDA) {
            Opcode = 0xb2,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpLDA_StackRelIndIndexY = new OpDef(OpLDA) {
            Opcode = 0xb3,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpLDY_DPIndexX = new OpDef(OpLDY) {
            Opcode = 0xb4,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_DPIndexX = new OpDef(OpLDA) {
            Opcode = 0xb5,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpLDX_DPIndexY = new OpDef(OpLDX) {
            Opcode = 0xb6,
            AddrMode = AddressMode.DPIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_DPIndIndexYLong = new OpDef(OpLDA) {
            Opcode = 0xb7,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCLV_Implied = new OpDef(OpCLV) {
            Opcode = 0xb8,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpLDA_AbsIndexY = new OpDef(OpLDA) {
            Opcode = 0xb9,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpTSX_Implied = new OpDef(OpTSX) {
            Opcode = 0xba,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpTYX_Implied = new OpDef(OpTYX) {
            Opcode = 0xbb,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpLDY_AbsIndexX = new OpDef(OpLDY) {
            Opcode = 0xbc,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfIndexPage | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_AbsIndexX = new OpDef(OpLDA) {
            Opcode = 0xbd,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpLDX_AbsIndexY = new OpDef(OpLDX) {
            Opcode = 0xbe,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfIndexPage | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpLDA_AbsIndexXLong = new OpDef(OpLDA) {
            Opcode = 0xbf,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpCPY_ImmLongXY = new OpDef(OpCPY) {
            Opcode = 0xc0,
            AddrMode = AddressMode.ImmLongXY,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpCPY_Imm = new OpDef(OpCPY) {
            Opcode = 0xc0,
            AddrMode = AddressMode.Imm,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpCMP_DPIndexXInd = new OpDef(OpCMP) {
            Opcode = 0xc1,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpREP_Imm = new OpDef(OpREP) {
            Opcode = 0xc2,
            AddrMode = AddressMode.Imm,
            CycDef = 3
        };
        public static readonly OpDef OpCMP_StackRel = new OpDef(OpCMP) {
            Opcode = 0xc3,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpCPY_DP = new OpDef(OpCPY) {
            Opcode = 0xc4,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpCMP_DP = new OpDef(OpCMP) {
            Opcode = 0xc5,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpDEC_DP = new OpDef(OpDEC) {
            Opcode = 0xc6,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpCMP_DPIndLong = new OpDef(OpCMP) {
            Opcode = 0xc7,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpINY_Implied = new OpDef(OpINY) {
            Opcode = 0xc8,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpCMP_ImmLongA = new OpDef(OpCMP) {
            Opcode = 0xc9,
            AddrMode = AddressMode.ImmLongA,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpCMP_Imm = new OpDef(OpCMP) {
            Opcode = 0xc9,
            AddrMode = AddressMode.Imm,
            CycDef = 2 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpDEX_Implied = new OpDef(OpDEX) {
            Opcode = 0xca,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpWAI_Implied = new OpDef(OpWAI) {
            Opcode = 0xcb,
            AddrMode = AddressMode.Implied,
            CycDef = 3      // 3 to shut down, more to restart
        };
        public static readonly OpDef OpCPY_Abs = new OpDef(OpCPY) {
            Opcode = 0xcc,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpCMP_Abs = new OpDef(OpCMP) {
            Opcode = 0xcd,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpDEC_Abs = new OpDef(OpDEC) {
            Opcode = 0xce,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpCMP_AbsLong = new OpDef(OpCMP) {
            Opcode = 0xcf,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpBNE_PCRel = new OpDef(OpBNE) {
            Opcode = 0xd0,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpCMP_DPIndIndexY = new OpDef(OpCMP) {
            Opcode = 0xd1,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpCMP_DPInd = new OpDef(OpCMP) {
            Opcode = 0xd2,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCMP_StackRelIndIndexY = new OpDef(OpCMP) {
            Opcode = 0xd3,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpPEI_StackDPInd = new OpDef(OpPEI) {
            Opcode = 0xd4,
            AddrMode = AddressMode.StackDPInd,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCMP_DPIndexX = new OpDef(OpCMP) {
            Opcode = 0xd5,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpDEC_DPIndexX = new OpDef(OpDEC) {
            Opcode = 0xd6,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpCMP_DPIndIndexYLong = new OpDef(OpCMP) {
            Opcode = 0xd7,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero)
        };
        public static readonly OpDef OpCLD_Implied = new OpDef(OpCLD) {
            Opcode = 0xd8,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpCMP_AbsIndexY = new OpDef(OpCMP) {
            Opcode = 0xd9,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpPHX_StackPush = new OpDef(OpPHX) {
            Opcode = 0xda,
            AddrMode = AddressMode.StackPush,
            CycDef = 3 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSTP_Implied = new OpDef(OpSTP) {
            Opcode = 0xdb,
            AddrMode = AddressMode.Implied,
            CycDef = 3      // 3 to shut down, more to reset out
        };
        public static readonly OpDef OpJMP_AbsIndLong = new OpDef(OpJML) {
            Opcode = 0xdc,
            AddrMode = AddressMode.AbsIndLong,
            CycDef = 6
        };
        public static readonly OpDef OpCMP_AbsIndexX = new OpDef(OpCMP) {
            Opcode = 0xdd,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpDEC_AbsIndexX = new OpDef(OpDEC) {
            Opcode = 0xde,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7 | (int)(CycleMod.TwoIfM0 | CycleMod.MinusOneIfNoPage)
        };
        public static readonly OpDef OpCMP_AbsIndexXLong = new OpDef(OpCMP) {
            Opcode = 0xdf,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0)
        };
        public static readonly OpDef OpCPX_ImmLongXY = new OpDef(OpCPX) {
            Opcode = 0xe0,
            AddrMode = AddressMode.ImmLongXY,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpCPX_Imm = new OpDef(OpCPX) {
            Opcode = 0xe0,
            AddrMode = AddressMode.Imm,
            CycDef = 2 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSBC_DPIndexXInd = new OpDef(OpSBC) {
            Opcode = 0xe1,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSEP_Imm = new OpDef(OpSEP) {
            Opcode = 0xe2,
            AddrMode = AddressMode.Imm,
            CycDef = 3
        };
        public static readonly OpDef OpSBC_StackRel = new OpDef(OpSBC) {
            Opcode = 0xe3,
            AddrMode = AddressMode.StackRel,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpCPX_DP = new OpDef(OpCPX) {
            Opcode = 0xe4,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfDpNonzero | CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSBC_DP = new OpDef(OpSBC) {
            Opcode = 0xe5,
            AddrMode = AddressMode.DP,
            CycDef = 3 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpINC_DP = new OpDef(OpINC) {
            Opcode = 0xe6,
            AddrMode = AddressMode.DP,
            CycDef = 5 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpSBC_DPIndLong = new OpDef(OpSBC) {
            Opcode = 0xe7,
            AddrMode = AddressMode.DPIndLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpINX_Implied = new OpDef(OpINX) {
            Opcode = 0xe8,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpSBC_ImmLongA = new OpDef(OpSBC) {
            Opcode = 0xe9,
            AddrMode = AddressMode.ImmLongA,
            CycDef = 2 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSBC_Imm = new OpDef(OpSBC) {
            Opcode = 0xe9,
            AddrMode = AddressMode.Imm,
            CycDef = 2 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpNOP_Implied = new OpDef(OpNOP) {
            Opcode = 0xea,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpXBA_Implied = new OpDef(OpXBA) {
            Opcode = 0xeb,
            AddrMode = AddressMode.Implied,
            CycDef = 3
        };
        public static readonly OpDef OpCPX_Abs = new OpDef(OpCPX) {
            Opcode = 0xec,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpSBC_Abs = new OpDef(OpSBC) {
            Opcode = 0xed,
            AddrMode = AddressMode.Abs,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpINC_Abs = new OpDef(OpINC) {
            Opcode = 0xee,
            AddrMode = AddressMode.Abs,
            CycDef = 6 | (int)(CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpSBC_AbsLong = new OpDef(OpSBC) {
            Opcode = 0xef,
            AddrMode = AddressMode.AbsLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpBEQ_PCRel = new OpDef(OpBEQ) {
            Opcode = 0xf0,
            AddrMode = AddressMode.PCRel,
            CycDef = 2 | (int)(CycleMod.OneIfBranchTaken | CycleMod.OneIfBranchPage)
        };
        public static readonly OpDef OpSBC_DPIndIndexY = new OpDef(OpSBC) {
            Opcode = 0xf1,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero |
                CycleMod.OneIfIndexPage | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSBC_DPInd = new OpDef(OpSBC) {
            Opcode = 0xf2,
            AddrMode = AddressMode.DPInd,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSBC_StackRelIndIndexY = new OpDef(OpSBC) {
            Opcode = 0xf3,
            AddrMode = AddressMode.StackRelIndIndexY,
            CycDef = 7 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpPEA_StackAbs = new OpDef(OpPEA) {
            Opcode = 0xf4,
            AddrMode = AddressMode.StackAbs,
            CycDef = 5
        };
        public static readonly OpDef OpSBC_DPIndexX = new OpDef(OpSBC) {
            Opcode = 0xf5,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpINC_DPIndexX = new OpDef(OpINC) {
            Opcode = 0xf6,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6 | (int)(CycleMod.OneIfDpNonzero | CycleMod.TwoIfM0)
        };
        public static readonly OpDef OpSBC_DPIndIndexYLong = new OpDef(OpSBC) {
            Opcode = 0xf7,
            AddrMode = AddressMode.DPIndIndexYLong,
            CycDef = 6 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfDpNonzero | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpSED_Implied = new OpDef(OpSED) {
            Opcode = 0xf8,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpSBC_AbsIndexY = new OpDef(OpSBC) {
            Opcode = 0xf9,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpPLX_StackPull = new OpDef(OpPLX) {
            Opcode = 0xfa,
            AddrMode = AddressMode.StackPull,
            CycDef = 4 | (int)(CycleMod.OneIfX0)
        };
        public static readonly OpDef OpXCE_Implied = new OpDef(OpXCE) {
            Opcode = 0xfb,
            AddrMode = AddressMode.Implied,
            CycDef = 2
        };
        public static readonly OpDef OpJSR_AbsIndexXInd = new OpDef(OpJSR) {
            Opcode = 0xfc,
            AddrMode = AddressMode.AbsIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpSBC_AbsIndexX = new OpDef(OpSBC) {
            Opcode = 0xfd,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfIndexPage | CycleMod.OneIfD1)
        };
        public static readonly OpDef OpINC_AbsIndexX = new OpDef(OpINC) {
            Opcode = 0xfe,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7 | (int)(CycleMod.TwoIfM0 | CycleMod.MinusOneIfNoPage)
        };
        public static readonly OpDef OpSBC_AbsIndexXLong = new OpDef(OpSBC) {
            Opcode = 0xff,
            AddrMode = AddressMode.AbsIndexXLong,
            CycDef = 5 | (int)(CycleMod.OneIfM0 | CycleMod.OneIfD1)
        };

        #endregion 65816 Instructions

        #region Undocumented 6502

        // ======================================================================================
        // Undocumented 6502 instructions.
        //
        // There are 151 defined opcodes.  The rest officially have undefined behavior.  In
        // most cases it's pretty stable, in others the behavior can differ between CPU
        // variants.
        //
        // There is no universally agreed-upon set of mnemonics for these instructions.  The
        // mnemonics "XAS" and "AXS" sometimes mean one thing and sometimes another.  The
        // reference I've chosen to use as primary is "NMOS 6510 Unintended Opcodes":
        //   https://csdb.dk/release/?id=161035
        //
        // Other references:
        //   http://nesdev.com/undocumented_opcodes.txt
        //   http://visual6502.org/wiki/index.php?title=6502_all_256_Opcodes
        //   http://www.ffd2.com/fridge/docs/6502-NMOS.extra.opcodes
        //

        private static OpDef OpANC = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.ANC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpANE = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.ANE,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpALR = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.ALR,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpARR = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.ARR,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NVZC,
            StatusFlagUpdater = FlagUpdater_NVZC
        };
        private static OpDef OpDCP = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.DCP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_C,
            StatusFlagUpdater = FlagUpdater_C
        };
        private static OpDef OpDOP = new OpDef() {      // double-byte NOP
            IsUndocumented = true,
            Mnemonic = OpName.DOP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpJAM = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.JAM,
            Effect = FlowEffect.NoCont,
            BaseMemEffect = MemoryEffect.None
        };
        private static OpDef OpISC = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.ISC,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NVZC,
            StatusFlagUpdater = FlagUpdater_NVZC
        };
        private static OpDef OpLAS = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.LAS,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpLAX = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.LAX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Read,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpRLA = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.RLA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpRRA = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.RRA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NVZC,
            StatusFlagUpdater = FlagUpdater_NVZC
        };
        private static OpDef OpSAX = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SAX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write,
            FlagsAffected = FlagsAffected_NZ,
            StatusFlagUpdater = FlagUpdater_NZ
        };
        private static OpDef OpSBX = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SBX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpSHA = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SHA,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write
        };
        private static OpDef OpSHX = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SHX,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write
        };
        private static OpDef OpSHY = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SHY,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write
        };
        private static OpDef OpSLO = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SLO,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpSRE = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.SRE,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.ReadModifyWrite,
            FlagsAffected = FlagsAffected_NZC,
            StatusFlagUpdater = FlagUpdater_NZC
        };
        private static OpDef OpTAS = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.TAS,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.Write
        };
        private static OpDef OpTOP = new OpDef() {      // triple-byte NOP
            IsUndocumented = true,
            Mnemonic = OpName.TOP,
            Effect = FlowEffect.Cont,
            BaseMemEffect = MemoryEffect.None
        };

        public static readonly OpDef OpSLO_DPIndexXInd = new OpDef(OpSLO) {
            Opcode = 0x03,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpSLO_DP = new OpDef(OpSLO) {
            Opcode = 0x07,
            AddrMode = AddressMode.DP,
            CycDef = 5
        };
        public static readonly OpDef OpSLO_Absolute = new OpDef(OpSLO) {
            Opcode = 0x0f,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpSLO_DPIndIndexY = new OpDef(OpSLO) {
            Opcode = 0x13,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 8
        };
        public static readonly OpDef OpSLO_DPIndexX = new OpDef(OpSLO) {
            Opcode = 0x17,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6
        };
        public static readonly OpDef OpSLO_AbsIndexY = new OpDef(OpSLO) {
            Opcode = 0x1b,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 7
        };
        public static readonly OpDef OpSLO_AbsIndexX = new OpDef(OpSLO) {
            Opcode = 0x1f,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7
        };
        public static readonly OpDef OpRLA_DPIndexXInd = new OpDef(OpRLA) {
            Opcode = 0x23,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpRLA_DP = new OpDef(OpRLA) {
            Opcode = 0x27,
            AddrMode = AddressMode.DP,
            CycDef = 5
        };
        public static readonly OpDef OpRLA_Absolute = new OpDef(OpRLA) {
            Opcode = 0x2f,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpRLA_DPIndIndexY = new OpDef(OpRLA) {
            Opcode = 0x33,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 8
        };
        public static readonly OpDef OpRLA_DPIndexX = new OpDef(OpRLA) {
            Opcode = 0x37,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6
        };
        public static readonly OpDef OpRLA_AbsIndexY = new OpDef(OpRLA) {
            Opcode = 0x3b,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 7
        };
        public static readonly OpDef OpRLA_AbsIndexX = new OpDef(OpRLA) {
            Opcode = 0x3f,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7
        };
        public static readonly OpDef OpSRE_DPIndexXInd = new OpDef(OpSRE) {
            Opcode = 0x43,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpSRE_DP = new OpDef(OpSRE) {
            Opcode = 0x47,
            AddrMode = AddressMode.DP,
            CycDef = 5
        };
        public static readonly OpDef OpSRE_Absolute = new OpDef(OpSRE) {
            Opcode = 0x4f,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpSRE_DPIndIndexY = new OpDef(OpSRE) {
            Opcode = 0x53,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 8
        };
        public static readonly OpDef OpSRE_DPIndexX = new OpDef(OpSRE) {
            Opcode = 0x57,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6
        };
        public static readonly OpDef OpSRE_AbsIndexY = new OpDef(OpSRE) {
            Opcode = 0x5b,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 7
        };
        public static readonly OpDef OpSRE_AbsIndexX = new OpDef(OpSRE) {
            Opcode = 0x5f,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7
        };
        public static readonly OpDef OpRRA_DPIndexXInd = new OpDef(OpRRA) {
            Opcode = 0x63,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpRRA_DP = new OpDef(OpRRA) {
            Opcode = 0x67,
            AddrMode = AddressMode.DP,
            CycDef = 5
        };
        public static readonly OpDef OpRRA_Absolute = new OpDef(OpRRA) {
            Opcode = 0x6f,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpRRA_DPIndIndexY = new OpDef(OpRRA) {
            Opcode = 0x73,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 8
        };
        public static readonly OpDef OpRRA_DPIndexX = new OpDef(OpRRA) {
            Opcode = 0x77,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6
        };
        public static readonly OpDef OpRRA_AbsIndexY = new OpDef(OpRRA) {
            Opcode = 0x7b,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 7
        };
        public static readonly OpDef OpRRA_AbsIndexX = new OpDef(OpRRA) {
            Opcode = 0x7f,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7
        };
        public static readonly OpDef OpSAX_DPIndexXInd = new OpDef(OpSAX) {
            Opcode = 0x83,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6
        };
        public static readonly OpDef OpSAX_DP = new OpDef(OpSAX) {
            Opcode = 0x87,
            AddrMode = AddressMode.DP,
            CycDef = 3
        };
        public static readonly OpDef OpSAX_Absolute = new OpDef(OpSAX) {
            Opcode = 0x8f,
            AddrMode = AddressMode.Abs,
            CycDef = 4
        };
        public static readonly OpDef OpSAX_DPIndexY = new OpDef(OpSAX) {
            Opcode = 0x97,
            AddrMode = AddressMode.DPIndexY,
            CycDef = 4
        };
        public static readonly OpDef OpLAX_DPIndexXInd = new OpDef(OpLAX) {
            Opcode = 0xa3,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 6
        };
        public static readonly OpDef OpLAX_DP = new OpDef(OpLAX) {
            Opcode = 0xa7,
            AddrMode = AddressMode.DP,
            CycDef = 3
        };
        public static readonly OpDef OpLAX_Absolute = new OpDef(OpLAX) {
            Opcode = 0xaf,
            AddrMode = AddressMode.Abs,
            CycDef = 4
        };
        public static readonly OpDef OpLAX_DPIndIndexY = new OpDef(OpLAX) {
            Opcode = 0xb3,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 5 | (int)(CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpLAX_DPIndexY = new OpDef(OpLAX) {
            Opcode = 0xb7,
            AddrMode = AddressMode.DPIndexY,
            CycDef = 4
        };
        public static readonly OpDef OpLAX_AbsIndexY = new OpDef(OpLAX) {
            Opcode = 0xbf,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpDCP_DPIndexXInd = new OpDef(OpDCP) {
            Opcode = 0xc3,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpDCP_DP = new OpDef(OpDCP) {
            Opcode = 0xc7,
            AddrMode = AddressMode.DP,
            CycDef = 5
        };
        public static readonly OpDef OpDCP_Abs = new OpDef(OpDCP) {
            Opcode = 0xcf,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpDCP_DPIndIndexY = new OpDef(OpDCP) {
            Opcode = 0xd3,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 8
        };
        public static readonly OpDef OpDCP_DPIndexX = new OpDef(OpDCP) {
            Opcode = 0xd7,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6
        };
        public static readonly OpDef OpDCP_AbsIndexY = new OpDef(OpDCP) {
            Opcode = 0xdb,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 7
        };
        public static readonly OpDef OpDCP_AbsIndexX = new OpDef(OpDCP) {
            Opcode = 0xdf,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7
        };
        public static readonly OpDef OpISC_DPIndexXInd = new OpDef(OpISC) {
            Opcode = 0xe3,
            AddrMode = AddressMode.DPIndexXInd,
            CycDef = 8
        };
        public static readonly OpDef OpISC_DP = new OpDef(OpISC) {
            Opcode = 0xe7,
            AddrMode = AddressMode.DP,
            CycDef = 5
        };
        public static readonly OpDef OpISC_Abs = new OpDef(OpISC) {
            Opcode = 0xef,
            AddrMode = AddressMode.Abs,
            CycDef = 6
        };
        public static readonly OpDef OpISC_DPIndIndexY = new OpDef(OpISC) {
            Opcode = 0xf3,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 8
        };
        public static readonly OpDef OpISC_DPIndexX = new OpDef(OpISC) {
            Opcode = 0xf7,
            AddrMode = AddressMode.DPIndexX,
            CycDef = 6
        };
        public static readonly OpDef OpISC_AbsIndexY = new OpDef(OpISC) {
            Opcode = 0xfb,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 7
        };
        public static readonly OpDef OpISC_AbsIndexX = new OpDef(OpISC) {
            Opcode = 0xff,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 7
        };
        public static readonly OpDef OpALR_Imm = new OpDef(OpALR) {
            Opcode = 0x4b,
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpARR_Imm = new OpDef(OpARR) {
            Opcode = 0x6b,
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpANE_Imm = new OpDef(OpANE) {
            Opcode = 0x8b,
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpLAX_Imm = new OpDef(OpLAX) {
            Opcode = 0xab,
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpSBX_Imm = new OpDef(OpSBX) {
            Opcode = 0xcb,
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpDOP_Imm = new OpDef(OpDOP) {
            // multiple opcodes
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpDOP_DP = new OpDef(OpDOP) {
            // multiple opcodes
            AddrMode = AddressMode.DP,
            CycDef = 3
        };
        public static readonly OpDef OpDOP_DPIndexX = new OpDef(OpDOP) {
            // multiple opcodes
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4
        };
        public static readonly OpDef OpTOP_Abs = new OpDef(OpTOP) {
            Opcode = 0x0c,
            AddrMode = AddressMode.Abs,
            CycDef = 4
        };
        public static readonly OpDef OpTOP_AbsIndeX = new OpDef(OpTOP) {
            // multiple opcodes
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 4 | (int)(CycleMod.OneIfIndexPage)
        };
        public static readonly OpDef OpJAM_Implied = new OpDef(OpJAM) {
            // multiple opcodes
            AddrMode = AddressMode.Implied,
            CycDef = 1
        };
        public static readonly OpDef OpTAS_AbsIndexY  = new OpDef(OpTAS) {
            Opcode = 0x9b,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 5
        };
        public static readonly OpDef OpSHY_AbsIndexX = new OpDef(OpSHY) {
            Opcode = 0x9c,
            AddrMode = AddressMode.AbsIndexX,
            CycDef = 5
        };
        public static readonly OpDef OpSHX_AbsIndexY = new OpDef(OpSHX) {
            Opcode = 0x9e,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 5
        };
        public static readonly OpDef OpSHA_DPIndIndexY = new OpDef(OpSHA) {
            Opcode = 0x93,
            AddrMode = AddressMode.DPIndIndexY,
            CycDef = 6
        };
        public static readonly OpDef OpSHA_AbsIndexY = new OpDef(OpSHA) {
            Opcode = 0x9f,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 5
        };
        public static readonly OpDef OpANC_Imm = new OpDef(OpANC) {
            // multiple opcodes
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpLAS_AbsIndexY = new OpDef(OpLAS) {
            Opcode = 0xbb,
            AddrMode = AddressMode.AbsIndexY,
            CycDef = 4 | (int)(CycleMod.OneIfIndexPage)
        };

        #endregion Undocumented 6502

        #region Undocumented 65C02

        // ======================================================================================
        // Undocumented 65C02 instructions.
        //
        // The 65C02 declared all undefined opcodes to be NOPs, but some of them can have
        // side effects.
        //
        // References:
        //   http://laughtonelectronics.com/Arcana/KimKlone/Kimklone_opcode_mapping.html
        //

        // Most undocumented instructions are a single-byte, single-cycle NOP.  I don't know
        // if these are "undocumented" in the strictest sense of the word, but we'll treat
        // them that way for disassembly purposes.
        public static readonly OpDef OpNOP_65C02 = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.NOP,
            Effect = FlowEffect.Cont,
            AddrMode = AddressMode.Implied,
            CycDef = 1
        };

        // 65C02 undocumented "Load and Discard" instruction.  These cause bus traffic.
        private static OpDef OpLDD = new OpDef() {
            IsUndocumented = true,
            Mnemonic = OpName.LDD,
            Effect = FlowEffect.Cont,
            AddrMode = AddressMode.Implied
        };
        public static readonly OpDef OpLDD_Absolute = new OpDef(OpLDD) {
            // multiple opcodes
            AddrMode = AddressMode.Abs,
            CycDef = 4
        };
        public static readonly OpDef OpLDD_DP = new OpDef(OpLDD) {
            // multiple opcodes
            AddrMode = AddressMode.DP,
            CycDef = 3
        };
        public static readonly OpDef OpLDD_DPIndexX = new OpDef(OpLDD) {
            // multiple opcodes
            AddrMode = AddressMode.DPIndexX,
            CycDef = 4
        };
        public static readonly OpDef OpLDD_Imm = new OpDef(OpLDD) {
            // multiple opcodes
            AddrMode = AddressMode.Imm,
            CycDef = 2
        };
        public static readonly OpDef OpLDD_Weird = new OpDef(OpLDD) {
            // multiple opcodes
            AddrMode = AddressMode.Abs,  // not really, but has the right number of bytes
            CycDef = 8
        };

        #endregion Undocumented


        /// <summary>
        /// Generates one of the multiply-defined opcodes from a prototype.  This is
        /// particularly useful for undocumented opcodes that are repeated several times.  In
        /// a couple of cases (SBC, some NOPs), the undocumented opcode is just another
        /// reference to an existing instruction.
        /// </summary>
        /// <param name="opcode">Instruction opcode.</param>
        /// <param name="proto">Instruction prototype.</param>
        /// <returns>Newly-created OpDef.</returns>
        public static OpDef GenerateUndoc(byte opcode, OpDef proto) {
            return new OpDef(proto) { Opcode = opcode, IsUndocumented = true };
        }

        public override string ToString() {
            return Opcode.ToString("x2") + "/" + Mnemonic + " " + AddrMode;
        }
    }
}
