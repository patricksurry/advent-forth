\ each item is simply a concatenated list of 0-terminated terminated
\ the first string is found in the ITEMS& array,
\ subsequent strings are found by skipping past ascii zeros

\ show an item message, with -1 indexed state
\ database.c:pspeak
: speak-item ( i state -- )
    1+ swap 1- 2* ITEMS& + @ ITEMS + swap               \ first item string
    ( strz state+1 )
    begin ?dup while 1- swap asciiz> + 1+ swap repeat   \ skip state+1 strings
    spkz
;

\ database.c:toting
: is-toting ( item -- flag )
    place{ c}@ NOWHERE =
;

\ database.c:here
: is-here ( item -- flag )
    dup
        place{ c}@ loc @ =
        swap is-toting
    or
;

\ database.c:is-at
: is-at ( item -- flag )
    dup
        place{ c}@ loc @ =
        swap fixed{ c}@ loc @ =
    or
;

\ database.c:liq2
: liquid-type ( i -- obj )
    case
        0 of 'WATER endof
        1 of 0 endof
        2 of 'OIL endof
    endcase
;

\ database.c:liq
: bottle-liquid ( -- 'WATER | 'OIL | 0 )
    'BOTTLE prop{ b}@ dup 0< if
        negate 1- then
    liquid-type
;

\ database.c:carry
: carry-item ( obj where -- )
	drop                                   \ where is unused
    dup MAXOBJ < if
        place{ c} dup c@ NOWHERE <> if
            NOWHERE swap c! 1 holding +! else
            drop
        then
    then
;

\ database.c:drop
: drop-item ( obj where -- )
    swap dup MAXOBJ < if
        ( where obj )
        place{ c} dup c@ NOWHERE = if
            ( where place+obj )
            -1 holding +!
        then
    else
        MAXOBJ - fixed{ c}
        ( where fixed+obj-MAXOBJ )
    then
    c!
;

\ database.c:juggle
: juggle-item ( obj -- ) drop ;          \ no-op

\ database.c:move
: move-item ( obj where -- )
    over dup MAXOBJ < if
        ( obj where obj )
        place{ c} else
        MAXOBJ - fixed{ c}
    then
    + c@
    ( obj where from )
    dup 0 > over MAXOBJ <= and if
        ( obj where from )
        >r over r> carry-item
    else
        drop
    then
    ( obj where )
    drop-item
;

\ database.c:dstroy
: destroy-item ( obj -- )
    0 move-item
;

\ database.c:put
: put-item ( obj where pval -- pval' )
    >r move-item -1 r> -        \ obj where move-item; return -1 - pval
;

: ?describe-item ( item -- )
    dup 'STEPS = 'NUGGET is-toting and if
        drop exit then

    dup prop{ b}@ 0< if
        closed @ if
            drop exit then
        dup 'RUG = over 'CHAIN = or
        over prop{ b}!
        -1 tally +!
    then

    dup 'STEPS = loc @ 'STEPS fixed{ c}@ = and if
        1 else
        dup prop{ b}@
    then
    ( item state )
    speak-item
;

\ turn.c:descitem
: describe-items ( -- )     \ describe visible items
    MAXOBJ 1- 1 do
        i is-at if i ?describe-item then
    loop

    tally @ dup tally2 @ = swap 0<> and limit @ 35 > and if
        35 limit !
    then
;
