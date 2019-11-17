         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$ff
         jsr     L1100
         jsr     L1107
         jmp     L2000

;         .segment "SEG001"
         .org    $1100
L1100:   bit     L1100
L1103:   lda     #$11
         ldx     #$11
L1107:   ldy     #$11
         per     L1103
         bra     L1103

;         .segment "SEG002"
         .org    $1100
@L1100_0: bit    @L1100_0
         lda     #$22
@L1105:  ldx     #$22
         ldy     #$22
         per     @L1105
         jmp     @L1105

;         .segment "SEG003"
         .org    $1100
@L1100_1: bit    @L1100_1
         lda     #$33
         ldx     #$33
@L1107_0: ldy    #$33
         per     @L1107_0
         bra     @L1107_0

;         .segment "SEG004"
         .org    $2000
L2000:   bit     L2000
         beq     $2018
         bra     @L2020

;         .segment "SEG005"
         .org    $2020
@L2020:  bit     @L2020
         beq     $2029
         brl     @L2080

@offend: nop
;         .segment "SEG006"
         .org    $2080
@L2080:  bit     @L2080
         lda     @offend
         jsr     @offend
         lda     $2029
         jsr     $2029
         lda     @L2080-1
         jsr     @L2080-1
         lda     @L2080
         jsr     @L2080
         lda     $00
         beq     @L2100
         .byte   $ad

;         .segment "SEG007"
         .org    $2100
@L2100:  nop
         nop
         jmp     @L3000

;         .segment "SEG008"
         .org    $2800
         .byte   $00
         .byte   $28
         .res    14,$00
;         .segment "SEG009"
         .org    $2820
         .res    18,$00

;         .segment "SEG010"
         .org    $3000
@L3000:  bit     @L3000
         lda     #$44
         ldx     #$44
         ldy     #$44
         brl     @fwd

@ulabel: .byte   $00
         .byte   $01
;         .segment "SEG011"
         .org    $3100
         .byte   $02

@fwd:    bit     @fwd
         lda     @ulabel
         lda     @ulabel+1
         lda     $300e
         lda     $300f
         lda     @fwd-1
         beq     @L3182
         .byte   $ea
         .byte   $ea
;         .segment "SEG012"
         .org    $3180
         .byte   $00
         .byte   $01

@L3182:  bit     @L3182
         lda     @label1
         lda     @label1+1
         lda     @label1+112
         bra     @L3200

@label1: .byte   $ea
         .byte   $ea

;         .segment "SEG013"
         .org    $3200
@L3200:  bit     @L3200
         .byte   $00
         .byte   $01
