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

         .byte   $80

L1035:   ora     (L0080,x)
         jam

L1038:   slo     (L0080,x)
         .byte   $04,$80
         ora     z:L0080
         asl     z:L0080
         slo     z:L0080
         php
         ora     #$80
         asl     A
         anc     #$80
         .byte   $0c,$86,$00
         ora     a:L0086
         asl     a:L0086
         slo     a:L0086
         bpl     @L1056
@L1056:  ora     (L0080),y
         .byte   $12

L1059:   slo     (L0080),y
         .byte   $14,$80
         ora     z:L0080,x
         asl     z:L0080,x
         slo     z:L0080,x
         clc
         ora     L0086,y
         .byte   $1a
         slo     L0086,y
         .byte   $1c,$86,$00
         ora     a:L0086,x
         asl     a:L0086,x
         slo     a:L0086,x
         jsr     L0086
         and     (L0080,x)
         .byte   $22

L107D:   rla     (L0080,x)
         bit     z:L0080
         and     z:L0080
         rol     z:L0080
         rla     z:L0080
         plp
         and     #$80
         rol     A
         .byte   $2b,$80
         bit     a:L0086
         and     a:L0086
         rol     a:L0086
         rla     a:L0086
         bmi     @L109B
@L109B:  and     (L0080),y
         .byte   $32

L109E:   rla     (L0080),y
         .byte   $34,$80
         and     z:L0080,x
         rol     z:L0080,x
         rla     z:L0080,x
         sec
         and     L0086,y
         .byte   $3a
         rla     L0086,y
         .byte   $3c,$86,$00
         and     a:L0086,x
         rol     a:L0086,x
         rla     a:L0086,x
         rti

L10BD:   eor     (L0080,x)
         .byte   $42

L10C0:   sre     (L0080,x)
         .byte   $44,$80
         eor     z:L0080
         lsr     z:L0080
         sre     z:L0080
         pha
         eor     #$80
         lsr     A
         alr     #$80
         jmp     @L10D3

@L10D3:  eor     a:L0086
         lsr     a:L0086
         sre     a:L0086
         bvc     @L10DE
@L10DE:  eor     (L0080),y
         .byte   $52

L10E1:   sre     (L0080),y
         .byte   $54,$80
         eor     z:L0080,x
         lsr     z:L0080,x
         sre     z:L0080,x
         cli
         eor     L0086,y
         .byte   $5a
         sre     L0086,y
         .byte   $5c,$86,$00
         eor     a:L0086,x
         lsr     a:L0086,x
         sre     a:L0086,x
         rts

L1100:   adc     (L0080,x)
         .byte   $62

L1103:   rra     (L0080,x)
         .byte   $64,$80
         adc     z:L0080
         ror     z:L0080
         rra     z:L0080
         pla
         adc     #$80
         ror     A
         arr     #$80
         jmp     (L0086)

L1116:   adc     a:L0086
         ror     a:L0086
         rra     a:L0086
         bvs     @L1121
@L1121:  adc     (L0080),y
         .byte   $72

L1124:   rra     (L0080),y
         .byte   $74,$80
         adc     z:L0080,x
         ror     z:L0080,x
         rra     z:L0080,x
         sei
         adc     L0086,y
         .byte   $7a
         rra     L0086,y
         .byte   $7c,$86,$00
         adc     a:L0086,x
         ror     a:L0086,x
         rra     a:L0086,x
         .byte   $80,$80
         sta     (L0080,x)
         .byte   $82,$80
         sax     (L0080,x)
         sty     z:L0080
         sta     z:L0080
         stx     z:L0080
         sax     z:L0080
         dey
         .byte   $89,$80
         txa
         ane     #$80
         sty     a:L0086
         sta     a:L0086
         stx     a:L0086
         sax     a:L0086
         bcc     @L1166
@L1166:  sta     (L0080),y
         .byte   $92

L1169:   sha     (L0080),y
         sty     z:L0080,x
         sta     z:L0080,x
         stx     z:L0080,y
         sax     z:L0080,y
         tya
         sta     L0086,y
         txs
         tas     L0086,y
         shy     a:L0086,x
         sta     a:L0086,x
         shx     L0086,y
         sha     L0086,y
         ldy     #$80
         lda     (L0080,x)
         ldx     #$80
         lax     (L0080,x)
         ldy     z:L0080
         lda     z:L0080
         ldx     z:L0080
         lax     z:L0080
         tay
         lda     #$80
         tax
         lax     #$80
         ldy     a:L0086
         lda     a:L0086
         ldx     a:L0086
         lax     a:L0086
         bcs     @L11AB
@L11AB:  lda     (L0080),y
         .byte   $b2

L11AE:   lax     (L0080),y
         ldy     z:L0080,x
         lda     z:L0080,x
         ldx     z:L0080,y
         lax     z:L0080,y
         clv
         lda     L0086,y
         tsx
         las     L0086,y
         ldy     a:L0086,x
         lda     a:L0086,x
         ldx     a:L0086,y
         lax     a:L0086,y
         cpy     #$80
         cmp     (L0080,x)
         .byte   $c2,$80
         dcp     (L0080,x)
         cpy     z:L0080
         cmp     z:L0080
         dec     z:L0080
         dcp     z:L0080
         iny
         cmp     #$80
         dex
         axs     #$80
         cpy     a:L0086
         cmp     a:L0086
         dec     a:L0086
         dcp     a:L0086
         bne     @L11F0
@L11F0:  cmp     (L0080),y
         .byte   $d2

L11F3:   dcp     (L0080),y
         .byte   $d4,$80
         cmp     z:L0080,x
         dec     z:L0080,x
         dcp     z:L0080,x
         cld
         cmp     L0086,y
         .byte   $da
         dcp     L0086,y
         .byte   $dc,$86,$00
         cmp     a:L0086,x
         dec     a:L0086,x
         dcp     a:L0086,x
         cpx     #$80
         sbc     (L0080,x)
         .byte   $e2,$80
         isc     (L0080,x)
         cpx     z:L0080
         sbc     z:L0080
         inc     z:L0080
         isc     z:L0080
         inx
         sbc     #$80
         nop
         .byte   $eb,$80
         cpx     a:L0086
         sbc     a:L0086
         inc     a:L0086
         isc     a:L0086
         beq     @L1235
@L1235:  sbc     (L0080),y
         .byte   $f2

L1238:   isc     (L0080),y
         .byte   $f4,$80
         sbc     z:L0080,x
         inc     z:L0080,x
         isc     z:L0080,x
         sed
         sbc     L0086,y
         .byte   $fa
         isc     L0086,y
         .byte   $fc,$86,$00
         sbc     a:L0086,x
         inc     a:L0086,x
         isc     a:L0086,x
         .org    $0080
L0080:   bit     z:@L0082
@L0082:  bit     @L0082
         bit     @L0082
L0086:   bit     a:L0086
