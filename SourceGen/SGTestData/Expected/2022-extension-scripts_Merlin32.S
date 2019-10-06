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
         jsr   L107C
         jsr   L110F
         jsr   L1108
         rts

PrintInline8String rts

PrintInlineRev8String rts

PrintInlineNullString rts

L107C    jsr   PrintInlineNullString
         per   $7ff4
         rtl

         dfb   $65
         dfb   $6e
         dfb   $20
         dfb   $01
         org   $1100
         asc   'string'
         dfb   $00
         dfb   $60

L1108    jsl   PrintInlineL2String
         asl   A
         brk

         dfb   $60

L110F    jsr   PrintInlineNullString
         adc   $6e
         dfb   $64
