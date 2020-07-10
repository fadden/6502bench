; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
;
; "Smart PLB" should be enabled.

        org     $1000

start
        clc
        xce
        sep     #$30
        mx      %11

        lda     start               ;initial offset has B=K
        lda     bank2               ;(should be hex)

        phk
        plb                         ;smart PLB should set B=K
        lda     start
        lda     bank2               ;(should be hex)

        lda     #$02
        pha
        plb                         ;smart PLB should set B=$02
        lda     start               ;(should be hex)
        lda     bank2

        ldx     #$08
        tax
        pha
        plb                         ;EDIT: set DBR=$08
        jsl     bank2
        jsl     bank2_redux

        lda     #$02
        pha
        plb                         ;EDIT: set DBR=$02
        jsl     bank3

        phk                         ;smart PLB should set B=K
        plb
        bit     start

        jml     alldone

code2bz
        dw      snippet             ;EDIT: format as 16-bit address
snippet rts                         ;EDIT: code hint

code2zero
        adr     code2b              ;EDIT: format as 24-bit address



        org     $022000
bank2
        ldal    bank2               ;EDIT: set DBR=$08
        lda     bank2               ;should become hex
        ldx     #$00
        jsr     code2a
        jsr     :call2bz
        jsr     :call2zero
        nop

        lda     #$03
        pha
        plb                         ;smart PLB should set B=$03
        lda     bank3dat

; try to fake out the "smart PLB" code
        rep     #$30
        mx      %00
        lda     #$00a9
        pha
        plb
        plb
        sep     #$30
        mx      %11

        rtl

:call2bz
        jmp     (code2bz)           ;source/target in bank 0
:call2zero
        jmp     [code2zero]         ;source in bank 0

code2a
        jmp     (bank2addr,x)       ;both addresses formed with K
code2b
        nop                         ;EDIT: code hint
        rts

bank2addr
        dw      code2b


        org     $033000
bank3
        ldal    bank3               ;EDIT: set DBR=$02
        lda     bank2
        jsr     :post_plb
        beq     :skip_mayhem
; Test PHK/PLB with an address shift in the middle.  Not really something
; that should ever happen, but we need to handle everything.
        phk
        org     $033020             ;EDIT: set address
:post_plb
        plb
:self   nop
        bra     :skip_mayhem

:skip_mayhem
        lda     bank2addr
        rtl

bank3dat
        dw      bank3dat            ;EDIT: format as address


        org     $024000
bank2_redux
        ldal    bank2_redux
        phb
        phk
        plb
        lda     bank2
        lda     bank3               ;should be hex
        plb
        rtl


alldone rts
