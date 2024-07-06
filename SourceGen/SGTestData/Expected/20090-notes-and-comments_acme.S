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
        lda     #$11
        lda     #$12
        lda     #$13
        lda     #$14
        lda     #$15
        lda     #$16
        lda     #$17
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

