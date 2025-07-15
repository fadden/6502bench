;Project file was edited to force ASCII formatting for some operands.
        !cpu    6502
*       =       $1000
        lda     $01
        lda     $0102
        lda     $fe
        lda     $feff
        lda     1
        lda     258
        lda     254
        lda     65279
        lda     1
        lda     258
        lda     254
        lda     65279
        lda     %00000001
        lda     %0000000100000010
        lda     %11111110
        lda     %1111111011111111
        jmp     skipdata

        !byte   $01
        !word   $0201
        !24     $030201
        !32     $04030201
        !byte   1
        !word   513
        !24     197121
        !32     67305985
        !byte   1
        !word   513
        !24     197121
        !32     67305985
        !byte   %00000001
        !word   %0000001000000001
        !24     %000000110000001000000001
        !32     %00000100000000110000001000000001
        !byte   255
        !word   65279
        !24     16645887
        !32     4244504319

skipdata lda    #'h'
        lda     'h'
        lda+2   'h'
        lda     #$1f
        lda     #' '
        lda     #'"'
        lda     #$27
        lda     #'~'
        lda     #$7f
        lda     #$80
        lda     #$9f
        lda     #' ' | $80
        lda     #'"' | $80
        lda     #$a7
        lda     #'~' | $80
        lda     #$ff
        jmp     L10A4

more_ascii !byte 'h'
        !byte   $80
        !word   $6868
        !byte   $80
        !word   skipdata
        !24     skipdata
        !byte   $10,$5d
        !byte   <more_ascii
        !byte   >more_ascii
        !word   more_ascii
        !24     more_ascii
        !32     more_ascii
        !byte   $10,$81
        !byte   '['
        !byte   '{'
        !byte   '|'
        !byte   '}'
        !byte   ','
        !byte   '[' | $80
        !byte   '{' | $80
        !byte   '|' | $80
        !byte   '}' | $80
        !byte   ',' | $80

;Signed-decimal tests.  All of these must be formatted as signed decimal; this
;may require hand-editing or modifications to the instruction operand editor.
L10A4   nop
        lda     #0
        lda     #1
        lda     #127
        lda     #-128
        lda     #-1
        lda     1
        lda     254
        lda+2   1
        lda     65534
        jmp     4311

        !byte   0
        !byte   1
        !byte   127
        !byte   -128
        !byte   -1
        !word   1
        !word   -2
        !byte   $00,$02
        !byte   $ff,$fd
        !24     66051
        !24     -259
        !32     16909060
        !32     -66052

        rts

