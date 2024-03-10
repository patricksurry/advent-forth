
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
    \ TODO does order matter here? occurs within case in c code
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

		dup 3 < over 7 > or if 37 bug then
		dup 3 - rot prop{ b}@ <>
    endcase
;

\ handle special movement
\ turn.c:spcmove
: move-special ( d -- )
    \ TODO
    drop

\ 	switch (rdest - 300) {
\ 	case 1: /* plover movement via alcove */
\ 		if (!holding || (holding == 1 && toting(EMERALD)))
\ 			newloc = (99 + 100) - loc;
\ 		else
\ 			rspeak(117);
\ 		break;
\ 	case 2: /* trying to remove plover, bad route */
\ 		drop(EMERALD, loc);
\ 		break;
\ 	case 3: /* troll bridge */
\ 		if (prop[TROLL] == 1) {
\ 			pspeak(TROLL, 1);
\ 			prop[TROLL] = 0;
\ 			move(TROLL2, 0);
\ 			move((TROLL2 + MAXOBJ), 0);
\ 			move(TROLL, 117);
\ 			move((TROLL + MAXOBJ), 122);
\ 			juggle(CHASM);
\ 			newloc = loc;
\ 		} else {
\ 			newloc = (loc == 117 ? 122 : 117);
\ 			if (prop[TROLL] == 0)
\ 				++prop[TROLL];
\ 			if (!toting(BEAR))
\ 				return;
\ 			rspeak(162);
\ 			prop[CHASM] = 1;
\ 			prop[TROLL] = 2;
\ 			drop(BEAR, newloc);
\ 			fixed[BEAR] = -1;
\ 			prop[BEAR] = 3;
\ 			if (prop[SPICES] < 0)
\ 				++tally2;
\ 			oldloc2 = newloc;
\ 			death();
\ 		}
\ 		break;
\ 	default:
\ 		bug(38);
\ 	}
;

\ turn.c:dotrav
: do-travel ( -- )      \ dotrav() loc -> newloc based on motion
    loc @ dup newloc !              \ default to current loc
    cave-links 0
    false >r                        \ hit flag
    ( link-addr n 0 )
    begin
        ( link n 0 )
        2dup 0= swap 0> and while   \ no move and more links?
        drop over cave-link
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
    oldloc2 ! loc @ dup oldloc !
    ( want loc )
    over = if
        91 speak-message
        exit
    then
    ( want )

    \ look through current loc links for wanted destination
    loc @ cave-links
    ( want link-addr n )
    false >r                        \ tmp verb via forced dest
    begin
        ?dup while
        -rot swap over cave-link
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
                cave-links drop cave-link 2drop
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
            loc @ false over visited{ c}!
            newloc ! 0 loc !
        endof
        'CAVE of
            loc @ 8 <
            if 57 else 58 then
            speak-message
        endof
        oldloc @ oldloc2 !
        loc @ oldloc !
        do-travel
    endcase
;

\ turn.c:doobj
: do-object ( -- )
    object @
    dup fixed{ c}@ loc @ = over is-here or if       \ is object here?
        drop transitive-verb exit
    then

    \ TODO special cases
\	/*
\		did he give grate as destination?
\	*/
\	else if (object == GRATE) {
\		if (loc == 1 || loc == 4 || loc == 7) {
\			motion = DEPRESSION;
\			domove();
\		} else if (loc > 9 && loc < 15) {
\			motion = ENTRANCE;
\			domove();
\		}
\	}
\	/*
\		is it a dwarf he is after?
\	*/
\	else if (dcheck() && dflag >= 2) {
\		object = DWARF;
\		trobj();
\	}
\	/*
\	   is he trying to get/use a liquid?
\	*/
\	else if ((liq() == object && here(BOTTLE)) || liqloc(loc) == object)
\		trobj();
\	else if (object == PLANT && at(PLANT2) && prop[PLANT2] == 0) {
\		object = PLANT2;
\		trobj();
\	}
\	/*
\	   is he trying to grab a knife?
\	*/
\	else if (object == KNIFE && knfloc == loc) {
\		rspeak(116);
\		knfloc = -1;
\	}
\	/*
\	   is he trying to get at dynamite?
\	*/
\	else if (object == ROD && here(ROD2)) {
\		object = ROD2;
\		trobj();
\	} else

    drop
    ." I see no " last-nonverb-cstr 2@ type ."  here." CR
;

\ turn.c:turn
: turn
    newloc @ dup 9 < swap 0<> and closing @ and if
        130 speak-message
        loc @ newloc !
        panic @ 0= if
            15 clock2 !
        then
        1 panic !
    then

    \ TODO
    \ see if a dwarf has seen him and has come from where he wants to go.
\	if (newloc != loc && !forced(loc) && (cond[loc] & NOPIRAT) == 0) {
\		for (i = 1; i < (DWARFMAX - 1); ++i) {
\			if (odloc[i] == newloc && dseen[i]) {
\				newloc = loc;
\				rspeak(2);
\				break;
\			}
\		}
\	}
\	dwarves(); /* & special dwarf(pirate who steals)	*/

    loc @ newloc @ <> if
        1 turns +!
        newloc @ dup loc !
        ( location )
        ?dup 0= if                  \ location 0 means death
            death exit
        then
        is-forced if                \ forced moved?
            describe
            do-move
            exit
        then

        \ wandering in the dark?
        wzdark @ is-dark and 35 pct and if
            23 speak-message
            loc @ oldloc2 !
            death
            exit
        then

        describe

        is-dark 0= if
            true loc @ visited{ c}!
            describe-items
        then

    then

\ TODO
\	if (closed) {
\		if (prop[OYSTER] < 0 && toting(OYSTER))
\			pspeak(OYSTER, 1);
\		for (i = 1; i < MAXOBJ; ++i) {
\			if (toting(i) && prop[i] < 0)
\				prop[i] = -1 - prop[i];
\		}
\	}

\	wzdark = dark();
\	if (knfloc > 0 && knfloc != loc)
\		knfloc = 0;
\
\	if (stimer()) /* as the grains of sand slip by	*/
\		return;

    begin english until     \ retrieve player instructions

    \ DEBUG ." loc " loc @ . ."  verb " verb @ . ."  obj " object @ . ."  motion " motion @ . CR

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
