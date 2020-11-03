oth_00   equ   $1234
loaddr   equ   $c030
oth_01   equ   $011234
oth_02   equ   $021234
addr_02  equ   $02c030
addr_03  equ   $03c030
hiaddr   equ   $e0c030
addr_e2  equ   $e2c030
addr_e3  equ   $e3c030

         org   $1000
         clc
         xce
         sep   #$30
         phk
         plb
         lda   loaddr
         ldal  loaddr+$10000
         ldal  addr_02
         ldal  addr_03
         ldal  hiaddr
         ldal  hiaddr+$10000
         ldal  addr_e2
         ldal  addr_e3
         lda   oth_00
         ldal  oth_01
         ldal  oth_02
         lda   #$01
         pha
         plb
         ldal  loaddr
         lda   loaddr
         ldal  addr_02
         ldal  addr_03
         ldal  hiaddr
         ldal  hiaddr+$10000
         ldal  addr_e2
         ldal  addr_e3
         ldal  oth_00
         lda   oth_01
         ldal  oth_02
         lda   #$02
         pha
         plb
         ldal  loaddr
         ldal  loaddr+$10000
         lda   addr_02
         ldal  addr_03
         ldal  hiaddr
         ldal  hiaddr+$10000
         ldal  addr_e2
         ldal  addr_e3
         ldal  oth_00
         ldal  oth_01
         lda   oth_02
         lda   #$03
         pha
         plb
         ldal  loaddr
         ldal  loaddr+$10000
         ldal  addr_02
         lda   addr_03
         ldal  hiaddr
         ldal  hiaddr+$10000
         ldal  addr_e2
         ldal  addr_e3
         ldal  oth_00
         ldal  oth_01
         ldal  oth_02
         lda   #$e0
         pha
         plb
         ldal  loaddr
         ldal  loaddr+$10000
         ldal  addr_02
         ldal  addr_03
         lda   hiaddr
         ldal  hiaddr+$10000
         ldal  addr_e2
         ldal  addr_e3
         lda   #$e1
         pha
         plb
         ldal  loaddr
         ldal  loaddr+$10000
         ldal  addr_02
         ldal  addr_03
         ldal  hiaddr
         lda   hiaddr
         ldal  addr_e2
         ldal  addr_e3
         rts

