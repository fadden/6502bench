        .cpu    "6502"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
InW_project =   $e000

*       =       $1000
        jmp     L1040

InAZ_test rts

InA1_test rts

InPZ_test rts

InP1_test rts

InW_test rts

InWA_test rts

InNR_test rts

        .fill   54,$00

L1040   nop
        jsr     InAZ_test
        .null   "Test AZ_ low"
        jsr     InAZ_test
        .enc    "sg_hiascii"
        .null   "Test AZ_ high"
        jsr     InA1_test
        .enc    "sg_ascii"
        .ptext  "Test A1_ low"
        jsr     InA1_test
        .enc    "sg_hiascii"
        .ptext  "Test A1_ high"
        jsr     InPZ_test
        .enc    "none"
        .null   "Test PZ",$5f
        jsr     InP1_test
        .ptext  "Test P1",$5f
        jsr     InW_test
        .word   $1234
        jsr     InWA_test
        .word   L1040
        jsr     _L10AD
        jsr     InNR_test

        .byte   $ea
        .byte   $00

_L10AD  nop
        jsr     InW_test1
        .word   $1100
        nop
        jmp     L4000

        .byte   $80

        .logical $2000
InW_test1 nop
        jsr     InW_test1
        .word   $1200
        rts

        .byte   $80
        .here

        .logical $2000
InW_test2 nop
        jsr     InW_test2
        .word   $1300
        rts

        .byte   $80
        .here

        .logical $2000
not_inline nop
        jsr     not_inline
        bit     not_inline
        rts

        .byte   $81
        .here
        .logical $0000
InW_na_test .byte $ea
        .byte   $20
        .byte   $00
        .byte   $30
        .byte   $60
        .byte   $81
        .here

        .logical $4000
L4000   bit     L4000
        jsr     InW_project
        jmp     _LF000

        .here
        .logical $f000
_LF000  jsr     _LF008
        jsr     _LF015
        nop
        rts

_LF008  jsr     InA1_test
        .byte   $ff
        .enc    "sg_ascii"
        .text   "too long"
        .byte   $ea

_LF015  jsr     InAZ_test
        .text   "does not end"
        .here
