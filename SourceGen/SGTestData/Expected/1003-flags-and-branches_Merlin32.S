         org   $1000
         clc
         xce
         sep   #$ff
         clv
         cld
         cli
         clc
         lda   #$80
         lda   #$01
         sed
         sei
         sec
         lda   #$ff
         adc   #$00
         sep   #$ff
         rep   #$80
         rep   #$40
         rep   #$20
         mx    %01
         rep   #$10
         mx    %00
         rep   #$08
         rep   #$04
         rep   #$02
         rep   #$01
         sep   #$00
         sep   #$ff
         mx    %11
         rep   #$00
         rep   #$ff
         mx    %00
         lda   #$feed
         sec
         xce
         mx    %11
         lda   #$ff
         rep   #$30
         mx    %11
         lda   #$ff
         clc
         xce
         lda   #$ff
         rep   #$20
         mx    %01
         sep   #$10
         lda   #$0000
         ldx   #$01
         ldy   #$02
         sep   #$20
         mx    %11
         rep   #$10
         mx    %10
         lda   #$01
         ldx   #$0000
         ldy   #$0000
         sep   #$30
         mx    %11
         lda   #$00
         pha
         plp
         rep   #$80
         bpl   L105F

         dfb   $00
         dfb   $00

L105F    sep   #$80
         bpl   L1065
         bmi   L1067

L1065    dfb   $00
         dfb   $00

L1067    rep   #$40
         bvc   L106D

         dfb   $00
         dfb   $00

L106D    sep   #$40
         bvs   L1073

         dfb   $00
         dfb   $00

L1073    rep   #$01
         bcc   L1079

         dfb   $00
         dfb   $00

L1079    sep   #$01
         bcs   L107F

         dfb   $00
         dfb   $00

L107F    rep   #$02
         bne   L1085

         dfb   $00
         dfb   $00

L1085    sep   #$02
         beq   L108B

         dfb   $00
         dfb   $00

L108B    sep   #$ff
         lda   #$01
         bne   L1093

         dfb   $00
         dfb   $db

L1093    lda   #$00
         beq   L1099

         dfb   $00
         dfb   $db

L1099    bpl   L109D

         dfb   $00
         dfb   $db

L109D    lda   #$80
         bmi   L10A3

         dfb   $00
         dfb   $db

L10A3    lda   #$ff
         and   #$00
         beq   L10AB

         dfb   $00
         dfb   $db

L10AB    lda   #$00
         and   #$ff
         beq   L10B3

         dfb   $00
         dfb   $db

L10B3    lda   #$ff
         and   #$7f
         bne   L10BB

         dfb   $00
         dfb   $db

L10BB    bpl   L10BF

         dfb   $00
         dfb   $db

L10BF    lda   #$ff
         and   #$80
         bmi   L10C7

         dfb   $00
         dfb   $db

L10C7    lda   #$00
         ora   #$00
         beq   L10CF

         dfb   $00
         dfb   $db

L10CF    ora   #$01
         bne   L10D5

         dfb   $00
         dfb   $db

L10D5    lda   #$00
         ora   #$7f
         bpl   L10DD

         dfb   $00
         dfb   $db

L10DD    ora   #$80
         bmi   L10E3

         dfb   $00
         dfb   $db

L10E3    lda   L10E3
         sec
         ror   A
         bmi   L10EC

         dfb   $00
         dfb   $dc

L10EC    clc
         ror   A
         bpl   L10F2

         dfb   $00
         dfb   $dc

L10F2    lda   #$00
         sec
         rol   A
         bne   L10FA

         dfb   $00
         dfb   $dc

L10FA    clc
         php
         sec
         plp
         bcc   L1102

         dfb   $00
         dfb   $00

L1102    sec
         bcs   L1106

L1105    clc
L1106    lda   $33
         beq   L1105
         bcs   L110E
         lda   $44
L110E    nop
         rep   #$20
         mx    %01
         sep   #$10
         jsr   L112A
         rep   #$30
         mx    %00
         jsr   L1130
         sep   #$30
         mx    %11
         jsr   L1130
         rep   #$20
         mx    %01
         sep   #$10
         jsr   L112A
         sep   #$30
         mx    %11
         rts

         mx    %01
L112A    lda   #$1234
         ldx   #$ff
         rts

         mx    %11
L1130    lda   #$ff
         ldx   #$ee
         ldy   #$dd
         rts

