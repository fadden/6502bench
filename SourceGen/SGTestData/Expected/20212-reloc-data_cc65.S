         .setcpu "65816"
;         .segment "SEG000"
         .org    $030000
         .a16
         .i16
L30000:  clc
         xce
         .a8
         .i8
         sep     #$30
         lda     L30000
         nop
         jsl     @L23456
         jsl     @L80000
         lda     f:@L23456
         lda     @L23456 & $ffff
         lda     #<@L23456
         lda     #>@L23456
         lda     #^@L23456
         lda     f:@L80019
         lda     a:@L80019 & $ffff
         lda     #<@L80019
         lda     #>@L80019
         lda     #^@L80019
         nop
         rep     #$30
         .a16
         .i16
         lda     #@L80019 & $ffff
         lda     #@L80019 >> 8
         lda     #@L80019 >> 16
         nop
         lda     #$000f
         ldx     #@L80026 & $ffff
         ldy     #@L23467 & $ffff
         mvn     #^@L23467+6,#^@L23467
         nop
         lda     $0000,y
         sta     $0002,y
@L3004B: pea     $0000
         pea     $f000
         pea     @L80019 & $ffff
         pea     @L80019 >> 16
         pea     @L3004B & $ffff
         pea     @L3004B >> 8
@L3005D: pea     @L3004B >> 16
         pea     @L3005D & $ffff
         pea     $104b
         nop
         jmp     @L30073 & $ffff

         .word   @L4FFE0 & $ffff
         .faraddr @L4FFE0
         .faraddr @L4FFE0
         .byte   $00

@L30073: phk
         plb
         lda     #$0000
         asl     A
         asl     A
         tax
         lda     a:@L30088 & $ffff,x
         pha
         lda     a:@L30086 & $ffff,x
         pha
         beq     @L3008E
         rts

@L30086: .byte   $4b
         .byte   $00
@L30088: .byte   $03
         .byte   $00
         .faraddr @L30073
         .byte   $00

@L3008E: rts

;         .segment "SEG001"
         .org    $04ffe0
@L4FFE0: .faraddr @L4FFE0
         .byte   $00
         .byte   $00
         .byte   $01
         .byte   $02
         .byte   $03
         .byte   $04
         .byte   $05
         .byte   $06
         .byte   $07
         .byte   $08
         .byte   $09
         .byte   $0a
         .byte   $0b
         .byte   $0c
         .byte   $0d
         .byte   $0e
         .byte   $0f
         .byte   $10
         .byte   $11
         .byte   $12
         .byte   $13
         .byte   $14
         .byte   $15
         .byte   $16
         .byte   $17
         .byte   $18
         .byte   $19
         .byte   $1a
         .byte   $1b
;         .segment "SEG002"
         .org    $050000
         .byte   $1c
         .byte   $1d
         .byte   $1e
         .byte   $1f
         .byte   " !",$22,"#$%&'()*+,-./"

;         .segment "SEG003"
         .org    $023456
         .a8
         .i8
@L23456: lda     @L23456
         jsr     @L23477 & $ffff
         ldy     #<@L23467
         ora     $3456
         ora     @L23456
         rtl

@L23467: .res    16,$00

@L23477: nop
         rts

;         .segment "SEG004"
         .org    $080000
@L80000: lda     @L80000
         lda     a:@L80019 & $ffff
         nop
         lda     $010000
         lda     $020000
         lda     L30000
         lda     @L80000
         rtl

@L80019: .faraddr @L80019
         .byte   $00
         .faraddr @L23456
         .byte   $00
         .byte   $80
         .byte   $00
         .byte   $10
         .byte   $08
         .byte   $00
@L80026: .byte   "This is a test."
         .byte   $00
;         .segment "SEG005"
         .org    $060000
         .byte   "hello, world!"
