;6502bench SourceGen v1.1.0-dev1
         .setcpu "65816"
         .org    $1000
         .a8
         .i8
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
         .byte   $00,$80

L101F:   ora     (L0080,x)
         cop     $80
         ora     $80,S
         tsb     z:L0080
         ora     z:L0080
         asl     z:L0080
         ora     [L0080]
         php
         ora     #$80
         asl     A
         phd
         tsb     a:L0086
         ora     a:L0086
         asl     a:L0086
         ora     f:L0089
         bpl     L1041
L1041:   ora     (L0080),y
         ora     (L0080)
         ora     ($80,S),y
         trb     z:L0080
         ora     z:L0080,x
         asl     z:L0080,x
         ora     [L0080],y
         clc
         ora     L0086,y
         inc     A
         tcs
         trb     a:L0086
         ora     a:L0086,x
         asl     a:L0086,x
         ora     f:L0089,x
         jsr     L0086
         and     (L0080,x)
         jsl     L0089
         and     $80,S
         bit     z:L0080
         and     z:L0080
         rol     z:L0080
         and     [L0080]
         plp
         and     #$80
         rol     A
         pld
         bit     a:L0086
         and     a:L0086
         rol     a:L0086
         and     f:L0089
         bmi     L1089
L1089:   and     (L0080),y
         and     (L0080)
         and     ($80,S),y
         bit     z:L0080,x
         and     z:L0080,x
         rol     z:L0080,x
         and     [L0080],y
         sec
         and     L0086,y
         dec     A
         tsc
         bit     a:L0086,x
         and     a:L0086,x
         rol     a:L0086,x
         and     f:L0089,x
         rti

L10AB:   eor     (L0080,x)
         .byte   $42,$80
         eor     $80,S
         .byte   $44,$83,$84
         eor     z:L0080
         lsr     z:L0080
         eor     [L0080]
         pha
         eor     #$80
         lsr     A
         phk
         jmp     L10C2

L10C2:   eor     a:L0086
         lsr     a:L0086
         eor     f:L0089
         bvc     L10CE
L10CE:   eor     (L0080),y
         eor     (L0080)
         eor     ($80,S),y
         .byte   $54,$83,$84
         eor     z:L0080,x
         lsr     z:L0080,x
         eor     [L0080],y
         cli
         eor     L0086,y
         phy
         tcd
         jml     L10E7

L10E7:   eor     a:L0086,x
         lsr     a:L0086,x
         eor     f:L0089,x
         rts

L10F2:   adc     (L0080,x)
         per     $0ff6
         adc     $80,S
         stz     z:L0080
         adc     z:L0080
         ror     z:L0080
         adc     [L0080]
         pla
         adc     #$80
         ror     A
         rtl

L1106:   jmp     (L0086)

L1109:   adc     a:L0086
         ror     a:L0086
         adc     f:L0089
         bvs     L1115
L1115:   adc     (L0080),y
         adc     (L0080)
         adc     ($80,S),y
         stz     z:L0080,x
         adc     z:L0080,x
         ror     z:L0080,x
         adc     [L0080],y
         sei
         adc     L0086,y
         ply
         tdc
         jmp     (L0086,x)

L112C:   adc     a:L0086,x
         ror     a:L0086,x
         adc     f:L0089,x
         bra     L1138

L1138:   sta     (L0080,x)
         brl     L113D

L113D:   sta     $80,S
         sty     z:L0080
         sta     z:L0080
         stx     z:L0080
         sta     [L0080]
         dey
         bit     #$80
         txa
         phb
         sty     a:L0086
         sta     a:L0086
         stx     a:L0086
         sta     f:L0089
         bcc     L115B
L115B:   sta     (L0080),y
         sta     (L0080)
         sta     ($80,S),y
         sty     z:L0080,x
         sta     z:L0080,x
         stx     z:L0080,y
         sta     [L0080],y
         tya
         sta     L0086,y
         txs
         txy
         stz     a:L0086
         sta     a:L0086,x
         stz     a:L0086,x
         sta     f:L0089,x
         ldy     #$80
         lda     (L0080,x)
         ldx     #$80
         lda     $80,S
         ldy     z:L0080
         lda     z:L0080
         ldx     z:L0080
         lda     [L0080]
         tay
         lda     #$80
         tax
         plb
         ldy     a:L0086
         lda     a:L0086
         ldx     a:L0086
         lda     f:L0089
         bcs     L11A0
L11A0:   lda     (L0080),y
         lda     (L0080)
         lda     ($80,S),y
         ldy     z:L0080,x
         lda     z:L0080,x
         ldx     z:L0080,y
         lda     [L0080],y
         clv
         lda     L0086,y
         tsx
         tyx
         ldy     a:L0086,x
         lda     a:L0086,x
         ldx     a:L0086,y
         lda     f:L0089,x
         cpy     #$80
         cmp     (L0080,x)
         rep     #$00
         cmp     $80,S
         cpy     z:L0080
         cmp     z:L0080
         dec     z:L0080
         cmp     [L0080]
         iny
         cmp     #$80
         dex
         wai
         cpy     a:L0086
         cmp     a:L0086
         dec     a:L0086
         cmp     f:L0089
         bne     L11E5
L11E5:   cmp     (L0080),y
         cmp     (L0080)
         cmp     ($80,S),y
         pei     (L0080)
         cmp     z:L0080,x
         dec     z:L0080,x
         cmp     [L0080],y
         cld
         cmp     L0086,y
         phx
         stp

L11F9:   jml     [L0086]

L11FC:   cmp     a:L0086,x
         dec     a:L0086,x
         cmp     f:L0089,x
         cpx     #$80
         sbc     (L0080,x)
         sep     #$00
         sbc     $80,S
         cpx     z:L0080
         sbc     z:L0080
         inc     z:L0080
         sbc     [L0080]
         inx
         sbc     #$80
         nop
         xba
         cpx     a:L0086
         sbc     a:L0086
         inc     a:L0086
         sbc     f:L0089
         beq     L122A
L122A:   sbc     (L0080),y
         sbc     (L0080)
         sbc     ($80,S),y
         pea     L0086
         sbc     z:L0080,x
         inc     z:L0080,x
         sbc     [L0080],y
         sed
         sbc     L0086,y
         plx
         xce
         jsr     (L0086,x)
         sbc     a:L0086,x
         inc     a:L0086,x
         sbc     f:L0089,x
         .org    $0080
L0080:   bit     z:L0082
L0082:   bit     z:L0082
         bit     L0082
L0086:   bit     a:L0086
L0089:   lda     f:L0089
