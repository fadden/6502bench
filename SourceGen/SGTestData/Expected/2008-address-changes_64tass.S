        .cpu    "65816"
*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$ff
        jsr     L1100
        jsr     L1107
        jmp     L2000

        .logical $1100
L1100   bit     L1100
L1103   lda     #$11
        ldx     #$11
L1107   ldy     #$11
        per     L1103
        bra     L1103

        .here
        .logical $1100
L1100_0 bit     L1100_0
        lda     #$22
L1105   ldx     #$22
        ldy     #$22
        per     L1105
        jmp     L1105

        .here
        .logical $1100
L1100_1 bit     L1100_1
        lda     #$33
        ldx     #$33
L1107_0 ldy     #$33
        per     L1107_0
        bra     L1107_0

        .here
        .logical $2000
L2000   bit     L2000
        beq     $2018
        bra     L2020

        .here
        .logical $2020
L2020   bit     L2020
        beq     offend+1
        brl     L2080

offend  nop
        .here
        .logical $2080
L2080   bit     L2080
        lda     offend
        jsr     offend
        lda     offend+1
        jsr     offend+1
        lda     L2080-1
        jsr     L2080-1
        lda     L2080
        jsr     L2080
        lda     $00
        beq     L2100
        .byte   $ad

        .here
        .logical $2100
L2100   nop
        nop
        jmp     L3000

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
L3000   bit     L3000
        lda     #$44
        ldx     #$44
        ldy     #$44
        brl     fwd

ulabel  .byte   $00
        .byte   $01
        .here
        .logical $3100
L3100   .byte   $02

fwd     bit     fwd
        lda     ulabel
        lda     ulabel+1
        lda     ulabel+2
        lda     $300f
        lda     L3100
        beq     L3182
        .byte   $ea
        .byte   $ea
        .here
        .logical $3180
        .byte   $00
        .byte   $01

L3182   bit     L3182
        lda     label1
        lda     label1+1
        lda     label1+112
        bra     L3200

label1  .byte   $ea
        .byte   $ea

        .here
        .logical $3200
L3200   bit     L3200
        .byte   $00
        .byte   $01
        .here
