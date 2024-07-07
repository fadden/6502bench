;***************************************
;* Old school boxed output header.     *
;* Brk                                 *
;* multiple lines yay.  How about a    *
;* hy-phenated word?                   *
;* Looonglonglonglonglonglonglonglongl *
;* onglonglongword.                    *
;***************************************
;* Throw in a line divider.  These     *
;* aren't:                             *
;* *!                                  *
;*  *                                  *
;* &XYZ                                *
;*                                     *
;***************************************
        !cpu    6502
plataddr =      $3000      ;address only in platform file

;Short, unboxed comment here!!
;Two spaces after.  More hyp-
;hens?
*       =       $1000
        lda     #$01       ;Comment!
;Comment rulers can be helpful in findin the edges of notes.  Comments are hyph-
;enatingly fun.  Like the note, this goes out to 80 columns.
        lda     #$02       ;&another comment with &&s!
;Down to 64 columns this time.  Why 64?  Why not 64.  A rose, by
;any other name, would break the line at the same place.  Or hy-
;phen split.
        lda     #$03
;Ah, the classic 40-column limitation...
;brings back memories.  Of, you know, h-
;yphenated things.
        lda     #$04
;Thirty columns.  'cause forty
;felt like too many.  Oh, hyp-
;henation!
        lda     #$05       ;comment ending in backslash\
;*******************************************************************************
;* Short box comment, 80 cols.                                                 *
;*******************************************************************************
        lda     #$06       ;comment, with, commas \\ \" \/ \\\
;***************************************************************
;*                                                             *
;* Choppy                                                      *
;*                                                             *
;* box                                                         *
;*                                                             *
;* comment                                                     *
;*                                                             *
;* 64 cols                                                     *
;*                                                             *
;***************************************************************
        lda     #$07
;***************************************************************
;* Some non-ASCII stuff:                                       *
;* †•�␇                                                        *
;* (right side of box looks pushed over on-screen because font *
;* isn't fully mono)                                           *
;***************************************************************
        lda     #$08
;***************************************
;* Specific line break case at end     *
;* of.  Sentence.                      *
;* Alternate version for semicolon zz. *
;* Output.                             *
;*                                     *
;* Same thing but with three spaces    *
;* zz.   Three!                        *
;* Again with the three spces between. *
;*  Things.                            *
;*                                     *
;* Try to make it the very last        *
;* thing..                             *
;***************************************
        lda     #$09
        lda     #$0a
        lda     #$0b
        lda     #$0c
        lda     #$0d
        lda     #$0e
        lda     #$0f
        jmp     L1062

        !byte   $ea
bytes   !hex    000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f ;Comment at the end of a lengthy bulk hex item might overflow various things, but could be wrapped.
        !hex    000102030405060708090a0b0c0d0e0f000102030405060708090a0b0c0d0e0f

L1062   bit     plataddr   ;Pull in plataddr to see the comment on the platform file entry.
;
;A trivial fancy comment.  Nothing special about it at all.  Go on about your
;business.
;
        lda     #$10
;Check for surrogate pair splitting. 1/2 should be split:
;zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzxxxxxxxxxxxxxxxxxxx1
;2x
;zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzUnicode_Character_“
;😊”_(U+1F60A)
        lda     #$11
;Test [width].  This line has the default width of 80, so things break way out
;here.
;Lines with width=8, width=10, and width=*: 
;0123456
;7890123
;4567890
;1234567
;89
;012345678
;901234567
;890123456
;789
;012345678901234567890123456789
        lda     #$12
;Test [hr] and [hr char='#'].
;-------------------------------------------------------------------------------
;-------------------------------------------------------------------------------
;x
;###############################################################################
;y
        lda     #$13

;Testing [br] blank lines.



;three above, one below

        lda     #$14
;-------------------------------------------------------------------------------
;Full width box, with default char.
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; stuff in the box zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz aaaa bbb  ;
; cccc                                                                         ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; Explicit char=';'.                                                          ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;Now with reduced width and char='#'.
;#########
;# A     #
;# much  #
;# small #
;# er    #
;# box.  #
;#########
;*******************************************************************************
;* Width changes are illegal inside[width=20]a box (with char='*').  Word wrap *
;* at the end.                                                                 *
;*******************************************************************************
        lda     #$15
;Simple URL https://example.com/, fancier URL link text (https://example.com/).
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; In a box https://example.com/.                                               ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
;A URL may not have a [box]...[/box] tag inside.
;Nor may it have an [hr], [br], or [width=2] tag.
;Line wrapping the contents of a URL link area should work +this is the area
;inside+ (https://example.com).
        lda     #$16
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
; ALL CAPS FOR EVERYTHING (HTTPS://EXAMPLE.COM/)                               ;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;SHORT
;LINES
;XXXXXXXXX
;X BOXED X
;XXXXXXXXX
        lda     #$17
;Test some folding
;with [width=20].
;0123456789012345678
;9
;012345678901234567-
;89
;0123456789012345678
;9
; 123456789012345678
;9
;-123456789012345678
;9
; -
;234567890123456789
        lda     #$18
        lda     #$19
        lda     #$1a
        lda     #$1b
        lda     #$1c
        lda     #$1d
        lda     #$1e
        lda     #$1f
        lda     #$20
        lda     #$21
        lda     #$22
        lda     #$23
        lda     #$24
        lda     #$25
        lda     #$26
        lda     #$27
        lda     #$28
        lda     #$29
        lda     #$2a
        lda     #$2b
        lda     #$2c
        lda     #$2d
        lda     #$2e
        lda     #$2f
        jmp     @L10A8

@L10A8  rts

