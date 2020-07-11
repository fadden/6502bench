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
        ldy     #_L23467 & $ffff
        mvn     #(`_L23467)+6,#`_L23467
        nop
        lda     $0000,y
        sta     $0002,y
_L3004B pea     $0000
        pea     $f000
        pea     _L80019 & $ffff
        pea     _L80019 >> 16
        pea     _L3004B & $ffff
        pea     _L3004B >> 8
_L3005D pea     _L3004B >> 16
        pea     _L3005D & $ffff
        pea     $104b
        nop
        jmp     _L30073

        .word   _L4FFE0 & $ffff
        .long   _L4FFE0
        .long   _L4FFE0
        .byte   $00

_L30073 phk
        plb
        lda     #$0000
        asl     a
        asl     a
        tax
        lda     @w_L30088 & $ffff,x
        pha
        lda     @w_L30086 & $ffff,x
        pha
        beq     _L3008E
        rts

_L30086 .byte   $4b
        .byte   $00
_L30088 .byte   $03
        .byte   $00
        .long   _L30073
        .byte   $00

_L3008E rts

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
        jsr     _L23477
        ldy     #<_L23467
        ora     $3456
        ora     _L23456
        rtl

_L23467 .fill   16,$00

_L23477 nop
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
