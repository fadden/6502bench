;Project file was edited to get zero-length strings and reverse DCI.
        !cpu    6502
*       =       $1000
        rts

        !text   "low ASCII str"
        !byte   $80
        !hex    e8e9e7e8a0c1d3c3c9c9a0f3f4f2
        !byte   $80
        !text   "'low'quoted",$22,"''text"
        !byte   $80
        !hex    a2e8e9e7e8a2f1f5eff4e5e4a7a2a2f4e5f8f4
        !byte   $80
        !text   "01234567890123456789012345678901234567890123456789012345678901"
        !text   "234567890123456789"
        !byte   $80
        !text   "0123456789012345678901234567890123456789012345678901234567'''"
        !byte   $80
        !text   "01234567890123456789012345678901234567890123456789012345678'''"
        !byte   $80
        !text   "012345678901234567890123456789012345678901234567890123456789''"
        !text   "'"
        !byte   $80
        !text   "0123456789012345678901234567890123456789012345678901234567890'"
        !text   "''"
        !byte   $80
        !text   "01234567890123456789012345678901234567890123456789012345678901"
        !text   "'''"
        !byte   $80
        !text   "012345678901234567890123456789012345678901234567890167",$22,$22
        !text   $22
        !byte   $80
        !text   "0123456789012345678901234567890123456789012345678901678",$22
        !text   $22,$22
        !byte   $80
        !text   "01234567890123456789012345678901234567890123456789016789",$22
        !text   $22,$22
        !byte   $80
        !text   "012345678901234567890123456789012345678901234567890167890",$22
        !text   $22,$22
        !byte   $80
        !text   "0123456789012345678901234567890123456789012345678901678901",$22
        !text   $22,$22
        !byte   $81
        !fill   62,$aa
        !byte   $80
        !fill   96,$aa
        !byte   $81
        !text   "ver IICSA wol"
        !byte   $80
        !hex    f6e5f2a0c9c9c3d3c1a0e8e7e9e8
        !byte   $80
        !text   ".eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes reven"
        !text   " erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsinimi"
        !text   "d htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo ronam a"
        !text   " fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht si e"
        !text   "poruE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo trap "
        !text   "a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne ,dn"
        !text   "alsi na si nam oN"
        !byte   $81
        !text   $00
        !byte   $80
        !text   "low ASCII strz",$00
        !byte   $80
        !hex    e8e9e7e8a0c1d3c3c9c9a0f3f4f2fa00
        !byte   $80
        !text   "'low'quoted",$22,"''text",$00
        !byte   $80
        !hex    a2e8e9e7e8a2f1f5eff4e5e4a7a2a2f4e5f8f400
        !byte   $80
        !text   "012345678901234567890123456789012345678901234567890123456789''"
        !text   "'",$00
        !byte   $80
        !text   "01234567890123456789012345678901234567890123456789012345678901"
        !text   "234567890123456789",$00
        !byte   $81
        !text   $00
        !byte   $80
        !text   $0e,"low ASCII str1"
        !byte   $80
        !hex    0fe8e9e7e8a0c1d3c3c9c9a0f3f4f2b1
        !byte   $80
        !text   $12,"'low'quoted",$22,"''text"
        !byte   $80
        !hex    13a2e8e9e7e8a2f1f5eff4e5e4a7a2a2f4e5f8f4
        !byte   $80
        !text   $3f,"0123456789012345678901234567890123456789012345678901234567"
        !text   "89'''"
        !byte   $80
        !text   $50,"0123456789012345678901234567890123456789012345678901234567"
        !text   "8901234567890123456789"
        !byte   $81
        !text   $00,$00
        !byte   $80
        !text   $0e,$00,"low ASCII str2"
        !byte   $80
        !hex    0f00e8e9e7e8a0c1d3c3c9c9a0f3f4f2b2
        !byte   $80
        !text   $12,$00,"'low'quoted",$22,"''text"
        !byte   $80
        !hex    1300a2e8e9e7e8a2f1f5eff4e5e4a7a2a2f4e5f8f4
        !byte   $80
        !text   $3f,$00,"012345678901234567890123456789012345678901234567890123"
        !text   "456789'''"
        !byte   $80
        !text   $50,$00,"012345678901234567890123456789012345678901234567890123"
        !text   "45678901234567890123456789"
        !byte   $80
        !text   $85,$01,"No man is an island, entire of itself; every man is a "
        !text   "piece of the continent, a part of the main.  If a clod be wash"
        !text   "ed away by the sea, Europe is the less. As well as if a promon"
        !text   "tory were. As well as if a manor of thy friends or of thine ow"
        !text   "n were.  Any mans death diminishes me, because I am involved i"
        !text   "n mankind, and therefore never send to know for whom the bell "
        !text   "tolls; it tolls for thee."
        !byte   $81
        !text   "low ASCII dc",$e9
        !byte   $80
        !hex    e8e9e7e8a0c1d3c3c9c9a0e4e369
        !byte   $80
        !text   "'low'quoted",$22,"''tex",$f4
        !byte   $80
        !hex    a2e8e9e7e8a2f1f5eff4e5e4a7a2a2f4e5f874
        !byte   $80
        !text   "012345678901234567890123456789012345678901234567890123456789''"
        !text   $a7
        !byte   $80
        !text   "01234567890123456789012345678901234567890123456789012345678901"
        !text   "23456789012345678",$b9
        !byte   $81
        !text   $f2,"icd IICSA wol"
        !byte   $80
        !hex    72e9e3e4a0c9c9c3d3c1a0e8e7e9e8
        !byte   $80
        !text   $b9,"8765432109876543210987654321098765432109876543210987654321"
        !text   "098765432109876543210"
        !byte   $80
        !text   $ae,"eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes re"
        !text   "ven erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsin"
        !text   "imid htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo rona"
        !text   "m a fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht s"
        !text   "i eporuE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo tr"
        !text   "ap a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne "
        !text   ",dnalsi na si nam oN"
        !byte   $81
