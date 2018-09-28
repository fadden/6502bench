;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
;
; cda.asm - HardPressed classic desk accessory
; Copyright (C) 1993 by Andy McFadden
;
; This is an example of communication with HardPressed.  It illustrates
; getting and setting the INIT's status with SendRequest.  The calls used
; here are the ONLY ones which are guaranteed to exist in future versions
; of HardPressed.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

           CASE  ON
           OBJCASE ON


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
Header     START
;
; System-required header for the CDA.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           dc    I1'str_end-str_begin'
str_begin  anop
           dc    C'HardPressed Control'
str_end    anop

           dc    A4'Main'
           dc    A4'ShutDown'

           dc    C'Copyright (C) 1993 by Andy McFadden',H'00'
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
Globals    DATA
;
; Program-wide defs.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

;
; DP defs
;
ptr        gequ  $00                      ;4b
ptr2       gequ  $04                      ;4b

;
; Softswitches
;
CLR80VID   equ   $e0c00c
SETALTCHR  equ   $e0c00f

;
; Global variables
;
global_mode ds   2
global_flags ds  2

; CDA display status
dMaxOpt    equ   2
hilite_opt ds    2
stat_tab   anop
stat1      ds    2
stat2      ds    2
max_tab    anop
max1       dc    I2'3'                    ;3 states for item 1
max2       dc    I2'2'                    ;2 here

;
; reasons for failure
;
failP8     gequ  1
failInactive gequ 2
failBusy   gequ  3

;
; Text screen line offsets
;
texttab    ANOP
           dc    I2'$400,$480,$500,$580,$600,$680,$700,$780'
           dc    I2'$428,$4a8,$528,$5a8,$628,$6a8,$728,$7a8'
           dc    I2'$450,$4d0,$550,$5d0,$650,$6d0,$750,$7d0'
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
Main       START
;
; Calling conventions:
;   (called by control panel)
;
; Stack is on page 1, DP is on page 0.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           using MesgData
           using Globals

           phb
           phk
           plb

; see if GS/OS is active
           lda   >$e100bc                 ;OS_KIND
           and   #$00ff
           cmp   #$0001                   ;GS/OS
           beq   is_gsos
           lda   #failP8
           jmp   ErrorMain
is_gsos    anop

; see if HardPressed is active
           lda   #dMping
           ldx   #$0000                   ;no data_in
           txy
           jsr   SendMsg                  ;leaves version in data_out+$02
           bcc   hp_active                ;if no error, HP is up
;           cmp   #$0120                   ;reqNotAccepted
;           beq   nohear                   ;else probably $0122 (invalidReq)

inactive   anop
           lda   #failInactive
           jmp   ErrorMain
hp_busy    anop
           lda   #failBusy
           jmp   ErrorMain

hp_active  anop
           lda   #dMgetStatus
           ldx   #$0000                   ;no data_in
           txy
           jsr   SendMsg                  ;failure here probably means that
           bcs   hp_busy                  ; HP is busy (must be _AlertWindow)

;
; From here on, assume HardPressed is active and functioning normally.
;
           lda   data_out+2               ;split the mode and the flags into
           tax                            ; two parts
           and   #$00ff
           sta   global_mode
           txa
           and   #$ff00
           sta   global_flags

           lda   #$0000
           jsr   InitScreen               ;set 40 cols, clear, draw border

; init menu stuff
           stz   hilite_opt

           ldx   #$0000
           lda   global_mode
           cmp   #dVpolOn
           beq   got_pol
           inx
           cmp   #dVpolDecode
           beq   got_pol
           inx
got_pol    anop
           stx   stat1

           ldx   #$0000
           lda   global_flags
           bit   #fGverify
           beq   got_ver
           inx
got_ver    anop
           stx   stat2

;
; main loop
;
redraw_loop ANOP
           jsr   DrawScreen

key_loop   ANOP
           jsr   GetKey

           cmp   #$000d                   ;return?
           beq   save_status
           cmp   #$001b                   ;escape?
           bne   not_esc
           brl   escape_hit
not_esc    anop

           cmp   #$000a                   ;Ctrl-J (down arrow)?
           beq   dn
           cmp   #$000b                   ;Ctrl-K (up arrow)?
           beq   up
           cmp   #$0008                   ;Ctrl-H (left arrow)?
           beq   left
           cmp   #$0015                   ;Ctrl-U (right arrow)?
           beq   right

           pea   $0008                    ;sbBadKeypress
           ldx   #$3803                   ;_SysBeep2
           jsl   $e10000
           bra   key_loop

dn         anop
           lda   hilite_opt
           inc   A
           sta   hilite_opt
           cmp   #dMaxOpt
           blt   redraw_loop
           stz   hilite_opt
           bra   redraw_loop

