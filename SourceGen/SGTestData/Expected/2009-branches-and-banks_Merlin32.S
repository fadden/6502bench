longsym  equ   $123456

         org   $1000
         clc
         xce
         sep   #$30
         jmp   L0000

         org   $0000
L0000    bit:  L0000
L0003    lda   L0000
         lda   L0003
         dfb   $d0,$ba
         dfb   $30,$b8
         dfb   $62,$b5,$ff
         bvs   L0016
         brl   L0080

lodat    dfb   $00
         dfb   $01
         dfb   $02

L0016    lda   lodat+1
         dfb   $82,$a5,$ff

         org   $0080
L0080    bit:  L0080
         jml   L440000

         org   $ffc0
LFFC0    bit   LFFC0
LFFC3    dfb   $82,$3d,$00

         org   $440000
L440000  cmpl  L440000
L440004  ldal  L440000
         lda:  L440000
         lda   L0000
         bmi   L440004
         dfb   $62,$b2,$ff
         dfb   $d0,$b0
         dfb   $82,$a9,$ff

dat44    dw    dat44
         adr   dat44

         org   $44ffc0
L44FFC0  cmpl  L44FFC0
high44   beq   L44FFCB
         dfb   $30,$3c
         dfb   $82,$39,$00

L44FFCB  jml   L2000

         org   $2000
L2000    bit   L2000
         pea   dat44
         pea   ^dat44
         bne   L200E
         jml   [lodat]

L200E    nop
         jsr   j2
j2       jsr   j2+3
         jsr   j2-3
         jsl   longsym
         rts

