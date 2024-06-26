*Edited to have duplicate labels (PROJ_ZERO, DPCODE).
CONST_ZERO equ $f0        ;project const

PROJ_ZERO equ  $00        ;project addr
PROJ_ONE equ   $01        ;project addr

         org   $1000
         ldy   PROJ_ZERO
         lda   (PROJ_ONE),y
         sta   $03        ;could be PROJ_ONE+2, but "nearby" is off
         ldx   $04
         lda   CONST_ZERO,S
         sta   $f1,S
]VAR_ZERO equ  $00
]VAR_TWO equ   $02
]VAR_THREE equ $03
]CONST_ZERO_VAR equ $f0
         ldy   ]VAR_ZERO
         lda   (]VAR_ZERO+1),y
         sta   ]VAR_THREE
         ldx   $04
         lda   ]CONST_ZERO_VAR,S
         sta   $f1,S
         eor   0
         ora   240,S
]PROJ_ZERO_DUP1 equ $10   ;clash with project symbol
]DPCODE_DUP1 equ $80      ;clash with user label
         lda   ]VAR_ZERO
         lda   ]VAR_ZERO+1
         lda   ]VAR_TWO
         lda   ]VAR_THREE
         lda   $04
         lda   ]PROJ_ZERO_DUP1
         lda   $11
         lda   DPCODE
         ldx   PROJ_ZERO
         ldx   PROJ_ONE
         ldx   $02
         bit   $ffa9
         ldy   PROJ_ZERO
         ldy   PROJ_ONE
         ldy   $02
         dfb   $2c
]NH0     equ   $00        ;not hidden
]NH1     equ   $01        ;not hidden
L103C    lda   #$fe
         beq   L103C
         ldy   ]NH0
         ldy   ]NH1
         ldy   $02
         nop
]PTR0    equ   $10
]CONST0  equ   $10
         lda   ]PTR0
         ldx   ]PTR0+1
         ldy   $12
         lda   (]CONST0,S),y
         sta   (]CONST0+3,S),y
*Test name redefinition.  This is mostly of interest for assemblers without
*redefinable variables, but also of interest to the cross-reference window.
]PTR     equ   $20        ;#1
         ldx   ]PTR
]PTR     equ   $22        ;#2
         ldx   ]PTR
]PTR     equ   $24        ;#3
         ldx   ]PTR
:PTR_1   nop
]PTR_A   equ   $20
         ldy   ]PTR_A
]PTR_B   equ   $1f
         ldy   ]PTR_B+1
]PTR_C   equ   $1d
         ldy   ]PTR_C+3
]PTR_D   equ   $21
         ldy   ]PTR_C+3
]VAL0    equ   $30
]VAL1    equ   $31
]VAL2    equ   $32
]VAL3    equ   $33
]VAL4    equ   $34
]VAL5    equ   $35
         and   ]VAL0
         and   ]VAL1
         and   ]VAL2
         and   ]VAL3
         and   ]VAL4
         and   ]VAL5
]VAL14   equ   $31
         and   ]VAL0
         and   ]VAL14
         and   ]VAL14+1
         and   ]VAL14+2
         and   ]VAL14+3
         and   ]VAL5
]DPNOP   equ   $80        ;same as org
         lda   DPCODE
         jsr   DPCODE
         rts

         org   $0080
DPCODE   nop
         lda   DPCODE
         lda:  DPCODE
         ldal  DPCODE
:SPLIT1  lda   #$2c
]SPLITTER equ  $80
         ldx   $1234
         beq   :SPLIT1
:SPLIT2  lda   $2c
         ldx   $5678
         beq   :SPLIT2
         nop
         clc
         xce
         rep   #$30
         mx    %00
         pea   ^STRING
         pea   STRING
         jsl   STRLEN
         sec
         xce
         mx    %11
         jmp   L00E6

STRING   asc   'testing'
         dfb   $00

]LEN     equ   $f2
]PTR     equ   $f4
]ARG     equ   $fd
         mx    %00
STRLEN   phd
         tsc
         sec
         sbc   #$00f7
         tcd
         adc   #$00f0
         tcs
         stz   ]LEN
:L00C1   ldx   ]ARG+2
         lda   ]ARG
         inc   ]ARG
         bne   :L00CB
         inc   ]ARG+2
:L00CB   sta   ]PTR
         stx   ]PTR+2
         lda   []PTR]
         and   #$00ff
         beq   :L00DA
         inc   ]LEN
         bra   :L00C1

:L00DA   lda   ]LEN
         tay
         tdc
         clc
         adc   #$00f7
         tcs
         tya
         pld
         rtl

         mx    %11
L00E6    bit   ]ARG+2
         bit:  $00ff
         bit   $0100
         rts

