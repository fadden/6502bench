shortnm  equ   $4000      ;short label
SomewhatLongName equ $4001 ;somewhat longer label

         org   $1000
         lda   shortnm
         ldx   SomewhatLongName
         clc
         bcc   BranchTargetLongName

data
         dfb   $cc

]ptr     equ   $00        ;local var with short name
]PointerWithLongName equ $02 ;local var with longer name
BranchTargetLongName
         sta   ]ptr
         stx   ]PointerWithLongName
         ldy   data
         lsr   A
         bcc   shortb
shortb
         nop
         jmp   done

done
         rts

