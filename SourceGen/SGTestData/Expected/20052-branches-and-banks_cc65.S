         .setcpu "65816"
zero     =       $00
longsym  =       $123456

;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$30
         jml     L440000

lodat:   .byte   $00
         .byte   $01
         .byte   $02

;         .segment "SEG001"
         .org    $440000
L440000: cmp     L440000
L440004: lda     L440000
         lda     a:L440000 & $ffff
         lda     zero
         bmi     L440004
         .byte   $62,$b2,$ff
         .byte   $d0,$b0
         .byte   $82,$a9,$ff

dat44:   .word   dat44 & $ffff
         .faraddr dat44

;         .segment "SEG002"
         .org    $44ffc0
L44FFC0: cmp     L44FFC0
high44:  beq     @L44FFCB
         .byte   $30,$3c
         .byte   $82,$39,$00

@L44FFCB: jml    @L2000

;         .segment "SEG003"
         .org    $2000
@L2000:  bit     @L2000
         pea     dat44 & $ffff
         pea     dat44 >> 16
         bne     @L200E
         jml     [lodat]

@L200E:  nop
         jsr     j2
j2:      jsr     j2+3
         jsr     j2-3
         jsl     longsym
         rts

