         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
start:   clc
         xce
         sep     #$30
         lda     start
         lda     $2000
         phk
         plb
         lda     start
         lda     $2000
         lda     #$02
         pha
         plb
         lda     $1000
         lda     bank2 & $ffff
         ldx     #$08
         tax
         pha
         plb
         jsl     bank2
         jsl     L24000
         lda     #$02
         pha
         plb
         jsl     bank3
         phk
         plb
         bit     start
         jml     L2400F

L103A:   .word   @L103C

@L103C:  rts

L103D:   .faraddr L2202E

;         .segment "SEG001"
         .org    $022000
bank2:   lda     bank2
         lda     bank2 & $ffff
         ldx     #$00
         jsr     @L2202B & $ffff
         jsr     @L22025 & $ffff
         jsr     @L22028 & $ffff
         nop
         lda     #$03
         pha
         plb
         lda     L33028 & $ffff
         rep     #$30
         .a16
         .i16
         lda     #$00a9
         pha
         plb
         plb
         sep     #$30
         .a8
         .i8
         rtl

@L22025: jmp     (L103A)

@L22028: jml     [L103D]

@L2202B: jmp     (bank2addr & $ffff,x)

L2202E:  nop
         rts

bank2addr: .word L2202E & $ffff

;         .segment "SEG002"
         .org    $033000
bank3:   lda     bank3
         lda     bank2 & $ffff
         jsr     @L33020 & $ffff
         beq     @L33024
         phk
;         .segment "SEG003"
         .org    $033020
@L33020: plb
         nop
         bra     @L33024

@L33024: lda     $2030
         rtl

L33028:  .word   L33028 & $ffff

;         .segment "SEG004"
         .org    $024000
L24000:  lda     L24000
         phb
         phk
         plb
         lda     bank2 & $ffff
         lda     $3000
         plb
         rtl

L2400F:  rts

