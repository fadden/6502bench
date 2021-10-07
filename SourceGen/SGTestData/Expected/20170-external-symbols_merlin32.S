_FatConst equ  $4000

OverVar  equ   $40
CodeWrap equ   $0f00      ;encases program
SameName1 equ  $2000
SameName2 equ  $2011
SameName3 equ  $2022
SameValA_A equ $2100
SameValB_A equ $2110
SameValC_C equ $2120
ChkProj1 equ   $2200
ProjSym1 equ   $2202
ChkProj2 equ   $2204
ProjSym2 equ   $220a
Over1    equ   $3000      ;$3000-300f, inclusive
Over2    equ   $3002      ;$3002-3009
Over3    equ   $3006      ;$3006-300c
Over2a   equ   $3006      ;$3006
SepOver1 equ   $3100      ;$3100-3103, inclusive
SepOver2 equ   $3102      ;$3102-3105, inclusive
ReadOnly equ   $5000      ;R
WriteOnly equ  $5001      ;W
WriteOnly2 equ $5002
OverA_0  equ   $6000      ;should win, alphabetically
OverA_1  equ   $6100      ;should win, alphabetically
OverB_2  equ   $6200      ;should win because narrower
OverB_3  equ   $6300      ;should win because narrower
OverA_4  equ   $6401      ;(ref OverA_4+2)
OverB_4  equ   $6402      ;should win because closer
OverC_4  equ   $6403
MultiRead equ  $c004      ;$c004/5/6, read-only
MultiWrite equ $c005      ;$c005/6/7, write-only
AlsoMultiZero equ $c010   ;wins (alphabetically)
NonMultiOver equ $c010    ;winner
MultiOne equ   $c021
MoreMultiZero equ $c100
AlsoMoreMultiZero equ $c110 ;winner
BankWrap equ   $fff0

         org   $1000
L1000    lda   CodeWrap+255
         ldx   L1000
         ldy   L1000+1
         lda   :L1160
         lda   CodeWrap+$261
         lda   CodeWrap+$268
         nop
         lda   $1ffe
         lda   SameName1-1
         lda   SameName1
         lda   $200f
         lda   SameName2-1
         lda   SameName2
         lda   $2012
         lda   $201f
         lda   $2020
         lda   SameName3-1
         lda   SameName3
         nop
         lda   SameValA_A
         lda   SameValB_A
         lda   SameValC_C
         nop
         lda   $21fe
         lda   ChkProj1-1
         lda   ChkProj1
         lda   ChkProj1+1
         lda   ProjSym1
         lda   ProjSym1+1
         lda   ProjSym1+2
         lda   ProjSym1+3
         lda   ChkProj2+2
         lda   ChkProj2+3
         lda   $2208
         lda   ProjSym2-1
         lda   ProjSym2
         lda   $220b
         nop
         lda   $2ffe
         lda   Over1-1
         lda   Over1
         lda   Over1+1
         lda   Over2
         lda   Over2+1
         lda   Over2+2
         lda   Over2+3
         lda   Over2a
         lda   Over3+1
         lda   Over3+2
         lda   Over3+3
         lda   Over3+4
         lda   Over3+5
         lda   Over3+6
         lda   Over1+13
         lda   Over1+14
         lda   Over1+15
         lda   $3010
         nop
         lda   $30fe
         lda   SepOver1-1
         lda   SepOver1
         lda   SepOver1+1
         lda   SepOver1+2
         lda   SepOver1+3
         lda   SepOver2+2
         lda   SepOver2+3
         lda   $3106
         nop
]LocalVar equ  $41
         ldx   $3e
         ldx   $3f
         ldx   OverVar
         ldx   ]LocalVar
         ldx   ]LocalVar+1
         ldx   OverVar+3
         ldx   $44
         nop
         lda   _FatConst-1
         lda   $4000
         lda   $4001
         lda   BankWrap+8
         lda   <BankWrap-$ffe8
         nop
         lda   ReadOnly
         lda   ReadOnly+1
         ldx   $5002
         ldy   $5003
         sta   WriteOnly-1
         sta   WriteOnly
         stx   WriteOnly2
         sty   $5003
         nop
         bit   AlsoMultiZero-16
         bit   NonMultiOver
         bit   AlsoMultiZero+16
         bit   AlsoMultiZero+224
         nop
         bit   MultiOne-32
         bit   MultiOne-16
         bit   MultiOne
         nop
         bit   $c002
         bit   $c012
         bit   $c022
         nop
         lda   MultiRead-1
         lda   MultiRead
         lda   MultiRead+1
         lda   MultiRead+2
         lda   $c007
         sta   MultiWrite-1
         sta   MultiWrite
         sta   MultiWrite+1
         sta   MultiWrite+2
         sta   $c008
         nop
         jsr   MultiRead+1
         nop
         bit   MoreMultiZero
         bit   AlsoMoreMultiZero
         bit   MoreMultiZero+32
         bit   MoreMultiZero+240
         nop
         lda   OverA_0
         lda   OverA_1
         lda   OverB_2
         lda   OverB_3+4
         lda   OverA_4-1
         lda   OverA_4
         lda   OverB_4
         lda   OverC_4
:L1160   rts

