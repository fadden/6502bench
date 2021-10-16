        .cpu    "6502"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
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
        jsr     _L10B6
        jsr     _L10C3
        nop
        rts

_L10B6  jsr     InA1_test
        .byte   $ff
        .enc    "sg_ascii"
        .text   "too long"
        .byte   $ea

_L10C3  jsr     InAZ_test
        .text   "does not end"
