        .cpu    "65816"
        .logical $011000
        .logical *+$010000
        .logical *+$010000
        .as
        .xs
L31000  lda     L31000
_L31004 lda     _L31004
        and     _L1101F
        jml     _L21010

        .here
_L21010 lda     _L21010
        jml     _L11018

        .here
_L11018 lda     _L11018
        jmp     _L11026

_L1101F .byte   $80
        .logical *-$011020
        .byte   $ea
        .byte   $60
        .dword  _L31004
        .here

_L11026 nop
        rts

        .here
