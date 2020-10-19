        !cpu    65816
*       =       $1000
        !as
        !rs
        clc
        xce
        rep     #$30
        !al
        !rl
        lda     #'B'
        lda     #'B' | $80
        lda     #$c2
        lda     #$42
        sep     #$30
        !as
        !rs
        rts

