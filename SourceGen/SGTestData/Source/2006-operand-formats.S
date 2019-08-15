; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

; 65816 with short regs
        clc
        xce
        sep     #$30
        mx      %11

; Basic operand formats.  Show first set as hex, second as decimal,
; third as binary.
        lda     $01
        lda     $0102
        ldal    $010203

        lda     $01
        lda     $0102
        ldal    $010203

        lda     $01
        lda     $0102
        ldal    $010203

        bra     :skipdata

; Now hex/decimal/binary, 1-2-3-4 bytes, in data area.
        hex     01010201020301020304
        hex     01010201020301020304
        hex     01010201020301020304

:skipdata

; Convert these to ASCII; requires editing file.  The code generator
; should display some of these as hex.
        lda     #$68
        lda     $68
        lda:    $0068
        ldal    $000068

        lda     #$1f
        lda     #$20
        lda     #$22
        lda     #$27
        lda     #$7e
        lda     #$7f
        lda     #$80
        lda     #$9f
        lda     #$a0
        lda     #$a2
        lda     #$a7
        lda     #$fe
        lda     #$ff

; Switch to long regs, continue with ASCII.
        rep     #$30
        mx      %00

        lda     #$0068
        lda     #$00c8
        lda     #$6868

        rts

; Continuing with ASCII
:ascii
        dfb     $68
        dfb     $80
        dw      $6868
        dfb     $80


; Format first set as address, second set as symbol.
        dw      :skipdata
        adr     :skipdata
        dfb     >:skipdata,:skipdata    ;format as big-endian address

        dfb     :ascii
        dfb     >:ascii
        dw      :ascii
        adr     :ascii
        dfb     >:ascii,:ascii          ;format as big-endian symbol

