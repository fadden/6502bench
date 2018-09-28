         org   $1000
         dfb   $03
         dfb   $02

L1002    bit   L1002
         dfb   $2c
         lda   #$11
         nop
         dfb   $2c
L100A    ldx   #$ff
         nop
         jsr   L100A
         nop
         dfb   $2c
L1012    ldx   #$ff
         nop
         jsr   L1012
         jsr   $2456
L101B    adrl  $22a211a9
         jsr   L101B
         jsr   L1028
         jsr   $2456
L1028    adrl  $44a233a9
         jsr   L1037
         jsr   L103A
         nop
         lda   $2456
         rts

L1037    jsr   $2456
L103A    lda   #$55
         ldx   #$66
         rts

