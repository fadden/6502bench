         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$30
         jsr     L1014
         jsr     L101C
         jsr     L102A
         jsr     L102F
         jsr     L1059
         rts

L1014:   lda     #$00
         .byte   $2c
L1017:   lda     #$01
         beq     L1017
         rts

L101C:   sep     #$30
         lda     $00
         beq     L1025
         lda     #$00
         brk

L1025:   sta     $012345
         rts

L102A:   .byte   $20
L102B:   rts

         .byte   $ea
         bra     L102B

L102F:   .byte   $2c
L1030:   .byte   $2c
L1031:   .byte   $2c
L1032:   .byte   $2c
L1033:   .byte   $2c
L1034:   .byte   $2c
L1035:   .byte   $2c
L1036:   .byte   $2c
L1037:   .byte   $2c
L1038:   nop
         nop
         asl     A
         bcc     L102F
         asl     A
         bcc     L1030
         asl     A
         bcc     L1031
         asl     A
         bcc     L1032
         asl     A
         bcc     L1033
         asl     A
         bcc     L1034
         asl     A
         bcc     L1035
         asl     A
         bcc     L1036
         asl     A
         bcc     L1037
         asl     A
         bcc     L1038
         rts

L1059:   .byte   $2c
L105A:   nop
         .byte   $ad
L105C:   lda     $00
         asl     A
         bcc     L105A
         asl     A
         bcc     L105C
         .byte   $af
