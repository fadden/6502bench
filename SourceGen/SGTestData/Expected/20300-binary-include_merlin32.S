         org   $1000
L1000    ldy   #$28
         lda   :L1017,y
         sta   $2000,y
         dey
         bpl   L1000
         lda   :L103F
         lda   :L103F+1
         lda   done-1
         jmp   done

:L1017   hex   20212223242526272829303132333435363738394142434445464748494a5051
         hex   5253545556575859
:L103F   ds    32,$ff

done     rts

