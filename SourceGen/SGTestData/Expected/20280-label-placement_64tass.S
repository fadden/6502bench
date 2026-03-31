        .cpu    "6502"
shortnm =       $4000      ;short label
SomewhatLongName = $4001   ;somewhat longer label

*       =       $1000
        lda     shortnm
        ldx     SomewhatLongName
L123456789012345678901234567890123456789012345678901234567890123
        clc
        bcc     BranchTargetLongName

data
        .byte   $cc

ptr     .var    $00        ;local var with short name
VariableWithExtremelyLongName01234567890123456789012345678901234 .var $02 ;local var with longer name
BranchTargetLongName
        sta     ptr
        stx     VariableWithExtremelyLongName01234567890123456789012345678901234
        ldy     data
        lsr     a
        bcc     shortb
shortb
        nop
        jmp     done

done
        rts

