        !cpu    6502
*       =       $0000
        !pseudopc $8000 {
        !byte   $ea
        !fill   8191,$00
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$01
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$02
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$03
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$04
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$05
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$06
        } ;!pseudopc
        !pseudopc $8000 {
        !fill   8192,$07
        } ;!pseudopc
