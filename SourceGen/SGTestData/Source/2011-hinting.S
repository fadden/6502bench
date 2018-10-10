; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; NOTE: set assembler to 6502, undocumented instructions disabled
; NOTE: add 2011-hinting.cs to extension scripts

; It may be necessary to review the analyzer output to verify certain things.

MAGIC   equ     $2456           ;recognized by extension script

        org     $1000

        dfb     $03             ;undocumented (SLO), will halt analyzer
        dfb     $02

entry   bit     entry           ;EDIT: add code hint to first byte
        bit     $11a9           ;EDIT: add code hint to middle byte (a9); creates embedded instr

        nop
embed   dfb     $2c
innerd  ldx     #$ff            ;EDIT: add data hint to middle byte (a2); will be ignored
        nop
        jsr     innerd          ;analyzer should follow this path

        nop
        dfb     $2c
inneri  ldx     #$ff            ;EDIT: add inline data hint to middle byte (a2); will be ignored
        nop
        jsr     inneri          ;analyzer should follow this path

        jsr     MAGIC           ;next 4 bytes are inline data tagged by the
magic11 lda     #$11            ; extension script -- if they show up as
        ldx     #$22            ; LDA/LDX, the script isn't working

        jsr     magic11         ;analyzer should ignore this path

        jsr     magic33         ;analyzer will add this path, but the target will be inline
        jsr     MAGIC           ; data before it gets to it
magic33 lda     #$33
        ldx     #$44

; We want to call into part2 before part1 to establish that part2 is code before the
; extension script tries to change it to inline data.  The code analyzer currently uses
; a stack, so we call part 1 first, which means it will be called last.
        jsr     part1
        jsr     part2

        nop

        lda     MAGIC           ;EDIT: hint as data, format as dense hex, remove hint

        rts


part1   jsr     MAGIC
part2   lda     #$55
        ldx     #$66
        rts

