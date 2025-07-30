         .setcpu "6502"
         .org    $2b00
         jsr     L2C00
         jsr     L2C05
         jsr     L2C07
         jsr     L2C0D
         jsr     L2C10
         jsr     L2C17
         jsr     L2C18
         jsr     L2C19
         jsr     L2C1A
         jsr     L2C1F
         jsr     L2C23
         jsr     site4a
         jsr     site4b
         jmp     L2D00

         .res    214,$2b

L2C00:   bit     $ff58
         bvs     L2C0C
L2C05:   sec
         .byte   $90
L2C07:   clc
         clv
         bvc     L2C0C

         .byte   $80

L2C0C:   rts

L2C0D:   lda     #$00
         .byte   $2c
L2C10:   lda     #$ff
         sta     @L2C16
         rts

@L2C16:  .byte   $cc

L2C17:   .byte   $2c
L2C18:   .byte   $2c
L2C19:   .byte   $2c
L2C1A:   .byte   $2c
         .byte   $2c
         nop
         nop
         rts

L2C1F:   lda     #$1f
         sec
         .byte   $90
L2C23:   .byte   $a9
         .byte   $f0
         clc
         nop
         bcc     @L2C2A
         asl     A
@L2C2A:  nop
         rts

site4a:  lda     site4b+1
         rts

site4b:  lda     site4a
         rts

         .res    204,$2c

L2D00:   rts

         .res    255,$2d
