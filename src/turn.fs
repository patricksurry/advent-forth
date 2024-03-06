
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

\ return from whence we came!
\ turn.c:goback
: go-back ( -- )
    \ TODO
\ 	int kk, k2, want, temp;
\ 	struct trav strav[MAXTRAV];
\
\ 	if (forced(oldloc))
\ 		want = oldloc2;
\ 	else
\ 		want = oldloc;
\ 	oldloc2 = oldloc;
\ 	oldloc = loc;
\ 	k2 = 0;
\ 	if (want == loc) {
\ 		rspeak(91);
\ 		return;
\ 	}
\ 	copytrv(travel, strav);
\ 	for (kk = 0; travel[kk].tdest != -1; ++kk) {
\ 		if (!travel[kk].tcond && travel[kk].tdest == want) {
\ 			motion = travel[kk].tverb;
\ 			dotrav();
\ 			return;
\ 		}
\ 		if (!travel[kk].tcond) {
\ 			k2 = kk;
\ 			temp = travel[kk].tdest;
\ 			gettrav(temp);
\ 			if (forced(temp) && travel[0].tdest == want)
\ 				k2 = temp;
\ 			copytrv(strav, travel);
\ 		}
\ 	}
\ 	if (k2) {
\ 		motion = travel[k2].tverb;
\ 		dotrav();
\ 	} else
\ 		rspeak(140);
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

\ turn.c:turn
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



\ TODO
\ handle player's demise via waking up the dwarves...
\ turn.c:dwarfend
\ void dwarfend(void)
\ {
\ 	death();
\ 	normend();
\ }
\
\ normal end of game
\ turn.c:normend
\ void normend(void)
\ {
\ 	score();
\ 	exit(-1);
\ }
\
\ scoring
\ turn.c:score
\ void score(void)
\ {
\ 	int t, i, k, s;
\ 	s = t = k = 0;
\ 	for (i = 50; i <= MAXTRS; ++i) {
\ 		if (i == CHEST)
\ 			k = 14;
\ 		else if (i > CHEST)
\ 			k = 16;
\ 		else
\ 			k = 12;
\ 		if (prop[i] >= 0)
\ 			t += 2;
\ 		if (place[i] == 3 && prop[i] == 0)
\ 			t += k - 2;
\ 	}
\ 	printf("%-20s%d\n", "Treasures:", s = t);
\ 	t = (MAXDIE - numdie) * 10;
\ 	if (t)
\ 		printf("%-20s%d\n", "Survival:", t);
\ 	s += t;
\ 	if (!gaveup)
\ 		s += 4;
\ 	t = dflag ? 25 : 0;
\ 	if (t)
\ 		printf("%-20s%d\n", "Getting well in:", t);
\ 	s += t;
\ 	t = closing ? 25 : 0;
\ 	if (t)
\ 		printf("%-20s%d\n", "Masters section:", t);
\ 	s += t;
\ 	if (closed) {
\ 		if (bonus == 0)
\ 			t = 10;
\ 		else if (bonus == 135)
\ 			t = 25;
\ 		else if (bonus == 134)
\ 			t = 30;
\ 		else if (bonus == 133)
\ 			t = 45;
\ 		printf("%-20s%d\n", "Bonus:", t);
\ 		s += t;
\ 	}
\ 	if (place[MAGAZINE] == 108)
\ 		s += 1;
\ 	s += 2;
\ 	printf("%-20s%d\n", "Score:", s);
\ }
\
\ player's incarnation has passed on
\ turn.c:death
\ void death(void)
\ {
\ 	char yea, i, j;
\
\ 	if (!closing) {
\ 		yea = yes(81 + numdie * 2, 82 + numdie * 2, 54);
\ 		if (++numdie >= MAXDIE || !yea)
\ 			normend();
\ 		place[WATER] = 0;
\ 		place[OIL] = 0;
\ 		if (toting(LAMP))
\ 			prop[LAMP] = 0;
\ 		for (j = 1; j < MAXOBJ; ++j) {
\ 			i = MAXOBJ - j;
\ 			if (toting(i))
\ 				drop(i, i == LAMP ? 1 : oldloc2);
\ 		}
\ 		newloc = 3;
\ 		oldloc = loc;
\ 		return;
\ 	}
\ 	/*
\ 	   closing -- no resurrection...
\ 	*/
\ 	rspeak(131);
\ 	++numdie;
\ 	normend();
\ }


