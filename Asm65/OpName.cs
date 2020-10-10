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

namespace Asm65 {
    /// <summary>
    /// String constants for opcodes.  These are not (and should not be) localized.  They
    /// must be lower-case.
    /// </summary>
    public static class OpName {
        // NOTE: these are generally three characters, but the pattern breaks with the Rockwell
        // extensions unless we want to show the bit index as a 3rd argument (which some
        // assemblers do).  On the 65816 some mnemonics are extended (e.g. LDAL for LDA with
        // a 24-bit operand), but that's assembler-specific and handled elsewhere.
        //
        // Bottom line: don't assume these will be 3 characters.
        public const string Unknown = "???";
        public const string ADC = "adc";
        public const string AND = "and";
        public const string ASL = "asl";
        public const string BCC = "bcc";
        public const string BCS = "bcs";
        public const string BEQ = "beq";
        public const string BIT = "bit";
        public const string BMI = "bmi";
        public const string BNE = "bne";
        public const string BPL = "bpl";
        public const string BRA = "bra";
        public const string BRK = "brk";
        public const string BRL = "brl";
        public const string BVC = "bvc";
        public const string BVS = "bvs";
        public const string CLC = "clc";
        public const string CLD = "cld";
        public const string CLI = "cli";
        public const string CLV = "clv";
        public const string CMP = "cmp";
        public const string COP = "cop";
        public const string CPX = "cpx";
        public const string CPY = "cpy";
        public const string DEC = "dec";
        public const string DEX = "dex";
        public const string DEY = "dey";
        public const string EOR = "eor";
        public const string INC = "inc";
        public const string INX = "inx";
        public const string INY = "iny";
        public const string JML = "jml";
        public const string JMP = "jmp";
        public const string JSL = "jsl";
        public const string JSR = "jsr";
        public const string LDA = "lda";
        public const string LDX = "ldx";
        public const string LDY = "ldy";
        public const string LSR = "lsr";
        public const string MVN = "mvn";
        public const string MVP = "mvp";
        public const string NOP = "nop";
        public const string ORA = "ora";
        public const string PEA = "pea";
        public const string PEI = "pei";
        public const string PER = "per";
        public const string PHA = "pha";
        public const string PHB = "phb";
        public const string PHD = "phd";
        public const string PHK = "phk";
        public const string PHP = "php";
        public const string PHX = "phx";
        public const string PHY = "phy";
        public const string PLA = "pla";
        public const string PLB = "plb";
        public const string PLD = "pld";
        public const string PLP = "plp";
        public const string PLX = "plx";
        public const string PLY = "ply";
        public const string REP = "rep";
        public const string ROL = "rol";
        public const string ROR = "ror";
        public const string RTI = "rti";
        public const string RTL = "rtl";
        public const string RTS = "rts";
        public const string SBC = "sbc";
        public const string SEC = "sec";
        public const string SED = "sed";
        public const string SEI = "sei";
        public const string SEP = "sep";
        public const string STA = "sta";
        public const string STP = "stp";
        public const string STX = "stx";
        public const string STY = "sty";
        public const string STZ = "stz";
        public const string TAX = "tax";
        public const string TAY = "tay";
        public const string TCD = "tcd";
        public const string TCS = "tcs";
        public const string TDC = "tdc";
        public const string TRB = "trb";
        public const string TSB = "tsb";
        public const string TSC = "tsc";
        public const string TSX = "tsx";
        public const string TXA = "txa";
        public const string TXS = "txs";
        public const string TXY = "txy";
        public const string TYA = "tya";
        public const string TYX = "tyx";
        public const string WAI = "wai";
        public const string WDM = "wdm";
        public const string XBA = "xba";
        public const string XCE = "xce";

        // Undocumented 6502 instructions.
        public const string ANC = "anc";
        public const string ANE = "ane";
        public const string ALR = "alr";
        public const string ARR = "arr";
        public const string DCP = "dcp";
        public const string DOP = "dop";
        public const string ISC = "isc";
        public const string JAM = "jam";
        public const string LAS = "las";
        public const string LAX = "lax";
        public const string RLA = "rla";
        public const string RRA = "rra";
        public const string SAX = "sax";
        public const string SBX = "sbx";
        public const string SHA = "sha";
        public const string SHX = "shx";
        public const string SHY = "shy";
        public const string SLO = "slo";
        public const string SRE = "sre";
        public const string TAS = "tas";
        public const string TOP = "top";

        // Undocumented 65C02 instructions.
        public const string LDD = "ldd";

        // Rockwell extensions
        public const string BBR0 = "bbr0";
        public const string BBR1 = "bbr1";
        public const string BBR2 = "bbr2";
        public const string BBR3 = "bbr3";
        public const string BBR4 = "bbr4";
        public const string BBR5 = "bbr5";
        public const string BBR6 = "bbr6";
        public const string BBR7 = "bbr7";
        public const string BBS0 = "bbs0";
        public const string BBS1 = "bbs1";
        public const string BBS2 = "bbs2";
        public const string BBS3 = "bbs3";
        public const string BBS4 = "bbs4";
        public const string BBS5 = "bbs5";
        public const string BBS6 = "bbs6";
        public const string BBS7 = "bbs7";
        public const string RMB0 = "rmb0";
        public const string RMB1 = "rmb1";
        public const string RMB2 = "rmb2";
        public const string RMB3 = "rmb3";
        public const string RMB4 = "rmb4";
        public const string RMB5 = "rmb5";
        public const string RMB6 = "rmb6";
        public const string RMB7 = "rmb7";
        public const string SMB0 = "smb0";
        public const string SMB1 = "smb1";
        public const string SMB2 = "smb2";
        public const string SMB3 = "smb3";
        public const string SMB4 = "smb4";
        public const string SMB5 = "smb5";
        public const string SMB6 = "smb6";
        public const string SMB7 = "smb7";
    }
}
