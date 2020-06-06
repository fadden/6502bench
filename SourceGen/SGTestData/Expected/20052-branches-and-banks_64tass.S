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
_L0003  lda     L0000
        lda     _L0003
        bne     _LFFC3
        bmi     $ffc3
        per     _LFFC3
        bvs     _L0016
        brl     _L0080

_lodat  .byte   $00
        .byte   $01
        .byte   $02

_L0016  lda     _lodat+1
        brl     _LFFC0

        .here
        .logical $0080
_L0080  bit     @w_L0080
        jml     _L440000

        .here
        .logical $ffc0
_LFFC0  bit     _LFFC0
_LFFC3  brl     _L0003

        .here
        .logical $440000
_L440000 cmp    _L440000
_L440004 lda    _L440000
        lda     @w0+(_L440000 & $ffff)
        lda     L0000
        bmi     _L440004
        per     _high44
        bne     _high44
        brl     _L44FFC0

_dat44  .word   0+(_dat44 & $ffff)
        .long   _dat44

        .here
        .logical $44ffc0
_L44FFC0 cmp    _L44FFC0
_high44 beq     _L44FFCB
        bmi     _L440004
        brl     _L440004

_L44FFCB jml    _L2000

        .here
        .logical $2000
_L2000  bit     _L2000
        pea     0+(_dat44 & $ffff)
        pea     0+(_dat44 >> 16)
        bne     _L200E
        jml     [_lodat]

_L200E  nop
        jsr     _j2
_j2     jsr     _j2+3
        jsr     _j2-3
        jsl     longsym
        rts

        .here
