: speak-item ( i state -- )     \ show an item message, with 0-indexed state
    swap 1- 2* ITEMS& + @ ITEMS + swap                  \ first item string ( strz state )
    begin ?dup while swap asciiz> + 1+ swap repeat      \ skip state-1 strings
    spkz
;

: is-toting ( item -- flag )
    place + c@ NOWHERE =
;

: is-here ( item -- flag )
    dup
        place + c@ loc @ =
        swap is-toting
    or
;

: is-dark ( -- flag )
    loc @ cond + c@ LIGHT and 0=    \ ! (cond[loc] & LIGHT)
        LAMP prop + c@ 0=
        LAMP is-here 0=
        or
    and
;

: is-at ( item -- flag )
    dup
        place + c@ loc @ =
        swap fixed + c@ loc @ =
    or
;

: carry-item ( obj where -- )
	drop               \ where is unused
    dup MAXOBJ < if
        place + dup c@ NOWHERE <> if
            NOWHERE swap c! 1 holding +! else
            drop
        then
    then
;

: drop-item ( obj where -- )
    swap dup MAXOBJ < if                    \ where obj
        place + dup c@ NOWHERE = if         \ where place+obj
            -1 holding +! then
        else
        MAXOBJ - fixed + then               \ where fixed+obj-MAXOBJ
    c!
;

: juggle ( obj -- ) drop ;          \ no-op

: move-item ( obj where -- )
    over dup MAXOBJ < if            \ obj where obj
        place else
        MAXOBJ - fixed then
    + c@                            \ obj where from
    dup 0 > over MAXOBJ <= and if   \ obj where from
        >r over r> carry-item else  \ obj where; after obj from carry-item
        drop then                   \ obj where
    drop-item
;

: destroy-item ( obj -- )
    0 move-item
;

: put-item ( obj where pval -- pval' )
    >r move-item -1 r> -        \ obj where move-item; return -1 - pval
;

: ?describe-item ( item -- )
    dup STEPS = NUGGET is-toting and if drop exit then

    dup prop + @ 0< if
        closed @ if drop exit then
        dup RUG = over CHAIN = or
        over prop + !
        -1 tally +!
    then

    dup STEPS = loc @ STEPS fixed + @ = and if
        1 else
        dup prop + @
    then                \ item state
    speak-item
;

: describe-items ( -- )     \ describe visible items
    MAXOBJ 1- 1 do
        i is-at if i ?describe-item then
    loop

    tally @ dup tally2 @ = swap 0<> and limit @ 35 > and if
        35 limit !
    then
;
