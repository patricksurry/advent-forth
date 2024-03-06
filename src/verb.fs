\ 1-indexed arrays for action handlers up to 'LOAD = 33

create obj-act&  34 cells 0,n
create just-act& 34 cells 0,n
create act-msg{  34 0,n

: set-actions ( action obj-act just-act msg -- )
    >r rot dup >r cells dup >r      \ obj-act just-act 2*act   R: msg act 2*act
    just-act& + ! r> obj-act& + !
    2r> act-msg{ c}!
;

\ verb.c:actspk
: act-speak ( -- )
    verb @ dup 1 < over 31 > or if
        39 bug then

    act-msg{ c}@ ?dup if speak-message then
;

: say-nothing 54 speak-message ;

\ verb.c:needobj
: need-obj last-verb-cstr 2@ type ."  what?" CR ;

\ handle verbs with or without object

\ verb.c:von
: verb-on
    'LAMP is-here if
        limit @ 0 < if
            184 speak-message
        else
            1 'LAMP prop{ b}!
            39 speak-message
            wzdark @ if
                0 wzdark !
                describe
            then
        then
    else
        act-speak
    then
;

\ verb.c:voff
: verb-off
    'LAMP is-here if
        0 'LAMP prop{ b}!
        40 speak-message
    else
        act-speak
    then
;

\ verb.c:vpour
: verb-pour
    object @
    dup 'BOTTLE = over 0= or if
        drop bottle-liquid dup object !
    then
    dup 0= if
        drop need-obj exit
    then
    dup is-toting invert if
        drop act-speak exit
    then
    dup 'OIL <> over 'WATER <> and if
        drop 78 speak-message exit
    then
    1 'BOTTLE prop{ b}!
    0 over place{ c}!

    'PLANT is-at if
        dup 'WATER <> if
            112 speak-message
        else
            'PLANT dup prop{ b}@ swap over 1+ speak-item
            2 + 6 mod dup 'PLANT prop{ b}!
            2/ 'PLANT2 prop{ b}@
            describe
        then
    else
        'DOOR is-at if
            dup 'OIL = 1 and dup
            'DOOR prop{ b}!
            113 +
        else
            77
        then
        speak-message
    then
    drop
;

\ verb.c:vblast
: verb-blast \ TODO
\	if (prop[ROD2] < 0 || !closed)
\		actspk(verb);
\	else {
\		bonus = 133;
\		if (loc == 115)
\			bonus = 134;
\		if (here(ROD2))
\			bonus = 135;
\		rspeak(bonus);
\		normend();
\	}
;

\ transitive verbs

: obj-rub
    object @ 'LAMP = if act-speak else 76 speak-message then
;

\ verb.c:vtake
: obj-take
    object @
    dup is-toting if
        act-speak
    then

    \ special case objects and fixed objects
    25
    over 'PLANT = 'PLANT prop{ b}@ 0 <= and if
        drop 115
    then
    over 'BEAR = 'BEAR prop{ b}@ 1 = and if
        drop 169
    then
    over 'CHAIN = 'BEAR prop{ b}@ 0<> and if
        drop 170
    then
    over fixed{ c}@ if
        speak-message drop exit
    else
        drop
    then                        \ ( object )

\ TODO
\	   special case for liquids
\
\	if (object == WATER || object == OIL) {
\		if (!here(BOTTLE) || liq() != object) {
\			object = BOTTLE;
\			if (toting(BOTTLE) && prop[BOTTLE] == 1) {
\				vfill();
\				return;
\			}
\			if (prop[BOTTLE] != 1)
\				msg = 105;
\			if (!toting(BOTTLE))
\				msg = 104;
\			rspeak(msg);
\			return;
\		}
\		object = BOTTLE;
\	}
\	if (holding >= 7) {
\		rspeak(92);
\		return;
\	}
\	/*
\	   special case for bird.
\	*/
\	if (object == BIRD && prop[BIRD] == 0) {
\		if (toting(ROD)) {
\			rspeak(26);
\			return;
\		}
\		if (!toting(CAGE)) {
\			rspeak(27);
\			return;
\		}
\		prop[BIRD] = 1;
\	}
\	if ((object == BIRD || object == CAGE) && prop[BIRD] != 0)
\		carry((BIRD + CAGE) - object, loc);

	\ ( object )
	loc @ carry-item

\	/*
\	   handle liquid in bottle
\	*/
\	i = liq();
\	if (object == BOTTLE && i != 0)
\		place[i] = NOWHERE;

 	54 speak-message
;

\ verb.c:vdrop
: obj-drop
    object @

    \ check for dynamite
    'ROD2 is-toting over 'ROD = and over is-toting invert and if
        drop 'ROD2 dup object !
    then

    dup is-toting invert if
        act-speak exit
    then

    \ TODO
\	\ snake and bird
\	if (object == BIRD && here(SNAKE)) {
\		rspeak(30);
\		if (closed)
\			dwarfend();
\		dstroy(SNAKE);
\		prop[SNAKE] = -1;
\	}
\	\ coins and vending machine
\	else if (object == COINS && here(VEND)) {
\		dstroy(COINS);
\		drop(BATTERIES, loc);
\		pspeak(BATTERIES, 0);
\		return;
\	}
\	\ bird and dragon (ouch!!)
\	else if (object == BIRD && at(DRAGON) && prop[DRAGON] == 0) {
\		rspeak(154);
\		dstroy(BIRD);
\		prop[BIRD] = 0;
\		if (place[SNAKE] != 0)
\			++tally2;
\		return;
\	}
\	\ Bear and troll
\	if (object == BEAR && at(TROLL)) {
\		rspeak(163);
\		move(TROLL, 0);
\		move((TROLL + MAXOBJ), 0);
\		move(TROLL2, 117);
\		move((TROLL2 + MAXOBJ), 122);
\		juggle(CHASM);
\		prop[TROLL] = 2;
\	}
\	\ vase
\	else if (object == VASE) {
\		if (loc == 96)
\			rspeak(54);
\		else {
\			prop[VASE] = at(PILLOW) ? 0 : 2;
\			pspeak(VASE, prop[VASE] + 1);
\			if (prop[VASE] != 0)
\				fixed[VASE] = -1;
\		}
\	}
\	\ handle liquid and bottle
\	i = liq();
\	if (i == object)
\		object = BOTTLE;
\	if (object == BOTTLE && i != 0)
\		place[i] = 0;
\	\  handle bird and cage
\	if (object == CAGE && prop[BIRD] != 0)
\		drop(BIRD, loc);
\	if (object == BIRD)
\		prop[BIRD] = 0;

    loc @ drop-item
;

\ verb.c:vopen
: obj-open
    object @ 33                         \ ( obj msg )

\ TODO
\	case CLAM:
\	case OYSTER:
\		oyclam = (object == OYSTER ? 1 : 0);
\		if (verb == LOCK)
\			msg = 61;
\		else if (!toting(TRIDENT))
\			msg = 122 + oyclam;
\		else if (toting(object))
\			msg = 120 + oyclam;
\		else {
\			msg = 124 + oyclam;
\			dstroy(CLAM);
\			drop(OYSTER, loc);
\			drop(PEARL, 105);
\		}
\		break;

    over 'DOOR = if
        drop
        'DOOR prop{ b}@ 1 = if
            54 else 111
        then
    then
    over 'CAGE = if drop 32 then
    over 'KEYS = if drop 55 then

\ TODO
\	case CHAIN:
\		if (!here(KEYS))
\			msg = 31;
\		else if (verb == LOCK) {
\			if (prop[CHAIN] != 0)
\				msg = 34;
\			else if (loc != 130)
\				msg = 173;
\			else {
\				prop[CHAIN] = 2;
\				if (toting(CHAIN))
\					drop(CHAIN, loc);
\				fixed[CHAIN] = -1;
\				msg = 172;
\			}
\		} else {
\			if (prop[BEAR] == 0)
\				msg = 41;
\			else if (prop[CHAIN] == 0)
\				msg = 37;
\			else {
\				prop[CHAIN] = 0;
\				fixed[CHAIN] = 0;
\				if (prop[BEAR] != 3)
\					prop[BEAR] = 2;
\				fixed[BEAR] = 2 - prop[BEAR];
\				msg = 171;
\			}
\		}
\		break;
\	case GRATE:
\		if (!here(KEYS))
\			msg = 31;
\		else if (closing) {
\			if (!panic) {
\				clock2 = 15;
\				++panic;
\			}
\			msg = 130;
\		} else {
\			msg = 34 + prop[GRATE];
\			prop[GRATE] = (verb == LOCK ? 0 : 1);
\			msg += 2 * prop[GRATE];
\		}
\		break;
\	}

    nip speak-message
