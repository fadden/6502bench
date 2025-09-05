;Project was edited to add a label in the middle of a dense hex region, and add
;a duplicate label.
        .cpu    "6502"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
bword   .macro
        .byte   (\1)>>8,(\1)&$ff
        .endmacro
BMI1    =       $30        ;opcode mnemonic
zip     =       $cd
absl    =       $1029
absh    =       $feed

plataddr =      $3000      ;address only in platform file
projalsa =      $3200      ;same val as projalso

*       =       $2345
start   lda     #zip
        lda     #zip+16
        lda     #zip-192
        lda     #<absh
        lda     #>absh
        lda     #<absh-192
        lda     #(>absh)+1
        lda     #<absl
        lda     #>absl
        lda     #<absl+192
        lda     #(>absl)-1
        lda     #<start
        lda     #>start
        lda     zip+1
        lda     @wzip+1
        lda     absh-1
        lda     absh+1
        lda     start+1
        lda     start-1
        jmp     _L23A3

        .byte   zip
        .byte   <absh
        .byte   >absh
        .byte   <start
        .byte   >start
        .word   zip
        .word   absl
        .word   absl >> 8
        .word   absl-$1000
        .word   0+(absl >> 8)-16
        .word   absh
        .word   absh >> 8
        .word   absh-$f000
        .word   0+(absh >> 8)+16
        .word   start
        .word   start >> 8
        .word   start+1
        .word   start >> 8
        bword   absh
        .long   zip
        .long   absh
        .long   absh >> 8
        .long   start
        .long   start >> 8

_L23A3  jmp     _L1000_1

        .logical $1000
_L1000_1 nop
_L1000  nop
_L1000_0 nop
_l1000  lda     plataddr
        lda     $3100
        lda     projalsa
        lda     $3300
        jmp     calls

        nop
targ    nop
        nop
L1015   jsr     targ-1
        jsr     targ
        jsr     targ+1
L101E   jmp     targ-1

L1021   jmp     targ

L1024   jmp     targ+1

L1027   jmp     targ-1

L102A   jmp     targ

L102D   jmp     targ+1

calls   jsr     L1015
        jsr     L101E
        jsr     L1021
        jsr     L1024
        jsr     L1027
        jsr     L102A
        jsr     L102D
        jmp     L1160

        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f ;bulky
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .text   "This is a long string. Put a label and comment on it to confir" ;stringy
        .text   "m that the label and comment only appear on the first line.  T"
        .text   "he quick brown fox jumps over the lazy dogs."

L1160   adc     #BMI1
        lda     <targ-1
        lda     <targ+1
        lda     <targ-1,x
        lda     (<targ+4),y
        rts

        .here
