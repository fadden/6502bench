;Project edited to mark some non-ASCII operands as ASCII.
        .cpu    "65816"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$30
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
        lda     #$7f
        lda     #$7f
        .enc    "none"
        lda     #$7f
        .enc    "screen"
        lda     #$7f
        .enc    "sg_ascii"
        lda     #$0d
        rep     #$30
        .al
        .xl
        lda     #'B'
        lda     #'B' | $80
        .enc    "none"
        lda     #'B'
        .enc    "screen"
        lda     #'B'
        sep     #$30
        .as
        .xs
        rts

        .enc    "sg_ascii"
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
        .enc    "sg_ascii"
        .text   "{|}~"
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
        .enc    "sg_ascii"
        .text   "{|}~"
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
        .ptext  "Screen Code with length"
        .byte   $84
        .text   $18,$00,"Screen Code with length2"
        .byte   $84
        .shift  "Screen Code DCI"
        .byte   $84
        .byte   $85
;$00-ff block
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
        .byte   $c1
        .byte   $c2
        .byte   $c3
        .byte   $c4
        .byte   $c5
        .byte   $c6
        .byte   $c7
        .byte   $c8
        .byte   $c9
        .byte   $ca
        .byte   $cb
        .byte   $cc
        .byte   $cd
        .byte   $ce
        .byte   $cf
        .byte   $d0
        .byte   $d1
        .byte   $d2
        .byte   $d3
        .byte   $d4
        .byte   $d5
        .byte   $d6
        .byte   $d7
        .byte   $d8
        .byte   $d9
        .byte   $da
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
