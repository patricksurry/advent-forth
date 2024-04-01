\ these are 0-indexed arrays but hint is 1 indexed

6 constant MAXHINT

create hints{
4 c,  2 c, 62 c,  63 c,    \ 1 grate
5 c,  2 c, 18 c,  19 c,    \ 2 bird
8 c,  2 c, 20 c,  21 c,    \ 3 snake
75 c, 4 c, 176 c, 177 c,   \ 4 maze
25 c, 5 c, 178 c, 179 c,   \ 5 plover
20 c, 3 c, 180 c, 181 c,   \ 6 dark

0 constant WAIT
1 constant COST
2 constant ASK
3 constant HINT

create hinted{
0 c, 0 c, 0 c, 0 c, 0 c, 0 c,

create hintlc{
0 c, 0 c, 0 c, 0 c, 0 c, 0 c,

: hintp ( hint0 -- hintp )
    2 lshift hints{ +
;

: offer-hint ( hint0 -- )
    dup hintp
    dup ASK c}@ 0 54 yes-no if
        ." I am prepared to give you a hint but, it will cost you "
        dup COST c}@ . ." points." cr
        175 swap HINT c}@ 54 yes-no
        swap hinted{ c}!
    else
        2drop
    then
;

: has-hint ( hint0 -- f )
    loc@ cond{ c}@ HINTMASK and 4 rshift 1- =
;

: valid-hint ( hint0 -- f )
    case
        0 of
            'GRATE prop{}@ 0=
            'KEYS not-here and
        endof
        1 of
            'BIRD is-here
            'ROD is-toting and
            'BIRD object @ = and
        endof
        2 of
            'SNAKE is-here
            'BIRD not-here and
        endof
        3 of
            loc@ location-empty
            oldloc @ location-empty and
            oldloc2 @ location-empty and
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

: check-hints
    MAXHINT 0 do
        i hinted{ c}@ 0= if
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
                    0 over hintlc{ c}!
                then
            then
        then
    loop
;
