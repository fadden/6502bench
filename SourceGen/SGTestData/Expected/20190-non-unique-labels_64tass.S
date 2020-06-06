        .cpu    "6502"
*       =       $1000
L1000   lda     #$00
_L1000  lda     #$01
        ldx     L1000
        ldy     _L1000
        ldx     #$02
loop1   dex
        bne     loop1
        ldx     #$03
_loop1  dex
        bne     _loop1
global1 nop
        ldx     #$04
_loop   ldy     #$05
_loop1  dey
        bne     _loop1
        dex
        bne     _loop
        jmp     loop

global2 .byte   $ea

loop    nop
global3 nop
        ldx     #$06
        ldy     #$07
        dex
        beq     _fwd1
        dey
        beq     _fwd2
_fwd1   nop
_fwd2   nop
global4 nop
        ldx     #$08
loop2   dex
global5 nop
        bne     loop2
        nop
global6 nop
_spin1  jsr     _spin2
_spin2  jsr     _spin1
        nop
_spin11 lda     _spin1+7
        beq     _spin11
        lda     #<_spin1
        ldx     #<_spin2
        lda     #>_spin1
        ldx     #>_spin2
        bne     _skip

        .word   _spin1
        .word   _spin2
        .word   _spin11
        .byte   <_spin1
        .byte   <_spin2
        .byte   >_spin1
        .byte   >_spin2

_skip   nop
global_ nop
X_global ldx    #$40
X__     dex
        bne     X__
        beq     X___

X___    ldx     #$41
_X__    dex
        bne     _X__
        nop
anno    lda     #$42
T106B   lda     anno
        clc
        bcc     _skip

        .word   T106B

_skip   nop
JMP1    lda     JMP1
JMP0    lda     JMP0
JMP11   lda     JMP11
_JMP    lda     _JMP
_JMP0   lda     _JMP0
_JMP1   lda     _JMP1
_JMP2   lda     _JMP2
jmp1    lda     jmp1
Jmp1    lda     Jmp1
BRA     lda     BRA
brl     lda     brl
LDAL    .byte   $af
        .byte   $95
        .byte   $10
        .byte   $00
        nop
plain_DUP1 .var $11
X_under1_DUP1 .var $12
X__dub1 .var    $13
        lda     plain_DUP1
        lda     X_under1_DUP1
        lda     X__dub1
_plain  lda     _plain
plain   lda     plain
global8 dex
        bne     plain
X_under1 lda    X_under1
_X__dub1 lda    _X__dub1
X_under1_DUP1 .var $22
        lda     plain_DUP1
        lda     X_under1_DUP1
        rts

