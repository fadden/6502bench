         .setcpu "65816"
         .org    $021000
         .a8
         .i8
         clc
         xce
         sep     #$ff
         jsr     L21100 & $ffff
         jsr     L21107 & $ffff
         jmp     L22000 & $ffff

         .org    $021100
L21100:  bit     L21100 & $ffff
L21103:  lda     #$11
         ldx     #$11
L21107:  ldy     #$11
         per     L21103
         bra     L21103

         .org    $021100
@L21100_0: bit   @L21100_0 & $ffff
         lda     #$22
@L21105: ldx     #$22
         ldy     #$22
         per     @L21105
         jmp     @L21105 & $ffff

         .org    $021100
@L21100_1: bit   @L21100_1 & $ffff
         lda     #$33
         ldx     #$33
@L21107_0: ldy   #$33
         per     @L21107_0
         bra     @L21107_0

         .org    $022000
L22000:  bit     L22000 & $ffff
         beq     $022018
         bra     @L22020

         .org    $022020
@L22020: bit     @L22020 & $ffff
         beq     $022029
         brl     @L22080

@offend: nop
         .org    $022080
@L22080: bit     @L22080 & $ffff
         lda     @offend & $ffff
         jsr     @offend & $ffff
         lda     $2029
         jsr     $2029
         lda     @L22080 & $ffff -1
         jsr     @L22080 & $ffff -1
         lda     @L22080 & $ffff
         jsr     @L22080 & $ffff
         lda     $00
         beq     @L22100
         .byte   $ad

         .org    $022100
@L22100: nop
         nop
         jmp     @L23000 & $ffff

         .org    $022800
         .byte   $00
         .byte   $28
         .res    14,$00
         .org    $022820
         .res    18,$00

         .org    $023000
@L23000: bit     @L23000 & $ffff
         lda     #$44
         ldx     #$44
         ldy     #$44
         brl     @fwd

@ulabel: .byte   $00
         .byte   $01
         .org    $023100
         .byte   $02

@fwd:    bit     @fwd & $ffff
         lda     @ulabel & $ffff
         lda     @ulabel & $ffff +1
         lda     $300e
         lda     $300f
         lda     @fwd & $ffff -1
         beq     @L23182
         .byte   $ea
         .byte   $ea
         .org    $023180
         .byte   $00
         .byte   $01

@L23182: bit     @L23182 & $ffff
         lda     @label1 & $ffff
         lda     @label1 & $ffff +1
         lda     @label1 & $ffff +112
         bra     @L23200

@label1: .byte   $ea
         .byte   $ea

         .org    $023200
@L23200: bit     @L23200 & $ffff
         .byte   $00
         .byte   $01