;

\ verb.c:vsay
: obj-say
    ." Okay." CR last-nonverb-cstr 2@ type CR
;

\ verb.c:vwave
: obj-wave
\ TODO
\	if (!toting(object) && (object != ROD || !toting(ROD2)))
\		rspeak(29);
\	else if (object != ROD || !at(FISSURE) || !toting(object) || closing)
\		actspk(verb);
\	else {
\		prop[FISSURE] = 1 - prop[FISSURE];
\		pspeak(FISSURE, 2 - prop[FISSURE]);
\	}
;

\ verb.c:vkill
: obj-kill
\ TODO
\	int msg;
\	int i;
\
\	switch (object) {
\	case BIRD:
\		if (closed)
\			msg = 137;
\		else {
\			dstroy(BIRD);
\			prop[BIRD] = 0;
\			if (place[SNAKE] == 19)
\				++tally2;
\			msg = 45;
\		}
\		break;
\	case 0:
\		msg = 44;
\		break;
\	case CLAM:
\	case OYSTER:
\		msg = 150;
\		break;
\	case SNAKE:
\		msg = 46;
\		break;
\	case DWARF:
\		if (closed)
\			dwarfend();
\		msg = 49;
\		break;
\	case TROLL:
\		msg = 157;
\		break;
\	case BEAR:
\		msg = 165 + (prop[BEAR] + 1) / 2;
\		break;
\	case DRAGON:
\		if (prop[DRAGON] != 0) {
\			msg = 167;
\			break;
\		}
\		if (!yes(49, 0, 0))
\			return;
\		pspeak(DRAGON, 1);
\		prop[DRAGON] = 2;
\		prop[RUG] = 0;
\		move((DRAGON + MAXOBJ), -1);
\		move((RUG + MAXOBJ), 0);
\		move(DRAGON, 120);
\		move(RUG, 120);
\		for (i = 1; i < MAXOBJ; ++i)
\			if (place[i] == 119 || place[i] == 121)
\				move(i, 120);
\		newloc = 120;
\		return;
\	default:
\		actspk(verb);
\		return;
\	}
\	rspeak(msg);
;

