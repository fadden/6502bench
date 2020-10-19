        !cpu    65816
*       =       $1000
        !as
        !rs
        clc
        xce
        sep     #$ff
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
        bpl     L1050

        !byte   $00
        !byte   $00

L1050   sep     #$80
        bpl     @L1056
        bmi     @L1058

@L1056  !byte   $00
        !byte   $00

@L1058  rep     #$40
        bvc     @L105E

        !byte   $00
        !byte   $00

@L105E  sep     #$40
        bvs     @L1064

        !byte   $00
        !byte   $00

@L1064  rep     #$01
        bcc     @L106A

        !byte   $00
        !byte   $00

@L106A  sep     #$01
        bcs     @L1070

        !byte   $00
        !byte   $00

@L1070  rep     #$02
        bne     @L1076

        !byte   $00
        !byte   $00

@L1076  sep     #$02
        beq     @L107C

        !byte   $00
        !byte   $00

@L107C  rep     #$20
        !al
        php
        plp
        lda     #$1234
        sep     #$20
        !as
        php
        plp
        lda     #$12
        rep     #$20
        !al
        sep     #$10
        jsr     @L10A6
        rep     #$30
        !rl
        jsr     @L10AC
        sep     #$30
        !as
        !rs
        jsr     @L10AC
        rep     #$20
        !al
        sep     #$10
        jsr     @L10A6
        sep     #$30
        !as
        jmp     @L10B3

        !al
@L10A6  lda     #$1234
        ldx     #$ff
        rts

        !as
@L10AC  lda     #$ff
        ldx     #$ee
        ldy     #$dd
        rts

@L10B3  rts

