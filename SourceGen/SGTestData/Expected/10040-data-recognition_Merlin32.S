         org   $1000
         lda   L10AC
         ora   L10BC
         rts

         dfb   $33
         dfb   $33
         dfb   $33
         dfb   $80
         asc   '4444'
         dfb   $80
         asc   '55555'
         dfb   $80
         asc   'MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM'
         dfb   $80
         ds    63,$4c
         dfb   $81
         dfb   $00
         dfb   $00
         dfb   $00
         dfb   $81
         dfb   $00
         dfb   $00
         dfb   $00
         dfb   $00
         dfb   $81
         ds    5
         dfb   $81
         ds    8
L10AC    ds    8
         ds    8,$82
L10BC    ds    8,$82
