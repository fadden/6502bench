         org   $1000
         jsr   L1035
         jsr   L1038
         jsr   L1059
         jsr   L107D
         jsr   L109E
         jsr   L10BD
         jsr   L10C0
         jsr   L10E1
         jsr   L1100
         jsr   L1103
         jsr   L1116
         jsr   L1124
         jsr   L1169
         jsr   L11AE
         jsr   L11F3
         jsr   L1238
         nop
         nop
         nop
         brk

         dfb   $80

L1035    dfb   $01,$80
         dfb   $02

L1038    dfb   $03,$80
         dfb   $04,$80
         ora   L0080
         asl   L0080
         dfb   $07,$80
         php
         ora   #$80
         asl   A
         dfb   $0b,$80
         dfb   $0c,$86,$00
         ora:  L0086
         asl:  L0086
         dfb   $0f,$86,$00
         bpl   :L1056
:L1056   ora   (L0080),y
         dfb   $12

L1059    dfb   $13,$80
         dfb   $14,$80
         ora   L0080,x
         asl   L0080,x
         dfb   $17,$80
         clc
         ora   L0086,y
         dfb   $1a
         dfb   $1b,$86,$00
         dfb   $1c,$86,$00
         ora:  L0086,x
         asl:  L0086,x
         dfb   $1f,$86,$00
         jsr   L0086
         dfb   $21,$80
         dfb   $22

L107D    dfb   $23,$80
         bit   L0080
         and   L0080
         rol   L0080
         dfb   $27,$80
         plp
         and   #$80
         rol   A
         dfb   $2b,$80
         bit:  L0086
         and:  L0086
         rol:  L0086
         dfb   $2f,$86,$00
         bmi   :L109B
:L109B   and   (L0080),y
         dfb   $32

L109E    dfb   $33,$80
         dfb   $34,$80
         and   L0080,x
         rol   L0080,x
         dfb   $37,$80
         sec
         and   L0086,y
         dfb   $3a
         dfb   $3b,$86,$00
         dfb   $3c,$86,$00
         and:  L0086,x
         rol:  L0086,x
         dfb   $3f,$86,$00
         rti

L10BD    dfb   $41,$80
         dfb   $42

L10C0    dfb   $43,$80
         dfb   $44,$80
         eor   L0080
         lsr   L0080
         dfb   $47,$80
         pha
         eor   #$80
         lsr   A
         dfb   $4b,$80
         jmp   :L10D3

:L10D3   eor:  L0086
         lsr:  L0086
         dfb   $4f,$86,$00
         bvc   :L10DE
:L10DE   eor   (L0080),y
         dfb   $52

L10E1    dfb   $53,$80
         dfb   $54,$80
         eor   L0080,x
         lsr   L0080,x
         dfb   $57,$80
         cli
         eor   L0086,y
         dfb   $5a
         dfb   $5b,$86,$00
         dfb   $5c,$86,$00
         eor:  L0086,x
         lsr:  L0086,x
         dfb   $5f,$86,$00
         rts

L1100    dfb   $61,$80
         dfb   $62

L1103    dfb   $63,$80
         dfb   $64,$80
         adc   L0080
         ror   L0080
         dfb   $67,$80
         pla
         adc   #$80
         ror   A
         dfb   $6b,$80
         jmp   (L0086)

L1116    adc:  L0086
         ror:  L0086
         dfb   $6f,$86,$00
         bvs   :L1121
:L1121   adc   (L0080),y
         dfb   $72

L1124    dfb   $73,$80
         dfb   $74,$80
         adc   L0080,x
         ror   L0080,x
         dfb   $77,$80
         sei
         adc   L0086,y
         dfb   $7a
         dfb   $7b,$86,$00
         dfb   $7c,$86,$00
         adc:  L0086,x
         ror:  L0086,x
         dfb   $7f,$86,$00
         dfb   $80,$80
         dfb   $81,$80
         dfb   $82,$80
         dfb   $83,$80
         sty   L0080
         sta   L0080
         stx   L0080
         dfb   $87,$80
         dey
         dfb   $89,$80
         txa
         dfb   $8b,$80
         sty:  L0086
         sta:  L0086
         stx:  L0086
         dfb   $8f,$86,$00
         bcc   :L1166
:L1166   sta   (L0080),y
         dfb   $92

L1169    dfb   $93,$80
         dfb   $94,$80
         sta   L0080,x
         dfb   $96,$80
         dfb   $97,$80
         tya
         sta   L0086,y
         txs
         dfb   $9b,$86,$00
         dfb   $9c,$86,$00
         sta:  L0086,x
         dfb   $9e,$86,$00
         dfb   $9f,$86,$00
         ldy   #$80
         dfb   $a1,$80
         ldx   #$80
         dfb   $a3,$80
         ldy   L0080
         lda   L0080
         ldx   L0080
         dfb   $a7,$80
         tay
         lda   #$80
         tax
         dfb   $ab,$80
         ldy:  L0086
         lda:  L0086
         ldx:  L0086
         dfb   $af,$86,$00
         bcs   :L11AB
:L11AB   lda   (L0080),y
         dfb   $b2

L11AE    dfb   $b3,$80
         ldy   L0080,x
         lda   L0080,x
         ldx   L0080,y
         dfb   $b7,$80
         clv
         lda   L0086,y
         tsx
         dfb   $bb,$86,$00
         ldy:  L0086,x
         lda:  L0086,x
         ldx:  L0086,y
         dfb   $bf,$86,$00
         cpy   #$80
         dfb   $c1,$80
         dfb   $c2,$80
         dfb   $c3,$80
         cpy   L0080
         cmp   L0080
         dec   L0080
         dfb   $c7,$80
         iny
         cmp   #$80
         dex
         dfb   $cb,$80
         cpy:  L0086
         cmp:  L0086
         dec:  L0086
         dfb   $cf,$86,$00
         bne   :L11F0
:L11F0   cmp   (L0080),y
         dfb   $d2

L11F3    dfb   $d3,$80
         dfb   $d4,$80
         cmp   L0080,x
         dec   L0080,x
         dfb   $d7,$80
         cld
         cmp   L0086,y
         dfb   $da
         dfb   $db,$86,$00
         dfb   $dc,$86,$00
         cmp:  L0086,x
         dec:  L0086,x
         dfb   $df,$86,$00
         cpx   #$80
         dfb   $e1,$80
         dfb   $e2,$80
         dfb   $e3,$80
         cpx   L0080
         sbc   L0080
         inc   L0080
         dfb   $e7,$80
         inx
         sbc   #$80
         nop
         dfb   $eb,$80
         cpx:  L0086
         sbc:  L0086
         inc:  L0086
         dfb   $ef,$86,$00
         beq   :L1235
:L1235   sbc   (L0080),y
         dfb   $f2

L1238    dfb   $f3,$80
         dfb   $f4,$80
         sbc   L0080,x
         inc   L0080,x
         dfb   $f7,$80
         sed
         sbc   L0086,y
         dfb   $fa
         dfb   $fb,$86,$00
         dfb   $fc,$86,$00
         sbc:  L0086,x
         inc:  L0086,x
         dfb   $ff,$86,$00
         org   $0080
L0080    bit   :L0082
:L0082   bit   :L0082
         bit   :L0082
L0086    bit:  L0086
