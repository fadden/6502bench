        !cpu    6502
FOOZP   =       $f0
OVERL   =       $3000
projover =      $3100      ;replaced by proj symbol w/same name, diff addr (no match)
FOO     =       $f000
FOO_5   =       $f005

*       =       $2000
        jmp     skip

        !text   "hello, "  ;string should be split by no-op addr change
        !pseudopc $200a {
        !text   "world"
        !byte   $80
        }

        !pseudopc $2100 {
skip    lda     #$00
        sta     addr1-1    ;edit this operand
        sta     addr1
        sta     addr1+1
        jmp     next1

        !text   "testing stuff."
addr1   !text   "!?---"
        }
        !pseudopc $00f4 {
zf4     !byte   $00
        !byte   $00
L00F6   !byte   $00
        !byte   $00
        }
        !pseudopc $2200 {
line1   !text   "this is line1",$00
line2   !text   "this is line2",$00
L221C   !text   "this is line3",$00
        !text   "this is line4",$00
        !byte   $80

        !zone   Z00006e
.foo_f0 =       $f0
.foo_f4 =       $f4
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
        ldy     .foo_f0
        ldy     .foo_f0+1
        ldy     FOOZP+2
        ldy+1   zf4
        ldy+1   L00F6
        lda     #$00
        nop
        bcc     bitsy+1
bitsy   !byte   $2c
        lda     #$ff
        jmp     @L228B

        !word   FOO
        !word   FOO_5
        !word   FOO+9
        !word   OVERL
        !word   projover
        !word   $4000
        !word   line1
        !word   line2+5
        !word   L221C
        !word   line2+33
        !word   zf4
        !byte   $80

@L228B  nop
        jmp     @L22A8

        !byte   $4c
        !byte   $8b
        !text   $22,"L9",$22,"L"
        !byte   $95
        !byte   $22
        !byte   $4c
        !byte   $00
        !byte   $30
        !byte   $ea
        !text   "L9",$22,"L"
        !byte   $00
        !text   "1LLLLLL"

@L22A8  rts

        }
