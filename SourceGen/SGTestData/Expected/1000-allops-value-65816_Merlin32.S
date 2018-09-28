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
         brk   $ff

L101F    ora   ($ff,x)
         cop   $ff
         ora   $ff,S
         tsb   $ff
         ora   $ff
         asl   $ff
         ora   [$ff]
         php
         ora   #$ff
         asl   A
         phd
         tsb   $feff
         ora   $feff
         asl   $feff
         oral  $fdfeff
         bpl   L1041
L1041    ora   ($ff),y
         ora   ($ff)
         ora   ($ff,S),y
         trb   $ff
         ora   $ff,x
         asl   $ff,x
         ora   [$ff],y
         clc
         ora   $feff,y
         inc   A
         tcs
         trb   $feff
         ora   $feff,x
         asl   $feff,x
         oral  $fdfeff,x
         jsr   $feff
         and   ($ff,x)
         jsl   $fdfeff
         and   $ff,S
         bit   $ff
         and   $ff
         rol   $ff
         and   [$ff]
         plp
         and   #$ff
         rol   A
         pld
         bit   $feff
         and   $feff
         rol   $feff
         andl  $fdfeff
         bmi   L1089
L1089    and   ($ff),y
         and   ($ff)
         and   ($ff,S),y
         bit   $ff,x
         and   $ff,x
         rol   $ff,x
         and   [$ff],y
         sec
         and   $feff,y
         dec   A
         tsc
         bit   $feff,x
         and   $feff,x
         rol   $feff,x
         andl  $fdfeff,x
         rti

L10AB    eor   ($ff,x)
         wdm   $ff
         eor   $ff,S
         mvp   $fe,$ff
         eor   $ff
         lsr   $ff
         eor   [$ff]
         pha
         eor   #$ff
         lsr   A
         phk
         jmp   L10C2

L10C2    eor   $feff
         lsr   $feff
         eorl  $fdfeff
         bvc   L10CE
L10CE    eor   ($ff),y
         eor   ($ff)
         eor   ($ff,S),y
         mvn   $fe,$ff
         eor   $ff,x
         lsr   $ff,x
         eor   [$ff],y
         cli
         eor   $feff,y
         phy
         tcd
         jml   L10E7

L10E7    eor   $feff,x
         lsr   $feff,x
         eorl  $fdfeff,x
         rts

L10F2    adc   ($ff,x)
         per   $0ff6
         adc   $ff,S
         stz   $ff
         adc   $ff
         ror   $ff
         adc   [$ff]
         pla
         adc   #$ff
         ror   A
         rtl

L1106    jmp   ($feff)

L1109    adc   $feff
         ror   $feff
         adcl  $fdfeff
         bvs   L1115
L1115    adc   ($ff),y
         adc   ($ff)
         adc   ($ff,S),y
         stz   $ff,x
         adc   $ff,x
         ror   $ff,x
         adc   [$ff],y
         sei
         adc   $feff,y
         ply
         tdc
         jmp   ($feff,x)

L112C    adc   $feff,x
         ror   $feff,x
         adcl  $fdfeff,x
         bra   L1138

L1138    sta   ($ff,x)
         brl   L113D

L113D    sta   $ff,S
         sty   $ff
         sta   $ff
         stx   $ff
         sta   [$ff]
         dey
         bit   #$ff
         txa
         phb
         sty   $feff
         sta   $feff
         stx   $feff
         stal  $fdfeff
         bcc   L115B
L115B    sta   ($ff),y
         sta   ($ff)
         sta   ($ff,S),y
         sty   $ff,x
         sta   $ff,x
         stx   $ff,y
         sta   [$ff],y
         tya
         sta   $feff,y
         txs
         txy
         stz   $feff
         sta   $feff,x
         stz   $feff,x
         stal  $fdfeff,x
         ldy   #$ff
         lda   ($ff,x)
         ldx   #$ff
         lda   $ff,S
         ldy   $ff
         lda   $ff
         ldx   $ff
         lda   [$ff]
         tay
         lda   #$ff
         tax
         plb
         ldy   $feff
         lda   $feff
         ldx   $feff
         ldal  $fdfeff
         bcs   L11A0
L11A0    lda   ($ff),y
         lda   ($ff)
         lda   ($ff,S),y
         ldy   $ff,x
         lda   $ff,x
         ldx   $ff,y
         lda   [$ff],y
         clv
         lda   $feff,y
         tsx
         tyx
         ldy   $feff,x
         lda   $feff,x
         ldx   $feff,y
         ldal  $fdfeff,x
         cpy   #$ff
         cmp   ($ff,x)
         rep   #$00
         cmp   $ff,S
         cpy   $ff
         cmp   $ff
         dec   $ff
         cmp   [$ff]
         iny
         cmp   #$ff
         dex
         wai
         cpy   $feff
         cmp   $feff
         dec   $feff
         cmpl  $fdfeff
         bne   L11E5
L11E5    cmp   ($ff),y
         cmp   ($ff)
         cmp   ($ff,S),y
         pei   ($ff)
         cmp   $ff,x
         dec   $ff,x
         cmp   [$ff],y
         cld
         cmp   $feff,y
         phx
         stp

L11F9    jml   [$feff]

L11FC    cmp   $feff,x
         dec   $feff,x
         cmpl  $fdfeff,x
         cpx   #$ff
         sbc   ($ff,x)
         sep   #$00
         sbc   $ff,S
         cpx   $ff
         sbc   $ff
         inc   $ff
         sbc   [$ff]
         inx
         sbc   #$ff
         nop
         xba
         cpx   $feff
         sbc   $feff
         inc   $feff
         sbcl  $fdfeff
         beq   L122A
L122A    sbc   ($ff),y
         sbc   ($ff)
         sbc   ($ff,S),y
         pea   $feff
         sbc   $ff,x
         inc   $ff,x
         sbc   [$ff],y
         sed
         sbc   $feff,y
         plx
         xce
         jsr   ($feff,x)
         sbc   $feff,x
         inc   $feff,x
         sbcl  $fdfeff,x
