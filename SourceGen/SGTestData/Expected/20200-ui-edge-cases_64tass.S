        .cpu    "6502"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
*       =       $2000
        jmp     L2100

        .text   "hello, "  ;string should be split by no-op addr change
        .logical $200a
        .text   "world"
        .byte   $80
        .here

        .logical $2100
L2100   lda     #$00
        sta     addr1-1    ;edit this operand
        sta     addr1
        sta     addr1+1
        jmp     L2121

        .text   "testing stuff."
addr1   .text   "!?---"

L2121   rts

        .here
