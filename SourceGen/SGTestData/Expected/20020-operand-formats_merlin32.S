*Project file was edited to force ASCII formatting for some operands.
         org   $1000
         lda   $01
         lda   $0102
         lda   $fe
         lda   $feff
         lda   1
         lda   258
         lda   254
         lda   65279
         lda   1
         lda   258
         lda   254
         lda   65279
         lda   %00000001
         lda   %0000000100000010
         lda   %11111110
         lda   %1111111011111111
         jmp   skipdata

         dfb   $01
         dw    $0201
         adr   $030201
         adrl  $04030201
         dfb   1
         dw    513
         adr   197121
         adrl  67305985
         dfb   1
         dw    513
         adr   197121
         adrl  67305985
         dfb   %00000001
         dw    %0000001000000001
         adr   %000000110000001000000001
         adrl  %00000100000000110000001000000001
         dfb   255
         dw    65279
         adr   16645887
         adrl  4244504319

skipdata lda   #'h'
         lda   'h'
         lda:  'h'
         lda   #$1f
         lda   #' '
         lda   #'"'
         lda   #$27
         lda   #'~'
         lda   #$7f
         lda   #$80
         lda   #$9f
         lda   #" "
         lda   #$a2
         lda   #"'"
         lda   #"~"
         lda   #$ff
         jmp   L10A4

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
         adrl  more_ascii
         ddb   more_ascii
         dfb   '['
         dfb   $7b
         dfb   '|'
         dfb   $7d
         dfb   ','
         dfb   "["
         dfb   $fb
         dfb   "|"
         dfb   $fd
         dfb   ","

*Signed-decimal tests.  All of these must be formatted as signed decimal; this
*may require hand-editing or modifications to the instruction operand editor.
L10A4    nop
         lda   #0
         lda   #1
         lda   #127
         lda   #-128
         lda   #-1
         lda   1
         lda   254
         lda:  1
         lda   65534
         jmp   4311

         dfb   0
         dfb   1
         dfb   127
         dfb   -128
         dfb   -1
         dw    1
         dw    -2
         ddb   2
         ddb   -3
         adr   66051
         adr   -259
         adrl  16909060
         adrl  -66052

         rts

