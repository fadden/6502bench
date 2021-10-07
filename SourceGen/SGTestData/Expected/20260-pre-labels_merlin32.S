zzz      equ   $103b

         org   $0000
         dw    $1000

         org   $1000
begin    bit   begin
         nop
         nop
         nop
         jmp   part2

         org   $2000
part2    bit   part2
         nop
         lda   :local1
         lda   local2
         lda   local4
         nop
         nop
         nop
         bit   b4_part3
         jmp   part3

:local1  dfb   $81
local2   dfb   $82

b4_part3
         org   $3000
part3    bit   part3
         nop
         lda   local2
         lda   :local3
         lda   local4
         nop
         nop
         nop
         bit   _b4_part4
         jmp   part4

:local3  dfb   $83
local4   dfb   $84

         org   $4000
b4_part4a
         org   $5000
_b4_part4
         org   $6000
part4    bit   part4
         bit   _b4_part4
         nop
         nop
         nop
         jmp   part4a

         org   $500c
part4a   bit   part4a
         bit   b4_part4a
         jsr   part4b
         org   $4015
         dfb   $00

part4b   bit   part4b
         bit   zzz
         jmp   :LF000

         org   $f000
:LF000   nop
         bne   $f003
         org   $0000
         dfb   $ea
         dfb   $00
