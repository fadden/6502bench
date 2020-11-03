; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; EDIT: set up a pair of MULTI_MASKs for 00/01 and e0/e1

addr_00 equ     $00c030
addr_01 equ     $01c030
addr_02 equ     $02c030
addr_03 equ     $03c030

addr_e0 equ     $e0c030
addr_e1 equ     $e1c030
addr_e2 equ     $e2c030
addr_e3 equ     $e3c030

oth_00  equ     $001234
oth_01  equ     $011234
oth_02  equ     $021234

        org     $1000

        clc
        xce
        sep     #$30
        mx      %11

; bank 00
        phk
        plb
        lda:    addr_00
        ldal    addr_01
        ldal    addr_02
        ldal    addr_03
        ldal    addr_e0
        ldal    addr_e1
        ldal    addr_e2
        ldal    addr_e3
        lda:    oth_00
        ldal    oth_01
        ldal    oth_02

; bank 01
        lda     #$01
        pha
        plb
        ldal    addr_00
        lda:    addr_01
        ldal    addr_02
        ldal    addr_03
        ldal    addr_e0
        ldal    addr_e1
        ldal    addr_e2
        ldal    addr_e3
        ldal    oth_00
        lda:    oth_01
        ldal    oth_02

; bank 02
        lda     #$02
        pha
        plb
        ldal    addr_00
        ldal    addr_01
        lda:    addr_02
        ldal    addr_03
        ldal    addr_e0
        ldal    addr_e1
        ldal    addr_e2
        ldal    addr_e3
        ldal    oth_00
        ldal    oth_01
        lda:    oth_02

; bank 03
        lda     #$03
        pha
        plb
        ldal    addr_00
        ldal    addr_01
        ldal    addr_02
        lda:    addr_03
        ldal    addr_e0
        ldal    addr_e1
        ldal    addr_e2
        ldal    addr_e3
        ldal    oth_00
        ldal    oth_01
        ldal    oth_02

; bank e0
        lda     #$e0
        pha
        plb
        ldal    addr_00
        ldal    addr_01
        ldal    addr_02
        ldal    addr_03
        lda:    addr_e0
        ldal    addr_e1
        ldal    addr_e2
        ldal    addr_e3

; bank e1
        lda     #$e1
        pha
        plb
        ldal    addr_00
        ldal    addr_01
        ldal    addr_02
        ldal    addr_03
        ldal    addr_e0
        lda:    addr_e1
        ldal    addr_e2
        ldal    addr_e3

END     rts
