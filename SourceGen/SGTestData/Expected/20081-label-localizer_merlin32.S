REALLYLONGLABELNAME equ $8888 ;that's a long name

         org   $1000
         nop
_start   lda   :start
         lda   X_start
         lda   pastglob
         lda   :__nopped
:start   nop
:__nopped
         nop
X_start  nop
pastglob nop
         lda   nlocal
nlocal   lda   #$11
reach1G  nop
         lda   reach1G+1
         lda   :reach2+2
:reach2  nop
reach3G  nop
:_reach4 nop
         lda   :_reach4-2
         lda   $00
         beq   :L102D
         jsr   :_reach4
         jsr   _start
:L102D   lda   #$22
         lda   gtest2
gtest1   nop
         lda   gtest3
gtest2   nop
gtest3   nop
         lda   #$33
         lda   $1041
topglob  nop
         lda   :L1043
         nop
         nop
:L1043   nop
         lda   #$44
globalnm jsr   :L104A
         nop
:L104A   nop
         nop
nglobal  nop
globlat  jsr   nglobal
         bra   NOT

NOT      nop
EXCESSIVELY_LONG_LABEL
         lda   REALLYLONGLABELNAME
:_uname  nop
:X_uname11
         nop
:X_uname1
         nop
:X_uname nop
         lda   #$00
:AND     bne   :AND       ;local
JMP      bne   JMP        ;global
jmp      bne   jmp
TSB      bne   TSB
XCE      bne   XCE
         rts

