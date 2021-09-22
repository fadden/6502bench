        !cpu    6510
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

        !byte   $80

L1035   ora     (L0080,x)
        jam

L1038   slo     (L0080,x)
        !byte   $04,$80
        ora+1   L0080
        asl+1   L0080
        slo+1   L0080
        php
        ora     #$80
        asl
        anc     #$80
        !byte   $0c,$86,$00
        ora+2   L0086
        asl+2   L0086
        slo+2   L0086
        bpl     @L1056
@L1056  ora     (L0080),y
        !byte   $12

L1059   slo     (L0080),y
        !byte   $14,$80
        ora+1   L0080,x
        asl+1   L0080,x
        slo+1   L0080,x
        clc
        ora     L0086,y
        !byte   $1a
        slo     L0086,y
        !byte   $1c,$86,$00
        ora+2   L0086,x
        asl+2   L0086,x
        slo+2   L0086,x
        jsr     L0086
        and     (L0080,x)
        !byte   $22

L107D   rla     (L0080,x)
        bit+1   L0080
        and+1   L0080
        rol+1   L0080
        rla+1   L0080
        plp
        and     #$80
        rol
        !byte   $2b,$80
        bit+2   L0086
        and+2   L0086
        rol+2   L0086
        rla+2   L0086
        bmi     @L109B
@L109B  and     (L0080),y
        !byte   $32

L109E   rla     (L0080),y
        !byte   $34,$80
        and+1   L0080,x
        rol+1   L0080,x
        rla+1   L0080,x
        sec
        and     L0086,y
        !byte   $3a
        rla     L0086,y
        !byte   $3c,$86,$00
        and+2   L0086,x
        rol+2   L0086,x
        rla+2   L0086,x
        rti

L10BD   eor     (L0080,x)
        !byte   $42

L10C0   sre     (L0080,x)
        !byte   $44,$80
        eor+1   L0080
        lsr+1   L0080
        sre+1   L0080
        pha
        eor     #$80
        lsr
        asr     #$80
        jmp     @L10D3

@L10D3  eor+2   L0086
        lsr+2   L0086
        sre+2   L0086
        bvc     @L10DE
@L10DE  eor     (L0080),y
        !byte   $52

L10E1   sre     (L0080),y
        !byte   $54,$80
        eor+1   L0080,x
        lsr+1   L0080,x
        sre+1   L0080,x
        cli
        eor     L0086,y
        !byte   $5a
        sre     L0086,y
        !byte   $5c,$86,$00
        eor+2   L0086,x
        lsr+2   L0086,x
        sre+2   L0086,x
        rts

L1100   adc     (L0080,x)
        !byte   $62

L1103   rra     (L0080,x)
        !byte   $64,$80
        adc+1   L0080
        ror+1   L0080
        rra+1   L0080
        pla
        adc     #$80
        ror
        arr     #$80
        jmp     (L0086)

L1116   adc+2   L0086
        ror+2   L0086
        rra+2   L0086
        bvs     @L1121
@L1121  adc     (L0080),y
        !byte   $72

L1124   rra     (L0080),y
        !byte   $74,$80
        adc+1   L0080,x
        ror+1   L0080,x
        rra+1   L0080,x
        sei
        adc     L0086,y
        !byte   $7a
        rra     L0086,y
        !byte   $7c,$86,$00
        adc+2   L0086,x
        ror+2   L0086,x
        rra+2   L0086,x
        !byte   $80,$80
        sta     (L0080,x)
        !byte   $82,$80
        sax     (L0080,x)
        sty+1   L0080
        sta+1   L0080
        stx+1   L0080
        sax+1   L0080
        dey
        !byte   $89,$80
        txa
        ane     #$80
        sty+2   L0086
        sta+2   L0086
        stx+2   L0086
        sax+2   L0086
        bcc     @L1166
@L1166  sta     (L0080),y
        !byte   $92

L1169   sha     (L0080),y
        sty+1   L0080,x
        sta+1   L0080,x
        stx+1   L0080,y
        sax+1   L0080,y
        tya
        sta     L0086,y
        txs
        tas     L0086,y
        shy+2   L0086,x
        sta+2   L0086,x
        shx     L0086,y
        sha     L0086,y
        ldy     #$80
        lda     (L0080,x)
        ldx     #$80
        lax     (L0080,x)
        ldy+1   L0080
        lda+1   L0080
        ldx+1   L0080
        lax+1   L0080
        tay
        lda     #$80
        tax
        !byte   $ab,$80
        ldy+2   L0086
        lda+2   L0086
        ldx+2   L0086
        lax+2   L0086
        bcs     @L11AB
@L11AB  lda     (L0080),y
        !byte   $b2

L11AE   lax     (L0080),y
        ldy+1   L0080,x
        lda+1   L0080,x
        ldx+1   L0080,y
        lax+1   L0080,y
        clv
        lda     L0086,y
        tsx
        las     L0086,y
        ldy+2   L0086,x
        lda+2   L0086,x
        ldx+2   L0086,y
        lax+2   L0086,y
        cpy     #$80
        cmp     (L0080,x)
        !byte   $c2,$80
        dcp     (L0080,x)
        cpy+1   L0080
        cmp+1   L0080
        dec+1   L0080
        dcp+1   L0080
        iny
        cmp     #$80
        dex
        sbx     #$80
        cpy+2   L0086
        cmp+2   L0086
        dec+2   L0086
        dcp+2   L0086
        bne     @L11F0
@L11F0  cmp     (L0080),y
        !byte   $d2

L11F3   dcp     (L0080),y
        !byte   $d4,$80
        cmp+1   L0080,x
        dec+1   L0080,x
        dcp+1   L0080,x
        cld
        cmp     L0086,y
        !byte   $da
        dcp     L0086,y
        !byte   $dc,$86,$00
        cmp+2   L0086,x
        dec+2   L0086,x
        dcp+2   L0086,x
        cpx     #$80
        sbc     (L0080,x)
        !byte   $e2,$80
        isc     (L0080,x)
        cpx+1   L0080
        sbc+1   L0080
        inc+1   L0080
        isc+1   L0080
        inx
        sbc     #$80
        nop
        !byte   $eb,$80
        cpx+2   L0086
        sbc+2   L0086
        inc+2   L0086
        isc+2   L0086
        beq     @L1235
@L1235  sbc     (L0080),y
        !byte   $f2

L1238   isc     (L0080),y
        !byte   $f4,$80
        sbc+1   L0080,x
        inc+1   L0080,x
        isc+1   L0080,x
        sed
        sbc     L0086,y
        !byte   $fa
        isc     L0086,y
        !byte   $fc,$86,$00
        sbc+2   L0086,x
        inc+2   L0086,x
        isc+2   L0086,x
        !pseudopc $0080 {
L0080   bit+1   @L0082
@L0082  bit+1   @L0082
        bit+1   @L0082
L0086   bit+2   L0086
        }
