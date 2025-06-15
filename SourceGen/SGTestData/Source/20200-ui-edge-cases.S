; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; These tests are for exercising UI edge cases.  They're not necessarily
; meaningful for the generate/assemble regression test.
;
; Assembler: Merlin 32

; EDIT: disable "find nearby targets"

        ORG     $2000

        jmp     skip

        asc     'hello, '
        ORG     $200a           ;EDIT: add this no-op ORG statement
        asc     'world'         ;(string finder should split the string)
        dfb     $80

        org     $2100           ;EDIT: add this
skip
        lda     #$00
        sta     addr0           ;EDIT: set label to "addr1"
        sta     addr1
        sta     addr2           ;EDIT: set label to "addr1"
        jmp     next1

        asc     'testing stuff'
addr0   asc     '.'
addr1   asc     '!'             ;EDIT: set label "addr1"
addr2   asc     '?'
        asc     '---'


; Exercise the various paths for "edit operand target label".
; EDIT: add a project symbol FOO, value $F000, width 16
; EDIT: add a project symbol FOO_5, value $F005 (overlaps w/FOO)
; EDIT: add a project symbol FOOZP, value $F0, width 8
; EDIT: add a project symbol OVERL, value $3000 (overlaps w/platform)
;       and add TestData/TestSyms.sym65 from runtime
FOO     equ     $f000
FOO_5   equ     $f005
FOOZP   equ     $f0
OVERL   equ     $3000

; Define a slice of zero page inside the program.
        org     $f4             ;EDIT: add this
zf4     ds      4               ;EDIT: add label "zf4"

        org     $2200           ;EDIT: add this

; EDIT: format block as zero-terminated strings
line1   asc     'this is line1',00  ;EDIT: label "line1"
line2   asc     'this is line2',00  ;EDIT: label "line2"
line3   asc     'this is line3',00  ;do not label (should get auto-label)
line4   asc     'this is line4',00  ;do not label (should not get autoed)
        dfb     $80

next1
        lda     addr1           ;internal addr
        sta     FOO             ;external addr
        sta     FOO_5           ;external addr (overlaps w/FOO)
        sta     FOO+8           ;external addr (range)
        ldx     OVERL           ;external addr, plat and proj
        ldx     line1           ;internal addr
        ldx     line2           ;internal addr
        ldx     line3           ;internal addr with offset, line start
        ldx     line3+1         ;internal addr with offset, inside line
        ldx     line4+7         ;EDIT: set operand symbol "line2"

; EDIT: add local variable entry "foo_f0", value $f0, width 2
; EDIT: add local variable entry "foo_f4", value $f4, width 2
        ldy     $00             ;no match
        ldy     FOOZP           ;can be either LVT or project
        ldy     FOOZP+1         ;can be either LVT or project
        ldy     FOOZP+2         ;can only be project
        ldy     FOOZP+4         ;can be either LVT or internal
        ldy     FOOZP+6         ;can be project or internal

        lda     #$00
        nop
        bcc     bitsy+1         ;EDIT: set symbol "bitsy"
bitsy   bit     $ffa9           ;EDIT: set label "bitsy" on BIT instruction

        jmp     done

; EDIT: format as 16-bit addresses
        dw      FOO
        dw      FOO_5
        dw      FOO+9
        dw      OVERL
        dw      line1
        dw      line2
        dw      line3
        dw      line4+5         ;EDIT: set operand symbol "line2"
        dw      zf4
        dfb     $80

done    rts
