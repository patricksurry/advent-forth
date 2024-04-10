\ turn.c:score
: score
    0 0
    ( s t )
    MAXTRS 1+ 50 do
        i prop{}@ 0 >= if
            2 +
        then
        i 'CHEST = if
            12 \ 14 - 2
        else i 'CHEST > if
            14 \ 16 - 2
        else
            10 \ 12 - 2
        then then
        i place{ c}@ 3 = i prop{}@ 0= and if
            +
        else
            drop
        then
    loop
    ( s t )
    ."       Treasures: " dup u. cr
    + MAXDIE numdie @ - 10 *
    ?dup if
        ."        Survival: " dup u. cr
        +
    then
    ( s )
    gaveup @ 0= if
        4 +
    then
    dflag @ ?dup if
        ." Getting well in: 25" cr
        25 +
    then
    closing @ ?dup if
        ." Masters section: 25" cr
        25 +
    then
    closed @ if
        bonus @ case
            0   of 10 endof
            135 of 25 endof
            134 of 30 endof
            133 of 45 endof
            0 swap
        endcase
        ."           Bonus: " dup u. cr
        +
    then
    'MAGAZINE place{ c}@ 108 = if
        1+
    then
    2 +
    ."           Score: " u. cr
;

\ turn.c:normend
: normal-end
    score
    abort               \ exit back to forth
;


\ player's incarnation has passed on
\ turn.c:death
: death
    closing @ 0= if
        numdie @ dup 2* dup 81 + swap 82 + 64 yes-no 0=
        ( numdie ?no )
        swap 1+ dup numdie ! MAXDIE >= or if
            normal-end
        then
        0 'WATER place{ c}!
        0 'OIL place{ c}!
        'LAMP is-toting if
            0 'LAMP prop{}!
        then
        MAXOBJ 1- 1 do      \ original source uses i = MAXOBJ - j, unclear why
            i is-toting if
                i dup 'LAMP = if 1 else oldloc2 @ then drop-item
            then
        loop
        3 newloc !
        loc@ oldloc !
    else
        \ closing -- no resurrection...
        131 speak-message
        1 numdie +!
        normal-end
    then
;

\ handle player's demise via waking up the dwarves...
\ turn.c:dwarfend
: dwarf-end
 	death
 	normal-end
;

: check-closing
    newloc @ dup 9 < swap 0<> and closing @ and if
        130 speak-message
        loc@ newloc !
        panic @ 0= if
            15 clock2 !
        then
        1 panic !
    then
;

: check-closed
    closed @ if
        'OYSTER prop{}@ 0< 'OYSTER is-toting and if
            'OYSTER 1 speak-item
        then
        MAXOBJ 1 do
            i is-toting i prop{}@ 0< and if
                -1 i prop{}@ - prop{}!
            then
        loop
    then
;

: reset-troll
    'TROLL 0 0 move-dual-item
    'TROLL2 117 122 move-dual-item
    \ 'CHASM juggle-item    \ no-op in this version
;

create stroom
    'BOTTLE 2* 1+ c, 115 c,
    'PLANT  2*    c, 115 c,
    'OYSTER 2*    c, 115 c,
    'LAMP   2*    c, 115 c,
    'ROD    2*    c, 115 c,
    'DWARF  2*    c, 115 c,
    'SNAKE  2* 1+ c, 116 c,
    'BIRD   2* 1+ c, 116 c,
    'CAGE   2*    c, 116 c,
    'ROD2   2*    c, 116 c,
    'PILLOW 2*    c, 116 c,
    'MIRROR 2*    c, 115 c,
    0 ,


