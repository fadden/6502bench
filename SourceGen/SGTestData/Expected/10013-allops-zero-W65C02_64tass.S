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

        .byte   $00

L1017   ora     ($00,x)
        .byte   $02,$00
        .byte   $03
        tsb     $00
        ora     $00
        asl     $00
        rmb     0,$00
        php
        ora     #$00
        asl     a
        .byte   $0b
        tsb     @w$0000
        ora     @w$0000
        asl     @w$0000
        bbr     0,$00,_L1035
_L1035  bpl     _L1037
_L1037  ora     ($00),y
        ora     ($00)
        .byte   $13
        trb     $00
        ora     $00,x
        asl     $00,x
        rmb     1,$00
        clc
        ora     $0000,y
        inc     a
        .byte   $1b
        trb     @w$0000
        ora     @w$0000,x
        asl     @w$0000,x
        bbr     1,$00,_L1056
_L1056  jsr     $0000
        and     ($00,x)
        .byte   $22,$00
        .byte   $23
        bit     $00
        and     $00
        rol     $00
        rmb     2,$00
        plp
        and     #$00
        rol     a
        .byte   $2b
        bit     @w$0000
        and     @w$0000
        rol     @w$0000
        bbr     2,$00,_L1077
_L1077  bmi     _L1079
_L1079  and     ($00),y
        and     ($00)
        .byte   $33
        bit     $00,x
        and     $00,x
        rol     $00,x
        rmb     3,$00
        sec
        and     $0000,y
        dec     a
        .byte   $3b
        bit     @w$0000,x
        and     @w$0000,x
        rol     @w$0000,x
        bbr     3,$00,_L1098
_L1098  rti

L1099   eor     ($00,x)
        .byte   $42,$00
        .byte   $43
        .byte   $44,$00
        eor     $00
        lsr     $00
        rmb     4,$00
        pha
        eor     #$00
        lsr     a
        .byte   $4b
        jmp     _L10AE

_L10AE  eor     @w$0000
        lsr     @w$0000
        bbr     4,$00,_L10B7
_L10B7  bvc     _L10B9
_L10B9  eor     ($00),y
        eor     ($00)
        .byte   $53
        .byte   $54,$00
        eor     $00,x
        lsr     $00,x
        rmb     5,$00
        cli
        eor     $0000,y
        phy
        .byte   $5b
        .byte   $5c,$00,$00
        eor     @w$0000,x
        lsr     @w$0000,x
        bbr     5,$00,_L10D8
_L10D8  rts

L10D9   adc     ($00,x)
        .byte   $62,$00
        .byte   $63
        stz     $00
        adc     $00
        ror     $00
        rmb     6,$00
        pla
        adc     #$00
        ror     a
        .byte   $6b
        jmp     ($0000)

L10EE   adc     @w$0000
        ror     @w$0000
        bbr     6,$00,_L10F7
_L10F7  bvs     _L10F9
_L10F9  adc     ($00),y
        adc     ($00)
        .byte   $73
        stz     $00,x
        adc     $00,x
        ror     $00,x
        rmb     7,$00
        sei
        adc     $0000,y
        ply
        .byte   $7b
        jmp     ($0000,x)

L110F   adc     @w$0000,x
        ror     @w$0000,x
        bbr     7,$00,_L1118
_L1118  bra     _L111A

_L111A  sta     ($00,x)
        .byte   $82,$00
        .byte   $83
        sty     $00
        sta     $00
        stx     $00
        smb     0,$00
        dey
        bit     #$00
        txa
        .byte   $8b
        sty     @w$0000
        sta     @w$0000
        stx     @w$0000
        bbs     0,$00,_L1138
_L1138  bcc     _L113A
_L113A  sta     ($00),y
        sta     ($00)
        .byte   $93
        sty     $00,x
        sta     $00,x
        stx     $00,y
        smb     1,$00
        tya
        sta     $0000,y
        txs
        .byte   $9b
        stz     @w$0000
        sta     @w$0000,x
        stz     @w$0000,x
        bbs     1,$00,_L1159
_L1159  ldy     #$00
        lda     ($00,x)
        ldx     #$00
        .byte   $a3
        ldy     $00
        lda     $00
        ldx     $00
        smb     2,$00
        tay
        lda     #$00
        tax
        .byte   $ab
        ldy     @w$0000
        lda     @w$0000
        ldx     @w$0000
        bbs     2,$00,_L1179
_L1179  bcs     _L117B
_L117B  lda     ($00),y
        lda     ($00)
        .byte   $b3
        ldy     $00,x
        lda     $00,x
        ldx     $00,y
        smb     3,$00
        clv
        lda     $0000,y
        tsx
        .byte   $bb
        ldy     @w$0000,x
        lda     @w$0000,x
        ldx     @w$0000,y
        bbs     3,$00,_L119A
_L119A  cpy     #$00
        cmp     ($00,x)
        .byte   $c2,$00
        .byte   $c3
        cpy     $00
        cmp     $00
        dec     $00
        smb     4,$00
        iny
        cmp     #$00
        dex
        wai
        cpy     @w$0000
        cmp     @w$0000
        dec     @w$0000
        bbs     4,$00,_L11BA
_L11BA  bne     _L11BC
_L11BC  cmp     ($00),y
        cmp     ($00)
        .byte   $d3
        .byte   $d4,$00
        cmp     $00,x
        dec     $00,x
        smb     5,$00
        cld
        cmp     $0000,y
        phx
        stp

L11CF   .byte   $dc,$00,$00
        cmp     @w$0000,x
        dec     @w$0000,x
        bbs     5,$00,_L11DB
_L11DB  cpx     #$00
        sbc     ($00,x)
        .byte   $e2,$00
        .byte   $e3
        cpx     $00
        sbc     $00
        inc     $00
        smb     6,$00
        inx
        sbc     #$00
        nop
        .byte   $eb
        cpx     @w$0000
        sbc     @w$0000
        inc     @w$0000
        bbs     6,$00,_L11FB
_L11FB  beq     _L11FD
_L11FD  sbc     ($00),y
        sbc     ($00)
        .byte   $f3
        .byte   $f4,$00
        sbc     $00,x
        inc     $00,x
        smb     7,$00
        sed
        sbc     $0000,y
        plx
        .byte   $fb
        .byte   $fc,$00,$00
        sbc     @w$0000,x
        inc     @w$0000,x
        bbs     7,$00,_L121C
_L121C  rts

