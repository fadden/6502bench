        .cpu    "6502i"
*       =       $1000
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
        lda     #$00
        pha
        plp
        clv
        bvc     L101A

        .byte   $70
        .byte   $02
        .byte   $00
        .byte   $00

L101A   clc
        bcc     _L101F

        .byte   $00
        .byte   $00

_L101F  sec
        bcs     _L1024

        .byte   $00
        .byte   $00

_L1024  lda     #$01
        bne     _L102A

        .byte   $00
        .byte   $db

_L102A  lda     #$00
        beq     _L1030

        .byte   $00
        .byte   $db

_L1030  bpl     _L1034

        .byte   $00
        .byte   $db

_L1034  lda     #$80
        bmi     _L103A

        .byte   $00
        .byte   $db

_L103A  lda     #$ff
        and     #$00
        beq     _L1042

        .byte   $00
        .byte   $db

_L1042  lda     #$00
        ldx     #$80
        and     #$ff
        beq     _L104C
        bne     _L104C

_L104C  lda     #$ff
        ldx     #$00
        and     #$7f
        beq     _L1056
        bne     _L1056

_L1056  bpl     _L105A

        .byte   $00
        .byte   $db

_L105A  lda     #$ff
        and     #$80
        bpl     _L1064
        bmi     _L1064

        .byte   $00
        .byte   $db

_L1064  lda     #$00
        ldx     #$80
        bne     _L106C

        .byte   $00
        .byte   $db

_L106C  ora     #$00
        beq     _L1072
        bne     _L1072

_L1072  ora     #$01
        bne     _L1078

        .byte   $00
        .byte   $db

_L1078  lda     #$00
        ldx     #$80
        bmi     _L1080

        .byte   $00
        .byte   $db

_L1080  ora     #$7f
        bpl     _L1088
        bmi     _L1088

        .byte   $00
        .byte   $db

_L1088  ora     #$80
        bmi     _L108E

        .byte   $00
        .byte   $db

_L108E  lda     _L108E
        sec
        ror     a
        bmi     _L1097

        .byte   $00
        .byte   $dc

_L1097  clc
        ror     a
        bpl     _L109D

        .byte   $00
        .byte   $dc

_L109D  lda     #$00
        sec
        rol     a
        bne     _L10A5

        .byte   $00
        .byte   $dc

_L10A5  lda     #$ff
        lsr     a
        bpl     _L10AC

        .byte   $00
        .byte   $dd

_L10AC  clc
        php
        sec
        plp
        bcc     _L10B3
        nop
_L10B3  sec
        bcs     _L10B7

_L10B6  clc
_L10B7  lda     $33
        beq     _L10B6
        bcs     _L10BF
        lda     $44
_L10BF  nop
        rts

