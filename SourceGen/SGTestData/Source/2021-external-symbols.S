; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; EDIT: the project must include the three platform symbol files.

        org     $1000

; make sure platform symbols don't get set for file data
; DO NOT set a user label here
Start   lda     Start-1     ;PlatStart-1
        ldx     Start       ;auto
        ldy     Start+1     ;auto+1

        nop

; test overlapping labels (multiple sym files)
        lda     $1ffe       ;should have no symbol
        lda     $1fff       ;should be SameName1-1
        lda     $2000       ;should be SameName1

        lda     $200f       ;should have no symbol
        lda     $2010       ;should be SameName2-1
        lda     $2011       ;should be SameName2

        lda     $201f       ;should have no symbol
        lda     $2020       ;should have no symbol
        lda     $2021       ;should be sym-1
        lda     $2022       ;should be SameName3

        nop

; test overlapping values (multiple sym files)
        lda     $2100       ;should be SameValA_A
        lda     $2110       ;should be SameValB_A
        lda     $2120       ;should be SameValC_C

        nop

; test overlap with project symbol
; EDIT: define project symbols ProjSym1 at $2202(4b) and ProjSim2 at $220a(1b)
        lda     $21fe       ;should have no symbol
        lda     $21ff       ;SYM-1
        lda     $2200       ;ChkProj1
        lda     $2201       ;ChkProj1+1
        lda     $2202       ;ProjSym
        lda     $2203       ;ProjSym1+1
        lda     $2204       ;ProjSym1+2
        lda     $2205       ;ProjSym1+3
        lda     $2206       ;ChkProj2+2
        lda     $2207       ;ChkProj2+3
        lda     $2208       ;should have no symbol
        lda     $2209       ;ProjSym2-1
        lda     $220a       ;ProjSym2
        lda     $220b       ;should have no symbol

        nop

; test overlapping regions, single file
        lda     $2ffe       ;should have no symbol
        lda     $2fff       ;Over1-1
        lda     $3000       ;Over1
        lda     $3001       ;Over1+1
        lda     $3002       ;Over2
        lda     $3003       ;Over2+1
        lda     $3004       ;Over2+2
        lda     $3005       ;Over2+3
        lda     $3006       ;Over2a
        lda     $3007       ;Over3+1
        lda     $3008       ;Over3+2
        lda     $3009       ;Over3+3
        lda     $300a       ;Over3+4
        lda     $300b       ;Over3+5
        lda     $300c       ;Over3+6
        lda     $300d       ;Over1+13
        lda     $300e       ;Over1+14
        lda     $300f       ;Over1+15
        lda     $3010       ;should have no symbol

        nop

; test overlapping regions, multiple platform files
        lda     $30fe       ;should have no symbol
        lda     $30ff       ;SepOver1-1
        lda     $3100       ;SepOver1
        lda     $3101       ;SepOver1+1
        lda     $3102       ;SepOver1+2
        lda     $3103       ;SepOver1+3
        lda     $3104       ;SepOver2+2
        lda     $3105       ;SepOver2+3
        lda     $3106       ;should have no symbol

        nop

; test overlap with local variable
; EDIT: create variable LocalVar at $41(2b)
        ldx     $3e         ;should have no symbol
        ldx     $3f         ;should be OverVar-1
        ldx     $40         ;should be OverVar
        ldx     $41         ;should be LocalVar
        ldx     $42         ;should be LocalVar+1
        ldx     $43         ;should be OverVar+3
        ldx     $44         ;should have no symbol

        nop

        lda     $3fff       ;EDIT: change to "FatConst"
        lda     $4000       ;should have no symbol
        lda     $4001       ;should have no symbol

        rts
