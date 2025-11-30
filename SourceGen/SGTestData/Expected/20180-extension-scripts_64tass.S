        .cpu    "6502i"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
bword   .macro
        .byte   (\1)>>8,(\1)&$ff
        .endmacro
*       =       $1000
        jsr     PrintInline8String
        .text   "01234567"
        jsr     PrintInlineRev8String
        .text   "76543210"
        jsr     PrintInlineNullString
        .null   "null-term string"
        jsr     L1800
        jsr     L184E
        jsr     L1848
        brk     #$01
        .word   data01
        brk     #$02
        .word   data02
        nop
        jsr     L1040
        .byte   $24
L1040   .byte   $a9
        .byte   $00
        sta     $ff
        .byte   $ea
        jmp     Next1

PrintInline8String rts

PrintInlineRev8String rts

PrintInlineNullString rts

data01  .word   4386
        bword   $3344
        .dword  $88776655
        .byte   $99,$88,$77,$66
        .byte   'f'
        .byte   'F' | $80
        .byte   $40
        .byte   $c1
        .byte   $42
        .byte   $c3
        .byte   $44
        .byte   $c5
        .byte   $46
        .byte   $c7
        .long   $012000
        .word   data02
        .byte   $80
data02  .word   _data03
        .byte   $80
        .enc    "sg_hiascii"
_data03 .text   "AllEight"

NoCont  pla
        pla
        rts

L1075   jsr     NoCont

        .byte   $00
        .byte   $80

L107A   jsr     NoCont

        .byte   $00
        .byte   $80

Next1   jsr     L1075
        jsr     _L108A
        clc
        jsr     L107A
        rts

_L108A  sec
        jsr     L107A
        clc
        brk     #$02
        .word   data02
        bcc     _L1099
        bcs     _L1099

        .byte   $00
        .byte   $80

_L1099  rts

        .fill   358,$00

        .logical $1800
L1800   jsr     PrintInlineNullString
        .byte   $62

        .enc    "sg_ascii"
        .text   "roken "
        .byte   $01
        .here
        .logical $1840
        .text   "string"
        .byte   $00
        .byte   $60

L1848   jsr     PrintInlineNullString
        .null   $0a
        rts

L184E   jsr     PrintInlineNullString
        adc     $6e
        .byte   $64
        .here
