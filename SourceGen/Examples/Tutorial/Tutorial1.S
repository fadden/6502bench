; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

INPUT   equ     $3000
OUTPUT  equ     $0400

        org     $1000

        ldy     #END-AFTER
copy    lda     BEFORE,y
        sta     AFTER,y
        dey
        bmi     done
        bpl     copy

        dfb     $00
stuff   asc     'hello!'

done    jmp     AFTER

BEFORE
        org     $2000
AFTER

        lda     INPUT           ;expecting 0-3
        cmp     #4
        blt     :valid
        lda     #4              ;error message
:valid  asl     A
        tax
        lda     stringtab,x       ;set load to address
        sta     _load+1
        lda     stringtab+1,x
        sta     _load+2

        ldy     #12             ;fixed-width strings
_load   lda     $0000,y         ;self-modifying code
        ora     #$80
        sta     OUTPUT,y
        dey
        bpl     _load

        rts

stringtab
        dw      string0
        dw      string1
        dw      string2
        dw      string3
        dw      stringX
        dfb     $00

string0 asc     'string zero  '
string1 asc     'string one   '
string2 asc     'string two   '
string3 asc     'string three '
stringX asc     'invalid index'

END

