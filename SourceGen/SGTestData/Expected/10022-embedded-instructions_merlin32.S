         org   $1000
         jsr   L1006
         jmp   L1020

L1006    sep   #$30
         lda   $00
         beq   :L100F
         lda   #$00
         brk

:L100F   stal  $012345
         lda   $00
         beq   :L101A
         lda   #$00
         dfb   $85
:L101A   dfb   $8f
         eor   $23
         dfb   $01
         nop
         rts

L1020    dfb   $af
