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
         bpl   :L1065
         bmi   :L1067

:L1065   dfb   $00
         dfb   $00

:L1067   rep   #$40
         bvc   :L106D

         dfb   $00
         dfb   $00

:L106D   sep   #$40
         bvs   :L1073

         dfb   $00
         dfb   $00

:L1073   rep   #$01
         bcc   :L1079

         dfb   $00
         dfb   $00

:L1079   sep   #$01
         bcs   :L107F

         dfb   $00
         dfb   $00

:L107F   rep   #$02
         bne   :L1085

         dfb   $00
         dfb   $00

:L1085   sep   #$02
         beq   :L108B

         dfb   $00
         dfb   $00

:L108B   sep   #$ff
         lda   #$01
         bne   :L1093

         dfb   $00
         dfb   $db

:L1093   lda   #$00
         beq   :L1099

         dfb   $00
         dfb   $db

:L1099   bpl   :L109D

         dfb   $00
         dfb   $db

:L109D   lda   #$80
         bmi   :L10A3

         dfb   $00
         dfb   $db

:L10A3   lda   #$ff
         and   #$00
         beq   :L10AB

         dfb   $00
         dfb   $db

:L10AB   lda   #$00
         ldx   #$80
         and   #$ff
         beq   :L10B5
         bne   :L10B5

:L10B5   lda   #$ff
         ldx   #$00
         and   #$7f
         beq   :L10BF
         bne   :L10BF

:L10BF   bpl   :L10C3

         dfb   $00
         dfb   $db

:L10C3   lda   #$ff
         and   #$80
         bmi   :L10CB
         brk

         dfb   $db

:L10CB   lda   #$00
         ldx   #$80
         bne   :L10D3

         dfb   $00
         dfb   $db

:L10D3   ora   #$00
         beq   :L10D9
         bne   :L10D9

:L10D9   ora   #$01
         bne   :L10DF

         dfb   $00
         dfb   $db

:L10DF   lda   #$00
         ldx   #$80
         bmi   :L10E7

         dfb   $00
         dfb   $db

:L10E7   ora   #$7f
         bpl   :L10EF
         bmi   :L10EF

         dfb   $00
         dfb   $db

:L10EF   ora   #$80
         bmi   :L10F5

         dfb   $00
         dfb   $db

:L10F5   lda   :L10F5
         sec
         ror   A
         bmi   :L10FE

         dfb   $00
         dfb   $dc

:L10FE   clc
         ror   A
         bpl   :L1104

         dfb   $00
         dfb   $dc

:L1104   lda   #$00
         sec
         rol   A
         bne   :L110C

         dfb   $00
         dfb   $dc

:L110C   lda   #$ff
         lsr   A
         bpl   :L1113

         dfb   $00
         dfb   $dd

:L1113   clc
         php
         sec
         plp
         bcc   :L111B

         dfb   $00
         dfb   $00

:L111B   sec
         bcs   :L111F

:L111E   clc
:L111F   lda   $33
         beq   :L111E
         bcs   :L1127
         lda   $44
:L1127   nop
         rep   #$20
         mx    %01
         sep   #$10
         jsr   :L1143
         rep   #$30
         mx    %00
         jsr   :L1149
         sep   #$30
         mx    %11
         jsr   :L1149
         rep   #$20
         mx    %01
         sep   #$10
         jsr   :L1143
         sep   #$30
         mx    %11
         rts

         mx    %01
:L1143   lda   #$1234
         ldx   #$ff
         rts

         mx    %11
:L1149   lda   #$ff
         ldx   #$ee
         ldy   #$dd
         rts

