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
    /// CPU definition.  Includes a set of 256 opcodes.
    /// </summary>
    public class CpuDef {
        /// <summary>
        /// Human-readable name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Maximum possible address.
        /// </summary>
        public int MaxAddressValue { get; private set; }

        /// <summary>
        /// Does this CPU have a 16-bit address space?
        /// </summary>
        public bool HasAddr16 { get { return MaxAddressValue <= 0xffff; } }

        /// <summary>
        /// Does this CPU support the emulation flag (65802/65816)?
        /// </summary>
        public bool HasEmuFlag { get; private set; }

        /// <summary>
        /// CPU type value.
        /// </summary>
        public CpuType Type { get; private set; }

        /// <summary>
        /// True if undocumented opcodes are included.  (This does not mean that the CPU has
        /// undocumented opcodes, only that they haven't been stripped out if they exist.)
        /// </summary>
        public bool HasUndocumented { get; private set; }

        /// <summary>
        /// Instruction set, 256 entries.
        /// </summary>
        private OpDef[] mOpDefs;

        /// <summary>
        /// Cycle counts, 256 entries.
        /// </summary>
        private int[] mCycleCounts;

        /// <summary>
        /// Cycle count modifiers, 256 entries.
        /// </summary>
        private OpDef.CycleMod[] mCycleMods;

        /// <summary>
        /// List of "interesting" CPUs.  These will be presented in the system definition
        /// file when starting a new project.  The actual set of CPUs that are unique (from
        /// our perspective) is much smaller, as most of these can be represented accurately
        /// (for disassembly purposes) by a handful of archetypes.
        /// </summary>
        public enum CpuType {
            CpuUnknown = 0,
            Cpu6502,    // Apple ][+
            Cpu6502B,   // Atari 800
            Cpu6507,    // Atari 2600
            Cpu6502C,   // Atari 5200
            Cpu6510,    // Commodore 64
            Cpu8502,    // Commodore 128
            Cpu2A03,    // NES

            Cpu65C02,   // Apple //e
            Cpu65SC02,  // Atari Lynx

            Cpu65802,   // ?
            Cpu65816,   // Apple IIgs
            Cpu5A22     // SNES
        }

        /// <summary>
        /// Converts a CPU type name string to a CpuType value.  Used for deserialization.
        /// </summary>
        /// <param name="name">Case-sensitive CPU name</param>
        /// <returns>CpuType value, or CpuUnknown if name wasn't recognized.</returns>
        public static CpuType GetCpuTypeFromName(string name) {
            switch (name) {
                case "6502":        return CpuType.Cpu6502;
                case "6502B":       return CpuType.Cpu6502B;
                case "6502C":       return CpuType.Cpu6502C;
                case "6507":        return CpuType.Cpu6507;
                case "6510":        return CpuType.Cpu6510;
                case "8502":        return CpuType.Cpu8502;
                case "2A03":        return CpuType.Cpu2A03;
                case "65C02":       return CpuType.Cpu65C02;
                case "65SC02":      return CpuType.Cpu65SC02;
                case "65802":       return CpuType.Cpu65802;
                case "65816":       return CpuType.Cpu65816;
                case "5A22":        return CpuType.Cpu5A22;
                default:
                    return CpuType.CpuUnknown;
            }
        }

        /// <summary>
        /// Converts a CpuType value to a CPU type name string.  Used for serialization.
        /// </summary>
        /// <param name="type">CPU type.</param>
        /// <returns>CPU name string.</returns>
        public static string GetCpuNameFromType(CpuType type) {
            switch (type) {
                case CpuType.Cpu6502:   return "6502";
                case CpuType.Cpu6502B:  return "6502B";
                case CpuType.Cpu6502C:  return "6502C";
                case CpuType.Cpu6507:   return "6507";
                case CpuType.Cpu6510:   return "6510";
                case CpuType.Cpu8502:   return "8502";
                case CpuType.Cpu2A03:   return "2A03";
                case CpuType.Cpu65C02:  return "65C02";
                case CpuType.Cpu65SC02: return "65SC02";
                case CpuType.Cpu65802:  return "65802";
                case CpuType.Cpu65816:  return "65816";
                case CpuType.Cpu5A22:   return "5A22";
                default:
                    return "65??";
            }
    }

        /// <summary>
        /// Generates a CpuDef that best matches the parameters.
        /// </summary>
        /// <param name="type">Specific CPU we want.</param>
        /// <param name="includeUndocumented">Set to true if "undocumented" opcodes should
        ///   be included in the definition.</param>
        /// <returns>Best CpuDef.</returns>
        public static CpuDef GetBestMatch(CpuType type, bool includeUndocumented) {
            // Many 65xx variants boil down to a 6502, 65C02, or 65816, at least as far as
            // a disassembler needs to know.  These do not, and would need full definitions:
            //
            //  Hudson Soft HuC6280 (PC Engine / TurboGrafx)
            //  Commodore CSG 4510 / CSG 65CE02 (Amiga A2232 serial port; 4510 has one
            //   additional instruction, so use that as archetype)
            //  Rockwell R65C02 (used in ???)
            //  Jeri's 65DTV02 (used in C64DTV single-chip computer); same as 6502 with
            //   some differences in illegal opcodes
            //  Eloraam 65EL02 (defined in a Minecraft-based emulator)

            CpuDef cpuDef;
            switch (type) {
                case CpuType.Cpu65802:
                case CpuType.Cpu65816:
                case CpuType.Cpu5A22:
                    cpuDef = Cpu65816;
                    break;
                case CpuType.Cpu65C02:
                case CpuType.Cpu65SC02:
                    cpuDef = Cpu65C02;
                    break;
                default:
                    cpuDef = Cpu6502;
                    break;
            }

            cpuDef.GenerateCycleCounts();

            // If we don't want undocumented opcodes, strip them out of the definition.
            // Entries are replaced with OpInvalid.
            if (!includeUndocumented) {
                CpuDef stripped = new CpuDef(cpuDef);
                for (int i = 0; i < 256; i++) {
                    if (cpuDef.mOpDefs[i].IsUndocumented) {
                        stripped.mOpDefs[i] = OpDef.OpInvalid;
                    } else {
                        stripped.mOpDefs[i] = cpuDef.mOpDefs[i];
                    }
                }
                cpuDef = stripped;
            }

            cpuDef.HasUndocumented = includeUndocumented;

            return cpuDef;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Human-readable name.</param>
        /// <param name="maxAddressValue">Address space bits (16 or 24).</param>
        private CpuDef(string name, int maxAddressValue, bool hasEmuFlag) {
            Name = name;
            MaxAddressValue = maxAddressValue;
            HasEmuFlag = hasEmuFlag;
            HasUndocumented = true;
        }

        /// <summary>
        /// Copy constructor.  Does not copy the contents of mOpDefs.  Only used internally.
        /// </summary>
        /// <param name="src">Object to copy from.</param>
        private CpuDef(CpuDef src) {
            Name = src.Name;
            MaxAddressValue = src.MaxAddressValue;
            HasEmuFlag = src.HasEmuFlag;
            Type = src.Type;
            mCycleCounts = src.mCycleCounts;
            mCycleMods = src.mCycleMods;

            mOpDefs = new OpDef[256];
        }


        /// <summary>
        /// Returns an entry from the OpDef array for the specified opcode, 0-255.  (We could
        /// probably just make this the class indexer.)
        /// </summary>
        /// <param name="op">Instruction opcode</param>
        /// <returns>Instruction definition.</returns>
        public OpDef GetOpDef(int op) { return mOpDefs[op]; }

        /// <summary>
        /// Returns the number of cycles required to execute the instruction.  If the value
        /// is negative, the negated value represents the minimum number of cycles for an
        /// instruction with variable timing.
        /// 
        /// The value returned will factor in any CPU-specific aspects.
        /// </summary>
        /// <param name="opNum">Instruction opcode value.</param>
        /// <returns>Cycle count.</returns>
        public int GetCycles(int opNum, StatusFlags flags, OpDef.BranchTaken branchTaken,
                bool branchCrossesPage) {
            // The irrelevant modifiers have already been stripped out.
            OpDef.CycleMod mods = mCycleMods[opNum];
            int cycles = mCycleCounts[opNum];

            // Walk through the various cycle mods.  If we can evaluate them definitively,
            // do so now and remove them from the set.

            // The M/X flags are defined to be in one state or the other, even when the flag
            // value is indeterminate, because we have to be able to size immediate operands
            // appropriately.  So there's no ambiguity here even when there's ambiguity.  We
            // make a similar statement about the E flag.
            if ((mods & OpDef.CycleMod.OneIfM0) != 0) {
                if (!flags.ShortM) {
                    cycles++;
                }
                mods &= ~OpDef.CycleMod.OneIfM0;
            }
            if ((mods & OpDef.CycleMod.TwoIfM0) != 0) {
                if (!flags.ShortM) {
                    cycles += 2;
                }
                mods &= ~OpDef.CycleMod.TwoIfM0;
            }
            if ((mods & OpDef.CycleMod.OneIfX0) != 0) {
                if (!flags.ShortX) {
                    cycles++;
                }
                mods &= ~OpDef.CycleMod.OneIfX0;
            }
            if ((mods & OpDef.CycleMod.OneIfE0) != 0) {
                if (flags.E == 0) {
                    cycles++;
                }
                mods &= ~OpDef.CycleMod.OneIfE0;
            }

            // Some of these can be known, some can't.
            if ((mods & OpDef.CycleMod.OneIfD1) != 0) {
                if (flags.D == 1) {
                    cycles++;
                }
                if (flags.D == 0 || flags.D == 1) {
                    mods &= ~OpDef.CycleMod.OneIfD1;
                }
            }
            if ((mods & OpDef.CycleMod.OneIfBranchTaken) != 0) {
                if (branchTaken == OpDef.BranchTaken.Always) {
                    cycles++;
                }
                if (branchTaken != OpDef.BranchTaken.Indeterminate) {
                    mods &= ~OpDef.CycleMod.OneIfBranchTaken;
                }
            }
            if ((mods & OpDef.CycleMod.OneIfBranchPage) != 0) {
                if (branchCrossesPage && flags.E != 0) {
                    cycles++;   // +1 unless we're in native mode on 65816
                }
                mods &= ~OpDef.CycleMod.OneIfBranchPage;
            }

            // We can't evaluate OneIfDpNonzero, OneIfIndexPage, or MinusOneIfNoPage.
            // OneIf65C02 was handled earlier.
            // TODO(maybe): in some cases we can know that the index doesn't cross a
            //   page boundary by checking the address, e.g. "LDA $2000,X" can't cross.

            if (mods != 0) {
                // Some unresolved mods remain.
                cycles = -cycles;
            }
            return cycles;
        }


        /// <summary>
        /// Consistency test.
        /// </summary>
        /// <returns>True on success.</returns>
        public static bool DebugValidate() {
            InternalValidate(Cpu6502);
            InternalValidate(Cpu65C02);
            InternalValidate(Cpu65816);
            Debug.WriteLine("CpuDefs okay");
            return true;
        }
        private static void InternalValidate(CpuDef cdef) {
            for (int i = 0; i < 256; i++) {
                OpDef op = cdef.mOpDefs[i];
                if (op.Opcode != i && op.AddrMode != OpDef.AddressMode.Unknown) {
                    throw new Exception("CpuDef for " + cdef.Type + ": entry 0x" +
                        i.ToString("x") + " has value " + op.Opcode.ToString("x"));
                }
                if (op.AddrMode != OpDef.AddressMode.Unknown && op.Cycles == 0) {
                    throw new Exception("Instruction 0x" + i.ToString("x2") + ": " +
                        op + " missing cycles");
                }
            }
        }

        public override string ToString() {
            return Name + " (has16=" + HasAddr16 + ", hasEmu=" + HasEmuFlag + ")";
        }

        /// <summary>
        /// Generates the mCycleCounts and mCycleMods arrays.
        /// </summary>
        private void GenerateCycleCounts() {
            if (mCycleCounts != null) {
                return;
            }
            mCycleCounts = new int[256];
            mCycleMods = new OpDef.CycleMod[256];

            // Figure out which mods apply for this CPU.
            OpDef.CycleMod ignoreMask = 0;
            switch (Type) {
                case CpuType.Cpu6502:
                    ignoreMask = OpDef.CycleMod.OneIfM0 |
                                 OpDef.CycleMod.TwoIfM0 |
                                 OpDef.CycleMod.OneIfX0 |
                                 OpDef.CycleMod.OneIfDpNonzero |
                                 OpDef.CycleMod.OneIfD1 |
                                 OpDef.CycleMod.OneIfE0 |
                                 OpDef.CycleMod.OneIf65C02 |
                                 OpDef.CycleMod.MinusOneIfNoPage |
                                 OpDef.CycleMod.BlockMove;
                    break;
                case CpuType.Cpu65C02:
                    ignoreMask = OpDef.CycleMod.OneIfM0 |
                                 OpDef.CycleMod.TwoIfM0 |
                                 OpDef.CycleMod.OneIfX0 |
                                 OpDef.CycleMod.OneIfDpNonzero |
                                 OpDef.CycleMod.OneIfE0 |
                                 OpDef.CycleMod.BlockMove;
                    break;
                case CpuType.Cpu65816:
                    ignoreMask = OpDef.CycleMod.OneIfD1 |
                                 OpDef.CycleMod.OneIf65C02 |
                                 OpDef.CycleMod.MinusOneIfNoPage;
                    break;
                default:
                    Debug.Assert(false, "unsupported cpu type " + Type);
                    return;
            }

            // If an instruction has one or more applicable mods, declare it as variable.
            for (int i = 0; i < 256; i++) {
                OpDef op = mOpDefs[i];
                int baseCycles = op.Cycles;
                OpDef.CycleMod mods = op.CycleMods & ~ignoreMask;
                if ((mods & OpDef.CycleMod.OneIf65C02) != 0) {
                    // This isn't variable -- the instruction always takes one cycle longer
                    // on the 65C02.  (Applies to $6C, JMP (addr).)
                    Debug.Assert(Type == CpuType.Cpu65C02);
                    baseCycles++;
                    mods &= ~OpDef.CycleMod.OneIf65C02;
                }
                mCycleCounts[i] = baseCycles;
                mCycleMods[i] = mods;
            }
        }


        // Classic MOS 6502, with full set of undocumented opcodes.
        private static CpuDef Cpu6502 { get; } = new CpuDef("MOS 6502", (1 << 16) - 1, false) {
            Type = CpuType.Cpu6502,
            mOpDefs = new OpDef[] {
                OpDef.OpBRK_StackInt,           // 0x00
                OpDef.OpORA_DPIndexXInd,
                OpDef.GenerateUndoc(0x02, OpDef.OpJAM_Implied),
                OpDef.OpSLO_DPIndexXInd,
                OpDef.GenerateUndoc(0x04, OpDef.OpDOP_DP),
                OpDef.OpORA_DP,
                OpDef.OpASL_DP,
                OpDef.OpSLO_DP,
                OpDef.OpPHP_StackPush,          // 0x08
                OpDef.OpORA_Imm,
                OpDef.OpASL_Acc,
                OpDef.GenerateUndoc(0x0b, OpDef.OpANC_Imm),
                OpDef.OpTOP_Abs,
                OpDef.OpORA_Abs,
                OpDef.OpASL_Abs,
                OpDef.OpSLO_Absolute,
                OpDef.OpBPL_PCRel,              // 0x10
                OpDef.OpORA_DPIndIndexY,
                OpDef.GenerateUndoc(0x12, OpDef.OpJAM_Implied),
                OpDef.OpSLO_DPIndIndexY,
                OpDef.GenerateUndoc(0x14, OpDef.OpDOP_DPIndexX),
                OpDef.OpORA_DPIndexX,
                OpDef.OpASL_DPIndexX,
                OpDef.OpSLO_DPIndexX,
                OpDef.OpCLC_Implied,            // 0x18
                OpDef.OpORA_AbsIndexY,
                OpDef.GenerateUndoc(0x1a, OpDef.OpNOP_Implied),
                OpDef.OpSLO_AbsIndexY,
                OpDef.GenerateUndoc(0x1c, OpDef.OpTOP_AbsIndeX),
                OpDef.OpORA_AbsIndexX,
                OpDef.OpASL_AbsIndexX,
                OpDef.OpSLO_AbsIndexX,
                OpDef.OpJSR_Abs,                // 0x20
                OpDef.OpAND_DPIndexXInd,
                OpDef.GenerateUndoc(0x22, OpDef.OpJAM_Implied),
                OpDef.OpRLA_DPIndexXInd,
                OpDef.OpBIT_DP,
                OpDef.OpAND_DP,
                OpDef.OpROL_DP,
                OpDef.OpRLA_DP,
                OpDef.OpPLP_StackPull,          // 0x28
                OpDef.OpAND_Imm,
                OpDef.OpROL_Acc,
                OpDef.GenerateUndoc(0x2b, OpDef.OpANC_Imm),
                OpDef.OpBIT_Abs,
                OpDef.OpAND_Abs,
                OpDef.OpROL_Abs,
                OpDef.OpRLA_Absolute,
                OpDef.OpBMI_PCRel,              // 0x30
                OpDef.OpAND_DPIndIndexY,
                OpDef.GenerateUndoc(0x32, OpDef.OpJAM_Implied),
                OpDef.OpRLA_DPIndIndexY,
                OpDef.GenerateUndoc(0x34, OpDef.OpDOP_DPIndexX),
                OpDef.OpAND_DPIndexX,
                OpDef.OpROL_DPIndexX,
                OpDef.OpRLA_DPIndexX,
                OpDef.OpSEC_Implied,            // 0x38
                OpDef.OpAND_AbsIndexY,
                OpDef.GenerateUndoc(0x3a, OpDef.OpNOP_Implied),
                OpDef.OpRLA_AbsIndexY,
                OpDef.GenerateUndoc(0x3c, OpDef.OpTOP_AbsIndeX),
                OpDef.OpAND_AbsIndexX,
                OpDef.OpROL_AbsIndexX,
                OpDef.OpRLA_AbsIndexX,
                OpDef.OpRTI_StackRTI,           // 0x40
                OpDef.OpEOR_DPIndexXInd,
                OpDef.GenerateUndoc(0x42, OpDef.OpJAM_Implied),
                OpDef.OpSRE_DPIndexXInd,
                OpDef.GenerateUndoc(0x44, OpDef.OpDOP_DP),
                OpDef.OpEOR_DP,
                OpDef.OpLSR_DP,
                OpDef.OpSRE_DP,
                OpDef.OpPHA_StackPush,          // 0x48
                OpDef.OpEOR_Imm,
                OpDef.OpLSR_Acc,
                OpDef.OpALR_Imm,
                OpDef.OpJMP_Abs,
                OpDef.OpEOR_Abs,
                OpDef.OpLSR_Abs,
                OpDef.OpSRE_Absolute,
                OpDef.OpBVC_PCRel,              // 0x50
                OpDef.OpEOR_DPIndIndexY,
                OpDef.GenerateUndoc(0x52, OpDef.OpJAM_Implied),
                OpDef.OpSRE_DPIndIndexY,
                OpDef.GenerateUndoc(0x54, OpDef.OpDOP_DPIndexX),
                OpDef.OpEOR_DPIndexX,
                OpDef.OpLSR_DPIndexX,
                OpDef.OpSRE_DPIndexX,
                OpDef.OpCLI_Implied,            // 0x58
                OpDef.OpEOR_AbsIndexY,
                OpDef.GenerateUndoc(0x5a, OpDef.OpNOP_Implied),
                OpDef.OpSRE_AbsIndexY,
                OpDef.GenerateUndoc(0x5c, OpDef.OpTOP_AbsIndeX),
                OpDef.OpEOR_AbsIndexX,
                OpDef.OpLSR_AbsIndexX,
                OpDef.OpSRE_AbsIndexX,
                OpDef.OpRTS_StackRTS,           // 0x60
                OpDef.OpADC_DPIndexXInd,
                OpDef.GenerateUndoc(0x62, OpDef.OpJAM_Implied),
                OpDef.OpRRA_DPIndexXInd,
                OpDef.GenerateUndoc(0x64, OpDef.OpDOP_DP),
                OpDef.OpADC_DP,
                OpDef.OpROR_DP,
                OpDef.OpRRA_DP,
                OpDef.OpPLA_StackPull,          // 0x68
                OpDef.OpADC_Imm,
                OpDef.OpROR_Acc,
                OpDef.OpARR_Imm,
                OpDef.OpJMP_AbsInd,
                OpDef.OpADC_Abs,
                OpDef.OpROR_Abs,
                OpDef.OpRRA_Absolute,
                OpDef.OpBVS_PCRel,              // 0x70
                OpDef.OpADC_DPIndIndexY,
                OpDef.GenerateUndoc(0x72, OpDef.OpJAM_Implied),
                OpDef.OpRRA_DPIndIndexY,
                OpDef.GenerateUndoc(0x74, OpDef.OpDOP_DPIndexX),
                OpDef.OpADC_DPIndexX,
                OpDef.OpROR_DPIndexX,
                OpDef.OpRRA_DPIndexX,
                OpDef.OpSEI_Implied,            // 0x78
                OpDef.OpADC_AbsIndexY,
                OpDef.GenerateUndoc(0x7a, OpDef.OpNOP_Implied),
                OpDef.OpRRA_AbsIndexY,
                OpDef.GenerateUndoc(0x7c, OpDef.OpTOP_AbsIndeX),
                OpDef.OpADC_AbsIndexX,
                OpDef.OpROR_AbsIndexX,
                OpDef.OpRRA_AbsIndexX,
                OpDef.GenerateUndoc(0x80, OpDef.OpDOP_Imm), // 0x80
                OpDef.OpSTA_DPIndexXInd,
                OpDef.GenerateUndoc(0x82, OpDef.OpDOP_Imm),
                OpDef.OpSAX_DPIndexXInd,
                OpDef.OpSTY_DP,
                OpDef.OpSTA_DP,
                OpDef.OpSTX_DP,
                OpDef.OpSAX_DP,
                OpDef.OpDEY_Implied,            // 0x88
                OpDef.GenerateUndoc(0x89, OpDef.OpDOP_Imm),
                OpDef.OpTXA_Implied,
                OpDef.OpANE_Imm,
                OpDef.OpSTY_Abs,
                OpDef.OpSTA_Abs,
                OpDef.OpSTX_Abs,
                OpDef.OpSAX_Absolute,
                OpDef.OpBCC_PCRel,              // 0x90
                OpDef.OpSTA_DPIndIndexY,
                OpDef.GenerateUndoc(0x92, OpDef.OpJAM_Implied),
                OpDef.OpSHA_DPIndIndexY,
                OpDef.OpSTY_DPIndexX,
                OpDef.OpSTA_DPIndexX,
                OpDef.OpSTX_DPIndexY,
                OpDef.OpSAX_DPIndexY,
                OpDef.OpTYA_Implied,            // 0x98
                OpDef.OpSTA_AbsIndexY,
                OpDef.OpTXS_Implied,
                OpDef.OpTAS_AbsIndexY,
                OpDef.OpSHY_AbsIndexX,
                OpDef.OpSTA_AbsIndexX,
                OpDef.OpSHX_AbsIndexY,
                OpDef.OpSHA_AbsIndexY,
                OpDef.OpLDY_Imm,                // 0xa0
                OpDef.OpLDA_DPIndexXInd,
                OpDef.OpLDX_Imm,
                OpDef.OpLAX_DPIndexXInd,
                OpDef.OpLDY_DP,
                OpDef.OpLDA_DP,
                OpDef.OpLDX_DP,
                OpDef.OpLAX_DP,
                OpDef.OpTAY_Implied,            // 0xa8
                OpDef.OpLDA_Imm,
                OpDef.OpTAX_Implied,
                OpDef.OpLAX_Imm,
                OpDef.OpLDY_Abs,
                OpDef.OpLDA_Abs,
                OpDef.OpLDX_Abs,
                OpDef.OpLAX_Absolute,
                OpDef.OpBCS_PCRel,              // 0xb0
                OpDef.OpLDA_DPIndIndexY,
                OpDef.GenerateUndoc(0xb2, OpDef.OpJAM_Implied),
                OpDef.OpLAX_DPIndIndexY,
                OpDef.OpLDY_DPIndexX,
                OpDef.OpLDA_DPIndexX,
                OpDef.OpLDX_DPIndexY,
                OpDef.OpLAX_DPIndexY,
                OpDef.OpCLV_Implied,            // 0xb8
                OpDef.OpLDA_AbsIndexY,
                OpDef.OpTSX_Implied,
                OpDef.OpLAS_AbsIndexY,
                OpDef.OpLDY_AbsIndexX,
                OpDef.OpLDA_AbsIndexX,
                OpDef.OpLDX_AbsIndexY,
                OpDef.OpLAX_AbsIndexY,
                OpDef.OpCPY_Imm,                // 0xc0
                OpDef.OpCMP_DPIndexXInd,
                OpDef.GenerateUndoc(0xc2, OpDef.OpDOP_Imm),
                OpDef.OpDCP_DPIndexXInd,
                OpDef.OpCPY_DP,
                OpDef.OpCMP_DP,
                OpDef.OpDEC_DP,
                OpDef.OpDCP_DP,
                OpDef.OpINY_Implied,            // 0xc8
                OpDef.OpCMP_Imm,
                OpDef.OpDEX_Implied,
                OpDef.OpSBX_Imm,
                OpDef.OpCPY_Abs,
                OpDef.OpCMP_Abs,
                OpDef.OpDEC_Abs,
                OpDef.OpDCP_Abs,
                OpDef.OpBNE_PCRel,              // 0xd0
                OpDef.OpCMP_DPIndIndexY,
                OpDef.GenerateUndoc(0xd2, OpDef.OpJAM_Implied),
                OpDef.OpDCP_DPIndIndexY,
                OpDef.GenerateUndoc(0xd4, OpDef.OpDOP_DPIndexX),
                OpDef.OpCMP_DPIndexX,
                OpDef.OpDEC_DPIndexX,
                OpDef.OpDCP_DPIndexX,
                OpDef.OpCLD_Implied,            // 0xd8
                OpDef.OpCMP_AbsIndexY,
                OpDef.GenerateUndoc(0xda, OpDef.OpNOP_Implied),
                OpDef.OpDCP_AbsIndexY,
                OpDef.GenerateUndoc(0xdc, OpDef.OpTOP_AbsIndeX),
                OpDef.OpCMP_AbsIndexX,
                OpDef.OpDEC_AbsIndexX,
                OpDef.OpDCP_AbsIndexX,
                OpDef.OpCPX_Imm,                // 0xe0
                OpDef.OpSBC_DPIndexXInd,
                OpDef.GenerateUndoc(0xe2, OpDef.OpDOP_Imm),
                OpDef.OpISC_DPIndexXInd,
                OpDef.OpCPX_DP,
                OpDef.OpSBC_DP,
                OpDef.OpINC_DP,
                OpDef.OpISC_DP,
                OpDef.OpINX_Implied,            // 0xe8
                OpDef.OpSBC_Imm,
                OpDef.OpNOP_Implied,
                OpDef.GenerateUndoc(0xeb, OpDef.OpSBC_Imm),
                OpDef.OpCPX_Abs,
                OpDef.OpSBC_Abs,
                OpDef.OpINC_Abs,
                OpDef.OpISC_Abs,
                OpDef.OpBEQ_PCRel,              // 0xf0
                OpDef.OpSBC_DPIndIndexY,
                OpDef.GenerateUndoc(0xf2, OpDef.OpJAM_Implied),
                OpDef.OpISC_DPIndIndexY,
                OpDef.GenerateUndoc(0xf4, OpDef.OpDOP_DPIndexX),
                OpDef.OpSBC_DPIndexX,
                OpDef.OpINC_DPIndexX,
                OpDef.OpISC_DPIndexX,
                OpDef.OpSED_Implied,            // 0xf8
                OpDef.OpSBC_AbsIndexY,
                OpDef.GenerateUndoc(0xfa, OpDef.OpNOP_Implied),
                OpDef.OpISC_AbsIndexY,
                OpDef.GenerateUndoc(0xfc, OpDef.OpTOP_AbsIndeX),
                OpDef.OpSBC_AbsIndexX,
                OpDef.OpINC_AbsIndexX,
                OpDef.OpISC_AbsIndexX,
            }
        };


        // WDC's 65C02, with new opcodes and a handful of slightly strange NOPs.
        private static CpuDef Cpu65C02 { get; } = new CpuDef("WDC W65C02S", (1 << 16) - 1, false) {
            Type = CpuType.Cpu65C02,
            mOpDefs = new OpDef[] {
                OpDef.OpBRK_StackInt,           // 0x00
                OpDef.OpORA_DPIndexXInd,
                OpDef.GenerateUndoc(0x02, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0x03, OpDef.OpNOP_65C02),
                OpDef.OpTSB_DP,
                OpDef.OpORA_DP,
                OpDef.OpASL_DP,
                OpDef.GenerateUndoc(0x07, OpDef.OpNOP_65C02),
                OpDef.OpPHP_StackPush,          // 0x08
                OpDef.OpORA_Imm,
                OpDef.OpASL_Acc,
                OpDef.GenerateUndoc(0x0b, OpDef.OpNOP_65C02),
                OpDef.OpTSB_Abs,
                OpDef.OpORA_Abs,
                OpDef.OpASL_Abs,
                OpDef.GenerateUndoc(0x0f, OpDef.OpNOP_65C02),
                OpDef.OpBPL_PCRel,              // 0x10
                OpDef.OpORA_DPIndIndexY,
                OpDef.OpORA_DPInd,
                OpDef.GenerateUndoc(0x13, OpDef.OpNOP_65C02),
                OpDef.OpTRB_DP,
                OpDef.OpORA_DPIndexX,
                OpDef.OpASL_DPIndexX,
                OpDef.GenerateUndoc(0x17, OpDef.OpNOP_65C02),
                OpDef.OpCLC_Implied,            // 0x18
                OpDef.OpORA_AbsIndexY,
                OpDef.OpINC_Acc,
                OpDef.GenerateUndoc(0x1b, OpDef.OpNOP_65C02),
                OpDef.OpTRB_Abs,
                OpDef.OpORA_AbsIndexX,
                OpDef.OpASL_AbsIndexX,
                OpDef.GenerateUndoc(0x1f, OpDef.OpNOP_65C02),
                OpDef.OpJSR_Abs,                // 0x20
                OpDef.OpAND_DPIndexXInd,
                OpDef.GenerateUndoc(0x22, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0x23, OpDef.OpNOP_65C02),
                OpDef.OpBIT_DP,
                OpDef.OpAND_DP,
                OpDef.OpROL_DP,
                OpDef.GenerateUndoc(0x27, OpDef.OpNOP_65C02),
                OpDef.OpPLP_StackPull,          // 0x28
                OpDef.OpAND_Imm,
                OpDef.OpROL_Acc,
                OpDef.GenerateUndoc(0x2b, OpDef.OpNOP_65C02),
                OpDef.OpBIT_Abs,
                OpDef.OpAND_Abs,
                OpDef.OpROL_Abs,
                OpDef.GenerateUndoc(0x2f, OpDef.OpNOP_65C02),
                OpDef.OpBMI_PCRel,              // 0x30
                OpDef.OpAND_DPIndIndexY,
                OpDef.OpAND_DPInd,
                OpDef.GenerateUndoc(0x33, OpDef.OpNOP_65C02),
                OpDef.OpBIT_DPIndexX,
                OpDef.OpAND_DPIndexX,
                OpDef.OpROL_DPIndexX,
                OpDef.GenerateUndoc(0x37, OpDef.OpNOP_65C02),
                OpDef.OpSEC_Implied,            // 0x38
                OpDef.OpAND_AbsIndexY,
                OpDef.OpDEC_Acc,
                OpDef.GenerateUndoc(0x3b, OpDef.OpNOP_65C02),
                OpDef.OpBIT_AbsIndexX,
                OpDef.OpAND_AbsIndexX,
                OpDef.OpROL_AbsIndexX,
                OpDef.GenerateUndoc(0x3f, OpDef.OpNOP_65C02),
                OpDef.OpRTI_StackRTI,           // 0x40
                OpDef.OpEOR_DPIndexXInd,
                OpDef.GenerateUndoc(0x42, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0x43, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0x44, OpDef.OpLDD_DP),
                OpDef.OpEOR_DP,
                OpDef.OpLSR_DP,
                OpDef.GenerateUndoc(0x47, OpDef.OpNOP_65C02),
                OpDef.OpPHA_StackPush,          // 0x48
                OpDef.OpEOR_Imm,
                OpDef.OpLSR_Acc,
                OpDef.GenerateUndoc(0x4b, OpDef.OpNOP_65C02),
                OpDef.OpJMP_Abs,
                OpDef.OpEOR_Abs,
                OpDef.OpLSR_Abs,
                OpDef.GenerateUndoc(0x4f, OpDef.OpNOP_65C02),
                OpDef.OpBVC_PCRel,              // 0x50
                OpDef.OpEOR_DPIndIndexY,
                OpDef.OpEOR_DPInd,
                OpDef.GenerateUndoc(0x53, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0x54, OpDef.OpLDD_DPIndexX),
                OpDef.OpEOR_DPIndexX,
                OpDef.OpLSR_DPIndexX,
                OpDef.GenerateUndoc(0x57, OpDef.OpNOP_65C02),
                OpDef.OpCLI_Implied,            // 0x58
                OpDef.OpEOR_AbsIndexY,
                OpDef.OpPHY_StackPush,
                OpDef.GenerateUndoc(0x5b, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0x5c, OpDef.OpLDD_Weird),
                OpDef.OpEOR_AbsIndexX,
                OpDef.OpLSR_AbsIndexX,
                OpDef.GenerateUndoc(0x5f, OpDef.OpNOP_65C02),
                OpDef.OpRTS_StackRTS,           // 0x60
                OpDef.OpADC_DPIndexXInd,
                OpDef.GenerateUndoc(0x62, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0x63, OpDef.OpNOP_65C02),
                OpDef.OpSTZ_DP,
                OpDef.OpADC_DP,
                OpDef.OpROR_DP,
                OpDef.GenerateUndoc(0x67, OpDef.OpNOP_65C02),
                OpDef.OpPLA_StackPull,          // 0x68
                OpDef.OpADC_Imm,
                OpDef.OpROR_Acc,
                OpDef.GenerateUndoc(0x6b, OpDef.OpNOP_65C02),
                OpDef.OpJMP_AbsInd,
                OpDef.OpADC_Abs,
                OpDef.OpROR_Abs,
                OpDef.GenerateUndoc(0x6f, OpDef.OpNOP_65C02),
                OpDef.OpBVS_PCRel,              // 0x70
                OpDef.OpADC_DPIndIndexY,
                OpDef.OpADC_DPInd,
                OpDef.GenerateUndoc(0x73, OpDef.OpNOP_65C02),
                OpDef.OpSTZ_DPIndexX,
                OpDef.OpADC_DPIndexX,
                OpDef.OpROR_DPIndexX,
                OpDef.GenerateUndoc(0x77, OpDef.OpNOP_65C02),
                OpDef.OpSEI_Implied,            // 0x78
                OpDef.OpADC_AbsIndexY,
                OpDef.OpPLY_StackPull,
                OpDef.GenerateUndoc(0x7b, OpDef.OpNOP_65C02),
                OpDef.OpJMP_AbsIndexXInd,
                OpDef.OpADC_AbsIndexX,
                OpDef.OpROR_AbsIndexX,
                OpDef.GenerateUndoc(0x7f, OpDef.OpNOP_65C02),
                OpDef.OpBRA_PCRel,              // 0x80
                OpDef.OpSTA_DPIndexXInd,
                OpDef.GenerateUndoc(0x82, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0x83, OpDef.OpNOP_65C02),
                OpDef.OpSTY_DP,
                OpDef.OpSTA_DP,
                OpDef.OpSTX_DP,
                OpDef.GenerateUndoc(0x87, OpDef.OpNOP_65C02),
                OpDef.OpDEY_Implied,            // 0x88
                OpDef.OpBIT_Imm,
                OpDef.OpTXA_Implied,
                OpDef.GenerateUndoc(0x8b, OpDef.OpNOP_65C02),
                OpDef.OpSTY_Abs,
                OpDef.OpSTA_Abs,
                OpDef.OpSTX_Abs,
                OpDef.GenerateUndoc(0x8f, OpDef.OpNOP_65C02),
                OpDef.OpBCC_PCRel,              // 0x90
                OpDef.OpSTA_DPIndIndexY,
                OpDef.OpSTA_DPInd,
                OpDef.GenerateUndoc(0x93, OpDef.OpNOP_65C02),
                OpDef.OpSTY_DPIndexX,
                OpDef.OpSTA_DPIndexX,
                OpDef.OpSTX_DPIndexY,
                OpDef.GenerateUndoc(0x97, OpDef.OpNOP_65C02),
                OpDef.OpTYA_Implied,            // 0x98
                OpDef.OpSTA_AbsIndexY,
                OpDef.OpTXS_Implied,
                OpDef.GenerateUndoc(0x9b, OpDef.OpNOP_65C02),
                OpDef.OpSTZ_Abs,
                OpDef.OpSTA_AbsIndexX,
                OpDef.OpSTZ_AbsIndexX,
                OpDef.GenerateUndoc(0x9f, OpDef.OpNOP_65C02),
                OpDef.OpLDY_Imm,                // 0xa0
                OpDef.OpLDA_DPIndexXInd,
                OpDef.OpLDX_Imm,
                OpDef.GenerateUndoc(0xa3, OpDef.OpNOP_65C02),
                OpDef.OpLDY_DP,
                OpDef.OpLDA_DP,
                OpDef.OpLDX_DP,
                OpDef.GenerateUndoc(0xa7, OpDef.OpNOP_65C02),
                OpDef.OpTAY_Implied,            // 0xa8
                OpDef.OpLDA_Imm,
                OpDef.OpTAX_Implied,
                OpDef.GenerateUndoc(0xab, OpDef.OpNOP_65C02),
                OpDef.OpLDY_Abs,
                OpDef.OpLDA_Abs,
                OpDef.OpLDX_Abs,
                OpDef.GenerateUndoc(0xaf, OpDef.OpNOP_65C02),
                OpDef.OpBCS_PCRel,              // 0xb0
                OpDef.OpLDA_DPIndIndexY,
                OpDef.OpLDA_DPInd,
                OpDef.GenerateUndoc(0xb3, OpDef.OpNOP_65C02),
                OpDef.OpLDY_DPIndexX,
                OpDef.OpLDA_DPIndexX,
                OpDef.OpLDX_DPIndexY,
                OpDef.GenerateUndoc(0xb7, OpDef.OpNOP_65C02),
                OpDef.OpCLV_Implied,            // 0xb8
                OpDef.OpLDA_AbsIndexY,
                OpDef.OpTSX_Implied,
                OpDef.GenerateUndoc(0xbb, OpDef.OpNOP_65C02),
                OpDef.OpLDY_AbsIndexX,
                OpDef.OpLDA_AbsIndexX,
                OpDef.OpLDX_AbsIndexY,
                OpDef.GenerateUndoc(0xbf, OpDef.OpNOP_65C02),
                OpDef.OpCPY_Imm,                // 0xc0
                OpDef.OpCMP_DPIndexXInd,
                OpDef.GenerateUndoc(0xc2, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0xc3, OpDef.OpNOP_65C02),
                OpDef.OpCPY_DP,
                OpDef.OpCMP_DP,
                OpDef.OpDEC_DP,
                OpDef.GenerateUndoc(0xc7, OpDef.OpNOP_65C02),
                OpDef.OpINY_Implied,            // 0xc8
                OpDef.OpCMP_Imm,
                OpDef.OpDEX_Implied,
                OpDef.GenerateUndoc(0xcb, OpDef.OpNOP_65C02),
                OpDef.OpCPY_Abs,
                OpDef.OpCMP_Abs,
                OpDef.OpDEC_Abs,
                OpDef.GenerateUndoc(0xcf, OpDef.OpNOP_65C02),
                OpDef.OpBNE_PCRel,              // 0xd0
                OpDef.OpCMP_DPIndIndexY,
                OpDef.OpCMP_DPInd,
                OpDef.GenerateUndoc(0xd3, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0xd4, OpDef.OpLDD_DPIndexX),
                OpDef.OpCMP_DPIndexX,
                OpDef.OpDEC_DPIndexX,
                OpDef.GenerateUndoc(0xd7, OpDef.OpNOP_65C02),
                OpDef.OpCLD_Implied,            // 0xd8
                OpDef.OpCMP_AbsIndexY,
                OpDef.OpPHX_StackPush,
                OpDef.GenerateUndoc(0xdb, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0xdc, OpDef.OpLDD_Absolute),
                OpDef.OpCMP_AbsIndexX,
                OpDef.OpDEC_AbsIndexX,
                OpDef.GenerateUndoc(0xdf, OpDef.OpNOP_65C02),
                OpDef.OpCPX_Imm,                // 0xe0
                OpDef.OpSBC_DPIndexXInd,
                OpDef.GenerateUndoc(0xe2, OpDef.OpLDD_Imm),
                OpDef.GenerateUndoc(0xe3, OpDef.OpNOP_65C02),
                OpDef.OpCPX_DP,
                OpDef.OpSBC_DP,
                OpDef.OpINC_DP,
                OpDef.GenerateUndoc(0xe7, OpDef.OpNOP_65C02),
                OpDef.OpINX_Implied,            // 0xe8
                OpDef.OpSBC_Imm,
                OpDef.OpNOP_Implied,
                OpDef.GenerateUndoc(0xeb, OpDef.OpNOP_65C02),
                OpDef.OpCPX_Abs,
                OpDef.OpSBC_Abs,
                OpDef.OpINC_Abs,
                OpDef.GenerateUndoc(0xef, OpDef.OpNOP_65C02),
                OpDef.OpBEQ_PCRel,              // 0xf0
                OpDef.OpSBC_DPIndIndexY,
                OpDef.OpSBC_DPInd,
                OpDef.GenerateUndoc(0xf3, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0xf4, OpDef.OpLDD_DPIndexX),
                OpDef.OpSBC_DPIndexX,
                OpDef.OpINC_DPIndexX,
                OpDef.GenerateUndoc(0xf7, OpDef.OpNOP_65C02),
                OpDef.OpSED_Implied,            // 0xf8
                OpDef.OpSBC_AbsIndexY,
                OpDef.OpPLX_StackPull,
                OpDef.GenerateUndoc(0xfb, OpDef.OpNOP_65C02),
                OpDef.GenerateUndoc(0xfc, OpDef.OpLDD_Absolute),
                OpDef.OpSBC_AbsIndexX,
                OpDef.OpINC_AbsIndexX,
                OpDef.GenerateUndoc(0xff, OpDef.OpNOP_65C02),
            }
        };


        // WDC 65802 and 65816.  No undocumented opcodes -- all 256 are used.
        private static CpuDef Cpu65816 { get; } = new CpuDef("WDC W65C816S", (1 << 24) - 1, true) {
            Type = CpuType.Cpu65816,
            mOpDefs = new OpDef[] {
                OpDef.OpBRK_StackInt,           // 0x00
                OpDef.OpORA_DPIndexXInd,
                OpDef.OpCOP_StackInt,
                OpDef.OpORA_StackRel,
                OpDef.OpTSB_DP,
                OpDef.OpORA_DP,
                OpDef.OpASL_DP,
                OpDef.OpORA_DPIndLong,
                OpDef.OpPHP_StackPush,          // 0x08
                OpDef.OpORA_ImmLongA,
                OpDef.OpASL_Acc,
                OpDef.OpPHD_StackPush,
                OpDef.OpTSB_Abs,
                OpDef.OpORA_Abs,
                OpDef.OpASL_Abs,
                OpDef.OpORA_AbsLong,
                OpDef.OpBPL_PCRel,              // 0x10
                OpDef.OpORA_DPIndIndexY,
                OpDef.OpORA_DPInd,
                OpDef.OpORA_StackRelIndIndexY,
                OpDef.OpTRB_DP,
                OpDef.OpORA_DPIndexX,
                OpDef.OpASL_DPIndexX,
                OpDef.OpORA_DPIndIndexYLong,
                OpDef.OpCLC_Implied,            // 0x18
                OpDef.OpORA_AbsIndexY,
                OpDef.OpINC_Acc,
                OpDef.OpTCS_Implied,
                OpDef.OpTRB_Abs,
                OpDef.OpORA_AbsIndexX,
                OpDef.OpASL_AbsIndexX,
                OpDef.OpORA_AbsIndexXLong,
                OpDef.OpJSR_Abs,                // 0x20
                OpDef.OpAND_DPIndexXInd,
                OpDef.OpJSR_AbsLong,
                OpDef.OpAND_StackRel,
                OpDef.OpBIT_DP,
                OpDef.OpAND_DP,
                OpDef.OpROL_DP,
                OpDef.OpAND_DPIndLong,
                OpDef.OpPLP_StackPull,          // 0x28
                OpDef.OpAND_ImmLongA,
                OpDef.OpROL_Acc,
                OpDef.OpPLD_StackPull,
                OpDef.OpBIT_Abs,
                OpDef.OpAND_Abs,
                OpDef.OpROL_Abs,
                OpDef.OpAND_AbsLong,
                OpDef.OpBMI_PCRel,              // 0x30
                OpDef.OpAND_DPIndIndexY,
                OpDef.OpAND_DPInd,
                OpDef.OpAND_StackRelIndIndexY,
                OpDef.OpBIT_DPIndexX,
                OpDef.OpAND_DPIndexX,
                OpDef.OpROL_DPIndexX,
                OpDef.OpAND_DPIndIndexYLong,
                OpDef.OpSEC_Implied,            // 0x38
                OpDef.OpAND_AbsIndexY,
                OpDef.OpDEC_Acc,
                OpDef.OpTSC_Implied,
                OpDef.OpBIT_AbsIndexX,
                OpDef.OpAND_AbsIndexX,
                OpDef.OpROL_AbsIndexX,
                OpDef.OpAND_AbsIndexXLong,
                OpDef.OpRTI_StackRTI,           // 0x40
                OpDef.OpEOR_DPIndexXInd,
                OpDef.OpWDM_WDM,
                OpDef.OpEOR_StackRel,
                OpDef.OpMVP_BlockMove,
                OpDef.OpEOR_DP,
                OpDef.OpLSR_DP,
                OpDef.OpEOR_DPIndLong,
                OpDef.OpPHA_StackPush,          // 0x48
                OpDef.OpEOR_ImmLongA,
                OpDef.OpLSR_Acc,
                OpDef.OpPHK_StackPush,
                OpDef.OpJMP_Abs,
                OpDef.OpEOR_Abs,
                OpDef.OpLSR_Abs,
                OpDef.OpEOR_AbsLong,
                OpDef.OpBVC_PCRel,              // 0x50
                OpDef.OpEOR_DPIndIndexY,
                OpDef.OpEOR_DPInd,
                OpDef.OpEOR_StackRelIndIndexY,
                OpDef.OpMVN_BlockMove,
                OpDef.OpEOR_DPIndexX,
                OpDef.OpLSR_DPIndexX,
                OpDef.OpEOR_DPIndIndexYLong,
                OpDef.OpCLI_Implied,            // 0x58
                OpDef.OpEOR_AbsIndexY,
                OpDef.OpPHY_StackPush,
                OpDef.OpTCD_Implied,
                OpDef.OpJMP_AbsLong,
                OpDef.OpEOR_AbsIndexX,
                OpDef.OpLSR_AbsIndexX,
                OpDef.OpEOR_AbsIndexXLong,
                OpDef.OpRTS_StackRTS,           // 0x60
                OpDef.OpADC_DPIndexXInd,
                OpDef.OpPER_StackPCRelLong,
                OpDef.OpADC_StackRel,
                OpDef.OpSTZ_DP,
                OpDef.OpADC_DP,
                OpDef.OpROR_DP,
                OpDef.OpADC_DPIndLong,
                OpDef.OpPLA_StackPull,          // 0x68
                OpDef.OpADC_ImmLongA,
                OpDef.OpROR_Acc,
                OpDef.OpRTL_StackRTL,
                OpDef.OpJMP_AbsInd,
                OpDef.OpADC_Abs,
                OpDef.OpROR_Abs,
                OpDef.OpADC_AbsLong,
                OpDef.OpBVS_PCRel,              // 0x70
                OpDef.OpADC_DPIndIndexY,
                OpDef.OpADC_DPInd,
                OpDef.OpADC_StackRelIndIndexY,
                OpDef.OpSTZ_DPIndexX,
                OpDef.OpADC_DPIndexX,
                OpDef.OpROR_DPIndexX,
                OpDef.OpADC_DPIndIndexYLong,
                OpDef.OpSEI_Implied,            // 0x78
                OpDef.OpADC_AbsIndexY,
                OpDef.OpPLY_StackPull,
                OpDef.OpTDC_Implied,
                OpDef.OpJMP_AbsIndexXInd,
                OpDef.OpADC_AbsIndexX,
                OpDef.OpROR_AbsIndexX,
                OpDef.OpADC_AbsIndexXLong,
                OpDef.OpBRA_PCRel,              // 0x80
                OpDef.OpSTA_DPIndexXInd,
                OpDef.OpBRL_PCRelLong,
                OpDef.OpSTA_StackRel,
                OpDef.OpSTY_DP,
                OpDef.OpSTA_DP,
                OpDef.OpSTX_DP,
                OpDef.OpSTA_DPIndLong,
                OpDef.OpDEY_Implied,            // 0x88
                OpDef.OpBIT_ImmLongA,
                OpDef.OpTXA_Implied,
                OpDef.OpPHB_StackPush,
                OpDef.OpSTY_Abs,
                OpDef.OpSTA_Abs,
                OpDef.OpSTX_Abs,
                OpDef.OpSTA_AbsLong,
                OpDef.OpBCC_PCRel,              // 0x90
                OpDef.OpSTA_DPIndIndexY,
                OpDef.OpSTA_DPInd,
                OpDef.OpSTA_StackRelIndIndexY,
                OpDef.OpSTY_DPIndexX,
                OpDef.OpSTA_DPIndexX,
                OpDef.OpSTX_DPIndexY,
                OpDef.OpSTA_DPIndIndexYLong,
                OpDef.OpTYA_Implied,            // 0x98
                OpDef.OpSTA_AbsIndexY,
                OpDef.OpTXS_Implied,
                OpDef.OpTXY_Implied,
                OpDef.OpSTZ_Abs,
                OpDef.OpSTA_AbsIndexX,
                OpDef.OpSTZ_AbsIndexX,
                OpDef.OpSTA_AbsIndexXLong,
                OpDef.OpLDY_ImmLongXY,          // 0xa0
                OpDef.OpLDA_DPIndexXInd,
                OpDef.OpLDX_ImmLongXY,
                OpDef.OpLDA_StackRel,
                OpDef.OpLDY_DP,
                OpDef.OpLDA_DP,
                OpDef.OpLDX_DP,
                OpDef.OpLDA_DPIndLong,
                OpDef.OpTAY_Implied,            // 0xa8
                OpDef.OpLDA_ImmLongA,
                OpDef.OpTAX_Implied,
                OpDef.OpPLB_StackPull,
                OpDef.OpLDY_Abs,
                OpDef.OpLDA_Abs,
                OpDef.OpLDX_Abs,
                OpDef.OpLDA_AbsLong,
                OpDef.OpBCS_PCRel,              // 0xb0
                OpDef.OpLDA_DPIndIndexY,
                OpDef.OpLDA_DPInd,
                OpDef.OpLDA_StackRelIndIndexY,
                OpDef.OpLDY_DPIndexX,
                OpDef.OpLDA_DPIndexX,
                OpDef.OpLDX_DPIndexY,
                OpDef.OpLDA_DPIndIndexYLong,
                OpDef.OpCLV_Implied,            // 0xb8
                OpDef.OpLDA_AbsIndexY,
                OpDef.OpTSX_Implied,
                OpDef.OpTYX_Implied,
                OpDef.OpLDY_AbsIndexX,
                OpDef.OpLDA_AbsIndexX,
                OpDef.OpLDX_AbsIndexY,
                OpDef.OpLDA_AbsIndexXLong,
                OpDef.OpCPY_ImmLongXY,          // 0xc0
                OpDef.OpCMP_DPIndexXInd,
                OpDef.OpREP_Imm,
                OpDef.OpCMP_StackRel,
                OpDef.OpCPY_DP,
                OpDef.OpCMP_DP,
                OpDef.OpDEC_DP,
                OpDef.OpCMP_DPIndLong,
                OpDef.OpINY_Implied,            // 0xc8
                OpDef.OpCMP_ImmLongA,
                OpDef.OpDEX_Implied,
                OpDef.OpWAI_Implied,
                OpDef.OpCPY_Abs,
                OpDef.OpCMP_Abs,
                OpDef.OpDEC_Abs,
                OpDef.OpCMP_AbsLong,
                OpDef.OpBNE_PCRel,              // 0xd0
                OpDef.OpCMP_DPIndIndexY,
                OpDef.OpCMP_DPInd,
                OpDef.OpCMP_StackRelIndIndexY,
                OpDef.OpPEI_StackDPInd,
                OpDef.OpCMP_DPIndexX,
                OpDef.OpDEC_DPIndexX,
                OpDef.OpCMP_DPIndIndexYLong,
                OpDef.OpCLD_Implied,            // 0xd8
                OpDef.OpCMP_AbsIndexY,
                OpDef.OpPHX_StackPush,
                OpDef.OpSTP_Implied,
                OpDef.OpJMP_AbsIndLong,
                OpDef.OpCMP_AbsIndexX,
                OpDef.OpDEC_AbsIndexX,
                OpDef.OpCMP_AbsIndexXLong,
                OpDef.OpCPX_ImmLongXY,          // 0xe0
                OpDef.OpSBC_DPIndexXInd,
                OpDef.OpSEP_Imm,
                OpDef.OpSBC_StackRel,
                OpDef.OpCPX_DP,
                OpDef.OpSBC_DP,
                OpDef.OpINC_DP,
                OpDef.OpSBC_DPIndLong,
                OpDef.OpINX_Implied,            // 0xe8
                OpDef.OpSBC_ImmLongA,
                OpDef.OpNOP_Implied,
                OpDef.OpXBA_Implied,
                OpDef.OpCPX_Abs,
                OpDef.OpSBC_Abs,
                OpDef.OpINC_Abs,
                OpDef.OpSBC_AbsLong,
                OpDef.OpBEQ_PCRel,              // 0xf0
                OpDef.OpSBC_DPIndIndexY,
                OpDef.OpSBC_DPInd,
                OpDef.OpSBC_StackRelIndIndexY,
                OpDef.OpPEA_StackAbs,
                OpDef.OpSBC_DPIndexX,
                OpDef.OpINC_DPIndexX,
                OpDef.OpSBC_DPIndIndexYLong,
                OpDef.OpSED_Implied,            // 0xf8
                OpDef.OpSBC_AbsIndexY,
                OpDef.OpPLX_StackPull,
                OpDef.OpXCE_Implied,
                OpDef.OpJSR_AbsIndexXInd,
                OpDef.OpSBC_AbsIndexX,
                OpDef.OpINC_AbsIndexX,
                OpDef.OpSBC_AbsIndexXLong
            }
        };
    }
}
