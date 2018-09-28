********************************
*                              *
* Amper-fdraw                  *
* By Andy McFadden             *
* For fdraw version 0.3        *
*                              *
* Applesoft ampersand          *
* interface for fdraw.         *
*                              *
* Developed with Merlin-16     *
*                              *
********************************

         lst   off
         org   $1d60

* All of the handler entry points can fit on a single
* page, so it's possible to save a few bytes by
* dropping the high jump table and just hardcoding
* the first page into the jump.  This requires that
* the ORG be at $xx00.

         PUT   FDRAW.DEFS

* Applesoft BASIC tokens.
tok_plot equ   $8d
tok_hgr2 equ   $90
tok_hgr  equ   $91
tok_hcolor equ $92
tok_hplot equ  $93
tok_draw equ   $94
tok_xdraw equ  $95
tok_inverse equ $9e
tok_clear equ  $bd
tok_new  equ   $bf
tok_to   equ   $c1
tok_at   equ   $c5
*tok_sgn equ $d2
tok_scrn equ   $d7
tok_exp  equ   $dd
tok_cos  equ   $de
tok_sin  equ   $df

* System locations.
PCL      equ   $3a        ;used by monitor
PCH      equ   $3b        ;used by monitor
A1L      equ   $3c        ;used by monitor
A1H      equ   $3d        ;used by monitor
LINNUM   equ   $50        ;50-51
FACLO    equ   $a1
CHRGET   equ   $b1        ;advance ptr, get next tok
CHRGOT   equ   $b7        ;get next tok (no advance)
TXTPTR   equ   $b8
HPAG     equ   $e6        ;$20 or $40

AMPERV   equ   $3f5

TXTCLR   equ   $c050
TXTSET   equ   $c051
MIXCLR   equ   $c052
MIXSET   equ   $c053
LOWSCR   equ   $c054
HISCR    equ   $c055
LORES    equ   $c056
HIRES    equ   $c057

ERROR    equ   $d412      ;error based on X reg
FRMNUM   equ   $dd67
SynError equ   $dec9      ;throw SYNTAX ERROR
CHKCOM   equ   $debe
IllQError equ  $e199      ;throw ILLEGAL QUANTITY ERROR
GETADR   equ   $e752
GETBYT   equ   $e6f8      ;gets byte, in X/FACLO
HFNS     equ   $f6b9      ;get hi-res x/y for hplot

* Prepare the ampersand vector.
*
* Ideally we'd check to see if the existing vector is
* different from ours, and if so, jump to it when we
* get a token we don't recognize.  Not convinced
* there's an actual use case for this.
init
         lda   #$4c       ;JMP, in case it got
         sta   AMPERV     ; trashed
         lda   #<dispatch
         sta   AMPERV+1
         lda   #>dispatch
         sta   AMPERV+2
         rts

* Entry point from BASIC.  The token is in A.
dispatch
         ldx   #:cmdend-:cmdtab-1
]loop    cmp   :cmdtab,x
         beq   :match
         dex
         bpl   ]loop
         jmp   SynError

:match
         lda   :jmptabh,x
* lda #>h_new ;all on first page
         pha
         lda   :jmptabl,x
         pha
         jmp   CHRGET     ;eat token, jump


:cmdtab  dfb   tok_new
         dfb   tok_hgr
         dfb   tok_hgr2
         dfb   tok_scrn
         dfb   tok_hcolor
         dfb   tok_inverse
         dfb   tok_clear
         dfb   tok_hplot
         dfb   tok_xdraw
         dfb   tok_draw
         dfb   tok_exp
         dfb   tok_cos
         dfb   tok_sin
         dfb   tok_at
         dfb   tok_plot
:cmdend

:jmptabl dfb   <h_new-1
         dfb   <h_hgr-1
         dfb   <h_hgr2-1
         dfb   <h_scrn-1
         dfb   <h_hcolor-1
         dfb   <h_inverse-1
         dfb   <h_clear-1
         dfb   <h_hplot-1
         dfb   <h_xdraw-1
         dfb   <h_draw-1
         dfb   <h_exp-1
         dfb   <h_cos-1
         dfb   <h_sin-1
         dfb   <h_at-1
         dfb   <h_plot-1
