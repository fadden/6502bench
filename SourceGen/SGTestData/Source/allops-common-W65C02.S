; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; Macros for Rockwell extensions.  We use the 3-arg format to
; cut down on the code size.
BBR      MAC                        ;BBR bit,zp,label
         DFB   ]1*16+$0f
         DFB   ]2
         DFB   ]3-*-1
         <<<
BBS      MAC                        ;BBS bit,zp,label
         DFB   ]1*16+$8f
         DFB   ]2
         DFB   ]3-*-1
         <<<
RMB      MAC                        ;RMB bit,zp
         DFB   ]1*16+$07
         DFB   ]2
         <<<
SMB      MAC                        ;SMB bit,zp
         DFB   ]1*16+$87
         DFB   ]2
         <<<

         ORG   $1000

         JSR   PostBRK
         JSR   PostRTI
         JSR   PostRTS
         JSR   PostJMPI
         JSR   PostJMPX
         JSR   PostSTP
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
         RMB   0;ZP
         PHP
         ORA   #ZP
         ASL
         DFB   $0B
         TSB:  ABS
         ORA:  ABS
         ASL:  ABS
         BBR   0;ZP;PostBBR0
PostBBR0 BPL   PostBPL              ;$10
PostBPL  ORA   (ZP),Y
         ORA   (ZP)
         DFB   $13
         TRB   ZP
         ORA   ZP,X
         ASL   ZP,X
         RMB   1;ZP
         CLC
         ORA:  ABS,Y
         INC
         DFB   $1B
         TRB:  ABS
         ORA:  ABS,X
         ASL:  ABS,X
         BBR   1;ZP;PostBBR1
PostBBR1 JSR   ABS                  ;$20
         AND   (ZP,X)
         DFB   $22,ZP
         DFB   $23
         BIT   ZP
         AND   ZP
         ROL   ZP
         RMB   2;ZP
         PLP
         AND   #ZP
         ROL
         DFB   $2B
         BIT:  ABS
         AND:  ABS
         ROL:  ABS
         BBR   2;ZP;PostBBR2
PostBBR2 BMI   PostBMI              ;$30
PostBMI  AND   (ZP),Y
         AND   (ZP)
         DFB   $33
         BIT   ZP,X
         AND   ZP,X
         ROL   ZP,X
         RMB   3;ZP
         SEC
         AND:  ABS,Y
         DEC
         DFB   $3B
         BIT:  ABS,X
         AND:  ABS,X
         ROL:  ABS,X
         BBR   3;ZP;PostBBR3
PostBBR3 RTI                        ;$40
PostRTI  EOR   (ZP,X)
         DFB   $42,ZP
         DFB   $43
         DFB   $44,ZP
         EOR   ZP
         LSR   ZP
         RMB   4;ZP
         PHA
         EOR   #ZP
         LSR
         DFB   $4B
         JMP   PostJMP
PostJMP  EOR:  ABS
         LSR:  ABS
         BBR   4;ZP;PostBBR4
PostBBR4 BVC   PostBVC              ;$50
PostBVC  EOR   (ZP),Y
         EOR   (ZP)
         DFB   $53
         DFB   $54,ZP
         EOR   ZP,X
         LSR   ZP,X
         RMB   5;ZP
         CLI
         EOR:  ABS,Y
         PHY
         DFB   $5B
         DFB   $5C,<ABS,>ABS
         EOR:  ABS,X
         LSR:  ABS,X
         BBR   5;ZP;PostBBR5
PostBBR5 RTS                        ;$60
PostRTS  ADC   (ZP,X)
         DFB   $62,ZP
         DFB   $63
         STZ   ZP
         ADC   ZP
         ROR   ZP
         RMB   6;ZP
         PLA
         ADC   #ZP
         ROR
         DFB   $6B
         JMP   (ABS)
PostJMPI ADC:  ABS
         ROR:  ABS
         BBR   6;ZP;PostBBR6
