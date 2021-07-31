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
         bne   skip
         jml   [lodat]

skip     nop
         jsr   j2
j2       jsr   j2+3
         jsr   j2-3
         jsl   longsym
         jml   bank54

         org   $543210
bank54   cmpl  bank54
         bra   L54321C

backchk  nop
         nop
         rts

backval  adr   backchk

L54321C  ldal  backchk
         ldal  fwdchk
         ldal  $543216
         ldal  $54327d
         lda   backchk+1
         lda   backchk-1
         lda   fwdchk+1
         lda   fwdval+2
         nop
         jsr   backchk
         jsr   backchk+1
         jsr   $3218
         jsr   fwdchk
         jsr   fwdchk+1
         jsr   $327f
         nop
         ldx   #$00
         jsr   (backval,x)
         jsr   (fwdval,x)
         jsr   :L54326E
         jsr   :L543271
         jsr   :L543268
         jsr   :L54326B
         jsr   :L543274
         jsr   :L543277
         bra   L543280

:L543268 jmp   (backval,x)

:L54326B jmp   (fwdval,x)

:L54326E jmp   (lodat)

:L543271 jmp   (lodat)

:L543274 jml   [lodat]

:L543277 jml   [lodat]

fwdval   adr   $54327d

fwdchk   nop
         nop
         rts

L543280  jsr   skip
         nop
         rep   #$30
         mx    %00
         php
         lda   #$0000
         sep   #$30
         mx    %11
         lda   #$00
         plp
         lda   #$ea
         nop
         sep   #$30
         php
         lda   #$00
         rep   #$30
         mx    %00
         lda   #$0000
         plp
         lda   #$eaea
         rep   #$30
         nop
         jsr   $edcb
         nop
         rtl

