; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

         ORG   $1000

         SEC
         XCE
         JSR   L101F
         JSR   L10AB
         JSR   L10F2
         JSR   L1106
         JSR   L1109
         JSR   L112C
         JSR   L11F9
         JSR   L11FC
         NOP
         NOP
         NOP
         BRK   ZP
L101F    ORA   (ZP,X)
         COP   ZP
         ORA   ZP,S
         TSB   ZP
         ORA   ZP
         ASL   ZP
         ORA   [ZP]
         PHP
         ORA   #ZP
         ASL
         PHD
         TSB:  ABS
         ORA:  ABS
         ASL:  ABS
         ORAL  LONG
         BPL   L1041
L1041    ORA   (ZP),Y
         ORA   (ZP)
         ORA   (ZP,S),Y
         TRB   ZP
         ORA   ZP,X
         ASL   ZP,X
         ORA   [ZP],Y
         CLC
         ORA:  ABS,Y
         INC
         TCS
         TRB:  ABS
         ORA:  ABS,X
         ASL:  ABS,X
         ORAL  LONG,X
         JSR   ABS
         AND   (ZP,X)
         JSL   LONG
         AND   ZP,S
         BIT   ZP
         AND   ZP
         ROL   ZP
         AND   [ZP]
         PLP
         AND   #ZP
         ROL
         PLD
         BIT:  ABS
         AND:  ABS
         ROL:  ABS
         ANDL  LONG
         BMI   L1089
L1089    AND   (ZP),Y
         AND   (ZP)
         AND   (ZP,S),Y
         BIT   ZP,X
         AND   ZP,X
         ROL   ZP,X
         AND   [ZP],Y
         SEC
         AND:  ABS,Y
         DEC
         TSC
         BIT:  ABS,X
         AND:  ABS,X
         ROL:  ABS,X
         ANDL  LONG,X
         RTI
L10AB    EOR   (ZP,X)
         WDM   ZP
         EOR   ZP,S
         MVP   MV1,MV0
         EOR   ZP
         LSR   ZP
         EOR   [ZP]
         PHA
         EOR   #ZP
         LSR
         PHK
         JMP   L10C2
L10C2    EOR:  ABS
         LSR:  ABS
         EORL  LONG
         BVC   L10CE
L10CE    EOR   (ZP),Y
         EOR   (ZP)
         EOR   (ZP,S),Y
         MVN   MV1,MV0
         EOR   ZP,X
         LSR   ZP,X
         EOR   [ZP],Y
         CLI
         EOR:  ABS,Y
         PHY
         TCD
         JML   L10E7
L10E7    EOR:  ABS,X
         LSR:  ABS,X
         EORL  LONG,X
         RTS
L10F2    ADC   (ZP,X)
         PER   $0FF6        ;TODO: change
         ADC   ZP,S
         STZ   ZP
         ADC   ZP
         ROR   ZP
         ADC   [ZP]
         PLA
         ADC   #ZP
         ROR
         RTL
L1106    JMP   (ABS)
L1109    ADC:  ABS
         ROR:  ABS
         ADCL  LONG
         BVS   L1115
L1115    ADC   (ZP),Y
         ADC   (ZP)
         ADC   (ZP,S),Y
         STZ   ZP,X
         ADC   ZP,X
         ROR   ZP,X
         ADC   [ZP],Y
         SEI
         ADC:  ABS,Y
         PLY
         TDC
         JMP   (ABS,X)
L112C    ADC:  ABS,X
         ROR:  ABS,X
         ADCL  LONG,X
         BRA   L1138
L1138    STA   (ZP,X)
         BRL   L113D
L113D    STA   ZP,S
         STY   ZP
         STA   ZP
         STX   ZP
         STA   [ZP]
         DEY
         BIT   #ZP
         TXA
         PHB
         STY:  ABS
         STA:  ABS
         STX:  ABS
         STAL  LONG
         BCC   L115B
L115B    STA   (ZP),Y
         STA   (ZP)
         STA   (ZP,S),Y
         STY   ZP,X
         STA   ZP,X
         STX   ZP,Y
         STA   [ZP],Y
         TYA
         STA:  ABS,Y
         TXS
         TXY
         STZ:  ABS
         STA:  ABS,X
         STZ:  ABS,X
         STAL  LONG,X
         LDY   #ZP
         LDA   (ZP,X)
         LDX   #ZP
         LDA   ZP,S
         LDY   ZP
         LDA   ZP
         LDX   ZP
         LDA   [ZP]
         TAY
         LDA   #ZP
         TAX
         PLB
         LDY:  ABS
         LDA:  ABS
         LDX:  ABS
         LDAL  LONG
         BCS   L11A0
L11A0    LDA   (ZP),Y
         LDA   (ZP)
         LDA   (ZP,S),Y
         LDY   ZP,X
         LDA   ZP,X
         LDX   ZP,Y
         LDA   [ZP],Y
         CLV
         LDA:  ABS,Y
         TSX
         TYX
         LDY:  ABS,X
         LDA:  ABS,X
         LDX:  ABS,Y
         LDAL  LONG,X
         CPY   #ZP
         CMP   (ZP,X)
         REP   #$00
         CMP   ZP,S
         CPY   ZP
         CMP   ZP
         DEC   ZP
         CMP   [ZP]
         INY
         CMP   #ZP
         DEX
         WAI
         CPY:  ABS
         CMP:  ABS
         DEC:  ABS
         CMPL  LONG
         BNE   L11E5
L11E5    CMP   (ZP),Y
         CMP   (ZP)
         CMP   (ZP,S),Y
         PEI   ZP
         CMP   ZP,X
         DEC   ZP,X
         CMP   [ZP],Y
         CLD
         CMP:  ABS,Y
         PHX
         STP
L11F9    JML   [ABS]
L11FC    CMP:  ABS,X
         DEC:  ABS,X
         CMPL  LONG,X
         CPX   #ZP
         SBC   (ZP,X)
         SEP   #$00
         SBC   ZP,S
         CPX   ZP
         SBC   ZP
         INC   ZP
         SBC   [ZP]
         INX
         SBC   #ZP
         NOP
         XBA
         CPX:  ABS
         SBC:  ABS
         INC:  ABS
         SBCL  LONG
         BEQ   L122A
L122A    SBC   (ZP),Y
         SBC   (ZP)
         SBC   (ZP,S),Y
         PEA   ABS
         SBC   ZP,X
         INC   ZP,X
         SBC   [ZP],Y
         SED
         SBC:  ABS,Y
         PLX
         XCE
         JSR   (ABS,X)
         SBC:  ABS,X
         INC:  ABS,X
         SBCL  LONG,X
