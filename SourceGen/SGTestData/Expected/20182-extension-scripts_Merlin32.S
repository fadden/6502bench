PrintInlineL1String equ $011000
PrintInlineL2String equ $012000
PrintInlineDciString equ $013000

         org   $1000
         clc
         xce
         sep   #$30
         jsr   PrintInline8String
         asc   '01234567'
         jsr   PrintInlineRev8String
         rev   '01234567'
         jsr   PrintInlineNullString
         asc   'null-term string',00
         jsl   PrintInlineL1String
         str   'string with length/1'
         jsl   PrintInlineL2String
         strl  'string with length/2'
         jsl   PrintInlineDciString
         dci   'DCI string'
         jsr   L1800
         jsr   L184F
         jsr   L1848
         brk   $01
         dw    data01
         brk   $02
         dw    data02
         nop
         jsr   L1085
         dfb   $24
L1085    dfb   $a9
         dfb   $00
         sta   $ff
         dfb   $ea
         jmp   Next1

PrintInline8String rts

PrintInlineRev8String rts

PrintInlineNullString rts

data01   dw    4386
         ddb   $3344
         adrl  $88776655
         dfb   $99,$88,$77,$66
         dfb   'f'
         dfb   "F"
         dfb   $40
         dfb   $c1
         dfb   $42
         dfb   $c3
         dfb   $44
         dfb   $c5
         dfb   $46
         dfb   $c7
         adr   PrintInlineL2String
         dw    data02
         dfb   $80
data02   dw    :data03
         dfb   $80
:data03  asc   "AllEight"

NoCont   pla              ;split across address change
         pla
         rts

L10BA    jsr   NoCont

         dfb   $00
         dfb   $80

L10BF    jsr   NoCont

         dfb   $00
         dfb   $80

Next1    jsr   L10BA
         jsr   :L10CF
         clc
         jsr   L10BF
         rts

:L10CF   sec
         jsr   L10BF
         rts

         ds    300

         org   $1800
L1800    jsr   PrintInlineNullString
         per   $8778
         rtl

         dfb   $65
         dfb   $6e
         dfb   $20
         dfb   $01
         org   $1840
         asc   'string'
         dfb   $00
         dfb   $60

L1848    jsl   PrintInlineL2String
         asl   A
         brk   $60

L184F    jsr   PrintInlineNullString
         adc   $6e
         dfb   $64