\ verb.c:veat
: obj-eat
    object @
    'FOOD over = if
        'FOOD destroy-item
        72 speak-message exit
    then

    0
    over 'BIRD = or
    over 'SNAKE = or
    over 'CLAM = or
	over 'OYSTER = or
	over 'DWARF = or
	over 'DRAGON = or
	over 'TROLL = or
	over 'BEAR = or

	if 71 speak-message
	else act-speak
	then
;

\ verb.c:vdrink
: obj-drink
    object @ 'WATER <> if
        110 speak-message
    else
        'WATER bottle-liquid <> 'BOTTLE is-here invert if
            act-speak
        else
            1 'BOTTLE prop{ b}!
            0 'WATER place{ c}!
            74 speak-message
        then
    then
;

\ verb.c:vthrow
: obj-throw
\ TODO
\	int msg;
\	int i;
\
\	if (toting(ROD2) && object == ROD && !toting(ROD))
\		object = ROD2;
\	if (!toting(object)) {
\		actspk(verb);
\		return;
\	}
\	/*
\	   treasure to troll
\	*/
\	if (at(TROLL) && object >= 50 && object < MAXOBJ) {
\		rspeak(159);
\		drop(object, 0);
\		move(TROLL, 0);
\		move((TROLL + MAXOBJ), 0);
\		drop(TROLL2, 117);
\		drop((TROLL2 + MAXOBJ), 122);
\		juggle(CHASM);
\		return;
\	}
\	/*
\	   feed the bears...
\	*/
\	if (object == FOOD && here(BEAR)) {
\		object = BEAR;
\		vfeed();
\		return;
\	}
\	/*
\	   if not axe, same as drop...
\	*/
\	if (object != AXE) {
\		vdrop();
\		return;
\	}
\	/*
\	   AXE is THROWN
\	*/
\	/*
\	   at a dwarf...
\	*/
\	if ((i = dcheck())) {
\		msg = 48;
\		if (pct(33)) {
\			dseen[i] = dloc[i] = 0;
\			msg = 47;
\			++dkill;
\			if (dkill == 1)
\				msg = 149;
\		}
\	}
\	/*
\	   at a dragon...
\	*/
\	else if (at(DRAGON) && prop[DRAGON] == 0)
\		msg = 152;
\	/*
\	   at the troll...
\	*/
\	else if (at(TROLL))
\		msg = 158;
\	/*
\	   at the bear...
\	*/
\	else if (here(BEAR) && prop[BEAR] == 0) {
\		rspeak(164);
\		drop(AXE, loc);
\		fixed[AXE] = -1;
\		prop[AXE] = 1;
\		juggle(BEAR);
\		return;
\	}
\	/*
\	   otherwise it is an attack
\	*/
\	else {
\		verb = KILL;
\		object = 0;
\		itverb();
\		return;
\	}
\	/*
\	   handle the left over axe...
\	*/
\	rspeak(msg);
\	drop(AXE, loc);
\	describe();
;

