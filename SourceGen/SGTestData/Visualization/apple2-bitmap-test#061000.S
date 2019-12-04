; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

HGR     equ     $f3e2
src_ptr equ     $00
xoff    equ     $02
width   equ     $03
line    equ     $04
count   equ     $05

        org     $1000

        bit     bitmap1     ;give the disassembler a reason to label them
        bit     bitmap2
        bit     bitmap3
        bit     bitmap4
        bit     bitmap5

        jsr     HGR         ;clear screen, set hi-res mode
        lda     #<bitmap1
        sta     src_ptr
        lda     #>bitmap1
        sta     src_ptr+1
        lda     #$00
        sta     xoff
        lda     #$01        ;1x8
        sta     width
        jsr     draw

        lda     #<bitmap2
        sta     src_ptr
        lda     #>bitmap2
        sta     src_ptr+1
        lda     #$03        ;start in odd column
        sta     xoff
        lda     #$01        ;1x8
        sta     width
        jsr     draw

        lda     #<bitmap3
        sta     src_ptr
        lda     #>bitmap3
        sta     src_ptr+1
        lda     #$06
        sta     xoff
        lda     #$02        ;2x8
        sta     width
        jsr     draw

        lda     #<bitmap4
        sta     src_ptr
        lda     #>bitmap4
        sta     src_ptr+1
        lda     #$0a
        sta     xoff
        lda     #$02        ;2x8
        sta     width
        jsr     draw

        lda     #<bitmap5
        sta     src_ptr
        lda     #>bitmap5
        sta     src_ptr+1
        lda     #$0e
        sta     xoff
        lda     #$03        ;3x8
        sta     width
        jsr     draw

        rts

; Copies an Nx8 bitmap to the hi-res screen.
;
; Put X offset in "xoff", width-1 in "width_m1", and a pointer to the bitmap
; data in "src_ptr".
draw
        ldy     #$00        ;index to input byte
        sty     line        ;init line index to zero
:loop   ldx     line
        lda     addrs,x
        beq     :done
        sta     _copy+2
        lda     xoff
        sta     _copy+1

        ldx     #$00
:loop1  lda     (src_ptr),y
_copy   sta     $2000,x
        iny
        inx
        cpx     width
        bne     :loop1
        inc     line
        bne     :loop       ;always (more or less)

:done   rts

; hi byte of base addresses on hi-res screen; must be 8 of them
addrs
        dfb     $20
        dfb     $24
        dfb     $28
        dfb     $2c
        dfb     $30
        dfb     $34
        dfb     $38
        dfb     $3c
        dfb     $00

; 1x8
bitmap1
        dfb     $00
        dfb     $01
        dfb     $02
        dfb     $04
        dfb     $08
        dfb     $10
        dfb     $20
        dfb     $40

; 1x8 (odd byte)
bitmap2
        dfb     $c0
        dfb     $a0
        dfb     $90
        dfb     $88
        dfb     $84
        dfb     $82
        dfb     $81
        dfb     $80

; 2x8
bitmap3
        dfb     $d5,$aa     ;pure colors
        dfb     $aa,$d5
        dfb     $55,$2a
        dfb     $2a,$55
        dfb     $d5,$2a     ;high bit split
        dfb     $aa,$55
        dfb     $55,$aa
        dfb     $2a,$d5

; 2x8
bitmap4
        dfb     $40,$00     ;bits at the edge
        dfb     $00,$01
        dfb     $40,$01
        dfb     $60,$03
        dfb     $40,$02
        dfb     $d5,$d5     ;same byte odd/even
        dfb     $aa,$aa
        dfb     $d5,$55

; 3x8 (do in B&W)
bitmap5
        dfb     $7f,$7f,$7f
        dfb     $03,$08,$60
        dfb     $03,$1c,$60
        dfb     $03,$3e,$60
        dfb     $03,$7f,$60
        dfb     $43,$77,$61
        dfb     $63,$63,$63
        dfb     $7f,$7f,$7f

