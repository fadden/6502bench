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
        brk

        .byte   $00

L101F   ora     ($00,x)
        cop     #$00
        ora     $00,s
        tsb     $00
        ora     $00
        asl     $00
        ora     [$00]
        php
        ora     #$00
        asl     a
        phd
        tsb     @w$0000
        ora     @w$0000
        asl     @w$0000
        ora     @l$000000
        bpl     _L1041
_L1041  ora     ($00),y
        ora     ($00)
        ora     ($00,s),y
        trb     $00
        ora     $00,x
        asl     $00,x
        ora     [$00],y
        clc
        ora     $0000,y
        inc     a
        tcs
        trb     @w$0000
        ora     @w$0000,x
        asl     @w$0000,x
        ora     @l$000000,x
        jsr     $0000
        and     ($00,x)
        jsl     $000000
        and     $00,s
        bit     $00
        and     $00
        rol     $00
        and     [$00]
        plp
        and     #$00
        rol     a
        pld
        bit     @w$0000
        and     @w$0000
        rol     @w$0000
        and     @l$000000
        bmi     _L1089
_L1089  and     ($00),y
        and     ($00)
        and     ($00,s),y
        bit     $00,x
        and     $00,x
        rol     $00,x
        and     [$00],y
        sec
        and     $0000,y
        dec     a
        tsc
        bit     @w$0000,x
        and     @w$0000,x
        rol     @w$0000,x
        and     @l$000000,x
        rti

L10AB   eor     ($00,x)
        .byte   $42,$00
        eor     $00,s
        mvp     #$00,#$00
        eor     $00
        lsr     $00
        eor     [$00]
        pha
        eor     #$00
        lsr     a
        phk
        jmp     _L10C2

_L10C2  eor     @w$0000
        lsr     @w$0000
        eor     @l$000000
        bvc     _L10CE
_L10CE  eor     ($00),y
        eor     ($00)
        eor     ($00,s),y
        mvn     #$00,#$00
        eor     $00,x
        lsr     $00,x
        eor     [$00],y
        cli
        eor     $0000,y
        phy
        tcd
        jml     _L10E7

_L10E7  eor     @w$0000,x
        lsr     @w$0000,x
        eor     @l$000000,x
        rts

L10F2   adc     ($00,x)
        per     $0ff6
        adc     $00,s
        stz     $00
        adc     $00
        ror     $00
        adc     [$00]
        pla
        adc     #$00
        ror     a
        rtl

L1106   jmp     ($0000)

L1109   adc     @w$0000
        ror     @w$0000
        adc     @l$000000
        bvs     _L1115
_L1115  adc     ($00),y
        adc     ($00)
        adc     ($00,s),y
        stz     $00,x
        adc     $00,x
        ror     $00,x
        adc     [$00],y
        sei
        adc     $0000,y
        ply
        tdc
        jmp     ($0000,x)

L112C   adc     @w$0000,x
        ror     @w$0000,x
        adc     @l$000000,x
        bra     _L1138

_L1138  sta     ($00,x)
        brl     _L113D

_L113D  sta     $00,s
        sty     $00
        sta     $00
        stx     $00
        sta     [$00]
        dey
        bit     #$00
        txa
        phb
        sty     @w$0000
        sta     @w$0000
        stx     @w$0000
        sta     @l$000000
        bcc     _L115B
_L115B  sta     ($00),y
        sta     ($00)
        sta     ($00,s),y
        sty     $00,x
        sta     $00,x
        stx     $00,y
        sta     [$00],y
        tya
        sta     $0000,y
        txs
        txy
        stz     @w$0000
        sta     @w$0000,x
        stz     @w$0000,x
        sta     @l$000000,x
        ldy     #$00
        lda     ($00,x)
        ldx     #$00
        lda     $00,s
        ldy     $00
        lda     $00
        ldx     $00
        lda     [$00]
        tay
        lda     #$00
        tax
        plb
        ldy     @w$0000
        lda     @w$0000
        ldx     @w$0000
        lda     @l$000000
        bcs     _L11A0
_L11A0  lda     ($00),y
        lda     ($00)
        lda     ($00,s),y
        ldy     $00,x
        lda     $00,x
        ldx     $00,y
        lda     [$00],y
        clv
        lda     $0000,y
        tsx
        tyx
        ldy     @w$0000,x
        lda     @w$0000,x
        ldx     @w$0000,y
        lda     @l$000000,x
        cpy     #$00
        cmp     ($00,x)
        rep     #$00
        cmp     $00,s
        cpy     $00
        cmp     $00
        dec     $00
        cmp     [$00]
        iny
        cmp     #$00
        dex
        wai
        cpy     @w$0000
        cmp     @w$0000
        dec     @w$0000
        cmp     @l$000000
        bne     _L11E5
_L11E5  cmp     ($00),y
        cmp     ($00)
        cmp     ($00,s),y
        pei     ($00)
        cmp     $00,x
        dec     $00,x
        cmp     [$00],y
        cld
        cmp     $0000,y
        phx
        stp

L11F9   jml     [$0000]

L11FC   cmp     @w$0000,x
        dec     @w$0000,x
        cmp     @l$000000,x
        cpx     #$00
        sbc     ($00,x)
        sep     #$00
        sbc     $00,s
        cpx     $00
        sbc     $00
        inc     $00
        sbc     [$00]
        inx
        sbc     #$00
        nop
        xba
        cpx     @w$0000
        sbc     @w$0000
        inc     @w$0000
        sbc     @l$000000
        beq     _L122A
_L122A  sbc     ($00),y
        sbc     ($00)
        sbc     ($00,s),y
        pea     $0000
        sbc     $00,x
        inc     $00,x
        sbc     [$00],y
        sed
        sbc     $0000,y
        plx
        xce
        jsr     ($0000,x)
        sbc     @w$0000,x
        inc     @w$0000,x
        sbc     @l$000000,x
