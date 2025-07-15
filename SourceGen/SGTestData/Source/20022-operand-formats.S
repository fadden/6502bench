; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Extended tests for 65816.
;
; Assembler: Merlin 32

        org     $1000

; 65816 with long regs
        clc
        xce
        rep     #$30
        mx      %00

; hex/dec/sdec/bin
        lda     #$1234
        lda     #$1234
        lda     #$1234
        lda     #$1234

        lda     #$fffe
        lda     #$fffe
        lda     #$fffe
        lda     #$fffe

        ldal    $fffefd
        ldal    $fffefd
        ldal    $fffefd
        ldal    $fffefd

; ASCII
        lda     #$0068      ;'h'
        lda     #$00c8      ;"h"
        lda     #$6868

        ldal    $000068     ;'h'
        ldal    $0000c8     ;"h"

; Signed-decimal tests.
        nop
        lda     #$0000
        lda     #$0001
        lda     #$7fff
        lda     #$8000
        lda     #$ffff

        ldal    $fffefd

        rts
