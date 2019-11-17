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
         jsr     L10B5
         jsr     L110F
         jsr     L1108
         brk     $01
         .word   data01
         brk     $02
         .word   data02
         nop
         jsr     L1085
         .byte   $24
L1085:   .byte   $a9
         .byte   $00
         sta     $ff
         .byte   $ea
         rts

PrintInline8String: rts

PrintInlineRev8String: rts

PrintInlineNullString: rts

data01:  .word   4386
         .dbyt   $3344
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
         .faraddr PrintInlineL2String
         .word   data02
         .byte   $80
data02:  .word   @data03
         .byte   $80
.macro   HiAscii Arg
         .repeat .strlen(Arg), I
         .byte   .strat(Arg, I) | $80
         .endrep
.endmacro
@data03: HiAscii "AllEight"

L10B5:   jsr     PrintInlineNullString ;split across address change
         per     $802d
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
         brk     $60

L110F:   jsr     PrintInlineNullString
         adc     $6e
         .byte   $64
