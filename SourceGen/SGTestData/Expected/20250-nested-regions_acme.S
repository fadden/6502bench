        !cpu    6502
*       =       $0000
        !word   $3000      ;load address

        !pseudopc $1000 {
        !pseudopc *+$1000 {
        !pseudopc *+$1000 {
L3000   bit     L3000
@L3003  lda     @L3003
        and     @LD003
        jmp     @L200C

        }
@L200C  bit     @L200C
        jmp     @L1012

        }
@L1012  bit     @L1012
        jsr     @L4000
        !pseudopc $0000 {
        !byte   $00
        !pet    "Null-term PETSCII string",$00
        !byte   $80
        !word   @L3003
        !word   @LD003
        !byte   $80
        }

        !pseudopc $4000 {
@L4000  bit     @L4000
        bit     @L5000
        bit     @L500F
        bit     @L500F
        nop
        jmp     @L4020

        !pseudopc $5000 {
@L5000  bit     @L5000
        bit     @L4000
        nop
        nop
@L5008  bit     @L5008
        bit     @L5017
        nop
@L500F  rts

        }
@L4020  bit     @L4020
        bit     @L500F
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        jmp     @L4040

        !pseudopc $5008 {
@L5008_0 bit    @L5008_0
        bit     @L5000
        nop
@L500F_0 bit    @L500F_0
        nop
        nop
        nop
        nop
        nop
@L5017  rts

        }
@L4040  bit     @L4040
        bit     @L5017
        nop
        jmp     @LD000

        }
        !pseudopc *+$bf7e {
@LD000  bit     @L200C
@LD003  nop
        jmp     @LE000

        !pseudopc $e000 {
@LE000  bit     @LE000
        jmp     @LF000

        !pseudopc $f000 {
@LF000  bit     @LF000
        lda     @L3003
        and     @LD003
        nop
        rts

        }
        }
        }
        }