up         anop
           lda   hilite_opt
           dec   A
           bpl   upstore
           lda   #dMaxOpt-1
upstore    anop
           sta   hilite_opt
           bra   redraw_loop

right      anop
           lda   hilite_opt
           asl   A
           tax
           lda   stat_tab,x
           inc   A
           cmp   max_tab,x
           blt   rightstore
           lda   #$0000
rightstore anop
           sta   stat_tab,x
           bra   redraw_loop

left       anop
           lda   hilite_opt
           asl   A
           tax
           lda   stat_tab,x
           dec   A
           bpl   leftstore
           lda   max_tab,x
           dec   A
leftstore  anop
           sta   stat_tab,x
           bra   redraw_loop

;
; If return was hit, send the new status to the INIT
;
save_status ANOP
           ldx   #dVpolOn                 ;translate the menu index into
           lda   stat1                    ; the HP state value
           beq   got_opol
           ldx   #dVpolDecode
           cmp   #$0001
           beq   got_opol
           ldx   #dVpolOff
got_opol   anop
           stx   global_mode

; set/clear the "verify" flag without disturbing any of the others
           lda   #fGverify                ;verify flag
           ldx   stat2                    ;is verify on?
           beq   v_off
           tsb   global_flags             ;enable verify
           bra   v_set
v_off      anop
           trb   global_flags             ;disable verify
v_set      anop

           lda   global_mode              ;now put it together with the mode
           ora   global_flags
           tay                            ;lo word is status
           ldx   #$0000                   ;hi word is zero
           lda   #dMsetStatus
           jsr   SendMsg
           bcc   Done
           pea   $000c                    ;sbOperationFailed
           ldx   #$3803                   ;_SysBeep2
           jsl   $e10000
           bra   Done

; if escape was hit, just exit
escape_hit ANOP

Done       ANOP
           plb
           rtl
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
ErrorMain  START
;
; Calling conventions:
;   JMP from Main with reason for failure in Acc
;
; Displays a screen which tells the user why he/she/it can't use the CDA at
; this time.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           using Globals

           sta   fail_cause

           lda   #$0001
           jsr   InitScreen

;
; Just draw the message from here
;
           phb
           pea   $e0e0                    ;write to text page in bank $e0
           plb
           plb

           ldx   #11*2
           lda   >texttab,x
           clc
           adc   #3                       ;start at column 4
           sta   <ptr

           lda   >fail_cause
           cmp   #failP8
           bne   fail2
fail1      anop
           lda   #fail_p8
           bra   fail_comm
fail2      anop
           cmp   #failInactive
           bne   fail3
           lda   #fail_hp
           bra   fail_comm
fail3      anop
           lda   #fail_busy
fail_comm  anop
           sta   <ptr2
           lda   #fail_msg|-16
           sta   <ptr2+2
           sep   #$20
           LONGA OFF
           ldy   #$0000
stat_loop  anop
           lda   [<ptr2],y
           beq   stat_done
           sta   (ptr),y
           iny
           bra   stat_loop
stat_done  anop
           rep   #$20
           LONGA ON

           plb

;
; Wait for keypress, then bail
;
key_loop   anop
           jsr   GetKey

           cmp   #$0020                   ;space? (also error result)
           beq   Done
           cmp   #$000d                   ;return?
           beq   Done
           cmp   #$001b                   ;escape?
           bne   key_loop

Done       ANOP
           plb
           rtl

fail_cause ds    2
fail_msg   anop
           MSB   ON
fail_p8    dc    C"  Not accessible from ProDOS 8",H'00'
fail_hp    dc    C"   HardPressed is not loaded",H'00'
fail_busy  dc    C" HardPressed is busy doing stuff",H'00'
           MSB   OFF
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
ShutDown   START
;
; (does nothing useful)
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           rtl
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
GetKey     START
;
; Calling conventions:
;   JSR
;   Returns with key hit in Acc
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

           pha
           ldx   #$0606                   ;_EMStatus
           jsl   $e10000
           pla
           beq   inactive

; EM is active, so use it
active     ANOP
           pea   $0000
           pea   $0028                    ;keyDown and autoKey events only
           pea   event_rec|-16
           pea   event_rec
           ldx   #$0a06                   ;_GetNextEvent
           jsl   $e10000
           pla                            ;boolean handleEventFlag
           bcs   Fail
           beq   active                   ;flag was 0, skip it

           lda   message
           bra   Done

; event manager not active, just poll the keyboard
inactive   ANOP
           sep   #$20
           LONGA OFF
           sta   >$e0c010
lp         lda   >$e0c000
           bpl   lp
           sta   >$e0c010
           rep   #$20
           LONGA ON
           and   #$007f

