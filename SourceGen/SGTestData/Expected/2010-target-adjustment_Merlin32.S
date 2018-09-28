         org   $1000
load11   lda   #$11
L1002    ldx   #$22
load33   ldy   #$33
L1006    lda   #$44
predat   bra   L1042

         dw    $0123
dat1     dw    $4567
         dw    $89ab
L1010    dw    $cdef
L1012    dw    $0011
L1014    dw    $2233
         dfb   $80
         asc   'The quick brown fox'
         dfb   $80
         dw    L1042
         dw    L1041
         dw    L1042+1
fill0    ds    16,$00
L1041    dfb   $80

L1042    lda   predat+2
         lda   L1041
         asl   dat1
         rol   dat1+2
         ror   L1010
         and   L1012
         ora   L1014
         lda   fill0
         sta   fill0+4
         lda   fill0+8
         sta   fill0+12
         jsr   L1002
         lda   L1006
L1069    pea   L1069-1
         per   L1069-1
         lda   L1069+1
         lda   L1069+2
         lda   #$ea
L1077    sta   L1077
L107A    sta   L107A+1
         sta   $107f
         brl   L2002

         dfb   $80
dat81    dfb   $81
         org   $2000
L2000    dfb   $82
         dfb   $83

L2002    bit   L2002
         lda   dat81
         lda   L2000
         rts

