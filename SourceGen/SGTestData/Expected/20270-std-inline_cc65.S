         .setcpu "6502"
         .org    $1000
         jmp     L1040

InAZ_test: rts

InA1_test: rts

InPZ_test: rts

InP1_test: rts

InW_test: rts

InWA_test: rts

InNR_test: rts

         .res    54,$00

L1040:   nop
         jsr     InAZ_test
         .asciiz "Test AZ_ low"
         jsr     InAZ_test
         .byte   $d4,$e5,$f3,$f4,$a0,$c1,$da,$df,$a0,$e8,$e9,$e7,$e8,$00
         jsr     InA1_test
         .byte   $0c,"Test A1_ low"
         jsr     InA1_test
         .byte   $0d,$d4,$e5,$f3,$f4,$a0,$c1,$b1,$df,$a0,$e8,$e9,$e7,$e8
         jsr     InPZ_test
         .byte   $d4,$45,$53,$54,$20,$d0,$da,$5f,$00
         jsr     InP1_test
         .byte   $08,$d4,$45,$53,$54,$20,$d0,$31,$5f
         jsr     InW_test
         .word   $1234
         jsr     InWA_test
         .word   L1040
         jsr     @L10AD
         jsr     InNR_test

         .byte   $ea
         .byte   $00

@L10AD:  nop
         jsr     @L10B6
         jsr     @L10C3
         nop
         rts

@L10B6:  jsr     InA1_test
         .byte   $ff
         .byte   "too long"
         .byte   $ea

@L10C3:  jsr     InAZ_test
         .byte   "does not end"
