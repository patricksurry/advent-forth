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
: cave-link ( cave& i -- dest' verb cond' )
    over 1+ c@ over <= if
        33 bug then
    2* 1+ 2* + dup                      \ p p
        @ dup $1ff and >r 9 rshift      \ p cobj    R: v
    swap 2 +                            \ cobj p2
        @ dup $3ff and >r 10 rshift     \ cobj ct   R: v  d'
    pack r> r> rot                      \ d' v c'
;

: speak-location ( i flag-long -- )     \ describe 1-indexed location as long or short version
    swap cave& swap                     \ addr f
    if dup c@ else 0 then               \ addr long-off|0
    ?dup 0= if dup 1+ c@ 2* 1+ 2* then  \ addr long-off|short-off
    + spkz
;

: is-forced ( at -- flag )
    cond + c@ 2 =
;
