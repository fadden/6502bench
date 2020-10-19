        .cpu    "65816"
*       =       $1000
        .as
        .xs
        jsr     L1006
        jmp     L1020

L1006   sep     #$30
        lda     $00
        beq     _L100F
        lda     #$00
        brk

_L100F  sta     $012345
        lda     $00
        beq     _L101A
        lda     #$00
        .byte   $85
_L101A  .byte   $8f
        eor     $23
        .byte   $01
        nop
        rts

L1020   .byte   $af
