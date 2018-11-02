;6502bench SourceGen v1.1.0-dev1
         org   $1000
         sec
         xce
         jsr   L101F
         jsr   L10AB
         jsr   L10F2
         jsr   L1106
         jsr   L1109
         jsr   L112C
         jsr   L11F9
         jsr   L11FC
         nop
         nop
         nop
         brk   $80

L101F    dfb   $01,$80
         cop   $80
         ora   $80,S
         tsb   L0080
         ora   L0080
         asl   L0080
         dfb   $07,$80
         php
         ora   #$80
         asl   A
         phd
         tsb:  L0086
         ora:  L0086
         asl:  L0086
         oral  L0089
         bpl   L1041
L1041    ora   (L0080),y
         dfb   $12,$80
         ora   ($80,S),y
         trb   L0080
         ora   L0080,x
         asl   L0080,x
         ora   [L0080],y
         clc
         ora   L0086,y
         inc   A
         tcs
         trb:  L0086
         ora:  L0086,x
         asl:  L0086,x
         oral  L0089,x
         jsr   L0086
         dfb   $21,$80
         jsl   L0089
         and   $80,S
         bit   L0080
         and   L0080
         rol   L0080
         dfb   $27,$80
         plp
         and   #$80
         rol   A
         pld
         bit:  L0086
         and:  L0086
         rol:  L0086
         andl  L0089
         bmi   L1089
L1089    and   (L0080),y
         dfb   $32,$80
         and   ($80,S),y
         bit   L0080,x
         and   L0080,x
         rol   L0080,x
         and   [L0080],y
         sec
         and   L0086,y
         dec   A
         tsc
         bit:  L0086,x
         and:  L0086,x
         rol:  L0086,x
         andl  L0089,x
         rti

L10AB    dfb   $41,$80
         wdm   $80
         eor   $80,S
         mvp   $84,$83
         eor   L0080
         lsr   L0080
         dfb   $47,$80
         pha
         eor   #$80
         lsr   A
         phk
         jmp   L10C2

L10C2    eor:  L0086
         lsr:  L0086
         eorl  L0089
         bvc   L10CE
L10CE    eor   (L0080),y
         dfb   $52,$80
         eor   ($80,S),y
         mvn   $84,$83
         eor   L0080,x
         lsr   L0080,x
         eor   [L0080],y
         cli
         eor   L0086,y
         phy
         tcd
         jml   L10E7

L10E7    eor:  L0086,x
         lsr:  L0086,x
         eorl  L0089,x
         rts

L10F2    dfb   $61,$80
         per   $0ff6
         adc   $80,S
         stz   L0080
         adc   L0080
         ror   L0080
         dfb   $67,$80
         pla
         adc   #$80
         ror   A
         rtl

L1106    jmp   (L0086)

L1109    adc:  L0086
         ror:  L0086
         adcl  L0089
         bvs   L1115
L1115    adc   (L0080),y
         dfb   $72,$80
         adc   ($80,S),y
         stz   L0080,x
         adc   L0080,x
         ror   L0080,x
         adc   [L0080],y
         sei
         adc   L0086,y
         ply
         tdc
         jmp   (L0086,x)

L112C    adc:  L0086,x
         ror:  L0086,x
         adcl  L0089,x
         bra   L1138

L1138    dfb   $81,$80
         brl   L113D

L113D    sta   $80,S
         sty   L0080
         sta   L0080
         stx   L0080
         dfb   $87,$80
         dey
         bit   #$80
         txa
         phb
         sty:  L0086
         sta:  L0086
         stx:  L0086
         stal  L0089
         bcc   L115B
L115B    sta   (L0080),y
         dfb   $92,$80
         sta   ($80,S),y
         dfb   $94,$80
         sta   L0080,x
         dfb   $96,$80
         sta   [L0080],y
         tya
         sta   L0086,y
         txs
         txy
         stz:  L0086
         sta:  L0086,x
         stz:  L0086,x
         stal  L0089,x
         ldy   #$80
         dfb   $a1,$80
         ldx   #$80
         lda   $80,S
         ldy   L0080
         lda   L0080
         ldx   L0080
         dfb   $a7,$80
         tay
         lda   #$80
         tax
         plb
         ldy:  L0086
         lda:  L0086
         ldx:  L0086
         ldal  L0089
         bcs   L11A0
L11A0    lda   (L0080),y
         dfb   $b2,$80
         lda   ($80,S),y
         ldy   L0080,x
         lda   L0080,x
         ldx   L0080,y
         lda   [L0080],y
         clv
         lda   L0086,y
         tsx
         tyx
         ldy:  L0086,x
         lda:  L0086,x
         ldx:  L0086,y
         ldal  L0089,x
         cpy   #$80
         dfb   $c1,$80
         rep   #$00
         cmp   $80,S
         cpy   L0080
         cmp   L0080
         dec   L0080
         dfb   $c7,$80
         iny
         cmp   #$80
         dex
         wai
         cpy:  L0086
         cmp:  L0086
         dec:  L0086
         cmpl  L0089
         bne   L11E5
L11E5    cmp   (L0080),y
         dfb   $d2,$80
         cmp   ($80,S),y
         dfb   $d4,$80
         cmp   L0080,x
         dec   L0080,x
         cmp   [L0080],y
         cld
         cmp   L0086,y
         phx
         stp

L11F9    jml   [L0086]

L11FC    cmp:  L0086,x
         dec:  L0086,x
         cmpl  L0089,x
         cpx   #$80
         dfb   $e1,$80
         sep   #$00
         sbc   $80,S
         cpx   L0080
         sbc   L0080
         inc   L0080
         dfb   $e7,$80
         inx
         sbc   #$80
         nop
         xba
         cpx:  L0086
         sbc:  L0086
         inc:  L0086
         sbcl  L0089
         beq   L122A
L122A    sbc   (L0080),y
         dfb   $f2,$80
         sbc   ($80,S),y
         pea   L0086
         sbc   L0080,x
         inc   L0080,x
         sbc   [L0080],y
         sed
         sbc   L0086,y
         plx
         xce
         jsr   (L0086,x)
         sbc:  L0086,x
         inc:  L0086,x
         sbcl  L0089,x
         org   $0080
L0080    bit   L0082
L0082    bit   L0082
         bit   L0082
L0086    bit:  L0086
L0089    ldal  L0089
