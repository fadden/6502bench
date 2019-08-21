;Project file was edited to get zero-length strings and reverse DCI.
        .cpu    "6502"
        .enc    sg_hiascii
        .cdef   $20,$7e,$a0
        .enc    sg_ascii
        .cdef   $20,$7e,$20
*       =       $1000
        rts

        .text   "low ASCII str"
        .byte   $80
        .enc    sg_hiascii
        .text   "high ASCII str"
        .byte   $80
        .enc    sg_ascii
        .text   "'low'quoted",$22,"''text"
        .byte   $80
        .enc    sg_hiascii
        .text   $a2,"high",$a2,"quoted'",$a2,$a2,"text"
        .byte   $80
        .enc    sg_ascii
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
        .enc    sg_hiascii
        .text   "**************************************************************"
        .byte   $80
        .text   "**************************************************************"
        .text   "**********************************"
        .byte   $81
        .enc    sg_ascii
        .text   "ver IICSA wol"
        .byte   $80
        .enc    sg_hiascii
        .text   "ver IICSA hgih"
        .byte   $80
        .enc    sg_ascii
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
        .enc    sg_hiascii
        .null   "high ASCII strz"
        .byte   $80
        .enc    sg_ascii
        .null   "'low'quoted",$22,"''text"
        .byte   $80
        .enc    sg_hiascii
        .null   $a2,"high",$a2,"quoted'",$a2,$a2,"text"
        .byte   $80
        .enc    sg_ascii
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
        .enc    sg_hiascii
        .ptext  "high ASCII str1"
        .byte   $80
        .enc    sg_ascii
        .ptext  "'low'quoted",$22,"''text"
        .byte   $80
        .enc    sg_hiascii
        .ptext  $a2,"high",$a2,"quoted'",$a2,$a2,"text"
        .byte   $80
        .enc    sg_ascii
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
        .enc    sg_hiascii
        .text   $0f,$00,"high ASCII str2"
        .byte   $80
        .enc    sg_ascii
        .text   $12,$00,"'low'quoted",$22,"''text"
        .byte   $80
        .enc    sg_hiascii
        .text   $13,$00,$a2,"high",$a2,"quoted'",$a2,$a2,"text"
        .byte   $80
        .enc    sg_ascii
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
        .shift  "low ASCII dci"
        .byte   $80
        .enc    sg_hiascii
        .byte   $e8,$e9,$e7,$e8,$a0,$c1,$d3,$c3,$c9,$c9,$a0,$e4,$e3,$69
        .byte   $80
        .enc    sg_ascii
        .shift  "'low'quoted",$22,"''text"
        .byte   $80
        .enc    sg_hiascii
        .byte   $a2,$e8,$e9,$e7,$e8,$a2,$f1,$f5,$ef,$f4,$e5,$e4,$a7,$a2,$a2,$f4
        .byte   $e5,$f8,$74
        .byte   $80
        .enc    sg_ascii
        .text   "012345678901234567890123456789012345678901234567890123456789''"
        .text   $a7
        .byte   $80
        .text   "01234567890123456789012345678901234567890123456789012345678901"
        .text   "23456789012345678",$b9
        .byte   $81
        .byte   $f2,$69,$63,$64,$20,$49,$49,$43,$53,$41,$20,$77,$6f,$6c
        .byte   $80
        .byte   $72,$e9,$e3,$e4,$a0,$c9,$c9,$c3,$d3,$c1,$a0,$e8,$e7,$e9,$e8
        .byte   $80
        .byte   $b9,$38,$37,$36,$35,$34,$33,$32,$31,$30,$39,$38,$37,$36,$35,$34
        .byte   $33,$32,$31,$30,$39,$38,$37,$36,$35,$34,$33,$32,$31,$30,$39,$38
        .byte   $37,$36,$35,$34,$33,$32,$31,$30,$39,$38,$37,$36,$35,$34,$33,$32
        .byte   $31,$30,$39,$38,$37,$36,$35,$34,$33,$32,$31,$30,$39,$38,$37,$36
        .byte   $35,$34,$33,$32,$31,$30,$39,$38,$37,$36,$35,$34,$33,$32,$31,$30
        .byte   $80
        .byte   $ae,$65,$65,$68,$74,$20,$72,$6f,$66,$20,$73,$6c,$6c,$6f,$74,$20
        .byte   $74,$69,$20,$3b,$73,$6c,$6c,$6f,$74,$20,$6c,$6c,$65,$62,$20,$65
        .byte   $68,$74,$20,$6d,$6f,$68,$77,$20,$72,$6f,$66,$20,$77,$6f,$6e,$6b
        .byte   $20,$6f,$74,$20,$64,$6e,$65,$73,$20,$72,$65,$76,$65,$6e,$20,$65
        .byte   $72,$6f,$66,$65,$72,$65,$68,$74,$20,$64,$6e,$61,$20,$2c,$64,$6e
        .byte   $69,$6b,$6e,$61,$6d,$20,$6e,$69,$20,$64,$65,$76,$6c,$6f,$76,$6e
        .byte   $69,$20,$6d,$61,$20,$49,$20,$65,$73,$75,$61,$63,$65,$62,$20,$2c
        .byte   $65,$6d,$20,$73,$65,$68,$73,$69,$6e,$69,$6d,$69,$64,$20,$68,$74
        .byte   $61,$65,$64,$20,$73,$6e,$61,$6d,$20,$79,$6e,$41,$20,$20,$2e,$65
        .byte   $72,$65,$77,$20,$6e,$77,$6f,$20,$65,$6e,$69,$68,$74,$20,$66,$6f
        .byte   $20,$72,$6f,$20,$73,$64,$6e,$65,$69,$72,$66,$20,$79,$68,$74,$20
        .byte   $66,$6f,$20,$72,$6f,$6e,$61,$6d,$20,$61,$20,$66,$69,$20,$73,$61
        .byte   $20,$6c,$6c,$65,$77,$20,$73,$41,$20,$2e,$65,$72,$65,$77,$20,$79
        .byte   $72,$6f,$74,$6e,$6f,$6d,$6f,$72,$70,$20,$61,$20,$66,$69,$20,$73
        .byte   $61,$20,$6c,$6c,$65,$77,$20,$73,$41,$20,$2e,$73,$73,$65,$6c,$20
        .byte   $65,$68,$74,$20,$73,$69,$20,$65,$70,$6f,$72,$75,$45,$20,$2c,$61
        .byte   $65,$73,$20,$65,$68,$74,$20,$79,$62,$20,$79,$61,$77,$61,$20,$64
        .byte   $65,$68,$73,$61,$77,$20,$65,$62,$20,$64,$6f,$6c,$63,$20,$61,$20
        .byte   $66,$49,$20,$20,$2e,$6e,$69,$61,$6d,$20,$65,$68,$74,$20,$66,$6f
        .byte   $20,$74,$72,$61,$70,$20,$61,$20,$2c,$74,$6e,$65,$6e,$69,$74,$6e
        .byte   $6f,$63,$20,$65,$68,$74,$20,$66,$6f,$20,$65,$63,$65,$69,$70,$20
        .byte   $61,$20,$73,$69,$20,$6e,$61,$6d,$20,$79,$72,$65,$76,$65,$20,$3b
        .byte   $66,$6c,$65,$73,$74,$69,$20,$66,$6f,$20,$65,$72,$69,$74,$6e,$65
        .byte   $20,$2c,$64,$6e,$61,$6c,$73,$69,$20,$6e,$61,$20,$73,$69,$20,$6e
        .byte   $61,$6d,$20,$6f,$4e
        .byte   $81
