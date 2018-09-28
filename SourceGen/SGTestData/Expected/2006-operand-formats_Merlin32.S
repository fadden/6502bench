;Project file was edited for some ASCII operands.
         org   $1000
         clc
         xce
         sep   #$30
         lda   $01
         lda   $0102
         ldal  $010203
         lda   1
         lda   258
         ldal  66051
         lda   %00000001
         lda   %0000000100000010
         ldal  %000000010000001000000011
         bra   skipdata

         dfb   $01
         dw    $0201
         adr   $030201
         adrl  $04030201
         dfb   1
         dw    513
         adr   197121
         adrl  67305985
         dfb   %00000001
         dw    %0000001000000001
         adr   %000000110000001000000001
         adrl  %00000100000000110000001000000001

skipdata lda   #'h'
         lda   'h'
         lda:  'h'
         ldal  'h'
         lda   #$1f
         lda   #' '
         lda   #'~'
         lda   #$7f
         lda   #$80
         lda   #$9f
         lda   #" "
         lda   #"~"
         lda   #$ff
         rep   #'0'
         mx    %00
         lda   #'h'
         lda   #"H"
         lda   #$6868
         rts

more_ascii dfb 'h'
         dfb   $80
         dw    $6868
         dfb   $80
         dw    skipdata
         adr   skipdata
         ddb   skipdata
         dfb   <more_ascii
         dfb   >more_ascii
         dw    more_ascii
         adr   more_ascii
         ddb   more_ascii
