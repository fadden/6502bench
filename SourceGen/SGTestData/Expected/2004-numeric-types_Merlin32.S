;Project file was edited to get all big-endian data types, and to have an
;incorrect .junk alignment directive.
         org   $1000
         rts

         dfb   $11
         dw    $1122
         adr   $112233
         adrl  $11223344
         dfb   $11
         ddb   $1122
         dfb   $11,$22,$33
         dfb   $11,$22,$33,$44
         ds    2,$00
         dfb   $80
         ds    3,$00
         dfb   $80
         ds    4,$00
         dfb   $80
         ds    5,$00
         dfb   $80
         ds    256,$00
         dfb   $80
         ds    257,$cc
         hex   11
         dfb   $80
         hex   11223344556677889900
         dfb   $80
LABEL    hex   00112233445566778899aabbccddeeff00112233445566778899aabbccddeeff ;comment
         hex   00112233445566778899aabbccddeeffffeeddccbbaa99887766554433221100
         dfb   $80
         ds    \,$aa
         dfb   $81
         ds    63,$00
         dfb   $81
         ds    31,$ab
         hex   0000000000000001
         dfb   $81
         hex   1000000000000000
         dfb   $81
         ds    2,$dd      ;incorrect alignment
         ds    \,$00
