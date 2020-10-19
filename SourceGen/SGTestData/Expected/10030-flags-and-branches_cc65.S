         .setcpu "6502X"
;         .segment "SEG000"
         .org    $1000
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
         lda     #$00
         pha
         plp
         clv
         bvc     L101A

         .byte   $70
         .byte   $02
         .byte   $00
         .byte   $00

L101A:   clc
         bcc     @L101F

         .byte   $00
         .byte   $00

@L101F:  sec
         bcs     @L1024

         .byte   $00
         .byte   $00

@L1024:  lda     #$01
         bne     @L102A

         .byte   $00
         .byte   $db

@L102A:  lda     #$00
         beq     @L1030

         .byte   $00
         .byte   $db

@L1030:  bpl     @L1034

         .byte   $00
         .byte   $db

@L1034:  lda     #$80
         bmi     @L103A

         .byte   $00
         .byte   $db

@L103A:  lda     #$ff
         and     #$00
         beq     @L1042

         .byte   $00
         .byte   $db

@L1042:  lda     #$00
         ldx     #$80
         and     #$ff
         beq     @L104C
         bne     @L104C

@L104C:  lda     #$ff
         ldx     #$00
         and     #$7f
         beq     @L1056
         bne     @L1056

@L1056:  bpl     @L105A

         .byte   $00
         .byte   $db

@L105A:  lda     #$ff
         and     #$80
         bpl     @L1064
         bmi     @L1064

         .byte   $00
         .byte   $db

@L1064:  lda     #$00
         ldx     #$80
         bne     @L106C

         .byte   $00
         .byte   $db

@L106C:  ora     #$00
         beq     @L1072
         bne     @L1072

@L1072:  ora     #$01
         bne     @L1078

         .byte   $00
         .byte   $db

@L1078:  lda     #$00
         ldx     #$80
         bmi     @L1080

         .byte   $00
         .byte   $db

@L1080:  ora     #$7f
         bpl     @L1088
         bmi     @L1088

         .byte   $00
         .byte   $db

@L1088:  ora     #$80
         bmi     @L108E

         .byte   $00
         .byte   $db

@L108E:  lda     @L108E
         sec
         ror     A
         bmi     @L1097

         .byte   $00
         .byte   $dc

@L1097:  clc
         ror     A
         bpl     @L109D

         .byte   $00
         .byte   $dc

@L109D:  lda     #$00
         sec
         rol     A
         bne     @L10A5

         .byte   $00
         .byte   $dc

@L10A5:  lda     #$ff
         lsr     A
         bpl     @L10AC

         .byte   $00
         .byte   $dd

@L10AC:  clc
         php
         sec
         plp
         bcc     @L10B3
         nop
@L10B3:  sec
         bcs     @L10B7

@L10B6:  clc
@L10B7:  lda     $33
         beq     @L10B6
         bcs     @L10BF
         lda     $44
@L10BF:  nop
         rts

