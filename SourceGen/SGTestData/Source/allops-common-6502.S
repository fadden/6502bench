; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

         ORG   $1000

         JSR   PostBRK
         JSR   PostH02
         JSR   PostH12
         JSR   PostH22
         JSR   PostH32
         JSR   PostRTI
         JSR   PostH42
         JSR   PostH52
         JSR   PostRTS
         JSR   PostH62
         JSR   PostJMPI
         JSR   PostH72
         JSR   PostH92
         JSR   PostHB2
         JSR   PostHD2
         JSR   PostHF2
         NOP
         NOP
         NOP
         BRK   ZP                   ;$00
PostBRK  ORA   (ZP,X)
         DFB   $02
PostH02  DFB   $03,ZP
         DFB   $04,ZP
         ORA   ZP
         ASL   ZP
         DFB   $07,ZP
         PHP
         ORA   #ZP
         ASL
         DFB   $0B,ZP
         DFB   $0C,#<ABS,#>ABS
         ORA:  ABS
         ASL:  ABS
         DFB   $0F,#<ABS,#>ABS
         BPL   PostBPL              ;$10
PostBPL  ORA   (ZP),Y
         DFB   $12
PostH12  DFB   $13,ZP
         DFB   $14,ZP
         ORA   ZP,X
         ASL   ZP,X
         DFB   $17,ZP
         CLC
         ORA:  ABS,Y
         DFB   $1A
         DFB   $1B,#<ABS,#>ABS
         DFB   $1C,#<ABS,#>ABS
         ORA:  ABS,X
         ASL:  ABS,X
         DFB   $1F,#<ABS,#>ABS
         JSR   ABS                  ;$20
         AND   (ZP,X)
         DFB   $22
PostH22  DFB   $23,ZP
         BIT   ZP
         AND   ZP
         ROL   ZP
         DFB   $27,ZP
         PLP
         AND   #ZP
         ROL
         DFB   $2B,ZP
         BIT:  ABS
         AND:  ABS
         ROL:  ABS
         DFB   $2F,#<ABS,#>ABS
         BMI   PostBMI              ;$30
PostBMI  AND   (ZP),Y
         DFB   $32
PostH32  DFB   $33,ZP
         DFB   $34,ZP
         AND   ZP,X
         ROL   ZP,X
         DFB   $37,ZP
         SEC
         AND:  ABS,Y
         DFB   $3A
         DFB   $3B,#<ABS,#>ABS
         BIT:  ABS,X
         AND:  ABS,X
         ROL:  ABS,X
         DFB   $3F,#<ABS,#>ABS
         RTI                        ;$40
PostRTI  EOR   (ZP,X)
         DFB   $42
PostH42  DFB   $43,ZP
         DFB   $44,ZP
         EOR   ZP
         LSR   ZP
         DFB   $47,ZP
         PHA
         EOR   #ZP
         LSR
         DFB   $4B,ZP
         JMP   PostJMP
PostJMP  EOR:  ABS
         LSR:  ABS
         DFB   $4f,#<ABS,#>ABS
         BVC   PostBVC             ;$50
PostBVC  EOR   (ZP),Y
         DFB   $52
PostH52  DFB   $53,ZP
         DFB   $54,ZP
         EOR   ZP,X
         LSR   ZP,X
         DFB   $57,ZP
         CLI
         EOR:  ABS,Y
         DFB   $5A
         DFB   $5B,#<ABS,#>ABS
         DFB   $5C,#<ABS,#>ABS
         EOR:  ABS,X
         LSR:  ABS,X
         DFB   $5F,#<ABS,#>ABS
         RTS                        ;$60
PostRTS  ADC   (ZP,X)
         DFB   $62
PostH62  DFB   $63,ZP
         DFB   $64,ZP
         ADC   ZP
         ROR   ZP
         DFB   $67,ZP
         PLA
         ADC   #ZP
         ROR
         DFB   $6B,ZP
         JMP   (ABS)
PostJMPI ADC:  ABS
         ROR:  ABS
         DFB   $6F,#<ABS,#>ABS
         BVS   PostBVS              ;$70
PostBVS  ADC   (ZP),Y
         DFB   $72
