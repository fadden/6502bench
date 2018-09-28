;Project file was edited to get zero-length strings and reverse DCI.
         org   $1000
         rts

         asc   'low ASCII str'
         dfb   $80
         asc   "high ASCII str"
         dfb   $80
         asc   27,'low',27,'quoted"',2727,'text'
         dfb   $80
         asc   a2,"high",a2,"quoted'",a2a2,"text"
         dfb   $80
         asc   '01234567890123456789012345678901234567890123456789012345678901'
         asc   '234567890123456789'
         dfb   $80
         asc   '0123456789012345678901234567890123456789012345678901234567',27
         asc   2727
         dfb   $80
         asc   '01234567890123456789012345678901234567890123456789012345678',27
         asc   2727
         dfb   $80
         asc   '012345678901234567890123456789012345678901234567890123456789'
         asc   272727
         dfb   $80
         asc   '0123456789012345678901234567890123456789012345678901234567890'
         asc   272727
         dfb   $80
         asc   '01234567890123456789012345678901234567890123456789012345678901'
         asc   272727
         dfb   $80
         asc   '012345678901234567890123456789012345678901234567890167"""'
         dfb   $80
         asc   '0123456789012345678901234567890123456789012345678901678"""'
         dfb   $80
         asc   '01234567890123456789012345678901234567890123456789016789"""'
         dfb   $80
         asc   '012345678901234567890123456789012345678901234567890167890"""'
         dfb   $80
         asc   '0123456789012345678901234567890123456789012345678901678901"""'
         dfb   $81
         asc   "**************************************************************"
         dfb   $80
         asc   "**************************************************************"
         asc   "**********************************"
         dfb   $81
         rev   'low ASCII rev'
         dfb   $80
         rev   "high ASCII rev"
         dfb   $80
         rev   'never send to know for whom the bell tolls; it tolls for thee.'
         rev   'iminishes me, because I am involved in mankind, and therefore '
         rev   'a manor of thy friends or of thine own were.  Any mans death d'
         rev   'e is the less. As well as if a promontory were. As well as if '
         rev   ' part of the main.  If a clod be washed away by the sea, Europ'
         rev   'nd, entire of itself; every man is a piece of the continent, a'
         rev   'No man is an isla'
         dfb   $81
         dfb   $00
         dfb   $80
         asc   'low ASCII strz',00
         dfb   $80
         asc   "high ASCII strz",00
         dfb   $80
         asc   27,'low',27,'quoted"',2727,'text',00
         dfb   $80
         asc   a2,"high",a2,"quoted'",a2a2,"text",00
         dfb   $80
         asc   '012345678901234567890123456789012345678901234567890123456789'
         asc   27272700
         dfb   $80
         asc   '01234567890123456789012345678901234567890123456789012345678901'
         asc   '234567890123456789',00
         dfb   $81
         str   ''
         dfb   $80
         str   'low ASCII str1'
         dfb   $80
         str   "high ASCII str1"
         dfb   $80
         str   27,'low',27,'quoted"',2727,'text'
         dfb   $80
         str   a2,"high",a2,"quoted'",a2a2,"text"
         dfb   $80
         asc   3f,'01234567890123456789012345678901234567890123456789012345678'
         asc   '9',272727
         dfb   $80
         asc   50,'01234567890123456789012345678901234567890123456789012345678'
         asc   '901234567890123456789'
         dfb   $81
         strl  ''
         dfb   $80
         strl  'low ASCII str2'
         dfb   $80
         strl  "high ASCII str2"
         dfb   $80
         strl  27,'low',27,'quoted"',2727,'text'
         dfb   $80
         strl  a2,"high",a2,"quoted'",a2a2,"text"
         dfb   $80
         asc   3f00,'012345678901234567890123456789012345678901234567890123456'
         asc   '789',272727
         dfb   $80
         asc   5000,'012345678901234567890123456789012345678901234567890123456'
         asc   '78901234567890123456789'
         dfb   $80
         asc   8501,'No man is an island, entire of itself; every man is a pie'
         asc   'ce of the continent, a part of the main.  If a clod be washed '
         asc   'away by the sea, Europe is the less. As well as if a promontor'
         asc   'y were. As well as if a manor of thy friends or of thine own w'
         asc   'ere.  Any mans death diminishes me, because I am involved in m'
         asc   'ankind, and therefore never send to know for whom the bell tol'
         asc   'ls; it tolls for thee.'
         dfb   $81
         dci   'low ASCII dci'
         dfb   $80
         dci   "high ASCII dci"
         dfb   $80
         dci   27,'low',27,'quoted"',2727,'text'
         dfb   $80
         dci   a2,"high",a2,"quoted'",a2a2,"text"
         dfb   $80
         asc   '012345678901234567890123456789012345678901234567890123456789'
         asc   2727a7
         dfb   $80
         asc   '01234567890123456789012345678901234567890123456789012345678901'
         asc   '23456789012345678',b9
         dfb   $81
         asc   f2,'icd IICSA wol'
         dfb   $80
         asc   72,"icd IICSA hgih"
         dfb   $80
         asc   b9,'87654321098765432109876543210987654321098765432109876543210'
         asc   '98765432109876543210'
         dfb   $80
         asc   ae,'eeht rof sllot ti ;sllot lleb eht mohw rof wonk ot dnes rev'
         asc   'en erofereht dna ,dniknam ni devlovni ma I esuaceb ,em sehsini'
         asc   'mid htaed snam ynA  .erew nwo eniht fo ro sdneirf yht fo ronam'
         asc   ' a fi sa llew sA .erew yrotnomorp a fi sa llew sA .ssel eht si'
         asc   ' eporuE ,aes eht yb yawa dehsaw eb dolc a fI  .niam eht fo tra'
         asc   'p a ,tnenitnoc eht fo eceip a si nam yreve ;flesti fo eritne ,'
         asc   'dnalsi na si nam oN'
         dfb   $81
