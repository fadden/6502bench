;Projected edited to format non-char operands as chars.
        !cpu    6502
*       =       $1000
        lda     #'A'
        lda     #'A' | $80
        lda     #$c1
        lda     #$41
        ldx     #'a'
        ldx     #'a' | $80
        ldx     #$41
        ldx     #$01
        lda     #$7f       ;inappropriate format should be ignored
        lda     #$7f
        lda     #$7f
        lda     #$7f
        lda     #$0d
        jmp     L144B

        !byte   'C'
        !byte   'C' | $80
        !byte   $c3
        !byte   $43
        !word   'd'
        !word   'd' | $80
        !word   $44
        !word   $04
        !byte   $00,$45
        !byte   $00,$c5
        !byte   $00,$c5
        !byte   $00,$45
        !byte   $80
        !text   "low ASCII str"
        !xor    $80 {
        !text   "high ASCII str"
        }
        !byte   $80
        !pet    "PETSCII str"
        !byte   $80
        !scr    "Screen Code str"
        !byte   $82
        !text   $07,"Low ASCII CRLF",$0d,$0a
        !byte   $82
        !hex    87c8e9e7e8a0c1d3c3c9c9a0c3d2ccc68d8a
        !byte   $82
        !pet    $93,"PETSCII with ",$96,"control",$05," codes",$0d
        !byte   $83
        !text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        !text   "XYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
        !byte   $83
        !hex    a0a1a2a3a4a5a6a7a8a9aaabacadaeafb0b1b2b3b4b5b6b7b8b9babbbcbdbebf
        !hex    c0c1c2c3c4c5c6c7c8c9cacbcccdcecfd0d1d2d3d4d5d6d7d8d9dadbdcdddedf
        !hex    e0e1e2e3e4e5e6e7e8e9eaebecedeeeff0f1f2f3f4f5f6f7f8f9fafbfcfdfe
        !byte   $83
        !pet    " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        !pet    "XYZ["
        !byte   $5c
        !byte   $5d
        !byte   $5e
        !byte   $5f
        !byte   $60
        !pet    "abcdefghijklmnopqrstuvwxyz"
        !byte   $7b
        !byte   $7c
        !byte   $7d
        !byte   $7e
        !byte   $83
        !scr    " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        !scr    "XYZ["
        !byte   $1c
        !byte   $1d
        !byte   $1e
        !byte   $1f
        !byte   $40
        !scr    "abcdefghijklmnopqrstuvwxyz"
        !byte   $7b
        !byte   $7c
        !byte   $7d
        !byte   $7e
        !byte   $84
        !pet    "IICSTEP esrever"
        !byte   $84
        !pet    "null term PETSCII",$00
        !byte   $84
        !pet    "This null-terminated string is too long to fit on a single lin"
        !pet    "e, and will be split.",$00
        !byte   $84
        !pet    $13,"PETSCII with length"
        !byte   $84
        !pet    $14,$00,"PETSCII with length2"
        !byte   $84
        !pet    "pet dcI"
        !byte   $84
        !scr    "edoC neercS esrever"
        !byte   $84
        !scr    "null-term screen code?@"
        !byte   $84
        !scr    $17,"Screen Code with length"
        !byte   $84
        !scr    $18,$00,"Screen Code with length2"
        !byte   $84
        !scr    "Screen Code DC",$c9
        !byte   $84
        !byte   $85
        !byte   $00
        !byte   $01
        !byte   $02
        !byte   $03
        !byte   $04
        !byte   $05
        !byte   $06
        !byte   $07
        !byte   $08
        !byte   $09
        !byte   $0a
        !byte   $0b
        !byte   $0c
        !byte   $0d
        !byte   $0e
        !byte   $0f
        !byte   $10
        !byte   $11
        !byte   $12
        !byte   $13
        !byte   $14
        !byte   $15
        !byte   $16
        !byte   $17
        !byte   $18
        !byte   $19
        !byte   $1a
        !byte   $1b
        !byte   $1c
        !byte   $1d
        !pet    $1e,$1f," !",$22,"#$%&'()*+,-./0123456789:;<=>?@abcdefghijklmno"
        !pet    "pqrstuvwxyz["
        !byte   $5c
        !byte   $5d
        !byte   $5e
        !byte   $5f
        !byte   $60
        !byte   $61
        !byte   $62
        !byte   $63
        !byte   $64
        !byte   $65
        !byte   $66
        !byte   $67
        !byte   $68
        !byte   $69
        !byte   $6a
        !byte   $6b
        !byte   $6c
        !byte   $6d
        !byte   $6e
        !byte   $6f
        !byte   $70
        !byte   $71
        !byte   $72
        !byte   $73
        !byte   $74
        !byte   $75
        !byte   $76
        !byte   $77
        !byte   $78
        !byte   $79
        !byte   $7a
        !byte   $7b
        !byte   $7c
        !byte   $7d
        !byte   $7e
        !byte   $7f
        !byte   $80
        !byte   $81
        !byte   $82
        !byte   $83
        !byte   $84
        !byte   $85
        !byte   $86
        !byte   $87
        !byte   $88
        !byte   $89
        !byte   $8a
        !byte   $8b
        !byte   $8c
        !byte   $8d
        !byte   $8e
        !byte   $8f
        !byte   $90
        !byte   $91
        !byte   $92
        !byte   $93
        !byte   $94
        !pet    $95,$96,$97,$98,$99,$9a,$9b,$9c
        !byte   $9d
        !byte   $9e
        !byte   $9f
        !byte   $a0
        !byte   $a1
        !byte   $a2
        !byte   $a3
        !byte   $a4
        !byte   $a5
        !byte   $a6
        !byte   $a7
        !byte   $a8
        !byte   $a9
        !byte   $aa
        !byte   $ab
        !byte   $ac
        !byte   $ad
        !byte   $ae
        !byte   $af
        !byte   $b0
        !byte   $b1
        !byte   $b2
        !byte   $b3
        !byte   $b4
        !byte   $b5
        !byte   $b6
        !byte   $b7
        !byte   $b8
        !byte   $b9
        !byte   $ba
        !byte   $bb
        !byte   $bc
        !byte   $bd
        !byte   $be
        !byte   $bf
        !byte   $c0
        !pet    "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        !byte   $db
        !byte   $dc
        !byte   $dd
        !byte   $de
        !byte   $df
        !byte   $e0
        !byte   $e1
        !byte   $e2
        !byte   $e3
        !byte   $e4
        !byte   $e5
        !byte   $e6
        !byte   $e7
        !byte   $e8
        !byte   $e9
        !byte   $ea
        !byte   $eb
        !byte   $ec
        !byte   $ed
        !byte   $ee
        !byte   $ef
        !byte   $f0
        !byte   $f1
        !byte   $f2
        !byte   $f3
        !byte   $f4
        !byte   $f5
        !byte   $f6
        !byte   $f7
        !byte   $f8
        !byte   $f9
        !byte   $fa
        !byte   $fb
        !byte   $fc
        !byte   $fd
        !byte   $fe
        !byte   $ff

L144B   nop
        jmp     @L14DA

        !text   "ASCII escapes \\: \\\\ \\r \\n \\",$22," \\' \\t"
        !byte   $80
        !pet    "PETSCII escapes "
        !byte   $5c
        !pet    ": "
        !byte   $5c
        !byte   $5c
        !byte   $20
        !byte   $5c
        !pet    "r "
        !byte   $5c
        !pet    "n "
        !byte   $5c
        !pet    $22," "
        !byte   $5c
        !pet    "' "
        !byte   $5c
        !byte   $54
        !byte   $80
        !pet    "scr ",$05
        !byte   $13
        !byte   $03
        !byte   $01
        !byte   $10
        !byte   $05
        !byte   $13
        !pet    " ",$1c,": ",$1c,$1c," ",$1c,$12," ",$1c
        !byte   $0e
        !pet    " ",$1c,$22," ",$1c,"' ",$1c
        !byte   $14
        !byte   $80
        !xor    $80 {
        !text   "HIGH escapes \\: \\\\ \\r \\n \\0 \\' \\t"
        }
        !byte   $80

@L14DA  nop
        rts