\ special time limit stuff...
\ turn.c:stimer
: stimer
    foobar dup @ dup 0> if negate else 0 and then !

    clock1 @
    tally 0= loc@ dup 15 >= swap 33 <> and and if
        1- dup clock1 !
    then
    ( clock1@ )
    ?dup 0= if
        \ start closing the cave
        0 'GRATE prop{}!
        0 'FISSURE prop{}!
        MAXDWARF 1 do
            0 i dseen{ c}!
        loop
        reset-troll
        3 'BEAR prop{}@ <> if
            'BEAR destroy-item
        then
        0 'CHAIN prop{}!
        0 'CHAIN fixed{ c}!
        0 'AXE prop{}!
        0 'AXE fixed{ c}!
        129 speak-message
        -1 clock1 !
        1 closing !
        0 exit
    then
    ( clock1@ )
    clock2 @ swap 0< if
        1- dup clock2 !
    then
    ( clock2@ )
    0= if
        \ set up storage room and close the cave...
        115 dup dup
        loc ! oldloc ! newloc !
        'GRATE      116 move-item
        116 'MIRROR fixed{ c}!

        stroom
        begin
            \ each value is (obj 2* pval + | loc << 16 )
            dup @ dup while
            unpack >r dup 1 and swap 2/ r>
            ( stroom pval obj loc )
            \ put-item      \ inlined - only used here

            \ database.c:put
            \ move-item and set obj prop{} to -1 - pval
            \ : put-item ( pval obj where -- )
                over swap move-item
                ( pval obj )
                -1 rot - swap prop{}!
            \ ;
            2 +
        repeat
        2drop

        MAXOBJ 1 do
            i is-toting if
                i destroy-item
            then
        loop
        132 speak-message
        1 closed !
        1 exit
    then

    limit @
    'LAMP prop{}@ 1 = if
        1- dup limit !
    then
    ( limit@ )
    dup 30 <= 'BATTERIES is-here and 'BATTERIES prop{}@ 0= and 'LAMP is-here and if
        188 speak-message
        1 'BATTERIES prop{}!
        'BATTERIES is-toting if
            'BATTERIES loc@ drop-item
        then
        ( limit@ )
        2500 + limit !
        0 lmwarn !
        0 exit
    then
    ( limit@ )
    dup 0= if
        1- limit !
        0 'LAMP prop{}!
        'LAMP is-here if
            184 speak-message
        then
        0 exit
    then
    ( limit@ )
    dup 0< loc@ 8 <= and if
        drop
        185 speak-message
        1 gaveup !
        normal-end
    then
    ( limit@ )
    30 <= lmwarn @ 0= and 'LAMP is-here and if
        1 lmwarn !
        187
        'BATTERIES place{ c}@ 0= if
            drop 183
        then
        'BATTERIES prop{}@ 1 = if
            drop 189
        then
        speak-message
    then
    0
;

\ check for presence of dwarves..
\ database.c:dcheck
: dwarf-check ( -- i|0 )
    loc @ 0
    MAXDWARF 1- 1 do
        ( loc 0 )
        over i dloc{ c}@ = if
            drop i leave
            ( loc i )
        then
    loop
    nip
;

: check-seen
    newloc @ loc@ <>
    loc@ is-forced 0= and
    loc@ cond{ c}@ NOPIRAT and 0= and if
        MAXDWARF 1- 1 do
            i odloc{ c}@ newloc @ =
            i dseen{ c}@ and if
                loc@ newloc !
                2 speak-message
                leave
            then
        loop
    then
;

: stealable ( i -- f )
    'PYRAMID <>
    newloc @ dup 'PYRAMID place{ c}@ <>
    swap 'EMERALD place{ c}@ <>
    and or
;

: put-chloc ( -- )
    chloc @ dup
    6 dloc{ c}!
    6 odloc{ c}!
    0 6 dseen{ c}!
;

: stealit
    128 speak-message

    'MESSAGE place{ c}@ 0= if
        'CHEST chloc @ move-item
    then

    'MESSAGE chloc2 @ move-item

    MAXTRS 1+ 50 do
        i stealable if
            i is-at i fixed{ c}@ 0= and if
                i newloc @ carry-item
            then
            i is-toting if
                i chloc @ drop-item
            then
        then
    loop

    put-chloc
;

