         org   $1000
         dw    $1000      ;PRG-style header

         org   $1000
         jsr   L1100
         jsr   L1107
         jmp   L2000

         org   $1100
L1100    bit   L1100
L1103    lda   #$11
         ldx   #$11
L1107    ldy   #$11
         clv
         bvc   L1103

         org   $1100
:L1100_0 bit   :L1100_0
         lda   #$22
:L1105   ldx   #$22
         ldy   #$22
         jmp   :L1105

         org   $1100
:L1100_1 bit   :L1100_1
         lda   #$33
         ldx   #$33
:L1107_0 ldy   #$33
         sec
         bcs   :L1107_0

         org   $2000
L2000    bit   L2000
         beq   $2018
         bne   :L2020

         org   $2020
:L2020   bit   :L2020
         beq   $2028
         bne   L2080

offend   nop
         org   $2080
L2080    bit   L2080
         lda   offend
         jsr   offend
         lda   $2028
         jsr   $2028
         lda   L2080-1
         jsr   L2080-1
         lda   L2080
         jsr   L2080
         lda   $00
         beq   :L2100
         dfb   $ad

         org   $2100
:L2100   nop
         nop
         jmp   :L3000

         org   $2800
         dfb   $00
         dfb   $28
         ds    14
         org   $2820
         ds    18

         org   $3000
:L3000   bit   :L3000
         lda   #$44
         ldx   #$44
         ldy   #$44
         jmp   fwd

ulabel   dfb   $00
         dfb   $01
         org   $3100
         dfb   $02

fwd      bit   fwd
         lda   ulabel
         lda   ulabel+1
         lda   $300e
         lda   $300f
         lda   fwd-1
         beq   :L3182
         dfb   $ea
         dfb   $ea
         org   $3180
         dfb   $00
         dfb   $01

:L3182   bit   :L3182
         lda   label1
         lda   label1+1
         lda   L3200
         clv
         bvc   L3200

label1   dfb   $ea
         dfb   $ea

         org   $3200
L3200    bit   L3200
         dfb   $00
         dfb   $01        ;execution continues off end of file
