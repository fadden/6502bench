;Projected edited to format non-char operands as chars.
        .cpu    "6502"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
*       =       $1000
        lda     #'A'
        lda     #'A' | $80
        .enc    "none"
        lda     #'A'
        .enc    "screen"
        lda     #'A'
        .enc    "sg_ascii"
        ldx     #'a'
        ldx     #'a' | $80
        .enc    "none"
        ldx     #'a'
        .enc    "screen"
        ldx     #'a'
        .enc    "sg_ascii"
        lda     #$7f       ;inappropriate format should be ignored
        lda     #$7f
        .enc    "none"
        lda     #$7f
        .enc    "screen"
        lda     #$7f
        .enc    "sg_ascii"
        lda     #$0d
        jmp     L144B

        .byte   'C'
        .byte   'C' | $80
        .enc    "none"
        .byte   'C'
        .enc    "screen"
        .byte   'C'
        .enc    "sg_ascii"
        .word   'd'
        .word   'd' | $80
        .enc    "none"
        .word   'd'
        .enc    "screen"
        .word   'd'
        .byte   $00,$45
        .byte   $00,$c5
        .byte   $00,$c5
        .byte   $00,$45
        .byte   $80
        .enc    "sg_ascii"
        .text   "low ASCII str"
        .enc    "sg_hiascii"
        .text   "high ASCII str"
        .byte   $80
        .enc    "none"
        .text   "PETSCII str"
        .byte   $80
        .enc    "screen"
        .text   "Screen Code str"
        .byte   $82
        .enc    "sg_ascii"
        .text   $07,"Low ASCII CRLF",$0d,$0a
        .byte   $82
        .enc    "sg_hiascii"
        .text   $87,"High ASCII CRLF",$8d,$8a
        .byte   $82
        .enc    "none"
        .text   $93,"PETSCII with ",$96,"control",$05," codes",$0d
        .byte   $83
        .enc    "sg_ascii"
        .text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        .text   "XYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
        .byte   $83
        .enc    "sg_hiascii"
        .text   " !",$a2,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        .text   "XYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
        .byte   $83
        .enc    "none"
        .text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        .text   "XYZ["
        .byte   $5c
        .byte   ']'
        .byte   $5e
        .byte   $5f
        .byte   $60
        .text   "abcdefghijklmnopqrstuvwxyz"
        .byte   $7b
        .byte   $7c
        .byte   $7d
        .byte   $7e
        .byte   $83
        .enc    "screen"
        .text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        .text   "XYZ["
        .byte   $1c
        .byte   ']'
        .byte   $1e
        .byte   $1f
        .byte   $40
        .text   "abcdefghijklmnopqrstuvwxyz"
        .byte   $7b
        .byte   $7c
        .byte   $7d
        .byte   $7e
        .byte   $84
        .enc    "none"
        .text   "IICSTEP esrever"
        .byte   $84
        .null   "null term PETSCII"
        .byte   $84
        .text   "This null-terminated string is too long to fit on a single lin"
        .text   "e, and will be split.",$00
        .byte   $84
        .ptext  "PETSCII with length"
        .byte   $84
        .text   $14,$00,"PETSCII with length2"
        .byte   $84
        .shift  "pet dci"
        .byte   $84
        .enc    "screen"
        .text   "edoC neercS esrever"
        .byte   $84
        .text   "null-term screen code?@"
        .byte   $84
        .ptext  "Screen Code with length"
        .byte   $84
        .text   $18,$00,"Screen Code with length2"
        .byte   $84
        .shift  "Screen Code DCI"
        .byte   $84
        .byte   $85
        .byte   $00
        .byte   $01
        .byte   $02
        .byte   $03
        .byte   $04
        .byte   $05
        .byte   $06
        .byte   $07
        .byte   $08
        .byte   $09
        .byte   $0a
        .byte   $0b
        .byte   $0c
        .byte   $0d
        .byte   $0e
        .byte   $0f
        .byte   $10
        .byte   $11
        .byte   $12
        .byte   $13
        .byte   $14
        .byte   $15
        .byte   $16
        .byte   $17
        .byte   $18
        .byte   $19
        .byte   $1a
        .byte   $1b
        .byte   $1c
        .byte   $1d
        .byte   $1e
        .byte   $1f
        .enc    "sg_ascii"
        .text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        .text   "XYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
        .byte   $7f
        .byte   $80
        .byte   $81
        .byte   $82
        .byte   $83
        .byte   $84
        .byte   $85
        .byte   $86
        .byte   $87
        .byte   $88
        .byte   $89
        .byte   $8a
        .byte   $8b
        .byte   $8c
        .byte   $8d
        .byte   $8e
        .byte   $8f
        .byte   $90
        .byte   $91
        .byte   $92
        .byte   $93
        .byte   $94
        .byte   $95
        .byte   $96
        .byte   $97
        .byte   $98
        .byte   $99
        .byte   $9a
        .byte   $9b
        .byte   $9c
        .byte   $9d
        .byte   $9e
        .byte   $9f
        .enc    "sg_hiascii"
        .text   " !",$a2,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        .text   "XYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
        .byte   $ff

L144B   nop
        jmp     _L14DA

        .enc    "sg_ascii"
        .text   "ASCII escapes \: \\ \r \n \",$22," \' \t"
        .byte   $80
        .enc    "none"
        .text   "PETSCII escapes "
        .byte   $5c
        .text   ": "
        .byte   $5c
        .byte   $5c
        .byte   ' '
        .byte   $5c
        .text   "r "
        .byte   $5c
        .text   "n "
        .byte   $5c
        .text   $22," "
        .byte   $5c
        .text   "' "
        .byte   $5c
        .byte   't'
        .byte   $80
        .enc    "screen"
        .text   "SCR escapes "
        .byte   $1c
        .text   ": "
        .byte   $1c
        .byte   $1c
        .byte   ' '
        .byte   $1c
        .text   "r "
        .byte   $1c
        .text   "n "
        .byte   $1c
        .byte   $22
        .byte   ' '
        .byte   $1c
        .byte   $27
        .byte   ' '
        .byte   $1c
        .byte   't'
        .byte   $80
        .enc    "sg_hiascii"
        .text   "HIGH escapes \: \\ \r \n \0 \' \t"
        .byte   $80

_L14DA  nop
        jmp     _L1511

        .byte   $86
        .enc    "sg_ascii"
        .shift  "Hello"
        .shift  "HI"
        .shift  "X"
        .shift  "!"
        .byte   $86
        .enc    "sg_hiascii"
        .byte   $c8,$e5,$ec,$ec,$6f
        .byte   $c8,$49
        .enc    "sg_ascii"
        .byte   'X'
        .byte   '!'
        .byte   $86
        .enc    "none"
        .shift  "hello"
        .shift  "hi"
        .shift  "x"
        .shift  "!"
        .byte   $86
        .byte   $c8,$c5,$cc,$cc,$4f
        .byte   $c8,$49
        .byte   'x'
        .byte   '!'
        .byte   $86
        .enc    "screen"
        .shift  "Hello"
        .shift  "HI"
        .shift  "X"
        .shift  "!"
        .byte   $86

_L1511  nop
        rts

