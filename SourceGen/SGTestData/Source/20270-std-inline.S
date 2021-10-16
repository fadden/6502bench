; Copyright 2021 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Test standard inline script.
;
; Assembler: ACME
; % tass64 --ascii --case-sensitive --nostart 20260-nested-regions.S

        !cpu    6502
*       =       $1000
        jmp     calls

; EDIT: put appropriate labels on these
f_AZ    rts
f_A1    rts
f_PZ    rts
f_P1    rts
f_W     rts
f_WA    rts
f_NR    rts

        !align  63,0,0
calls   nop
        jsr     f_AZ
        !text   "Test AZ_ low",$00

        jsr     f_AZ
        !xor $80 {
          !text "Test AZ_ high"
        }
        !byte   $00

        jsr     f_A1
        !text   12,"Test A1_ low"

        jsr     f_A1
        !byte   13
        !xor $80 {
          !text "Test A1_ high"
        }

        jsr     f_PZ
        !pet    "Test PZ_",$00
        jsr     f_P1
        !pet    8,"Test P1_"

        jsr     f_W
        !word   $1234

        jsr     f_WA
        !word   calls

        jsr     cont
        jsr     f_NR
        nop                 ;check: not formatted as instruction
        brk
cont    nop

; end-of-file error cases
        jsr     end_err1
        jsr     end_err2

        nop
        rts

end_err1
        jsr     f_A1
        !text   255,"too long"
        nop
end_err2
        jsr     f_AZ
        !text   "does not end"      ;must be last
