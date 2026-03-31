shortnm  equ   $4000      ;short label
SomewhatLongName equ $4001 ;somewhat longer label

         org   $1000
         lda   shortnm
         ldx   SomewhatLongName
L123456789012345678901234567890123456789012345678901234567890123
         clc
         bcc   BranchTargetLongName

data
         dfb   $cc

]ptr     equ   $00        ;local var with short name
]VariableWithExtremelyLongName01234567890123456789012345678901234 equ $02 ;local var with longer name
BranchTargetLongName
         sta   ]ptr
         stx   ]VariableWithExtremelyLongName01234567890123456789012345678901234
         ldy   data
         lsr   A
         bcc   shortb
shortb
         nop
         jmp   done

done
         rts

