        .cpu    "65816"
zero    =       $00
longsym =       $123456

*       =       $1000
        .as
        .xs
        clc
        xce
        sep     #$30
        jml     L440000

lodat   .byte   $00
        .byte   $01
        .byte   $02

        .logical $440000
L440000 cmp     L440000
L440004 lda     L440000
        lda     @w0+(L440000 & $ffff)
        lda     zero
        bmi     L440004
        per     high44
        bne     high44
        brl     L44FFC0

dat44   .word   0+(dat44 & $ffff)
        .long   dat44

        .here
        .logical $44ffc0
L44FFC0 cmp     L44FFC0
high44  beq     _L44FFCB
        bmi     L440004
        brl     L440004

_L44FFCB jml    _L2000

        .here
        .logical $2000
_L2000  bit     _L2000
        pea     0+(dat44 & $ffff)
        pea     0+(dat44 >> 16)
        bne     _L200E
        jml     [lodat]

_L200E  nop
        jsr     j2
j2      jsr     j2+3
        jsr     j2-3
        jsl     longsym
        rts

        .here
