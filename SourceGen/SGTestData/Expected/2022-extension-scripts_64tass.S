        .cpu    "65816"
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
        jsr     L107C
        jsr     L110F
        jsr     L1108
        rts

PrintInline8String rts

PrintInlineRev8String rts

PrintInlineNullString rts

L107C   jsr     PrintInlineNullString
        per     $7ff4
        rtl

        .byte   $65
        .byte   $6e
        .byte   $20
        .byte   $01
        .logical $1100
        .text   "string"
        .byte   $00
        .byte   $60

L1108   jsl     PrintInlineL2String
        asl     a
        brk

        .byte   $60

L110F   jsr     PrintInlineNullString
        adc     $6e
        .byte   $64
        .here
