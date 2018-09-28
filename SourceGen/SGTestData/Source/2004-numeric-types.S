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

        ds      2
        dfb     $80
        ds      3
        dfb     $80
        ds      4
        dfb     $80
        ds      5
        dfb     $80
        ds      256
        dfb     $80

        ds      257,$cc

        hex     11
        dfb     $80
        hex     11223344556677889900
        dfb     $80
        hex     00112233445566778899aabbccddeeff
        hex     00112233445566778899aabbccddeeff
        hex     00112233445566778899aabbccddeeff
        hex     ffeeddccbbaa99887766554433221100
        dfb     $80

