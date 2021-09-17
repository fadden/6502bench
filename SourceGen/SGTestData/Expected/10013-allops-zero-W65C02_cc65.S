         .setcpu "65C02"
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

         .byte   $00

L1017:   ora     ($00,x)
         .byte   $02,$00
         .byte   $03
         tsb     $00
         ora     $00
         asl     $00
         rmb0    $00
         php
         ora     #$00
         asl     A
         .byte   $0b
         tsb     a:$0000
         ora     a:$0000
         asl     a:$0000
         bbr0    $00,@L1035
@L1035:  bpl     @L1037
@L1037:  ora     ($00),y
         ora     ($00)
         .byte   $13
         trb     $00
         ora     $00,x
         asl     $00,x
         rmb1    $00
         clc
         ora     $0000,y
         inc     A
         .byte   $1b
         trb     a:$0000
         ora     a:$0000,x
         asl     a:$0000,x
         bbr1    $00,@L1056
@L1056:  jsr     $0000
         and     ($00,x)
         .byte   $22,$00
         .byte   $23
         bit     $00
         and     $00
         rol     $00
         rmb2    $00
         plp
         and     #$00
         rol     A
         .byte   $2b
         bit     a:$0000
         and     a:$0000
         rol     a:$0000
         bbr2    $00,@L1077
@L1077:  bmi     @L1079
@L1079:  and     ($00),y
         and     ($00)
         .byte   $33
         bit     $00,x
         and     $00,x
         rol     $00,x
         rmb3    $00
         sec
         and     $0000,y
         dec     A
         .byte   $3b
         bit     a:$0000,x
         and     a:$0000,x
         rol     a:$0000,x
         bbr3    $00,@L1098
@L1098:  rti

L1099:   eor     ($00,x)
         .byte   $42,$00
         .byte   $43
         .byte   $44,$00
         eor     $00
         lsr     $00
         rmb4    $00
         pha
         eor     #$00
         lsr     A
         .byte   $4b
         jmp     @L10AE

@L10AE:  eor     a:$0000
         lsr     a:$0000
         bbr4    $00,@L10B7
@L10B7:  bvc     @L10B9
@L10B9:  eor     ($00),y
         eor     ($00)
         .byte   $53
         .byte   $54,$00
         eor     $00,x
         lsr     $00,x
         rmb5    $00
         cli
         eor     $0000,y
         phy
         .byte   $5b
         .byte   $5c,$00,$00
         eor     a:$0000,x
         lsr     a:$0000,x
         bbr5    $00,@L10D8
@L10D8:  rts

L10D9:   adc     ($00,x)
         .byte   $62,$00
         .byte   $63
         stz     $00
         adc     $00
         ror     $00
         rmb6    $00
         pla
         adc     #$00
         ror     A
         .byte   $6b
         jmp     ($0000)

L10EE:   adc     a:$0000
         ror     a:$0000
         bbr6    $00,@L10F7
@L10F7:  bvs     @L10F9
@L10F9:  adc     ($00),y
         adc     ($00)
         .byte   $73
         stz     $00,x
         adc     $00,x
         ror     $00,x
         rmb7    $00
         sei
         adc     $0000,y
         ply
         .byte   $7b
         jmp     ($0000,x)

L110F:   adc     a:$0000,x
         ror     a:$0000,x
         bbr7    $00,@L1118
@L1118:  bra     @L111A

@L111A:  sta     ($00,x)
         .byte   $82,$00
         .byte   $83
         sty     $00
         sta     $00
         stx     $00
         smb0    $00
         dey
         bit     #$00
         txa
         .byte   $8b
         sty     a:$0000
         sta     a:$0000
         stx     a:$0000
         bbs0    $00,@L1138
@L1138:  bcc     @L113A
@L113A:  sta     ($00),y
         sta     ($00)
         .byte   $93
         sty     $00,x
         sta     $00,x
         stx     $00,y
         smb1    $00
         tya
         sta     $0000,y
         txs
         .byte   $9b
         stz     a:$0000
         sta     a:$0000,x
         stz     a:$0000,x
         bbs1    $00,@L1159
@L1159:  ldy     #$00
         lda     ($00,x)
         ldx     #$00
         .byte   $a3
         ldy     $00
         lda     $00
         ldx     $00
         smb2    $00
         tay
         lda     #$00
         tax
         .byte   $ab
         ldy     a:$0000
         lda     a:$0000
         ldx     a:$0000
         bbs2    $00,@L1179
@L1179:  bcs     @L117B
@L117B:  lda     ($00),y
         lda     ($00)
         .byte   $b3
         ldy     $00,x
         lda     $00,x
         ldx     $00,y
         smb3    $00
         clv
         lda     $0000,y
         tsx
         .byte   $bb
         ldy     a:$0000,x
         lda     a:$0000,x
         ldx     a:$0000,y
         bbs3    $00,@L119A
@L119A:  cpy     #$00
         cmp     ($00,x)
         .byte   $c2,$00
         .byte   $c3
         cpy     $00
         cmp     $00
         dec     $00
         smb4    $00
         iny
         cmp     #$00
         dex
         wai
         cpy     a:$0000
         cmp     a:$0000
         dec     a:$0000
         bbs4    $00,@L11BA
@L11BA:  bne     @L11BC
@L11BC:  cmp     ($00),y
         cmp     ($00)
         .byte   $d3
         .byte   $d4,$00
         cmp     $00,x
         dec     $00,x
         smb5    $00
         cld
         cmp     $0000,y
         phx
         stp

L11CF:   .byte   $dc,$00,$00
         cmp     a:$0000,x
         dec     a:$0000,x
         bbs5    $00,@L11DB
@L11DB:  cpx     #$00
         sbc     ($00,x)
         .byte   $e2,$00
         .byte   $e3
         cpx     $00
         sbc     $00
         inc     $00
         smb6    $00
         inx
         sbc     #$00
         nop
         .byte   $eb
         cpx     a:$0000
         sbc     a:$0000
         inc     a:$0000
         bbs6    $00,@L11FB
@L11FB:  beq     @L11FD
@L11FD:  sbc     ($00),y
         sbc     ($00)
         .byte   $f3
         .byte   $f4,$00
         sbc     $00,x
         inc     $00,x
         smb7    $00
         sed
         sbc     $0000,y
         plx
         .byte   $fb
         .byte   $fc,$00,$00
         sbc     a:$0000,x
         inc     a:$0000,x
         bbs7    $00,@L121C
@L121C:  rts

