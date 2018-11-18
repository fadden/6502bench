;Project file was edited for some ASCII operands.
         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$30
         lda     $01
         lda     $0102
         lda     $010203
         lda     1
         lda     258
         lda     66051
         lda     %00000001
         lda     %0000000100000010
         lda     %000000010000001000000011
         bra     skipdata

         .byte   $01
         .word   $0201
         .faraddr $030201
         .dword  $04030201
         .byte   1
         .word   513
         .faraddr 197121
         .dword  67305985
         .byte   %00000001
         .word   %0000001000000001
         .faraddr %000000110000001000000001
         .dword  %00000100000000110000001000000001

skipdata: lda    #'h'
         lda     'h'
         lda     a:'h'
         lda     f:'h'
         lda     #$1f
         lda     #' '
         lda     #'~'
         lda     #$7f
         lda     #$80
         lda     #$9f
         lda     #$a0
         lda     #$fe
         lda     #$ff
         rep     #'0'
         .a16
         .i16
         lda     #'h'
         lda     #$c8
         lda     #$6868
         rts

more_ascii: .byte 'h'
         .byte   $80
         .word   $6868
         .byte   $80
         .word   skipdata
         .faraddr skipdata
         .dbyt   skipdata
         .byte   <more_ascii
         .byte   >more_ascii
         .word   more_ascii
         .faraddr more_ascii
         .dbyt   more_ascii
