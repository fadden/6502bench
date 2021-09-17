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
         brk

         .byte   $00

L101F:   ora     ($00,x)
         cop     $00
         ora     $00,S
         tsb     $00
         ora     $00
         asl     $00
         ora     [$00]
         php
         ora     #$00
         asl     A
         phd
         tsb     a:$0000
         ora     a:$0000
         asl     a:$0000
         ora     f:$000000
         bpl     @L1041
@L1041:  ora     ($00),y
         ora     ($00)
         ora     ($00,S),y
         trb     $00
         ora     $00,x
         asl     $00,x
         ora     [$00],y
         clc
         ora     $0000,y
         inc     A
         tcs
         trb     a:$0000
         ora     a:$0000,x
         asl     a:$0000,x
         ora     f:$000000,x
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
         rol     A
         pld
         bit     a:$0000
         and     a:$0000
         rol     a:$0000
         and     f:$000000
         bmi     @L1089
@L1089:  and     ($00),y
         and     ($00)
         and     ($00,S),y
         bit     $00,x
         and     $00,x
         rol     $00,x
         and     [$00],y
         sec
         and     $0000,y
         dec     A
         tsc
         bit     a:$0000,x
         and     a:$0000,x
         rol     a:$0000,x
         and     f:$000000,x
         rti

L10AB:   eor     ($00,x)
         wdm     $00
         eor     $00,S
         mvp     #$00,#$00
         eor     $00
         lsr     $00
         eor     [$00]
         pha
         eor     #$00
         lsr     A
         phk
         jmp     @L10C2

@L10C2:  eor     a:$0000
         lsr     a:$0000
         eor     f:$000000
         bvc     @L10CE
@L10CE:  eor     ($00),y
         eor     ($00)
         eor     ($00,S),y
         mvn     #$00,#$00
         eor     $00,x
         lsr     $00,x
         eor     [$00],y
         cli
         eor     $0000,y
         phy
         tcd
         jml     @L10E7

@L10E7:  eor     a:$0000,x
         lsr     a:$0000,x
         eor     f:$000000,x
         rts

L10F2:   adc     ($00,x)
         per     $0ff6
         adc     $00,S
         stz     $00
         adc     $00
         ror     $00
         adc     [$00]
         pla
         adc     #$00
         ror     A
         rtl

L1106:   jmp     ($0000)

L1109:   adc     a:$0000
         ror     a:$0000
         adc     f:$000000
         bvs     @L1115
@L1115:  adc     ($00),y
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

L112C:   adc     a:$0000,x
         ror     a:$0000,x
         adc     f:$000000,x
         bra     @L1138

@L1138:  sta     ($00,x)
         brl     @L113D

@L113D:  sta     $00,S
         sty     $00
         sta     $00
         stx     $00
         sta     [$00]
         dey
         bit     #$00
         txa
         phb
         sty     a:$0000
         sta     a:$0000
         stx     a:$0000
         sta     f:$000000
         bcc     @L115B
@L115B:  sta     ($00),y
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
         stz     a:$0000
         sta     a:$0000,x
         stz     a:$0000,x
         sta     f:$000000,x
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
         ldy     a:$0000
         lda     a:$0000
         ldx     a:$0000
         lda     f:$000000
         bcs     @L11A0
@L11A0:  lda     ($00),y
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
         ldy     a:$0000,x
         lda     a:$0000,x
         ldx     a:$0000,y
         lda     f:$000000,x
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
         cpy     a:$0000
         cmp     a:$0000
         dec     a:$0000
         cmp     f:$000000
         bne     @L11E5
@L11E5:  cmp     ($00),y
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

L11F9:   jml     [$0000]

L11FC:   cmp     a:$0000,x
         dec     a:$0000,x
         cmp     f:$000000,x
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
         cpx     a:$0000
         sbc     a:$0000
         inc     a:$0000
         sbc     f:$000000
         beq     @L122A
@L122A:  sbc     ($00),y
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
         sbc     a:$0000,x
         inc     a:$0000,x
         sbc     f:$000000,x
