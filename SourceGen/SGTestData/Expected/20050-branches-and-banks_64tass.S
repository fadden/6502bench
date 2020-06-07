        .cpu    "6502"
*       =       $1000
        jmp     L0000

        .logical $0000
L0000   bit     @wL0000
L0003   lda     L0000
        lda     L0003
        bne     LFFC3
        bmi     $ffc3
        bvs     L0012
        bvc     L0080

lodat   .byte   $00
        .byte   $01
        .byte   $02

L0012   lda     lodat+1
        clc
        bcc     LFFC0

        .here
        .logical $0080
L0080   bit     @wL0080
        jmp     LFFC6

        .here
        .logical $ffc0
LFFC0   bit     LFFC0
LFFC3   clc
        bcc     L0003

LFFC6   rts

        .here
