         .setcpu "65816"
         .org    $1000
         .a8
         .i8
         nop
L1001:   bit     L1001
caddr:   pea     caddr-1
         per     caddr-1
         lda     L1001+2
         lda     caddr
         lda     caddr+1
         lda     caddr+2
         rts

