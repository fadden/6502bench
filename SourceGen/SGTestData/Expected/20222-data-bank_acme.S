;ACME can't handle 65816 code that lives outside bank zero
*       =       $0000
        !pseudopc $1000 {
        !hex    18fbe230ad0010ad00204babad0010ad0020a90248abad0010ad0020a208aa48
        !hex    ab2200200222004002a90248ab220030034bab2c00105c0f40023c10602e2002
        !hex    af002002ad0020a200202b20202520202820eaa90348abad2830c230a9a90048
        !hex    ababe2306b6c3a10dc3d107c3020ea602e20af003003ad0020202030f0184bab
        !hex    ea8000ad30206b2830af0040028b4babad0020ad0030ab6b60
        } ;!pseudopc
