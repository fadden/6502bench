*Project file was edited to get zero-length strings and reverse DCI.
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
         hex   f269636420494943534120776f6c
         dfb   $80
         hex   72e9e3e4a0c9c9c3d3c1a0e8e7e9e8
         dfb   $80
         hex   b938373635343332313039383736353433323130393837363534333231303938
         hex   3736353433323130393837363534333231303938373635343332313039383736
         hex   35343332313039383736353433323130
         dfb   $80
         hex   ae6565687420726f6620736c6c6f74207469203b736c6c6f74206c6c65622065
         hex   6874206d6f687720726f6620776f6e6b206f7420646e657320726576656e2065
         hex   726f66657265687420646e61202c646e696b6e616d206e69206465766c6f766e
         hex   69206d6120492065737561636562202c656d2073656873696e696d6964206874
         hex   61656420736e616d20796e4120202e65726577206e776f20656e69687420666f
         hex   20726f2073646e656972662079687420666f20726f6e616d2061206669207361
         hex   206c6c6577207341202e657265772079726f746e6f6d6f727020612066692073
         hex   61206c6c6577207341202e7373656c206568742073692065706f727545202c61
         hex   65732065687420796220796177612064656873617720656220646f6c63206120
         hex   664920202e6e69616d2065687420666f20747261702061202c746e656e69746e
         hex   6f632065687420666f2065636569702061207369206e616d207972657665203b
         hex   666c6573746920666f20657269746e65202c646e616c7369206e61207369206e
         hex   616d206f4e
         dfb   $81
