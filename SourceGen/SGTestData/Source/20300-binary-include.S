; Copyright 2024 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
;

        org $1000
start   ldy #40
        lda blob1,Y
        sta $2000,Y
        dey
        bpl start

        lda blob2
        lda blob2+1
        lda blob2+31

        jmp done

; EDIT: set blob as binary include, current dir
blob1
        asc ' !"#$%&',27,'()'
        asc '0123456789'
        asc 'ABCDEFGHIJ'
        asc 'PQRSTUVWXY'

; EDIT: set blob as binary include, file in subdir
blob2
        ds  32,$ff

done    rts
