\ each location entry is
\   0: long-offset|0
\   1: # travel
\   2,6,...: uint32 * #travel   - see advpack.py for 5 field to 4 byte bitmap
\   2+4k: short-desc strz
\   loff: long-desc strz

: cave& ( i -- )                    \ return pointer to 1-indexed location entry
    1- 2* CAVES& + @ CAVES +
;

: cave-links ( i -- link0 n )       \ database.c:gettrav
    dup 0> over MAXLOC <= and 0= if
        42 bug
    then
    cave& 1+ dup 1+ swap c@
;

\ decode cave link from four byte packed representation to 3 words:
\   dest' has 0-2 in high byte and loc in low byte, cf. tdest in C
\   verb is a vocab index
\   cond' has 0-7 in high byte and obj index in low byte, cf. tcond in C
\ database.c:gettrav
\ : cave-link ( link-addr -- dest' verb cond' )
\     dup @ dup $1ff and >r 9 rshift
\     ( p cobj    R: v )
\     swap 2 +
\     ( cobj p2 )
\     @ dup $3ff and >r 10 rshift
\     ( cobj ct   R: v  d' )
\     pack r> r> rot
\     ( d' v c' )
\ ;

\ show long or short description of 1-indexed location
\ database.c:desclg,descsh
: say-loc ( i flag-long -- )
    swap cave& swap
    ( addr f )
    if dup c@ else 0 then
    ( addr long-off|0 )
    ?dup 0= if dup 1+ c@ 2* 1+ 2* then
    ( addr long-off|short-off )
    + sayz
;

\ database.c:forced
: forced? ( at -- flag )
    cond{ c}@ 2 =
;

\ database.c:dark
: dark? ( -- flag )
    loc@ cond{ c}@ LIGHT and 0=      \ ! (cond[loc] & LIGHT)
        'LAMP prop{}@ 0=
        'LAMP not-here
        or
    and
;

\ turn.c:describe
: describe ( -- )               \ describe current location
    'BEAR toting? if
        141 say-msg
    then
    dark? if
        16 say-msg else
        loc@ dup visited{ c}@ 0= say-loc
    then
    33 loc@ = 25 pct and closing @ 0= and if
        8 say-msg
    then
;

\ database.c:liqloc
: liquid-at ( loc -- obj )
    cond{ c}@ dup LIQUID and if
        WATOIL and
    else
        drop 1
    then
    liquid-type
;
