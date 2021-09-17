         .setcpu "65C02"
G_DP     =       $20

         .org    $1000
         bbr0    $10,L1004
         rts

L1004:   bbs1    $10,L100B
         rts

         .byte   $80
         .byte   $80
         .byte   $80

L_DP     .set    $30
L100B:   nop
         rmb2    $10
         smb3    $10
         rmb4    G_DP
         smb5    G_DP
         rmb6    L_DP
         smb7    L_DP
@L1018:  bbr2    $10,@L1018
         bbs3    $10,@L1018
         bbr4    $20,@L1018
         bbs5    $20,@L1018
         bbr6    $30,@L1018
         bbs7    $30,@L1018
         nop
         rts

