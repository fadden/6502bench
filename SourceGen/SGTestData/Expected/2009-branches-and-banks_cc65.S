         .setcpu "65816"
longsym  =       $123456

;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         clc
         xce
         sep     #$30
         jmp     L0000

;         .segment "SEG001"
         .org    $0000
L0000:   bit     a:L0000
L0003:   lda     L0000
         lda     L0003
         .byte   $d0,$ba
         .byte   $30,$b8
         .byte   $62,$b5,$ff
         bcc     L0016
         brl     L0080

lodat:   .byte   $00
         .byte   $01
         .byte   $02

L0016:   lda     lodat+1
         .byte   $82,$a5,$ff

;         .segment "SEG002"
         .org    $0080
L0080:   bit     a:L0080
         jml     L440000

;         .segment "SEG003"
         .org    $ffc0
LFFC0:   bit     LFFC0
LFFC3:   .byte   $82,$3d,$00

;         .segment "SEG004"
         .org    $440000
L440000: cmp     L440000
L440004: lda     L440000
         lda     a:L440000 & $ffff
         lda     L0000
         bmi     L440004
         .byte   $62,$b2,$ff
         .byte   $d0,$b0
         .byte   $82,$a9,$ff

dat44:   .word   dat44 & $ffff
         .faraddr dat44

;         .segment "SEG005"
         .org    $44ffc0
L44FFC0: cmp     L44FFC0
high44:  beq     L44FFCB
         .byte   $30,$3c
         .byte   $82,$39,$00

L44FFCB: jml     L2000

;         .segment "SEG006"
         .org    $2000
L2000:   bit     L2000
         pea     dat44 & $ffff
         pea     dat44 >> 16
         bne     L200E
         jml     [lodat]

L200E:   nop
         jsr     j2
j2:      jsr     j2+3
         jsr     j2-3
         jsl     longsym
         rts

