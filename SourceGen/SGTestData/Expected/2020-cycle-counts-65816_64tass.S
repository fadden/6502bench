        .cpu    "65816"
*       =       $10f0
        .as
        .xs
L10F0   sec                ;2
        xce                ;2
        bra     L1108      ;4  crosses page boundary

        .fill   20,$00

L1108   bra     L110A      ;3

L110A   lda     #$00       ;2
        beq     L1110      ;3  branch always

        .byte   $00
        .byte   $00

L1110   bne     L1110      ;2  branch never
        lda     $1234      ;4
        beq     L1119      ;2+ branch maybe
        brk                ;7  BRK with E=1

        .byte   $01

L1119   lda     $22,s      ;4
        trb     $02        ;5+
        asl     $03,x      ;6+
        clc                ;2
        xce                ;2
        rep     #$30       ;3
        .al
        .xl
        lda     #$0011     ;3
        ldy     #$0022     ;3
        trb     $04        ;7+
        asl     $05,x      ;8+
        ldx     $1235      ;5
        beq     L1134      ;2+
        brk                ;8  BRK with E=0

        .byte   $02

L1134   lda     $33,s      ;5
        beq     L113A      ;2+
        bra     L10F0      ;3

L113A   rep     #$20       ;3
        sep     #$10       ;3
        .xs
        sta     $10        ;4+
        stx     $11        ;3+
        sty     $12        ;3+
        rep     #$10       ;3
        .xl
        sep     #$20       ;3
        .as
        sta     $11        ;3+
        stx     $12        ;4+
        sty     $13        ;4+
        rep     #$30       ;3
        .al
        lda     $1234      ;5
        adc     #$0066     ;3
        adc     $1235      ;5
        sed                ;2  should have no effect on cycle counts on 65816
        adc     #$0077     ;3
        adc     $1236      ;5
        sec                ;2
        xce                ;2
        .as
        .xs
        sbc     #$88       ;2
        sbc     $1237      ;4
        cld                ;2
        sbc     #$99       ;2
        sbc     $1238      ;4
        rts                ;6

