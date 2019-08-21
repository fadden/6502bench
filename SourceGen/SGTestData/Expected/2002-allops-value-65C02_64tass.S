        .cpu    "65c02"
*       =       $1000
        jsr     L1014
        jsr     L108A
        jsr     L10C4
        jsr     L10D8
        jsr     L10F6
        nop
        nop
        nop
        brk

        .byte   $ff

L1014   ora     ($ff,x)
        .byte   $02,$ff
        .byte   $03
        tsb     $ff
        ora     $ff
        asl     $ff
        .byte   $07
        php
        ora     #$ff
        asl     a
        .byte   $0b
        tsb     $feff
        ora     $feff
        asl     $feff
        .byte   $0f
        bpl     L1031
L1031   ora     ($ff),y
        ora     ($ff)
        .byte   $13
        trb     $ff
        ora     $ff,x
        asl     $ff,x
        .byte   $17
        clc
        ora     $feff,y
        inc     a
        .byte   $1b
        trb     $feff
        ora     $feff,x
        asl     $feff,x
        .byte   $1f
        jsr     $feff
        and     ($ff,x)
        .byte   $22,$ff
        .byte   $23
        bit     $ff
        and     $ff
        rol     $ff
        .byte   $27
        plp
        and     #$ff
        rol     a
        .byte   $2b
        bit     $feff
        and     $feff
        rol     $feff
        .byte   $2f
        bmi     L106D
L106D   and     ($ff),y
        and     ($ff)
        .byte   $33
        bit     $ff,x
        and     $ff,x
        rol     $ff,x
        .byte   $37
        sec
        and     $feff,y
        dec     a
        .byte   $3b
        bit     $feff,x
        and     $feff,x
        rol     $feff,x
        .byte   $3f
        rti

L108A   eor     ($ff,x)
        .byte   $42,$ff
        .byte   $43
        .byte   $44,$ff
        eor     $ff
        lsr     $ff
        .byte   $47
        pha
        eor     #$ff
        lsr     a
        .byte   $4b
        jmp     L109E

L109E   eor     $feff
        lsr     $feff
        .byte   $4f
        bvc     L10A7
L10A7   eor     ($ff),y
        eor     ($ff)
        .byte   $53
        .byte   $54,$ff
        eor     $ff,x
        lsr     $ff,x
        .byte   $57
        cli
        eor     $feff,y
        phy
        .byte   $5b
        .byte   $5c,$ff,$fe
        eor     $feff,x
        lsr     $feff,x
        .byte   $5f
        rts

L10C4   adc     ($ff,x)
        .byte   $62,$ff
        .byte   $63
        stz     $ff
        adc     $ff
        ror     $ff
        .byte   $67
        pla
        adc     #$ff
        ror     a
        .byte   $6b
        jmp     ($feff)

L10D8   adc     $feff
        ror     $feff
        .byte   $6f
        bvs     L10E1
L10E1   adc     ($ff),y
        adc     ($ff)
        .byte   $73
        stz     $ff,x
        adc     $ff,x
        ror     $ff,x
        .byte   $77
        sei
        adc     $feff,y
        ply
        .byte   $7b
        jmp     ($feff,x)

L10F6   adc     $feff,x
        ror     $feff,x
        .byte   $7f
        bra     L10FF

L10FF   sta     ($ff,x)
        .byte   $82,$ff
        .byte   $83
        sty     $ff
        sta     $ff
        stx     $ff
        .byte   $87
        dey
        bit     #$ff
        txa
        .byte   $8b
        sty     $feff
        sta     $feff
        stx     $feff
        .byte   $8f
        bcc     L111C
L111C   sta     ($ff),y
        sta     ($ff)
        .byte   $93
        sty     $ff,x
        sta     $ff,x
        stx     $ff,y
        .byte   $97
        tya
        sta     $feff,y
        txs
        .byte   $9b
        stz     $feff
        sta     $feff,x
        stz     $feff,x
        .byte   $9f
        ldy     #$ff
        lda     ($ff,x)
        ldx     #$ff
        .byte   $a3
        ldy     $ff
        lda     $ff
        ldx     $ff
        .byte   $a7
        tay
        lda     #$ff
        tax
        .byte   $ab
        ldy     $feff
        lda     $feff
        ldx     $feff
        .byte   $af
        bcs     L1157
L1157   lda     ($ff),y
        lda     ($ff)
        .byte   $b3
        ldy     $ff,x
        lda     $ff,x
        ldx     $ff,y
        .byte   $b7
        clv
        lda     $feff,y
        tsx
        .byte   $bb
        ldy     $feff,x
        lda     $feff,x
        ldx     $feff,y
        .byte   $bf
        cpy     #$ff
        cmp     ($ff,x)
        .byte   $c2,$ff
        .byte   $c3
        cpy     $ff
        cmp     $ff
        dec     $ff
        .byte   $c7
        iny
        cmp     #$ff
        dex
        .byte   $cb
        cpy     $feff
        cmp     $feff
        dec     $feff
        .byte   $cf
        bne     L1192
L1192   cmp     ($ff),y
        cmp     ($ff)
        .byte   $d3
        .byte   $d4,$ff
        cmp     $ff,x
        dec     $ff,x
        .byte   $d7
        cld
        cmp     $feff,y
        phx
        .byte   $db
        .byte   $dc,$ff,$fe
        cmp     $feff,x
        dec     $feff,x
        .byte   $df
        cpx     #$ff
        sbc     ($ff,x)
        .byte   $e2,$ff
        .byte   $e3
        cpx     $ff
        sbc     $ff
        inc     $ff
        .byte   $e7
        inx
        sbc     #$ff
        nop
        .byte   $eb
        cpx     $feff
        sbc     $feff
        inc     $feff
        .byte   $ef
        beq     L11CD
L11CD   sbc     ($ff),y
        sbc     ($ff)
        .byte   $f3
        .byte   $f4,$ff
        sbc     $ff,x
        inc     $ff,x
        .byte   $f7
        sed
        sbc     $feff,y
        plx
        .byte   $fb
        .byte   $fc,$ff,$fe
        sbc     $feff,x
        inc     $feff,x
        .byte   $ff
