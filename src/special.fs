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

\ special time limit stuff...
\ turn.c:stimer
: stimer
    foobar dup @ dup 0> if negate else 0 then !

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
        'TROLL 0 0 move-dual-item
        'TROLL2 117 122 move-dual-item
        'CHASM juggle-item

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
        'BOTTLE 115 1 put-item!
        'PLANT 115 0 put-item!
        'OYSTER 115 0 put-item!
        'LAMP 115 0 put-item!
        'ROD 115 0 put-item!
        'DWARF 115 0 put-item!
        115 dup dup
        loc ! oldloc ! newloc !
        'GRATE 116 0 put-item drop
        'SNAKE 116 1 put-item!
        'BIRD 116 1 put-item!
        'CAGE 116 0 put-item!
        'ROD2 116 0 put-item!
        'PILLOW 116 0 put-item!
        'MIRROR 115 0 put-item!
        116 'MIRROR fixed{ c}!

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

: do-dwarves ( -- )
\ TODO do-dwarves
\ /*
\ 	dwarf stuff.
\ */
\ turn.c:dwarves
\ void dwarves(void)
\ {
\ 	int i, j, k, try, attack, stick, dtotal;
\
\ 	/*
\ 		see if dwarves allowed here
\ 	*/
\ 	if (newloc == 0 || forced(newloc) || cond[newloc] & NOPIRAT)
\ 		return;
\ 	/*
\ 		see if dwarves are active.
\ 	*/
\ 	if (!dflag) {
\ 		if (newloc > 15)
\ 			++dflag;
\ 		return;
\ 	}
\ 	/*
\ 		if first close encounter (of 3rd kind)
\ 		kill 0, 1 or 2
\ 	*/
\ 	if (dflag == 1) {
\ 		if (newloc < 15 || pct(95))
\ 			return;
\ 		++dflag;
\ 		for (i = 1; i < 3; ++i)
\ 			if (pct(50))
\ 				dloc[rand() % 5 + 1] = 0;
\ 		for (i = 1; i < (DWARFMAX - 1); ++i) {
\ 			if (dloc[i] == newloc)
\ 				dloc[i] = daltloc;
\ 			odloc[i] = dloc[i];
\ 		}
\ 		rspeak(3);
\ 		drop(AXE, newloc);
\ 		return;
\ 	}
\ 	dtotal = attack = stick = 0;
\ 	for (i = 1; i < DWARFMAX; ++i) {
\ 		if (dloc[i] == 0)
\ 			continue;
\ 		/*
\ 			move a dwarf at random.  we don't
\ 			have a matrix around to do it
\ 			as in the original version...
\ 		*/
\ 		for (try = 1; try < 20; ++try) {
\ 			j = rand() % 106 + 15; /* allowed area */
\ 			if (j != odloc[i] && j != dloc[i] &&
\ 			    !(i == (DWARFMAX - 1) && (cond[j] & NOPIRAT) == NOPIRAT))
\ 				break;
\ 		}
\ 		if (j == 0)
\ 			j = odloc[i];
\ 		odloc[i] = dloc[i];
\ 		dloc[i] = j;
\ 		if ((dseen[i] && newloc >= 15) || dloc[i] == newloc || odloc[i] == newloc)
\ 			dseen[i] = 1;
\ 		else
\ 			dseen[i] = 0;
\ 		if (!dseen[i])
\ 			continue;
\ 		dloc[i] = newloc;
\ 		if (i == 6)
\ 			dopirate();
\ 		else {
\ 			++dtotal;
\ 			if (odloc[i] == dloc[i]) {
\ 				++attack;
\ 				if (knfloc >= 0)
\ 					knfloc = newloc;
\ 				if (rand() % 1000 < 95 * (dflag - 2))
\ 					++stick;
\ 			}
\ 		}
\ 	}
\ 	if (dtotal == 0)
\ 		return;
\ 	if (dtotal > 1)
\ 		printf("There are %d threatening little dwarves in the room with you!\n", dtotal);
\ 	else
\ 		rspeak(4);
\ 	if (attack == 0)
\ 		return;
\ 	if (dflag == 2)
\ 		++dflag;
\ 	if (attack > 1) {
\ 		printf("%d of them throw knives at you!!\n", attack);
\ 		k = 6;
\ 	} else {
\ 		rspeak(5);
\ 		k = 52;
\ 	}
\ 	if (stick <= 1) {
\ 		rspeak(stick + k);
\ 		if (stick == 0)
\ 			return;
\ 	} else
\ 		printf("%d of them get you !!!\n", stick);
\ 	oldloc2 = newloc;
\ 	death();
\ }
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
