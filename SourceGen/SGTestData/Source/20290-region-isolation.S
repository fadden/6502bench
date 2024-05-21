; Copyright 2024 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: 64tass
; % tass64 --ascii --case-sensitive --nostart 20290-region-isolation.S
;
; This is pretending to be a multi-bank ROM, with 256 bytes per bank.

        .cpu    "6502"

;
; Initial region.  This should end before region1, rather than span
; all sub-regions, so that we're exercising top-level regions for
; regions 1-4.
;
*       =       $0800
BANK    =       $ffc0
; EDIT: create project symbol THREE_K for $3000
        jsr     region1
        jsr     region2
        jsr     region3
        jsr     region4

; EDIT: create project symbols for $8000/9000/a000/b000, len=256
; These should resolve to the user labels (for 1/2) or the project
; symbols (for 3/4).  We use an offset from the base address to ensure
; that the range is being taken into account.
        lda     inner1a
        lda     inner2a
        lda     inner3a
        lda     inner4a

; Local reference, can use "do not follow" to avoid mid-instruction branch.
self    lda     $eaea

call    jsr     altbnk1
        jsr     altbnk2

        jmp     done

; These pretend to be code that switches the ROM bank, so the code
; after the BIT instruction is actually in a different ROM, and the
; BNE goes to something else.

; EDIT: mark individual BNE as "do not follow"
altbnk1 bit     BANK            ;e.g. trigger a ROM bank switch
        lda     self+1
        bne     self+2
        rts

; EDIT: put this in a separate region, mark "disallow outbound"
altbnk2 bit     BANK 
        lda     self+1
        bne     self+2
        ldx     call            ;EDIT: set symbol explicitly, should work
        rts

done    nop
        rts

        .align  256

;
; region 1x: fully closed
;
; This overlaps with region 1.  By closing it we should prevent any
; of the external references from resolving here.
;
        .logical $1000
region1x lda    region1x
        .fill   50,$ea
        rts

        .align 256
        .endlogical

;
; region 1: fully open
;
        .logical $1000              ;*
region1 lda     region1             ;*
        pha

        ldy     #inner1_end-inner1-1 ;*
        nop
_copy   lda     inner1_pre,y        ;*
        sta     inner1,y            ;*
        dey
        bpl     _copy
        bit     inner1_pre+4        ;* should be an unresolved hole
        jsr     inner1              ;*
        jmp     finish1             ;*

; relocated inner chunk
inner1_pre                          ;*
        .logical region1+$8000      ;*
inner1  ldx     inner1              ;*
        ldy     #$aa
inner1a ldy     finish1
        ldy     finish2
        ldy     finish3
        ldy     finish4
        rts
inner1_end                          ;*
        .endlogical

finish1 ldy     finish1             ;*
        ldx     region1
        ldx     region2
        ldx     region3
        ldx     region4
        lda     inner1
        lda     inner2
        lda     inner3
        lda     inner4
        pla
        rts

        .align  256         ;pad chunk to 256 bytes
        .endlogical

;
; region 2: disallow outbound
;
        .logical $2000
region2 lda     region2
        pha

        ldy     #inner2_end-inner2-1 ;*
        nop
_copy   lda     inner2_pre,y        ;*
        sta     inner2,y            ;*
        dey
        bpl     _copy
        bit     inner2_pre+4        ;* should be an unresolved hole
        jsr     inner2              ;*
        jmp     finish2             ;*

; relocated inner chunk
inner2_pre                          ;*
        .logical region2+$8000      ;*
inner2  ldx     inner2              ;*
        ldy     #$aa
        ldy     finish1
inner2a ldy     finish2
        ldy     finish3
        ldy     finish4
        rts
inner2_end                          ;*
        .endlogical

finish2 ldy     finish2             ;*
        ldx     region1
        ldx     region2
        ldx     region3
        ldx     region4
        lda     inner1
        lda     inner2
        lda     inner3
        lda     inner4
        pla
        rts

        .align  256         ;pad chunk to 256 bytes
        .endlogical

;
; region 3: disallow inbound
;
        .logical $3000
region3 lda     region3
        pha

        ldy     #inner3_end-inner3-1 ;*
        nop
_copy   lda     inner3_pre,y        ;*
        sta     inner3,y            ;*
        dey
        bpl     _copy
        bit     inner3_pre+4        ;* should be an unresolved hole
        jsr     inner3              ;*
        jmp     finish3             ;*

; relocated inner chunk
inner3_pre                          ;*
        .logical region3+$8000      ;*
inner3  ldx     inner3              ;*
        ldy     #$aa
        ldy     finish1
        ldy     finish2
inner3a ldy     finish3
        ldy     finish4
        rts
inner3_end                          ;*
        .endlogical

finish3 ldy     finish3             ;*
        ldx     region1
        ldx     region2
        ldx     region3
        ldx     region4
        lda     inner1
        lda     inner2
        lda     inner3
        lda     inner4
        pla
        rts

        .align  256         ;pad chunk to 256 bytes
        .endlogical

;
; region 4: disallow both
;
        .logical $4000
region4 lda     region4
        pha

        ldy     #inner4_end-inner4-1 ;*
        nop
_copy   lda     inner4_pre,y        ;*
        sta     inner4,y            ;*
        dey
        bpl     _copy
        bit     inner4_pre+4        ;* should be an unresolved hole
        jsr     inner4              ;*
        jmp     finish4             ;*

; relocated inner chunk
inner4_pre                          ;*
        .logical region4+$8000      ;*
inner4  ldx     inner4              ;*
        ldy     #$aa
        ldy     finish1
        ldy     finish2
        ldy     finish3
inner4a ldy     finish4
        rts
inner4_end                          ;*
        .endlogical

finish4 ldy     finish4             ;*
        ldx     region1
        ldx     region2
        ldx     region3
        ldx     region4
        lda     inner1
        lda     inner2
        lda     inner3
        lda     inner4
        pla
        rts

        .align  256         ;pad chunk to 256 bytes
        .endlogical

; Not sure how to force asm to output alignment padding at end of file.
; Leave this marked as non-addressable.
        .byte   $ff
