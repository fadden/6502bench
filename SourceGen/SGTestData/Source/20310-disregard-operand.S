; Copyright 2025 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Tests the "disregard operand address" feature.
;
; Assembler: Merlin 32
;

        org     $2b00

; Reference all code entry points.

        jsr     site1a
        jsr     site1b
        jsr     site1c
        jsr     site2a
        jsr     site2b
        jsr     site3a
        jsr     site3b
        jsr     site3c
        jsr     site3d
        jsr     cmdd
        jsr     cmdb
        jsr     site4a
        jsr     site4b
        jmp     done

        ds      \,$2b       ;pad to leave room for more

; Classic $C3xx entry

site1a  bit     $ff58       ;set V-flag
        bvs     later       ;in actual ROM, always taken
site1b  sec
        dfb     $90         ;BCC (never taken)  <-- EDIT: disregard operand
site1c  clc
        clv
        bvc     later       ;(always taken)

        dfb     $80
later   rts

; Basic register config.

site2a  lda     #$00
        dfb     $2c         ;BIT (ignored)  <-- EDIT: disregard operand
site2b  lda     #$ff
        sta     out
        rts
out     dfb     $cc

; Multi-nested BIT instructions.

site3a  dfb     $2c         ;BIT (ignored)  <-- EDIT: disregard operand
site3b  dfb     $2c         ;BIT (ignored)  <-- EDIT: disregard operand
site3c  dfb     $2c         ;BIT (ignored)  <-- EDIT: disregard operand
site3d  dfb     $2c         ;BIT (ignored)  <-- EDIT: disregard operand
        dfb     $2c         ;BIT (ignored)  <-- EDIT: disregard operand
        nop
        nop
        rts

; Piece of //c 16KB ROM ($ca79).
cmdd    lda     #$1f        ;mask
        sec                 ;C=1 means high 3 bits
        dfb     $90         ;BCC (never taken)  <-- EDIT: disregard operand (and below)
cmdb    lda     #$f0        ;mask lower 4 bits; also BEQ instruction (never taken)
        clc                 ;F0 will skip this if cmdd
        nop
        bcc     noshift
        asl     A
noshift nop
        rts

site4a  lda     site4b+1    ;EDIT: disregard operand, set operand to symbol
        rts
site4b  lda     site4a      ;EDIT: disregard operand, set operand to symbol
        rts

        ds      \,$2c       ;pad to leave room for more

done    rts
        ds      \,$2d       ;pad to ensure branches land internally
