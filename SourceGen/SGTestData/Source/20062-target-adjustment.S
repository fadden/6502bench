; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

start   nop

; PEA/PER may want to push code addr-1; make sure that works.  This
; is different from the way LDA works on the same address.
pre_addr
        bit     pre_addr
caddr
        pea     caddr-1                 ;should base off caddr
        per     caddr-1                 ;should base off caddr
        lda     caddr-1                 ;should base off pre_addr
        lda     caddr                   ;should base off caddr
        lda     caddr+1                 ;should base off caddr
        lda     caddr+2                 ;should base off caddr

        rts
