OverVar  equ   $40
PlatStart equ  $1000
SameName1 equ  $2000
SameName2 equ  $2011
SameName3 equ  $2022
SameValA_A equ $2100
SameValB_A equ $2110
SameValC_C equ $2120
ChkProj1 equ   $2200
ProjSim1 equ   $2202
ChkProj2 equ   $2204
ProjSim2 equ   $220a
Over1    equ   $3000      ;$3000-300f, inclusive
Over2    equ   $3002      ;$3002-3009
Over2a   equ   $3006      ;$3006
Over3    equ   $3006      ;$3006-300c
SepOver1 equ   $3100      ;$3100-3103, inclusive
SepOver2 equ   $3102      ;$3102-3105, inclusive
FatConst equ   $4000

         org   $1000
L1000    lda   PlatStart-1
         ldx   L1000
         ldy   L1000+1
         nop
         lda   $1ffe
         lda   SameName1-1
         lda   SameName1
         lda   $200f
         lda   SameName2-1
         lda   SameName2
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
         lda   ProjSim1
         lda   ProjSim1+1
         lda   ProjSim1+2
         lda   ProjSim1+3
         lda   ChkProj2+2
         lda   ChkProj2+3
         lda   $2208
         lda   ProjSim2-1
         lda   ProjSim2
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
         ldx   OverVar-1
         ldx   OverVar
         ldx   ]LocalVar
         ldx   ]LocalVar+1
         ldx   OverVar+3
         ldx   $44
         nop
         lda   FatConst-1
         lda   $4000
         lda   $4001
         rts

