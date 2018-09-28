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
         brk   $00

L1035    ora   ($00,x)
         dfb   $02

L1038    dfb   $03,$00
         dfb   $04,$00
         ora   $00
         asl   $00
         dfb   $07,$00
         php
         ora   #$00
         asl   A
         dfb   $0b,$00
         dfb   $0c,$00,$00
         ora:  $0000
         asl:  $0000
         dfb   $0f,$00,$00
         bpl   L1056
L1056    ora   ($00),y
         dfb   $12

L1059    dfb   $13,$00
         dfb   $14,$00
         ora   $00,x
         asl   $00,x
         dfb   $17,$00
         clc
         ora   $0000,y
         dfb   $1a
         dfb   $1b,$00,$00
         dfb   $1c,$00,$00
         ora:  $0000,x
         asl:  $0000,x
         dfb   $1f,$00,$00
         jsr   $0000
         and   ($00,x)
         dfb   $22

L107D    dfb   $23,$00
         bit   $00
         and   $00
         rol   $00
         dfb   $27,$00
         plp
         and   #$00
         rol   A
         dfb   $2b,$00
         bit:  $0000
         and:  $0000
         rol:  $0000
         dfb   $2f,$00,$00
         bmi   L109B
L109B    and   ($00),y
         dfb   $32

L109E    dfb   $33,$00
         dfb   $34,$00
         and   $00,x
         rol   $00,x
         dfb   $37,$00
         sec
         and   $0000,y
         dfb   $3a
         dfb   $3b,$00,$00
         dfb   $3c,$00,$00
         and:  $0000,x
         rol:  $0000,x
         dfb   $3f,$00,$00
         rti

L10BD    eor   ($00,x)
         dfb   $42

L10C0    dfb   $43,$00
         dfb   $44,$00
         eor   $00
         lsr   $00
         dfb   $47,$00
         pha
         eor   #$00
         lsr   A
         dfb   $4b,$00
         jmp   L10D3

L10D3    eor:  $0000
         lsr:  $0000
         dfb   $4f,$00,$00
         bvc   L10DE
L10DE    eor   ($00),y
         dfb   $52

L10E1    dfb   $53,$00
         dfb   $54,$00
         eor   $00,x
         lsr   $00,x
         dfb   $57,$00
         cli
         eor   $0000,y
         dfb   $5a
         dfb   $5b,$00,$00
         dfb   $5c,$00,$00
         eor:  $0000,x
         lsr:  $0000,x
         dfb   $5f,$00,$00
         rts

L1100    adc   ($00,x)
         dfb   $62

L1103    dfb   $63,$00
         dfb   $64,$00
         adc   $00
         ror   $00
         dfb   $67,$00
         pla
         adc   #$00
         ror   A
         dfb   $6b,$00
         jmp   ($0000)

L1116    adc:  $0000
         ror:  $0000
         dfb   $6f,$00,$00
         bvs   L1121
L1121    adc   ($00),y
         dfb   $72

L1124    dfb   $73,$00
         dfb   $74,$00
         adc   $00,x
         ror   $00,x
         dfb   $77,$00
         sei
         adc   $0000,y
         dfb   $7a
         dfb   $7b,$00,$00
         dfb   $7c,$00,$00
         adc:  $0000,x
         ror:  $0000,x
         dfb   $7f,$00,$00
         dfb   $80,$00
         sta   ($00,x)
         dfb   $82,$00
         dfb   $83,$00
         sty   $00
         sta   $00
         stx   $00
         dfb   $87,$00
         dey
         dfb   $89,$00
         txa
         dfb   $8b,$00
         sty:  $0000
         sta:  $0000
         stx:  $0000
         dfb   $8f,$00,$00
         bcc   L1166
L1166    sta   ($00),y
         dfb   $92

L1169    dfb   $93,$00
         sty   $00,x
         sta   $00,x
         stx   $00,y
         dfb   $97,$00
         tya
         sta   $0000,y
         txs
         dfb   $9b,$00,$00
         dfb   $9c,$00,$00
         sta:  $0000,x
         dfb   $9e,$00,$00
         dfb   $9f,$00,$00
         ldy   #$00
         lda   ($00,x)
         ldx   #$00
         dfb   $a3,$00
         ldy   $00
         lda   $00
         ldx   $00
         dfb   $a7,$00
         tay
         lda   #$00
         tax
         dfb   $ab,$00
         ldy:  $0000
         lda:  $0000
         ldx:  $0000
         dfb   $af,$00,$00
         bcs   L11AB
L11AB    lda   ($00),y
         dfb   $b2

L11AE    dfb   $b3,$00
         ldy   $00,x
         lda   $00,x
         ldx   $00,y
         dfb   $b7,$00
         clv
         lda   $0000,y
         tsx
         dfb   $bb,$00,$00
         ldy:  $0000,x
         lda:  $0000,x
         ldx:  $0000,y
         dfb   $bf,$00,$00
         cpy   #$00
         cmp   ($00,x)
         dfb   $c2,$00
         dfb   $c3,$00
         cpy   $00
         cmp   $00
         dec   $00
         dfb   $c7,$00
         iny
         cmp   #$00
         dex
         dfb   $cb,$00
         cpy:  $0000
         cmp:  $0000
         dec:  $0000
         dfb   $cf,$00,$00
         bne   L11F0
L11F0    cmp   ($00),y
         dfb   $d2

L11F3    dfb   $d3,$00
         dfb   $d4,$00
         cmp   $00,x
         dec   $00,x
         dfb   $d7,$00
         cld
         cmp   $0000,y
         dfb   $da
         dfb   $db,$00,$00
         dfb   $dc,$00,$00
         cmp:  $0000,x
         dec:  $0000,x
         dfb   $df,$00,$00
         cpx   #$00
         sbc   ($00,x)
         dfb   $e2,$00
         dfb   $e3,$00
         cpx   $00
         sbc   $00
         inc   $00
         dfb   $e7,$00
         inx
         sbc   #$00
         nop
         dfb   $eb,$00
         cpx:  $0000
         sbc:  $0000
         inc:  $0000
         dfb   $ef,$00,$00
         beq   L1235
L1235    sbc   ($00),y
         dfb   $f2

L1238    dfb   $f3,$00
         dfb   $f4,$00
         sbc   $00,x
         inc   $00,x
         dfb   $f7,$00
         sed
         sbc   $0000,y
         dfb   $fa
         dfb   $fb,$00,$00
         dfb   $fc,$00,$00
         sbc:  $0000,x
         inc:  $0000,x
         dfb   $ff,$00,$00
