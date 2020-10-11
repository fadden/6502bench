         .setcpu "65C02"
;         .segment "SEG000"
         .org    $1000
         jsr     L1017
         jsr     L1099
         jsr     L10D9
         jsr     L10EE
         jsr     L110F
         jsr     L11CF
         nop
         nop
         nop
         brk

         .byte   $ff

L1017:   ora     ($ff,x)
         .byte   $02,$ff
         .byte   $03
         tsb     $ff
         ora     $ff
         asl     $ff
         rmb0    $ff
         php
         ora     #$ff
         asl     A
         .byte   $0b
         tsb     $feff
         ora     $feff
         asl     $feff
         bbr0    $ff,@L1035
@L1035:  bpl     @L1037
@L1037:  ora     ($ff),y
         ora     ($ff)
         .byte   $13
         trb     $ff
         ora     $ff,x
         asl     $ff,x
         rmb1    $ff
         clc
         ora     $feff,y
         inc     A
         .byte   $1b
         trb     $feff
         ora     $feff,x
         asl     $feff,x
         bbr1    $ff,@L1056
@L1056:  jsr     $feff
         and     ($ff,x)
         .byte   $22,$ff
         .byte   $23
         bit     $ff
         and     $ff
         rol     $ff
         rmb2    $ff
         plp
         and     #$ff
         rol     A
         .byte   $2b
         bit     $feff
         and     $feff
         rol     $feff
         bbr2    $ff,@L1077
@L1077:  bmi     @L1079
@L1079:  and     ($ff),y
         and     ($ff)
         .byte   $33
         bit     $ff,x
         and     $ff,x
         rol     $ff,x
         rmb3    $ff
         sec
         and     $feff,y
         dec     A
         .byte   $3b
         bit     $feff,x
         and     $feff,x
         rol     $feff,x
         bbr3    $ff,@L1098
@L1098:  rti

L1099:   eor     ($ff,x)
         .byte   $42,$ff
         .byte   $43
         .byte   $44,$ff
         eor     $ff
         lsr     $ff
         rmb4    $ff
         pha
         eor     #$ff
         lsr     A
         .byte   $4b
         jmp     @L10AE

@L10AE:  eor     $feff
         lsr     $feff
         bbr4    $ff,@L10B7
@L10B7:  bvc     @L10B9
@L10B9:  eor     ($ff),y
         eor     ($ff)
         .byte   $53
         .byte   $54,$ff
         eor     $ff,x
         lsr     $ff,x
         rmb5    $ff
         cli
         eor     $feff,y
         phy
         .byte   $5b
         .byte   $5c,$ff,$fe
         eor     $feff,x
         lsr     $feff,x
         bbr5    $ff,@L10D8
@L10D8:  rts

L10D9:   adc     ($ff,x)
         .byte   $62,$ff
         .byte   $63
         stz     $ff
         adc     $ff
         ror     $ff
         rmb6    $ff
         pla
         adc     #$ff
         ror     A
         .byte   $6b
         jmp     ($feff)

L10EE:   adc     $feff
         ror     $feff
         bbr6    $ff,@L10F7
@L10F7:  bvs     @L10F9
@L10F9:  adc     ($ff),y
         adc     ($ff)
         .byte   $73
         stz     $ff,x
         adc     $ff,x
         ror     $ff,x
         rmb7    $ff
         sei
         adc     $feff,y
         ply
         .byte   $7b
         jmp     ($feff,x)

L110F:   adc     $feff,x
         ror     $feff,x
         bbr7    $ff,@L1118
@L1118:  bra     @L111A

@L111A:  sta     ($ff,x)
         .byte   $82,$ff
         .byte   $83
         sty     $ff
         sta     $ff
         stx     $ff
         smb0    $ff
         dey
         bit     #$ff
         txa
         .byte   $8b
         sty     $feff
         sta     $feff
         stx     $feff
         bbs0    $ff,@L1138
@L1138:  bcc     @L113A
@L113A:  sta     ($ff),y
         sta     ($ff)
         .byte   $93
         sty     $ff,x
         sta     $ff,x
         stx     $ff,y
         smb1    $ff
         tya
         sta     $feff,y
         txs
         .byte   $9b
         stz     $feff
         sta     $feff,x
         stz     $feff,x
         bbs1    $ff,@L1159
@L1159:  ldy     #$ff
         lda     ($ff,x)
         ldx     #$ff
         .byte   $a3
         ldy     $ff
         lda     $ff
         ldx     $ff
         smb2    $ff
         tay
         lda     #$ff
         tax
         .byte   $ab
         ldy     $feff
         lda     $feff
         ldx     $feff
         bbs2    $ff,@L1179
@L1179:  bcs     @L117B
@L117B:  lda     ($ff),y
         lda     ($ff)
         .byte   $b3
         ldy     $ff,x
         lda     $ff,x
         ldx     $ff,y
         smb3    $ff
         clv
         lda     $feff,y
         tsx
         .byte   $bb
         ldy     $feff,x
         lda     $feff,x
         ldx     $feff,y
         bbs3    $ff,@L119A
@L119A:  cpy     #$ff
         cmp     ($ff,x)
         .byte   $c2,$ff
         .byte   $c3
         cpy     $ff
         cmp     $ff
         dec     $ff
         smb4    $ff
         iny
         cmp     #$ff
         dex
         wai
         cpy     $feff
         cmp     $feff
         dec     $feff
         bbs4    $ff,@L11BA
@L11BA:  bne     @L11BC
@L11BC:  cmp     ($ff),y
         cmp     ($ff)
         .byte   $d3
         .byte   $d4,$ff
         cmp     $ff,x
         dec     $ff,x
         smb5    $ff
         cld
         cmp     $feff,y
         phx
         stp

L11CF:   .byte   $dc,$ff,$fe
         cmp     $feff,x
         dec     $feff,x
         bbs5    $ff,@L11DB
@L11DB:  cpx     #$ff
         sbc     ($ff,x)
         .byte   $e2,$ff
         .byte   $e3
         cpx     $ff
         sbc     $ff
         inc     $ff
         smb6    $ff
         inx
         sbc     #$ff
         nop
         .byte   $eb
         cpx     $feff
         sbc     $feff
         inc     $feff
         bbs6    $ff,@L11FB
@L11FB:  beq     @L11FD
@L11FD:  sbc     ($ff),y
         sbc     ($ff)
         .byte   $f3
         .byte   $f4,$ff
         sbc     $ff,x
         inc     $ff,x
         smb7    $ff
         sed
         sbc     $feff,y
         plx
         .byte   $fb
         .byte   $fc,$ff,$fe
         sbc     $feff,x
         inc     $feff,x
         bbs7    $ff,@L121C
@L121C:  rts

