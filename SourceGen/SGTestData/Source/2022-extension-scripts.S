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
