         org   $1000
load11   lda   #$11
:L1002   ldx   #$22
:load33  ldy   #$33
:L1006   lda   #$44
:predat  bne   :L1042

         dw    $0123
:dat1    dw    $4567
         dw    $89ab
:L1010   dw    $cdef
:L1012   dw    $0011
:L1014   dw    $2233
         dfb   $80
         asc   'The quick brown fox'
         dfb   $80
         dw    :L1042
         dw    :L1041
         dw    :L1042+1
:fill0   ds    16
:L1041   dfb   $80

:L1042   lda   :predat+2
         lda   :L1041
         asl   :dat1
         rol   :dat1+2
         ror   :L1010
         and   :L1012
         ora   :L1014
         lda   :fill0
         sta   :fill0+4
         lda   :fill0+8
         sta   :fill0+12
         jsr   :L1002
:L1066   lda   :L1006
:L1069   bit   :L1066+2
         ora   :L1066+2
         lda   :L1069+1
         lda   :L1069+2
         lda   #$ea
:L1077   sta   :L1077
:L107A   sta   :L107A+1
         sta   $107f      ;self-ref; operand format refs nonexistent symbol
         jmp   :L2002

         dfb   $80
:dat81   dfb   $81
         org   $2000
:L2000   dfb   $82
         dfb   $83

:L2002   nop
         nop
         lda   :dat81
         lda   :L2000
         jmp   :L2018

:L200D   dfb   $7c
:L200E   dfb   $7d
:L200F   dfb   $7e
         dfb   $7f
:nearby  dfb   $80
         dfb   $81
         dfb   $82
         dfb   $83
:L2015   dfb   $84
:L2016   dfb   $85
:L2017   dfb   $86

:L2018   lda   :L200D
         lda   :L200E
         lda   :L200F
         lda   :nearby-1
         lda   :nearby
         lda   :nearby+1
         lda   :nearby+2
         lda   :nearby+3
         lda   :L2015
         lda   :L2016
         lda   :L2017
         rts

