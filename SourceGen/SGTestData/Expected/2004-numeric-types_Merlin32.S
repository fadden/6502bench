;Project file was edited to get all big-endian data types, and to have an
;incorrect .junk alignment directive.
         org   $1000
         bit   L1448
         jsr   L14A8
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         rts

         dfb   $11
         dw    $1122
         adr   $112233
         adrl  $11223344
         dfb   $11
         ddb   $1122
         dfb   $11,$22,$33
         dfb   $11,$22,$33,$44
         ds    2
         dfb   $80
         ds    3
         dfb   $80
         ds    4
         dfb   $80
         ds    5
         dfb   $80
         ds    256
         dfb   $80
         ds    257,$cc
         hex   11
         dfb   $80
         hex   11223344556677889900
         dfb   $80
LABEL    hex   00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff ;comment on bulk
         hex   00112233445566778899aabbccddeeffffeeddccbbaa99887766554433221100
         dfb   $80
         ds    \,$aa
         dfb   $81
         ds    63
         dfb   $81
         ds    31,$ab
         hex   0000000000000001
         dfb   $81
         hex   1000000000000000
         dfb   $81
         ds    2,$dd      ;incorrect alignment
         ds    \
         ds    8,$82
         org   $1408
         ds    8,$82      ;note no-op .ORG
         ds    8,$83
         org   $1428
         ds    8,$83      ;meaningful .ORG
         ds    8,$84
UserLabel ds   8,$84
         ds    8,$85
L1448    ds    8,$85
         ds    16,$86
         asc   8787878787878787
;long comment
         asc   8787878787878787
         ds    16,$88
         ds    8,$89
         ds    8,$89
         asc   8a8a8a8a8a8a8a8a
         asc   8a8a8a8a8a8a8a8a
         ds    8,$8b
L14A8    ds    8,$8b
         ds    8,$8c
         dfb   %10001100
         ds    7,$8c
