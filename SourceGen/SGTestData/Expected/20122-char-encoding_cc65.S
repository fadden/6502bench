         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         rep     #$30
         .a16
         .i16
         lda     #'B'
         lda     #'B' | $80
         lda     #$c2
         lda     #$42
         sep     #$30
         .a8
         .i8
         rts

