        !cpu    6502
*       =       $1000
        jmp     L0000

        !pseudopc $0000 {
L0000   bit+2   L0000
L0003   lda+1   L0000
        lda+1   L0003
        bne     LFFC3
        bmi     $ffc3
        bvs     L0012
        bvc     L0080

lodat   !byte   $00
        !byte   $01
        !byte   $02

L0012   lda+1   lodat+1
        clc
        bcc     LFFC0

        } ;!pseudopc
        !pseudopc $0080 {
L0080   bit+2   L0080
        jmp     LFFC6

        } ;!pseudopc
        !pseudopc $ffc0 {
LFFC0   bit     LFFC0
LFFC3   clc
        bcc     L0003

LFFC6   rts

        } ;!pseudopc
