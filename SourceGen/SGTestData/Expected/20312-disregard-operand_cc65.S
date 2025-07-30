         .setcpu "65816"
         .org    $012b00
         .a8
         .i8
         clc
         xce
         sep     #$30
         jsr     L12C00 & $ffff
         jsl     L22C00
         jsl     L22C01
         jsl     L22C02
         jsl     L22C03
         jsl     L22C04
         jmp     L12D00 & $ffff

         .res    226,$2b

L12C00:  lda     #$02
         pha
         plb
         lda     $2c07
         rts

         .byte   $ad
         .byte   $00
         .res    246,$2c

L12D00:  rts

         .org    $022c00
L22C00:  .byte   $af
L22C01:  .byte   $af
L22C02:  .byte   $af
L22C03:  .byte   $af
L22C04:  .byte   $af
         .byte   $af
         .byte   $af
         nop
         nop
         nop
         rtl

         .res    245,$8c
