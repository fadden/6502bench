         .setcpu "6502X"
;         .segment "SEG000"
         .org    $1000
         lda     L10AC
         ora     L10BC
         rts

         .byte   $33
         .byte   $33
         .byte   $33
         .byte   $80
         .byte   "4444"
         .byte   $80
         .byte   "55555"
         .byte   $80
         .byte   "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"
         .byte   $80
         .res    63,$4c
         .byte   $81
         .byte   $00
         .byte   $00
         .byte   $00
         .byte   $81
         .byte   $00
         .byte   $00
         .byte   $00
         .byte   $00
         .byte   $81
         .res    5,$00
         .byte   $81
         .res    8,$00
L10AC:   .res    8,$00
         .res    8,$82
L10BC:   .res    8,$82
