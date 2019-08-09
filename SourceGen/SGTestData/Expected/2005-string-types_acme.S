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
        !hex    f269636420494943534120776f6c
        !byte   $80
        !hex    72e9e3e4a0c9c9c3d3c1a0e8e7e9e8
        !byte   $80
        !hex    b938373635343332313039383736353433323130393837363534333231303938
        !hex    3736353433323130393837363534333231303938373635343332313039383736
        !hex    35343332313039383736353433323130
        !byte   $80
        !hex    ae6565687420726f6620736c6c6f74207469203b736c6c6f74206c6c65622065
        !hex    6874206d6f687720726f6620776f6e6b206f7420646e657320726576656e2065
        !hex    726f66657265687420646e61202c646e696b6e616d206e69206465766c6f766e
        !hex    69206d6120492065737561636562202c656d2073656873696e696d6964206874
        !hex    61656420736e616d20796e4120202e65726577206e776f20656e69687420666f
        !hex    20726f2073646e656972662079687420666f20726f6e616d2061206669207361
        !hex    206c6c6577207341202e657265772079726f746e6f6d6f727020612066692073
        !hex    61206c6c6577207341202e7373656c206568742073692065706f727545202c61
        !hex    65732065687420796220796177612064656873617720656220646f6c63206120
        !hex    664920202e6e69616d2065687420666f20747261702061202c746e656e69746e
        !hex    6f632065687420666f2065636569702061207369206e616d207972657665203b
        !hex    666c6573746920666f20657269746e65202c646e616c7369206e61207369206e
        !hex    616d206f4e
        !byte   $81
