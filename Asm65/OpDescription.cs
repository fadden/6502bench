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
using System.Text;

namespace Asm65 {
    /// <summary>
    /// Human-readable text describing instructions.
    /// 
    /// The expectation is that the long description will include information about all
    /// address modes and any differences in behavior between CPUs.  So there will be one
    /// entry per instruction mnemonic, and one global table for all CPUs, rather than one
    /// entry per opcode and one instance per CpuDef.
    /// 
    /// There may, however, be different instances for different Cultures.  Also, the 65816
    /// traditionally splits JSR/JSL and JMP/JML, so in that case there will be two entries
    /// for the same instruction.
    /// </summary>
    public class OpDescription {
        private Dictionary<string, string> mShortDescriptions;

        private Dictionary<string, string> mLongDescriptions;

        private Dictionary<OpDef.AddressMode, string> mAddressModeDescriptions;

        private Dictionary<OpDef.CycleMod, string> mCycleModDescriptions;

        private OpDescription(Dictionary<string, string> sd, Dictionary<string, string> ld,
                Dictionary<OpDef.AddressMode, string> am, Dictionary<OpDef.CycleMod, string> cm) {
            mShortDescriptions = sd;
            mLongDescriptions = ld;
            mAddressModeDescriptions = am;
            mCycleModDescriptions = cm;
        }

        /// <summary>
        /// Returns an OpDescription instance for the requested region.
        /// </summary>
        /// <param name="region">TBD</param>
        public static OpDescription GetOpDescription(string region) {
            // ignoring region for now
            return new OpDescription(sShort_enUS, sLong_enUS, sAddrMode_enUS, sCycleMod_enUS);
        }

