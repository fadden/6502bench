        .cpu    "6502i"
*       =       $1000
        jsr     L100F
        jsr     L1017
        jsr     L101C
        jsr     L1046
        jmp     L1051

L100F   lda     #$00
        .byte   $2c
_L1012  lda     #$01
        beq     _L1012
        rts

L1017   .byte   $20
_L1018  rts

        .byte   $ea
        bvs     _L1018
L101C   .byte   $2c
_L101D  .byte   $2c
_L101E  .byte   $2c
_L101F  .byte   $2c
_L1020  .byte   $2c
_L1021  .byte   $2c
_L1022  .byte   $2c
_L1023  .byte   $2c
_L1024  .byte   $2c
_L1025  nop
        nop
        asl     a
        bcc     L101C
        asl     a
        bcc     _L101D
        asl     a
        bcc     _L101E
        asl     a
        bcc     _L101F
        asl     a
        bcc     _L1020
        asl     a
        bcc     _L1021
        asl     a
        bcc     _L1022
        asl     a
        bcc     _L1023
        asl     a
        bcc     _L1024
        asl     a
        bcc     _L1025
        rts

L1046   .byte   $2c
_L1047  nop
        .byte   $ad
_L1049  lda     $00
        asl     a
        bcc     _L1047
        asl     a
        bcc     _L1049
L1051   .byte   $ad
