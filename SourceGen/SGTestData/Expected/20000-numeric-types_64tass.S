;Project file was edited to get all big-endian data types, and to have an
;incorrect .junk alignment directive.
        .cpu    "6502"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
bword   .macro
        .byte   (\1)>>8,(\1)&$ff
        .endmacro
*       =       $1000
        bit     L1448
        jsr     L14A8
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        rts

        .byte   $11
        .word   $1122
        .long   $112233
        .dword  $11223344
        .byte   $11
        bword   $1122
        .byte   $11,$22,$33
        .byte   $11,$22,$33,$44
        .fill   2,$00
        .byte   $80
        .fill   3,$00
        .byte   $80
        .fill   4,$00
        .byte   $80
        .fill   5,$00
        .byte   $80
        .fill   256,$00
        .byte   $80
        .fill   257,$cc
        .byte   $11
        .byte   $80
        .byte   $11,$22,$33,$44,$55,$66,$77,$88,$99,$00
        .byte   $80
LABEL   .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff ;comment on bulk
        .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff
        .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff
        .byte   $ff,$ee,$dd,$cc,$bb,$aa,$99,$88,$77,$66,$55,$44,$33,$22,$11,$00
        .byte   $80
        .align  256,$aa
        .byte   $81
        .align  64,$00
        .byte   $81
        .align  32,$ab
        .byte   $00,$00,$00,$00,$00,$00,$00,$01
        .byte   $81
        .byte   $10,$00,$00,$00,$00,$00,$00,$00
        .byte   $81
        .fill   2,$dd      ;incorrect alignment
        .align  256,$00
        .fill   8,$82
        .logical $1408
        .fill   8,$82      ;note no-op .ORG
        .fill   8,$83
        .here
        .logical $1428
        .fill   8,$83      ;meaningful .ORG
        .fill   8,$84
UserLabel .fill 8,$84
        .fill   8,$85
L1448   .fill   8,$85
        .fill   16,$86
        .text   $87,$87,$87,$87,$87,$87,$87,$87
;long comment
        .text   $87,$87,$87,$87,$87,$87,$87,$87
        .fill   16,$88
        .fill   8,$89
        .fill   8,$89
        .text   $8a,$8a,$8a,$8a,$8a,$8a,$8a,$8a
        .text   $8a,$8a,$8a,$8a,$8a,$8a,$8a,$8a
        .fill   8,$8b
L14A8   .fill   8,$8b
        .fill   8,$8c
        .byte   %10001100
        .fill   7,$8c
        .byte   $90
        .byte   $92,$91
        .byte   $95,$94,$93
        .byte   $99,$98,$97,$96
        .fill   1,$00
        .fill   2,$00
        .fill   3,$00
        .fill   4,$00
        .byte   $80
        .fill   5,$00
        .byte   $80
        .here
