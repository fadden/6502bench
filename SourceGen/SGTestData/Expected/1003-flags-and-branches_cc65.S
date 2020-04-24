         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$ff
         clv
         cld
         cli
         clc
         lda     #$80
         lda     #$01
         sed
         sei
         sec
         lda     #$ff
         adc     #$00
         sep     #$ff
         rep     #$80
         rep     #$40
         rep     #$20
         .a16
         rep     #$10
         .i16
         rep     #$08
         rep     #$04
         rep     #$02
         rep     #$01
         sep     #$00
         sep     #$ff
         .a8
         .i8
         rep     #$00
         rep     #$ff
         .a16
         .i16
         lda     #$feed
         sec
         xce
         .a8
         .i8
         lda     #$ff
         rep     #$30
         lda     #$ff
         clc
         xce
         lda     #$ff
         rep     #$20
         .a16
         sep     #$10
         lda     #$0000
         ldx     #$01
         ldy     #$02
         sep     #$20
         .a8
         rep     #$10
         .i16
         lda     #$01
         ldx     #$0000
         ldy     #$0000
         sep     #$30
         .i8
         lda     #$00
         pha
         plp
         rep     #$80
         bpl     L105F

         .byte   $00
         .byte   $00

L105F:   sep     #$80
         bpl     @L1065
         bmi     @L1067

@L1065:  .byte   $00
         .byte   $00

@L1067:  rep     #$40
         bvc     @L106D

         .byte   $00
         .byte   $00

@L106D:  sep     #$40
         bvs     @L1073

         .byte   $00
         .byte   $00

@L1073:  rep     #$01
         bcc     @L1079

         .byte   $00
         .byte   $00

@L1079:  sep     #$01
         bcs     @L107F

         .byte   $00
         .byte   $00

@L107F:  rep     #$02
         bne     @L1085

         .byte   $00
         .byte   $00

@L1085:  sep     #$02
         beq     @L108B

         .byte   $00
         .byte   $00

@L108B:  sep     #$ff
         lda     #$01
         bne     @L1093

         .byte   $00
         .byte   $db

@L1093:  lda     #$00
         beq     @L1099

         .byte   $00
         .byte   $db

@L1099:  bpl     @L109D

         .byte   $00
         .byte   $db

@L109D:  lda     #$80
         bmi     @L10A3

         .byte   $00
         .byte   $db

@L10A3:  lda     #$ff
         and     #$00
         beq     @L10AB

         .byte   $00
         .byte   $db

@L10AB:  lda     #$00
         and     #$ff
         beq     @L10B3

         .byte   $00
         .byte   $db

@L10B3:  lda     #$ff
         and     #$7f
         bne     @L10BB
         brk

         .byte   $db

@L10BB:  bpl     @L10BF

         .byte   $00
         .byte   $db

@L10BF:  lda     #$ff
         and     #$80
         bmi     @L10C7

         .byte   $00
         .byte   $db

@L10C7:  lda     #$00
         ora     #$00
         beq     @L10CF

         .byte   $00
         .byte   $db

@L10CF:  ora     #$01
         bne     @L10D5

         .byte   $00
         .byte   $db

@L10D5:  lda     #$00
         ora     #$7f
         bpl     @L10DD

         .byte   $00
         .byte   $db

@L10DD:  ora     #$80
         bmi     @L10E3

         .byte   $00
         .byte   $db

@L10E3:  lda     @L10E3
         sec
         ror     A
         bmi     @L10EC

         .byte   $00
         .byte   $dc

@L10EC:  clc
         ror     A
         bpl     @L10F2

         .byte   $00
         .byte   $dc

@L10F2:  lda     #$00
         sec
         rol     A
         bne     @L10FA

         .byte   $00
         .byte   $dc

@L10FA:  lda     #$ff
         lsr     A
         bpl     @L1101

         .byte   $00
         .byte   $dd

@L1101:  clc
         php
         sec
         plp
         bcc     @L1109

         .byte   $00
         .byte   $00

@L1109:  sec
         bcs     @L110D

@L110C:  clc
@L110D:  lda     $33
         beq     @L110C
         bcs     @L1115
         lda     $44
@L1115:  nop
         rep     #$20
         .a16
         sep     #$10
         jsr     @L1131
         rep     #$30
         .i16
         jsr     @L1137
         sep     #$30
         .a8
         .i8
         jsr     @L1137
         rep     #$20
         .a16
         sep     #$10
         jsr     @L1131
         sep     #$30
         .a8
         rts

         .a16
@L1131:  lda     #$1234
         ldx     #$ff
         rts

         .a8
@L1137:  lda     #$ff
         ldx     #$ee
         ldy     #$dd
         rts