\ pirate stuff
\ turn.c:dopirate
: do-pirate ( -- )
    chloc @ newloc @ = 'CHEST prop{}@ 0 >= or if
        exit
    then

    0
    MAXTRS 50 do
        i stealable if
            i is-toting if
                drop stealit exit
            then
            i is-here if
                1+
            then
        then
    loop
    ( #treasure )

    0=
    tally @ tally2 @ 1+ and
    'CHEST place{ c}@ 0= and
    'LAMP is-here and
    'LAMP prop{}@ 1 = and
    if
        186 speak-message
        'CHEST chloc @ move-item
        'MESSAGE chloc2 @ move-item
        put-chloc
        exit
    then

    6 odloc{ c}@ 6 dloc{ c}@ <> 20 pct and if
        127 speak-message
    then
;

: move-dwarf ( i -- attack? here? )
    \ move an active dwarf at random.  we don't have
    \ a matrix around to do it as in the original version...

    0 swap
    begin
        nip
        106 randint  15 +  swap       \ allowed area
        ( loc i )
        2dup odloc{ c}@ <> >r  \ not current loc
        2dup dloc{ c}@ <> r> and >r
        ( loc i  R: f )
        over cond{ c}@ NOPIRAT and
        over MAXDWARF 1- = and 0= r> and
        ( loc i f )
    until

    ( loc i )
    dup dloc{ c}@  over odloc{ c}!
    2dup dloc{ c}!
    ( loc i )
    newloc @ tuck 2swap = >r
    ( newloc i  R: f )
    2dup odloc{ c}@ = r> or >r
    ( newloc i  R: f' )
    2dup dseen{ c}@ swap 15 >= and r> or
    ( newloc i f" )
    1 and 2dup swap dseen{ c}!
    ( newloc i 0|1 )
    0= if
        2drop 0 0 exit
    then
    tuck dloc{ c}!
    ( i )

    dup 6 = if
        drop do-pirate
        0 0
    else
        dup odloc{ c}@ swap dloc{ c}@ = 1 and   \ stick?
        1                                       \ here
    then
;


: do-dwarves ( -- )         \ dwarf stuff.
    \ see if dwarves allowed here
    newloc @  dup 0=
    over is-forced or
    swap cond{ c}@ NOPIRAT and  or  if
        exit
    then

    \ see if dwarves are active.
    dflag @ ?dup 0= if
        newloc @ 15 > if
            1 dflag +!
        then
        exit
    then
    ( dflag )

    \ if first close encounter (of 3rd kind) kill 0, 1 or 2
    1 = if
        newloc @ 15 < 95 pct or if
            exit
        then
        1 dflag +!
        3 1 do
            50 pct if
                0  5 randint 1+  dloc{ c}!
            then
        loop
        MAXDWARF 1- 1 do
            i dloc{ c}@ dup newloc @ = if
                drop daltloc @
                dup  i dloc{ c}!
            then
            i odloc{ c}!
        loop
        3 speak-message
        'AXE newloc @ drop-item
        exit
    then

    0 0
    ( #attack #dtotal )
    MAXDWARF 1 do
        i dloc{ c}@ if
            i move-dwarf rot + >r + r>
        then
    loop

    ( #attack #dtotal )
    dup 0= if
        2drop exit
    then

    0 knfloc @ >= if
        newloc @ knfloc !
    then

    dup 1 > if
        ." There are " u. ." threatening little dwarves in the room with you!" cr
    else
        drop 4 speak-message
    then

    ( #attack )
    dup 0= if
        drop exit
    then

    0
    dup 0 do
        1000 randint  95 dflag @ 2 - *  <  if
            1+
        then
    loop
    swap

    ( #stick #attack )
    dflag @ 2 = if
        1 dflag +!
    then

    dup 1 > if
        u. ." of them throw knives at you!!" cr
        6
    else
        drop 5 speak-message
        52
    then

    ( #stick k )
    over 1 <= if
        over + speak-message
        0= if
            exit
        then
    else
        drop
        u. ." of them get you !!!" cr
    then

    newloc @ oldloc2 !
    death
;
