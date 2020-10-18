         org   $1000
start    clc
         xce
         sep   #$30
         lda   start
         lda   $2000
         phk
         plb
         lda   start
         lda   $2000
         lda   #$02
         pha
         plb
         lda   $1000
         lda   bank2
         ldx   #$08
         tax
         pha
         plb
         jsl   bank2
         jsl   L24000
         lda   #$02
         pha
         plb
         jsl   bank3
         phk
         plb
         bit   start
         jml   L2400F

L103A    dw    :L103C

:L103C   rts

L103D    adr   L2202E

         org   $022000
bank2    ldal  bank2
         lda   bank2
         ldx   #$00
         jsr   :L2202B
         jsr   :L22025
         jsr   :L22028
         nop
         lda   #$03
         pha
         plb
         lda   L33028
         rep   #$30
         mx    %00
         lda   #$00a9
         pha
         plb
         plb
         sep   #$30
         mx    %11
         rtl

:L22025  jmp   (L103A)

:L22028  jml   [L103D]

:L2202B  jmp   (bank2addr,x)

L2202E   nop
         rts

bank2addr dw   L2202E

         org   $033000
bank3    ldal  bank3
         lda   bank2
         jsr   :L33020
         beq   :L33024
         phk
         org   $033020
:L33020  plb
         nop
         bra   :L33024

:L33024  lda   $2030
         rtl

L33028   dw    L33028

         org   $024000
L24000   ldal  L24000
         phb
         phk
         plb
         lda   bank2
         lda   $3000
         plb
         rtl

L2400F   rts

