        .cpu    "65816"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
*       =       $1000
        .as
        .xs
load11  lda     #$11
_L1002  ldx     #$22
_load33 ldy     #$33
_L1006  lda     #$44
_predat bra     _L1042

        .word   $0123
_dat1   .word   $4567
        .word   $89ab
_L1010  .word   $cdef
_L1012  .word   $0011
_L1014  .word   $2233
        .byte   $80
        .text   "The quick brown fox"
        .byte   $80
        .word   _L1042
        .word   _L1041
        .word   _L1042+1
_fill0  .fill   16,$00
_L1041  .byte   $80

_L1042  lda     _predat+2
        lda     _L1041
        asl     _dat1
        rol     _dat1+2
        ror     _L1010
        and     _L1012
        ora     _L1014
        lda     _fill0
        sta     _fill0+4
        lda     _fill0+8
        sta     _fill0+12
        jsr     _L1002
        lda     _L1006
_L1069  pea     _L1069-1
        per     _L1069-1
        lda     _L1069+1
        lda     _L1069+2
        lda     #$ea
_L1077  sta     _L1077
_L107A  sta     _L107A+1
        sta     $107f      ;self-ref; operand format refs nonexistent symbol
        brl     _L2002

        .byte   $80
_dat81  .byte   $81
        .logical $2000
_L2000  .byte   $82
        .byte   $83

_L2002  bit     _L2002
        lda     _dat81
        lda     _L2000
        bra     _L2018

_L200D  .byte   $7c
_L200E  .byte   $7d
_L200F  .byte   $7e
        .byte   $7f
_nearby .byte   $80
        .byte   $81
        .byte   $82
        .byte   $83
_L2015  .byte   $84
_L2016  .byte   $85
_L2017  .byte   $86

_L2018  lda     _L200D
        lda     _L200E
        lda     _L200F
        lda     _nearby-1
        lda     _nearby
        lda     _nearby+1
        lda     _nearby+2
        lda     _nearby+3
        lda     _L2015
        lda     _L2016
        lda     _L2017
        rts

        .here
