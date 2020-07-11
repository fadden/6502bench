; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Segment #3 : code, load at specific address ($02/3456)

        REL                 ;generate relocatable code

BANK2_START ENT
start   ldal    start
        jsr     later

        ldy     #BANK2_MOV_DST
        ora     $3456
        oral    $023456
        rtl

BANK2_MOV_DST ENT
        ds      16

later   nop
        rts
