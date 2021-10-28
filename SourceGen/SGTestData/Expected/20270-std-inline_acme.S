        !cpu    6502
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
        jsr     InW_test1
        !word   $1100
        nop
        jmp     L4000

        !byte   $80

        !pseudopc $2000 {
InW_test1 nop
        jsr     InW_test1
        !word   $1200
        rts

        !byte   $80
        }

        !pseudopc $2000 {
InW_test2 nop
        jsr     InW_test2
        !word   $1300
        rts

        !byte   $80
        }

        !pseudopc $2000 {
not_inline nop
        jsr     not_inline
        bit     not_inline
        rts

        !byte   $81
        }
        !pseudopc $0000 {
InW_na_test !byte $ea
        !byte   $20
        !byte   $00
        !byte   $30
        !byte   $60
        !byte   $81
        }

        !pseudopc $4000 {
L4000   bit     L4000
        jsr     InW_project
        jmp     @LF000

        }
        !pseudopc $f000 {
@LF000  jsr     @LF008
        jsr     @LF015
        nop
        rts

@LF008  jsr     InA1_test
        !byte   $ff
        !text   "too long"
        !byte   $ea

@LF015  jsr     InAZ_test
        !text   "does not end"
        }
