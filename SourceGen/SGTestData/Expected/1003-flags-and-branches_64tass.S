        .cpu    "65816"
*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$ff
        clv
        cld
        cli
        clc
        lda     #$80
        lda     #$01
        sed
        sei
        sec
        lda     #$ff
        adc     #$00
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
        bpl     L105F

        .byte   $00
        .byte   $00

L105F   sep     #$80
        bpl     _L1065
        bmi     _L1067

_L1065  .byte   $00
        .byte   $00

_L1067  rep     #$40
        bvc     _L106D

        .byte   $00
        .byte   $00

_L106D  sep     #$40
        bvs     _L1073

        .byte   $00
        .byte   $00

_L1073  rep     #$01
        bcc     _L1079

        .byte   $00
        .byte   $00

_L1079  sep     #$01
        bcs     _L107F

        .byte   $00
        .byte   $00

_L107F  rep     #$02
        bne     _L1085

        .byte   $00
        .byte   $00

_L1085  sep     #$02
        beq     _L108B

        .byte   $00
        .byte   $00

_L108B  sep     #$ff
        lda     #$01
        bne     _L1093

        .byte   $00
        .byte   $db

_L1093  lda     #$00
        beq     _L1099

        .byte   $00
        .byte   $db

_L1099  bpl     _L109D

        .byte   $00
        .byte   $db

_L109D  lda     #$80
        bmi     _L10A3

        .byte   $00
        .byte   $db

_L10A3  lda     #$ff
        and     #$00
        beq     _L10AB

        .byte   $00
        .byte   $db

_L10AB  lda     #$00
        and     #$ff
        beq     _L10B3

        .byte   $00
        .byte   $db

_L10B3  lda     #$ff
        and     #$7f
        bne     _L10BB
        brk

        .byte   $db

_L10BB  bpl     _L10BF

        .byte   $00
        .byte   $db

_L10BF  lda     #$ff
        and     #$80
        bmi     _L10C7

        .byte   $00
        .byte   $db

_L10C7  lda     #$00
        ora     #$00
        beq     _L10CF

        .byte   $00
        .byte   $db

_L10CF  ora     #$01
        bne     _L10D5

        .byte   $00
        .byte   $db

_L10D5  lda     #$00
        ora     #$7f
        bpl     _L10DD

        .byte   $00
        .byte   $db

_L10DD  ora     #$80
        bmi     _L10E3

        .byte   $00
        .byte   $db

_L10E3  lda     _L10E3
        sec
        ror     a
        bmi     _L10EC

        .byte   $00
        .byte   $dc

_L10EC  clc
        ror     a
        bpl     _L10F2

        .byte   $00
        .byte   $dc

_L10F2  lda     #$00
        sec
        rol     a
        bne     _L10FA

        .byte   $00
        .byte   $dc

_L10FA  lda     #$ff
        lsr     a
        bpl     _L1101

        .byte   $00
        .byte   $dd

_L1101  clc
        php
        sec
        plp
        bcc     _L1109

        .byte   $00
        .byte   $00

_L1109  sec
        bcs     _L110D

_L110C  clc
_L110D  lda     $33
        beq     _L110C
        bcs     _L1115
        lda     $44
_L1115  nop
        rep     #$20
        .al
        sep     #$10
        jsr     _L1131
        rep     #$30
        .xl
        jsr     _L1137
        sep     #$30
        .as
        .xs
        jsr     _L1137
        rep     #$20
        .al
        sep     #$10
        jsr     _L1131
        sep     #$30
        .as
        rts

        .al
_L1131  lda     #$1234
        ldx     #$ff
        rts

        .as
_L1137  lda     #$ff
        ldx     #$ee
        ldy     #$dd
        rts

