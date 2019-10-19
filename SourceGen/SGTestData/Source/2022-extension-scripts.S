; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; EDIT: the project must include the platform symbol file and extension script

PrintInlineL1String  equ $011000
PrintInlineL2String  equ $012000
PrintInlineDciString equ $013000        ;EDIT: add to project symbols

        org     $1000

        clc
        xce
        sep     #$30
        mx      %11

; check basics
        jsr     PrintInline8String
        asc     '01234567'
        jsr     PrintInlineRev8String
        asc     '76543210'
        jsr     PrintInlineNullString
        asc     'null-term string',00

        jsl     PrintInlineL1String
        str     'string with length/1'
        jsl     PrintInlineL2String
        strl    'string with length/2'
        jsl     PrintInlineDciString
        dci     'DCI string'

; check errors
        jsr     broken
        jsr     off_end
        jsr     too_long

; check block formatting
        brk     $01
        dw      data01

        brk     $02
        dw      data02

; Handle an edge case where the inline formatting gets thrown out.
; Two paths: BIT $A9 / BRK $85 / inline $FF/EA goes first, then
; LDA $00 / STA $FF / NOP goes.  When we get to the STA we notice
; that it's marked as inline data, so we remove it from $85 $ff
; but not from $ea.
;
; If we try to walk through the file, advancing offset by the anattrib
; length, we will traverse the first path, which (with 2-byte BRKs)
; runs into the $FF, which is marked as an instruction but not an
; instruction start.
;
; Switching to 1-byte BRKs makes the $85 an inline data item rather
; than an instruction.  When we come back through, we LDA $00 and
; then skip over the next 3 bytes.  No conflict.
        nop
        jsr     edge1                   ;alt path, evaluated later
        dfb     $24                     ;1: BIT dp
edge1   dfb     $a9                     ;2: LDA imm
        brk                             ;1: BRK <op>
        dfb     $85                     ;2: STA imm
        dfb     $ff                     ;1: address $eaff
        nop                             ;2:

        rts

PrintInline8String    rts               ;EDIT: set label
PrintInlineRev8String rts               ;EDIT: set label
PrintInlineNullString rts               ;EDIT: set label

data01                                  ;EDIT: set label
        dw      $1122                   ;should be little-endian
        dw      $4433                   ;should be big-endian
        hex     55667788                ;32-bit
        hex     99887766                ;32-bit big-endian
        dfb     'f'                     ;ASCII
        dfb     "F"                     ;high ASCII
        hex     40C142C344C546C7        ;bad DCI string
        hex     002001                  ;24-bit addr
        dw      data02                  ;by symbol

        dfb     $80

data02                                  ;EDIT: set label, must be "data02"
        dw      data03
        dfb     $80

data03                                  ;EDIT: set label
        asc     "AllEight"


; check address split across string
broken  jsr     PrintInlineNullString
        asc     'broken ',01
        org     $1100                   ;EDIT: split address
        asc     'string',00
        rts

too_long jsl    PrintInlineL2String
        dw      END-*+1                 ;should be 1 byte over; want it to be rejected by
        rts                             ; function in .cs (otherwise code overlap race)

; MUST be last
off_end jsr PrintInlineNullString
nonterm asc     'end'

END     equ     *
