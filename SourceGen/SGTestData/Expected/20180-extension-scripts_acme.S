        !cpu    6510
*       =       $1000
        jsr     PrintInline8String
        !text   "01234567"
        jsr     PrintInlineRev8String
        !text   "76543210"
        jsr     PrintInlineNullString
        !text   "null-term string",$00
        jsr     L1800
        jsr     L184E
        jsr     L1848
        !byte   $00,$01
        !word   data01
        !byte   $00,$02
        !word   data02
        nop
        jsr     L1040
        !byte   $24
L1040   !byte   $a9
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
        !24     $012000
        !word   data02
        !byte   $80
data02  !word   @data03
        !byte   $80
        !xor    $80 {
@data03 !text   "AllEight"
        }

NoCont  pla
        pla
        rts

L1075   jsr     NoCont

        !byte   $00
        !byte   $80

L107A   jsr     NoCont

        !byte   $00
        !byte   $80

Next1   jsr     L1075
        jsr     @L108A
        clc
        jsr     L107A
        rts

@L108A  sec
        jsr     L107A
        clc
        !byte   $00,$02
        !word   data02
        bcc     @L1099
        bcs     @L1099

        !byte   $00
        !byte   $80

@L1099  rts

        !fill   358,$00

        !pseudopc $1800 {
L1800   jsr     PrintInlineNullString
        !byte   $62

        !text   "roken "
        !byte   $01
        }
        !pseudopc $1840 {
        !text   "string"
        !byte   $00
        !byte   $60

L1848   jsr     PrintInlineNullString
        !text   $0a,$00
        rts

L184E   jsr     PrintInlineNullString
        adc     $6e
        !byte   $64
        }
