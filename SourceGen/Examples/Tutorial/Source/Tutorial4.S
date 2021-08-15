; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

        jsr     PrintInlineL1String
        str     'How long?'

        jsr     PrintInlineZString1
        asc     'Test one',00

        jsr     PrintInlineZString2
        asc     'Test two',00

        jsr     PrintInlineAddr
        dw      test_three
        rts

test_three asc  'Test three',00

PrintInlineL1String
        rts
PrintInlineZString1
        rts
PrintInlineZString2
        rts
PrintInlineAddr
        rts
