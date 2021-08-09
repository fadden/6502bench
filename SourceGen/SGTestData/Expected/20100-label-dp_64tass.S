        .cpu    "6502i"
*       =       $1000
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

L1035   ora     (L0080,x)
        jam

L1038   slo     (L0080,x)
        .byte   $04,$80
        ora     L0080
        asl     L0080
        slo     L0080
        php
        ora     #$80
        asl     a
        anc     #$80
        .byte   $0c,$86,$00
        ora     @wL0086
        asl     @wL0086
        slo     @wL0086
        bpl     _L1056
_L1056  ora     (L0080),y
        .byte   $12

L1059   slo     (L0080),y
        .byte   $14,$80
        ora     L0080,x
        asl     L0080,x
        slo     L0080,x
        clc
        ora     L0086,y
        .byte   $1a
        slo     L0086,y
        .byte   $1c,$86,$00
        ora     @wL0086,x
        asl     @wL0086,x
        slo     @wL0086,x
        jsr     L0086
        and     (L0080,x)
        .byte   $22

L107D   rla     (L0080,x)
        bit     L0080
        and     L0080
        rol     L0080
        rla     L0080
        plp
        and     #$80
        rol     a
        .byte   $2b,$80
        bit     @wL0086
        and     @wL0086
        rol     @wL0086
        rla     @wL0086
        bmi     _L109B
_L109B  and     (L0080),y
        .byte   $32

L109E   rla     (L0080),y
        .byte   $34,$80
        and     L0080,x
        rol     L0080,x
        rla     L0080,x
        sec
        and     L0086,y
        .byte   $3a
        rla     L0086,y
        .byte   $3c,$86,$00
        and     @wL0086,x
        rol     @wL0086,x
        rla     @wL0086,x
        rti

L10BD   eor     (L0080,x)
        .byte   $42

L10C0   sre     (L0080,x)
        .byte   $44,$80
        eor     L0080
        lsr     L0080
        sre     L0080
        pha
        eor     #$80
        lsr     a
        alr     #$80
        jmp     _L10D3

_L10D3  eor     @wL0086
        lsr     @wL0086
        sre     @wL0086
        bvc     _L10DE
_L10DE  eor     (L0080),y
        .byte   $52

L10E1   sre     (L0080),y
        .byte   $54,$80
        eor     L0080,x
        lsr     L0080,x
        sre     L0080,x
        cli
        eor     L0086,y
        .byte   $5a
        sre     L0086,y
        .byte   $5c,$86,$00
        eor     @wL0086,x
        lsr     @wL0086,x
        sre     @wL0086,x
        rts

L1100   adc     (L0080,x)
        .byte   $62

L1103   rra     (L0080,x)
        .byte   $64,$80
        adc     L0080
        ror     L0080
        rra     L0080
        pla
        adc     #$80
        ror     a
        arr     #$80
        jmp     (L0086)

L1116   adc     @wL0086
        ror     @wL0086
        rra     @wL0086
        bvs     _L1121
_L1121  adc     (L0080),y
        .byte   $72

L1124   rra     (L0080),y
        .byte   $74,$80
        adc     L0080,x
        ror     L0080,x
        rra     L0080,x
        sei
        adc     L0086,y
        .byte   $7a
        rra     L0086,y
        .byte   $7c,$86,$00
        adc     @wL0086,x
        ror     @wL0086,x
        rra     @wL0086,x
        .byte   $80,$80
        sta     (L0080,x)
        .byte   $82,$80
        sax     (L0080,x)
        sty     L0080
        sta     L0080
        stx     L0080
        sax     L0080
        dey
        .byte   $89,$80
        txa
        ane     #$80
        sty     @wL0086
        sta     @wL0086
        stx     @wL0086
        sax     @wL0086
        bcc     _L1166
_L1166  sta     (L0080),y
        .byte   $92

L1169   sha     (L0080),y
        sty     L0080,x
        sta     L0080,x
        stx     L0080,y
        sax     L0080,y
        tya
        sta     L0086,y
        txs
        tas     L0086,y
        shy     @wL0086,x
        sta     @wL0086,x
        shx     L0086,y
        sha     L0086,y
        ldy     #$80
        lda     (L0080,x)
        ldx     #$80
        lax     (L0080,x)
        ldy     L0080
        lda     L0080
        ldx     L0080
        lax     L0080
        tay
        lda     #$80
        tax
        lax     #$80
        ldy     @wL0086
        lda     @wL0086
        ldx     @wL0086
        lax     @wL0086
        bcs     _L11AB
_L11AB  lda     (L0080),y
        .byte   $b2

L11AE   lax     (L0080),y
        ldy     L0080,x
        lda     L0080,x
        ldx     L0080,y
        lax     L0080,y
        clv
        lda     L0086,y
        tsx
        las     L0086,y
        ldy     @wL0086,x
        lda     @wL0086,x
        ldx     @wL0086,y
        lax     @wL0086,y
        cpy     #$80
        cmp     (L0080,x)
        .byte   $c2,$80
        dcp     (L0080,x)
        cpy     L0080
        cmp     L0080
        dec     L0080
        dcp     L0080
        iny
        cmp     #$80
        dex
        sbx     #$80
        cpy     @wL0086
        cmp     @wL0086
        dec     @wL0086
        dcp     @wL0086
        bne     _L11F0
_L11F0  cmp     (L0080),y
        .byte   $d2

L11F3   dcp     (L0080),y
        .byte   $d4,$80
        cmp     L0080,x
        dec     L0080,x
        dcp     L0080,x
        cld
        cmp     L0086,y
        .byte   $da
        dcp     L0086,y
        .byte   $dc,$86,$00
        cmp     @wL0086,x
        dec     @wL0086,x
        dcp     @wL0086,x
        cpx     #$80
        sbc     (L0080,x)
        .byte   $e2,$80
        isc     (L0080,x)
        cpx     L0080
        sbc     L0080
        inc     L0080
        isc     L0080
        inx
        sbc     #$80
        nop
        .byte   $eb,$80
        cpx     @wL0086
        sbc     @wL0086
        inc     @wL0086
        isc     @wL0086
        beq     _L1235
_L1235  sbc     (L0080),y
        .byte   $f2

L1238   isc     (L0080),y
        .byte   $f4,$80
        sbc     L0080,x
        inc     L0080,x
        isc     L0080,x
        sed
        sbc     L0086,y
        .byte   $fa
        isc     L0086,y
        .byte   $fc,$86,$00
        sbc     @wL0086,x
        inc     @wL0086,x
        isc     @wL0086,x
        .logical $0080
L0080   bit     _L0082
_L0082  bit     _L0082
        bit     _L0082
L0086   bit     @wL0086
        .here
