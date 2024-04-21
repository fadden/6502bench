; Copyright 2024 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; The symbol __ENABLE_ALL_LABEL_NEWLINE must be defined in the project
; symbols, so that labels are placed on their own lines whenever possible.
;
; Assembler: Merlin 32

        org     $1000
shortnm equ     $4000
SomewhatLongName equ $4001

        lda     shortnm
        ldx     SomewhatLongName
        clc
        bcc     BranchTargetLongName
data    dfb     $cc

]ptr    equ     $00
]PointerWithLongName equ     $02

BranchTargetLongName
        sta     ]ptr
        stx     ]PointerWithLongName
        ldy     data
        lsr     A
        bcc     shortb
shortb  nop

        jmp     done

done    rts
