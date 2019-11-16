; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

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

; Test loop with a global in the middle.
global4 nop                 ;EDIT
        ldx     #$08
gloop   dex                 ;EDIT: local, name "loop"
global5 nop
        bne     gloop
        
        nop

; Test symbolic references.

global6 nop
:spin1  jsr     :spin2      ;EDIT: local, name "spin1", operand ref to ":spin2"
:spin2  jsr     :spin1      ;EDIT: local, name "spin2", operand ref to ":spin1"
        nop
:spin3  lda     :spin3      ;EDIT: local, name "spin1", operand ref to ":spin1"

; Semi-related: test labels that are nothing but underscores.
global_ nop
        ldx     #$40
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
anno1   lda     anno


        rts

        dw      anno1       ;EDIT: use table generator

