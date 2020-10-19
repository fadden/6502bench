         org   $1000
         jsr   L100F
         jsr   L1017
         jsr   L101C
         jsr   L1046
         jmp   L1051

L100F    lda   #$00
         dfb   $2c
:L1012   lda   #$01
         beq   :L1012
         rts

L1017    dfb   $20
:L1018   rts

         dfb   $ea
         bvs   :L1018
L101C    dfb   $2c
:L101D   dfb   $2c
:L101E   dfb   $2c
:L101F   dfb   $2c
:L1020   dfb   $2c
:L1021   dfb   $2c
:L1022   dfb   $2c
:L1023   dfb   $2c
:L1024   dfb   $2c
:L1025   nop
         nop
         asl   A
         bcc   L101C
         asl   A
         bcc   :L101D
         asl   A
         bcc   :L101E
         asl   A
         bcc   :L101F
         asl   A
         bcc   :L1020
         asl   A
         bcc   :L1021
         asl   A
         bcc   :L1022
         asl   A
         bcc   :L1023
         asl   A
         bcc   :L1024
         asl   A
         bcc   :L1025
         rts

L1046    dfb   $2c
:L1047   nop
         dfb   $ad
:L1049   lda   $00
         asl   A
         bcc   :L1047
         asl   A
         bcc   :L1049
L1051    dfb   $ad
