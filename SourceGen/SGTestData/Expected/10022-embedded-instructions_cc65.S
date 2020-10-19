         .setcpu "65816"
;         .segment "SEG000"
         .org    $1000
         .a8
         .i8
         jsr     L1006
         jmp     L1020

L1006:   sep     #$30
         lda     $00
         beq     @L100F
         lda     #$00
         brk

@L100F:  sta     $012345
         lda     $00
         beq     @L101A
         lda     #$00
         .byte   $85
@L101A:  .byte   $8f
         eor     $23
         .byte   $01
         nop
         rts

L1020:   .byte   $af
