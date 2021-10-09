         org   $011000
         org   *+$010000
         org   *+$010000
L31000   ldal  L31000
:L31004  ldal  :L31004
         andl  :L1101F
         jml   :L21010

         org   $021010
:L21010  ldal  :L21010
         jml   :L11018

         org   $011018
:L11018  ldal  :L11018
         jmp   :L11026

:L1101F  dfb   $80
         org   $0000
         dfb   $ea
         dfb   $60
         adrl  :L31004

         org   $011026
:L11026  nop
         rts

