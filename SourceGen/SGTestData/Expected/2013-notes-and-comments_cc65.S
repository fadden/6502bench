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
         .setcpu "6502"
plataddr =       $3000      ;address only in platform file

;Short, unboxed comment here!!
; Two spaces after.  More hyp-
;hens?
;         .segment "SEG000"
         .org    $1000
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
         lda     #$05
;*******************************************************************************
;* Short box comment, 80 cols.                                                 *
;*******************************************************************************
         lda     #$06
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
;*****************************
;* Some non-ASCII stuff:     *
;* †•�␇                      *
;*****************************
         lda     #$08
         lda     #$09
         lda     #$0a
         lda     #$0b
         lda     #$0c
         lda     #$0d
         lda     #$0e
         lda     #$0f
         bit     plataddr   ;Pull in plataddr to see the comment on the platform file entry.
         rts

bytes:   .byte   $00,$01,$02,$03,$04,$05,$06,$07,$08,$09,$0a,$0b,$0c,$0d,$0e,$0f ;Comment at the end of a lengthy bulk hex item might overflow various things, but could be wrapped.
         .byte   $00,$01,$02,$03,$04,$05,$06,$07,$08,$09,$0a,$0b,$0c,$0d,$0e,$0f
         .byte   $00,$01,$02,$03,$04,$05,$06,$07,$08,$09,$0a,$0b,$0c,$0d,$0e,$0f
         .byte   $00,$01,$02,$03,$04,$05,$06,$07,$08,$09,$0a,$0b,$0c,$0d,$0e,$0f
