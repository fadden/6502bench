;Edited to have duplicate labels (PROJ_ZERO, DPCODE).
        .cpu    "65816"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
CONST_ZERO =    $f0        ;project const

PROJ_ZERO =     $00        ;project addr
PROJ_ONE =      $01        ;project addr

*       =       $1000
        .as
        .xs
        ldy     PROJ_ZERO
        lda     (PROJ_ONE),y
        sta     $03        ;could be PROJ_ONE+2, but "nearby" is off
        ldx     $04
        lda     CONST_ZERO,s
        sta     $f1,s
VAR_ZERO .var   $00
VAR_TWO .var    $02
VAR_THREE .var  $03
CONST_ZERO_VAR .var $f0
        ldy     VAR_ZERO
        lda     (VAR_ZERO+1),y
        sta     VAR_THREE
        ldx     $04
        lda     CONST_ZERO_VAR,s
        sta     $f1,s
        eor     0
        ora     240,s
PROJ_ZERO_DUP1 .var $10    ;clash with project symbol
DPCODE_DUP1 .var $80       ;clash with user label
        lda     VAR_ZERO
        lda     VAR_ZERO+1
        lda     VAR_TWO
        lda     VAR_THREE
        lda     $04
        lda     PROJ_ZERO_DUP1
        lda     $11
        lda     DPCODE
        ldx     PROJ_ZERO
        ldx     PROJ_ONE
        ldx     $02
        bit     $ffa9
        ldy     PROJ_ZERO
        ldy     PROJ_ONE
        ldy     $02
        .byte   $2c
NH0     .var    $00        ;not hidden
NH1     .var    $01        ;not hidden
L103C   lda     #$fe
        beq     L103C
        ldy     NH0
        ldy     NH1
        ldy     $02
        nop
PTR0    .var    $10
CONST0  .var    $10
        lda     PTR0
        ldx     PTR0+1
        ldy     $12
        lda     (CONST0,s),y
        sta     (CONST0+3,s),y
;Test name redefinition.  This is mostly of interest for assemblers without
;redefinable variables, but also of interest to the cross-reference window.
PTR     .var    $20        ;#1
        ldx     PTR
PTR     .var    $22        ;#2
        ldx     PTR
PTR     .var    $24        ;#3
        ldx     PTR
_PTR_1  nop
PTR_A   .var    $20
        ldy     PTR_A
PTR_B   .var    $1f
        ldy     PTR_B+1
PTR_C   .var    $1d
        ldy     PTR_C+3
PTR_D   .var    $21
        ldy     PTR_C+3
VAL0    .var    $30
VAL1    .var    $31
VAL2    .var    $32
VAL3    .var    $33
VAL4    .var    $34
VAL5    .var    $35
        and     VAL0
        and     VAL1
        and     VAL2
        and     VAL3
        and     VAL4
        and     VAL5
VAL14   .var    $31
        and     VAL0
        and     VAL14
        and     VAL14+1
        and     VAL14+2
        and     VAL14+3
        and     VAL5
DPNOP   .var    $80        ;same as org
        lda     DPCODE
        jsr     DPCODE
        rts

        .logical $0080
DPCODE  nop
        lda     DPCODE
        lda     @wDPCODE
        lda     @lDPCODE
_SPLIT1 lda     #','
SPLITTER .var   $80
        ldx     $1234
        beq     _SPLIT1
_SPLIT2 lda     ','
        ldx     $5678
        beq     _SPLIT2
        rts

        .here
