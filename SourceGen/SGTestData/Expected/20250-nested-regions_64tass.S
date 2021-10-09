        .cpu    "6502"
*       =       $0000
        .word   $3000      ;load address

        .logical $1000
        .logical *+$1000
        .logical *+$1000
L3000   bit     L3000
_L3003  lda     _L3003
        and     _LE003
        jmp     _L200C

        .here
_L200C  bit     _L200C
        jmp     _L1012

        .here
_L1012  bit     _L1012
        jsr     _L4000
        .logical $0000
        .byte   $00
        .null   "Null-term PETSCII string"
        .byte   $80
        .word   _L3003
        .word   _LE003
        .byte   $80
        .here

        .logical $4000
_L4000  bit     _L4000
        bit     _L5000
        bit     _L500F
        bit     _L500F
        nop
        jmp     _L4020

        .logical $5000
_L5000  bit     _L5000
        bit     _L4000
        nop
        nop
_L5008  bit     _L5008
        bit     _L5017
        nop
_L500F  rts

        .here
_L4020  bit     _L4020
        bit     _L500F
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        jmp     _L4040

        .logical $5008
_L5008_0 bit    _L5008_0
        bit     _L5000
        nop
_L500F_0 bit    _L500F_0
        nop
        nop
        nop
        nop
        nop
_L5017  rts

        .here
_L4040  bit     _L4040
        bit     _L5017
        nop
        jmp     _LE000

        .here
        .logical *+$cf7e
_LE000  bit     _L200C
_LE003  nop
        jmp     _LD000

        .logical *-$1007
_LD000  bit     _LD000
        jmp     _LF000

        .logical $f000
_LF000  bit     _LF000
        lda     _L3003
        and     _LE003
        nop
        rts

        .here
        .here
        .here
        .here
