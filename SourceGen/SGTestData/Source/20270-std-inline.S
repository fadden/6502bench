; Copyright 2021 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Test standard inline script.
;
; Assembler: ACME
; % tass64 --ascii --case-sensitive --nostart 20260-nested-regions.S

        !cpu    6502
*       =       $1000
f_W_ext =      $e000       ;EDIT: add project symbol
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


; Test having multiple address spaces with the same target address.
; Two of the spaces have a matching symbol, one doesn't.  If we
; match strictly by address we'll get it wrong.
        jsr     f_W_2k1
        !word   $1100

        nop
        jmp     test_ext

        !byte   $80

        !pseudopc $2000 {   ;EDIT: add address space, set label InW_
f_W_2k1 nop
        jsr     f_W_2k1
        !word   $1200
        rts
        }

        !byte   $80

        !pseudopc $2000 {   ;EDIT: add address space, set label InW_
f_W_2k2 nop
        jsr     f_W_2k2
        !word   $1300
        rts
        }

        !byte   $80

        !pseudopc $2000 {   ;EDIT: add address space, no label
notspec nop
        jsr     notspec
        bit     notspec
        rts
        }

        !byte   $81

; Test having a label in a non-addressable area.  The formatter should
; ignore it, since such areas can't have code in them.  Note we can't
; actually call it, since that would require referencing a label in a
; non-addressable region, so we're really just using this as a way to
; exercise the setup code in the script.
        !pseudopc $3000 {   ;EDIT: add NA address space, set label InW_
f_W_na  nop
        jsr     f_W_na
        rts
        }

        !byte   $81


; Test a call to a project symbol with a matching name.  These don't
; have offsets, so the question of whether or not to handle the JSR
; hinges on the specific symbol used in the source.
        !pseudopc $4000 {
test_ext bit    test_ext

        jsr     f_W_ext

        jmp     end_stuff
        }

        !pseudopc   $f000 {
; end-of-file error cases
end_stuff
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
        }
