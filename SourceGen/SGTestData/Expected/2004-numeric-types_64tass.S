;Project file was edited to get all big-endian data types, and to have an
;incorrect .junk alignment directive.
        .cpu    "6502"
*       =       $1000
        rts

        .byte   $11
        .word   $1122
        .long   $112233
        .dword  $11223344
        .byte   $11
        .byte   $11,$22
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
LABEL   .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff ;comment
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
