\ each location entry is
\   0: long-offset|0
\   1: # travel
\   2,6,...: uint32 * #travel   - see advpack.py for 5 field to 4 byte bitmap
\   2+4k: short-desc strz
\   loff: long-desc strz

: cave& ( i -- )                        \ return pointer to 1-indexed location entry
    1- 2* CAVES& + @ CAVES +
;

\ decode cave link from four byte packed representation to 3 words:
\   dest' has 0-2 in high byte and loc in low byte
\   verb is a vocab index
\   cond' has 0-7 in high byte and obj index in low byte
\ database.c:gettrav
: cave-link ( link-addr -- dest' verb cond' )
    dup @ dup $1ff and >r 9 rshift      \ p cobj    R: v
    swap 2 +                            \ cobj p2
        @ dup $3ff and >r 10 rshift     \ cobj ct   R: v  d'
    pack r> r> rot                      \ d' v c'
;

\ describe 1-indexed location as long or short version
\ database.c:desclg,descsh
: speak-location ( i flag-long -- )
    swap cave& swap                     \ addr f
    if dup c@ else 0 then               \ addr long-off|0
    ?dup 0= if dup 1+ c@ 2* 1+ 2* then  \ addr long-off|short-off
    + spkz
;

\ database.c:forced
: is-forced ( at -- flag )
    cond{ c}@ 2 =
;

\ turn.c:describe
: describe ( -- )               \ describe current location
    'BEAR is-toting if
        141 speak-message then
    is-dark if
        16 speak-message else
        loc @ dup visited{ c}@ invert speak-location then
    33 loc @ = 25 pct and closing @ invert and if
        8 speak-message then
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

\ check for presence of dwarves..
\ database.c:dcheck
\ unsigned char dcheck(void) {
\   unsigned char i;
\
\   for (i = 1; i < (DWARFMAX - 1); ++i) {
\     if (dloc[i] == loc)
\       return i;
\   }
\   return 0;
\ }
