        !cpu    6502
zzz     =       $103b

*       =       $0000
        !word   $1000

        !pseudopc $1000 {
begin   bit     begin
        nop
        nop
        nop
        jmp     part2

        }
        !pseudopc $2000 {
part2   bit     part2
        nop
        lda     @local1
        lda     local2
        lda     local4
        nop
        nop
        nop
        bit     b4_part3
        jmp     part3

@local1 !byte   $81
local2  !byte   $82

b4_part3
        !pseudopc $3000 {
part3   bit     part3
        nop
        lda     local2
        lda     @local3
        lda     local4
        nop
        nop
        nop
        bit     _b4_part4
        jmp     part4

@local3 !byte   $83
local4  !byte   $84
        }
        }

        !pseudopc $4000 {
b4_part4a
        !pseudopc $5000 {
_b4_part4
        !pseudopc $6000 {
part4   bit     part4
        bit     _b4_part4
        nop
        nop
        nop
        jmp     part4a

        }
part4a  bit     part4a
        bit     b4_part4a
        jsr     part4b
        }
        !byte   $00

part4b  bit     part4b
        bit     zzz
        jmp     @LF000

        }
        !pseudopc $f000 {
@LF000  nop
        bne     $f003
        }
        !pseudopc $0000 {
        !byte   $ea
        !byte   $00
        }