\ verb.c:vfeed
: obj-feed
\ TODO
\	int msg;
\
\	switch (object) {
\	case BIRD:
\		msg = 100;
\		break;
\	case DWARF:
\		if (!here(FOOD)) {
\			actspk(verb);
\			return;
\		}
\		++dflag;
\		msg = 103;
\		break;
\	case BEAR:
\		if (!here(FOOD)) {
\			if (prop[BEAR] == 0)
\				msg = 102;
\			else if (prop[BEAR] == 3)
\				msg = 110;
\			else {
\				actspk(verb);
\				return;
\			}
\			break;
\		}
\		dstroy(FOOD);
\		prop[BEAR] = 1;
\		fixed[AXE] = 0;
\		prop[AXE] = 0;
\		msg = 168;
\		break;
\	case DRAGON:
\		msg = (prop[DRAGON] != 0 ? 110 : 102);
\		break;
\	case TROLL:
\		msg = 182;
\		break;
\	case SNAKE:
\		if (closed || !here(BIRD)) {
\			msg = 102;
\			break;
\		}
\		msg = 101;
\		dstroy(BIRD);
\		prop[BIRD] = 0;
\		++tally2;
\		break;
\	default:
\		msg = 14;
\	}
\	rspeak(msg);
;

\ INVENTORY, FIND etc.
\ verb.c:vfind
: obj-find
\ TODO
\	int msg;
\
\	if (toting(object))
\		msg = 24;
\	else if (closed)
\		msg = 138;
\	else if (dcheck() && dflag >= 2 && object == DWARF)
\		msg = 94;
\	else if (at(object) || (liq() == object && here(BOTTLE)) || object == liqloc(loc))
\		msg = 94;
\	else {
\		actspk(verb);
\		return;
\	}
\	rspeak(msg);
;

\ verb.c:vfill
: obj-fill
\ TODO
\	int msg;
\	int i;
\
\	switch (object) {
\	case BOTTLE:
\		if (liq() != 0)
\			msg = 105;
\		else if (liqloc(loc) == 0)
\			msg = 106;
\		else {
\			prop[BOTTLE] = cond[loc] & WATOIL;
\			i = liq();
\			if (toting(BOTTLE))
\				place[i] = NOWHERE;
\			msg = (i == OIL ? 108 : 107);
\		}
\		break;
\	case VASE:
\		if (liqloc(loc) == 0) {
\			msg = 144;
\			break;
\		}
\		if (!toting(VASE)) {
\			msg = 29;
\			break;
\		}
\		rspeak(145);
\		vdrop();
\		return;
\	default:
\		msg = 29;
\	}
\	rspeak(msg);
;

