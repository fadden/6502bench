         .setcpu "6502"
shortnm  =       $4000      ;short label
SomewhatLongName = $4001    ;somewhat longer label

         .org    $1000
         lda     shortnm
         ldx     SomewhatLongName
         clc
         bcc     BranchTargetLongName

data:
         .byte   $cc

ptr      .set    $00        ;local var with short name
PointerWithLongName .set $02 ;local var with longer name
BranchTargetLongName:
         sta     ptr
         stx     PointerWithLongName
         ldy     data
         lsr     A
         bcc     shortb
shortb:
         nop
         jmp     done

done:
         rts

