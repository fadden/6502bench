        !cpu    65816
oth_00  =       $1234
loaddr  =       $c030
oth_01  =       $011234
oth_02  =       $021234
addr_02 =       $02c030
addr_03 =       $03c030
hiaddr  =       $e0c030
addr_e2 =       $e2c030
addr_e3 =       $e3c030

*       =       $1000
        !as
        !rs
        clc
        xce
        sep     #$30
        phk
        plb
        lda     loaddr
        lda+3   loaddr+$10000
        lda+3   addr_02
        lda+3   addr_03
        lda+3   hiaddr
        lda+3   hiaddr+$10000
        lda+3   addr_e2
        lda+3   addr_e3
        lda     oth_00
        lda+3   oth_01
        lda+3   oth_02
        lda     #$01
        pha
        plb
        lda+3   loaddr
        lda     loaddr
        lda+3   addr_02
        lda+3   addr_03
        lda+3   hiaddr
        lda+3   hiaddr+$10000
        lda+3   addr_e2
        lda+3   addr_e3
        lda+3   oth_00
        lda     oth_01 & $ffff
        lda+3   oth_02
        lda     #$02
        pha
        plb
        lda+3   loaddr
        lda+3   loaddr+$10000
        lda     addr_02 & $ffff
        lda+3   addr_03
        lda+3   hiaddr
        lda+3   hiaddr+$10000
        lda+3   addr_e2
        lda+3   addr_e3
        lda+3   oth_00
        lda+3   oth_01
        lda     oth_02 & $ffff
        lda     #$03
        pha
        plb
        lda+3   loaddr
        lda+3   loaddr+$10000
        lda+3   addr_02
        lda     addr_03 & $ffff
        lda+3   hiaddr
        lda+3   hiaddr+$10000
        lda+3   addr_e2
        lda+3   addr_e3
        lda+3   oth_00
        lda+3   oth_01
        lda+3   oth_02
        lda     #$e0
        pha
        plb
        lda+3   loaddr
        lda+3   loaddr+$10000
        lda+3   addr_02
        lda+3   addr_03
        lda     hiaddr & $ffff
        lda+3   hiaddr+$10000
        lda+3   addr_e2
        lda+3   addr_e3
        lda     #$e1
        pha
        plb
        lda+3   loaddr
        lda+3   loaddr+$10000
        lda+3   addr_02
        lda+3   addr_03
        lda+3   hiaddr
        lda     hiaddr & $ffff
        lda+3   addr_e2
        lda+3   addr_e3
        rts

