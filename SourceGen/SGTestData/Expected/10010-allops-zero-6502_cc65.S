         .setcpu "6502X"
         .org    $1000
         jsr     L1035
         jsr     L1038
         jsr     L1059
         jsr     L107D
         jsr     L109E
         jsr     L10BD
         jsr     L10C0
         jsr     L10E1
         jsr     L1100
         jsr     L1103
         jsr     L1116
         jsr     L1124
         jsr     L1169
         jsr     L11AE
         jsr     L11F3
         jsr     L1238
         nop
         nop
         nop
         brk

         .byte   $00

L1035:   ora     ($00,x)
         jam

L1038:   slo     ($00,x)
         .byte   $04,$00
         ora     $00
         asl     $00
         slo     $00
         php
         ora     #$00
         asl     A
         anc     #$00
         .byte   $0c,$00,$00
         ora     a:$0000
         asl     a:$0000
         slo     a:$0000
         bpl     @L1056
@L1056:  ora     ($00),y
         .byte   $12

L1059:   slo     ($00),y
         .byte   $14,$00
         ora     $00,x
         asl     $00,x
         slo     $00,x
         clc
         ora     $0000,y
         .byte   $1a
         slo     $0000,y
         .byte   $1c,$00,$00
         ora     a:$0000,x
         asl     a:$0000,x
         slo     a:$0000,x
         jsr     $0000
         and     ($00,x)
         .byte   $22

L107D:   rla     ($00,x)
         bit     $00
         and     $00
         rol     $00
         rla     $00
         plp
         and     #$00
         rol     A
         .byte   $2b,$00
         bit     a:$0000
         and     a:$0000
         rol     a:$0000
         rla     a:$0000
         bmi     @L109B
@L109B:  and     ($00),y
         .byte   $32

L109E:   rla     ($00),y
         .byte   $34,$00
         and     $00,x
         rol     $00,x
         rla     $00,x
         sec
         and     $0000,y
         .byte   $3a
         rla     $0000,y
         .byte   $3c,$00,$00
         and     a:$0000,x
         rol     a:$0000,x
         rla     a:$0000,x
         rti

L10BD:   eor     ($00,x)
         .byte   $42

L10C0:   sre     ($00,x)
         .byte   $44,$00
         eor     $00
         lsr     $00
         sre     $00
         pha
         eor     #$00
         lsr     A
         alr     #$00
         jmp     @L10D3

@L10D3:  eor     a:$0000
         lsr     a:$0000
         sre     a:$0000
         bvc     @L10DE
@L10DE:  eor     ($00),y
         .byte   $52

L10E1:   sre     ($00),y
         .byte   $54,$00
         eor     $00,x
         lsr     $00,x
         sre     $00,x
         cli
         eor     $0000,y
         .byte   $5a
         sre     $0000,y
         .byte   $5c,$00,$00
         eor     a:$0000,x
         lsr     a:$0000,x
         sre     a:$0000,x
         rts

L1100:   adc     ($00,x)
         .byte   $62

L1103:   rra     ($00,x)
         .byte   $64,$00
         adc     $00
         ror     $00
         rra     $00
         pla
         adc     #$00
         ror     A
         arr     #$00
         jmp     ($0000)

L1116:   adc     a:$0000
         ror     a:$0000
         rra     a:$0000
         bvs     @L1121
@L1121:  adc     ($00),y
         .byte   $72

L1124:   rra     ($00),y
         .byte   $74,$00
         adc     $00,x
         ror     $00,x
         rra     $00,x
         sei
         adc     $0000,y
         .byte   $7a
         rra     $0000,y
         .byte   $7c,$00,$00
         adc     a:$0000,x
         ror     a:$0000,x
         rra     a:$0000,x
         .byte   $80,$00
         sta     ($00,x)
         .byte   $82,$00
         sax     ($00,x)
         sty     $00
         sta     $00
         stx     $00
         sax     $00
         dey
         .byte   $89,$00
         txa
         ane     #$00
         sty     a:$0000
         sta     a:$0000
         stx     a:$0000
         sax     a:$0000
         bcc     @L1166
@L1166:  sta     ($00),y
         .byte   $92

L1169:   sha     ($00),y
         sty     $00,x
         sta     $00,x
         stx     $00,y
         sax     $00,y
         tya
         sta     $0000,y
         txs
         tas     $0000,y
         shy     a:$0000,x
         sta     a:$0000,x
         shx     $0000,y
         sha     $0000,y
         ldy     #$00
         lda     ($00,x)
         ldx     #$00
         lax     ($00,x)
         ldy     $00
         lda     $00
         ldx     $00
         lax     $00
         tay
         lda     #$00
         tax
         lax     #$00
         ldy     a:$0000
         lda     a:$0000
         ldx     a:$0000
         lax     a:$0000
         bcs     @L11AB
@L11AB:  lda     ($00),y
         .byte   $b2

L11AE:   lax     ($00),y
         ldy     $00,x
         lda     $00,x
         ldx     $00,y
         lax     $00,y
         clv
         lda     $0000,y
         tsx
         las     $0000,y
         ldy     a:$0000,x
         lda     a:$0000,x
         ldx     a:$0000,y
         lax     a:$0000,y
         cpy     #$00
         cmp     ($00,x)
         .byte   $c2,$00
         dcp     ($00,x)
         cpy     $00
         cmp     $00
         dec     $00
         dcp     $00
         iny
         cmp     #$00
         dex
         axs     #$00
         cpy     a:$0000
         cmp     a:$0000
         dec     a:$0000
         dcp     a:$0000
         bne     @L11F0
@L11F0:  cmp     ($00),y
         .byte   $d2

L11F3:   dcp     ($00),y
         .byte   $d4,$00
         cmp     $00,x
         dec     $00,x
         dcp     $00,x
         cld
         cmp     $0000,y
         .byte   $da
         dcp     $0000,y
         .byte   $dc,$00,$00
         cmp     a:$0000,x
         dec     a:$0000,x
         dcp     a:$0000,x
         cpx     #$00
         sbc     ($00,x)
         .byte   $e2,$00
         isc     ($00,x)
         cpx     $00
         sbc     $00
         inc     $00
         isc     $00
         inx
         sbc     #$00
         nop
         .byte   $eb,$00
         cpx     a:$0000
         sbc     a:$0000
         inc     a:$0000
         isc     a:$0000
         beq     @L1235
@L1235:  sbc     ($00),y
         .byte   $f2

L1238:   isc     ($00),y
         .byte   $f4,$00
         sbc     $00,x
         inc     $00,x
         isc     $00,x
         sed
         sbc     $0000,y
         .byte   $fa
         isc     $0000,y
         .byte   $fc,$00,$00
         sbc     a:$0000,x
         inc     a:$0000,x
         isc     a:$0000,x