\ verb.c:vread
: obj-read
    is-dark if
        ." I see no " last-nonverb-cstr type ."  here." CR
    	exit
    then

    object @ 'OYSTER = if
	    'OYSTER is-toting closed and if
      		192 193 54 yes-no drop exit
        then
    then

	0 object @ case
	    'MAGAZINE of drop 190 endof
	    'TABLET of drop 196 endof
        'MESSAGE of drop 191 endof
    endcase
	?dup if speak-message
	else act-speak
	then
;

\ verb.c:vbreak
: obj-break
\ TODO
\	int msg;
\
\	if (object == MIRROR) {
\		msg = 148;
\		if (closed) {
\			rspeak(197);
\			dwarfend();
\		}
\	} else if (object == VASE && prop[VASE] == 0) {
\		msg = 198;
\		if (toting(VASE))
\			drop(VASE, loc);
\		prop[VASE] = 2;
\		fixed[VASE] = -1;
\	} else {
\		actspk(verb);
\		return;
\	}
\	rspeak(msg);
;

\ verb.c:vwake
: obj-wake
\ TODO
\	if (object != DWARF || !closed)
\		actspk(verb);
\	else {
\		rspeak(199);
\		dwarfend();
\	}
;

\ intransitive verb handlers

\ itverb.c:ivtake
: just-take
\ TODO
\	OBJ_T anobj, item;
\
\	anobj = 0;
\	for (item = 1; item < MAXOBJ; ++item) {
\		if (place[item] == loc) {
\			if (anobj != 0) {
\				needobj();
\				return;
\			}
\			anobj = item;
\		}
\	}
\	if (anobj == 0 || (dcheck() && dflag >= 2)) {
\		needobj();
\		return;
\	}
\	object = anobj;
\	vtake();
;

\ itverb.c:ivopen
: just-open \ TODO
\	if (here(CLAM))
\		object = CLAM;
\	if (here(OYSTER))
\		object = OYSTER;
\	if (at(DOOR))
\		object = DOOR;
\	if (at(GRATE))
\		object = GRATE;
\	if (here(CHAIN)) {
\		if (object != 0) {
\			needobj();
\			return;
\		}
\		object = CHAIN;
\	}
\	if (object == 0) {
\		rspeak(28);
\		return;
\	}
\	vopen();
;

\ track count and latest obj where f is true
\ itverb.c:addobj
: add-obj ( last count obj f -- new count )
    if -rot nip 1+ else drop then
;

\ itverb.c:ivkill
: just-kill
    \ check exactly one target is here
    0 0                             \ obj count

    \ TODO
	\ if (dcheck() && dflag >= 2)
	\	object = DWARF;

	'SNAKE dup is-here add-obj
	'DRAGON dup is-at over prop{ b}@ 0= and add-obj
	'TROLL dup is-at add-obj
    'BEAR dup is-here over prop{ b}@ 0= and add-obj

    dup 1 > if                  \ more than one match?
        2drop need-obj exit
    then
    dup 1 = if
        drop object ! obj-kill exit
    then

    'BIRD dup is-here 'THROW verb @ <> and add-obj
    'CLAM dup is-here 'OYSTER is-here or add-obj
    1 > if
        drop need-obj exit
    then
    object ! obj-kill
;

\ itverb.c:iveat
: just-eat
    'FOOD is-here if
        'FOOD object !
        obj-eat
    else
        need-obj
    then
;

\ itverb.c:ivdrink
: just-drink
\ TODO
\	if (liqloc(loc) != WATER && (liq() != WATER || !here(BOTTLE)))
\		needobj();
\	else {
\		object = WATER;
\		vdrink();
\	}
;

\ itverb.c:ivquit
: just-quit
\ TODO
\	if ((gaveup = yes(22, 54, 54)))
\		normend();
;

\ itverb.c:ivfill
: just-fill
\ TODO
\	if (!here(BOTTLE))
\		needobj();
\	else {
\		object = BOTTLE;
\		vfill();
\	}
;

: just-score \ TODO
;

