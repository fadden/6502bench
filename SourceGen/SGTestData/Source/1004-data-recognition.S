; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

        lda     zsplit
        ora     esplit
        rts

asc3    asc     '333'               ;no match
        dfb     $80
asc4    asc     '4444'              ;run of 4, as ASCII
        dfb     $80
asc5    asc     '55555'             ;run of 5, as ASCII
        dfb     $80
ascM    asc     'MMMMMMMMMM'        ;run of 62, as ASCII
        asc     'MMMMMMMMMM'
        asc     'MMMMMMMMMM'
        asc     'MMMMMMMMMM'
        asc     'MMMMMMMMMM'
        asc     'MMMMMMMMMM'
        asc     'MM'
        dfb     $80
ascL    asc     'LLLLLLLLLL'        ;run of 63, as fill
        asc     'LLLLLLLLLL'
        asc     'LLLLLLLLLL'
        asc     'LLLLLLLLLL'
        asc     'LLLLLLLLLL'
        asc     'LLLLLLLLLL'
        asc     'LLL'

        dfb     $81

zero    ds      3                   ;no match
        dfb     $81
        ds      4                   ;no match
        dfb     $81
        ds      5                   ;fill of 5
        dfb     $81

; The auto-label should split these in half.
        ds      8
zsplit  ds      8

        hex     8282828282828282
esplit  hex     8282828282828282

