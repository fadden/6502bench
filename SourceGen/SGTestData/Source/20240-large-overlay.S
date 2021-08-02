; Copyright 2021 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $0000

; EDIT: change each 8KB segment to start at $8000, so they overlap

        nop                 ;work around Merlin 1.0 bug
        ds      $1fff,$00
        ds      $2000,$01
        ds      $2000,$02
        ds      $2000,$03
        ds      $2000,$04
        ds      $2000,$05
        ds      $2000,$06
        ds      $2000,$07
