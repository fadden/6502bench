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
        jml     bank54



        .org    $543210
bank54: cmp     f:bank54
        bra     nxt54a

backchk:
        nop                             ;EDIT: set label
        nop
        rts
backval:
        .word   backchk & $ffff
        .byte   bank54 >> 16

nxt54a:
; Test forward/backward refs.  In cc65 this makes a difference because it's
; a one-pass assembler.
        lda     f:backchk               ;EDIT: use symbol
        lda     f:fwdchk                ;EDIT: use symbol
        lda     f:backchk               ;EDIT: use hex
        lda     f:fwdchk                ;EDIT: use hex

        lda     a:backchk & $ffff + 1   ;EDIT: use symbol
        lda     a:backchk & $ffff - 1   ;EDIT: use symbol
        lda     a:fwdchk & $ffff + 1    ;EDIT: use symbol
        lda     a:fwdchk & $ffff - 1    ;EDIT: use symbol

; Test non-bank-0 JSRs.  The behavior varies significantly by assembler.  The
; trick is that the high byte comes from the 'K' register.  cc65 wants you
; to remove it for a 16-bit JSR (or fails because the operand is too big),
; 64tass wants you to keep it in place (or fails because you're trying to
; jump to the wrong bank), and Merlin32 doesn't care.

        nop
        jsr     backchk & $ffff
        jsr     backchk & $ffff + 1     ;EDIT: set to "backchk"
        jsr     backchk & $ffff + 2     ;leave in hex
        jsr     fwdchk & $ffff
        jsr     fwdchk & $ffff + 1      ;EDIT: set to "fwdchk"
        jsr     fwdchk & $ffff + 2      ;leave in hex

        nop
        ldx     #$00
        jsr     (backval & $ffff,X)
        jsr     (fwdval & $ffff,X)
        jsr     jmp1b & $ffff
        jsr     jmp1f & $ffff
        jsr     jmp2b & $ffff
        jsr     jmp2f & $ffff
        jsr     jmp3b & $ffff
        jsr     jmp3f & $ffff
        bra     nxt54b

; Fun fact: "JMP (addr)" and "JML [addr]" always load the indirect value
; from zero page.  They do *not* use B or K.  The (addr,X) mode uses the
; program bank (K).  According to Eyes & Lichty, this is because the
; non-indexed form assumes you're jumping through a variable, while the
; indexed form assumes you've got an address table in your code.  (Think
; how they would be used in ROM.)
jmp2b:  jmp     (backval & $ffff,X)
jmp2f:  jmp     (fwdval & $ffff,X)
jmp1b:  jmp     (lodat)
jmp1f:  jmp     (lodat)
jmp3b:  jmp     [lodat]
jmp3f:  jmp     [lodat]

fwdval:
        .word   fwdchk & $ffff
        .byte   bank54 >> 16

fwdchk:
        nop                             ;EDIT: set label
        nop
        rts

nxt54b:
        jsr     skip                    ;EDIT: set to "skip" label
        rtl
