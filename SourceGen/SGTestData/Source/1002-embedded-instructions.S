; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

; 65816 mode with short regs
        clc
        xce
        sep     #$30
        mx      %11

        jsr     test1
        jsr     test2
        jsr     test3
        jsr     test4
        jsr     test5
        rts

; TEST #1: simple example
test1   lda     #$00
        dfb     $2c         ;BIT abs
:inner  lda     #$01
        beq     :inner
        rts

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
test2   sep     #$30        ;short regs
        mx      %00         ;pretend they're long

        lda     $00         ;load something to scramble flags
        beq     :store
        lda     #$0000
:store  stal    $012345
        rts

; TEST #3: embedded with non-instruction byte
;
; The code analyzer sees two paths, involving the three bytes.
; The first is the three-byte JSR, the second is the one-byte
; RTS.  The third NOP byte is never "executed" by the analyzer,
; but because of the way we display embedded instructions it
; gets put on its own line.  Since it's not an instruction start
; or a data item, things get confused.  (This is referred to as
; an "embedded orphan" in the code.)

test3   dfb     $20         ;JSR
:mid    dfb     $60         ;RTS
        dfb     $ea         ;NOP
        bra     :mid


; TEST #4: overlapping chain
;
; Each BIT instruction is three bytes, and each byte is a branch target,
; so we get a string of embedded instructions.
test4
:bits   hex     2c2c2c2c2c2c2c2c2ceaea
        asl
        bcc     :bits
        asl
        bcc     :bits+1
        asl
        bcc     :bits+2
        asl
        bcc     :bits+3
        asl
        bcc     :bits+4
        asl
        bcc     :bits+5
        asl
        bcc     :bits+6
        asl
        bcc     :bits+7
        asl
        bcc     :bits+8
        asl
        bcc     :bits+9
        rts

; TEST #5: another overlap
;
; Trying to be a little different.
test5   dfb     $2c
:mid1   nop
        hex     ad
:mid2   lda     $00
        asl
        bcc     :mid1
        asl
        bcc     :mid2

; TEST #6: "embedded" off the end of the file
        dfb     $af         ;ldal

