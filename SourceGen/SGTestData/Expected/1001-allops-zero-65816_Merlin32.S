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
         brk

         dfb   $00

L101F    ora   ($00,x)
         cop   $00
         ora   $00,S
         tsb   $00
         ora   $00
         asl   $00
         ora   [$00]
         php
         ora   #$00
         asl   A
         phd
         tsb:  $0000
         ora:  $0000
         asl:  $0000
         oral  $000000
         bpl   L1041
L1041    ora   ($00),y
         ora   ($00)
         ora   ($00,S),y
         trb   $00
         ora   $00,x
         asl   $00,x
         ora   [$00],y
         clc
         ora   $0000,y
         inc   A
         tcs
         trb:  $0000
         ora:  $0000,x
         asl:  $0000,x
         oral  $000000,x
         jsr   $0000
         and   ($00,x)
         jsl   $000000
         and   $00,S
         bit   $00
         and   $00
         rol   $00
         and   [$00]
         plp
         and   #$00
         rol   A
         pld
         bit:  $0000
         and:  $0000
         rol:  $0000
         andl  $000000
         bmi   L1089
L1089    and   ($00),y
         and   ($00)
         and   ($00,S),y
         bit   $00,x
         and   $00,x
         rol   $00,x
         and   [$00],y
         sec
         and   $0000,y
         dec   A
         tsc
         bit:  $0000,x
         and:  $0000,x
         rol:  $0000,x
         andl  $000000,x
         rti

L10AB    eor   ($00,x)
         wdm   $00
         eor   $00,S
         mvp   #$00,#$00
         eor   $00
         lsr   $00
         eor   [$00]
         pha
         eor   #$00
         lsr   A
         phk
         jmp   L10C2

L10C2    eor:  $0000
         lsr:  $0000
         eorl  $000000
         bvc   L10CE
L10CE    eor   ($00),y
         eor   ($00)
         eor   ($00,S),y
         mvn   #$00,#$00
         eor   $00,x
         lsr   $00,x
         eor   [$00],y
         cli
         eor   $0000,y
         phy
         tcd
         jml   L10E7

L10E7    eor:  $0000,x
         lsr:  $0000,x
         eorl  $000000,x
         rts

L10F2    adc   ($00,x)
         per   $0ff6
         adc   $00,S
         stz   $00
         adc   $00
         ror   $00
         adc   [$00]
         pla
         adc   #$00
         ror   A
         rtl

L1106    jmp   ($0000)

L1109    adc:  $0000
         ror:  $0000
         adcl  $000000
         bvs   L1115
L1115    adc   ($00),y
         adc   ($00)
         adc   ($00,S),y
         stz   $00,x
         adc   $00,x
         ror   $00,x
         adc   [$00],y
         sei
         adc   $0000,y
         ply
         tdc
         jmp   ($0000,x)

L112C    adc:  $0000,x
         ror:  $0000,x
         adcl  $000000,x
         bra   L1138

L1138    sta   ($00,x)
         brl   L113D

L113D    sta   $00,S
         sty   $00
         sta   $00
         stx   $00
         sta   [$00]
         dey
         bit   #$00
         txa
         phb
         sty:  $0000
         sta:  $0000
         stx:  $0000
         stal  $000000
         bcc   L115B
L115B    sta   ($00),y
         sta   ($00)
         sta   ($00,S),y
         sty   $00,x
         sta   $00,x
         stx   $00,y
         sta   [$00],y
         tya
         sta   $0000,y
         txs
         txy
         stz:  $0000
         sta:  $0000,x
         stz:  $0000,x
         stal  $000000,x
         ldy   #$00
         lda   ($00,x)
         ldx   #$00
         lda   $00,S
         ldy   $00
         lda   $00
         ldx   $00
         lda   [$00]
         tay
         lda   #$00
         tax
         plb
         ldy:  $0000
         lda:  $0000
         ldx:  $0000
         ldal  $000000
         bcs   L11A0
L11A0    lda   ($00),y
         lda   ($00)
         lda   ($00,S),y
         ldy   $00,x
         lda   $00,x
         ldx   $00,y
         lda   [$00],y
         clv
         lda   $0000,y
         tsx
         tyx
         ldy:  $0000,x
         lda:  $0000,x
         ldx:  $0000,y
         ldal  $000000,x
         cpy   #$00
         cmp   ($00,x)
         rep   #$00
         cmp   $00,S
         cpy   $00
         cmp   $00
         dec   $00
         cmp   [$00]
         iny
         cmp   #$00
         dex
         wai
         cpy:  $0000
         cmp:  $0000
         dec:  $0000
         cmpl  $000000
         bne   L11E5
L11E5    cmp   ($00),y
         cmp   ($00)
         cmp   ($00,S),y
         pei   ($00)
         cmp   $00,x
         dec   $00,x
         cmp   [$00],y
         cld
         cmp   $0000,y
         phx
         stp

L11F9    jml   [$0000]

L11FC    cmp:  $0000,x
         dec:  $0000,x
         cmpl  $000000,x
         cpx   #$00
         sbc   ($00,x)
         sep   #$00
         sbc   $00,S
         cpx   $00
         sbc   $00
         inc   $00
         sbc   [$00]
         inx
         sbc   #$00
         nop
         xba
         cpx:  $0000
         sbc:  $0000
         inc:  $0000
         sbcl  $000000
         beq   L122A
L122A    sbc   ($00),y
         sbc   ($00)
         sbc   ($00,S),y
         pea   $0000
         sbc   $00,x
         inc   $00,x
         sbc   [$00],y
         sed
         sbc   $0000,y
         plx
         xce
         jsr   ($0000,x)
         sbc:  $0000,x
         inc:  $0000,x
         sbcl  $000000,x
