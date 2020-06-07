zero     equ   $00
longsym  equ   $123456

         org   $1000
         clc
         xce
         sep   #$30
         jml   L440000

lodat    dfb   $00
         dfb   $01
         dfb   $02

         org   $440000
L440000  cmpl  L440000
L440004  ldal  L440000
         lda:  L440000
         lda   zero
         bmi   L440004
         dfb   $62,$b2,$ff
         dfb   $d0,$b0
         dfb   $82,$a9,$ff

dat44    dw    dat44
         adr   dat44

         org   $44ffc0
L44FFC0  cmpl  L44FFC0
high44   beq   :L44FFCB
         dfb   $30,$3c
         dfb   $82,$39,$00

:L44FFCB jml   :L2000

         org   $2000
:L2000   bit   :L2000
         pea   dat44
         pea   ^dat44
         bne   :L200E
         jml   [lodat]

:L200E   nop
         jsr   j2
j2       jsr   j2+3
         jsr   j2-3
         jsl   longsym
         rts

