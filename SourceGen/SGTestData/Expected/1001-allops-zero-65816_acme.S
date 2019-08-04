        !cpu    65816
*       =       $1000
        !as
        !rs
        sec
        xce
        jsr     L101F
        jsr     L10AB
        jsr     L10F2
        jsr     L1106
        jsr     L1109
        jsr     L112C
        jsr     L11F9
        jsr     L11FC
        nop
        nop
        nop
        brk

        !byte   $00

L101F   ora     ($00,x)
        cop     $00
        ora     $00,S
        tsb     $00
        ora     $00
        asl     $00
        ora     [$00]
        php
        ora     #$00
        asl
        phd
        tsb+2   $0000
        ora+2   $0000
        asl+2   $0000
        ora+3   $000000
        bpl     L1041
L1041   ora     ($00),y
        ora     ($00)
        ora     ($00,S),y
        trb     $00
        ora     $00,x
        asl     $00,x
        ora     [$00],y
        clc
        ora     $0000,y
        inc
        tcs
        trb+2   $0000
        ora+2   $0000,x
        asl+2   $0000,x
        ora+3   $000000,x
        jsr     $0000
        and     ($00,x)
        jsl     $000000
        and     $00,S
        bit     $00
        and     $00
        rol     $00
        and     [$00]
        plp
        and     #$00
        rol
        pld
        bit+2   $0000
        and+2   $0000
        rol+2   $0000
        and+3   $000000
        bmi     L1089
L1089   and     ($00),y
        and     ($00)
        and     ($00,S),y
        bit     $00,x
        and     $00,x
        rol     $00,x
        and     [$00],y
        sec
        and     $0000,y
        dec
        tsc
        bit+2   $0000,x
        and+2   $0000,x
        rol+2   $0000,x
        and+3   $000000,x
        rti

L10AB   eor     ($00,x)
        !byte   $42,$00
        eor     $00,S
        mvp     $00,$00
        eor     $00
        lsr     $00
        eor     [$00]
        pha
        eor     #$00
        lsr
        phk
        jmp     L10C2

L10C2   eor+2   $0000
        lsr+2   $0000
        eor+3   $000000
        bvc     L10CE
L10CE   eor     ($00),y
        eor     ($00)
        eor     ($00,S),y
        mvn     $00,$00
        eor     $00,x
        lsr     $00,x
        eor     [$00],y
        cli
        eor     $0000,y
        phy
        tcd
        jml     L10E7

L10E7   eor+2   $0000,x
        lsr+2   $0000,x
        eor+3   $000000,x
        rts

L10F2   adc     ($00,x)
        per     $0ff6
        adc     $00,S
        stz     $00
        adc     $00
        ror     $00
        adc     [$00]
        pla
        adc     #$00
        ror
        rtl

L1106   jmp     ($0000)

L1109   adc+2   $0000
        ror+2   $0000
        adc+3   $000000
        bvs     L1115
L1115   adc     ($00),y
        adc     ($00)
        adc     ($00,S),y
        stz     $00,x
        adc     $00,x
        ror     $00,x
        adc     [$00],y
        sei
        adc     $0000,y
        ply
        tdc
        jmp     ($0000,x)

L112C   adc+2   $0000,x
        ror+2   $0000,x
        adc+3   $000000,x
        bra     L1138

L1138   sta     ($00,x)
        brl     L113D

L113D   sta     $00,S
        sty     $00
        sta     $00
        stx     $00
        sta     [$00]
        dey
        bit     #$00
        txa
        phb
        sty+2   $0000
        sta+2   $0000
        stx+2   $0000
        sta+3   $000000
        bcc     L115B
L115B   sta     ($00),y
        sta     ($00)
        sta     ($00,S),y
        sty     $00,x
        sta     $00,x
        stx     $00,y
        sta     [$00],y
        tya
        sta     $0000,y
        txs
        txy
        stz+2   $0000
        sta+2   $0000,x
        stz+2   $0000,x
        sta+3   $000000,x
        ldy     #$00
        lda     ($00,x)
        ldx     #$00
        lda     $00,S
        ldy     $00
        lda     $00
        ldx     $00
        lda     [$00]
        tay
        lda     #$00
        tax
        plb
        ldy+2   $0000
        lda+2   $0000
        ldx+2   $0000
        lda+3   $000000
        bcs     L11A0
L11A0   lda     ($00),y
        lda     ($00)
        lda     ($00,S),y
        ldy     $00,x
        lda     $00,x
        ldx     $00,y
        lda     [$00],y
        clv
        lda     $0000,y
        tsx
        tyx
        ldy+2   $0000,x
        lda+2   $0000,x
        ldx+2   $0000,y
        lda+3   $000000,x
        cpy     #$00
        cmp     ($00,x)
        rep     #$00
        cmp     $00,S
        cpy     $00
        cmp     $00
        dec     $00
        cmp     [$00]
        iny
        cmp     #$00
        dex
        wai
        cpy+2   $0000
        cmp+2   $0000
        dec+2   $0000
        cmp+3   $000000
        bne     L11E5
L11E5   cmp     ($00),y
        cmp     ($00)
        cmp     ($00,S),y
        pei     ($00)
        cmp     $00,x
        dec     $00,x
        cmp     [$00],y
        cld
        cmp     $0000,y
        phx
        stp

L11F9   jml     [$0000]

L11FC   cmp+2   $0000,x
        dec+2   $0000,x
        cmp+3   $000000,x
        cpx     #$00
        sbc     ($00,x)
        sep     #$00
        sbc     $00,S
        cpx     $00
        sbc     $00
        inc     $00
        sbc     [$00]
        inx
        sbc     #$00
        nop
        xba
        cpx+2   $0000
        sbc+2   $0000
        inc+2   $0000
        sbc+3   $000000
        beq     L122A
L122A   sbc     ($00),y
        sbc     ($00)
        sbc     ($00,S),y
        pea     $0000
        sbc     $00,x
        inc     $00,x
        sbc     [$00],y
        sed
        sbc     $0000,y
        plx
        xce
        jsr     ($0000,x)
        sbc+2   $0000,x
        inc+2   $0000,x
        sbc+3   $000000,x
