;ACME can't handle 65816 code that lives outside bank zero
*       =       $0000
        !pseudopc $1000 {
        !hex    18fbe2305c000044000102cf000044af000044ad0000a50030f562b2ffd0b082
        !hex    a9ff1700170044cfc0ff44f005303c8239005c0020002c0020f41700f44400d0
        !hex    03dc0810ea201220201520200f202256341260
        } ;!pseudopc
