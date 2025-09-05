;Project file was edited to force ASCII formatting for some operands.
        .cpu    "6502"
        .enc    "sg_hiascii"
        .cdef   $20,$7e,$a0
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
bword   .macro
        .byte   (\1)>>8,(\1)&$ff
        .endmacro
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

        .byte   $01
        .word   $0201
        .long   $030201
        .dword  $04030201
        .byte   1
        .word   513
        .long   197121
        .dword  67305985
        .byte   1
        .word   513
        .long   197121
        .dword  67305985
        .byte   %00000001
        .word   %0000001000000001
        .long   %000000110000001000000001
        .dword  %00000100000000110000001000000001
        .byte   255
        .word   65279
        .long   16645887
        .dword  4244504319

skipdata lda    #'h'
        lda     'h'
        lda     @w'h'
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

more_ascii .byte 'h'
        .byte   $80
        .word   $6868
        .byte   $80
        .word   skipdata
        .long   skipdata
        bword   skipdata
        .byte   <more_ascii
        .byte   >more_ascii
        .word   more_ascii
        .long   more_ascii
        .dword  more_ascii
        bword   more_ascii
        .byte   '['
        .byte   '{'
        .byte   '|'
        .byte   '}'
        .byte   ','
        .byte   '[' | $80
        .byte   '{' | $80
        .byte   '|' | $80
        .byte   '}' | $80
        .byte   ',' | $80

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
        lda     @w1
        lda     65534
        jmp     4311

        .byte   0
        .byte   1
        .byte   127
        .byte   128
        .byte   255
        .word   1
        .word   65534
        bword   2
        bword   65533
        .long   66051
        .long   16776957
        .dword  16909060
        .dword  4294901244

        rts

