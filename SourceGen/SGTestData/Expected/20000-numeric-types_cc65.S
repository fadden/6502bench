;Project file was edited to get all big-endian data types, and to have an
;incorrect .junk alignment directive.
         .setcpu "6502"
;         .segment "SEG000"
         .org    $1000
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
         .faraddr $112233
         .dword  $11223344
         .byte   $11
         .dbyt   $1122
         .byte   $11,$22,$33
         .byte   $11,$22,$33,$44
         .res    2,$00
         .byte   $80
         .res    3,$00
         .byte   $80
         .res    4,$00
         .byte   $80
         .res    5,$00
         .byte   $80
         .res    256,$00
         .byte   $80
         .res    257,$cc
         .byte   $11
         .byte   $80
         .byte   $11,$22,$33,$44,$55,$66,$77,$88,$99,$00
         .byte   $80
LABEL:   .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff ;comment on bulk
         .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff
         .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff
         .byte   $ff,$ee,$dd,$cc,$bb,$aa,$99,$88,$77,$66,$55,$44,$33,$22,$11,$00
         .byte   $80
         .res    115,$aa
         .byte   $81
         .res    63,$00
         .byte   $81
         .res    31,$ab
         .byte   $00,$00,$00,$00,$00,$00,$00,$01
         .byte   $81
         .byte   $10,$00,$00,$00,$00,$00,$00,$00
         .byte   $81
         .res    2,$dd      ;incorrect alignment
         .res    140,$00
         .res    8,$82
;         .segment "SEG001"
         .org    $1408
         .res    8,$82      ;note no-op .ORG
         .res    8,$83
;         .segment "SEG002"
         .org    $1428
         .res    8,$83      ;meaningful .ORG
         .res    8,$84
UserLabel: .res  8,$84
         .res    8,$85
L1448:   .res    8,$85
         .res    16,$86
         .res    8,$87
;long comment
         .res    8,$87
         .res    16,$88
         .res    8,$89
         .res    8,$89
         .res    8,$8a
         .res    8,$8a
         .res    8,$8b
L14A8:   .res    8,$8b
         .res    8,$8c
         .byte   %10001100
         .res    7,$8c
