;Project file was edited to force ASCII formatting for some operands.
         .setcpu "65816"
         .org    $1000
         .a8
         .i8
         clc
         xce
         rep     #$30
         .a16
         .i16
         lda     #$1234
         lda     #4660
         lda     #4660
         lda     #%0001001000110100
         lda     #$fffe
         lda     #65534
         lda     #65534
         lda     #%1111111111111110
         lda     $fffefd
         lda     16776957
         lda     16776957
         lda     %111111111111111011111101
         lda     #'h'
         lda     #'H' | $80
         lda     #$6868
         lda     f:'h'
         lda     f:'H' | $80
         rts

