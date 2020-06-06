         org   $1000
         jsr   L1014
         jsr   L108A
         jsr   L10C4
         jsr   L10D8
         jsr   L10F6
         nop
         nop
         nop
         brk

         dfb   $ff

L1014    ora   ($ff,x)
         dfb   $02,$ff
         dfb   $03
         tsb   $ff
         ora   $ff
         asl   $ff
         dfb   $07
         php
         ora   #$ff
         asl   A
         dfb   $0b
         tsb   $feff
         ora   $feff
         asl   $feff
         dfb   $0f
         bpl   :L1031
:L1031   ora   ($ff),y
         ora   ($ff)
         dfb   $13
         trb   $ff
         ora   $ff,x
         asl   $ff,x
         dfb   $17
         clc
         ora   $feff,y
         inc   A
         dfb   $1b
         trb   $feff
         ora   $feff,x
         asl   $feff,x
         dfb   $1f
         jsr   $feff
         and   ($ff,x)
         dfb   $22,$ff
         dfb   $23
         bit   $ff
         and   $ff
         rol   $ff
         dfb   $27
         plp
         and   #$ff
         rol   A
         dfb   $2b
         bit   $feff
         and   $feff
         rol   $feff
         dfb   $2f
         bmi   :L106D
:L106D   and   ($ff),y
         and   ($ff)
         dfb   $33
         bit   $ff,x
         and   $ff,x
         rol   $ff,x
         dfb   $37
         sec
         and   $feff,y
         dec   A
         dfb   $3b
         bit   $feff,x
         and   $feff,x
         rol   $feff,x
         dfb   $3f
         rti

L108A    eor   ($ff,x)
         dfb   $42,$ff
         dfb   $43
         dfb   $44,$ff
         eor   $ff
         lsr   $ff
         dfb   $47
         pha
         eor   #$ff
         lsr   A
         dfb   $4b
         jmp   :L109E

:L109E   eor   $feff
         lsr   $feff
         dfb   $4f
         bvc   :L10A7
:L10A7   eor   ($ff),y
         eor   ($ff)
         dfb   $53
         dfb   $54,$ff
         eor   $ff,x
         lsr   $ff,x
         dfb   $57
         cli
         eor   $feff,y
         phy
         dfb   $5b
         dfb   $5c,$ff,$fe
         eor   $feff,x
         lsr   $feff,x
         dfb   $5f
         rts

L10C4    adc   ($ff,x)
         dfb   $62,$ff
         dfb   $63
         stz   $ff
         adc   $ff
         ror   $ff
         dfb   $67
         pla
         adc   #$ff
         ror   A
         dfb   $6b
         jmp   ($feff)

L10D8    adc   $feff
         ror   $feff
         dfb   $6f
         bvs   :L10E1
:L10E1   adc   ($ff),y
         adc   ($ff)
         dfb   $73
         stz   $ff,x
         adc   $ff,x
         ror   $ff,x
         dfb   $77
         sei
         adc   $feff,y
         ply
         dfb   $7b
         jmp   ($feff,x)

L10F6    adc   $feff,x
         ror   $feff,x
         dfb   $7f
         bra   :L10FF

:L10FF   sta   ($ff,x)
         dfb   $82,$ff
         dfb   $83
         sty   $ff
         sta   $ff
         stx   $ff
         dfb   $87
         dey
         bit   #$ff
         txa
         dfb   $8b
         sty   $feff
         sta   $feff
         stx   $feff
         dfb   $8f
         bcc   :L111C
:L111C   sta   ($ff),y
         sta   ($ff)
         dfb   $93
         sty   $ff,x
         sta   $ff,x
         stx   $ff,y
         dfb   $97
         tya
         sta   $feff,y
         txs
         dfb   $9b
         stz   $feff
         sta   $feff,x
         stz   $feff,x
         dfb   $9f
         ldy   #$ff
         lda   ($ff,x)
         ldx   #$ff
         dfb   $a3
         ldy   $ff
         lda   $ff
         ldx   $ff
         dfb   $a7
         tay
         lda   #$ff
         tax
         dfb   $ab
         ldy   $feff
         lda   $feff
         ldx   $feff
         dfb   $af
         bcs   :L1157
:L1157   lda   ($ff),y
         lda   ($ff)
         dfb   $b3
         ldy   $ff,x
         lda   $ff,x
         ldx   $ff,y
         dfb   $b7
         clv
         lda   $feff,y
         tsx
         dfb   $bb
         ldy   $feff,x
         lda   $feff,x
         ldx   $feff,y
         dfb   $bf
         cpy   #$ff
         cmp   ($ff,x)
         dfb   $c2,$ff
         dfb   $c3
         cpy   $ff
         cmp   $ff
         dec   $ff
         dfb   $c7
         iny
         cmp   #$ff
         dex
         dfb   $cb
         cpy   $feff
         cmp   $feff
         dec   $feff
         dfb   $cf
         bne   :L1192
:L1192   cmp   ($ff),y
         cmp   ($ff)
         dfb   $d3
         dfb   $d4,$ff
         cmp   $ff,x
         dec   $ff,x
         dfb   $d7
         cld
         cmp   $feff,y
         phx
         dfb   $db
         dfb   $dc,$ff,$fe
         cmp   $feff,x
         dec   $feff,x
         dfb   $df
         cpx   #$ff
         sbc   ($ff,x)
         dfb   $e2,$ff
         dfb   $e3
         cpx   $ff
         sbc   $ff
         inc   $ff
         dfb   $e7
         inx
         sbc   #$ff
         nop
         dfb   $eb
         cpx   $feff
         sbc   $feff
         inc   $feff
         dfb   $ef
         beq   :L11CD
:L11CD   sbc   ($ff),y
         sbc   ($ff)
         dfb   $f3
         dfb   $f4,$ff
         sbc   $ff,x
         inc   $ff,x
         dfb   $f7
         sed
         sbc   $feff,y
         plx
         dfb   $fb
         dfb   $fc,$ff,$fe
         sbc   $feff,x
         inc   $feff,x
         dfb   $ff
