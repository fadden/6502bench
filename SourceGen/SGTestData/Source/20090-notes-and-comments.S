; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; Everything of interest is in the notes and comments.  We're just
; providing some anchor points.

plataddr    equ     $3000       ;defined in TestSyms

            org     $1000

            lda     #$01
            lda     #$02
            lda     #$03
            lda     #$04
            lda     #$05
            lda     #$06
            lda     #$07
            lda     #$08
            lda     #$09
            lda     #$0a
            lda     #$0b
            lda     #$0c
            lda     #$0d
            lda     #$0e
            lda     #$0f

            bit     plataddr
            rts

            hex     000102030405060708090a0b0c0d0e0f
            hex     000102030405060708090a0b0c0d0e0f
            hex     000102030405060708090a0b0c0d0e0f
            hex     000102030405060708090a0b0c0d0e0f
