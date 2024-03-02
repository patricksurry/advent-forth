
\ external word linkage

$80 constant RAND16
$f900 constant KERNEL
KERNEL $08 + constant NATIVE-RNG
KERNEL $1c + constant NATIVE-STRLEN
KERNEL $43 + constant NATIVE-DECODE
KERNEL $9e + constant NATIVE-STRWRAP

42 RAND16 !     \ initialize random

: random ( -- n )
    NATIVE-RNG execute RAND16 @
;

: asciiz> ( c-addr -- c-addr u )
    NATIVE-STRLEN execute
;

\ packed data file; see scripts/advpack.py

: advptr ( k -- addr )
    ADVDAT @ 8 <> if -1 bug then
    0 swap begin ?dup 0> while
        dup 1- -rot                     \ k-1 off k
        2* ADVDAT + @ + swap            \ off k-1
    repeat
    ADVDAT + [ 8 1 + 2 * ] literal +              \ skip header
;

0 advptr constant DIGRAMS
1 advptr constant VOCAB
2 advptr constant CAVES&
3 advptr constant CAVES
4 advptr constant MSGS&
5 advptr constant MSGS
6 advptr constant ITEMS&
7 advptr constant ITEMS

ADVDAT $c00 - constant MSGBUF   \ reserve 3K for two-stage decode of string max len 1394 bytes
: decode ( strz DIGRAMS outz -- addr n )
    NATIVE-DECODE execute
;
