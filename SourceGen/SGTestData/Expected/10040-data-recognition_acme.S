        !cpu    6510
*       =       $1000
        lda     L10AC
        ora     L10BC
        rts

        !byte   $33
        !byte   $33
        !byte   $33
        !byte   $80
        !text   "4444"
        !byte   $80
        !text   "55555"
        !byte   $80
        !text   "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"
        !byte   $80
        !fill   63,$4c
        !byte   $81
        !byte   $00
        !byte   $00
        !byte   $00
        !byte   $81
        !byte   $00
        !byte   $00
        !byte   $00
        !byte   $00
        !byte   $81
        !fill   5,$00
        !byte   $81
        !fill   8,$00
L10AC   !fill   8,$00
        !fill   8,$82
L10BC   !fill   8,$82
