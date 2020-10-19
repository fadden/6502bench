; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
; 65816 version

        org     $1000
        clc
        xce
        sep     #$ff        ;set all flags
        mx      %11

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


; Test M/X behavior across PHP/PLP.  The flags should be left alone
; (non-smart mode), or set to whatever they were on the previous PHP
; (smart mode).
        rep     #$20
        php
        plp                 ;should restore cleared carry
        lda     #$1234
        sep     #$20
        php
        plp
        lda     #$12

; test tracking M/X across subroutine calls
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
        jmp     done


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

done
        rts
