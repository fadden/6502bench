        .cpu    "65c02"
REALLYLONGLABELNAME = $8888 ;that's a long name

*       =       $1000
        nop
X_start lda     _start
        lda     X_start1
        lda     pastglob
        lda     _X__nopped
_start  nop
_X__nopped
        nop
X_start1
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
_X_reach4
        nop
        lda     _X_reach4-2
        lda     $00
        beq     _L102D
        jsr     _X_reach4
        jsr     X_start
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
globalnm
        jsr     _L104A
        nop
_L104A  nop
        nop
nglobal nop
globlat jsr     nglobal
        bra     NOT

NOT     nop
EXCESSIVELY_LONG_LABEL
        lda     REALLYLONGLABELNAME
_X_uname
        nop
_X_uname11
        nop
_X_uname1
        nop
_X_uname2
        nop
        lda     #$00
_AND    bne     _AND       ;local
JMP1    bne     JMP1       ;global
jmp1    bne     jmp1
TSB1    bne     TSB1
XCE     bne     XCE
        rts

