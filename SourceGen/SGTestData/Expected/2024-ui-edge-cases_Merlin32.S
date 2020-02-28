         org   $2000
         jmp   L2100

         asc   'hello, '  ;string should be split by no-op addr change
         org   $200a
         asc   'world'
         dfb   $80

         org   $2100
L2100    lda   #$00
         sta   addr1-1    ;edit this operand
         sta   addr1
         sta   addr1+1
         jmp   L2121

         asc   'testing stuff.'
addr1    asc   '!?---'

L2121    rts

