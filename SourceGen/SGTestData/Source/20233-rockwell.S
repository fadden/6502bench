; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Test Rockwell's 65C02 extensions.
; Assembler: ACME

        !cpu    w65c02
*       =       $1000

U_DP    =       $10             ;don't define
G_DP    =       $20             ;define as global project symbol
L_DP    =       $30             ;define as local variable

; Verify both halves of branches are traced.  In theory, BBR followed
; by BBS on the same bit number should be always-taken, but we don't
; try to model memory.

        bbr0    U_DP,cont0
        rts
cont0   bbs1    U_DP,cont1
        rts
        !byte   $80,$80,$80     ;filler to get different branch offset
cont1   nop

; Access the three zero-page values in different ways.  Also exercise
; backward and to-self branches.
; EDIT: add $30 to a local variable table here
        rmb2    U_DP
        smb3    U_DP
        rmb4    G_DP
        smb5    G_DP
        rmb6    L_DP
        smb7    L_DP

bback   bbr2    U_DP,bback
        bbs3    U_DP,bback
        bbr4    G_DP,bback
        bbs5    G_DP,bback
        bbr6    L_DP,bback
        bbs7    L_DP,bback

        nop
        rts
