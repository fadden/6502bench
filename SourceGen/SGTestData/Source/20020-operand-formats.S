; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Basic tests for 6502.
;
; Assembler: Merlin 32

        org     $1000

; Basic operand formats.  Show first set as hex, second as decimal,
; third as sign-extended decimal, fourth as binary.
        lda     $01
        lda     $0102
        lda     $fe
        lda     $feff

        lda     $01
        lda     $0102
        lda     $fe
        lda     $feff

        lda     $01
        lda     $0102
        lda     $fe
        lda     $feff

        lda     $01
        lda     $0102
        lda     $fe
        lda     $feff

        jmp     :skipdata

; Now hex/decimal/sdec/binary, each with .dd1/.dd2/.dd3/.dd4
        hex     01010201020301020304
        hex     01010201020301020304
        hex     01010201020301020304
        hex     01010201020301020304

; bonus round for sdec
        hex     fffffefffefdfffefdfc

:skipdata

; Convert these to ASCII; requires editing file.  The code generator
; should display some of these as hex.
        lda     #$68
        lda     $68
        lda:    $0068

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

        jmp     next1

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
        adrl    :ascii
        dfb     >:ascii,:ascii          ;format as big-endian symbol

; Merlin 1.0 has trouble with "DFB '{'".  Test it and the neighbors.
        dfb     '['
        dfb     $7b
        dfb     $7c
        dfb     $7d
        dfb     ','
        dfb     "["
        dfb     $fb
        dfb     $fc
        dfb     $fd
        dfb     ","

; Signed-decimal tests.  All operands should be formatted as signed.  This
; will require hand-editing or tweaking the operand editor.
next1   nop
        lda     #$00                    ;these should format as signed
        lda     #$01
        lda     #$7f
        lda     #$80
        lda     #$ff

        lda     $01                     ;these should appear unsigned
        lda     $fe
        lda:    $0001
        lda     $fffe
        jmp     :skipdat1

        dfb     $00                     ;all of these should appear signed
        dfb     $01
        dfb     $7f
        dfb     $80
        dfb     $ff
        dw      $0001
        dw      $fffe
        dw      $0200                   ;big-endian
        dw      $fdff                   ;big-endian
        adr     $010203
        adr     $fffefd
        adrl    $01020304
        adrl    $fffefdfc
:skipdat1

        rts