:jmptabh dfb   >h_new-1
         dfb   >h_hgr-1
         dfb   >h_hgr2-1
         dfb   >h_scrn-1
         dfb   >h_hcolor-1
         dfb   >h_inverse-1
         dfb   >h_clear-1
         dfb   >h_hplot-1
         dfb   >h_xdraw-1
         dfb   >h_draw-1
         dfb   >h_exp-1
         dfb   >h_cos-1
         dfb   >h_sin-1
         dfb   >h_at-1
         dfb   >h_plot-1


********************************
* &NEW - initialize
h_new
         lda   #$20       ;match Init result
         sta   g_cur_page
         lda   #$00
         sta   g_hcolor
         tax              ;init "previous hplot"
         tay              ; coord to zero
         jsr   storeprv
         ldx   #139       ;279/2
         ldy   #0
         lda   #95        ;191/2
         jsr   storeac
         jmp   f_Init

********************************
* &HGR - show page 1 with mixed text, and clear screen.
* Sets the color to zero.
h_hgr
         ldx   #$20       ;page 1
         lda   #$00       ;$c054
         beq   hgr_com

********************************
* &HGR2 - show page 2 with no text, and clear screen.
* Sets the color to zero.
h_hgr2
         ldx   #$40       ;page 2
         lda   #$01       ;$c055
                          ;fall through to hgr_com

* We go slightly out of our way to clear the screen
* before tripping the softswitches.  This avoids
* flashing the previous hi-res page contents when
* entering from text mode.
*
* We also want to go nomix-page2 but page1-mix
* (note reverse order) to avoid flashing text pg 2.
hgr_com  stx   f_in_arg
         stx   g_cur_page
         stx   HPAG       ;probably useful
         pha
         jsr   f_SetPage
         lda   #$00
         sta   f_in_arg
         jsr   f_SetColor
         jsr   f_Clear
         lda   g_hcolor   ;restore color
         sta   f_in_arg
         jsr   f_SetColor
         bit   TXTCLR     ;$c050
         bit   HIRES      ;$c057
         pla
         beq   :pg1
         bit   MIXCLR     ;$c052
         bit   HISCR      ;$c055
         rts
:pg1     bit   LOWSCR     ;$c054
         bit   MIXSET     ;$c053
         rts

********************************
* &SCRN({1,2}) - set the current hi-res page
h_scrn
         jsr   GETBYT
         cpx   #1
         beq   :okay
         cpx   #2
         beq   :okay
         jmp   IllQError
:okay    jsr   CHRGET     ;eat ')' (we assume)
         txa              ;X/Y unaltered
         asl
         asl
         asl
         asl
         asl              ;multiply x32
         sta   g_cur_page
         sta   f_in_arg
         jmp   f_SetPage

********************************
* &HCOLOR={0-7} - set the current color
h_hcolor
         jsr   GETBYT     ;get color
         cpx   #8
         blt   :okay
         jmp   IllQError
:okay    stx   f_in_arg
         stx   g_hcolor
         jmp   f_SetColor

********************************
* &INVERSE - flip pages
*
* If we're currently drawing on $20, we set the page
* to $40 and hit $c054 to show $20.  And vice-versa.
* The goal is to make double-buffered animation easy.
h_inverse
         lda   g_cur_page
         eor   #$60
         sta   g_cur_page
         ldx   #$00
         cmp   #$40       ;about to start drawing on 2?
         beq   :showpg1   ;yes, show page 1
         inx              ;no, show page 2
:showpg1 ldy   LOWSCR,x
         sta   f_in_arg
         jmp   f_SetPage

********************************
* &CLEAR - clear current page to current color.
h_clear
         jmp   f_Clear    ;well, that was easy

