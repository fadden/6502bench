;Edited to have duplicate labels (PROJ_ZERO, DPCODE).
         .setcpu "6502"
CONST_ZERO =     $f0        ;project const

PROJ_ZERO =      $00        ;project addr
PROJ_ONE =       $01        ;project addr

         .org    $1000
         ldy     PROJ_ZERO
         lda     (PROJ_ONE),y
         sta     $03        ;could be PROJ_ONE+2, but "nearby" is off
         ldx     $04
         lda     #CONST_ZERO
         ora     #$f1
VAR_ZERO .set    $00
VAR_TWO  .set    $02
VAR_THREE .set   $03
CONST_ZERO_VAR .set $f0
         ldy     VAR_ZERO
         lda     (VAR_ZERO+1),y
         sta     VAR_THREE
         ldx     $04
         lda     #$f0
         ora     #$f1
         eor     0
         ora     240
PROJ_ZERO_DUP1 .set $10     ;clash with project symbol
DPCODE_DUP1 .set $80        ;clash with user label
         lda     VAR_ZERO
         lda     VAR_ZERO+1
         lda     VAR_TWO
         lda     VAR_THREE
         lda     $04
         lda     PROJ_ZERO_DUP1
         lda     $11
         lda     z:DPCODE
         ldx     PROJ_ZERO
         ldx     PROJ_ONE
         ldx     $02
         bit     $ffa9
         ldy     PROJ_ZERO
         ldy     PROJ_ONE
         ldy     $02
         .byte   $2c
NH0      .set    $00        ;not hidden
NH1      .set    $01        ;not hidden
L103C:   lda     #$fe
         beq     L103C
         ldy     NH0
         ldy     NH1
         ldy     $02
         nop
PTR0     .set    $10
CONST0   .set    $10
         lda     PTR0
         ldx     PTR0+1
         ldy     $12
         lda     #$10
         ora     #$13
;Test name redefinition.  This is mostly of interest for assemblers without
;redefinable variables, but also of interest to the cross-reference window.
PTR      .set    $20        ;#1
         ldx     PTR
PTR      .set    $22        ;#2
         ldx     PTR
PTR      .set    $24        ;#3
         ldx     PTR
PTR_1:   nop
PTR_A    .set    $20
         ldy     PTR_A
PTR_B    .set    $1f
         ldy     PTR_B+1
PTR_C    .set    $1d
         ldy     PTR_C+3
PTR_D    .set    $21
         ldy     PTR_C+3
VAL0     .set    $30
VAL1     .set    $31
VAL2     .set    $32
VAL3     .set    $33
VAL4     .set    $34
VAL5     .set    $35
         and     VAL0
         and     VAL1
         and     VAL2
         and     VAL3
         and     VAL4
         and     VAL5
VAL14    .set    $31
         and     VAL0
         and     VAL14
         and     VAL14+1
         and     VAL14+2
         and     VAL14+3
         and     VAL5
DPNOP    .set    $80        ;same as org
         lda     z:DPCODE
         jsr     DPCODE
         rts

         .org    $0080
DPCODE:  nop
         lda     DPCODE
         lda     a:DPCODE
         nop
         ora     a:DPCODE
SPLIT1:  lda     #','
SPLITTER .set    $80
         ldx     $1234
         beq     SPLIT1
@SPLIT2: lda     ','
         ldx     $5678
         beq     @SPLIT2
         rts

