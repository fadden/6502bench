; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
;
; Segment #2 : data, loads at $FFE0 in arbitrary bank and overflows

        REL                 ;generate relocatable code

ADDR_FFE0 ENT

start   adrl    start
        hex     000102030405060708090a0b0c0d0e0f
        hex     101112131415161718191a1b1c1d1e1f
        hex     202122232425262728292a2b2c2d2e2f
