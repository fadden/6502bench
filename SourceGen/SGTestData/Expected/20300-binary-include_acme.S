        !cpu    6502
*       =       $1000
L1000   ldy     #$28
        lda     @L1017,y
        sta     $2000,y
        dey
        bpl     L1000
        lda     @L103F
        lda     @L103F+1
        lda     done-1
        jmp     done

@L1017  !binary "20300-1.bin"
@L103F  !binary "20300sub/20300-2.bin"

done    rts

