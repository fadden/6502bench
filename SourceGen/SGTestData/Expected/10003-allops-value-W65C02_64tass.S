        .cpu    "w65c02"
*       =       $1000
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

        .byte   $ff

L1017   ora     ($ff,x)
        .byte   $02,$ff
        .byte   $03
        tsb     $ff
        ora     $ff
        asl     $ff
        rmb     0,$ff
        php
        ora     #$ff
        asl     a
        .byte   $0b
        tsb     $feff
        ora     $feff
        asl     $feff
        bbr     0,$ff,_L1035
_L1035  bpl     _L1037
_L1037  ora     ($ff),y
        ora     ($ff)
        .byte   $13
        trb     $ff
        ora     $ff,x
        asl     $ff,x
        rmb     1,$ff
        clc
        ora     $feff,y
        inc     a
        .byte   $1b
        trb     $feff
        ora     $feff,x
        asl     $feff,x
        bbr     1,$ff,_L1056
_L1056  jsr     $feff
        and     ($ff,x)
        .byte   $22,$ff
        .byte   $23
        bit     $ff
        and     $ff
        rol     $ff
        rmb     2,$ff
        plp
        and     #$ff
        rol     a
        .byte   $2b
        bit     $feff
        and     $feff
        rol     $feff
        bbr     2,$ff,_L1077
_L1077  bmi     _L1079
_L1079  and     ($ff),y
        and     ($ff)
        .byte   $33
        bit     $ff,x
        and     $ff,x
        rol     $ff,x
        rmb     3,$ff
        sec
        and     $feff,y
        dec     a
        .byte   $3b
        bit     $feff,x
        and     $feff,x
        rol     $feff,x
        bbr     3,$ff,_L1098
_L1098  rti

L1099   eor     ($ff,x)
        .byte   $42,$ff
        .byte   $43
        .byte   $44,$ff
        eor     $ff
        lsr     $ff
        rmb     4,$ff
        pha
        eor     #$ff
        lsr     a
        .byte   $4b
        jmp     _L10AE

_L10AE  eor     $feff
        lsr     $feff
        bbr     4,$ff,_L10B7
_L10B7  bvc     _L10B9
_L10B9  eor     ($ff),y
        eor     ($ff)
        .byte   $53
        .byte   $54,$ff
        eor     $ff,x
        lsr     $ff,x
        rmb     5,$ff
        cli
        eor     $feff,y
        phy
        .byte   $5b
        .byte   $5c,$ff,$fe
        eor     $feff,x
        lsr     $feff,x
        bbr     5,$ff,_L10D8
_L10D8  rts

L10D9   adc     ($ff,x)
        .byte   $62,$ff
        .byte   $63
        stz     $ff
        adc     $ff
        ror     $ff
        rmb     6,$ff
        pla
        adc     #$ff
        ror     a
        .byte   $6b
        jmp     ($feff)

L10EE   adc     $feff
        ror     $feff
        bbr     6,$ff,_L10F7
_L10F7  bvs     _L10F9
_L10F9  adc     ($ff),y
        adc     ($ff)
        .byte   $73
        stz     $ff,x
        adc     $ff,x
        ror     $ff,x
        rmb     7,$ff
        sei
        adc     $feff,y
        ply
        .byte   $7b
        jmp     ($feff,x)

L110F   adc     $feff,x
        ror     $feff,x
        bbr     7,$ff,_L1118
_L1118  bra     _L111A

_L111A  sta     ($ff,x)
        .byte   $82,$ff
        .byte   $83
        sty     $ff
        sta     $ff
        stx     $ff
        smb     0,$ff
        dey
        bit     #$ff
        txa
        .byte   $8b
        sty     $feff
        sta     $feff
        stx     $feff
        bbs     0,$ff,_L1138
_L1138  bcc     _L113A
_L113A  sta     ($ff),y
        sta     ($ff)
        .byte   $93
        sty     $ff,x
        sta     $ff,x
        stx     $ff,y
        smb     1,$ff
        tya
        sta     $feff,y
        txs
        .byte   $9b
        stz     $feff
        sta     $feff,x
        stz     $feff,x
        bbs     1,$ff,_L1159
_L1159  ldy     #$ff
        lda     ($ff,x)
        ldx     #$ff
        .byte   $a3
        ldy     $ff
        lda     $ff
        ldx     $ff
        smb     2,$ff
        tay
        lda     #$ff
        tax
        .byte   $ab
        ldy     $feff
        lda     $feff
        ldx     $feff
        bbs     2,$ff,_L1179
_L1179  bcs     _L117B
_L117B  lda     ($ff),y
        lda     ($ff)
        .byte   $b3
        ldy     $ff,x
        lda     $ff,x
        ldx     $ff,y
        smb     3,$ff
        clv
        lda     $feff,y
        tsx
        .byte   $bb
        ldy     $feff,x
        lda     $feff,x
        ldx     $feff,y
        bbs     3,$ff,_L119A
_L119A  cpy     #$ff
        cmp     ($ff,x)
        .byte   $c2,$ff
        .byte   $c3
        cpy     $ff
        cmp     $ff
        dec     $ff
        smb     4,$ff
        iny
        cmp     #$ff
        dex
        wai
        cpy     $feff
        cmp     $feff
        dec     $feff
        bbs     4,$ff,_L11BA
_L11BA  bne     _L11BC
_L11BC  cmp     ($ff),y
        cmp     ($ff)
        .byte   $d3
        .byte   $d4,$ff
        cmp     $ff,x
        dec     $ff,x
        smb     5,$ff
        cld
        cmp     $feff,y
        phx
        stp

L11CF   .byte   $dc,$ff,$fe
        cmp     $feff,x
        dec     $feff,x
        bbs     5,$ff,_L11DB
_L11DB  cpx     #$ff
        sbc     ($ff,x)
        .byte   $e2,$ff
        .byte   $e3
        cpx     $ff
        sbc     $ff
        inc     $ff
        smb     6,$ff
        inx
        sbc     #$ff
        nop
        .byte   $eb
        cpx     $feff
        sbc     $feff
        inc     $feff
        bbs     6,$ff,_L11FB
_L11FB  beq     _L11FD
_L11FD  sbc     ($ff),y
        sbc     ($ff)
        .byte   $f3
        .byte   $f4,$ff
        sbc     $ff,x
        inc     $ff,x
        smb     7,$ff
        sed
        sbc     $feff,y
        plx
        .byte   $fb
        .byte   $fc,$ff,$fe
        sbc     $feff,x
        inc     $feff,x
        bbs     7,$ff,_L121C
_L121C  rts

