        .cpu    "6502"
*       =       $0800
        jsr     region1
        jsr     region2
        jsr     $3000
        jsr     $4000
        lda     inner1
        lda     inner2
        lda     $b000
        lda     $c000
self    .byte   $ad,$ea
L081A   nop
        jsr     altbnk1
        jsr     altbnk2
        jmp     done

altbnk1 bit     $c080
        lda     self+1
        bne     L081A
        rts

        .logical *+$0000
altbnk2 bit     $c080
        lda     $0819
        bne     $081a
        ldx     $081b
        rts

        .here
done    nop
        rts

        .fill   197,$00

        .logical $1000
region1x lda    region1x
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        nop
        rts

        .fill   202,$00
        .here

        .logical $1000
region1 lda     region1
        pha
        ldy     #$11
        nop
_copy   lda     inner1_pre,y
        sta     inner1,y
        dey
        bpl     _copy
        bit     $101d
        jsr     inner1
        jmp     finish1

inner1_pre
        .logical $9000
inner1  ldx     inner1
        ldy     #$aa
        ldy     finish1
        ldy     finish2
        ldy     $302b
        ldy     $402b
        rts

        .here
finish1 ldy     finish1
        ldx     region1
        ldx     region2
        ldx     $3000
        ldx     $4000
        lda     inner1
        lda     inner2
        lda     $b000
        lda     $c000
        pla
        rts

        .fill   184,$00
        .here

        .logical $2000
region2 lda     region2
        pha
        ldy     #$11
        nop
_copy   lda     inner2_pre,y
        sta     inner2,y
        dey
        bpl     _copy
        bit     $201d
        jsr     inner2
        jmp     finish2

inner2_pre
        .logical $a000
inner2  ldx     inner2
        ldy     #$aa
        ldy     $102b
        ldy     finish2
        ldy     $302b
        ldy     $402b
        rts

        .here
finish2 ldy     finish2
        ldx     $1000
        ldx     region2
        ldx     $3000
        ldx     $4000
        pla
        rts

        .fill   196,$00
        .here

        .logical $3000
region3 lda     region3
        pha
        ldy     #$11
        nop
_copy   lda     inner3_pre,y
        sta     inner3,y
        dey
        bpl     _copy
        bit     $301d
        jsr     inner3
        jmp     finish3

inner3_pre
        .logical $b000
inner3  ldx     inner3
        ldy     #$aa
        ldy     finish1
        ldy     finish2
        ldy     finish3
        ldy     $402b
        rts

        .here
finish3 ldy     finish3
        ldx     region1
        ldx     region2
        ldx     region3
        ldx     $4000
        pla
        rts

        .fill   196,$00
        .here

        .logical $4000
region4 lda     region4
        pha
        ldy     #$11
        nop
_copy   lda     inner4_pre,y
        sta     inner4,y
        dey
        bpl     _copy
        bit     $401d
        jsr     inner4
        jmp     finish4

inner4_pre
        .logical $c000
inner4  ldx     inner4
        ldy     #$aa
        ldy     $102b
        ldy     $202b
        ldy     $302b
        ldy     finish4
        rts

        .here
finish4 ldy     finish4
        ldx     $1000
        ldx     $2000
        ldx     $3000
        ldx     region4
        pla
        rts

        .fill   196,$00
        .here
        .logical $0000
        .byte   $ff
        .here
