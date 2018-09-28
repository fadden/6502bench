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
         brk   $ff

L1035    ora   ($ff,x)
         dfb   $02

L1038    dfb   $03,$ff
         dfb   $04,$ff
         ora   $ff
         asl   $ff
         dfb   $07,$ff
         php
         ora   #$ff
         asl   A
         dfb   $0b,$ff
         dfb   $0c,$ff,$fe
         ora   $feff
         asl   $feff
         dfb   $0f,$ff,$fe
         bpl   L1056
L1056    ora   ($ff),y
         dfb   $12

L1059    dfb   $13,$ff
         dfb   $14,$ff
         ora   $ff,x
         asl   $ff,x
         dfb   $17,$ff
         clc
         ora   $feff,y
         dfb   $1a
         dfb   $1b,$ff,$fe
         dfb   $1c,$ff,$fe
         ora   $feff,x
         asl   $feff,x
         dfb   $1f,$ff,$fe
         jsr   $feff
         and   ($ff,x)
         dfb   $22

L107D    dfb   $23,$ff
         bit   $ff
         and   $ff
         rol   $ff
         dfb   $27,$ff
         plp
         and   #$ff
         rol   A
         dfb   $2b,$ff
         bit   $feff
         and   $feff
         rol   $feff
         dfb   $2f,$ff,$fe
         bmi   L109B
L109B    and   ($ff),y
         dfb   $32

L109E    dfb   $33,$ff
         dfb   $34,$ff
         and   $ff,x
         rol   $ff,x
         dfb   $37,$ff
         sec
         and   $feff,y
         dfb   $3a
         dfb   $3b,$ff,$fe
         dfb   $3c,$ff,$fe
         and   $feff,x
         rol   $feff,x
         dfb   $3f,$ff,$fe
         rti

L10BD    eor   ($ff,x)
         dfb   $42

L10C0    dfb   $43,$ff
         dfb   $44,$ff
         eor   $ff
         lsr   $ff
         dfb   $47,$ff
         pha
         eor   #$ff
         lsr   A
         dfb   $4b,$ff
         jmp   L10D3

L10D3    eor   $feff
         lsr   $feff
         dfb   $4f,$ff,$fe
         bvc   L10DE
L10DE    eor   ($ff),y
         dfb   $52

L10E1    dfb   $53,$ff
         dfb   $54,$ff
         eor   $ff,x
         lsr   $ff,x
         dfb   $57,$ff
         cli
         eor   $feff,y
         dfb   $5a
         dfb   $5b,$ff,$fe
         dfb   $5c,$ff,$fe
         eor   $feff,x
         lsr   $feff,x
         dfb   $5f,$ff,$fe
         rts

L1100    adc   ($ff,x)
         dfb   $62

L1103    dfb   $63,$ff
         dfb   $64,$ff
         adc   $ff
         ror   $ff
         dfb   $67,$ff
         pla
         adc   #$ff
         ror   A
         dfb   $6b,$ff
         jmp   ($feff)

L1116    adc   $feff
         ror   $feff
         dfb   $6f,$ff,$fe
         bvs   L1121
L1121    adc   ($ff),y
         dfb   $72

L1124    dfb   $73,$ff
         dfb   $74,$ff
         adc   $ff,x
         ror   $ff,x
         dfb   $77,$ff
         sei
         adc   $feff,y
         dfb   $7a
         dfb   $7b,$ff,$fe
         dfb   $7c,$ff,$fe
         adc   $feff,x
         ror   $feff,x
         dfb   $7f,$ff,$fe
         dfb   $80,$ff
         sta   ($ff,x)
         dfb   $82,$ff
         dfb   $83,$ff
         sty   $ff
         sta   $ff
         stx   $ff
         dfb   $87,$ff
         dey
         dfb   $89,$ff
         txa
         dfb   $8b,$ff
         sty   $feff
         sta   $feff
         stx   $feff
         dfb   $8f,$ff,$fe
         bcc   L1166
L1166    sta   ($ff),y
         dfb   $92

L1169    dfb   $93,$ff
         sty   $ff,x
         sta   $ff,x
         stx   $ff,y
         dfb   $97,$ff
         tya
         sta   $feff,y
         txs
         dfb   $9b,$ff,$fe
         dfb   $9c,$ff,$fe
         sta   $feff,x
         dfb   $9e,$ff,$fe
         dfb   $9f,$ff,$fe
         ldy   #$ff
         lda   ($ff,x)
         ldx   #$ff
         dfb   $a3,$ff
         ldy   $ff
         lda   $ff
         ldx   $ff
         dfb   $a7,$ff
         tay
         lda   #$ff
         tax
         dfb   $ab,$ff
         ldy   $feff
         lda   $feff
         ldx   $feff
         dfb   $af,$ff,$fe
         bcs   L11AB
L11AB    lda   ($ff),y
         dfb   $b2

L11AE    dfb   $b3,$ff
         ldy   $ff,x
         lda   $ff,x
         ldx   $ff,y
         dfb   $b7,$ff
         clv
         lda   $feff,y
         tsx
         dfb   $bb,$ff,$fe
         ldy   $feff,x
         lda   $feff,x
         ldx   $feff,y
         dfb   $bf,$ff,$fe
         cpy   #$ff
         cmp   ($ff,x)
         dfb   $c2,$ff
         dfb   $c3,$ff
         cpy   $ff
         cmp   $ff
         dec   $ff
         dfb   $c7,$ff
         iny
         cmp   #$ff
         dex
         dfb   $cb,$ff
         cpy   $feff
         cmp   $feff
         dec   $feff
         dfb   $cf,$ff,$fe
         bne   L11F0
L11F0    cmp   ($ff),y
         dfb   $d2

L11F3    dfb   $d3,$ff
         dfb   $d4,$ff
         cmp   $ff,x
         dec   $ff,x
         dfb   $d7,$ff
         cld
         cmp   $feff,y
         dfb   $da
         dfb   $db,$ff,$fe
         dfb   $dc,$ff,$fe
         cmp   $feff,x
         dec   $feff,x
         dfb   $df,$ff,$fe
         cpx   #$ff
         sbc   ($ff,x)
         dfb   $e2,$ff
         dfb   $e3,$ff
         cpx   $ff
         sbc   $ff
         inc   $ff
         dfb   $e7,$ff
         inx
         sbc   #$ff
         nop
         dfb   $eb,$ff
         cpx   $feff
         sbc   $feff
         inc   $feff
         dfb   $ef,$ff,$fe
         beq   L1235
L1235    sbc   ($ff),y
         dfb   $f2

L1238    dfb   $f3,$ff
         dfb   $f4,$ff
         sbc   $ff,x
         inc   $ff,x
         dfb   $f7,$ff
         sed
         sbc   $feff,y
         dfb   $fa
         dfb   $fb,$ff,$fe
         dfb   $fc,$ff,$fe
         sbc   $feff,x
         inc   $feff,x
         dfb   $ff,$ff,$fe
