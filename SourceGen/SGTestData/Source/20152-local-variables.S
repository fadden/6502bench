; Copyright 2019 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32
; 65816 version

        org     $1000

; Define these as project symbols.
PROJ_ZERO equ   $00
PROJ_ONE equ    $01
CONST_ZERO equ  $f0
CONST_ONE equ   $f1
PTR_2   equ     $f8             ;used to test uniqifier later on

        ldy     PROJ_ZERO
        lda     (PROJ_ONE),y
        sta     $03             ;undefined yet
        ldx     $04             ;undefined yet
        lda     CONST_ZERO,S    ;explicitly set symbol
        sta     CONST_ONE,S     ;leave as default (hex)

; TABLE - test redefinition of project symbol values
]VAR_ZERO equ   $00             ;2b
]VAR_TWO  equ   $02             ;1b
]VAR_THREE equ  $03             ;1b
]CONST_ZERO equ $f0             ;1b const

        ldy     PROJ_ZERO       ;should become ]VAR_ZERO
        lda     (PROJ_ONE),y    ;should become ]VAR_ZERO+1
        sta     $03             ;should become ]VAR_THREE
        ldx     $04             ;undefined yet
        lda     CONST_ZERO,S    ;should become ]CONST_ZERO
        sta     CONST_ONE,S     ;leave as default (hex)

        eor     $00             ;explicitly format as decimal
        ora     $f0,S           ;explicitly format as decimal

; TABLE - test redefinition of symbol labels
]PROJ_ZERO equ  $10             ;EDIT: rename to PROJ_ZERO
]DPCODE equ     $80             ;EDIT: rename to DPCODE

        lda     $00
        lda     $01
        lda     $02
        lda     $03
        lda     $04
        lda     $10
        lda     $11
        lda     $80

; TABLE/CLEAR - empty table
        ldx     $00
        ldx     $01
        ldx     $02

; confirm that hidden tables are skipped over
        dfb     $2c
; TABLE (hidden)
]HIDDEN0 equ    $00
]HIDDEN1 equ    $01
        lda     #$ff
        ldy     $00
        ldy     $01
        ldy     $02

        dfb     $2c
; TABLE (not hidden)
]NH0    equ     $00
]NH1    equ     $01
:label  lda     #$fe
        beq     :label
        ldy     $00
        ldy     $01
        ldy     $02

        nop

; TABLE
]PTR0   equ     $10             ;2b
]CONST0 equ     $10             ;4b
        lda     ]PTR0
        ldx     ]PTR0+1
        ldy     ]PTR0+2         ;should be hex
        lda     (]CONST0,S),y
        sta     (]CONST0+3,S),y

; test redefinition of name

; TABLE
]PTR    equ     $20             ;2b
        ldx     ]PTR
; TABLE
]PTR    equ     $22             ;2b
        ldx     ]PTR
; TABLE
]PTR    equ     $24             ;2b
        ldx     ]PTR

; define user label to try to trip up the uniqifier
PTR_1   nop


; test redefinition of value
; TABLE/CLEAR
]PTR_A  equ     $20             ;2b
        ldy     ]PTR_A          ;PTR_A
]PTR_B  equ     $1f             ;2b
        ldy     ]PTR_A          ;PTR_B+1
]PTR_C  equ     $1d             ;4b
        ldy     ]PTR_A          ;PTR_C+3
]PTR_D  equ     $21             ;1b
        ldy     ]PTR_A          ;should come up as hex

; TABLE
]VAL0   equ     $30             ;1b
]VAL1   equ     $31             ;1b
]VAL2   equ     $32             ;1b
]VAL3   equ     $33             ;1b
]VAL4   equ     $34             ;1b
]VAL5   equ     $35             ;1b
        and     ]VAL0
        and     ]VAL1
        and     ]VAL2
        and     ]VAL3
        and     ]VAL4
        and     ]VAL5
; TABLE
]VAL14  equ     $31             ;4b
        and     ]VAL0
        and     ]VAL1           ;these four become ]VAL14
        and     ]VAL2
        and     ]VAL3
        and     ]VAL4
        and     ]VAL5


; TABLE - test direct page label vs. variable
]DPNOP   equ     $80
        lda     ]DPNOP
        jsr     DPCODE
        rts

        org     $0080
DPCODE  nop
        lda     DPCODE          ;should be DPNOP
        lda     |DPCODE         ;should be DPCODE
        lda     >DPCODE         ;should be DPCODE

; Local label test.  ca65 v2.18 erases cheap local label scope when it
; encounters a constant or .set.
LOCAL1  lda     #$2c            ;EDIT: format as ASCII
        ldx     $1234           ;put variable table here with one arbitrary entry
        beq     LOCAL1
LOCAL2  lda     $2c             ;EDIT: format as ASCII
        ldx     $5678           ;put empty variable table here
        beq     LOCAL2

; Test C-style call frame (issue #96).
        nop
        clc
        xce
        rep     #$30
        pea     ^STRING
        pea     STRING
        jsl     STRLEN
        sec
        xce

        jmp     AFTER_STRLEN

STRING  asc     'testing',00

; C sample, for something like:

; int strlen(const char* arg) {
;    len = 0;
;    while (*arg++ != '\0')
;        len++;
;    return len;
; }
;
; Assume SP at $2000 before call.  On entry:
;  $2000  string-$00
;  $1fff  string-bank
;  $1ffe  string-hi
;  $1ffd  string-lo
;  $1ffc  ret-bank
;  $1ffb  ret-hi
;  $1ffa  ret-lo
;  S=$1ff9
;
; After call frame set up:
;  $1ff9  saved-D-hi
;  $1ff8  saved-D-lo
; S=$1ff7 before SBC/ADC
;  $1ff7  (ptr-$00)
;  $1ff6  (ptr-bank)
;  $1ff5  (ptr-hi)
;  $1ff4  (ptr-lo)
;  $1ff3  (len-hi)
;  $1ff2  (len-lo)
;
; D=$1f00
; S=$1ff1
;
]LEN    equ     $f2             ;2 bytes
]PTR    equ     $f4             ;4 bytes
]ARG    equ     $fd             ;4 bytes; spans [$fd,$100]

        mx      %00
STRLEN  phd
        tsc
        sec
        sbc     #$00f7
        tcd
        adc     #$00f0          ;note carry set
        tcs
        stz     ]LEN
:Loop   ldx     ]ARG+2
        lda     ]ARG
        inc     ]ARG
        bne     :NoInc
        inc     ]ARG+2
:NoInc  sta     ]PTR
        stx     ]PTR+2
        lda     []PTR]
        and     #$00ff
        beq     :Ret
        inc     ]LEN
        bra     :Loop
:Ret    lda     ]LEN
        tay
        tdc
        clc
        adc     #$00f7
        tcs
        tya
        pld
        rtl

AFTER_STRLEN
        bit     $ff
        bit:    $00ff
        bit     $0100
        rts
