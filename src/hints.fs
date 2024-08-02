\ support six 0-indexed hints
\ locations are tagged with relevant hint using bits 4-6 in cond byte

6 constant MAXHINT

\ columns in the hints{ array
0 constant WAIT     \ number of turns in place before hint
1 constant COST     \ score cost
2 constant ASK      \ prompt message
3 constant HINT     \ hint message

create hints{
4 c,  2 c, 62 c,  63 c,    \ 1 grate
5 c,  2 c, 18 c,  19 c,    \ 2 bird
8 c,  2 c, 20 c,  21 c,    \ 3 snake
75 c, 4 c, 176 c, 177 c,   \ 4 maze
25 c, 5 c, 178 c, 179 c,   \ 5 plover
20 c, 3 c, 180 c, 181 c,   \ 6 dark

: hintp ( hint0 -- hintp )
    2 lshift hints{ +
;

: hint-cost ( -- n )
    0  hinted c@
    MAXHINT 0 do
        dup 1 and if
            swap  i hintp COST c}@ +  swap
        then
        1 rshift
    loop
    drop
;

: offer-hint ( hint0 -- )
    dup hintp
    dup ASK c}@ 0 54 yes-no if
        ." I am prepared to give you a hint but, it will cost you "
        dup COST c}@ . ." points." cr
        175 swap HINT c}@ 54 yes-no
        ( hint0 f )
        1 rot lshift and
        ( hint-bit | 0 )
        hinted c@ or hinted c!
    else
        2drop
    then
;

: has-hint ( hint0 -- f )
    \ is this location tagged for this hint?
    \ get 1-based hint bits from cond and subtract one
    loc@ cond{ c}@ HINTMASK and 4 rshift 1- =
;

: valid-hint ( hint0 -- f )
    case
        0 of
            'GRATE prop{}@ 0=
            'KEYS not-here and
        endof
        1 of
            'BIRD here?
            'ROD toting? and
            'BIRD object @ = and
        endof
        2 of
            'SNAKE here?
            'BIRD not-here and
        endof
        3 of
            loc@ vacant?
            oldloc @ vacant? and
            oldloc2 @ vacant? and
            holding 1 > and
        endof
        4 of
            'EMERALD prop{}@ -1 <>
            'PYRAMID prop{}@ -1 = and
        endof
        5 of
            true
        endof
        27 bug
    endcase
;

: hints?
    MAXHINT 0 do
        1 i lshift hinted c@ and 0= if
            i has-hint if
                i hintlc{ c}@ 1+
            else
                0
            then
            dup i hintlc{ c}!
            i hintp WAIT c}@ >= if
                i valid-hint if
                    i offer-hint
                    true
                else
                    i 1 <>      \ bird hint doesn't reset on invalid
                then
                if
                    0 i hintlc{ c}!
                then
            then
        then
    loop
;
