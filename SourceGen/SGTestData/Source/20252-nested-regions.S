; Copyright 2021 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Test nested address regions.
;
; Assembler: 64tass
; % tass64 --ascii --case-sensitive --nostart 20252-nested-regions.S

        .cpu    "65816"
*       =       $081000
START

; Start with 3 nested regions that share a starting address.

; EDIT: create these as fixed-end regions; first one spans full file
        .logical $011000
        .logical $021000
        .logical $031000

part3k  lda     @lpart3k
early   lda     early
        and     late
        jmp     part2k

        .here               ;031000

part2k  lda     @lpart2k
        jmp     part1k

        .here               ;021000

part1k  lda     @lpart1k
        jmp     end

late    .byte   $80

; EIDT: make this a non-addressable region
; EDIT: put a code start tag here
tag     nop
        rts
        .dword  early

end
        nop
        rts
REGION2_END
        .here               ;$011000
