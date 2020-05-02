; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000
        clc
        xce
        sep     #$ff        ;set all flags
        mx      %11

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

; clear individual flags with REP
        sep     #$ff
        rep     #$80
        rep     #$40
        rep     #$20
        rep     #$10
        rep     #$08
        rep     #$04
        rep     #$02
        rep     #$01

; exercise SEP/REP with #$00
        sep     #$00
        sep     #$ff
        rep     #$00
        rep     #$ff

; confirm emulation behavior
        mx      %00         ;long regs
        lda     #$feed      ;check it
        sec
        xce                 ;emulation mode
        lda     #$ff        ;check it
        rep     #$30        ;should have no effect...
        mx      %11         ;...but Merlin32 doesn't know that
        lda     #$ff
        clc
        xce                 ;back to native, should set M/X=1
        lda     #$ff        ;check it

; try one long, one short
        rep     #$20        ;long a
        sep     #$10        ;short x/y
        mx      %01
        lda     #$0000
        ldx     #$01
        ldy     #$02

        sep     #$20        ;short a
        rep     #$10        ;long x/y
        mx      %10
        lda     #$01
        ldx     #$0000
        ldy     #$0000

; check branch instructions; NVMXDIZC
        sep     #$30
        mx      %11
        lda     #$00
        pha
        plp                 ;without a nearby PHP, flags will be scrambled

        rep     #$80
        bpl     ok_bpl
        brk     $00

ok_bpl  sep     #$80
        bpl     :bad        ;branch never taken
        bmi     ok_bmi
:bad    brk     $00

ok_bmi  rep     #$40
        bvc     ok_bvc
        brk     $00

ok_bvc  sep     #$40
        bvs     ok_bvs
        brk     $00

ok_bvs  rep     #$01
        bcc     ok_bcc
        brk     $00

ok_bcc  sep     #$01
        bcs     ok_bcs
        brk     $00

ok_bcs  rep     #$02
        bne     ok_bne
        brk     $00

ok_bne  sep     #$02
        beq     ok_beq
        brk     $00

ok_beq

; check NZ flags set by immediate load
        sep     #$ff        ;set all
        mx      %11
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

; check NZ flags set by immediate AND
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
        bmi     ok_andN1
        brk     $db
ok_andN1

; check NZ flags set by immediate ORA
        lda     #$00
        ldx     #$80
        bne     :next1
        brk     $db
:next1  ora     #$00
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
:next2  ora     #$7f        ;N-flag clear, but analyzer doesn't know
        bpl     ok_oraN0    ;so both of these are considered viable
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

; simple php/plp pair test
        clc
        php
        sec
        plp                 ;should restore cleared carry
        bcc     ok_plp
        brk     $00
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

; test tracking across subroutine calls
        rep     #$20        ;long a
        sep     #$10        ;short x/y
        mx      %01
        jsr     long_subr   ;confirm flag propagation

        rep     #$30
        mx      %00
        jsr     ambig_subr

        sep     #$30
        mx      %11
        jsr     ambig_subr

        rep     #$20        ;long a
        sep     #$10        ;short x/y
        mx      %01
        jsr     long_subr   ;call it again


; leave the main routine with short flags set
        sep     #$30
        mx      %11
        rts


; only called with longm/shortx
        mx      %01
long_subr
        lda     #$1234
        ldx     #$ff
        rts

; this is called with different values for M/X, so it defaults to short
        mx      %11
ambig_subr
        lda     #$ff
        ldx     #$ee
        ldy     #$dd
        rts

