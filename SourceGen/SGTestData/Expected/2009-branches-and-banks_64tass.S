        .cpu    "65816"
longsym =       $123456

*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$30
        jmp     L0000

        .logical $0000
L0000   bit     @wL0000
L0003   lda     L0000
        lda     L0003
        bne     LFFC3
        bmi     $ffc3
        per     LFFC3
        bcc     L0016
        brl     L0080

lodat   .byte   $00
        .byte   $01
        .byte   $02

L0016   lda     lodat+1
        brl     LFFC0

        .here
        .logical $0080
L0080   bit     @wL0080
        jml     L440000

        .here
        .logical $ffc0
LFFC0   bit     LFFC0
LFFC3   brl     L0003

        .here
        .logical $440000
L440000 cmp     L440000
L440004 lda     L440000
        lda     @wL440000 & $ffff
        lda     L0000
        bmi     L440004
        per     high44
        bne     high44
        brl     L44FFC0

dat44   .word   dat44 & $ffff
        .long   dat44

        .here
        .logical $44ffc0
L44FFC0 cmp     L44FFC0
high44  beq     L44FFCB
        bmi     L440004
        brl     L440004

L44FFCB jml     L2000

        .here
        .logical $2000
L2000   bit     L2000
        pea     dat44 & $ffff
        pea     dat44 >> 16
        bne     L200E
        jml     [lodat]

L200E   nop
        jsr     j2
j2      jsr     j2+3
        jsr     j2-3
        jsl     longsym
        rts

        .here
