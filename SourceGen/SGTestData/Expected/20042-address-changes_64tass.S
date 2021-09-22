        .cpu    "65816"
        .logical $021000
        .as
        .xs
        clc
        xce
        sep     #$ff
        jsr     L21100
        jsr     L21107
        jmp     L22000

        .here
        .logical $021100
L21100  bit     L21100 & $ffff
L21103  lda     #$11
        ldx     #$11
L21107  ldy     #$11
        per     L21103
        bra     L21103

        .here
        .logical $021100
_L21100_0 bit   _L21100_0 & $ffff
        lda     #$22
_L21105 ldx     #$22
        ldy     #$22
        per     _L21105
        jmp     _L21105

        .here
        .logical $021100
_L21100_1 bit   _L21100_1 & $ffff
        lda     #$33
        ldx     #$33
_L21107_0 ldy   #$33
        per     _L21107_0
        bra     _L21107_0

        .here
        .logical $022000
L22000  bit     L22000 & $ffff
        beq     $022018
        bra     _L22020

        .here
        .logical $022020
_L22020 bit     _L22020 & $ffff
        beq     $022029
        brl     _L22080

_offend nop
        .here
        .logical $022080
_L22080 bit     _L22080 & $ffff
        lda     _offend & $ffff
        jsr     _offend
        lda     $2029
        jsr     $022029
        lda     0+(_L22080 & $ffff)-1
        jsr     _L22080-1
        lda     _L22080 & $ffff
        jsr     _L22080
        lda     $00
        beq     _L22100
        .byte   $ad
        .here

        .logical $022100
_L22100 nop
        nop
        jmp     _L23000

        .here
        .logical $022800
        .byte   $00
        .byte   $28
        .fill   14,$00
        .here
        .logical $022820
        .fill   18,$00
        .here

        .logical $023000
_L23000 bit     _L23000 & $ffff
        lda     #$44
        ldx     #$44
        ldy     #$44
        brl     _fwd

_ulabel .byte   $00
        .byte   $01
        .here
        .logical $023100
        .byte   $02

_fwd    bit     _fwd & $ffff
        lda     _ulabel & $ffff
        lda     0+(_ulabel & $ffff)+1
        lda     $300e
        lda     $300f
        lda     0+(_fwd & $ffff)-1
        beq     _L23182
        .byte   $ea
        .byte   $ea
        .here
        .logical $023180
        .byte   $00
        .byte   $01

_L23182 bit     _L23182 & $ffff
        lda     _label1 & $ffff
        lda     0+(_label1 & $ffff)+1
        lda     0+(_label1 & $ffff)+112
        bra     _L23200

_label1 .byte   $ea
        .byte   $ea
        .here

        .logical $023200
_L23200 bit     _L23200 & $ffff
        .byte   $00
        .byte   $01
        .here
