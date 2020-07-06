; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Merlin 32 link file.
;
; According to the documentation we can use ORG directives to set the segment
; load addresses, but in practice Merlin 32 rejects them.  Also, DS
; directives are ignored, and KND uses a different default value than
; what the documentation says.
;
; cf. https://github.com/apple2accumulator/merlin32/issues/39
;
; Instructions:
; - Assemble this file with Merlin32.  It will pull everything in.
; - Open out.20212 with the segment viewer and find the start offsets
;   of the segments.  Set the ORG field with a hex editor.  It's a
;   4-byte field at +$18.  (Easy way in a v2.0 header: find the start
;   of the SEGNAME and back up $1e bytes.)
;    "PosFFE0" -> E0 FF 00
;    "Bank2"   -> 56 34 02
;    "Bank8"   -> 00 10 08
; - Convert out.20212 to "20212-reloc-data" with OMF converter.
; - The project file generally requires no edits, except to delete the
;   header comment.  Leaving the segment-start comments in seems fine.

        DSK     out.20212           ;output file name, must be ProDOS-compat
        TYP     $b3                 ;S16
        XPL                         ;add ~ExpressLoad

; Segment #1
        ASM     20212-reloc-data-main.S
        KND     $1000               ;Code; NoSpec
;        SNA     <default>


; Segment #2
        ASM     20212-reloc-data-relover.S
        KND     $8101               ;Data; Dynamic, BankRel
        ALI     None
;        BSZ     0
;        ORG     $00ffe0             ;load in any bank, at $ffe0
        SNA     PosFFE0

; Segment #3
        ASM     20212-reloc-data-bank2.S
        KND     $1000
        DS      256
;        ORG     $023456             ;load exactly here
        SNA     Bank2

; Segment #4
        ASM     20212-reloc-data-bank8.S
        KND     $8800               ;Code; Dynamic, AbsBank
;        ORG     $081000             ;load in bank 8, at any address
        SNA     Bank8

; Segment #5
        ASM     20212-reloc-data-filler.S
        KND     $0001               ;Data
        SNA     Filler
