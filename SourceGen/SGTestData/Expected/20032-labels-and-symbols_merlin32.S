*Project was edited to add a label in the middle of a dense hex region, and add
*a duplicate label.
BMI      equ   $30        ;opcode mnemonic
zip      equ   $cd
absl     equ   $1029
absh     equ   $feed
biggie   equ   $123456
thirty2  equ   $12345678  ;32-bit constant test

plataddr equ   $3000      ;address only in platform file
projalsa equ   $3200      ;same val as projalso

         org   $012345
start    clc
         xce
         sep   #$30
         lda   #zip
         lda   #zip+16
         lda   #zip+64
         lda   #<absh
         lda   #>absh
         lda   #<absh+64
         lda   #>absh+$100
         lda   #<absl
         lda   #>absl
         lda   #<absl-64
         lda   #>absl-$100
         lda   #<start
         lda   #>start
         lda   #^start
         pea   $feed
         pea   $2345
         pea   $0001
         pea   $3456
         pea   $0012
         pea   absh
         pea   start
         pea   ^start
         pea   biggie
         pea   ^biggie
         lda   zip+1
         lda:  zip+1
         ldal  zip+1
         lda   absh-1
         ldal  absh-1
         lda   absh+1
         ldal  absh+1
         lda   start+1
         ldal  start+1
         lda   start-1
         ldal  start-1
         lda   biggie+1
         ldal  biggie+1
         lda   biggie-1
         ldal  biggie-1
         rep   #$30
         mx    %00
         lda   #zip
         lda   #zip+16
         lda   #zip+64
         lda   #absl
         lda   #>absl
         lda   #absl-$1000
         lda   #>absl-$1000
         lda   #^absl
         lda   #absh
         lda   #>absh
         lda   #absh+$1000
         lda   #>absh+$1000
         lda   #^absh+$10000
         lda   #start
         lda   #>start
         lda   #^start
         lda   #biggie
         lda   #>biggie
         lda   #^biggie
         bra   :skipdata

         dfb   zip
         dfb   <absh
         dfb   >absh
         dfb   <start
         dfb   >start
         dfb   ^start
         dw    zip
         dw    absl
         dw    >absl
         dw    absl-$1000
         dw    >absl-$1000
         dw    absh
         dw    >absh
         dw    absh+$1000
         dw    >absh+$1000
         dw    start
         dw    >start
         dw    ^start
         dw    start+1
         dw    >start
         dw    ^start
         ddb   absh
         adr   zip
         adr   absh
         adr   >absh
         adr   start
         adr   >start
         adr   ^start
         adrl  zip
         adrl  absh
         adrl  >absh
         adrl  start-1
         adrl  >start
         adrl  ^start

:skipdata lda  #^biggie-$10000
         mvn   #^biggie,#^biggie-$110000
         mvp   #^start,#^start+$110000
         mvn   #18,#1
         mvp   #%00000001,#%00010010
         per   :skipdata
         brl   :nextchunk

:nextchunk jml :L1000_1

         org   $1000
:L1000_1 nop
:L1000   nop
:L1000_0 nop
:l1000   sep   #$30
         mx    %11
         lda   plataddr
         lda   $3100
         lda   projalsa
         lda   $3300
         bra   :calls

         nop
:targ    nop
         nop
:L1016   per   :targ-1
         per   :targ
         per   :targ+1
         jsr   :targ-1
         jsr   :targ
         jsr   :targ+1
:L1028   bra   :targ-1

:L102A   bra   :targ

:L102C   bra   :targ+1

:L102E   brl   :targ-1

:L1031   brl   :targ

:L1034   brl   :targ+1

:L1037   jmp   :targ-1

:L103A   jmp   :targ

:L103D   jmp   :targ+1

:L1040   jml   :targ-1

:L1044   jml   :targ

:L1048   jml   :targ+1

:calls   jsr   :L1016
         jsr   :L1028
         jsr   :L102A
         jsr   :L102C
         jsr   :L102E
         jsr   :L1031
         jsr   :L1034
         jsr   :L1037
         jsr   :L103A
         jsr   :L103D
         jsr   :L1040
         jsr   :L1044
         jsr   :L1048
         brl   :L118E

:bulk    hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f ;bulky
         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f
         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f
         hex   808182838485868788898a8b8c8d8e8f
:string  asc   'This is a long string. Put a label and comment on it to confir' ;stringy
         asc   'm that the label and comment only appear on the first line.  T'
         asc   'he quick brown fox jumps over the lazy dogs.'

:L118E   lda   #<thirty2+2
         lda   #>thirty2+$300
         lda   #^thirty2
         rep   #BMI
         mx    %00
         lda   #thirty2+3
         lda   #>thirty2+$400
         lda   #^thirty2
before   nop
         lda   before
         lda   <before&$ff
         lda   <before+1&$ff
         dfb   $b2,$a1
         lda   (<before+3&$ff),y
         lda   [<before+4&$ff],y
         dfb   $a7,$a4
         lda   <before+6&$ff,x
         dfb   $a1,$a6
         lda   before-$10f8,y
         lda   <after+6&$ff
         lda   (<after+7&$ff),y
after    ldx   <after+8&$ff,y
         ldy   <after+9&$ff,x
         dfb   $d4,$c4
         nop
         jml   :L21000

         org   $021000
:L21000  nop
ptest    pea   $1001
         pea   $1001
         pea   ptest
         pea   $3456
         pea   $0012
         pea   $3456
         pea   $0012
         pea   biggie
         pea   ^biggie
         rts