        /// <summary>
        /// Short description of instruction, e.g. "Load Accumulator".
        /// </summary>
        /// <param name="mnemonic">Instruction mnemonic.</param>
        /// <returns>Short description string, or empty string if not found.</returns>
        public string GetShortDescription(string mnemonic) {
            if (mShortDescriptions.TryGetValue(mnemonic, out string desc)) {
                return desc;
            } else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Long description of instruction.  May span multiple lines, with embedded CRLF at
        /// paragraph breaks.
        /// </summary>
        /// <param name="mnemonic">Instruction mnemonic.</param>
        /// <returns>Long description string, or empty string if not found.</returns>
        public string GetLongDescription(string mnemonic) {
            if (mLongDescriptions.TryGetValue(mnemonic, out string desc)) {
                return desc;
            } else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Address mode short description.
        /// </summary>
        /// <param name="addrMode">Address mode to look up.</param>
        /// <returns>Description string, or an empty string for instructions
        ///   with implied address modes.</returns>
        public string GetAddressModeDescription(OpDef.AddressMode addrMode) {
            if (mAddressModeDescriptions.TryGetValue(addrMode, out string desc)) {
                return desc;
            } else {
                return string.Empty;
            }
        }

        /// <summary>
        /// Cycle modifier description.
        /// </summary>
        /// <param name="modBit">A single-bit item from the CycleMod enum.</param>
        /// <returns>Description string, or question marks if not found.</returns>
        public string GetCycleModDescription(OpDef.CycleMod modBit) {
            if (mCycleModDescriptions.TryGetValue(modBit, out string desc)) {
                return desc;
            } else {
                return "???";
            }
        }

        /// <summary>
        /// Short descriptions, USA English.
        /// 
        /// Text is adapted from instruction summaries in Eyes & Lichty, which are slightly
        /// shorter than those in the CPU data sheet.
        /// </summary>
        private static Dictionary<string, string> sShort_enUS = new Dictionary<string, string>() {
            { OpName.ADC, "Add With Carry" },
            { OpName.AND, "AND Accumulator With Memory" },
            { OpName.ASL, "Shift Memory or Accumulator Left" },
            { OpName.BCC, "Branch If Carry Clear" },
            { OpName.BCS, "Branch If Carry Set" },
            { OpName.BEQ, "Branch If Equal" },
            { OpName.BIT, "Test Memory Against Accumulator" },
            { OpName.BMI, "Branch If Minus" },
            { OpName.BNE, "Branch If Not Equal" },
            { OpName.BPL, "Branch If Plus" },
            { OpName.BRA, "Branch Always" },
            { OpName.BRK, "Software Break" },
            { OpName.BRL, "Branch Always Long" },
            { OpName.BVC, "Branch If Overflow Clear" },
            { OpName.BVS, "Branch If Overflow Set" },
            { OpName.CLC, "Clear Carry Flag" },
            { OpName.CLD, "Clear Decimal Flag" },
            { OpName.CLI, "Clear Interrupt Disable Flag" },
            { OpName.CLV, "Clear Overflow Flag" },
            { OpName.CMP, "Compare Accumulator With Memory" },
            { OpName.COP, "Co-Processor" },
            { OpName.CPX, "Compare Index X With Memory" },
            { OpName.CPY, "Compare Index Y With Memory" },
            { OpName.DEC, "Decrement Accumulator" },
            { OpName.DEX, "Decrement Index X" },
            { OpName.DEY, "Decrement Index Y" },
            { OpName.EOR, "XOR Accumulator With Memory" },
            { OpName.INC, "Increment Accumulator" },
            { OpName.INX, "Increment Index X" },
            { OpName.INY, "Increment Index Y" },
            { OpName.JMP, "Jump" },
            { OpName.JSL, "Jump to Subroutine Long" },
            { OpName.JSR, "Jump to Subroutine" },
            { OpName.LDA, "Load Accumulator from Memory" },
            { OpName.LDX, "Load Index X from Memory" },
            { OpName.LDY, "Load Index Y from Memory" },
            { OpName.LSR, "Logical Shift Memory or Accumulator Right" },
            { OpName.MVN, "Block Move Next" },
            { OpName.MVP, "Block Move Previous" },
            { OpName.NOP, "No Operation" },
            { OpName.ORA, "OR Accumulator With Memory" },
            { OpName.PEA, "Push Effective Absolute Address" },
            { OpName.PEI, "Push Effective Indirect Address" },
            { OpName.PER, "Push Effective Relative Indirect Address" },
            { OpName.PHA, "Push Accumulator" },
            { OpName.PHB, "Push Data Bank Register" },
            { OpName.PHD, "Push Direct Page Register" },
            { OpName.PHK, "Push Program Bank Register" },
            { OpName.PHP, "Push Processor Status Register" },
            { OpName.PHX, "Push Index Register X" },
            { OpName.PHY, "Push Index Register Y" },
            { OpName.PLA, "Pull Accumulator" },
            { OpName.PLB, "Pull Data Bank Register" },
            { OpName.PLD, "Pull Direct Page Register" },
            { OpName.PLP, "Pull Processor Status Register" },
            { OpName.PLX, "Pull Index Register X" },
            { OpName.PLY, "Pull Index Register Y" },
            { OpName.REP, "Reset Status Bits" },
            { OpName.ROL, "Rotate Memory or Accumulator Left" },
            { OpName.ROR, "Rotate Memory or Accumulator Right" },
            { OpName.RTI, "Return from Interrupt" },
            { OpName.RTL, "Return from Subroutine Long" },
            { OpName.RTS, "Return from Subroutine" },
            { OpName.SBC, "Subtract With Borrow" },
            { OpName.SEC, "Set Carry Flag" },
            { OpName.SED, "Set Decimal Flag" },
            { OpName.SEI, "Set Interrupt Disable Flag" },
            { OpName.SEP, "Set Status Bits" },
            { OpName.STA, "Store Accumulator to Memory" },
            { OpName.STP, "Stop Processor" },
            { OpName.STX, "Store Index X to Memory" },
            { OpName.STY, "Store Index Y to Memory" },
            { OpName.STZ, "Store Zero to Memory" },
            { OpName.TAX, "Transfer Accumulator to Index X" },
            { OpName.TAY, "Transfer Accumulator to Index Y" },
            { OpName.TCD, "Transfer 16-Bit Accumulator to Direct Page Register" },
            { OpName.TCS, "Transfer Accumulator to Stack Pointer" },
            { OpName.TDC, "Transfer Direct Page Register to 16-Bit Accumulator" },
            { OpName.TRB, "Test and Reset Memory Bits" },
            { OpName.TSB, "Test and Set Memory Bits" },
            { OpName.TSC, "Transfer Stack Pointer to 16-Bit Accumulator" },
            { OpName.TSX, "Transfer Stack Pointer to Index X" },
            { OpName.TXA, "Transfer Index X to Accumulator" },
            { OpName.TXS, "Transfer Index X to Stack Pointer" },
            { OpName.TXY, "Transfer Index X to Index Y" },
            { OpName.TYA, "Transfer Index Y to Accumulator" },
            { OpName.TYX, "Transfer Index Y to Index X" },
            { OpName.WAI, "Wait for Interrupt" },
            { OpName.WDM, "Future Expansion" },
            { OpName.XBA, "Exchange Accumulator B and A" },
            { OpName.XCE, "Exchange Carry and Emulation Bits" },

            // MOS 6502 undocumented ops
            { OpName.ANC, "AND Accumulator With Value and Set Carry" },
            { OpName.ANE, "Transfer Index X to Accumulator and AND" },
            { OpName.ARR, "AND and Rotate Right" },
            { OpName.ASR, "AND and Shift Right" },
            { OpName.DCP, "Decrement and Compare" },
            { OpName.DOP, "Double-Byte NOP" },
            { OpName.HLT, "Halt CPU" },
            { OpName.ISB, "Increment and Subtract" },
            { OpName.LAE, "Load Acc, X, and Stack Pointer with Memory AND Stack Pointer" },
            { OpName.LAX, "Load Accumulator and Index X" },
            { OpName.LXA, "OR, AND, and Transfer to X" },
            { OpName.RLA, "Rotate Left and AND" },
            { OpName.RRA, "Rotate Right and Add" },
            { OpName.SAX, "Store Accumulator AND Index X" },                            // AXS
            { OpName.SBX, "AND Acc With Index X, Subtract, and Store in X" },           // SAX
            { OpName.SHA, "AND Acc With Index X and High Byte, and Store" },            // AXA
            { OpName.SHS, "AND Acc with Index X, Transfer to Stack, AND High Byte" },   // TAS
            { OpName.SHX, "AND Acc With Index X and High Byte, and Store" },            // XAS
            { OpName.SHY, "AND Acc With Index Y and High Byte, and Store" },            // SAY
            { OpName.SLO, "Shift Left and OR" },
            { OpName.SRE, "Shift right and EOR" },
            { OpName.TOP, "Triple-Byte NOP" },

            // WDC 65C02 undocumented
            { OpName.LDD, "Load and Discard" },
        };

        /// <summary>
        /// Long descriptions, USA English.
        /// </summary>
        private static Dictionary<string, string> sLong_enUS = new Dictionary<string, string>() {
            { OpName.ADC,
                "Adds the accumulator and a value in memory, storing the result in the " +
                "accumulator.  Adds one if the carry is set."
            },
            { OpName.AND,
                "Performs a bitwise AND of the accumulator with a value in memory, storing " +
                "the result in the accumulator."
            },
            { OpName.ASL,
                "Shifts memory or the accumulator one bit left.  The low bit is set to zero, " +
                "and the carry flag receives the high bit."
            },
            { OpName.BCC,
                "Branches to a relative address if the processor carry flag (C) is zero.  " +
                "Sometimes referred to as Branch If Less Than, or BLT."
            },
            { OpName.BCS,
                "Branches to a relative address if the processor carry flag (C) is one.  " +
                "Sometimes referred to as Branch If Greater Than or Equal, or BGE."
            },
            { OpName.BEQ,
                "Branches to a relative address if the processor zero flag (Z) is one."
            },
            { OpName.BIT,
                "Sets processor flags based on the result of two operations.  The N and V flags " +
                "are set according to bits 7 and 6, and the Z flag is set based on an AND of " +
                "the accumulator and memory.  However, when used with immediate addressing, " +
                "the N and V flags are not affected."
            },
            { OpName.BMI,
                "Branches to a relative address if the processor negative flag (N) is one."
            },
            { OpName.BNE,
                "Branches to a relative address if the processor zero flag (Z) is zero."
            },
            { OpName.BPL,
                "Branches to a relative address if the processor negative flag (N) is zero."
            },
            { OpName.BRA,
                "Branches to a relative address."
            },
            { OpName.BRK,
                "Pushes state onto the stack, and jumps to the software break vector at " +
                "$fffe-ffff.  While this is technically a single-byte instruction, the " +
                "program counter pushed onto the stack is incremented by two."
            },
            { OpName.BRL,
                "Branches to a long relative address."
            },
            { OpName.BVC,
                "Branches to a relative address if the processor overflow flag (V) is zero."
            },
            { OpName.BVS,
                "Branches to a relative address if the processor overflow flag (V) is one."
            },
            { OpName.CLC,
                "Sets the processor carry flag (C) to zero."
            },
            { OpName.CLD,
                "Sets the processor decimal flag (D) to zero."
            },
            { OpName.CLI,
                "Sets the processor interrupt disable flag (I) to zero."
            },
            { OpName.CLV,
                "Sets the processor overflow flag (V) to zero."
            },
            { OpName.CMP,
                "Subtracts the value specified by the operand from the contents of the " +
                "accumulator.  Sets the carry, zero, and negative flags, but does not alter " +
                "memory or the accumulator."
            },
            { OpName.COP,
                "Pushes state onto the stack, and jumps to the software interrupt vector at " +
                "$fff4-fff5."
            },
            { OpName.CPX,
                "Subtracts the value specified by the operand from the contents of the " +
                "X register.  Sets the carry, zero, and negative flags, but does not alter " +
                "memory or the X register."
            },
            { OpName.CPY,
                "Subtracts the value specified by the operand from the contents of the " +
                "Y register.  Sets the carry, zero, and negative flags, but does not alter " +
                "memory or the Y register."
            },
            { OpName.DEC,
                "Decrements the contents of the location specified by the operand by one."
            },
            { OpName.DEX,
                "Decrements the X register by one."
            },
            { OpName.DEY,
                "Decrements the Y register by one."
            },
            { OpName.EOR,
                "Performs a bitwise EOR of the accumulator with a value in memory, storing " +
                "the result in the accumulator."
            },
            { OpName.INC,
                "Increments the contents of the location specified by the operand by one."
            },
            { OpName.INX,
                "Increments the X register by one."
            },
            { OpName.INY,
                "Increments the Y register by one."
            },
            { OpName.JML,
                "Branches to a long absolute address."
            },
            { OpName.JMP,
                "Branches to an absolute address."
            },
            { OpName.JSL,
                "Branches to a long absolute address after pushing the current address onto " +
                "the stack.  The value pushed is the address of the last operand byte."
            },
            { OpName.JSR,
                "Branches to an absolute address after pushing the current address onto " +
                "the stack.  The value pushed is the address of the last operand byte."
            },
            { OpName.LDA,
                "Loads the accumulator from memory."
            },
            { OpName.LDX,
                "Loads the X register from memory."
            },
            { OpName.LDY,
                "Loads the Y register from memory."
            },
            { OpName.LSR,
                "Shifts memory or the accumulator one bit right.  The high bit is set to zero, " +
                "and the carry flag receives the low bit."
            },
            { OpName.MVN,
                "Moves a block of memory, starting from a low address and incrementing.  " +
                "The source and destination addresses are in the X and Y registers, " +
                "respectively.  The accumulator holds the number of bytes to move minus 1, " +
                "and the source and destination banks are specified by the operands."
            },
            { OpName.MVP,
                "Moves a block of memory, starting from a high address and decrementing.  " +
                "The source and destination addresses are in the X and Y registers, " +
                "respectively.  The accumulator holds the number of bytes to move minus 1, " +
                "and the source and destination banks are specified by the operands."
            },
            { OpName.NOP,
                "No operation."
            },
            { OpName.ORA,
                "Performs a bitwise OR of the accumulator with a value in memory, storing " +
                "the result in the accumulator."
            },
            { OpName.PEA,
                "Pushes the 16-bit operand onto the stack.  This always pushes two bytes, " +
                "regardless of the M/X processor flags."
            },
            { OpName.PEI,
                "Pushes a 16-bit value from the direct page onto the stack."
            },
            { OpName.PER,
                "Converts a relative offset to an absolute address, and pushes it onto the stack."
            },
            { OpName.PHA,
                "Pushes the accumulator onto the stack."
            },
            { OpName.PHB,
                "Pushes the data bank register onto the stack."
            },
            { OpName.PHD,
                "Pushes the direct page register onto the stack."
            },
            { OpName.PHK,
                "Pushes the program bank register onto the stack."
            },
            { OpName.PHP,
                "Pushes the processor status register onto the stack."
            },
            { OpName.PHX,
                "Pushes the X register onto the stack."
            },
            { OpName.PHY,
                "Pushes the Y register onto the stack."
            },
            { OpName.PLA,
                "Pulls the accumulator off of the stack."
            },
            { OpName.PLB,
                "Pulls the data bank register off of the stack."
            },
            { OpName.PLD,
                "Pulls the direct page register off of the stack."
            },
            { OpName.PLP,
                "Pulls the processor status register off of the stack."
            },
            { OpName.PLX,
                "Pulls the X register off of the stack."
            },
            { OpName.PLY,
                "Pulls the Y register off of the stack."
            },
            { OpName.REP,
                "Sets specific bits in the processor status register to zero."
            },
            { OpName.ROL,
                "Rotates memory or the accumulator one bit left.  The low bit is set to the " +
                "carry flag, and the carry flag receives the high bit."
            },
            { OpName.ROR,
                "Rotates memory or the accumulator one bit right.  The high bit is set to the " +
                "carry flag, and the carry flag receives the low bit."
            },
            { OpName.RTI,
                "Pulls the status register and return address from the stack, and jumps " +
                "to the exact address pulled (note this is different from RTL/RTS)."
            },
            { OpName.RTL,
                "Pulls the 24-bit return address from the stack, increments it, and jumps to it."
            },
            { OpName.RTS,
                "Pulls the 16-bit return address from the stack, increments it, and jumps to it."
            },
            { OpName.SBC,
                "Subtracts the value specified by the operand from the contents of the " +
                "accumulator, and leaves the result in the accumulator.  Sets the carry, " +
                "zero, and negative flags."
            },
            { OpName.SEC,
                "Sets the processor carry flag (C) to one."
            },
            { OpName.SED,
                "Sets the processor decimal flag (D) to one."
            },
            { OpName.SEI,
                "Sets the processor interrupt disable flag (I) to one."
            },
            { OpName.SEP,
                "Sets specific bits in the processor status register to one."
            },
            { OpName.STA,
                "Stores the value in the accumulator into memory."
            },
            { OpName.STP,
                "Stops the processor until a CPU reset occurs."
            },
            { OpName.STX,
                "Stores the value in the X register into memory."
            },
            { OpName.STY,
                "Stores the value in the Y register into memory."
            },
            { OpName.STZ,
                "Stores zero into memory."
            },
            { OpName.TAX,
                "Transfers the contents of the accumulator to the X register."
            },
            { OpName.TAY,
                "Transfers the contents of the accumulator to the Y register."
            },
            { OpName.TCD,
                "Transfers the 16-bit accumulator to the direct page register."
            },
            { OpName.TCS,
                "Transfers the 16-bit accumulator to the stack pointer register."
            },
            { OpName.TDC,
                "Transfers the direct page register to the 16-bit accumulator."
            },
            { OpName.TRB,
                "Logically ANDs the complement of the value in the accumulator with a value " +
                "in memory, and stores it in memory.  This can be used to clear specific bits " +
                "in memory."
            },
            { OpName.TSB,
                "Logically ORs the value in the accumulator with a value in memory, and " +
                "stores it in memory.  This can be used to set specific bits in memory."
            },
            { OpName.TSC,
                "Transfers the stack pointer register to the 16-bit accumulator."
            },
            { OpName.TSX,
                "Transfers the stack pointer register to the X register."
            },
            { OpName.TXA,
                "Transfers the X register to the accumulator."
            },
            { OpName.TXS,
                "Transfers the X register to the stack pointer register."
            },
            { OpName.TXY,
                "Transfers the X register to the Y register."
            },
            { OpName.TYA,
                "Transfers the Y register to the accumulator."
            },
            { OpName.TYX,
                "Transfers the Y register to the X register."
            },
            { OpName.WAI,
                "Stalls the processor until an interrupt is received.  If the interrupt " +
                "disable flag (I) is set to one, execution will continue with the next " +
                "instruction rather than calling through an interrupt vector."
            },
            { OpName.WDM,
                "Reserved for future expansion.  (Behaves as a two-byte NOP.)"
            },
            { OpName.XBA,
                "Swaps the high and low bytes in the 16-bit accumulator.  Sometimes referred " +
                "to as SWA."
            },
            { OpName.XCE,
                "Exchanges carry and emulation bits."
            },

            //
            // 6502 undocumented instructions.
            //
            // References:
            //  http://www.ffd2.com/fridge/docs/6502-NMOS.extra.opcodes
            //  http://nesdev.com/undocumented_opcodes.txt
            //
            { OpName.ANC,
                "AND byte with accumulator. If result is negative then carry is set." +
                "\r\n\r\nAlt mnemonic: AAC"
            },
            { OpName.ANE,
                "Transfer X register to accumulator, then AND accumulator with value." +
                "\r\n\r\nAlt mnemonic: XAA"
            },
            { OpName.ARR,
                "AND byte with accumulator, then rotate one bit right.  Equivalent to " +
                "AND + ROR."
            },
            { OpName.ASR,
                "AND byte with accumulator, then shift right one bit.  Equivalent to AND + LSR." +
                "\r\n\r\nAlt mnemonic: ALR"
            },
            { OpName.DCP,
                "Decrement memory location, then compare result to accumulator.  Equivalent " +
                "to DEC + CMP." +
                "\r\n\r\nAlt mnemonic: DCM"
            },
            { OpName.DOP,
                "Double-byte no-operation." +
                "\r\n\r\nAlt mnemonic: NOP / SKB"
            },
            { OpName.HLT,
                "Crash the CPU, halting execution and ignoring interrupts." +
                "\r\n\r\nAlt mnemonic: KIL / JAM"
            },
            { OpName.ISB,
                "Increment memory, then subtract memory from accumulator with borrow.  " +
                "Equivalent to INC + SBC." +
                "\r\n\r\nAlt mnemonic: ISC / INS"
            },
            { OpName.LAE,
                "AND memory with stack pointer, then transfer result to accumulator, " +
                "X register, and stack pointer.  (Note: possibly unreliable.)" +
                "\r\n\r\nAlt mnemonic: LAR / LAS"
            },
            { OpName.LAX,
                "Load accumulator and X register from memory.  Equivalent to LDA + LDX."
            },
            { OpName.LXA,
                "ORs accumulator with a value, ANDs result with immediate value, then stores " +
                "the result in accumulator and X register." +
                "Equivalent to ORA + AND + TAX." +
                "\r\n\r\nAlt mnemonic: ATX / OAL"
            },
            { OpName.RLA,
                "Rotate memory one bit left, then AND accumulator with memory.  Equivalent " +
                "to ROL + AND."
            },
            { OpName.RRA,
                "Rotate memory one bit right, then add accumulator to memory with carry.  " +
                "Equivalent to ROR + ADC."
            },
            { OpName.SAX,
                "AND X register with accumulator, without changing the contents of either " +
                "register, subtract an immediate value, then store result in X register." +
                "\r\n\r\nAlt mnemonic: AAX / AXS"
            },
            { OpName.SBX,
                "AND X register with accumulator and transfer to X register, then " +
                "subtract byte from X register without borrow." +
                "\r\n\r\nAlt mnemonic: AXS / SAX"
            },
            { OpName.SHA,
                "AND X register with accumulator, then AND result with 7 and store." +
                "\r\n\r\nAlt mnemonic: AXA"
            },
            { OpName.SHS,
                "AND X register with accumulator, without changing the contents of either" +
                "register, and transfer to stack pointer.  Then " +
                "AND stack pointer with high byte of operand + 1." +
                "\r\n\r\nAlt mnemonic: XAS / TAS"
            },
            { OpName.SHX,
                "AND X register with the high byte of the argument + 1, and store the result." +
                "\r\n\r\nAlt mnemonic: SXA / XAS"
            },
            { OpName.SHY,
                "AND Y register with the high byte of the argument + 1, and store the result." +
                "\r\n\r\nAlt mnemonic: SYA / SAY"
            },
            { OpName.SLO,
                "Shift memory left one bit, then OR accumulator with memory.  Equivalent to " +
                "ASL + ORA." +
                "\r\n\r\nAlt mnemonic: ASO"
            },
            { OpName.SRE,
                "Shift memory right one bit, then EOR accumulator with memory.  Equivalent to " +
                "LSR + EOR." +
                "\r\n\r\nAlt mnemonic: LSE"
            },
            { OpName.TOP,
                "Triple-byte no-operation.  This actually performs a load." +
                "\r\n\r\nAlt mnemonic: NOP / SKW"
            },

            //
            // 65C02 undocumented instructions.
            //
            { OpName.LDD,
                "Load and Discard.  Usually a no-op, but the activity on the address bus " +
                "can affect memory-mapped I/O."
            },
        };

        /// <summary>
        /// Address mode short descriptions, USA English.
        /// </summary>
        private static Dictionary<OpDef.AddressMode, string> sAddrMode_enUS =
                new Dictionary<OpDef.AddressMode, string>() {
            { OpDef.AddressMode.Abs, "Absolute" },
            { OpDef.AddressMode.AbsInd, "Absolute Indirect" },
            { OpDef.AddressMode.AbsIndLong, "Absolute Indirect Long" },
            { OpDef.AddressMode.AbsIndexX, "Absolute Indexed X" },
            { OpDef.AddressMode.AbsIndexXInd, "Absolute Indexed X Indirect" },
            { OpDef.AddressMode.AbsIndexXLong, "Absolute Indexed X Long" },
            { OpDef.AddressMode.AbsIndexY, "Absolute Indexed Y" },
            { OpDef.AddressMode.AbsLong, "Absolute Long" },
            { OpDef.AddressMode.Acc, "Accumulator" },
            { OpDef.AddressMode.BlockMove, "Block Move" },
            { OpDef.AddressMode.DP, "Direct Page" },
            { OpDef.AddressMode.DPInd, "Direct Page Indirect" },
            { OpDef.AddressMode.DPIndIndexY, "Direct Page Indirect Indexed Y" },
            { OpDef.AddressMode.DPIndIndexYLong, "Direct Page Indirect Indexed Y Long" },
            { OpDef.AddressMode.DPIndLong, "Direct Page Indirect Long" },
            { OpDef.AddressMode.DPIndexX, "Direct Page Indexed X" },
            { OpDef.AddressMode.DPIndexXInd, "Direct Page Indexed X Indirect" },
            { OpDef.AddressMode.DPIndexY, "Direct Page Indexed Y" },
            { OpDef.AddressMode.Imm, "Immediate" },
            { OpDef.AddressMode.ImmLongA, "Immediate" },
            { OpDef.AddressMode.ImmLongXY, "Immediate" },
            { OpDef.AddressMode.Implied, "" },
            { OpDef.AddressMode.PCRel, "PC Relative" },
            { OpDef.AddressMode.PCRelLong, "PC Relative Long" },
            { OpDef.AddressMode.StackAbs, "Stack Absolute" },
            { OpDef.AddressMode.StackDPInd, "Stack Direct Page Indirect" },
            { OpDef.AddressMode.StackInt, "" },
            { OpDef.AddressMode.StackPCRelLong, "Stack PC Relative Long" },
            { OpDef.AddressMode.StackPull, "" },
            { OpDef.AddressMode.StackPush, "" },
            { OpDef.AddressMode.StackRTI, "" },
            { OpDef.AddressMode.StackRTL, "" },
            { OpDef.AddressMode.StackRTS, "" },
            { OpDef.AddressMode.StackRel, "" },
            { OpDef.AddressMode.StackRelIndIndexY, "Stack Relative Indirect Index Y" },
            { OpDef.AddressMode.WDM, "" }
        };

        /// <summary>
        /// Cycle modifier descriptions.  These are intended to be very terse.
        /// </summary>
        private static Dictionary<OpDef.CycleMod, string> sCycleMod_enUS =
                new Dictionary<OpDef.CycleMod, string>() {
            { OpDef.CycleMod.OneIfM0, "+1 if M=0" },
            { OpDef.CycleMod.TwoIfM0, "+2 if M=0" },
            { OpDef.CycleMod.OneIfX0, "+1 if X=0" },
            { OpDef.CycleMod.OneIfDpNonzero, "+1 if DL != 0" },
            { OpDef.CycleMod.OneIfIndexPage, "+1 if index across page" },
            { OpDef.CycleMod.OneIfD1, "+1 if D=1 on 65C02" },
            { OpDef.CycleMod.OneIfBranchTaken, "+1 if branch taken" },
            { OpDef.CycleMod.OneIfBranchPage, "+1 if branch across page unless E=0" },
            { OpDef.CycleMod.OneIfE0, "+1 if E=0" },
            { OpDef.CycleMod.OneIf65C02, "+1 if 65C02" },
            { OpDef.CycleMod.MinusOneIfNoPage, "-1 if 65C02 and not across page" },
            { OpDef.CycleMod.BlockMove, "+7 per byte" },
        };
   }
}
