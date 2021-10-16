        !cpu    6502
*       =       $1000
        jmp     L1040

InAZ_test rts

InA1_test rts

InPZ_test rts

InP1_test rts

InW_test rts

InWA_test rts

InNR_test rts

        !fill   54,$00

L1040   nop
        jsr     InAZ_test
        !text   "Test AZ_ low",$00
        jsr     InAZ_test
        !hex    d4e5f3f4a0c1dadfa0e8e9e7e800
        jsr     InA1_test
        !text   $0c,"Test A1_ low"
        jsr     InA1_test
        !hex    0dd4e5f3f4a0c1b1dfa0e8e9e7e8
        jsr     InPZ_test
        !pet    "Test PZ",$5f,$00
        jsr     InP1_test
        !pet    $08,"Test P1",$5f
        jsr     InW_test
        !word   $1234
        jsr     InWA_test
        !word   L1040
        jsr     @L10AD
        jsr     InNR_test

        !byte   $ea
        !byte   $00

@L10AD  nop
        jsr     @L10B6
        jsr     @L10C3
        nop
        rts

@L10B6  jsr     InA1_test
        !byte   $ff
        !text   "too long"
        !byte   $ea

@L10C3  jsr     InAZ_test
        !text   "does not end"
