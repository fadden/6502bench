; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; These tests are for exercising UI edge cases.  They're not necessarily
; meaningful for the generate/assemble regression test.
;
; Assembler: Merlin 32

        ORG     $2000

        jmp     skip

        asc     'hello, '
        ORG     $200a           ;EDIT: add this no-op ORG statement
        asc     'world'         ;(string finder should split the string)
        dfb     $80

        org     $2100           ;EDIT: add this
skip
        lda     #$00
        sta     addr0           ;TEST: edit this operand ("addr1-1")
        sta     addr1
        sta     addr2
        jmp     done

L1      asc     'testing stuff'
addr0   asc     '.'
addr1   asc     '!'             ;EDIT: place label here
addr2   asc     '?'
        asc     '---'


done    rts
