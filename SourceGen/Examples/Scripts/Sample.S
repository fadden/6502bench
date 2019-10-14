        org     $1000

        jsr     PrintInlineL1String
        str     'How long?'

        jsr     PrintInlineZString1
        asc     'Test one',00

        jsr     PrintInlineZString2
        asc     'Test two',00
        rts

PrintInlineL1String
        rts
PrintInlineZString1
        rts
PrintInlineZString2
        rts
