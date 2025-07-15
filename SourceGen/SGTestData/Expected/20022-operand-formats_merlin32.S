*Project file was edited to force ASCII formatting for some operands.
         org   $1000
         clc
         xce
         rep   #$30
         mx    %00
         lda   #$1234
         lda   #4660
         lda   #4660
         lda   #%0001001000110100
         lda   #$fffe
         lda   #65534
         lda   #65534
         lda   #%1111111111111110
         ldal  $fffefd
         ldal  16776957
         ldal  16776957
         ldal  %111111111111111011111101
         lda   #'h'
         lda   #"H"
         lda   #$6868
         ldal  'h'
         ldal  "H"
         nop
         lda   #0
         lda   #1
         lda   #32767
         lda   #-32768
         lda   #-1
         ldal  16776957
         rts

