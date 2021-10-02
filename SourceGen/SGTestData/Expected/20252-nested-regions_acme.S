;ACME can't handle 65816 code that lives outside bank zero
*       =       $0000
        !pseudopc $031000 {
        !hex    af001003af0410032f1f10015c101002af1010025c181001af1810014c261080
        !hex    ea6004100300ea60
        }
