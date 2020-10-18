G_DP     equ   $20

         org   $1000
         dfb   $0f,$10,$01
         rts

L1004    dfb   $9f,$10,$04
         rts

         dfb   $80
         dfb   $80
         dfb   $80

]L_DP    equ   $30
:L100B   nop
         dfb   $27,$10
         dfb   $b7,$10
         dfb   $47,$20
         dfb   $d7,$20
         dfb   $67,$30
         dfb   $f7,$30
:L1018   dfb   $2f,$10,$fd
         dfb   $bf,$10,$fa
         dfb   $4f,$20,$f7
         dfb   $df,$20,$f4
         dfb   $6f,$30,$f1
         dfb   $ff,$30,$ee
         nop
         rts

