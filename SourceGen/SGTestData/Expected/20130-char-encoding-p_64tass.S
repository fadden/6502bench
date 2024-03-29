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
        .enc    "none"
        .text   $1e,$1f," !",$22,"#$%&'()*+,-./0123456789:;<=>?@abcdefghijklmno"
        .text   "pqrstuvwxyz["
        .byte   $5c
        .byte   $5d
        .byte   $5e
        .byte   $5f
        .byte   $60
        .byte   $61
        .byte   $62
        .byte   $63
        .byte   $64
        .byte   $65
        .byte   $66
        .byte   $67
        .byte   $68
        .byte   $69
        .byte   $6a
        .byte   $6b
        .byte   $6c
        .byte   $6d
        .byte   $6e
        .byte   $6f
        .byte   $70
        .byte   $71
        .byte   $72
        .byte   $73
        .byte   $74
        .byte   $75
        .byte   $76
        .byte   $77
        .byte   $78
        .byte   $79
        .byte   $7a
        .byte   $7b
        .byte   $7c
        .byte   $7d
        .byte   $7e
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
        .text   $95,$96,$97,$98,$99,$9a,$9b,$9c
        .byte   $9d
        .byte   $9e
        .byte   $9f
        .byte   $a0
        .byte   $a1
        .byte   $a2
        .byte   $a3
        .byte   $a4
        .byte   $a5
        .byte   $a6
        .byte   $a7
        .byte   $a8
        .byte   $a9
        .byte   $aa
        .byte   $ab
        .byte   $ac
        .byte   $ad
        .byte   $ae
        .byte   $af
        .byte   $b0
        .byte   $b1
        .byte   $b2
        .byte   $b3
        .byte   $b4
        .byte   $b5
        .byte   $b6
        .byte   $b7
        .byte   $b8
        .byte   $b9
        .byte   $ba
        .byte   $bb
        .byte   $bc
        .byte   $bd
        .byte   $be
        .byte   $bf
        .byte   $c0
        .text   "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        .byte   $db
        .byte   $dc
        .byte   $dd
        .byte   $de
        .byte   $df
        .byte   $e0
        .byte   $e1
        .byte   $e2
        .byte   $e3
        .byte   $e4
        .byte   $e5
        .byte   $e6
        .byte   $e7
        .byte   $e8
        .byte   $e9
        .byte   $ea
        .byte   $eb
        .byte   $ec
        .byte   $ed
        .byte   $ee
        .byte   $ef
        .byte   $f0
        .byte   $f1
        .byte   $f2
        .byte   $f3
        .byte   $f4
        .byte   $f5
        .byte   $f6
        .byte   $f7
        .byte   $f8
        .byte   $f9
        .byte   $fa
        .byte   $fb
        .byte   $fc
        .byte   $fd
        .byte   $fe
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
        jmp     _L1587

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
        .enc    "sg_ascii"
        .shift  "ascii",$7f
        .byte   $86
        .shift  $22,"A quote1",$22
        .shift  "'A quote2'"
        .byte   $86
        .enc    "sg_hiascii"
        .byte   $a2,$c8,$c1,$a0,$f1,$f5,$ef,$f4,$e5,$b1,$22
        .byte   $a7,$c8,$c1,$a0,$f1,$f5,$ef,$f4,$e5,$b2,$27
        .byte   $86
        .enc    "none"
        .shift  "petscii",$5e
        .byte   $86
        .shift  $22,"a quote1",$22
        .shift  "'a quote2'"
        .byte   $86
        .shift  $5c
        .byte   $86
        .enc    "screen"
        .shift  "ScreenCode",$1e
        .byte   $86
        .shift  $22,"A quote1",$22
        .shift  "'A quote2'"
        .byte   $86
        .shift  $1c
        .byte   $86

_L1587  nop
        rts

