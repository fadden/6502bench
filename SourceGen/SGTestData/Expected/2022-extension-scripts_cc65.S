         .setcpu "65816"
PrintInlineL1String = $011000
PrintInlineL2String = $012000
PrintInlineDciString = $013000

;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$30
         jsr     PrintInline8String
         .byte   "01234567"
         jsr     PrintInlineRev8String
         .byte   "76543210"
         jsr     PrintInlineNullString
         .asciiz "null-term string"
         jsl     PrintInlineL1String
         .byte   $14,"string with length/1"
         jsl     PrintInlineL2String
         .byte   $14,$00,"string with length/2"
         jsl     PrintInlineDciString
         .byte   "DCI strin",$e7
         jsr     L107C
         jsr     L110F
         jsr     L1108
         rts

PrintInline8String: rts

PrintInlineRev8String: rts

PrintInlineNullString: rts

L107C:   jsr     PrintInlineNullString
         per     $7ff4
         rtl

         .byte   $65
         .byte   $6e
         .byte   $20
         .byte   $01
;         .segment "SEG001"
         .org    $1100
         .byte   "string"
         .byte   $00
         .byte   $60

L1108:   jsl     PrintInlineL2String
         asl     A
         brk

         .byte   $60

L110F:   jsr     PrintInlineNullString
         adc     $6e
         .byte   $64
