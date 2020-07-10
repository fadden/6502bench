;Segment 02: Kind=Code; Attrs=NoSpecial; Name='          '
         org   $030000
         mx    %00
L30000   clc
         xce
         mx    %11
         sep   #$30
         ldal  L30000
         nop
         jsl   :L23456
         jsl   :L80000
         ldal  :L23456
         lda   :L23456
         lda   #<:L23456
         lda   #>:L23456
         lda   #^:L23456
         ldal  :L80019
         lda:  :L80019
         lda   #<:L80019
         lda   #>:L80019
         lda   #^:L80019
         nop
         rep   #$30
         mx    %00
         lda   #:L80019
         lda   #>:L80019
         lda   #^:L80019
         nop
         lda   #$000f
         ldx   #:L80026
         ldy   #:L2345E
         mvn   #^:L2345E+$60000,#^:L2345E
         nop
:L30045  pea   $0000
         pea   $f000
         pea   :L80019
         pea   ^:L80019
         pea   :L30045
         pea   >:L30045
:L30057  pea   ^:L30045
         pea   :L30057
         pea   $1045
         nop
         jmp   :L3006D

         dw    :L4FFE0
         adr   :L4FFE0
         adr   :L4FFE0
         dfb   $00

:L3006D  phk
         plb
         lda   #$0000
         asl   A
         asl   A
         tax
         lda:  :L30082,x
         pha
         lda:  :L30080,x
         pha
         beq   :L30088
         rts

:L30080  dfb   $45
         dfb   $00
:L30082  dfb   $03
         dfb   $00
         adr   :L3006D
         dfb   $00

:L30088  rts

;Segment 03: Kind=Data; Attrs=BankRel, Dynamic; Name='PosFFE0   '
         org   $04ffe0
:L4FFE0  adr   :L4FFE0
         dfb   $00
         dfb   $00
         dfb   $01
         dfb   $02
         dfb   $03
         dfb   $04
         dfb   $05
         dfb   $06
         dfb   $07
         dfb   $08
         dfb   $09
         dfb   $0a
         dfb   $0b
         dfb   $0c
         dfb   $0d
         dfb   $0e
         dfb   $0f
         dfb   $10
         dfb   $11
         dfb   $12
         dfb   $13
         dfb   $14
         dfb   $15
         dfb   $16
         dfb   $17
         dfb   $18
         dfb   $19
         dfb   $1a
         dfb   $1b
         org   $050000
         dfb   $1c
         dfb   $1d
         dfb   $1e
         dfb   $1f
         asc   ' !"#$%&',27,'()*+,-./'

;Segment 04: Kind=Code; Attrs=NoSpecial; Name='Bank2     '
         org   $023456
         mx    %11
:L23456  ldal  :L23456
         jsr   :L2346E
         rtl

:L2345E  ds    16

:L2346E  nop
         rts

;Segment 05: Kind=Code; Attrs=AbsBank, Dynamic; Name='Bank8     '
         org   $080000
:L80000  ldal  :L80000
         lda:  :L80019
         nop
         ldal  $010000
         ldal  $020000
         ldal  L30000
         ldal  :L80000
         rtl

:L80019  adr   :L80019
         dfb   $00
         adr   :L23456
         dfb   $00
         dfb   $80
         dfb   $00
         dfb   $10
         dfb   $08
         dfb   $00
:L80026  asc   'This is a test.'
         dfb   $00
;Segment 06: Kind=Data; Attrs=0; Name='Filler    '
         org   $060000
         asc   'hello, world!'
