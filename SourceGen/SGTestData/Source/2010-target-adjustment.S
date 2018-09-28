; Copyright 2018 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

load11  lda     #$11                    ;EDIT: set label=load11
load22  ldx     #$22
load33  ldy     #$33                    ;EDIT: set label=load33
load44  lda     #$44
predat  bra     skipdat                 ;EDIT: set label=predat

dat0    dw      $0123
dat1    dw      $4567                   ;EDIT: set label=dat1
dat2    dw      $89ab
dat3    dw      $cdef
dat4    dw      $0011
dat5    dw      $2233
        dfb     $80
        asc     'The quick brown fox'
        dfb     $80
        dw      skipdat                 ;EDIT: format these as addresses
        dw      skipdat-1
        dw      skipdat+1
fill0   ds      4                       ;EDIT: set label=fill0
fill1   ds      4
fill2   ds      4
fill3   ds      4
postfill dfb    $80

skipdat                                 ;EDIT: set label=skipdat
        lda     dat0                    ;this should get mapped to predat+2
        lda     postfill

        asl     dat1                    ;these should coalesce to dat1
        rol     dat2
        ror     dat3                    ;these should be auto-labeled
        and     dat4
        ora     dat5

        lda     fill0                   ;EDIT: set all labels to fill0
        sta     fill1                   ; and confirm that auto-labels vanish
        lda     fill2                   ; and we get a single 16-byte .fill
        sta     fill3

        jsr     load22                  ;should get auto label
        lda     load44                  ;should get auto label

; PEA/PER may want to push addr-1; make sure that works
caddr
        pea     caddr-1                 ;should base off caddr
        per     caddr-1                 ;should base off caddr
        lda     caddr+1                 ;should base off caddr
        lda     caddr+2                 ;should base off caddr

; self-referential, self-modifying code (edge case for xrefs)
        lda     #$ea
srop1   sta     srop1
srop2   sta     srop2+1
srop3   sta     srop3+2                 ;EDIT: set operand to non-existent symbol
        brl     skipdat1

        dfb     $80
dat81   dfb     $81                     ;EDIT: set label dat81

        org     $2000
dat82   dfb     $82
        dfb     $83

skipdat1
        bit     skipdat1
        lda     dat81
        lda     dat82                   ;this should NOT use dat81
        rts
