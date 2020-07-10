        .cpu    "65816"
*       =       $1000
        .as
        .xs
start   clc
        xce
        sep     #$30
        lda     start
        lda     $2000
        phk
        plb
        lda     start
        lda     $2000
        lda     #$02
        pha
        plb
        lda     $1000
        lda     bank2 & $ffff
        ldx     #$08
        tax
        pha
        plb
        jsl     bank2
        jsl     L24000
        lda     #$02
        pha
        plb
        jsl     bank3
        phk
        plb
        bit     start
        jml     L2400F

L103A   .word   _L103C

_L103C  rts

L103D   .long   L2202E

        .logical $022000
bank2   lda     bank2
        lda     bank2 & $ffff
        ldx     #$00
        jsr     _L2202B
        jsr     _L22025
        jsr     _L22028
        nop
        lda     #$03
        pha
        plb
        lda     L33028 & $ffff
        rep     #$30
        .al
        .xl
        lda     #$00a9
        pha
        plb
        plb
        sep     #$30
        .as
        .xs
        rtl

_L22025 jmp     (L103A)

_L22028 jml     [L103D]

_L2202B jmp     (bank2addr,x)

L2202E  nop
        rts

bank2addr .word L2202E & $ffff

        .here
        .logical $033000
bank3   lda     bank3
        lda     bank2 & $ffff
        jsr     _L33020
        beq     _L33024
        phk
        .here
        .logical $033020
_L33020 plb
        nop
        bra     _L33024

_L33024 lda     $2030
        rtl

L33028  .word   L33028 & $ffff

        .here
        .logical $024000
L24000  lda     L24000
        phb
        phk
        plb
        lda     bank2 & $ffff
        lda     $3000
        plb
        rtl

L2400F  rts

        .here
