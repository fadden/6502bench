; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: cc65
;
; Both cc65 (2.17) and Merlin32 (1.0) have problems computing branches that
; wrap around a bank (e.g. from $0010 to $ffd0).  cc65 is slightly less
; egregious in that a workaround is possible: if you specify a label that
; is in range, and then an offset, it will generate code.

        .setcpu "65816"

symlong = $123456


        .org    $1000
        clc
        xce
        sep     #$30
        .a8
        .i8
        jmp     zero

        .org    $0000
zero:   bit     a:zero
low:    lda     zero
        lda     low
        bne     low-$40                 ;reference symbol
        bmi     low-$40                 ;EDIT: format as hex
        per     low-$40
        bvs     more
        brl     more1
lodat:  .byte   $00,$01,$02             ;EDIT: set label
more:   lda     more-2
        brl     zero-$40                ;branch to high

        .org    $0080
more1:  bit     a:more1
        jml     bank44

        .org    $ffc0
high:
        bit     high
        brl     high+$43                ;branch to low

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
