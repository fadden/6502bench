        .cpu    "65816"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
;Segment 02: Kind=Code; Attrs=NoSpecial; Name='          '
*       =       $030000
        .al
        .xl
L30000  clc
        xce
        .as
        .xs
        sep     #$30
        lda     L30000
        nop
        jsl     _L23456
        jsl     _L80000
        lda     _L23456
        lda     _L23456 & $ffff
        lda     #<_L23456
        lda     #>_L23456
        lda     #`_L23456
        lda     _L80019
        lda     @w_L80019 & $ffff
        lda     #<_L80019
        lda     #>_L80019
        lda     #`_L80019
        nop
        rep     #$30
        .al
        .xl
        lda     #_L80019 & $ffff
        lda     #_L80019 >> 8
        lda     #_L80019 >> 16
        nop
        lda     #$000f
        ldx     #_L80026 & $ffff
        ldy     #_L2345E & $ffff
        mvn     #(`_L2345E)+6,#`_L2345E
        nop
_L30045 pea     $0000
        pea     $f000
        pea     _L80019 & $ffff
        pea     _L80019 >> 16
        pea     _L30045 & $ffff
        pea     _L30045 >> 8
_L30057 pea     _L30045 >> 16
        pea     _L30057 & $ffff
        pea     $1045
        nop
        jmp     _L3006D

        .word   _L4FFE0 & $ffff
        .long   _L4FFE0
        .long   _L4FFE0
        .byte   $00

_L3006D rts

;Segment 03: Kind=Data; Attrs=BankRel, Dynamic; Name='PosFFE0   '
        .logical $04ffe0
_L4FFE0 .long   _L4FFE0
        .byte   $00
        .byte   $00
        .byte   $01
        .byte   $02
        .byte   $03
        .byte   $04
        .byte   $05
        .byte   $06
        .byte   $07
        .byte   $08
        .byte   $09
        .byte   $0a
        .byte   $0b
        .byte   $0c
        .byte   $0d
        .byte   $0e
        .byte   $0f
        .byte   $10
        .byte   $11
        .byte   $12
        .byte   $13
        .byte   $14
        .byte   $15
        .byte   $16
        .byte   $17
        .byte   $18
        .byte   $19
        .byte   $1a
        .byte   $1b
        .here
        .logical $050000
        .byte   $1c
        .byte   $1d
        .byte   $1e
        .byte   $1f
        .text   " !",$22,"#$%&'()*+,-./"

;Segment 04: Kind=Code; Attrs=NoSpecial; Name='Bank2     '
        .here
        .logical $023456
        .as
        .xs
_L23456 lda     _L23456
        jsr     _L2346E
        rtl

_L2345E .fill   16,$00

_L2346E nop
        rts

;Segment 05: Kind=Code; Attrs=AbsBank, Dynamic; Name='Bank8     '
        .here
        .logical $080000
_L80000 lda     _L80000
        lda     @w_L80019 & $ffff
        nop
        lda     $010000
        lda     $020000
        lda     L30000
        lda     _L80000
        rtl

_L80019 .long   _L80019
        .byte   $00
        .long   _L23456
        .byte   $00
        .byte   $80
        .byte   $00
        .byte   $10
        .byte   $08
        .byte   $00
_L80026 .text   "This is a test."
        .byte   $00
;Segment 06: Kind=Data; Attrs=0; Name='Filler    '
        .here
        .logical $060000
        .text   "hello, world!"
        .here
