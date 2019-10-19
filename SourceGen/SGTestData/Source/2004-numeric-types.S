; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

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
