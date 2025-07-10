; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; This is a reduced version of the 65816-based test.
;
; Assembler: Merlin 32

absl    equ     $1029
absh    equ     $feed
zip     equ     $cd

        org     $2345

start                               ;set this label
; load 8/16-bit symbols in different ways
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

        lda     zip+1
        lda:    zip+1
        lda     absh-1
        lda     absh+1
        lda     start+1
        lda     start-1

        jmp     :skipdata

; reference 16/24-bit symbol in data statements
        dfb     zip
        dfb     <absh
        dfb     >absh
        dfb     <start
        dfb     >start

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
        dw      <start+1
        dw      >start+1

        dfb     >absh               ;format as 16-bit big-endian
        dfb     <absh

        adr    zip
        adr    <absh
        adr    >absh
        adr    <start
        adr    >start

:skipdata
        jmp     nextchunk

        org     $1000
nextchunk
        nop                         ;leave this as auto-label
        nop                         ;label this L1000
        nop                         ;label this L1000_0

plataddr equ    $3000       ;address only in platform file
projover equ    $3100       ;replaced by proj symbol w/same name, diff addr (no match)
projalso equ    $3200       ;also in project, twice w/different name +/- alpha; low should win
nosym   equ     $3300       ;should not match anything

        lda     plataddr
        lda     projover
        lda     projalso
        lda     nosym

        jmp     next

target0 nop
target1 nop                 ;point everything here
target2 nop


t0      jsr     target0
        jsr     target1
        jsr     target2
t1a     jmp     target0
t1b     jmp     target1
t1c     jmp     target2
t3a     jmp     target0
t3b     jmp     target1
t3c     jmp     target2

next
        jsr     t0
        jsr     t1a
        jsr     t1b
        jsr     t1c
        jsr     t3a
        jsr     t3b
        jsr     t3c

        jmp     :skiphex

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
        adc     #$30            ;set to BMI to test constant vs. instr
        lda     <target0&$ff
        lda     <target2&$ff
        lda     <target0&$ff,x
        lda     (<target2+3&$ff),y
        rts
