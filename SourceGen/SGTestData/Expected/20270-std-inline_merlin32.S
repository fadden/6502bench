         org   $1000
         jmp   L1040

InAZ_test rts

InA1_test rts

InPZ_test rts

InP1_test rts

InW_test rts

InWA_test rts

InNR_test rts

         ds    54

L1040    nop
         jsr   InAZ_test
         asc   'Test AZ_ low',00
         jsr   InAZ_test
         asc   "Test AZ_ high",00
         jsr   InA1_test
         str   'Test A1_ low'
         jsr   InA1_test
         str   "Test A1_ high"
         jsr   InPZ_test
         hex   d445535420d0da5f00
         jsr   InP1_test
         hex   08d445535420d0315f
         jsr   InW_test
         dw    $1234
         jsr   InWA_test
         dw    L1040
         jsr   :L10AD
         jsr   InNR_test

         dfb   $ea
         dfb   $00

:L10AD   nop
         jsr   :L10B6
         jsr   :L10C3
         nop
         rts

:L10B6   jsr   InA1_test
         dfb   $ff
         asc   'too long'
         dfb   $ea

:L10C3   jsr   InAZ_test
         asc   'does not end'
