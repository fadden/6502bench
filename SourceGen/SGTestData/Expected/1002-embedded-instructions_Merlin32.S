         org   $1000
         clc
         xce
         sep   #$30
         jsr   L1014
         jsr   L101C
         jsr   L102A
         jsr   L102F
         jsr   L1059
         rts

L1014    lda   #$00
         dfb   $2c
L1017    lda   #$01
         beq   L1017
         rts

L101C    sep   #$30
         lda   $00
         beq   L1025
         lda   #$00
         brk

L1025    stal  $012345
         rts

L102A    dfb   $20
L102B    rts

         dfb   $ea
         bra   L102B

L102F    dfb   $2c
L1030    dfb   $2c
L1031    dfb   $2c
L1032    dfb   $2c
L1033    dfb   $2c
L1034    dfb   $2c
L1035    dfb   $2c
L1036    dfb   $2c
L1037    dfb   $2c
L1038    nop
         nop
         asl   A
         bcc   L102F
         asl   A
         bcc   L1030
         asl   A
         bcc   L1031
         asl   A
         bcc   L1032
         asl   A
         bcc   L1033
         asl   A
         bcc   L1034
         asl   A
         bcc   L1035
         asl   A
         bcc   L1036
         asl   A
         bcc   L1037
         asl   A
         bcc   L1038
         rts

L1059    dfb   $2c
L105A    nop
         dfb   $ad
L105C    lda   $00
         asl   A
         bcc   L105A
         asl   A
         bcc   L105C
         dfb   $af
