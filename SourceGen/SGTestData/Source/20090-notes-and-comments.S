; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; Everything of interest is in the notes and comments.  We're just
; providing some anchor points.

plataddr    equ     $3000       ;defined in TestSyms

            org     $1000

; places to put "basic" comments
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

            jmp     next
            nop

; multi-line operand
            hex     000102030405060708090a0b0c0d0e0f
            hex     000102030405060708090a0b0c0d0e0f
            hex     000102030405060708090a0b0c0d0e0f
            hex     000102030405060708090a0b0c0d0e0f

next
            bit     plataddr        ;pull in platform symbol, with comment

            lda     #$10
            lda     #$11
            lda     #$12
            lda     #$13
            lda     #$14
            lda     #$15
            lda     #$16
            lda     #$17
            lda     #$18
            lda     #$19
            lda     #$1a
            lda     #$1b
            lda     #$1c
            lda     #$1d
            lda     #$1e
            lda     #$1f

            lda     #$20
            lda     #$21
            lda     #$22
            lda     #$23
            lda     #$24
            lda     #$25
            lda     #$26
            lda     #$27
            lda     #$28
            lda     #$29
            lda     #$2a
            lda     #$2b
            lda     #$2c
            lda     #$2d
            lda     #$2e
            lda     #$2f

            jmp     done

done        rts

