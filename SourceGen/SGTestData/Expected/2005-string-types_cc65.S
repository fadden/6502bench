;Project file was edited to get zero-length strings and reverse DCI.
         .setcpu "6502"
         .org    $1000
         rts

         .byte   "low ASCII str"
         .byte   $80
.macro   HiAscii Arg
         .repeat .strlen(Arg), I
         .byte   .strat(Arg, I) | $80
         .endrep
.endmacro
         HiAscii "high ASCII str"
         .byte   $80
         .byte   "'low'quoted",$22,"''text"
         .byte   $80
         .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
         .byte   $e5,$f8,$f4
         .byte   $80
         .byte   "01234567890123456789012345678901234567890123456789012345678901"
         .byte   "234567890123456789"
         .byte   $80
         .byte   "0123456789012345678901234567890123456789012345678901234567'''"
         .byte   $80
         .byte   "01234567890123456789012345678901234567890123456789012345678'''"
         .byte   $80
         .byte   "012345678901234567890123456789012345678901234567890123456789''"
         .byte   "'"
         .byte   $80
         .byte   "0123456789012345678901234567890123456789012345678901234567890'"
         .byte   "''"
         .byte   $80
         .byte   "01234567890123456789012345678901234567890123456789012345678901"
         .byte   "'''"
         .byte   $80
         .byte   "012345678901234567890123456789012345678901234567890167",$22,$22
         .byte   $22
         .byte   $80
         .byte   "0123456789012345678901234567890123456789012345678901678",$22
         .byte   $22,$22
         .byte   $80
         .byte   "01234567890123456789012345678901234567890123456789016789",$22
         .byte   $22,$22
         .byte   $80
         .byte   "012345678901234567890123456789012345678901234567890167890",$22
         .byte   $22,$22
         .byte   $80
         .byte   "0123456789012345678901234567890123456789012345678901678901",$22
         .byte   $22,$22
         .byte   $81
         HiAscii "**************************************************************"
         .byte   $80
         .res    96,$aa
         .byte   $81
         .byte   "ver IICSA wol"
         .byte   $80
         .byte   $f6,$e5,$f2,$a0,$c9,$c9,$c3,$d3,$c1,$a0,$e8,$e7,$e9,$e8
         .byte   $80
         .byte   ".eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes reven"
         .byte   " erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsinimi"
         .byte   "d htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo ronam a"
         .byte   " fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht si e"
         .byte   "poruE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo trap "
         .byte   "a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne ,dn"
         .byte   "alsi na si nam oN"
         .byte   $81
         .asciiz ""
         .byte   $80
         .asciiz "low ASCII strz"
         .byte   $80
         .byte   $e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2,$fa,$00
         .byte   $80
         .byte   "'low'quoted",$22,"''text",$00
         .byte   $80
         .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
         .byte   $e5,$f8,$f4,$00
         .byte   $80
         .byte   "012345678901234567890123456789012345678901234567890123456789''"
         .byte   "'",$00
         .byte   $80
         .byte   "01234567890123456789012345678901234567890123456789012345678901"
         .byte   "234567890123456789",$00
         .byte   $81
         .byte   $00
         .byte   $80
         .byte   $0e,"low ASCII str1"
         .byte   $80
         .byte   $0f,$e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2,$b1
         .byte   $80
         .byte   $12,"'low'quoted",$22,"''text"
         .byte   $80
         .byte   $13,$a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2
         .byte   $f4,$e5,$f8,$f4
         .byte   $80
         .byte   $3f,"0123456789012345678901234567890123456789012345678901234567"
         .byte   "89'''"
         .byte   $80
         .byte   $50,"0123456789012345678901234567890123456789012345678901234567"
         .byte   "8901234567890123456789"
         .byte   $81
         .byte   $00,$00
         .byte   $80
         .byte   $0e,$00,"low ASCII str2"
         .byte   $80
         .byte   $0f,$00,$e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$f3,$f4,$f2
         .byte   $b2
         .byte   $80
         .byte   $12,$00,"'low'quoted",$22,"''text"
         .byte   $80
         .byte   $13,$00,$a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2
         .byte   $a2,$f4,$e5,$f8,$f4
         .byte   $80
         .byte   $3f,$00,"012345678901234567890123456789012345678901234567890123"
         .byte   "456789'''"
         .byte   $80
         .byte   $50,$00,"012345678901234567890123456789012345678901234567890123"
         .byte   "45678901234567890123456789"
         .byte   $80
         .byte   $85,$01,"No man is an island, entire of itself; every man is a "
         .byte   "piece of the continent, a part of the main.  If a clod be wash"
         .byte   "ed away by the sea, Europe is the less. As well as if a promon"
         .byte   "tory were. As well as if a manor of thy friends or of thine ow"
         .byte   "n were.  Any mans death diminishes me, because I am involved i"
         .byte   "n mankind, and therefore never send to know for whom the bell "
         .byte   "tolls; it tolls for thee."
         .byte   $81
         .byte   "low ASCII dc",$e9
         .byte   $80
         .byte   $e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$e4,$e3,$69
         .byte   $80
         .byte   "'low'quoted",$22,"''tex",$f4
         .byte   $80
         .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
         .byte   $e5,$f8,$74
         .byte   $80
         .byte   "012345678901234567890123456789012345678901234567890123456789''"
         .byte   $a7
         .byte   $80
         .byte   "01234567890123456789012345678901234567890123456789012345678901"
         .byte   "23456789012345678",$b9
         .byte   $81
         .byte   $f2,"icd IICSA wol"
         .byte   $80
         .byte   $72,$e9,$e3,$e4,$a0,$c9,$c9,$c3,$d3,$c1,$a0,$e8,$e7,$e9,$e8
         .byte   $80
         .byte   $b9,"8765432109876543210987654321098765432109876543210987654321"
         .byte   "098765432109876543210"
         .byte   $80
         .byte   $ae,"eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes re"
         .byte   "ven erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsin"
         .byte   "imid htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo rona"
         .byte   "m a fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht s"
         .byte   "i eporuE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo tr"
         .byte   "ap a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne "
         .byte   ",dnalsi na si nam oN"
         .byte   $81
