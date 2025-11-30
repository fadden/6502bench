         org   $1000
         jsr   PrintInline8String
         asc   '01234567'
         jsr   PrintInlineRev8String
         rev   '01234567'
         jsr   PrintInlineNullString
         asc   'null-term string',00
         jsr   L1800
         jsr   L184E
         jsr   L1848
         brk   $01
         dw    data01
         brk   $02
         dw    data02
         nop
         jsr   L1040
         dfb   $24
L1040    dfb   $a9
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
         adr   $012000
         dw    data02
         dfb   $80
data02   dw    :data03
         dfb   $80
:data03  asc   "AllEight"

NoCont   pla
         pla
         rts

L1075    jsr   NoCont

         dfb   $00
         dfb   $80

L107A    jsr   NoCont

         dfb   $00
         dfb   $80

Next1    jsr   L1075
         jsr   :L108A
         clc
         jsr   L107A
         rts

:L108A   sec
         jsr   L107A
         clc
         brk   $02
         dw    data02
         bcc   :L1099
         bcs   :L1099

         dfb   $00
         dfb   $80

:L1099   rts

         ds    358

         org   $1800
L1800    jsr   PrintInlineNullString
         dfb   $62

         asc   'roken '
         dfb   $01
         org   $1840
         asc   'string'
         dfb   $00
         dfb   $60

L1848    jsr   PrintInlineNullString
         asc   0a00
         rts

L184E    jsr   PrintInlineNullString
         adc   $6e
         dfb   $64
