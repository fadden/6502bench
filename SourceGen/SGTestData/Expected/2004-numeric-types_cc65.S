;Project file was edited to get all big-endian data types.
         .setcpu "6502"
;         .segment "SEG000"
         .org    $1000
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
LABEL:   .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff ;comment
         .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff
         .byte   $00,$11,$22,$33,$44,$55,$66,$77,$88,$99,$aa,$bb,$cc,$dd,$ee,$ff
         .byte   $ff,$ee,$dd,$cc,$bb,$aa,$99,$88,$77,$66,$55,$44,$33,$22,$11,$00
         .byte   $80
