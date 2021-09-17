         .setcpu "65C02"
         .org    $1000
         jsr     L1014
         jsr     L108A
         jsr     L10C4
         jsr     L10D8
         jsr     L10F6
         nop
         nop
         nop
         brk

         .byte   $00

L1014:   ora     ($00,x)
         .byte   $02,$00
         .byte   $03
         tsb     $00
         ora     $00
         asl     $00
         .byte   $07
         php
         ora     #$00
         asl     A
         .byte   $0b
         tsb     a:$0000
         ora     a:$0000
         asl     a:$0000
         .byte   $0f
         bpl     @L1031
@L1031:  ora     ($00),y
         ora     ($00)
         .byte   $13
         trb     $00
         ora     $00,x
         asl     $00,x
         .byte   $17
         clc
         ora     $0000,y
         inc     A
         .byte   $1b
         trb     a:$0000
         ora     a:$0000,x
         asl     a:$0000,x
         .byte   $1f
         jsr     $0000
         and     ($00,x)
         .byte   $22,$00
         .byte   $23
         bit     $00
         and     $00
         rol     $00
         .byte   $27
         plp
         and     #$00
         rol     A
         .byte   $2b
         bit     a:$0000
         and     a:$0000
         rol     a:$0000
         .byte   $2f
         bmi     @L106D
@L106D:  and     ($00),y
         and     ($00)
         .byte   $33
         bit     $00,x
         and     $00,x
         rol     $00,x
         .byte   $37
         sec
         and     $0000,y
         dec     A
         .byte   $3b
         bit     a:$0000,x
         and     a:$0000,x
         rol     a:$0000,x
         .byte   $3f
         rti

L108A:   eor     ($00,x)
         .byte   $42,$00
         .byte   $43
         .byte   $44,$00
         eor     $00
         lsr     $00
         .byte   $47
         pha
         eor     #$00
         lsr     A
         .byte   $4b
         jmp     @L109E

@L109E:  eor     a:$0000
         lsr     a:$0000
         .byte   $4f
         bvc     @L10A7
@L10A7:  eor     ($00),y
         eor     ($00)
         .byte   $53
         .byte   $54,$00
         eor     $00,x
         lsr     $00,x
         .byte   $57
         cli
         eor     $0000,y
         phy
         .byte   $5b
         .byte   $5c,$00,$00
         eor     a:$0000,x
         lsr     a:$0000,x
         .byte   $5f
         rts

L10C4:   adc     ($00,x)
         .byte   $62,$00
         .byte   $63
         stz     $00
         adc     $00
         ror     $00
         .byte   $67
         pla
         adc     #$00
         ror     A
         .byte   $6b
         jmp     ($0000)

L10D8:   adc     a:$0000
         ror     a:$0000
         .byte   $6f
         bvs     @L10E1
@L10E1:  adc     ($00),y
         adc     ($00)
         .byte   $73
         stz     $00,x
         adc     $00,x
         ror     $00,x
         .byte   $77
         sei
         adc     $0000,y
         ply
         .byte   $7b
         jmp     ($0000,x)

L10F6:   adc     a:$0000,x
         ror     a:$0000,x
         .byte   $7f
         bra     @L10FF

@L10FF:  sta     ($00,x)
         .byte   $82,$00
         .byte   $83
         sty     $00
         sta     $00
         stx     $00
         .byte   $87
         dey
         bit     #$00
         txa
         .byte   $8b
         sty     a:$0000
         sta     a:$0000
         stx     a:$0000
         .byte   $8f
         bcc     @L111C
@L111C:  sta     ($00),y
         sta     ($00)
         .byte   $93
         sty     $00,x
         sta     $00,x
         stx     $00,y
         .byte   $97
         tya
         sta     $0000,y
         txs
         .byte   $9b
         stz     a:$0000
         sta     a:$0000,x
         stz     a:$0000,x
         .byte   $9f
         ldy     #$00
         lda     ($00,x)
         ldx     #$00
         .byte   $a3
         ldy     $00
         lda     $00
         ldx     $00
         .byte   $a7
         tay
         lda     #$00
         tax
         .byte   $ab
         ldy     a:$0000
         lda     a:$0000
         ldx     a:$0000
         .byte   $af
         bcs     @L1157
@L1157:  lda     ($00),y
         lda     ($00)
         .byte   $b3
         ldy     $00,x
         lda     $00,x
         ldx     $00,y
         .byte   $b7
         clv
         lda     $0000,y
         tsx
         .byte   $bb
         ldy     a:$0000,x
         lda     a:$0000,x
         ldx     a:$0000,y
         .byte   $bf
         cpy     #$00
         cmp     ($00,x)
         .byte   $c2,$00
         .byte   $c3
         cpy     $00
         cmp     $00
         dec     $00
         .byte   $c7
         iny
         cmp     #$00
         dex
         .byte   $cb
         cpy     a:$0000
         cmp     a:$0000
         dec     a:$0000
         .byte   $cf
         bne     @L1192
@L1192:  cmp     ($00),y
         cmp     ($00)
         .byte   $d3
         .byte   $d4,$00
         cmp     $00,x
         dec     $00,x
         .byte   $d7
         cld
         cmp     $0000,y
         phx
         .byte   $db
         .byte   $dc,$00,$00
         cmp     a:$0000,x
         dec     a:$0000,x
         .byte   $df
         cpx     #$00
         sbc     ($00,x)
         .byte   $e2,$00
         .byte   $e3
         cpx     $00
         sbc     $00
         inc     $00
         .byte   $e7
         inx
         sbc     #$00
         nop
         .byte   $eb
         cpx     a:$0000
         sbc     a:$0000
         inc     a:$0000
         .byte   $ef
         beq     @L11CD
@L11CD:  sbc     ($00),y
         sbc     ($00)
         .byte   $f3
         .byte   $f4,$00
         sbc     $00,x
         inc     $00,x
         .byte   $f7
         sed
         sbc     $0000,y
         plx
         .byte   $fb
         .byte   $fc,$00,$00
         sbc     a:$0000,x
         inc     a:$0000,x
         .byte   $ff
