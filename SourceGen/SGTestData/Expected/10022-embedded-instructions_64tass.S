        .cpu    "65816"
*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$30
        jsr     L1014
        jsr     L101C
        jsr     L102A
        jsr     L102F
        jsr     L1059
        rts

L1014   lda     #$00
        .byte   $2c
_L1017  lda     #$01
        beq     _L1017
        rts

L101C   sep     #$30
        lda     $00
        beq     _L1025
        lda     #$00
        brk

_L1025  sta     $012345
        rts

L102A   .byte   $20
_L102B  rts

        .byte   $ea
        bra     _L102B

L102F   .byte   $2c
_L1030  .byte   $2c
_L1031  .byte   $2c
_L1032  .byte   $2c
_L1033  .byte   $2c
_L1034  .byte   $2c
_L1035  .byte   $2c
_L1036  .byte   $2c
_L1037  .byte   $2c
_L1038  nop
        nop
        asl     a
        bcc     L102F
        asl     a
        bcc     _L1030
        asl     a
        bcc     _L1031
        asl     a
        bcc     _L1032
        asl     a
        bcc     _L1033
        asl     a
        bcc     _L1034
        asl     a
        bcc     _L1035
        asl     a
        bcc     _L1036
        asl     a
        bcc     _L1037
        asl     a
        bcc     _L1038
        rts

L1059   .byte   $2c
_L105A  nop
        .byte   $ad
_L105C  lda     $00
        asl     a
        bcc     _L105A
        asl     a
        bcc     _L105C
        .byte   $af
