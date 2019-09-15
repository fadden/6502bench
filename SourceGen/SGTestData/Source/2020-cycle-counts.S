; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

; Exercise cycle counting.  We use 65816 to test the behavior with variable-
; width registers.  This means we're not testing the 65C02 D-flag behavior
; or some other non-65816 quirks.  If that becomes an issue we'll need to
; create additional tests.
;
; Very little editing required.  Just set the initial ORG and add
; "__ENABLE_CYCLE_COUNTS" to the list of project symbols.

        org     $10f0
start   sec
        xce
        mx      %11
        bra     cont        ;crosses page boundary
        ds      20

cont    bra     near
near

        lda     #$00
        beq     next        ;always (3 cycles)
        brk     $00
next    bne     next        ;never (2 cycles)
        lda     $1234
        beq     maybe       ;sometimes (2+ cycles)
        brk     $01         ;(7 cycles)
maybe   lda     $22,S
        trb     $02
        asl     $03,x
        
        clc
        xce
        rep     #$30
        mx      %00

        lda     #$11
        ldy     #$22
        trb     $04         ;should be +2 cycles
        asl     $05,x       ;should be +2 cycles
        ldx     $1235
        beq     maybe2
        brk     $02         ;(8 cycles)
maybe2  lda     $33,S
        beq     maybe3
        bra     start       ;crosses page boundary, no + when E=0
maybe3

        rep     #$20
        sep     #$10
        sta     $10
        stx     $11
        sty     $12
        rep     #$10
        sep     #$20
        sta     $11
        stx     $12
        sty     $13
        rep     #$30

; On the 65816, the setting of the decimal flag should NOT have any effect.
        lda     $1234
        adc     #$66
        adc     $1235
        sed
        adc     #$77
        adc     $1236

        sec
        xce
        sbc     #$88
        sbc     $1237
        cld
        sbc     #$99
        sbc     $1238
        rts

