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

         .byte   $ff

L1035:   ora     ($ff,x)
         jam

L1038:   slo     ($ff,x)
         .byte   $04,$ff
         ora     $ff
         asl     $ff
         slo     $ff
         php
         ora     #$ff
         asl     A
         anc     #$ff
         .byte   $0c,$ff,$fe
         ora     $feff
         asl     $feff
         slo     $feff
         bpl     @L1056
@L1056:  ora     ($ff),y
         .byte   $12

L1059:   slo     ($ff),y
         .byte   $14,$ff
         ora     $ff,x
         asl     $ff,x
         slo     $ff,x
         clc
         ora     $feff,y
         .byte   $1a
         slo     $feff,y
         .byte   $1c,$ff,$fe
         ora     $feff,x
         asl     $feff,x
         slo     $feff,x
         jsr     $feff
         and     ($ff,x)
         .byte   $22

L107D:   rla     ($ff,x)
         bit     $ff
         and     $ff
         rol     $ff
         rla     $ff
         plp
         and     #$ff
         rol     A
         .byte   $2b,$ff
         bit     $feff
         and     $feff
         rol     $feff
         rla     $feff
         bmi     @L109B
@L109B:  and     ($ff),y
         .byte   $32

L109E:   rla     ($ff),y
         .byte   $34,$ff
         and     $ff,x
         rol     $ff,x
         rla     $ff,x
         sec
         and     $feff,y
         .byte   $3a
         rla     $feff,y
         .byte   $3c,$ff,$fe
         and     $feff,x
         rol     $feff,x
         rla     $feff,x
         rti

L10BD:   eor     ($ff,x)
         .byte   $42

L10C0:   sre     ($ff,x)
         .byte   $44,$ff
         eor     $ff
         lsr     $ff
         sre     $ff
         pha
         eor     #$ff
         lsr     A
         alr     #$ff
         jmp     @L10D3

@L10D3:  eor     $feff
         lsr     $feff
         sre     $feff
         bvc     @L10DE
@L10DE:  eor     ($ff),y
         .byte   $52

L10E1:   sre     ($ff),y
         .byte   $54,$ff
         eor     $ff,x
         lsr     $ff,x
         sre     $ff,x
         cli
         eor     $feff,y
         .byte   $5a
         sre     $feff,y
         .byte   $5c,$ff,$fe
         eor     $feff,x
         lsr     $feff,x
         sre     $feff,x
         rts

L1100:   adc     ($ff,x)
         .byte   $62

L1103:   rra     ($ff,x)
         .byte   $64,$ff
         adc     $ff
         ror     $ff
         rra     $ff
         pla
         adc     #$ff
         ror     A
         arr     #$ff
         jmp     ($feff)

L1116:   adc     $feff
         ror     $feff
         rra     $feff
         bvs     @L1121
@L1121:  adc     ($ff),y
         .byte   $72

L1124:   rra     ($ff),y
         .byte   $74,$ff
         adc     $ff,x
         ror     $ff,x
         rra     $ff,x
         sei
         adc     $feff,y
         .byte   $7a
         rra     $feff,y
         .byte   $7c,$ff,$fe
         adc     $feff,x
         ror     $feff,x
         rra     $feff,x
         .byte   $80,$ff
         sta     ($ff,x)
         .byte   $82,$ff
         sax     ($ff,x)
         sty     $ff
         sta     $ff
         stx     $ff
         sax     $ff
         dey
         .byte   $89,$ff
         txa
         ane     #$ff
         sty     $feff
         sta     $feff
         stx     $feff
         sax     $feff
         bcc     @L1166
@L1166:  sta     ($ff),y
         .byte   $92

L1169:   sha     ($ff),y
         sty     $ff,x
         sta     $ff,x
         stx     $ff,y
         sax     $ff,y
         tya
         sta     $feff,y
         txs
         tas     $feff,y
         shy     $feff,x
         sta     $feff,x
         shx     $feff,y
         sha     $feff,y
         ldy     #$ff
         lda     ($ff,x)
         ldx     #$ff
         lax     ($ff,x)
         ldy     $ff
         lda     $ff
         ldx     $ff
         lax     $ff
         tay
         lda     #$ff
         tax
         lax     #$ff
         ldy     $feff
         lda     $feff
         ldx     $feff
         lax     $feff
         bcs     @L11AB
@L11AB:  lda     ($ff),y
         .byte   $b2

L11AE:   lax     ($ff),y
         ldy     $ff,x
         lda     $ff,x
         ldx     $ff,y
         lax     $ff,y
         clv
         lda     $feff,y
         tsx
         las     $feff,y
         ldy     $feff,x
         lda     $feff,x
         ldx     $feff,y
         lax     $feff,y
         cpy     #$ff
         cmp     ($ff,x)
         .byte   $c2,$ff
         dcp     ($ff,x)
         cpy     $ff
         cmp     $ff
         dec     $ff
         dcp     $ff
         iny
         cmp     #$ff
         dex
         axs     #$ff
         cpy     $feff
         cmp     $feff
         dec     $feff
         dcp     $feff
         bne     @L11F0
@L11F0:  cmp     ($ff),y
         .byte   $d2

L11F3:   dcp     ($ff),y
         .byte   $d4,$ff
         cmp     $ff,x
         dec     $ff,x
         dcp     $ff,x
         cld
         cmp     $feff,y
         .byte   $da
         dcp     $feff,y
         .byte   $dc,$ff,$fe
         cmp     $feff,x
         dec     $feff,x
         dcp     $feff,x
         cpx     #$ff
         sbc     ($ff,x)
         .byte   $e2,$ff
         isc     ($ff,x)
         cpx     $ff
         sbc     $ff
         inc     $ff
         isc     $ff
         inx
         sbc     #$ff
         nop
         .byte   $eb,$ff
         cpx     $feff
         sbc     $feff
         inc     $feff
         isc     $feff
         beq     @L1235
@L1235:  sbc     ($ff),y
         .byte   $f2

L1238:   isc     ($ff),y
         .byte   $f4,$ff
         sbc     $ff,x
         inc     $ff,x
         isc     $ff,x
         sed
         sbc     $feff,y
         .byte   $fa
         isc     $feff,y
         .byte   $fc,$ff,$fe
         sbc     $feff,x
         inc     $feff,x
         isc     $feff,x
