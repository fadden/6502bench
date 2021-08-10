; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        rts

* simple strings
* High-ASCII quoted text uses embedded high-ascii double-quotes, mostly to
* test Merlin's behavior.  Note that a mix of high- and low-ASCII won't be
* handled as a single string.
        asc     'low ASCII str'
        dfb     $80
        asc     "high ASCII str"
        dfb     $80
        asc     27,'low',27,'quoted',222727,'text'
        dfb     $80
        asc     A2,"high",A2,"quoted",A7A2A2,"text"
        dfb     $80
        asc     '01234567890123456789012345678901234567890123456789012345678901234567890123456789'  ; 80 chars
        dfb     $80

* edge cases for 64-char operand, single-quote delimiter (primarily for Merlin 32)
        asc     '0123456789012345678901234567890123456789012345678901234567',272727
        dfb     $80
        asc     '01234567890123456789012345678901234567890123456789012345678',272727
        dfb     $80
        asc     '012345678901234567890123456789012345678901234567890123456789',272727
        dfb     $80
        asc     '0123456789012345678901234567890123456789012345678901234567890',272727
        dfb     $80
        asc     '01234567890123456789012345678901234567890123456789012345678901',272727
        dfb     $80

* edge cases for 64-char operand, double-quote delimiter (primarily for cc65)
        asc     '012345678901234567890123456789012345678901234567890167',222222
        dfb     $80
        asc     '0123456789012345678901234567890123456789012345678901678',222222
        dfb     $80
        asc     '01234567890123456789012345678901234567890123456789016789',222222
        dfb     $80
        asc     '012345678901234567890123456789012345678901234567890167890',222222
        dfb     $80
        asc     '0123456789012345678901234567890123456789012345678901678901',222222

        dfb     $81

* 62 high-ASCII asterisks.  Should be one line.
        asc     "********************************"
        asc     "******************************"

        dfb     $80

* 96 high-ASCII asterisks.  Might be converted to "fill".
        asc     "********************************"
        asc     "********************************"
        asc     "********************************"

        dfb     $81

* reverse strings; cannot intersperse hex
        rev     'low ASCII rev'
        dfb     $80
        rev     "high ASCII rev"
        dfb     $80
        rev     'No man is an island, entire of itself; every man is a piece of the continent, a part of the main.  If a clod be washed away by the sea, Europe is the less. As well as if a promontory were. As well as if a manor of thy friends or of thine own were.  Any mans death diminishes me, because I am involved in mankind, and therefore never send to know for whom the bell tolls; it tolls for thee.'       ; 389 chars

        dfb     $81

* null-terminated strings
        dfb     $00                 ;empty string, requires project edit
        dfb     $80
        asc     'low ASCII strz',00
        dfb     $80
        asc     "high ASCII strz",00
        dfb     $80
        asc     27,'low',27,'quoted',222727,'text',00
        dfb     $80
        asc     A2,"high",A2,"quoted",A7A2A2,"text",00
        dfb     $80
        asc     '012345678901234567890123456789012345678901234567890123456789',27272700
        dfb     $80
        asc     '01234567890123456789012345678901234567890123456789012345678901234567890123456789',00

        dfb     $81

* string with leading length byte
        str     ''                  ;requires project edit
        dfb     $80
        str     'low ASCII str1'
        dfb     $80
        str     "high ASCII str1"
        dfb     $80
        str     27,'low',27,'quoted',222727,'text'
        dfb     $80
        str     A2,"high",A2,"quoted",A7A2A2,"text"
        dfb     $80
        str     '012345678901234567890123456789012345678901234567890123456789',272727
        dfb     $80
        str     '01234567890123456789012345678901234567890123456789012345678901234567890123456789'

        dfb     $81

* string with leading length word
        strl    ''                  ;requires project edit
        dfb     $80
        strl    'low ASCII str2'
        dfb     $80
        strl    "high ASCII str2"
        dfb     $80
        strl    27,'low',27,'quoted',222727,'text'
        dfb     $80
        strl    A2,"high",A2,"quoted",A7A2A2,"text"
        dfb     $80
        strl    '012345678901234567890123456789012345678901234567890123456789',272727
        dfb     $80
        strl    '01234567890123456789012345678901234567890123456789012345678901234567890123456789'
        dfb     $80
        strl    'No man is an island, entire of itself; every man is a piece of the continent, a part of the main.  If a clod be washed away by the sea, Europe is the less. As well as if a promontory were. As well as if a manor of thy friends or of thine own were.  Any mans death diminishes me, because I am involved in mankind, and therefore never send to know for whom the bell tolls; it tolls for thee.'       ; 389 chars

        dfb     $81

* DCI (dextral character inverted)
        dci     'low ASCII dci'
        dfb     $80
        dci     "high ASCII dci"
        dfb     $80
        dci     27,'low',27,'quoted',222727,'text'
        dfb     $80
        dci     A2,"high",A2,"quoted",A7A2A2,"text"
        dfb     $80
        dci     '012345678901234567890123456789012345678901234567890123456789',272727
        dfb     $80
        dci     '01234567890123456789012345678901234567890123456789012345678901234567890123456789'

        dfb     $81

* reverse DCI (deprecated, requires project edit)
        asc     F2,'icd IICSA wol'
        dfb     $80
        asc     72,"icd IICSA hgih"
        dfb     $80
        asc     B9,'8765432109876543210987654321098765432109876543210987654321098765432109876543210'  ; 80 chars
        dfb     $80
        asc     AE,'eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes reven erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsinimid htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo ronam a fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht si eporuE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo trap a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne ,dnalsi na si nam oN'

        dfb     $81
