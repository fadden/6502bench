; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: cc65
;
; For the 65816 we want to exercise some additional things.

        .setcpu "65816"

symlong = $123456

        .org    $1000
        clc
        xce
        sep     #$30
        jml     bank44
lodat:  .byte   $00,$01,$02             ;EDIT: set label

        .org    $440000
bank44: cmp     f:bank44
low44:  lda     bank44
        lda     a:bank44 & $ffff
        lda     z:bank44 & $ffff        ;DP ref, should resolve to "zero"
        bmi     low44
        per     low44-$40
        bne     low44-$40               ;branch to high44
        brl     bank44-$40              ;branch to late44

dat44:                                  ;EDIT: set label
        .addr   dat44                   ;EDIT: format as 16-bit Address
        .faraddr dat44                  ;EDIT: format as 24-bit Address

        .org    $44ffc0
late44: cmp     f:late44
high44: beq     cont44                  ;EDIT: set label
        bmi     late44+$44              ;branch to low44
        brl     late44+$44              ;branch to low44

cont44: jml     twok

        .org    $2000
twok:   bit     twok
        pea     dat44 & $ffff           ;EDIT: set symbol=dat44
        pea     dat44 >> 16             ;EDIT: set symbol=dat44
        bne     skip
        jmp     [lodat]

skip:   nop
j1:     jsr     j2                      ;EDIT: set symbol=j2 for all, confirm auto-labels vanish
j2:     jsr     j3                      ;EDIT: set label
j3:     jsr     j1

        jsl     symlong
        rts
