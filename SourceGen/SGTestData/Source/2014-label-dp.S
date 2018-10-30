; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

ZP      equ     $80         ;must NOT be "$0080"

MV0     EQU     $83
MV1     EQU     $84

        PUT     allops-common-65816.S

        org     $0080
        bit     _ZP
_ZP     bit     _ZP
        bit     _ZP
ABS     bit:    ABS
LONG    ldal    LONG

