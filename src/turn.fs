
: try-move ( cond' -- flag )
    unpack                  \ ( cobj ct )
    case
        0 of dup 0= swap pct or endof
		1 of dup 0= swap is-toting or endof
		2 of dup is-toting swap is-at or endof

		dup 3 < over 7 > or if 37 bug then
		dup 3 - rot prop{ b}@ <>
    endcase
;

: bad-move ( -- )       \ explain ppor move choice
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

: move-special ( d -- )
    ." move-special TODO"
    drop
;

: do-travel ( -- )      \ dotrav() loc -> newloc based on motion
    loc @ dup newloc !              \ default to current loc
    cave& dup 2 + swap 1+ c@ 0      \ get # of links and link 0 ( link-addr n 0 )
    false >r                        \ hit flag
    begin                           \ ( link-addr n 0 )
        2dup 0= swap 0> and while   \ no move and more links?
        drop 1- swap 4 + swap       \ ( next-link n-1 )
        over cave-link              \ ( next-link n-1 d' v c' )
        \ TODO ." link " .s CR
        \ verb matches motion or 1, or already hit?
        swap dup 1 = swap motion @ = or r> or if        \ cave-addr n-1 d' c'
            true >r try-move        \ cave-addr n-1 d' f
            ." tried " .s CR
            0= if drop 0 then       \ cave-addr n-1 d'|0
        else
            false >r 2drop 0        \ cave-addr n-1 0
        then
    repeat
    r> drop -rot 2drop              \ d'|0

    ?dup if
        unpack case                 \ dest dt
            0 of newloc ! endof
            1 of move-special endof
            2 of speak-message endof
        endcase
        else bad-move
    then
;

: go-back ( -- )
    ." go-back TODO"
;

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

: do-object ( -- )
    object @
    dup fixed{ c}@ loc @ = over is-here or if       \ is object here?
        transitive-verb else

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

        ." I see no " last-nonverb-cstr 2@ type ."  here." CR
    then
;

: turn
    \ TODO
\	if (newloc < 9 && newloc != 0 && closing) {
\		rspeak(130);
\		newloc = loc;
\		if (!panic)
\			clock2 = 15;
\		panic = 1;
\	}

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
        newloc @ loc !

\		/* check for death */
\		if (loc == 0) {
\			death();
\			return;
\		}
\		/* check for forced move */
\		if (forced(loc)) {
\			describe();
\			domove();
\			return;
\		}
\		/* check for wandering in dark */
\		if (wzdark && dark() && pct(35)) {
\			rspeak(23);
\			oldloc2 = loc;
\			death();
\			return;
\		}

        describe

        is-dark invert if
            true loc @ visited{ c}!
            describe-items
        then

    then

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

    ." loc " loc @ . ."  verb " verb @ . ."  obj " object @ . ."  motion " motion @ . CR

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