\ fee fie foe fum
\ itverb.c:ivfoo
: just-foo \ TODO
\	int k, msg;
\
\	k = vocab(word1, 3000);
\	msg = 42;
\	if (foobar != 1 - k) {
\		if (foobar != 0)
\			msg = 151;
\		rspeak(msg);
\		return;
\	}
\
\	foobar = k;
\	if (k != 4)
\		return;
\
\	foobar = 0;
\	if (place[EGGS] == 92 || (toting(EGGS) && loc == 92)) {
\		rspeak(msg);
\		return;
\	}
\
\	if (place[EGGS] == 0 && place[TROLL] == 0 && prop[TROLL] == 0)
\		prop[TROLL] = 1;
\	if (here(EGGS))
\		k = 1;
\	else if (loc == 92)
\		k = 0;
\	else
\		k = 2;
\	move(EGGS, 92);
\	pspeak(EGGS, k);
\	return;
;

\ itverb.c:ivread
: just-read
\	if (here(MAGAZINE))
\		object = MAGAZINE;
\	if (here(TABLET))
\		object = object*100 + TABLET;
\	if (here(MESSAGE))
\		object = object*100 + MESSAGE;
\	if (object > 100 || object == 0 || dark()) {
\		needobj();
\		return;
\	}
\	vread();
;

\ itverb.c:inventory
: just-inventory
	98             \ message
	65 1 do
	    i is-toting i 'BEAR <> and if
	        if 99 speak-message then
			i -1 speak-item
			0
	    then
	loop
    'BEAR is-toting if
        drop 141
    then
    if speak-message then
;

: just-suspend  \ TODO   saveflg = 1;
;

: just-load \ TODO
;

'CALM      ' act-speak     ' need-obj       14  set-actions
'WALK      ' act-speak     ' act-speak      43  set-actions
'QUIT      ' act-speak     ' just-quit      13  set-actions
'SCORE     ' act-speak     ' just-score     13  set-actions
'FOO       ' act-speak     ' just-foo       147 set-actions
'BRIEF     ' act-speak     0                155 set-actions
'SUSPEND   ' act-speak     ' just-suspend   13  set-actions
'LOAD      0               ' just-load      0   set-actions
'HOURS     ' act-speak     0                13  set-actions
'LOG       ' act-speak     0                0   set-actions
'TAKE      ' obj-take      ' just-take      24  set-actions
'DROP      ' obj-drop      ' need-obj       29  set-actions
'OPEN      ' obj-open      ' just-open      33  set-actions
'LOCK      ' obj-open      ' just-open      33  set-actions
'SAY       ' obj-say       ' need-obj       0   set-actions
'NOTHING   ' say-nothing   ' say-nothing    0   set-actions
'ON        ' verb-on       ' verb-on        38  set-actions
'OFF       ' verb-off      ' verb-off       38  set-actions
'WAVE      ' obj-wave      ' need-obj       42  set-actions
'KILL      ' obj-kill      ' just-kill      110 set-actions
'POUR      ' verb-pour     ' verb-pour      29  set-actions
'EAT       ' obj-eat       ' just-eat       110 set-actions
'DRINK     ' obj-drink     ' just-drink     73  set-actions
'RUB       ' obj-rub       ' need-obj       75  set-actions
'THROW     ' obj-throw     ' need-obj       29  set-actions
'FEED      ' obj-feed      ' need-obj       174 set-actions
'FIND      ' obj-find      ' need-obj       59  set-actions
'INVENTORY ' obj-find      ' just-inventory 59  set-actions
'FILL      ' obj-fill      ' just-fill      109 set-actions
'READ      ' obj-read      ' just-read      195 set-actions
'BLAST     ' verb-blast    ' verb-blast     67  set-actions
'BREAK     ' obj-break     ' need-obj       146 set-actions
'WAKE      ' obj-wake      ' need-obj       110 set-actions


\ turn.c:trverb,trobj
: transitive-verb ( -- )  \ act on an object
    verb @ ?dup if
        ." trverb " .s CR
        cells obj-act& + @
        ?dup 0= if
            ." This verb is not implemented yet." CR
        else
            execute
        then
    else
        ." What do you want to do with the "
        last-nonverb-cstr 2@ type ." ?" CR
    then
;

\ itverb.c:itverb
: intransitive-verb ( -- )  \ just act
    verb @ ?dup if
        cells just-act& + @
        ?dup 0= if
            ." This intransitive not implemented yet." CR
        else
            execute
        then
    then
;
