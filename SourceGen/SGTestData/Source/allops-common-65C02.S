; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

         ORG   $1000

         JSR   PostBRK
         JSR   PostRTI
         JSR   PostRTS
         JSR   PostJMPI
         JSR   PostJMPX
         NOP
         NOP
         NOP
         BRK   ZP                   ;$00
PostBRK  ORA   (ZP,X)
         DFB   $02,ZP
         DFB   $03
         TSB   ZP
         ORA   ZP
         ASL   ZP
         DFB   $07
         PHP
         ORA   #ZP
         ASL
         DFB   $0B
         TSB:  ABS
         ORA:  ABS
         ASL:  ABS
         DFB   $0F
         BPL   PostBPL              ;$10
PostBPL  ORA   (ZP),Y
         ORA   (ZP)
         DFB   $13
         TRB   ZP
         ORA   ZP,X
         ASL   ZP,X
         DFB   $17
         CLC
         ORA:  ABS,Y
         INC
         DFB   $1B
         TRB:  ABS
         ORA:  ABS,X
         ASL:  ABS,X
         DFB   $1F
         JSR   ABS                  ;$20
         AND   (ZP,X)
         DFB   $22,ZP
         DFB   $23
         BIT   ZP
         AND   ZP
         ROL   ZP
         DFB   $27
         PLP
         AND   #ZP
         ROL
         DFB   $2B
         BIT:  ABS
         AND:  ABS
         ROL:  ABS
         DFB   $2F
         BMI   PostBMI              ;$30
PostBMI  AND   (ZP),Y
         AND   (ZP)
         DFB   $33
         BIT   ZP,X
         AND   ZP,X
         ROL   ZP,X
         DFB   $37
         SEC
         AND:  ABS,Y
         DEC
         DFB   $3B
         BIT:  ABS,X
         AND:  ABS,X
         ROL:  ABS,X
         DFB   $3F
         RTI                        ;$40
PostRTI  EOR   (ZP,X)
         DFB   $42,ZP
         DFB   $43
         DFB   $44,ZP
         EOR   ZP
         LSR   ZP
         DFB   $47
         PHA
         EOR   #ZP
         LSR
         DFB   $4B
         JMP   PostJMP
PostJMP  EOR:  ABS
         LSR:  ABS
         DFB   $4F
         BVC   PostBVC              ;$50
PostBVC  EOR   (ZP),Y
         EOR   (ZP)
         DFB   $53
         DFB   $54,ZP
         EOR   ZP,X
         LSR   ZP,X
         DFB   $57
         CLI
         EOR:  ABS,Y
         PHY
         DFB   $5B
         DFB   $5C,<ABS,>ABS
         EOR:  ABS,X
         LSR:  ABS,X
         DFB   $5F
         RTS                        ;$60
PostRTS  ADC   (ZP,X)
         DFB   $62,ZP
         DFB   $63
         STZ   ZP
         ADC   ZP
         ROR   ZP
         DFB   $67
         PLA
         ADC   #ZP
         ROR
         DFB   $6B
         JMP   (ABS)
PostJMPI ADC:  ABS
         ROR:  ABS
         DFB   $6F
         BVS   PostBVS              ;$70
PostBVS  ADC   (ZP),Y
         ADC   (ZP)
         DFB   $73
         STZ   ZP,X
         ADC   ZP,X
         ROR   ZP,X
         DFB   $77
         SEI
         ADC:  ABS,Y
         PLY
         DFB   $7B
         JMP   (ABS,X)
PostJMPX ADC:  ABS,X
         ROR:  ABS,X
         DFB   $7F
         BRA   PostBRA              ;$80
PostBRA  STA   (ZP,X)
         DFB   $82,ZP
         DFB   $83
         STY   ZP
         STA   ZP
         STX   ZP
         DFB   $87
         DEY
         BIT   #ZP
         TXA
         DFB   $8B
         STY:  ABS
         STA:  ABS
         STX:  ABS
         DFB   $8F
         BCC   PostBCC              ;$90
PostBCC  STA   (ZP),Y
         STA   (ZP)
         DFB   $93
         STY   ZP,X
         STA   ZP,X
         STX   ZP,Y
         DFB   $97
         TYA
         STA:  ABS,Y
         TXS
         DFB   $9B
         STZ:  ABS
         STA:  ABS,X
         STZ:  ABS,X
         DFB   $9F
         LDY   #ZP                  ;$A0
         LDA   (ZP,X)
         LDX   #ZP
         DFB   $A3
         LDY   ZP
         LDA   ZP
         LDX   ZP
         DFB   $A7
         TAY
         LDA   #ZP
         TAX
         DFB   $AB
         LDY:  ABS
         LDA:  ABS
         LDX:  ABS
         DFB   $AF
         BCS   PostBCS              ;$B0
PostBCS  LDA   (ZP),Y
         LDA   (ZP)
         DFB   $B3
         LDY   ZP,X
         LDA   ZP,X
         LDX   ZP,Y
         DFB   $B7
         CLV
         LDA:  ABS,Y
         TSX
         DFB   $BB
         LDY:  ABS,X
         LDA:  ABS,X
         LDX:  ABS,Y
         DFB   $BF
         CPY   #ZP                  ;$C0
         CMP   (ZP,X)
         DFB   $C2,ZP
         DFB   $C3
         CPY   ZP
         CMP   ZP
         DEC   ZP
         DFB   $C7
         INY
         CMP   #ZP
         DEX
         DFB   $CB
         CPY:  ABS
         CMP:  ABS
         DEC:  ABS
         DFB   $CF
         BNE   PostBNE              ;$D0
PostBNE  CMP   (ZP),Y
         CMP   (ZP)
         DFB   $D3
         DFB   $D4,ZP
         CMP   ZP,X
         DEC   ZP,X
         DFB   $D7
         CLD
         CMP:  ABS,Y
         PHX
         DFB   $DB
         DFB   $DC,<ABS,>ABS
         CMP:  ABS,X
         DEC:  ABS,X
         DFB   $DF
         CPX   #ZP                  ;$E0
         SBC   (ZP,X)
         DFB   $E2,ZP
         DFB   $E3
         CPX   ZP
         SBC   ZP
         INC   ZP
         DFB   $E7
         INX
         SBC   #ZP
         NOP
         DFB   $EB
         CPX:  ABS
         SBC:  ABS
         INC:  ABS
         DFB   $EF
         BEQ   PostBEQ              ;$F0
PostBEQ  SBC   (ZP),Y
         SBC   (ZP)
         DFB   $F3
         DFB   $F4,ZP
         SBC   ZP,X
         INC   ZP,X
         DFB   $F7
         SED
         SBC:  ABS,Y
         PLX
         DFB   $FB
         DFB   $FC,<ABS,>ABS
         SBC:  ABS,X
         INC:  ABS,X
         DFB   $FF
