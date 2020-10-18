         org   $021000
         clc
         xce
         sep   #$ff
         jsr   L21100
         jsr   L21107
         jmp   L22000

         org   $021100
L21100   bit   L21100
L21103   lda   #$11
         ldx   #$11
L21107   ldy   #$11
         per   L21103
         bra   L21103

         org   $021100
:L21100_0 bit  :L21100_0
         lda   #$22
:L21105  ldx   #$22
         ldy   #$22
         per   :L21105
         jmp   :L21105

         org   $021100
:L21100_1 bit  :L21100_1
         lda   #$33
         ldx   #$33
:L21107_0 ldy  #$33
         per   :L21107_0
         bra   :L21107_0

         org   $022000
L22000   bit   L22000
         beq   $022018
         bra   :L22020

         org   $022020
:L22020  bit   :L22020
         beq   $022029
         brl   :L22080

:offend  nop
         org   $022080
:L22080  bit   :L22080
         lda   :offend
         jsr   :offend
         lda   $2029
         jsr   $2029
         lda   :L22080-1
         jsr   :L22080-1
         lda   :L22080
         jsr   :L22080
         lda   $00
         beq   :L22100
         dfb   $ad

         org   $022100
:L22100  nop
         nop
         jmp   :L23000

         org   $022800
         dfb   $00
         dfb   $28
         ds    14
         org   $022820
         ds    18

         org   $023000
:L23000  bit   :L23000
         lda   #$44
         ldx   #$44
         ldy   #$44
         brl   :fwd

:ulabel  dfb   $00
         dfb   $01
         org   $023100
         dfb   $02

:fwd     bit   :fwd
         lda   :ulabel
         lda   :ulabel+1
         lda   $300e
         lda   $300f
         lda   :fwd-1
         beq   :L23182
         dfb   $ea
         dfb   $ea
         org   $023180
         dfb   $00
         dfb   $01

:L23182  bit   :L23182
         lda   :label1
         lda   :label1+1
         lda   :label1+112
         bra   :L23200

:label1  dfb   $ea
         dfb   $ea

         org   $023200
:L23200  bit   :L23200
         dfb   $00
         dfb   $01
