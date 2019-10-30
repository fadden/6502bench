; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

INPUT   equ     $3000
OUTPUT  equ     $0400
PTR1    equ     $02
PTR2    equ     $04

START   equ     $2000
        org     START-2

        dw      START

        sec
        ror     A
        bmi     skipstr

string0 asc     'first string',00
string1 asc     'another string',00
string2 asc     'string the third',00
string3 asc     'last string',00

skipstr
        lda     #<stringtab
        sta     PTR1
        lda     #>stringtab
        sta     PTR1+1

        jsr     thing
        and     #$03
        asl     A
        tay
        lda     (PTR1),y
        sta     PTR2
        iny
        lda     (PTR1),y
        sta     PTR2+1

        ldy     #$ff
]loop   iny
        lda     (PTR2),y
        beq     copydone
        ora     #$80
        sta     OUTPUT,y
        bne     ]loop           ;always taken

stringtab
        dw      string0
        dw      string1
        dw      string2
        dw      string3

copydone
        jsr     PrintInlineZString
        asc     'Embedded!',00

        lda     #$ff            ;self-modifying code example
        sta     _mod+1
_mod    lda     #$00
        bne     skipbrk

middat  ds      4
outdat  ds      1

skipbrk
        lda     middat          ;nearby-label example
        ora     middat+1
        and     middat+2
        eor     middat+3

        ldx     #$00            ;embedded instruction example
        dfb     $2c
rebr    ldx     #$01
        sta     outdat
        lda     INPUT
        lsr     A
        lsr     A
        lsr     A
        lsr     A
        sta     INPUT
        bne     rebr

        rts

thing
        lda     INPUT
        rts

PrintInlineZString
        pla
        sta     PTR1
        pla
        sta     PTR1+1
        ldy     #$01
:loop   lda     (PTR1),Y
        beq     strend
        ora     #$80
        jsr     $FDED
        iny
        bne     :loop

strend  tya
        clc
        adc     PTR1
        sta     PTR1
        lda     PTR1+1
        adc     #$00
        pha
        lda     PTR1
        pha
        rts

