;Project file was edited to get zero-length strings and reverse DCI.
        .cpu    "6502"
*       =       $1000
        rts

        .text   "low ASCII str"
        .byte   $80
        .byte   $e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2
        .byte   $80
        .text   "'low'quoted",$22,"''text"
        .byte   $80
        .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
        .byte   $e5,$f8,$f4
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789012345678901"
        .text   "234567890123456789"
        .byte   $80
        .text   "0123456789012345678901234567890123456789012345678901234567'''"
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789012345678'''"
        .byte   $80
        .text   "012345678901234567890123456789012345678901234567890123456789''"
        .text   "'"
        .byte   $80
        .text   "0123456789012345678901234567890123456789012345678901234567890'"
        .text   "''"
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789012345678901"
        .text   "'''"
        .byte   $80
        .text   "012345678901234567890123456789012345678901234567890167",$22,$22
        .text   $22
        .byte   $80
        .text   "0123456789012345678901234567890123456789012345678901678",$22
        .text   $22,$22
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789016789",$22
        .text   $22,$22
        .byte   $80
        .text   "012345678901234567890123456789012345678901234567890167890",$22
        .text   $22,$22
        .byte   $80
        .text   "0123456789012345678901234567890123456789012345678901678901",$22
        .text   $22,$22
        .byte   $81
        .fill   62,$aa
        .byte   $80
        .fill   96,$aa
        .byte   $81
        .text   "ver IICSA wol"
        .byte   $80
        .byte   $f6,$e5,$f2,$a0,$c9,$c9,$c3,$d3,$c1,$a0,$e8,$e7,$e9,$e8
        .byte   $80
        .text   ".eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes reven"
        .text   " erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsinimi"
        .text   "d htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo ronam a"
        .text   " fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht si e"
        .text   "poruE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo trap "
        .text   "a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne ,dn"
        .text   "alsi na si nam oN"
        .byte   $81
        .null   ""
        .byte   $80
        .null   "low ASCII strz"
        .byte   $80
        .byte   $e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2,$fa,$00
        .byte   $80
        .text   "'low'quoted",$22,"''text",$00
        .byte   $80
        .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
        .byte   $e5,$f8,$f4,$00
        .byte   $80
        .text   "012345678901234567890123456789012345678901234567890123456789''"
        .text   "'",$00
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789012345678901"
        .text   "234567890123456789",$00
        .byte   $81
        .ptext  ""
        .byte   $80
        .ptext  "low ASCII str1"
        .byte   $80
        .byte   $0f,$e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2,$b1
        .byte   $80
        .text   $12,"'low'quoted",$22,"''text"
        .byte   $80
        .byte   $13,$a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2
        .byte   $f4,$e5,$f8,$f4
        .byte   $80
        .text   $3f,"0123456789012345678901234567890123456789012345678901234567"
        .text   "89'''"
        .byte   $80
        .text   $50,"0123456789012345678901234567890123456789012345678901234567"
        .text   "8901234567890123456789"
        .byte   $81
        .text   $00,$00
        .byte   $80
        .text   $0e,$00,"low ASCII str2"
        .byte   $80
        .byte   $0f,$00,$e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2
        .byte   $b2
        .byte   $80
        .text   $12,$00,"'low'quoted",$22,"''text"
        .byte   $80
        .byte   $13,$00,$a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2
        .byte   $a2,$f4,$e5,$f8,$f4
        .byte   $80
        .text   $3f,$00,"012345678901234567890123456789012345678901234567890123"
        .text   "456789'''"
        .byte   $80
        .text   $50,$00,"012345678901234567890123456789012345678901234567890123"
        .text   "45678901234567890123456789"
        .byte   $80
        .text   $85,$01,"No man is an island, entire of itself; every man is a "
        .text   "piece of the continent, a part of the main.  If a clod be wash"
        .text   "ed away by the sea, Europe is the less. As well as if a promon"
        .text   "tory were. As well as if a manor of thy friends or of thine ow"
        .text   "n were.  Any mans death diminishes me, because I am involved i"
        .text   "n mankind, and therefore never send to know for whom the bell "
        .text   "tolls; it tolls for thee."
        .byte   $81
        .text   "low ASCII dc",$e9
        .byte   $80
        .byte   $e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$e4,$e3,$69
        .byte   $80
        .text   "'low'quoted",$22,"''tex",$f4
        .byte   $80
        .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
        .byte   $e5,$f8,$74
        .byte   $80
        .text   "012345678901234567890123456789012345678901234567890123456789''"
        .text   $a7
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789012345678901"
        .text   "23456789012345678",$b9
        .byte   $81
        .text   $f2,"icd IICSA wol"
        .byte   $80
        .byte   $72,$e9,$e3,$e4,$a0,$c9,$c9,$c3,$d3,$c1,$a0,$e8,$e7,$e9,$e8
        .byte   $80
        .text   $b9,"8765432109876543210987654321098765432109876543210987654321"
        .text   "098765432109876543210"
        .byte   $80
        .text   $ae,"eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes re"
        .text   "ven erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsin"
        .text   "imid htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo rona"
        .text   "m a fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht s"
        .text   "i eporuE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo tr"
        .text   "ap a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne "
        .text   ",dnalsi na si nam oN"
        .byte   $81
