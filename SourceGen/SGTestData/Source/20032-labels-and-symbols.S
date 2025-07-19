; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

absl    equ     $1029
absh    equ     $feed
zip     equ     $cd
biggie  equ     $123456
thirty2 equ     $12345678

        org     $012345

start                               ;set this label
        clc
        xce

; load 8/16/24-bit symbols in different ways
        sep     #$30
        mx      %11
        lda     #zip
        lda     #zip+16
        lda     #zip+64

        lda     #<absh
        lda     #>absh
        lda     #<absh+64
        lda     #>absh+64

        lda     #<absl
        lda     #>absl
        lda     #<absl-64
        lda     #>absl-64

        lda     #<start
        lda     #>start
        lda     #^start

        pea     absh                ;leave as default
        pea     start
        pea     ^start
        pea     biggie
        pea     ^biggie
        pea     absh                ;convert to symbol
        pea     start
        pea     ^start
        pea     biggie
        pea     ^biggie

        lda     zip+1
        lda:    zip+1
        ldal    zip+1
        lda     absh-1
        ldal    absh-1
        lda     absh+1
        ldal    absh+1
        lda     start+1
        ldal    start+1
        lda     start-1
        ldal    start-1
        lda     biggie+1
        ldal    biggie+1
        lda     biggie-1
        ldal    biggie-1

        rep     #$30
        mx      %00
        lda     #zip
        lda     #zip+16
        lda     #zip+64

        lda     #<absl
        lda     #>absl
        lda     #<absl-$1000
        lda     #>absl-$1000
        lda     #^absl-$1000

        lda     #<absh
        lda     #>absh
        lda     #<absh+$1000
        lda     #>absh+$1000
        lda     #^absh+$1000

        lda     #<start
        lda     #>start
        lda     #^start

        lda     #<biggie            ;note: constant, not a label
        lda     #>biggie
        lda     #^biggie

        bra     :skipdata

; reference 16/24-bit symbol in data statements
        dfb     zip
        dfb     <absh
        dfb     >absh
        dfb     <start
        dfb     >start
        dfb     ^start

        dw      zip
        dw      <absl
        dw      >absl
        dw      <absl-$1000
        dw      >absl-$1000
        dw      <absh
        dw      >absh
        dw      <absh+$1000
        dw      >absh+$1000
        dw      <start
        dw      >start
        dw      ^start
        dw      <start+1
        dw      >start+1
        dw      ^start+1

        dfb     >absh               ;format as 16-bit big-endian
        dfb     <absh

        adr    zip
        adr    <absh
        adr    >absh
        adr    <start
        adr    >start
        adr    ^start

        adrl    zip
        adrl    <absh
        adrl    >absh
        adrl    <start-1
        adrl    >start-1
        adrl    ^start-1

:skipdata
        lda     #$11                ;format as biggie
        mvn     biggie,start        ;format w/symbols
        mvp     start,biggie
        mvn     biggie,start        ;format as decimal
        mvp     start,biggie        ;format as binary
        per     :skipdata
        brl     :forward
:forward jml    nextchunk

        org     $1000
nextchunk
        nop                         ;leave this as auto-label
        nop                         ;label this L1000
        nop                         ;label this L1000_0
        sep     #$30
        mx      %11

plataddr equ    $3000       ;address only in platform file
projover equ    $3100       ;replaced by proj symbol w/same name, diff addr (no match)
projalso equ    $3200       ;also in project, twice w/different name +/- alpha; low should win
nosym   equ     $3300       ;should not match anything

        lda     plataddr
        lda     projover
        lda     projalso
        lda     nosym

        bra     :next

target0 nop
target1 nop                 ;point everything here
target2 nop


t0      per     target0
        per     target1
        per     target2
        jsr     target0
        jsr     target1
        jsr     target2