PostH72  DFB   $73,ZP
         DFB   $74,ZP
         ADC   ZP,X
         ROR   ZP,X
         DFB   $77,ZP
         SEI
         ADC:  ABS,Y
         DFB   $7A
         DFB   $7B,#<ABS,#>ABS
         DFB   $7C,#<ABS,#>ABS
         ADC:  ABS,X
         ROR:  ABS,X
         DFB   $7F,#<ABS,#>ABS
         DFB   $80,ZP               ;$80
         STA   (ZP,X)
         DFB   $82,ZP
         DFB   $83,ZP
         STY   ZP
         STA   ZP
         STX   ZP
         DFB   $87,ZP
         DEY
         DFB   $89,ZP
         TXA
         DFB   $8B,ZP
         STY:  ABS
         STA:  ABS
         STX:  ABS
         DFB   $8F,#<ABS,#>ABS
         BCC   PostBCC              ;$90
PostBCC  STA   (ZP),Y
         DFB   $92
PostH92  DFB   $93,ZP
         STY   ZP,X
         STA   ZP,X
         STX   ZP,Y
         DFB   $97,ZP
         TYA
         STA:  ABS,Y
         TXS
         DFB   $9B,#<ABS,#>ABS
         DFB   $9C,#<ABS,#>ABS
         STA:  ABS,X
         DFB   $9E,#<ABS,#>ABS
         DFB   $9F,#<ABS,#>ABS
         LDY   #ZP                  ;$A0
         LDA   (ZP,X)
         LDX   #ZP
         DFB   $A3,ZP
         LDY   ZP
         LDA   ZP
         LDX   ZP
         DFB   $A7,ZP
         TAY
         LDA   #ZP
         TAX
         DFB   $AB,ZP
         LDY:  ABS
         LDA:  ABS
         LDX:  ABS
         DFB   $AF,#<ABS,#>ABS
         BCS   PostBCS              ;$B0
PostBCS  LDA   (ZP),Y
         DFB   $B2
PostHB2  DFB   $B3,ZP
         LDY   ZP,X
         LDA   ZP,X
         LDX   ZP,Y
         DFB   $B7,ZP
         CLV
         LDA:  ABS,Y
         TSX
         DFB   $BB,#<ABS,#>ABS
         LDY:  ABS,X
         LDA:  ABS,X
         LDX:  ABS,Y
         DFB   $BF,#<ABS,#>ABS
         CPY   #ZP                  ;$C0
         CMP   (ZP,X)
         DFB   $C2,ZP
         DFB   $C3,ZP
         CPY   ZP
         CMP   ZP
         DEC   ZP
         DFB   $C7,ZP
         INY
         CMP   #ZP
         DEX
         DFB   $CB,ZP
         CPY:  ABS
         CMP:  ABS
         DEC:  ABS
         DFB   $CF,#<ABS,#>ABS
         BNE   PostBNE              ;$D0
PostBNE  CMP   (ZP),Y
         DFB   $D2
PostHD2  DFB   $D3,ZP
         DFB   $D4,ZP
         CMP   ZP,X
         DEC   ZP,X
         DFB   $D7,ZP
         CLD
         CMP:  ABS,Y
         DFB   $DA
         DFB   $DB,#<ABS,#>ABS
         DFB   $DC,#<ABS,#>ABS
L11FC    CMP:  ABS,X
         DEC:  ABS,X
         DFB   $DF,#<ABS,#>ABS
         CPX   #ZP                  ;$E0
         SBC   (ZP,X)
         DFB   $E2,ZP
         DFB   $E3,ZP
         CPX   ZP
         SBC   ZP
         INC   ZP
         DFB   $E7,ZP
         INX
         SBC   #ZP
         NOP
         DFB   $EB,ZP
         CPX:  ABS
         SBC:  ABS
         INC:  ABS
         DFB   $EF,#<ABS,#>ABS
         BEQ   PostBEQ              ;$F0
PostBEQ  SBC   (ZP),Y
         DFB   $F2
PostHF2  DFB   $F3,ZP
         DFB   $F4,ZP
         SBC   ZP,X
         INC   ZP,X
         DFB   $F7,ZP
         SED
         SBC:  ABS,Y
         DFB   $FA
         DFB   $FB,#<ABS,#>ABS
         DFB   $FC,#<ABS,#>ABS
         SBC:  ABS,X
         INC:  ABS,X
         DFB   $FF,#<ABS,#>ABS