********************************
* &XDRAW left,top,right,bottom - draw rectangle outline
h_xdraw
         jsr   getltrb
         jmp   f_DrawRect

********************************
* &DRAW left,top,right,bottom - draw filled rectangle
h_draw
         jsr   getltrb
         jmp   f_FillRect

********************************
* &EXP {0,1} - set line draw mode
h_exp
         jsr   GETBYT
         cpx   #2
         blt   :okay
         jmp   IllQError
:okay    stx   f_in_arg
         jmp   f_SetLineMode

********************************
* &COS cx,cy,rad - draw outline circle
h_cos
         jsr   getcxcyr
         jmp   f_DrawCircle

********************************
* &SIN cx,cy,rad - draw filled circle
h_sin
         jsr   getcxcyr
         jmp   f_FillCircle

********************************
* &AT x,y - select center for array draw
h_at
         jsr   HFNS
         jmp   storeac

********************************
* &PLOT vertexAddr, indexAddr, indexCount [AT cx,cy]
*  draw lines from arrays of vertices and indices
h_plot   jmp   array_draw

********************************
* &HPLOT x,y - draw a point
* &HPLOT TO x,y - draw a line from last point to x,y
* &HPLOT x0,y0 to x1,y1 - draw a line
         lst   on         ;last token handler --
h_hplot  equ   *          ; must be on first page
         lst   off        ; to omit high byte table

         jsr   CHRGOT     ;check next token
         lst   off
         cmp   #tok_to    ;is this an "HPLOT TO"?
         beq   :leadingto
         jsr   getx1y1    ;get the first coord
         jsr   copy1to0
         jsr   CHRGOT     ;see if single point
         cmp   #tok_to
         beq   :hplot_to  ;nope, draw line
         jsr   copy0toprev ;draw point, and save x/y
         jmp   f_DrawPoint ; for subsequent HPLOT TO

:leadingto                ;"HPLOT TO", restore the
         lda   g_prevxl   ; previous coord to x0/y0
         sta   f_in_x0l   ;(can't rely on f_in_zzz
         lda   g_prevxh   ; being there -- we might
         sta   f_in_x0h   ; have drawn a rect)
         lda   g_prevy
         sta   f_in_y0
:hplot_to
         jsr   CHRGET     ;eat the TO
         jsr   getx1y1    ;get the coords
         jsr   f_DrawLine ;draw it
         jsr   copy1to0   ;shift 1->0 for next round
         jsr   CHRGOT
         cmp   #tok_to    ;another TO?
         beq   :hplot_to  ;yes, branch
         jmp   copy0toprev ;no, save prev and bail

* Get coordinates and store in X1/Y1.
getx1y1
         jsr   HFNS
store1   stx   f_in_x1l   ;store X/Y/A in coord1
         sty   f_in_x1h
         sta   f_in_y1
         rts

* Save x0/y0 as our "previous" coordinate.
copy0toprev
         ldx   f_in_x0l
         ldy   f_in_x0h
         lda   f_in_y0
storeprv stx   g_prevxl   ;store X/Y/A in g_prev
         sty   g_prevxh
         sta   g_prevy
         rts

* Copy X1/Y1 into X0/Y0.
copy1to0
         ldx   f_in_x1l
         ldy   f_in_x1h
         lda   f_in_y1
store0   stx   f_in_x0l   ;store X/Y/A in coord 0
         sty   f_in_x0h
         sta   f_in_y0
         rts

* Store X/Y/A into array-center.
storeac  stx   g_ac_xl
         sty   g_ac_xh
         sta   g_ac_y
         rts

* Get left/top/right/bottom coordinates.
getltrb
         jsr   HFNS
         jsr   store0     ;save as X0/Y0
         jsr   CHKCOM     ;eat a comma
         jsr   HFNS
         jsr   store1     ;save as X1/Y1
         rts

* Get center coordinates and radius.
getcxcyr
         jsr   HFNS       ;get CX and CY
         jsr   store0     ;save as X0/Y0
         jsr   CHKCOM     ;eat a comma
         jsr   GETBYT     ;convert to 0-255
         stx   f_in_rad
         rts