\ special time limit stuff...
\ turn.c:stimer
\ int stimer(void)
\ {
\ 	int i;
\
\ 	foobar = foobar > 0 ? -foobar : 0;
\ 	if (tally == 0 && loc >= 15 && loc != 33)
\ 		--clock1;
\ 	if (clock1 == 0) {
\ 		/*
\ 			start closing the cave
\ 		*/
\ 		prop[GRATE] = 0;
\ 		prop[FISSURE] = 0;
\ 		for (i = 1; i < DWARFMAX; ++i)
\ 			dseen[i] = 0;
\ 		move(TROLL, 0);
\ 		move((TROLL + MAXOBJ), 0);
\ 		move(TROLL2, 117);
\ 		move((TROLL2 + MAXOBJ), 122);
\ 		juggle(CHASM);
\ 		if (prop[BEAR] != 3)
\ 			dstroy(BEAR);
\ 		prop[CHAIN] = 0;
\ 		fixed[CHAIN] = 0;
\ 		prop[AXE] = 0;
\ 		fixed[AXE] = 0;
\ 		rspeak(129);
\ 		clock1 = -1;
\ 		closing = 1;
\ 		return 0;
\ 	}
\ 	if (clock1 < 0)
\ 		--clock2;
\ 	if (clock2 == 0) {
\ 		/*
\ 			set up storage room...
\ 			and close the cave...
\ 		*/
\ 		prop[BOTTLE] = put(BOTTLE, 115, 1);
\ 		prop[PLANT] = put(PLANT, 115, 0);
\ 		prop[OYSTER] = put(OYSTER, 115, 0);
\ 		prop[LAMP] = put(LAMP, 115, 0);
\ 		prop[ROD] = put(ROD, 115, 0);
\ 		prop[DWARF] = put(DWARF, 115, 0);
\ 		loc = 115;
\ 		oldloc = 115;
\ 		newloc = 115;
\ 		put(GRATE, 116, 0);
\ 		prop[SNAKE] = put(SNAKE, 116, 1);
\ 		prop[BIRD] = put(BIRD, 116, 1);
\ 		prop[CAGE] = put(CAGE, 116, 0);
\ 		prop[ROD2] = put(ROD2, 116, 0);
\ 		prop[PILLOW] = put(PILLOW, 116, 0);
\ 		prop[MIRROR] = put(MIRROR, 115, 0);
\ 		fixed[MIRROR] = 116;
\ 		for (i = 1; i < MAXOBJ; ++i) {
\ 			if (toting(i))
\ 				dstroy(i);
\ 		}
\ 		rspeak(132);
\ 		closed = 1;
\ 		return 1;
\ 	}
\ 	if (prop[LAMP] == 1)
\ 		--limit;
\ 	if (limit <= 30 && here(BATTERIES) && prop[BATTERIES] == 0 && here(LAMP)) {
\ 		rspeak(188);
\ 		prop[BATTERIES] = 1;
\ 		if (toting(BATTERIES))
\ 			drop(BATTERIES, loc);
\ 		limit += 2500;
\ 		lmwarn = 0;
\ 		return 0;
\ 	}
\ 	if (limit == 0) {
\ 		--limit;
\ 		prop[LAMP] = 0;
\ 		if (here(LAMP))
\ 			rspeak(184);
\ 		return 0;
\ 	}
\ 	if (limit < 0 && loc <= 8) {
\ 		rspeak(185);
\ 		gaveup = 1;
\ 		normend();
\ 	}
\ 	if (limit <= 30) {
\ 		if (lmwarn || !here(LAMP))
\ 			return 0;
\ 		lmwarn = 1;
\ 		i = 187;
\ 		if (place[BATTERIES] == 0)
\ 			i = 183;
\ 		if (prop[BATTERIES] == 1)
\ 			i = 189;
\ 		rspeak(i);
\ 		return 0;
\ 	}
\ 	return 0;
\ }
