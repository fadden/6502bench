        .cpu    "6502"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
FOOZP   =       $f0
OVERL   =       $3000
projover =      $3100      ;replaced by proj symbol w/same name, diff addr (no match)
FOO     =       $f000
FOO_5   =       $f005

*       =       $2000
        jmp     skip

        .text   "hello, "  ;string should be split by no-op addr change
        .logical $200a
        .text   "world"
        .byte   $80
        .here

        .logical $2100
skip    lda     #$00
        sta     addr1-1    ;edit this operand
        sta     addr1
        sta     addr1+1
        jmp     next1

        .text   "testing stuff."
addr1   .text   "!?---"
        .here
        .logical $00f4
zf4     .byte   $00
        .byte   $00
L00F6   .byte   $00
        .byte   $00
        .here
        .logical $2200
line1   .null   "this is line1"
line2   .null   "this is line2"
L221C   .null   "this is line3"
        .null   "this is line4"
        .byte   $80

foo_f0  .var    $f0
foo_f4  .var    $f4
next1   lda     addr1
        sta     FOO
        sta     FOO_5
        sta     FOO+8
        ldx     OVERL
        ldx     projover
        ldx     $4000
        ldx     line1
        ldx     line2+1
        ldx     L221C
        ldx     L221C+1
        ldx     line2+35
        ldy     $00
        ldy     foo_f0
        ldy     foo_f0+1
        ldy     FOOZP+2
        ldy     zf4
        ldy     L00F6
        lda     #$00
        nop
        bcc     bitsy+1
bitsy   .byte   $2c
        lda     #$ff
        jmp     _L228B

        .word   FOO
        .word   FOO_5
        .word   FOO+9
        .word   OVERL
        .word   projover
        .word   $4000
        .word   line1
        .word   line2+5
        .word   L221C
        .word   line2+33
        .word   zf4
        .byte   $80

_L228B  nop
        jmp     _L22A8

        .byte   $4c
        .byte   $8b
        .text   $22,"L9",$22,"L"
        .byte   $95
        .byte   $22
        .byte   $4c
        .byte   $00
        .byte   $30
        .byte   $ea
        .text   "L9",$22,"L"
        .byte   $00
        .text   "1LLLLLL"

_L22A8  rts

        .here
