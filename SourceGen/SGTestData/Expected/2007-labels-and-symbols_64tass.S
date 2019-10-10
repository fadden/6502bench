;Project was edited to add a label in the middle of a dense hex region, and add
;a duplicate label.
        .cpu    "65816"
        .enc    sg_ascii
        .cdef   $20,$7e,$20
zip     =       $cd
absl    =       $1029
absh    =       $feed
biggie  =       $123456
thirty2 =       $12345678  ;32-bit constant test
plataddr =      $3000      ;address only in platform file
projalsa =      $3200      ;same val as projalso

*       =       $012345
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
        pea     0+(start & $ffff)
        pea     $0001
        pea     $3456
        pea     $0012
        pea     absh
        pea     0+(start & $ffff)
        pea     0+(start >> 16)
        pea     0+(biggie & $ffff)
        pea     0+(biggie >> 16)
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
        lda     #(absl >> 8)
        lda     #absl-4096
        lda     #(absl >> 8)-16
        lda     #(absl >> 16)
        lda     #absh
        lda     #(absh >> 8)
        lda     #absh-61440
        lda     #(absh >> 8)+16
        lda     #(absh >> 16)+1
        lda     #(start & $ffff)
        lda     #(start >> 8)
        lda     #(start >> 16)
        lda     #(biggie & $ffff)
        lda     #(biggie >> 8)
        lda     #(biggie >> 16)
        bra     skipdata

        .byte   zip
        .byte   <absh
        .byte   >absh
        .byte   <start
        .byte   >start
        .byte   `start
        .word   zip
        .word   absl
        .word   0+(absl >> 8)
        .word   absl-4096
        .word   0+(absl >> 8)-16
        .word   absh
        .word   0+(absh >> 8)
        .word   absh-61440
        .word   0+(absh >> 8)+16
        .word   0+(start & $ffff)
        .word   0+(start >> 8)
        .word   0+(start >> 16)
        .word   0+(start & $ffff)+1
        .word   0+(start >> 8)
        .word   0+(start >> 16)
        .byte   $fe,$ed
        .long   zip
        .long   absh
        .long   0+(absh >> 8)
        .long   start
        .long   0+(start >> 8)
        .long   0+(start >> 16)
        .dword  zip
        .dword  absh
        .dword  0+(absh >> 8)
        .dword  start-1
        .dword  0+(start >> 8)
        .dword  0+(start >> 16)

skipdata lda    #(biggie >> 16)-1
        mvn     #`biggie,#(`biggie)-17
        mvp     #`start,#(`start)+17
        mvn     #18,#1
        mvp     #%00000001,#%00010010
        per     skipdata
        brl     nextchunk

nextchunk jml   L1000_1

        .logical $1000
L1000_1 nop
L1000   nop
L1000_0 nop
l1000   sep     #$30
        .as
        .xs
        lda     plataddr
        lda     $3100
        lda     projalsa
        lda     $3300
        bra     calls

        nop
targ    nop
        nop
L1016   per     targ-1
        per     targ
        per     targ+1
        jsr     targ-1
        jsr     targ
        jsr     targ+1
L1028   bra     targ-1

L102A   bra     targ

L102C   bra     targ+1

L102E   brl     targ-1

L1031   brl     targ

L1034   brl     targ+1

L1037   jmp     targ-1

L103A   jmp     targ

L103D   jmp     targ+1

L1040   jml     targ-1

        jml     targ

        jml     targ+1

calls   jsr     L1016
        jsr     L1028
        jsr     L102A
        jsr     L102C
        jsr     L102E
        jsr     L1031
        jsr     L1034
        jsr     L1037
        jsr     L103A
        jsr     L103D
        jsr     L1040
        jsr     $1044
        jsr     $1048
        brl     L118E

bulk    .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f ;bulky
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
        .byte   $80,$81,$82,$83,$84,$85,$86,$87,$88,$89,$8a,$8b,$8c,$8d,$8e,$8f
string  .text   "This is a long string. Put a label and comment on it to confir" ;stringy
        .text   "m that the label and comment only appear on the first line.  T"
        .text   "he quick brown fox jumps over the lazy dogs."

L118E   lda     #<thirty2+2
        lda     #(>thirty2)+3
        lda     #`thirty2
        rep     #$30
        .al
        .xl
        lda     #(thirty2 & $ffff)+3
        lda     #((thirty2 >> 8) & $ffff)+4
        lda     #(thirty2 >> 16)
        rts

        .here
