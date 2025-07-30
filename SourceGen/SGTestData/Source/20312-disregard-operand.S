; Copyright 2025 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Tests the "disregard operand address" feature.
;
; Assembler: Merlin 32
;

        org     $012b00

        clc
        xce
        sep     #$30
        mx      %11

        jsr     st1_1a

        jsl     st2_1a
        jsl     st2_1b
        jsl     st2_1c
        jsl     st2_1d
        jsl     st2_1e

        jmp     done

        ds      \,$2b

st1_1a  lda     #$02
        pha
        plb
        lda     st1x        ;EDIT: disregard operand
st1x    rts

        lda     st1_1a      ;EDIT: disregard operand, then remove code entry tag

        ds      \,$2c

done    rts


;EDIT: create a project symbol $AFAFAF
        org     $022c00

st2_1a  dfb     $af         ;EDIT: disregard operand (here and below)
st2_1b  dfb     $af         ;EDIT: also format this one as decimal
st2_1c  dfb     $af
st2_1d  dfb     $af
st2_1e  dfb     $af
        dfb     $af
        dfb     $af
        nop
        nop
        nop
        rtl

        ds      \,$8c
