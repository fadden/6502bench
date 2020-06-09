;Project file was edited to force ASCII formatting for some operands.
        !cpu    65816
*       =       $1000
        !as
        !rs
        clc
        xce
        rep     #$30
        !al
        !rl
        lda     #$1234
        lda     #4660
        lda     #4660
        lda     #%0001001000110100
        lda     #$fffe
        lda     #65534
        lda     #65534
        lda     #%1111111111111110
        lda+3   $fffefd
        lda+3   16776957
        lda+3   16776957
        lda+3   %111111111111111011111101
        lda     #'h'
        lda     #'H' | $80
        lda     #$6868
        lda+3   'h'
        lda+3   'H' | $80
        rts

