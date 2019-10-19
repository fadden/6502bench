        .cpu    "65816"
        .enc    sg_hiascii
        .cdef   $20,$7e,$a0
        .enc    sg_ascii
        .cdef   $20,$7e,$20
PrintInlineL1String = $011000
PrintInlineL2String = $012000
PrintInlineDciString = $013000

*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$30
        jsr     PrintInline8String
        .text   "01234567"
        jsr     PrintInlineRev8String
        .text   "76543210"
        jsr     PrintInlineNullString
        .null   "null-term string"
        jsl     PrintInlineL1String
        .ptext  "string with length/1"
        jsl     PrintInlineL2String
        .text   $14,$00,"string with length/2"
        jsl     PrintInlineDciString
        .shift  "DCI string"
        jsr     L10B5
        jsr     L110F
        jsr     L1108
        brk     #$01
        .word   data01
        brk     #$02
        .word   data02
        nop
        jsr     L1085
        .byte   $24
L1085   .byte   $a9
        .byte   $00
        sta     $ff
        .byte   $ea
        rts

PrintInline8String rts

PrintInlineRev8String rts

PrintInlineNullString rts

data01  .word   4386
        .byte   $33,$44
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
        .long   PrintInlineL2String
        .word   data02
        .byte   $80
data02  .word   data03
        .byte   $80
        .enc    sg_hiascii
data03  .text   "AllEight"

L10B5   jsr     PrintInlineNullString ;split across address change
        per     $802d
        rtl

        .byte   $65
        .byte   $6e
        .byte   $20
        .byte   $01
        .logical $1100
        .enc    sg_ascii
        .text   "string"
        .byte   $00
        .byte   $60

L1108   jsl     PrintInlineL2String
        asl     a
        brk     #$60

L110F   jsr     PrintInlineNullString
        adc     $6e
        .byte   $64
        .here
