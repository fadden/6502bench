        .cpu    "65816"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
*       =       $1000
        .as
        .xs
        clc
        xce
        rep     #$30
        .al
        .xl
        lda     #'B'
        lda     #'B' | $80
        .enc    "none"
        lda     #'B'
        .enc    "screen"
        lda     #'B'
        sep     #$30
        .as
        .xs
        rts

