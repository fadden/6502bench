        !cpu    65816
*       =       $1000
        !as
        !rs
        nop
L1001   bit     L1001
caddr   pea     $1003
        per     caddr-1
        lda     L1001+2
        lda     caddr
        lda     caddr+1
        lda     caddr+2
        rts