Done       ANOP
           rts
Fail       ANOP
           lda   #$0020                   ;call it a space bar
           rts

event_rec  anop
what       ds    2
message    ds    4
when       ds    4
where      ds    4
modifiers  ds    2
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
InitScreen START
;
; Calling conventions:
;   JSR with 0 or 1 in acc (indicating normal or error start)
;
; Sets 40 columns and mousetext, then draws the title, border, and key
; instructions.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           using Globals

           sta   err_flag

           phb
           pea   $e0e0                    ;write to text page in bank $e0
           plb
           plb

           sep   #$20
           LONGA OFF
           sta   |CLR80VID                ;set 40 columns
           sta   |SETALTCHR               ;we want MouseText
           rep   #$20
           LONGA ON

home       ANOP
           ldx   #23*2
loop       anop
           lda   >texttab,x
           sta   <ptr
           ldy   #38
loop2      anop
           lda   #$a0a0
           sta   (ptr),y
           dey
           dey
           bpl   loop2

           dex
           dex
           bpl   loop

title      ANOP
           lda   >texttab                 ;addr of line 0 ($400, duh)
           sta   ptr
           ldx   #2*2
           lda   >texttab,x
           sta   ptr2
           ldy   #37                      ;two at a time, starting from 37/38
tloop1     anop
           lda   #$dfdf                   ;normal '_'
           sta   (ptr),y
           lda   #$4c4c                   ;flashing 'L'
           sta   (ptr2),y
           dey
           dey
           bpl   tloop1                   ;should end at -1

           ldx   #1*2
           lda   >texttab,x
           sta   ptr
           lda   #$a05a                   ;'Z ' (right-bar, space)
           sta   (ptr)
;           lda   #$a041                   ;'A ' (apple symbol, space)
;           ldy   #$0002
;           sta   (ptr),y

           sep   #$20
           LONGA OFF
           ldy   #$0002
           ldx   #$0000
tloop2     anop
           lda   >title_str,x
           beq   tloop2_done
           sta   (ptr),y
           inx
           iny
           bra   tloop2
tloop2_done anop
           lda   #$a0
           sta   (ptr),y
           iny

           lda   #$20
tloop3     anop
           cpy   #39
           bge   tloop3_done
           sta   (ptr),y
           iny
           bra   tloop3
tloop3_done anop
           lda   #$5f                     ;left-bar
           sta   (ptr),y
           rep   #$20
           LONGA ON


box        ANOP
           ldx   #2*2
edge       anop
           lda   >texttab,x
           sta   <ptr
           sep   #$20
           LONGA OFF
           lda   #$5a
           sta   (ptr)
           lda   #$5f
           ldy   #39
           sta   (ptr),y
           rep   #$20
           LONGA ON
           inx
           inx
           cpx   #23*2
           blt   edge

bottom     anop
           ldx   #23*2
           lda   >texttab,x
           sta   ptr
           ldy   #37
bloop      anop
           lda   #$4c4c                   ;flashing 'L' - upper line
           sta   (ptr),y
           dey
           dey
           bpl   bloop                    ;should end at -1

text       ANOP
           ldx   #22*2
           lda   >texttab,x
           sta   ptr

           ldy   #$0002
           ldx   #$0000
           lda   >err_flag
           beq   noerr
           ldx   #finstr_str-instr_str
noerr      anop
           sep   #$20
           LONGA OFF
instr_loop anop
           lda   >instr_str,x
           beq   instr_done
           sta   (ptr),y
           iny
           inx
           bra   instr_loop
instr_done anop
           rep   #$20
           LONGA ON


Done       ANOP
           plb
           rts

err_flag   ds    2

msg_tab    anop
           MSB   ON
title_str  dc    C'HardPressed Control',H'00'
instr_str  dc    C'Select: ',H'48a055a04aa04b'
           dc    C'  Cancel:Esc  Save: ',H'4d00'
finstr_str dc    C'                            Exit: ',H'4d00'
           MSB   OFF
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
DrawScreen START
;
; Calling conventions:
;   JSR
;
; Draws the menu items (selected one in inverse), the appropriate selection,
; and a check mark for defaults.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           using Globals

;
; Initialize counters
;
           lda   #4
           sta   cur_line
           lda   #0
           sta   cur_item
           lda   #$00e0
           sta   <ptr2+2

item_loop  ANOP
           lda   cur_line                 ;set text ptr
           asl   A
           tax
           lda   texttab,x
           sta   <ptr2

           ldy   #2                       ;start drawing at column 2
           lda   cur_item
           asl   A
           tax
           lda   items,x
           bne   _cont1
           brl   Done
