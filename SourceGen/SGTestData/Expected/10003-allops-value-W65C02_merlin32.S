         org   $1000
         jsr   L1017
         jsr   L1099
         jsr   L10D9
         jsr   L10EE
         jsr   L110F
         jsr   L11CF
         nop
         nop
         nop
         brk

         dfb   $ff

L1017    ora   ($ff,x)
         dfb   $02,$ff
         dfb   $03
         tsb   $ff
         ora   $ff
         asl   $ff
         dfb   $07,$ff
         php
         ora   #$ff
         asl   A
         dfb   $0b
         tsb   $feff
         ora   $feff
         asl   $feff
         dfb   $0f,$ff,$00
:L1035   bpl   :L1037
:L1037   ora   ($ff),y
         ora   ($ff)
         dfb   $13
         trb   $ff
         ora   $ff,x
         asl   $ff,x
         dfb   $17,$ff
         clc
         ora   $feff,y
         inc   A
         dfb   $1b
         trb   $feff
         ora   $feff,x
         asl   $feff,x
         dfb   $1f,$ff,$00
:L1056   jsr   $feff
         and   ($ff,x)
         dfb   $22,$ff
         dfb   $23
         bit   $ff
         and   $ff
         rol   $ff
         dfb   $27,$ff
         plp
         and   #$ff
         rol   A
         dfb   $2b
         bit   $feff
         and   $feff
         rol   $feff
         dfb   $2f,$ff,$00
:L1077   bmi   :L1079
:L1079   and   ($ff),y
         and   ($ff)
         dfb   $33
         bit   $ff,x
         and   $ff,x
         rol   $ff,x
         dfb   $37,$ff
         sec
         and   $feff,y
         dec   A
         dfb   $3b
         bit   $feff,x
         and   $feff,x
         rol   $feff,x
         dfb   $3f,$ff,$00
:L1098   rti

L1099    eor   ($ff,x)
         dfb   $42,$ff
         dfb   $43
         dfb   $44,$ff
         eor   $ff
         lsr   $ff
         dfb   $47,$ff
         pha
         eor   #$ff
         lsr   A
         dfb   $4b
         jmp   :L10AE

:L10AE   eor   $feff
         lsr   $feff
         dfb   $4f,$ff,$00
:L10B7   bvc   :L10B9
:L10B9   eor   ($ff),y
         eor   ($ff)
         dfb   $53
         dfb   $54,$ff
         eor   $ff,x
         lsr   $ff,x
         dfb   $57,$ff
         cli
         eor   $feff,y
         phy
         dfb   $5b
         dfb   $5c,$ff,$fe
         eor   $feff,x
         lsr   $feff,x
         dfb   $5f,$ff,$00
:L10D8   rts

L10D9    adc   ($ff,x)
         dfb   $62,$ff
         dfb   $63
         stz   $ff
         adc   $ff
         ror   $ff
         dfb   $67,$ff
         pla
         adc   #$ff
         ror   A
         dfb   $6b
         jmp   ($feff)

L10EE    adc   $feff
         ror   $feff
         dfb   $6f,$ff,$00
:L10F7   bvs   :L10F9
:L10F9   adc   ($ff),y
         adc   ($ff)
         dfb   $73
         stz   $ff,x
         adc   $ff,x
         ror   $ff,x
         dfb   $77,$ff
         sei
         adc   $feff,y
         ply
         dfb   $7b
         jmp   ($feff,x)

L110F    adc   $feff,x
         ror   $feff,x
         dfb   $7f,$ff,$00
:L1118   bra   :L111A

:L111A   sta   ($ff,x)
         dfb   $82,$ff
         dfb   $83
         sty   $ff
         sta   $ff
         stx   $ff
         dfb   $87,$ff
         dey
         bit   #$ff
         txa
         dfb   $8b
         sty   $feff
         sta   $feff
         stx   $feff
         dfb   $8f,$ff,$00
:L1138   bcc   :L113A
:L113A   sta   ($ff),y
         sta   ($ff)
         dfb   $93
         sty   $ff,x
         sta   $ff,x
         stx   $ff,y
         dfb   $97,$ff
         tya
         sta   $feff,y
         txs
         dfb   $9b
         stz   $feff
         sta   $feff,x
         stz   $feff,x
         dfb   $9f,$ff,$00
:L1159   ldy   #$ff
         lda   ($ff,x)
         ldx   #$ff
         dfb   $a3
         ldy   $ff
         lda   $ff
         ldx   $ff
         dfb   $a7,$ff
         tay
         lda   #$ff
         tax
         dfb   $ab
         ldy   $feff
         lda   $feff
         ldx   $feff
         dfb   $af,$ff,$00
:L1179   bcs   :L117B
:L117B   lda   ($ff),y
         lda   ($ff)
         dfb   $b3
         ldy   $ff,x
         lda   $ff,x
         ldx   $ff,y
         dfb   $b7,$ff
         clv
         lda   $feff,y
         tsx
         dfb   $bb
         ldy   $feff,x
         lda   $feff,x
         ldx   $feff,y
         dfb   $bf,$ff,$00
:L119A   cpy   #$ff
         cmp   ($ff,x)
         dfb   $c2,$ff
         dfb   $c3
         cpy   $ff
         cmp   $ff
         dec   $ff
         dfb   $c7,$ff
         iny
         cmp   #$ff
         dex
         wai
         cpy   $feff
         cmp   $feff
         dec   $feff
         dfb   $cf,$ff,$00
:L11BA   bne   :L11BC
:L11BC   cmp   ($ff),y
         cmp   ($ff)
         dfb   $d3
         dfb   $d4,$ff
         cmp   $ff,x
         dec   $ff,x
         dfb   $d7,$ff
         cld
         cmp   $feff,y
         phx
         stp

L11CF    dfb   $dc,$ff,$fe
         cmp   $feff,x
         dec   $feff,x
         dfb   $df,$ff,$00
:L11DB   cpx   #$ff
         sbc   ($ff,x)
         dfb   $e2,$ff
         dfb   $e3
         cpx   $ff
         sbc   $ff
         inc   $ff
         dfb   $e7,$ff
         inx
         sbc   #$ff
         nop
         dfb   $eb
         cpx   $feff
         sbc   $feff
         inc   $feff
         dfb   $ef,$ff,$00
:L11FB   beq   :L11FD
:L11FD   sbc   ($ff),y
         sbc   ($ff)
         dfb   $f3
         dfb   $f4,$ff
         sbc   $ff,x
         inc   $ff,x
         dfb   $f7,$ff
         sed
         sbc   $feff,y
         plx
         dfb   $fb
         dfb   $fc,$ff,$fe
         sbc   $feff,x
         inc   $feff,x
         dfb   $ff,$ff,$00
:L121C   rts

