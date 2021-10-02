; Copyright 2021 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Test nested address regions.
;
; Assembler: 64tass
; % tass64 --ascii --case-sensitive --nostart 20250-nested-regions.S

        .cpu    "6502"
*       =       $1000
START
        .word   $3000

; NOTE: leave PRG header as a 2-byte hole with no explicit region

; Start with 3 nested regions that share a starting address.

; EDIT: create region starting at $1000, ending at REGION_END
; EDIT: make all 3 of these relative (especially the first one, which follows non-addr)


; EDIT: create these as fixed-end regions; first one spans full file
        .logical $1000
        .logical $2000
        .logical $3000

part3k  bit     part3k
early   lda     early
        and     late
        jmp     part2k

        .here               ;$3000; now $20xx

part2k  bit     part2k
        jmp     part1k

        .here               ;$2000; now $10xx

part1k  bit     part1k
        jsr     overhole    ;let execution try to fall into non-addr

REGION1_END

; EDIT: next chunk is a *floating* non-addr region

        brk
        .null   "Null-term PETSCII string"
        .byte   $80
        .word   early
        .word   late
        .byte   $80

; 4000-40xx, with two overlapping regions in the $5000 area.  Tests order
; of resolution and descent into children.  (This is mostly tested internally
; by the AddressMap unit tests, so we don't need to go crazy here.)

        .logical $4000
overhole
a4000   bit     a4000
        bit     b5000
        bit     b500f       ;CHECK: resolves to b500f
        bit     c500f       ;CHECK: also resolves to b500f
        nop
        jmp     a4020

; EDIT: set region, add code start tag
        .logical $5000
b5000   bit     b5000
b5003   bit     a4000
        nop
        nop
b5008   bit     b5008       ;CHECK: resolves locally
        bit     c5017       ;CHECK: resolves to other segment
        nop
b500f   rts
        .here               ;$5000, now at $4010

a4020   bit     a4020
        bit     c500f
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        jmp     a4040

; EDIT: set region, add code start tag
        .logical $5008
c5008   bit     c5008       ;CHECK: resolves locally
        bit     b5000       ;CHECK: resolves to other segment
        nop
c500f   bit     c500f
        nop
        nop
        nop
        nop
        nop
c5017   rts
        .here               ;$5008

a4040   bit     a4040
        bit     c5017
        nop
        jmp     tailend
        .here               ;$4000


; EDIT: create region starting at $d000, ending at REGION2_END; relative
        .logical $d000
tailend
partdk  bit     part2k
late    nop
        jmp     partek

; EDIT: create
        .logical $e000

partek  bit     partek
        jmp     partfk

; EDIT: create, make it floating
        .logical $f000

partfk  bit     partfk
        lda     early
        and     late

end
        nop
        rts
REGION2_END
        .here               ;$f000
        .here               ;$e000
        .here               ;$d000
        .here               ;$1000
