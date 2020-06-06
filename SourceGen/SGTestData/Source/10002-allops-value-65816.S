; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
ZP       EQU   $FF
ABS      EQU   $FEFF
LONG     EQU   $FDFEFF
MV0      EQU   ZP           ;Merlin 32 bug -- must use 8-bit constant, or
MV1      EQU   ZP-1         ; '^' modifier is implicitly applied

         PUT   allops-common-65816.S
