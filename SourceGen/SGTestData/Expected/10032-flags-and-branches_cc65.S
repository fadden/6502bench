         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$ff
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
         bpl     L1050

         .byte   $00
         .byte   $00

L1050:   sep     #$80
         bpl     @L1056
         bmi     @L1058

@L1056:  .byte   $00
         .byte   $00

@L1058:  rep     #$40
         bvc     @L105E

         .byte   $00
         .byte   $00

@L105E:  sep     #$40
         bvs     @L1064

         .byte   $00
         .byte   $00

@L1064:  rep     #$01
         bcc     @L106A

         .byte   $00
         .byte   $00

@L106A:  sep     #$01
         bcs     @L1070

         .byte   $00
         .byte   $00

@L1070:  rep     #$02
         bne     @L1076

         .byte   $00
         .byte   $00

@L1076:  sep     #$02
         beq     @L107C

         .byte   $00
         .byte   $00

@L107C:  rep     #$20
         .a16
         php
         plp
         lda     #$1234
         sep     #$20
         .a8
         php
         plp
         lda     #$12
         rep     #$20
         .a16
         sep     #$10
         jsr     @L10A6
         rep     #$30
         .i16
         jsr     @L10AC
         sep     #$30
         .a8
         .i8
         jsr     @L10AC
         rep     #$20
         .a16
         sep     #$10
         jsr     @L10A6
         sep     #$30
         .a8
         jmp     @L10B3

         .a16
@L10A6:  lda     #$1234
         ldx     #$ff
         rts

         .a8
@L10AC:  lda     #$ff
         ldx     #$ee
         ldy     #$dd
         rts

@L10B3:  rts

