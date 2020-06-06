; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; EDIT: the project must include the three platform symbol files.

        org     $1000

; make sure platform symbols don't get set for file data
; DO NOT set a user label here
Start   lda     Start-1     ;CodeWrap+255
        ldx     Start       ;(auto)
        ldy     Start+1     ;(auto+1)
        lda     END         ;(auto)
        lda     END+1       ;CodeWrap+X
        lda     END+8       ;CodeWrap+Y

        nop

; test overlapping labels (multiple sym files)
        lda     $1ffe       ;(no sym)
        lda     $1fff       ;should be SameName1-1
        lda     $2000       ;should be SameName1

        lda     $200f       ;(no sym)
        lda     $2010       ;should be SameName2-1
        lda     $2011       ;should be SameName2
        lda     $2012       ;(no sym)

        lda     $201f       ;(no sym)
        lda     $2020       ;(no sym)
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
        lda     $21fe       ;(no sym)
        lda     $21ff       ;SYM-1
        lda     $2200       ;ChkProj1
        lda     $2201       ;ChkProj1+1
        lda     $2202       ;ProjSym
        lda     $2203       ;ProjSym1+1
        lda     $2204       ;ProjSym1+2
        lda     $2205       ;ProjSym1+3
        lda     $2206       ;ChkProj2+2
        lda     $2207       ;ChkProj2+3
        lda     $2208       ;(no sym)
        lda     $2209       ;ProjSym2-1
        lda     $220a       ;ProjSym2
        lda     $220b       ;(no sym)

        nop

; test overlapping regions, single file
        lda     $2ffe       ;(no sym)
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
        lda     $3010       ;(no sym)

        nop

; test overlapping regions, multiple platform files
        lda     $30fe       ;(no sym)
        lda     $30ff       ;SepOver1-1
        lda     $3100       ;SepOver1
        lda     $3101       ;SepOver1+1
        lda     $3102       ;SepOver1+2
        lda     $3103       ;SepOver1+3
        lda     $3104       ;SepOver2+2
        lda     $3105       ;SepOver2+3
        lda     $3106       ;(no sym)

        nop

; test overlap with local variable
; EDIT: create variable LocalVar at $41(2b)
        ldx     $3e         ;(no sym)
        ldx     $3f         ;should be OverVar-1
        ldx     $40         ;should be OverVar
        ldx     $41         ;should be LocalVar
        ldx     $42         ;should be LocalVar+1
        ldx     $43         ;should be OverVar+3
        ldx     $44         ;(no sym)

        nop

        lda     $3fff       ;EDIT: change to "FatConst"
        lda     $4000       ;(no sym)
        lda     $4001       ;(no sym)

; test bank wrap
        lda     $fff8       ;should be BankWrap+8
        lda     $08         ;should be BankWrap+24 or <BankWrap-232

        nop

; test I/O direction
Dir     equ     $5000
        lda     Dir
        lda     Dir+1
        ldx     Dir+2
        ldy     Dir+3
        sta     Dir
        sta     Dir+1
        stx     Dir+2
        sty     Dir+3

        nop

; test MULTI_MASK stuff
        bit     $c000       ;should all be AlsoMultiZero
        bit     $c010       ;<-- except this NonMultiOver
        bit     $c020
        bit     $c0f0
        nop
        bit     $c001       ;should all be MultiOne
        bit     $c011
        bit     $c021
        nop
        bit     $c002       ;should all be hex
        bit     $c012
        bit     $c022
        nop
        lda     $c003
        lda     $c004       ;MultiRead
        lda     $c005       ;MultiRead+1
        lda     $c006       ;MultiRead+2
        lda     $c007
        sta     $c004
        sta     $c005       ;MultiWrite
        sta     $c006       ;MultiWrite+1
        sta     $c007       ;MultiWrite+2
        sta     $c008
        nop
        jsr     $c005       ;MultiRead+1
        nop
        bit     $c100       ;should all be MoreMultiZero
        bit     $c110       ;<-- except this AlsoMoreMultiZero
        bit     $c120
        bit     $c1f0

        nop

; test some more overlapping symbols
        lda     $6000       ;OverA_0
        lda     $6100       ;OverA_1
        lda     $6200       ;OverB_2
        lda     $6304       ;OverB_3 + 4
        lda     $6400       ;OverA_4 - 1
        lda     $6401       ;OverA_4
        lda     $6402       ;OverB_4
        lda     $6403       ;OverC_4

END     rts
