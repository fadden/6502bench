         .setcpu "65816"
oth_00   =       $1234
loaddr   =       $c030
oth_01   =       $011234
oth_02   =       $021234
addr_02  =       $02c030
addr_03  =       $03c030
hiaddr   =       $e0c030
addr_e2  =       $e2c030
addr_e3  =       $e3c030

         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$30
         phk
         plb
         lda     loaddr
         lda     loaddr+$10000
         lda     addr_02
         lda     addr_03
         lda     hiaddr
         lda     hiaddr+$10000
         lda     addr_e2
         lda     addr_e3
         lda     oth_00
         lda     oth_01
         lda     oth_02
         lda     #$01
         pha
         plb
         lda     f:loaddr
         lda     loaddr
         lda     addr_02
         lda     addr_03
         lda     hiaddr
         lda     hiaddr+$10000
         lda     addr_e2
         lda     addr_e3
         lda     f:oth_00
         lda     oth_01 & $ffff
         lda     oth_02
         lda     #$02
         pha
         plb
         lda     f:loaddr
         lda     loaddr+$10000
         lda     addr_02 & $ffff
         lda     addr_03
         lda     hiaddr
         lda     hiaddr+$10000
         lda     addr_e2
         lda     addr_e3
         lda     f:oth_00
         lda     oth_01
         lda     oth_02 & $ffff
         lda     #$03
         pha
         plb
         lda     f:loaddr
         lda     loaddr+$10000
         lda     addr_02
         lda     addr_03 & $ffff
         lda     hiaddr
         lda     hiaddr+$10000
         lda     addr_e2
         lda     addr_e3
         lda     f:oth_00
         lda     oth_01
         lda     oth_02
         lda     #$e0
         pha
         plb
         lda     f:loaddr
         lda     loaddr+$10000
         lda     addr_02
         lda     addr_03
         lda     hiaddr & $ffff
         lda     hiaddr+$10000
         lda     addr_e2
         lda     addr_e3
         lda     #$e1
         pha
         plb
         lda     f:loaddr
         lda     loaddr+$10000
         lda     addr_02
         lda     addr_03
         lda     hiaddr
         lda     hiaddr & $ffff
         lda     addr_e2
         lda     addr_e3
         rts

