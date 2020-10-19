        .cpu    "65816"
*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$ff
        sep     #$ff
        rep     #$80
        rep     #$40
        rep     #$20
        .al
        rep     #$10
        .xl
        rep     #$08
        rep     #$04
        rep     #$02
        rep     #$01
        sep     #$00
        sep     #$ff
        .as
        .xs
        rep     #$00
        rep     #$ff
        .al
        .xl
        lda     #$feed
        sec
        xce
        .as
        .xs
        lda     #$ff
        rep     #$30
        lda     #$ff
        clc
        xce
        lda     #$ff
        rep     #$20
        .al
        sep     #$10
        lda     #$0000
        ldx     #$01
        ldy     #$02
        sep     #$20
        .as
        rep     #$10
        .xl
        lda     #$01
        ldx     #$0000
        ldy     #$0000
        sep     #$30
        .xs
        lda     #$00
        pha
        plp
        rep     #$80
        bpl     L1050

        .byte   $00
        .byte   $00

L1050   sep     #$80
        bpl     _L1056
        bmi     _L1058

_L1056  .byte   $00
        .byte   $00

_L1058  rep     #$40
        bvc     _L105E

        .byte   $00
        .byte   $00

_L105E  sep     #$40
        bvs     _L1064

        .byte   $00
        .byte   $00

_L1064  rep     #$01
        bcc     _L106A

        .byte   $00
        .byte   $00

_L106A  sep     #$01
        bcs     _L1070

        .byte   $00
        .byte   $00

_L1070  rep     #$02
        bne     _L1076

        .byte   $00
        .byte   $00

_L1076  sep     #$02
        beq     _L107C

        .byte   $00
        .byte   $00

_L107C  rep     #$20
        .al
        php
        plp
        lda     #$1234
        sep     #$20
        .as
        php
        plp
        lda     #$12
        rep     #$20
        .al
        sep     #$10
        jsr     _L10A6
        rep     #$30
        .xl
        jsr     _L10AC
        sep     #$30
        .as
        .xs
        jsr     _L10AC
        rep     #$20
        .al
        sep     #$10
        jsr     _L10A6
        sep     #$30
        .as
        jmp     _L10B3

        .al
_L10A6  lda     #$1234
        ldx     #$ff
        rts

        .as
_L10AC  lda     #$ff
        ldx     #$ee
        ldy     #$dd
        rts

_L10B3  rts

