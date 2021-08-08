; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: ACME v0.96.4 (has good PETSCII/screen code support)
; (acme -o 20120-char-encoding 20120-char-encoding.S)
;
; 6502 version
;
; This becomes multiple tests that differ only in how the default
; character set is configured in project properties.  Generate one
; project file, copy it on top of the others, change the property.
; All binaries are the same.

        !cpu    6502
*       =       $1000

; Single-byte operand
        lda     #'A'            ;format as low ASCII
        lda     #'A' | $80      ;format as high ASCII
        lda     #'A' | $80      ;format as PETSCII
        lda     #'A'            ;format as screen code

        ldx     #'a'            ;format as low ASCII
        ldx     #'a' | $80      ;format as high ASCII
        ldx     #'a' - $20      ;format as PETSCII
        ldx     #$01            ;format as screen code

        lda     #$7f            ;EDIT proj: force to low ASCII
        lda     #$7f            ;EDIT proj: force to high ASCII
        lda     #$7f            ;EDIT proj: force to PETSCII
        lda     #$7f            ;EDIT proj: force to screen code

        lda     #$0d            ;verify instr operand editor only allows C64SC

        jmp     end

; Single-byte data items
        !byte   'C'             ;format as above
        !byte   'C' | $80
        !byte   'C' | $80
        !byte   'C'

; Double-byte data items
        !byte   'd', 0          ;format as above
        !byte   'd' | $80, 0
        !byte   'd' - $20, 0
        !byte   $04, 0

; Double-byte big-endian data items
        !byte   0, 'E'          ;format as above
        !byte   0, 'E' | $80
        !byte   0, 'E' | $80
        !byte   0, 'E'

; Start with the basics.  Apply appropriate formatting.
        !byte   $80
        !text   "low ASCII str"
;        !byte   $80        ; let them run together to test scan / dialog behavior
        !xor $80 {
            !text   "high ASCII str"
        }
        !byte   $80
        !pet    "PETSCII str"
        !byte   $80
        !scr    "Screen Code str"

; Get a bit fancy.
        !byte   $82
        !text   $07,"Low ASCII CRLF",$0d,$0a
        !byte   $82
        !xor $80 {
            !text   $07,"High ASCII CRLF",$0d,$0a
        }
        !byte   $82
        !pet    $93,"PETSCII with ",$96,"control",$05," codes",$0d
        ; no control chars in screen code

; Test the ASCII $20-7e range.
        !byte   $83
        !text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?"
        !text   "@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_"
        !text   "`abcdefghijklmnopqrstuvwxyz{|}~"
        !byte   $83
        !xor $80 {
            !text   " !",$22,"#$%&'()*+,-./0123456789:;<=>?"
            !text   "@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_"
            !text   "`abcdefghijklmnopqrstuvwxyz{|}~"
        }
        !byte   $83
        !pet    " !",$22,"#$%&'()*+,-./0123456789:;<=>?"
        !pet    "@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_"
        !pet    "`abcdefghijklmnopqrstuvwxyz{|}~"
        !byte   $83
        !scr    " !",$22,"#$%&'()*+,-./0123456789:;<=>?"
        !scr    "@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_"
        !scr    "`abcdefghijklmnopqrstuvwxyz{|}~"

; The 20010-string-types test exercises low/high ASCII strings, so no need
; to do that here.  Do a quick test with C64 characters.  Note Screen Code
; can't be null-terminated without giving up '@', but we include it here
; anyway for completeness.
        !byte   $84
        !pet    "IICSTEP esrever"               ;format as StringReverse
        !byte   $84
        !pet    "null term PETSCII",0           ;format as StringNullTerm
        !byte   $84
        !pet    "This null-terminated string is too long to fit on a single line, and will be split.",0
        !byte   $84
        !pet    19,"PETSCII with length"        ;format as StringL8
        !byte   $84
        !pet    20,0,"PETSCII with length2"     ;format as StringL16
        !byte   $84
        !pet    "pet dcI"                       ;format as StringDCI

        !byte   $84
        !scr    "edoC neercS esrever"           ;format as StringReverse
        !byte   $84
        !scr    "null-term screen code?",0      ;format as String
        !byte   $84
        !scr    23,"Screen Code with length"    ;format as StringL8
        !byte   $84
        !scr    24,0,"Screen Code with length2" ;format as StringL16
        !byte   $84
        !scr    "Screen Code DC",$c9            ;format as StringDCI
        !byte   $84

        !byte   $85

; All bytes, from 00-ff.  Handy for seeing what the auto-scanner picks up
; based on the default character encoding.  DO NOT format this section.
allbytes
        !hex    000102030405060708090a0b0c0d0e0f
        !hex    101112131415161718191a1b1c1d1e1f
        !hex    202122232425262728292a2b2c2d2e2f
        !hex    303132333435363738393a3b3c3d3e3f
        !hex    404142434445464748494a4b4c4d4e4f
        !hex    505152535455565758595a5b5c5d5e5f
        !hex    606162636465666768696a6b6c6d6e6f
        !hex    707172737475767778797a7b7c7d7e7f
        !hex    808182838485868788898a8b8c8d8e8f
        !hex    909192939495969798999a9b9c9d9e9f
        !hex    a0a1a2a3a4a5a6a7a8a9aaabacadaeaf
        !hex    b0b1b2b3b4b5b6b7b8b9babbbcbdbebf
        !hex    c0c1c2c3c4c5c6c7c8c9cacbcccdcecf
        !hex    d0d1d2d3d4d5d6d7d8d9dadbdcdddedf
        !hex    e0e1e2e3e4e5e6e7e8e9eaebecedeeef
        !hex    f0f1f2f3f4f5f6f7f8f9fafbfcfdfeff

end     nop
        jmp     skip_esc

; String literals with backslash escapes.  Necessary because ACME v0.97
; introduced the feature and broke compatibility with previous versions.
        !text   "ASCII escapes \: \\ \r \n \",$22," \",$27," \t"
        !byte   $80
        !pet    "PETSCII escapes \: \\ \r \n \",$22," \",$27," \t"
        !byte   $80
        !scr    "SCR escapes \: \\ \r \n \",$22," \",$27," \t"
        !byte   $80
        !xor $80 {
; Can't include string delimiter due to code gen limitation.  Do "\0" instead.
            !text   "HIGH escapes \: \\ \r \n \",$30," \",$27," \t"
        }
        !byte   $80

skip_esc nop
        jmp     skip_dci

; Small batches of DCI strings, for testing single-character strings.
; EDIT: format all of these as DCI strings
        !byte   $86

        !text   "Hell",$ef                      ;Hello
        !text   "H",$c9                         ;Hi
        !byte   $d8                             ;X
        !byte   $a1                             ;!
        !byte   $86

        !xor $80 {
            !text   "Hell",$ef                      ;Hello
            !text   "H",$c9                         ;Hi
            !byte   $d8                             ;X
            !byte   $a1                             ;!
        }
        !byte   $86

        !pet    "hellO"
        !pet    "hI"
        !byte   $d8
        !byte   $a1
        !byte   $86

        !xor $80 {              ;hi then lo; probably not necessary
            !pet    "hellO"     ;(all C64 DCI seems to be lo-then-hi)
            !pet    "hI"
            !byte   $d8
            !byte   $a1
        }
        !byte   $86

        !scr    "Hell",$8f      ;just do lo-then-hi for screen codes
        !scr    "H",$c9
        !byte   $d8
        !byte   $a1
        !byte   $86

skip_dci nop
        rts
