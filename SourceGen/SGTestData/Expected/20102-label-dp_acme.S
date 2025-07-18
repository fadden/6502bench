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

        !byte   $80

L101F   ora     (L0080,x)
        cop     $80
        ora     $80,S
        tsb+1   L0080
        ora+1   L0080
        asl+1   L0080
        ora     [L0080]
        php
        ora     #$80
        asl
        phd
        tsb+2   L0086
        ora+2   L0086
        asl+2   L0086
        ora+3   L0089
        bpl     @L1041
@L1041  ora     (L0080),y
        ora     (L0080)
        ora     ($80,S),y
        trb+1   L0080
        ora+1   L0080,x
        asl+1   L0080,x
        ora     [L0080],y
        clc
        ora     L0086,y
        inc
        tcs
        trb+2   L0086
        ora+2   L0086,x
        asl+2   L0086,x
        ora+3   L0089,x
        jsr     L0086
        and     (L0080,x)
        jsl     L0089
        and     $80,S
        bit+1   L0080
        and+1   L0080
        rol+1   L0080
        and     [L0080]
        plp
        and     #$80
        rol
        pld
        bit+2   L0086
        and+2   L0086
        rol+2   L0086
        and+3   L0089
        bmi     @L1089
@L1089  and     (L0080),y
        and     (L0080)
        and     ($80,S),y
        bit+1   L0080,x
        and+1   L0080,x
        rol+1   L0080,x
        and     [L0080],y
        sec
        and     L0086,y
        dec
        tsc
        bit+2   L0086,x
        and+2   L0086,x
        rol+2   L0086,x
        and+3   L0089,x
        rti

L10AB   eor     (L0080,x)
        !byte   $42,$80
        eor     $80,S
        mvp     #$84,#$83
        eor+1   L0080
        lsr+1   L0080
        eor     [L0080]
        pha
        eor     #$80
        lsr
        phk
        jmp     @L10C2

@L10C2  eor+2   L0086
        lsr+2   L0086
        eor+3   L0089
        bvc     @L10CE
@L10CE  eor     (L0080),y
        eor     (L0080)
        eor     ($80,S),y
        mvn     #$84,#$83
        eor+1   L0080,x
        lsr+1   L0080,x
        eor     [L0080],y
        cli
        eor     L0086,y
        phy
        tcd
        jml     @L10E7

@L10E7  eor+2   L0086,x
        lsr+2   L0086,x
        eor+3   L0089,x
        rts

L10F2   adc     (L0080,x)
        per     $0ff6
        adc     $80,S
        stz+1   L0080
        adc+1   L0080
        ror+1   L0080
        adc     [L0080]
        pla
        adc     #$80
        ror
        rtl

L1106   jmp     (L0086)

L1109   adc+2   L0086
        ror+2   L0086
        adc+3   L0089
        bvs     @L1115
@L1115  adc     (L0080),y
        adc     (L0080)
        adc     ($80,S),y
        stz+1   L0080,x
        adc+1   L0080,x
        ror+1   L0080,x
        adc     [L0080],y
        sei
        adc     L0086,y
        ply
        tdc
        jmp     (L0086,x)

L112C   adc+2   L0086,x
        ror+2   L0086,x
        adc+3   L0089,x
        bra     @L1138

@L1138  sta     (L0080,x)
        brl     @L113D

@L113D  sta     $80,S
        sty+1   L0080
        sta+1   L0080
        stx+1   L0080
        sta     [L0080]
        dey
        bit     #$80
        txa
        phb
        sty+2   L0086
        sta+2   L0086
        stx+2   L0086
        sta+3   L0089
        bcc     @L115B
@L115B  sta     (L0080),y
        sta     (L0080)
        sta     ($80,S),y
        sty+1   L0080,x
        sta+1   L0080,x
        stx+1   L0080,y
        sta     [L0080],y
        tya
        sta     L0086,y
        txs
        txy
        stz+2   L0086
        sta+2   L0086,x
        stz+2   L0086,x
        sta+3   L0089,x
        ldy     #$80
        lda     (L0080,x)
        ldx     #$80
        lda     $80,S
        ldy+1   L0080
        lda+1   L0080
        ldx+1   L0080
        lda     [L0080]
        tay
        lda     #$80
        tax
        plb
        ldy+2   L0086
        lda+2   L0086
        ldx+2   L0086
        lda+3   L0089
        bcs     @L11A0
@L11A0  lda     (L0080),y
        lda     (L0080)
        lda     ($80,S),y
        ldy+1   L0080,x
        lda+1   L0080,x
        ldx+1   L0080,y
        lda     [L0080],y
        clv
        lda     L0086,y
        tsx
        tyx
        ldy+2   L0086,x
        lda+2   L0086,x
        ldx+2   L0086,y
        lda+3   L0089,x
        cpy     #$80
        cmp     (L0080,x)
        rep     #$00
        cmp     $80,S
        cpy+1   L0080
        cmp+1   L0080
        dec+1   L0080
        cmp     [L0080]
        iny
        cmp     #$80
        dex
        wai
        cpy+2   L0086
        cmp+2   L0086
        dec+2   L0086
        cmp+3   L0089
        bne     @L11E5
@L11E5  cmp     (L0080),y
        cmp     (L0080)
        cmp     ($80,S),y
        pei     (L0080)
        cmp+1   L0080,x
        dec+1   L0080,x
        cmp     [L0080],y
        cld
        cmp     L0086,y
        phx
        stp

L11F9   jml     [L0086]

L11FC   cmp+2   L0086,x
        dec+2   L0086,x
        cmp+3   L0089,x
        cpx     #$80
        sbc     (L0080,x)
        sep     #$00
        sbc     $80,S
        cpx+1   L0080
        sbc+1   L0080
        inc+1   L0080
        sbc     [L0080]
        inx
        sbc     #$80
        nop
        xba
        cpx+2   L0086
        sbc+2   L0086
        inc+2   L0086
        sbc+3   L0089
        beq     @L122A
@L122A  sbc     (L0080),y
        sbc     (L0080)
        sbc     ($80,S),y
        pea     $0086
        sbc+1   L0080,x
        inc+1   L0080,x
        sbc     [L0080],y
        sed
        sbc     L0086,y
        plx
        xce
        jsr     (L0086,x)
        sbc+2   L0086,x
        inc+2   L0086,x
        sbc+3   L0089,x
        !pseudopc $0080 {
L0080   bit+1   @L0082
@L0082  bit+1   @L0082
        bit+1   @L0082
L0086   bit+2   L0086
L0089   lda+3   L0089
        }
