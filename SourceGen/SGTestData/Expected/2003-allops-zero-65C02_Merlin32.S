         org   $1000
         jsr   L1014
         jsr   L108A
         jsr   L10C4
         jsr   L10D8
         jsr   L10F6
         nop
         nop
         nop
         brk   $00

L1014    ora   ($00,x)
         dfb   $02,$00
         dfb   $03
         tsb   $00
         ora   $00
         asl   $00
         dfb   $07
         php
         ora   #$00
         asl   A
         dfb   $0b
         tsb:  $0000
         ora:  $0000
         asl:  $0000
         dfb   $0f
         bpl   L1031
L1031    ora   ($00),y
         ora   ($00)
         dfb   $13
         trb   $00
         ora   $00,x
         asl   $00,x
         dfb   $17
         clc
         ora   $0000,y
         inc   A
         dfb   $1b
         trb:  $0000
         ora:  $0000,x
         asl:  $0000,x
         dfb   $1f
         jsr   $0000
         and   ($00,x)
         dfb   $22,$00
         dfb   $23
         bit   $00
         and   $00
         rol   $00
         dfb   $27
         plp
         and   #$00
         rol   A
         dfb   $2b
         bit:  $0000
         and:  $0000
         rol:  $0000
         dfb   $2f
         bmi   L106D
L106D    and   ($00),y
         and   ($00)
         dfb   $33
         bit   $00,x
         and   $00,x
         rol   $00,x
         dfb   $37
         sec
         and   $0000,y
         dec   A
         dfb   $3b
         bit:  $0000,x
         and:  $0000,x
         rol:  $0000,x
         dfb   $3f
         rti

L108A    eor   ($00,x)
         dfb   $42,$00
         dfb   $43
         dfb   $44,$00
         eor   $00
         lsr   $00
         dfb   $47
         pha
         eor   #$00
         lsr   A
         dfb   $4b
         jmp   L109E

L109E    eor:  $0000
         lsr:  $0000
         dfb   $4f
         bvc   L10A7
L10A7    eor   ($00),y
         eor   ($00)
         dfb   $53
         dfb   $54,$00
         eor   $00,x
         lsr   $00,x
         dfb   $57
         cli
         eor   $0000,y
         phy
         dfb   $5b
         dfb   $5c,$00,$00
         eor:  $0000,x
         lsr:  $0000,x
         dfb   $5f
         rts

L10C4    adc   ($00,x)
         dfb   $62,$00
         dfb   $63
         stz   $00
         adc   $00
         ror   $00
         dfb   $67
         pla
         adc   #$00
         ror   A
         dfb   $6b
         jmp   ($0000)

L10D8    adc:  $0000
         ror:  $0000
         dfb   $6f
         bvs   L10E1
L10E1    adc   ($00),y
         adc   ($00)
         dfb   $73
         stz   $00,x
         adc   $00,x
         ror   $00,x
         dfb   $77
         sei
         adc   $0000,y
         ply
         dfb   $7b
         jmp   ($0000,x)

L10F6    adc:  $0000,x
         ror:  $0000,x
         dfb   $7f
         bra   L10FF

L10FF    sta   ($00,x)
         dfb   $82,$00
         dfb   $83
         sty   $00
         sta   $00
         stx   $00
         dfb   $87
         dey
         bit   #$00
         txa
         dfb   $8b
         sty:  $0000
         sta:  $0000
         stx:  $0000
         dfb   $8f
         bcc   L111C
L111C    sta   ($00),y
         sta   ($00)
         dfb   $93
         sty   $00,x
         sta   $00,x
         stx   $00,y
         dfb   $97
         tya
         sta   $0000,y
         txs
         dfb   $9b
         stz:  $0000
         sta:  $0000,x
         stz:  $0000,x
         dfb   $9f
         ldy   #$00
         lda   ($00,x)
         ldx   #$00
         dfb   $a3
         ldy   $00
         lda   $00
         ldx   $00
         dfb   $a7
         tay
         lda   #$00
         tax
         dfb   $ab
         ldy:  $0000
         lda:  $0000
         ldx:  $0000
         dfb   $af
         bcs   L1157
L1157    lda   ($00),y
         lda   ($00)
         dfb   $b3
         ldy   $00,x
         lda   $00,x
         ldx   $00,y
         dfb   $b7
         clv
         lda   $0000,y
         tsx
         dfb   $bb
         ldy:  $0000,x
         lda:  $0000,x
         ldx:  $0000,y
         dfb   $bf
         cpy   #$00
         cmp   ($00,x)
         dfb   $c2,$00
         dfb   $c3
         cpy   $00
         cmp   $00
         dec   $00
         dfb   $c7
         iny
         cmp   #$00
         dex
         dfb   $cb
         cpy:  $0000
         cmp:  $0000
         dec:  $0000
         dfb   $cf
         bne   L1192
L1192    cmp   ($00),y
         cmp   ($00)
         dfb   $d3
         dfb   $d4,$00
         cmp   $00,x
         dec   $00,x
         dfb   $d7
         cld
         cmp   $0000,y
         phx
         dfb   $db
         dfb   $dc,$00,$00
         cmp:  $0000,x
         dec:  $0000,x
         dfb   $df
         cpx   #$00
         sbc   ($00,x)
         dfb   $e2,$00
         dfb   $e3
         cpx   $00
         sbc   $00
         inc   $00
         dfb   $e7
         inx
         sbc   #$00
         nop
         dfb   $eb
         cpx:  $0000
         sbc:  $0000
         inc:  $0000
         dfb   $ef
         beq   L11CD
L11CD    sbc   ($00),y
         sbc   ($00)
         dfb   $f3
         dfb   $f4,$00
         sbc   $00,x
         inc   $00,x
         dfb   $f7
         sed
         sbc   $0000,y
         plx
         dfb   $fb
         dfb   $fc,$00,$00
         sbc:  $0000,x
         inc:  $0000,x
         dfb   $ff
