         .setcpu "6502"
         .org    $2000
         jmp     L2100

         .byte   "hello, "  ;string should be split by no-op addr change
         .org    $200a
         .byte   "world"
         .byte   $80

         .org    $2100
L2100:   lda     #$00
         sta     addr1-1    ;edit this operand
         sta     addr1
         sta     addr1+1
         jmp     L2121

         .byte   "testing stuff."
addr1:   .byte   "!?---"

L2121:   rts

