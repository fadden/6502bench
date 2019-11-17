;Project was edited to add a label in the middle of a dense hex region, and add
;a duplicate label.
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
         lda   #>absh+256
         lda   #<absl
         lda   #>absl
         lda   #<absl-64
         lda   #>absl-256
         lda   #<start
         lda   #>start
         lda   #^start
         pea   $feed
         pea   start
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
         lda   #absl-4096
         lda   #>absl-4096
         lda   #^absl
         lda   #absh
         lda   #>absh
         lda   #absh+4096
         lda   #>absh+4096
         lda   #^absh+65536
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
         dw    absl-4096
         dw    >absl-4096
         dw    absh
         dw    >absh
         dw    absh+4096
         dw    >absh+4096
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

:skipdata lda  #^biggie-65536
         mvn   #^biggie,#^biggie-1114112
         mvp   #^start,#^start+1114112
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

         jml   :targ

         jml   :targ+1

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
         jsr   $1044
         jsr   $1048
         brl   :L118E

:bulk    hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f ;bulky
         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f
         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f
         hex   808182838485868788898a8b8c8d8e8f
:string  asc   'This is a long string. Put a label and comment on it to confir' ;stringy
         asc   'm that the label and comment only appear on the first line.  T'
         asc   'he quick brown fox jumps over the lazy dogs.'

:L118E   lda   #<thirty2+2
         lda   #>thirty2+768
         lda   #^thirty2
         rep   #$30
         mx    %00
         lda   #thirty2+3
         lda   #>thirty2+1024
         lda   #^thirty2
         rts

