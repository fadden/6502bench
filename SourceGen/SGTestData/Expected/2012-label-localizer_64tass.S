        .cpu    "65816"
REALLYLONGLABELNAME = $8888 ;that's a long name

*       =       $1000
        .as
        .xs
        nop
start   lda     _L100D
        lda     nextglob
        lda     pastglob
        lda     _L100E
_L100D  nop
_L100E  nop
nextglob
        nop
pastglob
        nop
        lda     nlocal
nlocal  lda     #$11
reach1G nop
        lda     reach1G+1
        lda     _reach2+2
_reach2 nop
reach3G nop
_reach4 nop
        lda     _reach4-2
        lda     $00
        beq     _L102D
        jsr     _reach4
        jsr     start
_L102D  lda     #$22
        lda     gtest2
gtest1  nop
        lda     gtest3
gtest2  nop
gtest3  nop
        lda     #$33
        lda     $1041
topglob nop
        lda     _L1043
        nop
        nop
_L1043  nop
        lda     #$44
_globalnm
        jsr     _L104A
        nop
_L104A  nop
        nop
nglobal nop
globlat jsr     nglobal
        bra     end

end     nop
EXCESSIVELY_LONG_LABEL
        lda     REALLYLONGLABELNAME
        rts

