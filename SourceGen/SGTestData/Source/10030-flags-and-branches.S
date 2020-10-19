; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
; 6502 version

        org     $1000

; clear individual flags with instructions
; (this has no effect on the assembled output, but you can see the
; effects on the "status" column in the display list)
        clv
        cld
        cli
        clc
        lda     #$80        ;clear Z
        lda     #$01        ;clear N

        sed
        sei
        sec
        lda     #$ff        ;set N
        adc     #$00        ;set V, Z (actually scrambles NVZC)

; check branch instructions as best we can; NVMXDIZC
        lda     #$00
        pha
        plp                 ;without a nearby PHP, flags will be scrambled

ok_bmi  clv
        bvc     ok_bvc
        bvs     ok_bvs
        brk     $00

ok_bvc
ok_bvs  clc
        bcc     ok_bcc
        brk     $00

ok_bcc  sec
        bcs     ok_bcs
        brk     $00
ok_bcs

; check NZ flags set by immediate load
        lda     #$01
        bne     ok_nzero
        brk     $db
ok_nzero
        lda     #$00
        beq     ok_zero
        brk     $db
ok_zero
        bpl     ok_pos
        brk     $db
ok_pos
        lda     #$80
        bmi     ok_neg
        brk     $db
ok_neg

; check NZ flags set by immediate AND; only $00 / <=$7f have meaning
        lda     #$ff
        and     #$00
        beq     ok_andZ1
        brk     $db
ok_andZ1
        lda     #$00
        ldx     #$80
        and     #$ff
        beq     ok_andZ1A
        bne     ok_andZ1A
ok_andZ1A
        lda     #$ff
        ldx     #$00
        and     #$7f
        beq     ok_andZ0
        bne     ok_andZ0
ok_andZ0
        bpl     ok_andN0
        brk     $db
ok_andN0
        lda     #$ff
        and     #$80
        bpl     ok_andN1
        bmi     ok_andN1
        brk     $db
ok_andN1

; check NZ flags set by immediate ORA; only nonzero / >=$80 have meaning
        lda     #$00
        ldx     #$80
        bne     :next1
        brk     $db
:next1  ora     #$00        ;can't know what A-reg holds, so Z=indeterminate
        beq     ok_oraZ1
        bne     ok_oraZ1
ok_oraZ1
        ora     #$01
        bne     ok_oraZ0
        brk     $db
ok_oraZ0
        lda     #$00
        ldx     #$80
        bmi     :next2
        brk     $db
:next2  ora     #$7f        ;N-flag clear, but analyzer doesn't know,
        bpl     ok_oraN0    ; so both of these are considered viable
        bmi     ok_oraN0
        brk     $db
ok_oraN0
        ora     #$80
        bmi     ok_oraN1
        brk     $db
ok_oraN1

; check rol/ror
:foo    lda     :foo        ;scramble N/V
        sec
        ror     A           ;rotates the carry into the hi bit (N)
        bmi     ok_ror1
        brk     $dc
ok_ror1
        clc
        ror     A           ;now try with carry clear
        bpl     ok_ror2
        brk     $dc
ok_ror2
        lda     #$00        ;set Z=1
        sec
        rol     A           ;set Z=0 (could also set C=0)
        bne     ok_rol1
        brk     $dc
ok_rol1

; check lsr
        lda     #$ff
        lsr     A           ;lsr always clears the high bit
        bpl     ok_lsr
        brk     $dd
ok_lsr

; simple "smart" plp test, changes state according to default
        clc
        php
        sec
        plp                 ;"smart" restores carry
        bcc     ok_plp      ; and makes this a branch-always
        nop                 ;non-smart is indeterminate
ok_plp

; regression test for bug in analyzer
        sec                 ;here carry is clear
        bcs     flg2
flg1    clc

flg2    lda     $33
        beq     flg1
        bcs     flg3        ;this should NOT be branch-always
        lda     $44
flg3    nop

done
        rts
