         .setcpu "6502"
;         .segment "SEG000"
         .org    $1000
         .byte   $03
         .byte   $02

L1002:   bit     L1002
         .byte   $2c
         lda     #$11
         nop
         .byte   $2c
@L100A:  ldx     #$ff
         nop
         jsr     @L100A
         nop
         .byte   $2c
@L1012:  ldx     #$ff
         nop
         jsr     @L1012
         jsr     $2456
@L101B:  .dword  $22a211a9
         jsr     @L101B
         jsr     @L1028
         jsr     $2456
@L1028:  .dword  $44a233a9
         jsr     @L103A
         jsr     @L103D
         nop
         lda     $2456
         jsr     @L1045
         rts

@L103A:  jsr     $2456
@L103D:  lda     #$55
         ldx     #$66
         rts

         .byte   $81
         .byte   $82
         .byte   $83

@L1045:  lda     #$99
         rts

