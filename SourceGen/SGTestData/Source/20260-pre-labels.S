; Copyright 2021 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Test address region pre-labels.
;
; Assembler: 64tass
; % tass64 --ascii --case-sensitive --nostart 20260-nested-regions.S

        .cpu    "6502"
*       =       $1000

; EDIT: create project symbol with same name as pre-label (pre-label should win)
; FILE-EDIT: change pre-label to match user label

; EDIT: create explicit 2-byte NON_ADDR region here.
START
        .word   $1000


        .logical $1000
; EDIT: create floating address region with pre-label here ($1000)
;  (label should not appear because parent is non-addr)
begin   bit     begin
        nop
        nop
        nop
        jmp     part2
        .here

; EDIT: create fixed address region with pre-label here ($2000)
;  (label should not appear because parent is non-addr)
        .logical $2000
part2   bit     part2
        nop
        lda     _local1
        lda     _local2
        lda     _local4     ;this should force global conv
; (don't do _local3 here, or it gets promoted to global and interferes
;  with local access to both _local3 and _local 4 later on)
        nop
        nop
        nop
        bit     _b4_part3
        jmp     _part3

_local1 .byte   $81         ;data item with local label
_local2 .byte   $82         ;data item with local label

; EDIT: create floating address region with pre-label here ($3000)
_b4_part3
        .logical $3000
_part3  bit     _part3      ;NOTE: label must be declared local
        nop
        lda     _local2     ;this should force global conv
        lda     _local3
        lda     _local4
        nop
        nop
        nop
        bit     b4_part4
        jmp     part4

_local3 .byte   $83         ;data item with local label
_local4 .byte   $84         ;data item with local label

; the $2000 range ends here; the $3000 floater also ends here
        .here
        .here

; Stack up multiple pre-labels at same offset.


; EDIT: create fixed address region with pre-label here ($4000)
;  (label should not appear)
b4_part4b                   ;(not accessible)
        .logical $4000
; EDIT: create fixed address region with pre-label here ($5000)
b4_part4a
        .logical $5000
; EDIT: create fixed address region with pre-label here ($6000)
b4_part4
        .logical $6000
part4   bit     part4
        bit     b4_part4
        nop
        nop
        nop
        jmp     part4a
        .here
part4a  bit     part4a
        bit     b4_part4a
        jsr     part4b          ;JSR to test code analysis halt
        .here
        brk                     ;shouldn't reach here
part4b  bit     part4b
        bit     b4_part4b
        jmp     end
        .here



; EDIT: created fixed 3-byte address region
        .logical $f000
end
        nop
        bne     past            ;BNE operand should be shown as hex value

; EDIT: put actual end here, before the nop
past    nop
        .here
        brk
