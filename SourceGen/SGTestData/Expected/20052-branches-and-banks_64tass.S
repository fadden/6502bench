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
        lda     @wL440000 & $ffff
        lda     zero
        bmi     L440004
        per     high44
        bne     high44
        brl     L44FFC0

dat44   .word   dat44 & $ffff
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
        pea     dat44 & $ffff
        pea     dat44 >> 16
        bne     skip
        jml     [lodat]

skip    nop
        jsr     j2
j2      jsr     j2+3
        jsr     j2-3
        jsl     longsym
        jml     bank54

        .here
        .logical $543210
bank54  cmp     bank54
        bra     L54321C

backchk nop
        nop
        rts

backval .long   backchk

L54321C lda     backchk
        lda     fwdchk
        lda     $543216
        lda     $54327d
        lda     0+(backchk & $ffff)+1
        lda     0+(backchk & $ffff)-1
        lda     0+(fwdchk & $ffff)+1
        lda     0+(fwdval & $ffff)+2
        nop
        jsr     backchk
        jsr     backchk+1
        jsr     $543218
        jsr     fwdchk
        jsr     fwdchk+1
        jsr     $54327f
        nop
        ldx     #$00
        jsr     (backval,x)
        jsr     (fwdval,x)
        jsr     _L54326E
        jsr     _L543271
        jsr     _L543268
        jsr     _L54326B
        jsr     _L543274
        jsr     _L543277
        bra     L543280

_L543268 jmp    (backval,x)

_L54326B jmp    (fwdval,x)

_L54326E jmp    (lodat)

_L543271 jmp    (lodat)

_L543274 jml    [lodat]

_L543277 jml    [lodat]

fwdval  .long   fwdchk

fwdchk  nop
        nop
        rts

L543280 jsr     skip+$540000
        nop
        phk
        plb
        lda     $544280
        jsl     $544280
        nop
        lda     $4280
        jsr     $544280
        jsr     ($544280,x)
        nop
        rtl

        .here
