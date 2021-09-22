        !cpu    6502
*       =       $0000
        !pseudopc $8000 {
        !byte   $ea
        !fill   8191,$00
        }
        !pseudopc $8000 {
        !fill   8192,$01
        }
        !pseudopc $8000 {
        !fill   8192,$02
        }
        !pseudopc $8000 {
        !fill   8192,$03
        }
        !pseudopc $8000 {
        !fill   8192,$04
        }
        !pseudopc $8000 {
        !fill   8192,$05
        }
        !pseudopc $8000 {
        !fill   8192,$06
        }
        !pseudopc $8000 {
        !fill   8192,$07
        }