_cont1     anop
           sta   <ptr
           sta   item_ptr
           lda   (<ptr)                   ;get pointer to title
           sta   <ptr
           lda   stat_tab,x
           sta   item_stat
           bne   not_default

           lda   #$44                     ;check mark
           bra   def_comm
not_default anop
           lda   #$a0                     ;space
def_comm   anop
           sep   #$20
           LONGA OFF
           sta   [<ptr2],y
           rep   #$20
           LONGA ON

           lda   #$00ff
           sta   cmask
           stz   csub
           lda   hilite_opt
           cmp   cur_item
           bne   not_hilite
           lda   #$007f
           sta   cmask
           lda   #$0040
           sta   csub
not_hilite anop

           iny
           iny
           stz   <ptr+2                   ;temp
           sep   #$20
           LONGA OFF
title_loop anop
           tyx
           ldy   <ptr+2
           lda   (<ptr),y
           beq   title_done
           iny
           sty   <ptr+2
           txy
           and   cmask                    ;make it inverse if this is current
           cmp   #$60                     ;if it's > $60, then it's inverse
           bge   islorn                   ; lower case or normal text
           cmp   #$40                     ;if it's < $40, it's inverse
           blt   islorn                   ; punctuation (e.g. ':')
           sec
           sbc   csub
islorn     anop
           sta   [<ptr2],y
           iny
           bra   title_loop
title_done anop
           rep   #$20
           LONGA ON
           txy
           iny                            ;space between title and status

           lda   item_stat
           inc   A
           asl   A
           clc
           adc   item_ptr
           sta   <ptr
           lda   (<ptr)
           sta   <ptr

           stz   <ptr+2
           sep   #$20
           LONGA OFF
stat_loop  anop
           tyx
           ldy   <ptr+2
           lda   (<ptr),y
           beq   stat_done
           iny
           sty   <ptr+2
           txy
           sta   [<ptr2],y
           iny
           bra   stat_loop
stat_done  anop
           rep   #$20
           LONGA ON
           txy

;
; increment counters and branch
;
           inc   cur_item
           inc   cur_line
           inc   cur_line
           brl   item_loop

Done       ANOP
           rts

; counters and temp vars
cur_line   ds    2
cur_item   ds    2
item_stat  ds    2
item_ptr   ds    2
cmask      ds    2
csub       ds    2

;
; Definitions for CDA items
;
items      anop
           dc    A2'item1,item2,0'

item1      anop
           dc    A2'title1'
           dc    A2'i1compr,i1decomp,i1inact,0'
           MSB   ON
title1     dc    C'Status:',H'00'
i1compr    dc    C'Compress and Expand    ',H'00'
i1decomp   dc    C'Expand only            ',H'00'
i1inact    dc    C'Inactive               ',H'00'
           MSB   OFF

item2      anop
           dc    A2'title2'
           dc    A2'i2off,i2on,0'
           MSB   ON
title2     dc    C'Verify:',H'00'
i2off      dc    C'Off',H'00'
i2on       dc    C'On ',H'00'
           MSB   OFF
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
MesgData   DATA
;
; IPC stuff
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;

idstring   anop
           dc    I1'idend-idbeg'
idbeg      dc    C'WestCode~HardPressed~Main~'
idend      anop

data_in    anop
           ds    2                        ;room for status word
data_out   anop
           ds    2                        ;recipient count (set by system)
           ds    4                        ;room for version # or status info
           ds    16                       ;in case HP starts returning more

; calls that non-WestCode programs may make:
dMping      equ  $8000                    ;return HP version number (4 bytes)
dMsetStatus equ  $8001                    ;set global status (on/off/decode)
dMgetStatus equ  $8002                    ;get global status

; low byte of status word (choose exactly one):
dVpolDecode equ  $0001                    ;Expand only
dVpolOn    equ   $0002                    ;Compress and Expand
dVpolOff   equ   $0003                    ;Inactive

; high byte of status word (these are flags; OR them together):
; (other flags may be added later; make sure you copy the existing values)
fGverify   equ   $4000                    ;do verify on encoded close
fGoneTemp  equ   $2000                    ;use a single temp directory
fGmouseTrix equ  $1000                    ;show progress by changing the mouse
           END


;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
SendMsg    START
;
; Calling conventions:
;   JSR with message in Acc and data_in in X/Y (hi/lo)
;   Returns with acc/carry as set by _SendRequest
;
; Sends a message to HP.
;
;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
           using MesgData

           pha                            ;request code
           pea   $8001                    ;send to one, by name
           pea   idstring|-16
           pea   idstring
           phx                            ;push data_in on
           phy
           pea   data_out|-16             ;data_out is always here
           pea   data_out
           ldx   #$1c01
           jsl   $e10000                  ;_SendRequest
           rts
           END

