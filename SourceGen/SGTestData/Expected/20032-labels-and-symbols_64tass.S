;Project was edited to add a label in the middle of a dense hex region, and add
;a duplicate label.
        .cpu    "65816"
        .enc    "sg_ascii"
        .cdef   $20,$7e,$20
BMI1    =       $30        ;opcode mnemonic
zip     =       $cd
absl    =       $1029
absh    =       $feed
biggie  =       $123456
thirty2 =       $12345678  ;32-bit constant test

plataddr =      $3000      ;address only in platform file
projalsa =      $3200      ;same val as projalso

        .logical $012345
        .as
        .xs
start   clc
        xce
        sep     #$30
        lda     #zip
        lda     #zip+16
        lda     #zip-192
        lda     #<absh
        lda     #>absh
        lda     #<absh-192
        lda     #(>absh)+1
        lda     #<absl
        lda     #>absl
        lda     #<absl+192
        lda     #(>absl)-1
        lda     #<start
        lda     #>start
        lda     #`start
        pea     $feed
        pea     $2345
        pea     $0001
        pea     $3456
        pea     $0012
        pea     absh
        pea     start & $ffff
        pea     start >> 16
        pea     biggie & $ffff
        pea     biggie >> 16
        lda     zip+1
        lda     @wzip+1
        lda     @lzip+1
        lda     absh-1
        lda     @labsh-1
        lda     absh+1
        lda     @labsh+1
        lda     0+(start & $ffff)+1
        lda     start+1
        lda     0+(start & $ffff)-1
        lda     start-1
        lda     0+(biggie & $ffff)+1
        lda     biggie+1
        lda     0+(biggie & $ffff)-1
        lda     biggie-1
        rep     #$30
        .al
        .xl
        lda     #zip
        lda     #zip+16
        lda     #zip+64
        lda     #absl
        lda     #absl >> 8
        lda     #absl-$1000
        lda     #(absl >> 8)-16
        lda     #absl >> 16
        lda     #absh
        lda     #absh >> 8
        lda     #absh-$f000
        lda     #(absh >> 8)+16
        lda     #(absh >> 16)+1
        lda     #start & $ffff
        lda     #start >> 8
        lda     #start >> 16
        lda     #biggie & $ffff
        lda     #biggie >> 8
        lda     #biggie >> 16
        bra     _skipdata

        .byte   zip
        .byte   <absh
        .byte   >absh
        .byte   <start
        .byte   >start
        .byte   `start
        .word   zip
        .word   absl
        .word   absl >> 8
        .word   absl-$1000
        .word   0+(absl >> 8)-16
        .word   absh
        .word   absh >> 8
        .word   absh-$f000
        .word   0+(absh >> 8)+16
        .word   start & $ffff
        .word   start >> 8
        .word   start >> 16
        .word   0+(start & $ffff)+1
        .word   start >> 8
        .word   start >> 16
        .byte   $fe,$ed
        .long   zip
        .long   absh
        .long   absh >> 8
        .long   start
        .long   start >> 8
        .long   start >> 16
        .dword  zip
        .dword  absh
        .dword  absh >> 8
        .dword  start-1
        .dword  start >> 8
        .dword  start >> 16

_skipdata lda   #(biggie >> 16)-1
        mvn     #`biggie,#(`biggie)-17
        mvp     #`start,#(`start)+17
        mvn     #18,#1
        mvp     #%00000001,#%00010010
        per     _skipdata
        brl     _nextchunk

_nextchunk jml  _L1000_1

        .here
        .logical $1000
_L1000_1 nop
_L1000  nop
_L1000_0 nop
_l1000  sep     #$30
        .as
        .xs
        lda     plataddr
        lda     $3100
        lda     projalsa
        lda     $3300
        bra     _calls

        nop
_targ   nop
        nop
_L1016  per     _targ-1
        per     _targ
        per     _targ+1
        jsr     _targ-1
        jsr     _targ
        jsr     _targ+1
_L1028  bra     _targ-1

_L102A  bra     _targ

_L102C  bra     _targ+1

_L102E  brl     _targ-1

_L1031  brl     _targ

_L1034  brl     _targ+1

_L1037  jmp     _targ-1

_L103A  jmp     _targ

_L103D  jmp     _targ+1

_L1040  jml     _targ-1

_L1044  jml     _targ

_L1048  jml     _targ+1

_calls  jsr     _L1016
        jsr     _L1028
        jsr     _L102A
        jsr     _L102C
        jsr     _L102E
        jsr     _L1031
        jsr     _L1034
        jsr     _L1037
        jsr     _L103A
        jsr     _L103D
        jsr     _L1040
        jsr     _L1044
        jsr     _L1048
        brl     _L118E

_bulk   .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f ;bulky
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
_string .text   "This is a long string. Put a label and comment on it to confir" ;stringy
        .text   "m that the label and comment only appear on the first line.  T"
        .text   "he quick brown fox jumps over the lazy dogs."

_L118E  lda     #<thirty2+2
        lda     #(>thirty2)+3
        lda     #`thirty2
        rep     #BMI1
        .al
        .xl
        lda     #(thirty2 & $ffff)+3
        lda     #((thirty2 >> 8) & $ffff)+4
        lda     #thirty2 >> 16
before  nop
        lda     before
        lda     <before
        lda     <before+1
        lda     (<before+2)
        lda     (<before+3),y
        lda     [<before+4],y
        lda     [<before+5]
        lda     <before+6,x
        lda     (<before+7,x)
        lda     before-$10f8,y
        lda     <after+6
        lda     (<after+7),y
after   ldx     <after+8,y
        ldy     <after+9,x
        pei     (<after+10)
        nop
        jml     _L21000

        .here
        .logical $021000
_L21000 nop
        .as
        .xs
ptest   pea     $1001
        pea     $1001
        pea     ptest & $ffff
        pea     $3456
        pea     $0012
        pea     $3456
        pea     $0012
        pea     biggie & $ffff
        pea     biggie >> 16
        jmp     L22000

        .here
        .logical $020010
testlow .byte   $02
        .here
        .logical $0010
testzp  .byte   $00
        .here

        .logical $022000
L22000  nop
        phk
        plb
        lda     $123456,x
        jsl     $123456
        jsr     $023456
        lda     ptest & $ffff,x
        jsr     ptest
        lda     @wtestlow & $ffff,x
        jsr     testlow
        lda     testzp,x
        nop
        lda     biggie,x
        jsl     biggie
        jsr     biggie & $ffff,k
        lda     ptest & $ffff,x
        jsr     ptest
        lda     @wtestlow & $ffff,x
        jsr     testlow
        lda     testzp,x
        nop
        lda     $123456,x
        jsl     $123456
        jsr     $023456
        lda     $1001,x
        jsr     $021001
        lda     @w$0010,x
        jsr     $020010
        lda     $10,x
        nop
        sep     #$30
        lda     #$e0
        pha
        plb
        lda     1193046,x
        jsl     1193046
        jsr     144470
        lda     4097,x
        jsr     135169
        lda     @w16,x
        jsr     131088
        lda     16,x
        rts

        .here
