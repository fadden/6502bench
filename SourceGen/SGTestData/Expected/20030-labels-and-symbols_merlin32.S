*Project was edited to add a label in the middle of a dense hex region, and add
*a duplicate label.
BMI      equ   $30        ;opcode mnemonic
zip      equ   $cd
absl     equ   $1029
absh     equ   $feed

plataddr equ   $3000      ;address only in platform file
projalsa equ   $3200      ;same val as projalso

         org   $2345
start    lda   #zip
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
         lda   zip+1
         lda:  zip+1
         lda   absh-1
         lda   absh+1
         lda   start+1
         lda   start-1
         jmp   :L23A3

         dfb   zip
         dfb   <absh
         dfb   >absh
         dfb   <start
         dfb   >start
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
         dw    start+1
         dw    >start
         ddb   absh
         adr   zip
         adr   absh
         adr   >absh
         adr   start
         adr   >start

:L23A3   jmp   :L1000_1

         org   $1000
:L1000_1 nop
:L1000   nop
:L1000_0 nop
:l1000   lda   plataddr
         lda   $3100
         lda   projalsa
         lda   $3300
         jmp   calls

         nop
targ     nop
         nop
L1015    jsr   targ-1
         jsr   targ
         jsr   targ+1
L101E    jmp   targ-1

L1021    jmp   targ

L1024    jmp   targ+1

L1027    jmp   targ-1

L102A    jmp   targ

L102D    jmp   targ+1

calls    jsr   L1015
         jsr   L101E
         jsr   L1021
         jsr   L1024
         jsr   L1027
         jsr   L102A
         jsr   L102D
         jmp   L1160

         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f ;bulky
         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f
         hex   808182838485868788898a8b8c8d8e8f808182838485868788898a8b8c8d8e8f
         hex   808182838485868788898a8b8c8d8e8f
         asc   'This is a long string. Put a label and comment on it to confir' ;stringy
         asc   'm that the label and comment only appear on the first line.  T'
         asc   'he quick brown fox jumps over the lazy dogs.'

L1160    adc   #BMI
         lda   <targ-1&$ff
         lda   <targ+1&$ff
         lda   <targ-1&$ff,x
         lda   (<targ+4&$ff),y
         rts

