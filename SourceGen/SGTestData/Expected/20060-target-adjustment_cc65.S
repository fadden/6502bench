         .setcpu "6502"
         .org    $1000
load11:  lda     #$11
@L1002:  ldx     #$22
@load33: ldy     #$33
@L1006:  lda     #$44
@predat: bne     @L1042

         .word   $0123
@dat1:   .word   $4567
         .word   $89ab
@L1010:  .word   $cdef
@L1012:  .word   $0011
@L1014:  .word   $2233
         .byte   $80
         .byte   "The quick brown fox"
         .byte   $80
         .word   @L1042
         .word   @L1041
         .word   @L1042+1
@fill0:  .res    16,$00
@L1041:  .byte   $80

@L1042:  lda     @predat+2
         lda     @L1041
         asl     @dat1
         rol     @dat1+2
         ror     @L1010
         and     @L1012
         ora     @L1014
         lda     @fill0
         sta     @fill0+4
         lda     @fill0+8
         sta     @fill0+12
         jsr     @L1002
@L1066:  lda     @L1006
@L1069:  bit     @L1066+2
         ora     @L1066+2
         lda     @L1069+1
         lda     @L1069+2
         lda     #$ea
@L1077:  sta     @L1077
@L107A:  sta     @L107A+1
         sta     $107f      ;self-ref; operand format refs nonexistent symbol
         jmp     @L2002

         .byte   $80
@dat81:  .byte   $81
         .org    $2000
@L2000:  .byte   $82
         .byte   $83

@L2002:  nop
         nop
         lda     @dat81
         lda     @L2000
         jmp     @L2018

@L200D:  .byte   $7c
@L200E:  .byte   $7d
@L200F:  .byte   $7e
         .byte   $7f
@nearby: .byte   $80
         .byte   $81
         .byte   $82
         .byte   $83
@L2015:  .byte   $84
@L2016:  .byte   $85
@L2017:  .byte   $86

@L2018:  lda     @L200D
         lda     @L200E
         lda     @L200F
         lda     @nearby-1
         lda     @nearby
         lda     @nearby+1
         lda     @nearby+2
         lda     @nearby+3
         lda     @L2015
         lda     @L2016
         lda     @L2017
         rts

