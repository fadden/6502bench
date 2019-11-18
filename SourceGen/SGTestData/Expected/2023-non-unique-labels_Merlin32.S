         org   $1000
L1000    lda   #$00
:L1000   lda   #$01
         ldx   L1000
         ldy   :L1000
         ldx   #$02
loop1    dex
         bne   loop1
         ldx   #$03
:loop1   dex
         bne   :loop1
global1  nop
         ldx   #$04
:loop    ldy   #$05
:loop1   dey
         bne   :loop1
         dex
         bne   :loop
         jmp   loop

global2  dfb   $ea

loop     nop
global3  nop
         ldx   #$06
         ldy   #$07
         dex
         beq   :fwd1
         dey
         beq   :fwd2
:fwd1    nop
:fwd2    nop
global4  nop
         ldx   #$08
loop2    dex
global5  nop
         bne   loop2
         nop
global6  nop
:spin1   jsr   :spin2
:spin2   jsr   :spin1
         nop
:spin11  lda   :spin1+7
         beq   :spin11
         lda   #<:spin1
         ldx   #<:spin2
         lda   #>:spin1
         ldx   #>:spin2
         bne   :skip

         dw    :spin1
         dw    :spin2
         dw    :spin11
         dfb   <:spin1
         dfb   <:spin2
         dfb   >:spin1
         dfb   >:spin2

:skip    nop
global_  nop
_global  ldx   #$40
__       dex
         bne   __
         beq   ___

___      ldx   #$41
:__      dex
         bne   :__
         nop
anno     lda   #$42
T106B    lda   anno
         clc
         bcc   :skip

         dw    T106B

:skip    nop
JMP      lda   JMP
JMP0     lda   JMP0
JMP1     lda   JMP1
:JMP     lda   :JMP
:JMP0    lda   :JMP0
:JMP1    lda   :JMP1
:JMP2    lda   :JMP2
jmp      lda   jmp
Jmp      lda   Jmp
BRA      lda   BRA
brl      lda   brl
LDAL     dfb   $af
         dfb   $95
         dfb   $10
         dfb   $00
         nop
]plain_1 equ   $11
]_under1 equ   $12
]__dub1  equ   $13
         lda   ]plain_1
         lda   ]_under1
         lda   ]__dub1
:plain   lda   :plain
plain    lda   plain
global8  dex
         bne   plain
X_under1 lda   X_under1
:X__dub1 lda   :X__dub1
]_under1 equ   $22
         lda   ]plain_1
         lda   ]_under1
         rts

