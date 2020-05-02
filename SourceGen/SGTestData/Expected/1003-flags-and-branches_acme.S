        !cpu    65816
*       =       $1000
        !as
        !rs
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
        !al
        rep     #$10
        !rl
        rep     #$08
        rep     #$04
        rep     #$02
        rep     #$01
        sep     #$00
        sep     #$ff
        !as
        !rs
        rep     #$00
        rep     #$ff
        !al
        !rl
        lda     #$feed
        sec
        xce
        !as
        !rs
        lda     #$ff
        rep     #$30
        lda     #$ff
        clc
        xce
        lda     #$ff
        rep     #$20
        !al
        sep     #$10
        lda     #$0000
        ldx     #$01
        ldy     #$02
        sep     #$20
        !as
        rep     #$10
        !rl
        lda     #$01
        ldx     #$0000
        ldy     #$0000
        sep     #$30
        !rs
        lda     #$00
        pha
        plp
        rep     #$80
        bpl     L105F

        !byte   $00
        !byte   $00

L105F   sep     #$80
        bpl     @L1065
        bmi     @L1067

@L1065  !byte   $00
        !byte   $00

@L1067  rep     #$40
        bvc     @L106D

        !byte   $00
        !byte   $00

@L106D  sep     #$40
        bvs     @L1073

        !byte   $00
        !byte   $00

@L1073  rep     #$01
        bcc     @L1079

        !byte   $00
        !byte   $00

@L1079  sep     #$01
        bcs     @L107F

        !byte   $00
        !byte   $00

@L107F  rep     #$02
        bne     @L1085

        !byte   $00
        !byte   $00

@L1085  sep     #$02
        beq     @L108B

        !byte   $00
        !byte   $00

@L108B  sep     #$ff
        lda     #$01
        bne     @L1093

        !byte   $00
        !byte   $db

@L1093  lda     #$00
        beq     @L1099

        !byte   $00
        !byte   $db

@L1099  bpl     @L109D

        !byte   $00
        !byte   $db

@L109D  lda     #$80
        bmi     @L10A3

        !byte   $00
        !byte   $db

@L10A3  lda     #$ff
        and     #$00
        beq     @L10AB

        !byte   $00
        !byte   $db

@L10AB  lda     #$00
        ldx     #$80
        and     #$ff
        beq     @L10B5
        bne     @L10B5

@L10B5  lda     #$ff
        ldx     #$00
        and     #$7f
        beq     @L10BF
        bne     @L10BF

@L10BF  bpl     @L10C3

        !byte   $00
        !byte   $db

@L10C3  lda     #$ff
        and     #$80
        bmi     @L10CB
        brk

        !byte   $db

@L10CB  lda     #$00
        ldx     #$80
        bne     @L10D3

        !byte   $00
        !byte   $db

@L10D3  ora     #$00
        beq     @L10D9
        bne     @L10D9

@L10D9  ora     #$01
        bne     @L10DF

        !byte   $00
        !byte   $db

@L10DF  lda     #$00
        ldx     #$80
        bmi     @L10E7

        !byte   $00
        !byte   $db

@L10E7  ora     #$7f
        bpl     @L10EF
        bmi     @L10EF

        !byte   $00
        !byte   $db

@L10EF  ora     #$80
        bmi     @L10F5

        !byte   $00
        !byte   $db

@L10F5  lda     @L10F5
        sec
        ror
        bmi     @L10FE

        !byte   $00
        !byte   $dc

@L10FE  clc
        ror
        bpl     @L1104

        !byte   $00
        !byte   $dc

@L1104  lda     #$00
        sec
        rol
        bne     @L110C

        !byte   $00
        !byte   $dc

@L110C  lda     #$ff
        lsr
        bpl     @L1113

        !byte   $00
        !byte   $dd

@L1113  clc
        php
        sec
        plp
        bcc     @L111B

        !byte   $00
        !byte   $00

@L111B  sec
        bcs     @L111F

@L111E  clc
@L111F  lda     $33
        beq     @L111E
        bcs     @L1127
        lda     $44
@L1127  nop
        rep     #$20
        !al
        sep     #$10
        jsr     @L1143
        rep     #$30
        !rl
        jsr     @L1149
        sep     #$30
        !as
        !rs
        jsr     @L1149
        rep     #$20
        !al
        sep     #$10
        jsr     @L1143
        sep     #$30
        !as
        rts

        !al
@L1143  lda     #$1234
        ldx     #$ff
        rts

        !as
@L1149  lda     #$ff
        ldx     #$ee
        ldy     #$dd
        rts

