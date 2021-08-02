         .setcpu "6502"
;         .segment "SEG000"
         .org    $8000
         .byte   $ea
         .res    8191,$00
;         .segment "SEG001"
         .org    $8000
         .res    8192,$01
;         .segment "SEG002"
         .org    $8000
         .res    8192,$02
;         .segment "SEG003"
         .org    $8000
         .res    8192,$03
;         .segment "SEG004"
         .org    $8000
         .res    8192,$04
;         .segment "SEG005"
         .org    $8000
         .res    8192,$05
;         .segment "SEG006"
         .org    $8000
         .res    8192,$06
;         .segment "SEG007"
         .org    $8000
         .res    8192,$07
