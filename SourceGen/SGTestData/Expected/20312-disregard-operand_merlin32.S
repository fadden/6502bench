         org   $012b00
         clc
         xce
         sep   #$30
         jsr   L12C00
         jsl   L22C00
         jsl   L22C01
         jsl   L22C02
         jsl   L22C03
         jsl   L22C04
         jmp   L12D00

         ds    226,$2b

L12C00   lda   #$02
         pha
         plb
         lda   $2c07
         rts

         dfb   $ad
         dfb   $00
         ds    246,$2c

L12D00   rts

         org   $022c00
L22C00   dfb   $af
L22C01   dfb   $af
L22C02   dfb   $af
L22C03   dfb   $af
L22C04   dfb   $af
         dfb   $af
         dfb   $af
         nop
         nop
         nop
         rtl

         ds    245,$8c
