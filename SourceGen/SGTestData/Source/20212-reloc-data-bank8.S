; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
;
; Segment #4 : code, loads somewhere in bank 8

        REL                     ;generate relocatable code

BANK2_START EXT

BANK8_START ENT
start   ldal    start
        lda     addr
        nop
        ldal    $010000
        ldal    $020000
        ldal    $030000
        ldal    $080000
        rtl

BANK8_ADDR ENT
addr
        adrl    addr
        adrl    BANK2_START
        dfb     $80
        adrl    $081000

BANK8_MOV_SRC ENT
        asc     'This is a test.',$00
