;Project file was edited to force ASCII formatting for some operands.
        .cpu    "65816"
        .enc    sg_hiascii
        .cdef   $20,$7e,$a0
        .enc    sg_ascii
        .cdef   $20,$7e,$20
*       =       $1000
        .as
        .xs
        clc
        xce
        rep     #$30
        .al
        .xl
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
        lda     @l'h'
        lda     @l'H' | $80
        rts

