         .setcpu "6502"
         .org    $1000
         jmp     L0000

         .org    $0000
L0000:   bit     a:L0000
L0003:   lda     L0000
         lda     L0003
         .byte   $d0,$ba
         .byte   $30,$b8
         bvs     L0012
         bvc     L0080

lodat:   .byte   $00
         .byte   $01
         .byte   $02

L0012:   lda     lodat+1
         clc
         .byte   $90,$a9

         .org    $0080
L0080:   bit     a:L0080
         jmp     LFFC6

         .org    $ffc0
LFFC0:   bit     LFFC0
LFFC3:   clc
         .byte   $90,$3d

LFFC6:   rts

