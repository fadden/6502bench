         .setcpu "65816"
         .org    $031000
         .a8
         .i8
L31000:  lda     L31000
@L31004: lda     @L31004
         and     f:@L1101F
         jml     @L21010

         .org    $021010
@L21010: lda     @L21010
         jml     @L11018

         .org    $011018
@L11018: lda     @L11018
         jmp     @L11026 & $ffff

@L1101F: .byte   $80
         .org    $0000
         .byte   $ea
         .byte   $60
         .dword  @L31004

         .org    $011026
@L11026: nop
         rts

