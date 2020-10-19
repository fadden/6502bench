; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: ACME (has good PETSCII/screen code support)
; 65816 version

        !cpu    65816
*       =       $1000

        clc
        xce

; Single letter in a 16-bit immediate
        rep     #$30
        !al
        !rl
        lda     #'B'            ;format as low ASCII
        lda     #'B' | $80      ;format as high ASCII
        lda     #'B' | $80      ;format as PETSCII
        lda     #'B'            ;format as screen code

        sep     #$30
        !as
        !rs

        rts
