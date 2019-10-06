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
        jsr     Print8String
        asc     '01234567'
        jsr     PrintRev8String
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

        rts

PrintInline8String    rts               ;EDIT: set label
PrintInlineRev8String rts               ;EDIT: set label
PrintInlineNullString rts               ;EDIT: set label


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
