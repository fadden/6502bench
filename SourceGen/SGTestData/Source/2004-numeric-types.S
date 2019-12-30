; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        ORG     $1000

        bit     dref+8
        jsr     cref+8
        ds      16,$ea              ;bunch of NOPs
        rts

        dfb     $11                 ;.dd1
        dw      $1122               ;.dd2
        adr     $112233             ;.dd3
        adrl    $11223344           ;.dd4

        dfb     $11                 ;.dbd1
        ddb     $1122               ;.dbd2
        dfb     $11,$22,$33         ;.dbd3
        dfb     $11,$22,$33,$44     ;.dbd4

        ds      2                   ;.fill
        dfb     $80
        ds      3                   ;.fill
        dfb     $80
        ds      4                   ;.fill
        dfb     $80
        ds      5                   ;.fill
        dfb     $80
        ds      256                 ;.fill
        dfb     $80

        ds      257,$cc             ;.fill

        hex     11                  ;.bulk
        dfb     $80
        hex     11223344556677889900    ;.bulk
        dfb     $80
        hex     00112233445566778899aabbccddeeff    ;4 lines .bulk
        hex     00112233445566778899aabbccddeeff    ;add a comment
        hex     00112233445566778899aabbccddeeff
        hex     ffeeddccbbaa99887766554433221100
        dfb     $80

; align to 256-byte boundary
        ds      \,$aa               ;.junk, align 256
        dfb     $81
        ds      63,$00              ;.junk, align 64
        dfb     $81
        ds      31,$ab              ;.junk, align 32
        hex     0000000000000001    ;.junk (should become .dense)
        dfb     $81
        hex     1000000000000000    ;.junk (should become .dense)
        dfb     $81
        hex     dddd                ;EDIT FILE: give this a bogus alignment
        ds      \,$00               ;.junk, align 256

; Check to see what splits a .fill block.  Each 16-byte chunk has some sort
; of item added at +8.  DO NOT format these; the goal is to check the behavior
; of the data analyzer.
        ds      16,$82              ;EDIT: add no-op .ORG
        ds      16,$83              ;EDIT: add .ORG that adjusts +16
        ORG     *+16
        ds      16,$84              ;EDIT: add user label
dref    ds      16,$85              ;has a data reference
        ds      16,$86              ;EDIT: add a local variable table (may not split)
        ds      16,$87              ;EDIT: add full-line comment
        ds      16,$88              ;EDIT: add end-of-line comment (should not split)
        ds      16,$89              ;EDIT: add note
        ds      16,$8a              ;EDIT: add visualization
cref    ds      16,$8b              ;has a code reference
        ds      16,$8c              ;EDIT: format byte as binary
