; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
; 65816 version

        org     $1000

        jsr     test2
        jmp     test6

; TEST #2: embedded with break path
;
; Example inspired by incorrect analysis...
;
; The code analyzer sees:
;   beq {+03}   ;jumps to the $8f
;   lda #$00
;   brk $8f
; and stops, then pursues the branch.  If we try to walk from top
; to bottom, skipping forward by the full length of an instruction,
; we'll appear to find ourselves in the middle of an embedded
; instruction.
;
; This is different from the typical embedded instruction,
; where the inner is contained entirely within the outer.
;
; (Not sure this is interesting anymore with default 1-byte BRK.)
test2   sep     #$30        ;short regs
        mx      %00         ;pretend they're long

        lda     $00         ;load something to scramble flags
        beq     :store1
        lda     #$0000
:store1 stal    $012345

; Try it a different way.  This creates two streams of execution.
        lda     $00
        beq     :store2
        lda     #$8500
:store2 stal    $012345
        nop
        rts

; TEST #6: "embedded" off the end of the file
test6
        dfb     $af         ;ldal
