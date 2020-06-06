; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; NOTE: configure for plain 6502

        org     $1000

; Test conflict with auto-label.
start   lda     #$00        ;do not label
:L1000  lda     #$01        ;EDIT: set label to :L1000 (dup of auto)
        ldx     start
        ldy     :L1000

; Test local/global having same name.
        ldx     #$02
loop1   dex                 ;EDIT
        bne     loop1

        ldx     #$03
:loop1  dex                 ;EDIT
        bne     :loop1

; Test nested loops, and ref to a non-unique local on the other side
; of a global.
global1 nop                 ;EDIT
        ldx     #$04
:loop1  ldy     #$05        ;EDIT: local, name "loop"

:loop2  dey                 ;EDIT: local, name "loop"
        bne     :loop2
        dex
        bne     :loop1
        jmp     btarg

global2 nop                 ;EDIT

btarg   nop                 ;EDIT: local, name "loop"

; Test hand-over-hand locals branching forward.
global3 nop                 ;EDIT
        ldx     #$06
        ldy     #$07
        dex
        beq     :fwd1
        dey
        beq     :fwd2
:fwd1   nop                 ;EDIT
:fwd2   nop                 ;EDIT

; Test loop with an unreferenced global in the middle.
global4 nop                 ;EDIT
        ldx     #$08
gloop   dex                 ;EDIT: local, name "loop"
global5 nop
        bne     gloop

        nop

; Test symbolic references.
; NOTE: start them as spin1/spin2/spin3, then rename spin3 to spin1
global6 nop
:spin1  jsr     :spin2      ;EDIT: local, name "spin1", operand ref to ":spin2"
:spin2  jsr     :spin1      ;EDIT: local, name "spin2", operand ref to ":spin1"
        nop
:spin3  lda     :spin3      ;EDIT: local, name "spin1", operand ref to ":spin1" (will be adjusted)
        beq     :spin3      ;EDIT: operand ref to ":spin1" (NOT adjusted)

        lda     #<:spin1
        ldx     #<:spin2
        lda     #>:spin1
        ldx     #>:spin2
        bne     :skip

        dw      :spin1      ;EDIT: local, name "spin1"
        dw      :spin2      ;EDIT: local, name "spin1" (will be offset)
        dw      :spin3      ;EDIT: local, name "spin1"

        dfb     <:spin1     ;EDIT: local, name "spin1" (may need to do as
        dfb     <:spin2     ;EDIT: local, name "spin1"  unique names and then
        dfb     >:spin1     ;EDIT: local, name "spin1"  rename afterward)
        dfb     >:spin2     ;EDIT: local, name "spin1"

:skip   nop                 ;EDIT: local


; Semi-related: test labels that are nothing but underscores.
global_ nop
_global ldx     #$40
__      dex
        bne     __
        beq     ___
___     ldx     #$41
:__     dex
        bne     :__

        nop

; Semi-related: test annotations (mostly to confirm that the suffix chars
; aren't appearing in the assembly output)
anno    lda     #$42        ;EDIT: add '?'
anno1   lda     anno        ;NOTE: do not label, let table gen do it
        clc
        bcc     :skip
        dw      anno1       ;EDIT: use table generator to get annotation
:skip   nop

; Semi-related: test opcode name labels (which are illegal for assemblers
; other than Merlin 32).  We're configured for plain 6502, so it should
; remap some but not others.
JMP     lda     JMP         ;EDIT set label (becomes JMP1 on non-Merlin)
JMP0    lda     JMP0        ;EDIT set label
JMP1    lda     JMP1        ;EDIT set label (becomes JMP11 on non-Merlin)
:JMP    lda     :JMP        ;EDIT set label
:JMP0   lda     :JMP0       ;EDIT set label
:JMP1   lda     :JMP1       ;EDIT set label
:JMP_   lda     :JMP_       ;EDIT set label :JMP (should become :JMP2)
jmp     lda     jmp         ;EDIT set label
Jmp     lda     Jmp         ;EDIT set label
BRA     lda     BRA         ;EDIT set label (should NOT be remapped)
brl     lda     brl         ;EDIT set label (should NOT be remapped)
LDAL    ldal    LDAL        ;EDIT set label (should NOT be remapped)

; Test local-variable interaction.  The goal here is to try to trick the
; code generator into creating duplicate labels.
global7 nop
]plain  equ     $11         ;EDIT create local var table
]_under1 equ    $12
]__dub1 equ     $13
        lda     ]plain
        lda     ]_under1
        lda     ]__dub1

:plain0 lda     :plain0     ;EDIT: set label :plain
plain1  lda     plain1      ;EDIT: set label :plain (should uniquify to plain)
global8 dex                 ;  (which will then clash with the local var)
        bne     plain1

X_under1 lda    X_under1    ;EDIT: set label (global) (trying to clash on 64tass)

:X__dub1 lda    :X__dub1    ;EDIT: set label

]_under1 equ    $22         ;EDIT: create local var table (don't clear)
        lda     ]plain
        lda     ]_under1

        rts
