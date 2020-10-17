; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
; 6502 version

        dw      $1000       ;PRG-style header

        org     $1000
        jsr     one
        jsr     three_p     ;should land inside one
        jmp     nextthing

        org     $1100
one     bit     one
one_p   lda     #$11
        ldx     #$11
        ldy     #$11
        clv
        bvc     one_p

        org     $1100
two     bit     two
        lda     #$22
two_p   ldx     #$22
        ldy     #$22
        jmp     two_p

        org     $1100
three   bit     three
        lda     #$33
        ldx     #$33
three_p ldy     #$33
        sec
        bcs     three_p


        org     $2000
nextthing
        bit     nextthing
        beq     :fwd-8          ;should just appear as hex since it's outside
        bne     :fwd            ;BNE across org segments (always)

        org     $2020
:fwd    bit     :fwd
        beq     offend          ;branch off the end of the address area into dead space
        bne     endcheck        ; (which wouldn't be dead without the org)
        nop
offend

        org     $2080
endcheck
        bit     endcheck
        lda     offend-1        ;touch bytes at the ends, and one byte before/after
        jsr     offend-1
        lda     offend
        jsr     offend
        lda     endcheck-1
        jsr     endcheck-1
        lda     endcheck
        jsr     endcheck

        lda     $00
        beq     :midinst
        dfb     $ad             ;LDA abs
        org     $2100
:midinst dfb     $ea,$ea

        jmp     pastdata

        org     $2800
        dw      *
        ds      16              ;EDIT: put an org change in the middle
        org     $2820
        ds      16

        org     $3000
pastdata
        bit     pastdata
        lda     #$44
        ldx     #$44
        ldy     #$44
        jmp     :fwd
        dfb     $00             ;put user label here or next inst
:datend dfb     $01

        org     $3100
        dfb     $02             ;data target should NOT get merged with previous user label
:fwd
        bit     :fwd
        lda     :datend-1
        lda     :datend
        lda     :datend+1
        lda     :datend+2
        lda     :fwd-1
        beq     :more

        dfb     $ea,$ea         ;EDIT: mark as inline data

        org     $3180
        dfb     $00,$01
:more   bit     :more

; xref edge case test: make sure adjustment shown is based on address
        lda     label1
        lda     label2          ;EDIT: set operand to sym=label1
        lda     label3          ;EDIT: set operand to sym=label1
        clv
        bvc     label3
label1  nop
label2  nop
        org     $3200
label3  bit     label3

        dfb     $00,$01         ;EDIT: mark as inline data to test execution off end

; add nothing here -- execution should run off end
