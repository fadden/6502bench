         .setcpu "65816"
REALLYLONGLABELNAME = $8888 ;that's a long name

         .org    $1000
         .a8
         .i8
         nop
start:   lda     @L100D
         lda     nextglob
         lda     pastglob
         lda     @L100E
@L100D:  nop
@L100E:  nop
nextglob:
         nop
pastglob:
         nop
         lda     nlocal
nlocal:  lda     #$11
reach1G: nop
         lda     reach1G+1
         lda     @reach2+2
@reach2: nop
reach3G: nop
@reach4: nop
         lda     @reach4-2
         lda     $00
         beq     @L102D
         jsr     @reach4
         jsr     start
@L102D:  lda     #$22
         lda     gtest2
gtest1:  nop
         lda     gtest3
gtest2:  nop
gtest3:  nop
         lda     #$33
         lda     $1041
topglob: nop
         lda     @L1043
         nop
         nop
@L1043:  nop
         lda     #$44
@globalnm:
         jsr     @L104A
         nop
@L104A:  nop
         nop
nglobal: nop
globlat: jsr     nglobal
         bra     end

end:     nop
EXCESSIVELY_LONG_LABEL:
         lda     REALLYLONGLABELNAME
         rts