t1a     bra     target0
t1b     bra     target1
t1c     bra     target2
t2a     brl     target0
t2b     brl     target1
t2c     brl     target2
t3a     jmp     target0
t3b     jmp     target1
t3c     jmp     target2
t4a     jml     target0
t4b     jml     target1
t4c     jml     target2

:next
        jsr     t0
        jsr     t1a
        jsr     t1b
        jsr     t1c
        jsr     t2a
        jsr     t2b
        jsr     t2c
        jsr     t3a
        jsr     t3b
        jsr     t3c
        jsr     t4a
        jsr     t4b
        jsr     t4c

        brl     :skiphex

        hex     808182838485868788898a8b8c8d8e8f        ;add label and comment
        hex     808182838485868788898a8b8c8d8e8f
        hex     808182838485868788898a8b8c8d8e8f
        hex     808182838485868788898a8b8c8d8e8f
        hex     808182838485868788898a8b8c8d8e8f
        hex     808182838485868788898a8b8c8d8e8f
        hex     808182838485868788898a8b8c8d8e8f

        asc     'This is a long string. Put a label and comment '
        asc     'on it to confirm that the label and comment only '
        asc     'appear on the first line.  The quick brown fox '
        asc     'jumps over the lazy dogs.'

:skiphex

; extract bytes from 32-bit value with short regs
        lda     #<thirty2 + 2
        lda     #>thirty2 + 768
        lda     #^thirty2
        rep     #$30
        mx      %00
        lda     #<thirty2 + 3
        lda     #>thirty2 + 1024
        lda     #^thirty2

; test direct-page access with a non-DP label, forward and backward, with adjustments
before  nop

        lda     before
        lda     <before&$ff

        lda     <before+1&$ff
        lda     (<before+2&$ff)
        lda     (<before+3&$ff),y
        lda     [<before+4&$ff],y
        lda     [<before+5&$ff]
        lda     <before+6&$ff,x
        dfb     $a1,$a6             ;lda     (<before+7&$ff,x)
        lda     <before+8&$ff,y

        lda     <after&$ff
        lda     (<after+1&$ff),y
        ldx     <after+2&$ff,y
        ldy     <after+3&$ff,x
        dfb     $d4,$c4             ;pei     (<zip+4&$ff)
after   nop
        jml     checkpea

; Dig into PEA, using both internal and external addresses.  We want to do
; this outside bank 0 to see if the bank byte creeps in.
        org     $021000
checkpea nop
ptest   pea     ptest               ;don't format
        pea     ptest               ;explicitly format as hex
        pea     ptest               ;format as symbol

        pea     biggie              ;don't format
        pea     ^biggie
        pea     biggie              ;explicitly format as hex
        pea     ^biggie
        pea     biggie              ;format as symbol (low+bank)
        pea     ^biggie
        jmp     btest

; Check some in-bank and cross-bank internal/external operands.
        org     $020010
testlow dfb     $02

        org     $0010
testzp  dfb     $00

        org     $022000
btest   nop
        phk
        plb

        ldal    biggie,x            ;don't format
        jsl     biggie
        jsr     biggie
        lda     ptest,x
        jsr     ptest
        lda:    testlow,x
        jsr     testlow
        lda     testzp,x
        nop
        ldal    biggie,x            ;format as symbol
        jsl     biggie
        jsr     biggie
        lda     ptest,x
        jsr     ptest
        lda:    testlow,x
        jsr     testlow
        lda     testzp,x
        nop
        ldal    biggie,x            ;explicitly format as hex
        jsl     biggie
        jsr     biggie
        lda     ptest,x
        jsr     ptest
        lda:    testlow,x
        jsr     testlow
        lda     testzp,x
        nop
        sep     #$30
        mx      %11
        lda     #$e0
        pha
        plb
        ldal    biggie,x            ;explicitly format as decimal
        jsl     biggie
        jsr     biggie
        lda     ptest,x
        jsr     ptest
        lda:    testlow,x
        jsr     testlow
        lda     testzp,x

        rts
