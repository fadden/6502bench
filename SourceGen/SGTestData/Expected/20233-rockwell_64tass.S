        .cpu    "w65c02"
G_DP    =       $20

*       =       $1000
        bbr     0,$10,L1004
        rts

L1004   bbs     1,$10,_L100B
        rts

        .byte   $80
        .byte   $80
        .byte   $80

L_DP    .var    $30
_L100B  nop
        rmb     2,$10
        smb     3,$10
        rmb     4,G_DP
        smb     5,G_DP
        rmb     6,L_DP
        smb     7,L_DP
_L1018  bbr     2,$10,_L1018
        bbs     3,$10,_L1018
        bbr     4,$20,_L1018
        bbs     5,$20,_L1018
        bbr     6,$30,_L1018
        bbs     7,$30,_L1018
        nop
        rts