* Array-draw handler.
*
* We know that fdraw doesn't use LINNUM or A1L/A1H,
* so it's safe to use them here.
array_draw
]vertices equ  A1L        ;2b
]indices equ   LINNUM     ;2b
]count   equ   PCL
]cur     equ   PCH

         jsr   FRMNUM     ;get vertex buffer address
         jsr   GETADR
         lda   LINNUM     ;copy to A1L
         sta   ]vertices
         lda   LINNUM+1
         sta   ]vertices+1
         jsr   CHKCOM     ;eat the comma
         jsr   FRMNUM     ;get index buffer address
         jsr   GETADR     ;leave it in LINNUM
         jsr   CHKCOM
         jsr   GETBYT     ;get the count
         cpx   #128       ;range check (0-127)
         blt   :countok
         jmp   IllQError
:countok txa
         beq   :done      ;nothing to do
         asl              ;double it
         sta   ]count     ;stash it
         lda   #$00
         sta   ]cur

* Check for optional AT cx,cy.
         jsr   CHRGOT
         cmp   #tok_at
         bne   :noat
         JSR   CHRGET     ;eat the AT
         lda   LINNUM     ;the code that reads the
         pha              ; hi-res coordinates will
         lda   LINNUM+1   ; overwrite LINNUM, so
         pha              ; we have to save & restore
         jsr   h_at
         pla
         sta   LINNUM+1
         pla
         sta   LINNUM
:noat

]loop    jsr   getvertex
         bcs   :skip2
         jsr   store0
         jsr   getvertex
         bcs   :skip
         jsr   store1
         jsr   f_DrawLine
         dfb   $2c        ;BIT addr
:skip2   inc   ]cur
:skip    lda   ]cur
         cmp   ]count
         blt   ]loop
:done    rts

* Get the Nth vertex, specified by ]cur, and load it
* into X/Y/A (xlo/xhi/y).  Returns with carry set if
* the vertex is invalid.
*
* Increments ]cur by 1.
getvertex
         ldy   ]cur
         inc   ]cur
         lda   (]indices),y
         bmi   :badv      ;must be 0-127
         jsr   :calcvertex

         ldx   g_out_x
         ldy   g_out_x+1
         beq   :xok       ;0-255, ok
         cpy   #1
         bne   :badv      ;512+
         cpx   #280-256
         bge   :badv      ;280-511
:xok
         lda   g_out_y+1
         bne   :badv      ;Y is neg or > 255
         lda   g_out_y
         cmp   #192
         bcc   :goodv
:badv
         sec
:goodv   rts

* Get VX and VY, merging with AC, and store in
* 16-bit g_out_x and g_out_y.  Range not checked
* here.  On entry, A has vertex index.
:calcvertex
         asl
         tay
         ldx   #$00       ;hi byte of vertex
         lda   (]vertices),y ;x-coord
         bpl   :xpos
         dex              ;sign-extend hi byte
:xpos    clc
         adc   g_ac_xl
         sta   g_out_x
         txa
         adc   g_ac_xh
         sta   g_out_x+1

         iny
         ldx   #$00
         lda   (]vertices),y ;y-coord
         bpl   :ypos
         dex              ;sign-extend hi byte
:ypos    clc
         adc   g_ac_y
         sta   g_out_y
         bcc   :nocarry
         inx
:nocarry stx   g_out_y+1
         rts



********************************
* Global variables

g_cur_page ds  1          ;$20 or $40
g_hcolor ds    1
g_prevxl ds    1
g_prevxh ds    1
g_prevy  ds    1
g_ac_xl  ds    1          ;Center-point coordinates
g_ac_xh  ds    1          ; for array-based line
g_ac_y   ds    1          ; draw (&AT, &PLOT).
g_out_x  ds    2          ;16-bit coordinates for
g_out_y  ds    2          ; array-based line draw



         lst   on
end      equ   *
         sav   amperfdraw
         lst   off
