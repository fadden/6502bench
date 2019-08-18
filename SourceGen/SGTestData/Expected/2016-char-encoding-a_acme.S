;Project edited to mark some non-ASCII operands as ASCII.
        !cpu    65816
*       =       $1000
        !as
        !rs
        clc
        xce
        sep     #$30
        lda     #'A'
        lda     #'A' | $80
        lda     #$c1
        lda     #$41
        ldx     #'a'
        ldx     #'a' | $80
        ldx     #$41
        ldx     #$01
        lda     #$7f
        lda     #$7f
        lda     #$7f
        lda     #$7f
        lda     #$0d
        rep     #$30
        !al
        !rl
        lda     #'B'
        lda     #'B' | $80
        lda     #$c2
        lda     #$42
        sep     #$30
        !as
        !rs
        rts

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
        !text   "XYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
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
        !text   "{|}~"
        !byte   $83
        !scr    " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        !scr    "XYZ["
        !byte   $1c
        !byte   $1d
        !byte   $1e
        !byte   $1f
        !byte   $40
        !scr    "abcdefghijklmnopqrstuvwxyz"
        !text   "{|}~"
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
        !pet    "bad dcI"
        !byte   $84
        !scr    "edoC neercS esrever"
        !byte   $84
        !scr    $17,"Screen Code with length"
        !byte   $84
        !scr    $18,$00,"Screen Code with length2"
        !byte   $84
        !scr    "Screen Code DC",$c9
        !byte   $84
        !byte   $85
;$00-ff block
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
        !byte   $1e
        !byte   $1f
        !text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVW"
        !text   "XYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~"
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
        !byte   $95
        !byte   $96
        !byte   $97
        !byte   $98
        !byte   $99
        !byte   $9a
        !byte   $9b
        !byte   $9c
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
        !byte   $c1
        !byte   $c2
        !byte   $c3
        !byte   $c4
        !byte   $c5
        !byte   $c6
        !byte   $c7
        !byte   $c8
        !byte   $c9
        !byte   $ca
        !byte   $cb
        !byte   $cc
        !byte   $cd
        !byte   $ce
        !byte   $cf
        !byte   $d0
        !byte   $d1
        !byte   $d2
        !byte   $d3
        !byte   $d4
        !byte   $d5
        !byte   $d6
        !byte   $d7
        !byte   $d8
        !byte   $d9
        !byte   $da
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
