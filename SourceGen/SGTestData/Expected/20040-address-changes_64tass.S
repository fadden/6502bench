        .cpu    "6502"

*       =       $1000
        jsr     L1100
        jsr     L1107
        jmp     L2000

        .logical $1100
L1100   bit     L1100
L1103   lda     #$11
        ldx     #$11
L1107   ldy     #$11
        clv
        bvc     L1103

        .here
        .logical $1100
_L1100_0 bit    _L1100_0
        lda     #$22
_L1105  ldx     #$22
        ldy     #$22
        jmp     _L1105

        .here
        .logical $1100
_L1100_1 bit    _L1100_1
        lda     #$33
        ldx     #$33
_L1107_0 ldy    #$33
        sec
        bcs     _L1107_0

        .here
        .logical $2000
L2000   bit     L2000
        beq     $2018
        bne     _L2020

        .here
        .logical $2020
_L2020  bit     _L2020
        beq     $2028
        bne     L2080

offend  nop
        .here
        .logical $2080
L2080   bit     L2080
        lda     offend
        jsr     offend
        lda     $2028
        jsr     $2028
        lda     L2080-1
        jsr     L2080-1
        lda     L2080
        jsr     L2080
        lda     $00
        beq     _L2100
        .byte   $ad
        .here

        .logical $2100
_L2100  nop
        nop
        jmp     _L3000

        .here
        .logical $2800
        .byte   $00
        .byte   $28
        .fill   14,$00
        .here
        .logical $2820
        .fill   18,$00
        .here

        .logical $3000
_L3000  bit     _L3000
        lda     #$44
        ldx     #$44
        ldy     #$44
        jmp     fwd

ulabel  .byte   $00
        .byte   $01
        .here
        .logical $3100
        .byte   $02

fwd     bit     fwd
        lda     ulabel
        lda     ulabel+1
        lda     $300e
        lda     $300f
        lda     fwd-1
        beq     _L3182
        .byte   $ea
        .byte   $ea
        .here
        .logical $3180
        .byte   $00
        .byte   $01

_L3182  bit     _L3182
        lda     label1
        lda     label1+1
        lda     L3200
        clv
        bvc     L3200

label1  .byte   $ea
        .byte   $ea
        .here

        .logical $3200
L3200   bit     L3200
        .byte   $00
        .byte   $01        ;execution continues off end of file
        .here
