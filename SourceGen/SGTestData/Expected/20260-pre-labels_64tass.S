        .cpu    "6502"
zzz     =       $103b


*       =       $1000
begin   bit     begin
        nop
        nop
        nop
        jmp     part2

        .logical $2000
part2   bit     part2
        nop
        lda     _local1
        lda     local2
        lda     local4
        nop
        nop
        nop
        bit     b4_part3
        jmp     part3

_local1 .byte   $81
local2  .byte   $82

b4_part3
        .logical $3000
part3   bit     part3
        nop
        lda     local2
        lda     _local3
        lda     local4
        nop
        nop
        nop
        bit     X_b4_part4
        jmp     part4

_local3 .byte   $83
local4  .byte   $84
        .here
        .here

        .logical $4000
b4_part4a
        .logical $5000
X_b4_part4
        .logical $6000
part4   bit     part4
        bit     X_b4_part4
        nop
        nop
        nop
        jmp     part4a

        .here
part4a  bit     part4a
        bit     b4_part4a
        jsr     part4b
        .here
        .byte   $00

part4b  bit     part4b
        bit     zzz
        jmp     _LF000

        .here
        .logical $f000
_LF000  nop
        bne     $f003
        .here
        .logical $0000
        .byte   $ea
        .byte   $00
        .here