PostBBR6 BVS   PostBVS              ;$70
PostBVS  ADC   (ZP),Y
         ADC   (ZP)
         DFB   $73
         STZ   ZP,X
         ADC   ZP,X
         ROR   ZP,X
         RMB   7;ZP
         SEI
         ADC:  ABS,Y
         PLY
         DFB   $7B
         JMP   (ABS,X)
PostJMPX ADC:  ABS,X
         ROR:  ABS,X
         BBR   7;ZP;PostBBR7
PostBBR7 BRA   PostBRA              ;$80
PostBRA  STA   (ZP,X)
         DFB   $82,ZP
         DFB   $83
         STY   ZP
         STA   ZP
         STX   ZP
         SMB   0;ZP
         DEY
         BIT   #ZP
         TXA
         DFB   $8B
         STY:  ABS
         STA:  ABS
         STX:  ABS
         BBS   0;ZP;PostBBS0
PostBBS0 BCC   PostBCC              ;$90
PostBCC  STA   (ZP),Y
         STA   (ZP)
         DFB   $93
         STY   ZP,X
         STA   ZP,X
         STX   ZP,Y
         SMB   1;ZP
         TYA
         STA:  ABS,Y
         TXS
         DFB   $9B
         STZ:  ABS
         STA:  ABS,X
         STZ:  ABS,X
         BBS   1;ZP;PostBBS1
PostBBS1 LDY   #ZP                  ;$A0
         LDA   (ZP,X)
         LDX   #ZP
         DFB   $A3
         LDY   ZP
         LDA   ZP
         LDX   ZP
         SMB   2;ZP
         TAY
         LDA   #ZP
         TAX
         DFB   $AB
         LDY:  ABS
         LDA:  ABS
         LDX:  ABS
         BBS   2;ZP;PostBBS2
PostBBS2 BCS   PostBCS              ;$B0
PostBCS  LDA   (ZP),Y
         LDA   (ZP)
         DFB   $B3
         LDY   ZP,X
         LDA   ZP,X
         LDX   ZP,Y
         SMB   3;ZP
         CLV
         LDA:  ABS,Y
         TSX
         DFB   $BB
         LDY:  ABS,X
         LDA:  ABS,X
         LDX:  ABS,Y
         BBS   3;ZP;PostBBS3
PostBBS3 CPY   #ZP                  ;$C0
         CMP   (ZP,X)
         DFB   $C2,ZP
         DFB   $C3
         CPY   ZP
         CMP   ZP
         DEC   ZP
         SMB   4;ZP
         INY
         CMP   #ZP
         DEX
         WAI
         CPY:  ABS
         CMP:  ABS
         DEC:  ABS
         BBS   4;ZP;PostBBS4
PostBBS4 BNE   PostBNE              ;$D0
PostBNE  CMP   (ZP),Y
         CMP   (ZP)
         DFB   $D3
         DFB   $D4,ZP
         CMP   ZP,X
         DEC   ZP,X
         SMB   5;ZP
         CLD
         CMP:  ABS,Y
         PHX
         STP
PostSTP  DFB   $DC,<ABS,>ABS
         CMP:  ABS,X
         DEC:  ABS,X
         BBS   5;ZP;PostBBS5
PostBBS5 CPX   #ZP                  ;$E0
         SBC   (ZP,X)
         DFB   $E2,ZP
         DFB   $E3
         CPX   ZP
         SBC   ZP
         INC   ZP
         SMB   6;ZP
         INX
         SBC   #ZP
         NOP
         DFB   $EB
         CPX:  ABS
         SBC:  ABS
         INC:  ABS
         BBS   6;ZP;PostBBS6
PostBBS6 BEQ   PostBEQ              ;$F0
PostBEQ  SBC   (ZP),Y
         SBC   (ZP)
         DFB   $F3
         DFB   $F4,ZP
         SBC   ZP,X
         INC   ZP,X
         SMB   7;ZP
         SED
         SBC:  ABS,Y
         PLX
         DFB   $FB
         DFB   $FC,<ABS,>ABS
         SBC:  ABS,X
         INC:  ABS,X
         BBS   7;ZP;PostBBS7

PostBBS7 rts
