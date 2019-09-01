;Would need to be edited to test duplicate labels.  Those currently cause
;assembly to fail (see the X_* vars).
        !cpu    65816
PROJ_ZERO =     $00        ;project addr
PROJ_ONE =      $01        ;project addr
CONST_ZERO =    $f0        ;project const

*       =       $1000
        !as
        !rs
        ldy     PROJ_ZERO
        lda     (PROJ_ONE),y
        sta     $03
        ldx     $04
        lda     CONST_ZERO,S
        sta     $f1,S
VAR_ZERO =      $00
VAR_TWO =       $02
VAR_THREE =     $03
CONST_ZERO_VAR = $f0
        ldy     VAR_ZERO
        lda     (VAR_ZERO+1),y
        sta     VAR_THREE
        ldx     $04
        lda     CONST_ZERO_VAR,S
        sta     $f1,S
        eor     0
        ora     240,S
X_PROJ_ZERO =   $10        ;clash with project symbol
X_DPCODE =      $80        ;clash with user label
        lda     VAR_ZERO
        lda     VAR_ZERO+1
        lda     VAR_TWO
        lda     VAR_THREE
        lda     $04
        lda     X_PROJ_ZERO
        lda     $11
        lda+1   DPCODE
        ldx     PROJ_ZERO
        ldx     PROJ_ONE
        ldx     $02
        bit     $ffa9
        ldy     PROJ_ZERO
        ldy     PROJ_ONE
        ldy     $02
        !byte   $2c
NH0     =       $00        ;not hidden
NH1     =       $01        ;not hidden
L103C   lda     #$fe
        beq     L103C
        ldy     NH0
        ldy     NH1
        ldy     $02
        nop
PTR0    =       $10
CONST0  =       $10
        lda     PTR0
        ldx     PTR0+1
        ldy     $12
        lda     (CONST0,S),y
        sta     (CONST0+3,S),y
;Test name redefinition.  This is mostly of interest for assemblers without
;redefinable variables, but also of interest to the cross-reference window.
PTR     =       $20        ;#1
        ldx     PTR
PTR_3   =       $22        ;#2
        ldx     PTR_3
PTR_4   =       $24        ;#3
        ldx     PTR_4
PTR_1   nop
PTR_A   =       $20
        ldy     PTR_A
PTR_B   =       $1f
        ldy     PTR_B+1
PTR_C   =       $1d
        ldy     PTR_C+3
PTR_D   =       $21
        ldy     PTR_C+3
VAL0    =       $30
VAL1    =       $31
VAL2    =       $32
VAL3    =       $33
VAL4    =       $34
VAL5    =       $35
        and     VAL0
        and     VAL1
        and     VAL2
        and     VAL3
        and     VAL4
        and     VAL5
VAL14   =       $31
        and     VAL0
        and     VAL14
        and     VAL14+1
        and     VAL14+2
        and     VAL14+3
        and     VAL5
DPNOP   =       $80        ;same as org
        lda+1   DPCODE
        jsr     DPCODE
        rts

        !pseudopc $0080 {
DPCODE  nop
        lda+1   DPCODE
        lda+2   DPCODE
        lda+3   DPCODE
        rts

        } ;!pseudopc
