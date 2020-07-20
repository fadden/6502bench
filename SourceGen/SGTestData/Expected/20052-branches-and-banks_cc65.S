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
         bne     skip
         jml     [lodat]

skip:    nop
         jsr     j2
j2:      jsr     j2+3
         jsr     j2-3
         jsl     longsym
         jml     bank54

;         .segment "SEG004"
         .org    $543210
bank54:  cmp     bank54
         bra     L54321C

backchk: nop
         nop
         rts

backval: .faraddr backchk

L54321C: lda     backchk
         lda     f:fwdchk
         lda     $543216
         lda     $54327d
         lda     backchk & $ffff +1
         lda     backchk & $ffff -1
         lda     fwdchk & $ffff +1
         lda     fwdval & $ffff +2
         nop
         jsr     backchk & $ffff
         jsr     backchk & $ffff +1
         jsr     $3218
         jsr     fwdchk & $ffff
         jsr     fwdchk & $ffff +1
         jsr     $327f
         nop
         ldx     #$00
         jsr     (backval & $ffff,x)
         jsr     (fwdval & $ffff,x)
         jsr     @L54326E & $ffff
         jsr     @L543271 & $ffff
         jsr     @L543268 & $ffff
         jsr     @L54326B & $ffff
         jsr     @L543274 & $ffff
         jsr     @L543277 & $ffff
         bra     L543280

@L543268: jmp    (backval & $ffff,x)

@L54326B: jmp    (fwdval & $ffff,x)

@L54326E: jmp    (lodat)

@L543271: jmp    (lodat)

@L543274: jml    [lodat]

@L543277: jml    [lodat]

fwdval:  .faraddr fwdchk

fwdchk:  nop
         nop
         rts

L543280: jsr     skip
         nop
         rep     #$30
         .a16
         .i16
         php
         lda     #$0000
         sep     #$30
         .a8
         .i8
         lda     #$00
         plp
         lda     #$ea
         nop
         sep     #$30
         php
         lda     #$00
         rep     #$30
         .a16
         .i16
         lda     #$0000
         plp
         lda     #$eaea
         rep     #$30
         nop
         rtl

