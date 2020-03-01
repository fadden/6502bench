; Copyright 2020 faddenSoft. All Rights Reserved.
; See the LICENSE.txt file for distribution terms (Apache 2.0).
;
; Assembler: Merlin 32

        org     $1000

        lda     vertices
        lda     edges
        lda     faces
        rts

; List of vertices (X,Y,Z).
vertices
        dfb     -32,32,32       ;0
        dfb     -32,-32,32      ;1
        dfb     32,-32,32       ;2
        dfb     32,32,32        ;3
        dfb     -32,32,-32      ;4
        dfb     -32,-32,-32     ;5
        dfb     32,-32,-32      ;6
        dfb     32,32,-32       ;7

; put a decoration on the front face; should look like a '7'
        dfb     -20,-20,32      ;8
        dfb     20,20,32        ;9
        dfb     10,20,32        ;10
        dfb     $80

; List of edges (vertex0, vertex1, face0, face1).
edges
        dfb     0,1, 0,1        ;0
        dfb     1,2, 0,3        ;1
        dfb     2,3, 0,4        ;2
        dfb     3,0, 0,2        ;3
        dfb     4,5, 1,5        ;4
        dfb     5,6, 1,3        ;5
        dfb     6,7, 1,4        ;6
        dfb     7,4, 1,2        ;7
        dfb     0,4, 2,5        ;8
        dfb     1,5, 3,5        ;9
        dfb     2,6, 3,4        ;10
        dfb     3,7, 2,4        ;11

        dfb     8,9, 0,0        ;12
        dfb     9,10,0,0        ;13
        dfb     $80

; List of faces (surface normal X,Y,Z).
faces
        dfb     0,0,1           ;0 front
        dfb     0,0,-1          ;1 back
        dfb     0,1,0           ;2 top
        dfb     0,-1,0          ;3 bottom
        dfb     1,0,0           ;4 right
        dfb     -1,0,0          ;5 left
        dfb     $80

