; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: cc65
;
; Both cc65 (2.17) and Merlin32 (1.0) have problems computing branches that
; wrap around a bank (e.g. from $0010 to $ffd0).  cc65 is slightly less
; egregious in that a workaround is possible: if you specify a label that
; is in range, and then an offset, it will generate code.
;
; 6502 version

        .setcpu "6502"

        .org    $1000
        jmp     zero

        .org    $0000
zero:   bit     a:zero
low:    lda     zero
        lda     low
        bne     low-$40                 ;reference symbol
        bmi     low-$40                 ;EDIT: format as hex
        bvs     more
        bvc     more1
lodat:  .byte   $00,$01,$02             ;EDIT: set label
more:   lda     more-2
        clc
        bcc     zero-$40                ;branch to high

        .org    $0080
more1:  bit     a:more1
        jmp     end

        .org    $ffc0
high:
        bit     high
        clc
        bcc     high+$43                ;branch to low

end:    rts
