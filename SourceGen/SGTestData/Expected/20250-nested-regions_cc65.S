         .setcpu "6502"
         .org    $0000
         .word   $3000      ;load address

         .org    $3000
L3000:   bit     L3000
@L3003:  lda     @L3003
         and     @LD003
         jmp     @L200C

         .org    $200c
@L200C:  bit     @L200C
         jmp     @L1012

         .org    $1012
@L1012:  bit     @L1012
         jsr     @L4000
         .org    $0000
         .byte   $00
         .byte   $ce,$55,$4c,$4c,$2d,$54,$45,$52,$4d,$20,$d0,$c5,$d4,$d3,$c3,$c9
         .byte   $c9,$20,$53,$54,$52,$49,$4e,$47,$00
         .byte   $80
         .word   @L3003
         .word   @LD003
         .byte   $80

         .org    $4000
@L4000:  bit     @L4000
         bit     @L5000
         bit     @L500F
         bit     @L500F
         nop
         jmp     @L4020

         .org    $5000
@L5000:  bit     @L5000
         bit     @L4000
         nop
         nop
@L5008:  bit     @L5008
         bit     @L5017
         nop
@L500F:  rts

         .org    $4020
@L4020:  bit     @L4020
         bit     @L500F
         nop
         nop
         nop
         nop
         nop
         nop
         nop
         jmp     @L4040

         .org    $5008
@L5008_0: bit    @L5008_0
         bit     @L5000
         nop
@L500F_0: bit    @L500F_0
         nop
         nop
         nop
         nop
         nop
@L5017:  rts

         .org    $4040
@L4040:  bit     @L4040
         bit     @L5017
         nop
         jmp     @LD000

         .org    $d000
@LD000:  bit     @L200C
@LD003:  nop
         jmp     @LE000

         .org    $e000
@LE000:  bit     @LE000
         jmp     @LF000

         .org    $f000
@LF000:  bit     @LF000
         lda     @L3003
         and     @LD003
         nop
         rts

