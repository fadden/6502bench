        !cpu    65816
PrintInlineL1String = $011000
PrintInlineL2String = $012000
PrintInlineDciString = $013000

*       =       $1000
        !as
        !rs
        clc
        xce
        sep     #$30
        jsr     PrintInline8String
        !text   "01234567"
        jsr     PrintInlineRev8String
        !text   "76543210"
        jsr     PrintInlineNullString
        !text   "null-term string",$00
        jsl     PrintInlineL1String
        !text   $14,"string with length/1"
        jsl     PrintInlineL2String
        !text   $14,$00,"string with length/2"
        jsl     PrintInlineDciString
        !text   "DCI strin",$e7
        jsr     L1800
        jsr     L184F
        jsr     L1848
        !byte   $00,$01
        !word   data01
        !byte   $00,$02
        !word   data02
        nop
        jsr     L1085
        !byte   $24
L1085   !byte   $a9
        !byte   $00
        sta     $ff
        !byte   $ea
        jmp     Next1

PrintInline8String rts

PrintInlineRev8String rts

PrintInlineNullString rts

data01  !word   4386
        !byte   $33,$44
        !32     $88776655
        !byte   $99,$88,$77,$66
        !byte   'f'
        !byte   'F' | $80
        !byte   $40
        !byte   $c1
        !byte   $42
        !byte   $c3
        !byte   $44
        !byte   $c5
        !byte   $46
        !byte   $c7
        !24     PrintInlineL2String
        !word   data02
        !byte   $80
data02  !word   @data03
        !byte   $80
        !xor    $80 {
@data03 !text   "AllEight"
        }

NoCont  pla                ;split across address change
        pla
        rts

L10BA   jsr     NoCont

        !byte   $00
        !byte   $80

L10BF   jsr     NoCont

        !byte   $00
        !byte   $80

Next1   jsr     L10BA
        jsr     @L10CF
        clc
        jsr     L10BF
        rts

@L10CF  sec
        jsr     L10BF
        rts

        !fill   300,$00

        !pseudopc $1800 {
L1800   jsr     PrintInlineNullString
        per     $8778
        rtl

        !byte   $65
        !byte   $6e
        !byte   $20
        !byte   $01
        } ;!pseudopc
        !pseudopc $1840 {
        !text   "string"
        !byte   $00
        !byte   $60

L1848   jsl     PrintInlineL2String
        asl
        !byte   $00,$60

L184F   jsr     PrintInlineNullString
        adc     $6e
        !byte   $64
        } ;!pseudopc
