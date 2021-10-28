InW_project equ $e000

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
         jsr   InW_test1
         dw    $1100
         nop
         jmp   L4000

         dfb   $80

         org   $2000
InW_test1 nop
         jsr   InW_test1
         dw    $1200
         rts

         dfb   $80

         org   $2000
InW_test2 nop
         jsr   InW_test2
         dw    $1300
         rts

         dfb   $80

         org   $2000
not_inline nop
         jsr   not_inline
         bit   not_inline
         rts

         dfb   $81
         org   $0000
InW_na_test dfb $ea
         dfb   $20
         dfb   $00
         dfb   $30
         dfb   $60
         dfb   $81

         org   $4000
L4000    bit   L4000
         jsr   InW_project
         jmp   :LF000

         org   $f000
:LF000   jsr   :LF008
         jsr   :LF015
         nop
         rts

:LF008   jsr   InA1_test
         dfb   $ff
         asc   'too long'
         dfb   $ea

:LF015   jsr   InAZ_test
         asc   'does not end'
