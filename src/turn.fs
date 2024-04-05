
\ explain poor move choice
\ turn.c:badmove
: bad-move ( -- )
	motion c@
    dup 43 >= over 50 <= and if
        drop 9 else
        case
            29 of 9 endof
            30 of 9 endof
            7  of 10 endof
            36 of 10 endof
            37 of 10 endof
            11 of 11 endof
            19 of 11 endof
            62 of 42 endof
            65 of 42 endof
            17 of 80 endof
            12 swap
        endcase then
    \ ?? does order matter here? occurs within case in c code
    verb c@ dup 'FIND = swap 'INVENTORY = or
    if drop 59 then
    speak-message
;

: check-move ( cond' -- flag )
    unpack
    ( cobj ct )
    case
        0 of dup 0= swap pct or endof
		1 of dup 0= swap is-toting or endof
		2 of dup is-toting swap is-at or endof
        \ default case needs to leave ( flag ct )
        \ ( cobj ct )
		dup 3 8 within 0= if 37 bug then
		dup 3 - rot prop{}@ <>
		( ct flag )
		swap
    endcase
;

\ handle special movement
\ turn.c:spcmove
: move-special ( d -- )
    case
        1 of        \ plover movement via alcove
            holding @ dup 0= swap 1 = 'EMERALD is-toting and or if
                199 loc@ - newloc !
            else
                117 speak-message
            then
        endof

        2 of        \ trying to remove plover, bad route
            'EMERALD loc@ drop-item
        endof

        3 of        \ troll bridge
            'TROLL prop{}@ 1 = if
                'TROLL 1 speak-item
                0 'TROLL prop{}!
                'TROLL2 0 move-item
                [ 'TROLL2 MAXOBJ + ] literal 0 move-item
                'TROLL 117 move-item
                [ 'TROLL MAXOBJ + ] literal 122 move-item
                'CHASM juggle-item
                loc@ newloc !
            else
                loc@ 117 = if 122 else 117 then newloc !
                'TROLL prop{}@ 0= if
                    1 prop{}!
                then
                'BEAR is-toting if
                    162 speak-message
                    1 'CHASM prop{}!
                    2 'TROLL prop{}!
                    'BEAR newloc @ drop-item
                    NOWHERE 'BEAR fixed{ c}@
                    3 'BEAR prop{}!
                    'SPICES prop{}@ 0< if
                        1 tally2 +!
                    then
                    newloc @ oldloc2 !
                    death
                then
            then
        endof
        38 bug
    endcase
;


\ turn.c:dotrav
: do-travel ( -- )      \ dotrav() loc -> newloc based on motion
    loc@ dup newloc !              \ default to current loc
    cave-links 0
    false >r                        \ hit flag
    ( link-addr n 0 )
    begin
        ( link n 0 )
        2dup 0= swap 0> and while   \ no move and more links?
        drop over decode-link
        ( link n d' v c' )
        \ verb matches motion or 1, or already hit?
        swap dup 1 = swap motion @ = or r> or if
            ( link n d' c' )
            true >r check-move
            ( link n d' f )
            0= if drop 0 then
            ( link n d'|0 )
        else
            false >r 2drop 0
            ( link n 0 )
        then
        >r 1- swap 4 + swap r>
        ( next-link n-1 d'|0 )
    repeat
    r> drop -rot 2drop
    ( d'|0 )

    ?dup if
        unpack case
            ( dest dt )
            0 of newloc ! endof
            1 of move-special endof
            2 of speak-message endof
        endcase
        else bad-move
    then
;

\ return from whence we came!
\ turn.c:goback
: go-back ( -- )
    oldloc @ dup dup is-forced if
        oldloc2 @ swap
    then
    oldloc2 ! loc@ dup oldloc !
    ( want loc )
    over = if
        91 speak-message
        exit
    then
    ( want )

    \ look through current loc links for wanted destination
    loc@ cave-links
    ( want link-addr n )
    false >r                        \ tmp verb via forced dest
    begin
        ?dup while
        -rot swap over decode-link
        ( n addr want d' v c' )

        \ unconditional link?
        0= if
            \ is it the destination we want?
            -rot 2dup = if
                ( n addr v want d' )
                r> drop 2drop motion ! 2drop
                do-travel exit
            then
            ( n addr v want d' )
            \ is dest forced to where we want to go?
            unpack 0= over is-forced and if
                cave-links drop decode-link 2drop
                ( n addr v want d0' )
                over = if
                    swap r> drop >r
                else
                    nip
                then
            else
                drop nip
            then
            ( n addr want )
        else
            2drop
        then

        swap 4 + rot 1-
        ( want addr' n-1 )
    repeat
    2drop
    r> ?dup if
        motion !
    else
        140 speak-message
    then
;

\ turn.c:domove
: do-move ( -- )
    motion @ case
        'NULLX of endof
        'BACK of go-back endof
        'LOOK of
            detail @ 1+ dup detail !
            3 < if 15 speak-message then
            0 wzdark !
            loc@ false over visited{ c}!
            newloc ! 0 loc !
        endof
        'CAVE of
            loc@ 8 <
            if 57 else 58 then
            speak-message
        endof
        oldloc @ oldloc2 !
        loc@ oldloc !
        do-travel
    endcase
;

\ turn.c:doobj
: do-object ( -- )
    object @
    dup
    fixed{ c}@ loc@ = over is-here or if       \ is object here?
        drop transitive-verb exit
    then

    \ did they give grate as destination?
    dup
    'GRATE = if
        loc@ dup 1 = over 4 = or over 7 = or if
            ( obj loc )
            'DEPRESSION motion !
            2drop do-move exit
        else 10 15 within if
            'ENTRANCE motion !
            drop do-move exit
        then then
    then

    \ is it a dwarf he is after?
    dwarf-check dflag @ 2 >= and if
        'DWARF object !
        drop transitive-verb exit
    then

    \ is he trying to get/use a liquid?
    dup
    bottle-liquid = 'BOTTLE is-here and
    over loc@ liquid-at = or if
        drop transitive-verb exit
    then

    dup
    'PLANT = 'PLANT2 is-at and 'PLANT2 prop{}@ 0= and if
        'PLANT2 object !
        drop transitive-verb exit
    then

    \ is he trying to grab a knife?
    dup 'KNIFE = loc@ knfloc @ = and if
        116 speak-message
        -1 knfloc !
        drop exit
    then

    \ is he trying to get at dynamite?
    'ROD = 'ROD2 is-here and if
        'ROD2 object !
        transitive-verb exit
    then

    say-not-here
;

\ turn.c:turn
: turn
    check-closing

    \ see if a dwarf has seen him and has come from where he wants to go.
    check-seen

    do-dwarves \ including special dwarf (pirate who steals)

    \ moving?
    loc@ newloc @ <> if
        1 turns +!
        newloc @ dup loc !
        ( location )
        ?dup 0= if                  \ location 0 means death
            death exit
        then
        is-forced if                \ forced moved?
            describe-location
            do-move
            exit
        then

        \ wandering in the dark?
        wzdark @ is-dark and 35 pct and if
            23 speak-message
            loc@ oldloc2 !
            death
            exit
        then

        describe-location

        is-dark 0= if
            true loc@ visited{ c}!
            describe-items
        then
    then

    check-hints

    check-closed

    is-dark wzdark !
    knfloc @ dup 0> swap loc@ <> and if
        0 knfloc !      \ ?? knfloc = 1
    then

    \ as the grains of sand slip by
    stimer if
        exit
    then

    begin english until     \ retrieve player instructions

    ." loc " loc@ . ."  verb " verb @ . ."  obj " object @ . ."  motion " motion @ . .s CR

    motion @ if             \ execute player instructions
        do-move
    else
        object @ if
            do-object
        else
            intransitive-verb
        then
    then
;
