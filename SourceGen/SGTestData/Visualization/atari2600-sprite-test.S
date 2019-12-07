; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $f000

        bit     sprite1
        bit     sprite2
        bit     sprite3
        nop
        bit     playfield1
        bit     playfield2
        bit     playfield3
        rts

; Atari Adventure: GfxKey (1x3)
sprite1
        dfb     $07
        dfb     $fd
        dfb     $a7
        dfb     $00

; Atari Adventure: GfxDrag0 (1x20)
sprite2
        dfb     $06
        dfb     $0f
        dfb     $f3
        dfb     $fe
        dfb     $0e
        dfb     $04
        dfb     $04
        dfb     $1e
        dfb     $3f
        dfb     $7f
        dfb     $e3
        dfb     $c3
        dfb     $c3
        dfb     $c7
        dfb     $ff
        dfb     $3c
        dfb     $08
        dfb     $8f
        dfb     $e1
        dfb     $3f
        dfb     $00

; Atari Adventure: GfxDrag1 (1x22)
sprite3
        dfb     $80
        dfb     $40
        dfb     $26
        dfb     $1f
        dfb     $0b
        dfb     $0e
        dfb     $1e
        dfb     $24
        dfb     $44
        dfb     $8e
        dfb     $1e
        dfb     $3f
        dfb     $7f
        dfb     $7f
        dfb     $7f
        dfb     $7f
        dfb     $3e
        dfb     $1c
        dfb     $08
        dfb     $f8
        dfb     $80
        dfb     $e0
        dfb     $00


; Atari Adventure: BlackMaze3 -- x7 not reflected
playfield1
        dfb     $f0,$f0,$ff
        dfb     $30,$00,$00
        dfb     $30,$3f,$ff
        dfb     $00,$30,$00
        dfb     $f0,$f0,$ff
        dfb     $30,$00,$03
        dfb     $f0,$f0,$ff

; Atari Adventure: RedMazeBottom -- x7 reflected
playfield2
        dfb     $f0,$33,$cf
        dfb     $f0,$30,$00
        dfb     $f0,$33,$ff
        dfb     $00,$33,$00
        dfb     $f0,$ff,$00
        dfb     $00,$00,$00
        dfb     $f0,$ff,$0f

; Atari Adventure: CastleDef -- x7 reflected
playfield3
        dfb     $f0,$fe,$15
        dfb     $30,$03,$1f
        dfb     $30,$03,$ff
        dfb     $30,$00,$ff
        dfb     $30,$00,$3f
        dfb     $30,$00,$00
        dfb     $f0,$ff,$0f
