        .cpu    "65816"
*       =       $1000
        .as
        .xs
        sec
        xce
        jsr     L101F
        jsr     L10AB
        jsr     L10F2
        jsr     L1106
        jsr     L1109
        jsr     L112C
        jsr     L11F9
        jsr     L11FC
        nop
        nop
        nop
        .byte   $00,$ff

L101F   ora     ($ff,x)
        .byte   $02,$ff
        ora     $ff,s
        tsb     $ff
        ora     $ff
        asl     $ff
        ora     [$ff]
        php
        ora     #$ff
        asl     a
        phd
        tsb     $feff
        ora     $feff
        asl     $feff
        ora     $fdfeff
        bpl     L1041
L1041   ora     ($ff),y
        ora     ($ff)
        ora     ($ff,s),y
        trb     $ff
        ora     $ff,x
        asl     $ff,x
        ora     [$ff],y
        clc
        ora     $feff,y
        inc     a
        tcs
        trb     $feff
        ora     $feff,x
        asl     $feff,x
        ora     $fdfeff,x
        jsr     $feff
        and     ($ff,x)
        jsl     $fdfeff
        and     $ff,s
        bit     $ff
        and     $ff
        rol     $ff
        and     [$ff]
        plp
        and     #$ff
        rol     a
        pld
        bit     $feff
        and     $feff
        rol     $feff
        and     $fdfeff
        bmi     L1089
L1089   and     ($ff),y
        and     ($ff)
        and     ($ff,s),y
        bit     $ff,x
        and     $ff,x
        rol     $ff,x
        and     [$ff],y
        sec
        and     $feff,y
        dec     a
        tsc
        bit     $feff,x
        and     $feff,x
        rol     $feff,x
        and     $fdfeff,x
        rti

L10AB   eor     ($ff,x)
        .byte   $42,$ff
        eor     $ff,s
        mvp     $fe,$ff
        eor     $ff
        lsr     $ff
        eor     [$ff]
        pha
        eor     #$ff
        lsr     a
        phk
        jmp     L10C2

L10C2   eor     $feff
        lsr     $feff
        eor     $fdfeff
        bvc     L10CE
L10CE   eor     ($ff),y
        eor     ($ff)
        eor     ($ff,s),y
        mvn     $fe,$ff
        eor     $ff,x
        lsr     $ff,x
        eor     [$ff],y
        cli
        eor     $feff,y
        phy
        tcd
        jml     L10E7

L10E7   eor     $feff,x
        lsr     $feff,x
        eor     $fdfeff,x
        rts

L10F2   adc     ($ff,x)
        per     $0ff6
        adc     $ff,s
        stz     $ff
        adc     $ff
        ror     $ff
        adc     [$ff]
        pla
        adc     #$ff
        ror     a
        rtl

L1106   jmp     ($feff)

L1109   adc     $feff
        ror     $feff
        adc     $fdfeff
        bvs     L1115
L1115   adc     ($ff),y
        adc     ($ff)
        adc     ($ff,s),y
        stz     $ff,x
        adc     $ff,x
        ror     $ff,x
        adc     [$ff],y
        sei
        adc     $feff,y
        ply
        tdc
        jmp     ($feff,x)

L112C   adc     $feff,x
        ror     $feff,x
        adc     $fdfeff,x
        bra     L1138

L1138   sta     ($ff,x)
        brl     L113D

L113D   sta     $ff,s
        sty     $ff
        sta     $ff
        stx     $ff
        sta     [$ff]
        dey
        bit     #$ff
        txa
        phb
        sty     $feff
        sta     $feff
        stx     $feff
        sta     $fdfeff
        bcc     L115B
L115B   sta     ($ff),y
        sta     ($ff)
        sta     ($ff,s),y
        sty     $ff,x
        sta     $ff,x
        stx     $ff,y
        sta     [$ff],y
        tya
        sta     $feff,y
        txs
        txy
        stz     $feff
        sta     $feff,x
        stz     $feff,x
        sta     $fdfeff,x
        ldy     #$ff
        lda     ($ff,x)
        ldx     #$ff
        lda     $ff,s
        ldy     $ff
        lda     $ff
        ldx     $ff
        lda     [$ff]
        tay
        lda     #$ff
        tax
        plb
        ldy     $feff
        lda     $feff
        ldx     $feff
        lda     $fdfeff
        bcs     L11A0
L11A0   lda     ($ff),y
        lda     ($ff)
        lda     ($ff,s),y
        ldy     $ff,x
        lda     $ff,x
        ldx     $ff,y
        lda     [$ff],y
        clv
        lda     $feff,y
        tsx
        tyx
        ldy     $feff,x
        lda     $feff,x
        ldx     $feff,y
        lda     $fdfeff,x
        cpy     #$ff
        cmp     ($ff,x)
        rep     #$00
        cmp     $ff,s
        cpy     $ff
        cmp     $ff
        dec     $ff
        cmp     [$ff]
        iny
        cmp     #$ff
        dex
        wai
        cpy     $feff
        cmp     $feff
        dec     $feff
        cmp     $fdfeff
        bne     L11E5
L11E5   cmp     ($ff),y
        cmp     ($ff)
        cmp     ($ff,s),y
        pei     ($ff)
        cmp     $ff,x
        dec     $ff,x
        cmp     [$ff],y
        cld
        cmp     $feff,y
        phx
        stp

L11F9   jml     [$feff]

L11FC   cmp     $feff,x
        dec     $feff,x
        cmp     $fdfeff,x
        cpx     #$ff
        sbc     ($ff,x)
        sep     #$00
        sbc     $ff,s
        cpx     $ff
        sbc     $ff
        inc     $ff
        sbc     [$ff]
        inx
        sbc     #$ff
        nop
        xba
        cpx     $feff
        sbc     $feff
        inc     $feff
        sbc     $fdfeff
        beq     L122A
L122A   sbc     ($ff),y
        sbc     ($ff)
        sbc     ($ff,s),y
        pea     $feff
        sbc     $ff,x
        inc     $ff,x
        sbc     [$ff],y
        sed
        sbc     $feff,y
        plx
        xce
        jsr     ($feff,x)
        sbc     $feff,x
        inc     $feff,x
        sbc     $fdfeff,x
