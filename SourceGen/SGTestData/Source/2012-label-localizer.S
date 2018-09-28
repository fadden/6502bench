; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

EXTSYM      equ     $8888           ;EDIT: give this a long label

            org     $1000

            nop
start       lda     :local1
            lda     nextglob
            lda     pastglob
            lda     :local2

:local1     nop
:local2     nop
nextglob    nop                     ;EDIT: mark label as global
pastglob    nop

            lda     nlocal
nlocal      lda     #$11            ;EDIT: mark label as global

reach1G     nop                     ;EDIT: mark label as global
:local      lda     :local          ;EDIT: edit operand to symbol=reach1G

            lda     reach4          ;EDIT: set operand to symbol=reach2; should stay local
reach2      nop
reach3G     nop                     ;EDIT: mark label as global
reach4      nop
            lda     reach2          ;EDIT: edit operand to symbol=reach4; should stay local

            lda     $00
            beq     :local          ;should be local
            jsr     reach4          ;should be local
            jsr     start           ;should be global
:local      lda     #$22

            lda     gtest2
gtest1      nop                     ;EDIT: mark label as global
            lda     gtest3
gtest2      nop                     ;...which forces this to be global
gtest3      nop                     ;...and then this is forced to be global

            lda     #$33

            lda     midlocal        ;EDIT: reformat as hex
topglob     nop                     ;EDIT: mark label as global
            lda     farlocal        ;should be local label ref
midlocal    nop                     ;auto-label here should vanish
            nop
farlocal    nop

            lda     #$44
globalnm    jsr     :local2         ;EDIT: mark label as global
:local1     nop
:local2     nop
:local3     nop
nglobal     nop                     ;should get marked as global
globlat     jsr     nglobal         ;EDIT: mark label as global

            bra     cont
cont        nop                     ;EDIT: mark label as global
            lda     EXTSYM
            rts
