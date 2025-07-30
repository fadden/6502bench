        .cpu    "65816"
        .logical $012b00
        .as
        .xs
        clc
        xce
        sep     #$30
        jsr     L12C00
        jsl     L22C00
        jsl     L22C01
        jsl     L22C02
        jsl     L22C03
        jsl     L22C04
        jmp     L12D00

        .fill   226,$2b

L12C00  lda     #$02
        pha
        plb
        lda     $2c07
        rts

        .byte   $ad
        .byte   $00
        .fill   246,$2c

L12D00  rts

        .here
        .logical $022c00
L22C00  .byte   $af
L22C01  .byte   $af
L22C02  .byte   $af
L22C03  .byte   $af
L22C04  .byte   $af
        .byte   $af
        .byte   $af
        nop
        nop
        nop
        rtl

        .fill   245,$8c
        .here
