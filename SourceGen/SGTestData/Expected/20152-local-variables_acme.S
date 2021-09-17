;Edited to have duplicate labels (PROJ_ZERO, DPCODE).
        !cpu    65816
CONST_ZERO =    $f0        ;project const

PROJ_ZERO =     $00        ;project addr
PROJ_ONE =      $01        ;project addr

*       =       $1000
        !as
        !rs
        ldy     PROJ_ZERO
        lda     (PROJ_ONE),y
        sta     $03        ;could be PROJ_ONE+2, but "nearby" is off
        ldx     $04
        lda     CONST_ZERO,S
        sta     $f1,S
        !zone   Z00000c
.VAR_ZERO =     $00
.VAR_TWO =      $02
.VAR_THREE =    $03
.CONST_ZERO_VAR = $f0
        ldy     .VAR_ZERO
        lda     (.VAR_ZERO+1),y
        sta     .VAR_THREE
        ldx     $04
        lda     .CONST_ZERO_VAR,S
        sta     $f1,S
        eor     0
        ora     240,S
        !zone   Z00001c
.VAR_ZERO =     $00
.VAR_TWO =      $02
.VAR_THREE =    $03
.PROJ_ZERO_DUP1 = $10      ;clash with project symbol
.DPCODE_DUP1 =  $80        ;clash with user label
.CONST_ZERO_VAR = $f0
        lda     .VAR_ZERO
        lda     .VAR_ZERO+1
        lda     .VAR_TWO
        !zone   Z000022
.VAR_ZERO =     $00
.VAR_TWO =      $02
.VAR_THREE =    $03
.PROJ_ZERO_DUP1 = $10      ;clash with project symbol
.DPCODE_DUP1 =  $80        ;clash with user label
.CONST_ZERO_VAR = $f0
        lda     .VAR_THREE
        lda     $04
        lda     .PROJ_ZERO_DUP1
        lda     $11
        lda+1   DPCODE
        !zone   Z00002c
        ldx     PROJ_ZERO
        ldx     PROJ_ONE
        ldx     $02
        bit     $ffa9
        ldy     PROJ_ZERO
        ldy     PROJ_ONE
        ldy     $02
        !byte   $2c
        !zone   Z00003c
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
L103C   lda     #$fe
        beq     L103C
        ldy     .NH0
        ldy     .NH1
        ldy     $02
        nop
        !zone   Z000047
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
        lda     .PTR0
        ldx     .PTR0+1
        ldy     $12
        lda     (.CONST0,S),y
        sta     (.CONST0+3,S),y
;Test name redefinition.  This is mostly of interest for assemblers without
;redefinable variables, but also of interest to the cross-reference window.
        !zone   Z000051
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR    =       $20        ;#1
        ldx     .PTR
        !zone   Z000053
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR    =       $22        ;#2
        ldx     .PTR
        !zone   Z000055
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR    =       $24        ;#3
        ldx     .PTR
@PTR_1  nop
        !zone   Z000058
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_A  =       $20
.PTR    =       $24        ;#3
        ldy     .PTR_A
        !zone   Z00005a
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_B  =       $1f
.PTR    =       $24        ;#3
        ldy     .PTR_B+1
        !zone   Z00005c
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR    =       $24        ;#3
        ldy     .PTR_C+3
        !zone   Z00005e
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.PTR    =       $24        ;#3
        ldy     .PTR_C+3
        !zone   Z000060
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.PTR    =       $24        ;#3
.VAL0   =       $30
.VAL1   =       $31
.VAL2   =       $32
.VAL3   =       $33
.VAL4   =       $34
.VAL5   =       $35
        and     .VAL0
        and     .VAL1
        and     .VAL2
        and     .VAL3
        and     .VAL4
        and     .VAL5
        !zone   Z00006c
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.PTR    =       $24        ;#3
.VAL0   =       $30
.VAL14  =       $31
.VAL5   =       $35
        and     .VAL0
        and     .VAL14
        and     .VAL14+1
        and     .VAL14+2
        and     .VAL14+3
        and     .VAL5
        !zone   Z000078
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.PTR    =       $24        ;#3
.VAL0   =       $30
.VAL14  =       $31
.VAL5   =       $35
.DPNOP  =       $80        ;same as org
        lda+1   DPCODE
        jsr     DPCODE
        rts

        !pseudopc $0080 {
DPCODE  nop
        lda+1   DPCODE
        lda+2   DPCODE
        lda+3   DPCODE
@SPLIT1 lda     #','
        !zone   Z00008a
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.PTR    =       $24        ;#3
.VAL0   =       $30
.VAL14  =       $31
.VAL5   =       $35
.SPLITTER =     $80
        ldx     $1234
        beq     @SPLIT1
@SPLIT2 lda     ','
        !zone   Z000091
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.PTR    =       $24        ;#3
.VAL0   =       $30
.VAL14  =       $31
.VAL5   =       $35
.SPLITTER =     $80
        ldx     $5678
        beq     @SPLIT2
        nop
        clc
        xce
        rep     #$30
        !al
        !rl
        pea     STRING >> 16
        pea     STRING
        jsl     STRLEN
        sec
        xce
        !as
        !rs
        jmp     L00E6

STRING  !text   "testing"
        !byte   $00

        !zone   Z0000b2
.NH0    =       $00        ;not hidden
.NH1    =       $01        ;not hidden
.PTR0   =       $10
.CONST0 =       $10
.PTR_C  =       $1d
.PTR_D  =       $21
.VAL0   =       $30
.VAL14  =       $31
.VAL5   =       $35
.SPLITTER =     $80
.LEN    =       $f2
.PTR    =       $f4
.ARG    =       $fd
        !al
        !rl
STRLEN  phd
        tsc
        sec
        sbc     #$00f7
        tcd
        adc     #$00f0
        tcs
        stz     .LEN
@L00C1  ldx     .ARG+2
        lda     .ARG
        inc     .ARG
        bne     @L00CB
        inc     .ARG+2
@L00CB  sta     .PTR
        stx     .PTR+2
        lda     [.PTR]
        and     #$00ff
        beq     @L00DA
        inc     .LEN
        bra     @L00C1

@L00DA  lda     .LEN
        tay
        tdc
        clc
        adc     #$00f7
        tcs
        tya
        pld
        rtl

        !as
        !rs
L00E6   bit     .ARG+2
        bit+2   $00ff
        bit     $0100
        rts

        } ;!pseudopc
