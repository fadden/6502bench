; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
; 6502 version

        org     $1000

        jsr     test1
        jsr     test3
        jsr     test4
        jsr     test5
        jmp     test6

; TEST #1: simple example
test1   lda     #$00
        dfb     $2c         ;BIT abs
:inner  lda     #$01
        beq     :inner
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
        bvs     :mid


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
test6
        dfb     $ad         ;lda
