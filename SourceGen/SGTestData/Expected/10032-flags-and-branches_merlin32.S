         org   $1000
         clc
         xce
         sep   #$ff
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
         bpl   L1050

         dfb   $00
         dfb   $00

L1050    sep   #$80
         bpl   :L1056
         bmi   :L1058

:L1056   dfb   $00
         dfb   $00

:L1058   rep   #$40
         bvc   :L105E

         dfb   $00
         dfb   $00

:L105E   sep   #$40
         bvs   :L1064

         dfb   $00
         dfb   $00

:L1064   rep   #$01
         bcc   :L106A

         dfb   $00
         dfb   $00

:L106A   sep   #$01
         bcs   :L1070

         dfb   $00
         dfb   $00

:L1070   rep   #$02
         bne   :L1076

         dfb   $00
         dfb   $00

:L1076   sep   #$02
         beq   :L107C

         dfb   $00
         dfb   $00

:L107C   rep   #$20
         mx    %01
         php
         plp
         lda   #$1234
         sep   #$20
         mx    %11
         php
         plp
         lda   #$12
         rep   #$20
         mx    %01
         sep   #$10
         jsr   :L10A6
         rep   #$30
         mx    %00
         jsr   :L10AC
         sep   #$30
         mx    %11
         jsr   :L10AC
         rep   #$20
         mx    %01
         sep   #$10
         jsr   :L10A6
         sep   #$30
         mx    %11
         jmp   :L10B3

         mx    %01
:L10A6   lda   #$1234
         ldx   #$ff
         rts

         mx    %11
:L10AC   lda   #$ff
         ldx   #$ee
         ldy   #$dd
         rts

:L10B3   rts

