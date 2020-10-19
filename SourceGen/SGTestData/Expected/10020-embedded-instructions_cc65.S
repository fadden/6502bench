         .setcpu "6502X"
;         .segment "SEG000"
         .org    $1000
         jsr     L100F
         jsr     L1017
         jsr     L101C
         jsr     L1046
         jmp     L1051

L100F:   lda     #$00
         .byte   $2c
@L1012:  lda     #$01
         beq     @L1012
         rts

L1017:   .byte   $20
@L1018:  rts

         .byte   $ea
         bvs     @L1018
L101C:   .byte   $2c
@L101D:  .byte   $2c
@L101E:  .byte   $2c
@L101F:  .byte   $2c
@L1020:  .byte   $2c
@L1021:  .byte   $2c
@L1022:  .byte   $2c
@L1023:  .byte   $2c
@L1024:  .byte   $2c
@L1025:  nop
         nop
         asl     A
         bcc     L101C
         asl     A
         bcc     @L101D
         asl     A
         bcc     @L101E
         asl     A
         bcc     @L101F
         asl     A
         bcc     @L1020
         asl     A
         bcc     @L1021
         asl     A
         bcc     @L1022
         asl     A
         bcc     @L1023
         asl     A
         bcc     @L1024
         asl     A
         bcc     @L1025
         rts

L1046:   .byte   $2c
@L1047:  nop
         .byte   $ad
@L1049:  lda     $00
         asl     A
         bcc     @L1047
         asl     A
         bcc     @L1049
L1051:   .byte   $ad
