
\ turn.c:score
: score
\ TODO
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
;


\ turn.c:normend
: normal-end
    score
    abort   \ TODO clean exit?
;


\ player's incarnation has passed on
\ turn.c:death
: death

    closing @ 0= if
        numdie @ dup 2* dup 81 + swap 82 + 64 yes-no 0=       ( numdie ?no )
        swap 1+ dup numdie ! MAXDIE >= or if
            normal-end
        then
        0 'WATER place{ c}!
        0 'OIL place{ c}!
        'LAMP is-toting if
            0 'LAMP prop{ b}!
        then
        MAXOBJ 1- 1 do      \ original source uses i = MAXOBJ - j, unclear why
            i is-toting if
                i dup 'LAMP = if 1 else oldloc2 @ then drop-item
            then
        loop
        3 newloc !
        loc @ oldloc !
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

\ check for presence of dwarves..
\ database.c:dcheck
: dwarf-check ( -- i )
    0
\ TODO
\ unsigned char dcheck(void) {
\   unsigned char i;
\
\   for (i = 1; i < (DWARFMAX - 1); ++i) {
\     if (dloc[i] == loc)
\       return i;
\   }
\   return 0;
;

\ TODO
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
\ /*
\ 	pirate stuff
\ */
\ turn.c:dopirate
\ void dopirate(void)
\ {
\ 	int j, k;
\
\ 	if (newloc == chloc || prop[CHEST] >= 0)
\ 		return;
\
\ 	k = 0;
\ 	for (j = 50; j <= MAXTRS; ++j)
\ 		if (j != PYRAMID || (newloc != place[PYRAMID] && newloc != place[EMERALD])) {
\ 			if (toting(j))
\ 				goto stealit;
\ 			if (here(j))
\ 				++k;
\ 		}
\ 	if (tally == tally2 + 1 && k == 0 && place[CHEST] == 0 && here(LAMP) && prop[LAMP] == 1) {
\ 		rspeak(186);
\ 		move(CHEST, chloc);
\ 		move(MESSAGE, chloc2);
\ 		dloc[6] = chloc;
\ 		odloc[6] = chloc;
\ 		dseen[6] = 0;
\ 		return;
\ 	}
\ 	if (odloc[6] != dloc[6] && pct(20)) {
\ 		rspeak(127);
\ 		return;
\ 	}
\ 	return;
\
\ stealit:
\
\ 	rspeak(128);
\ 	if (place[MESSAGE] == 0)
\ 		move(CHEST, chloc);
\ 	move(MESSAGE, chloc2);
\ 	for (j = 50; j <= MAXTRS; ++j) {
\ 		if (j == PYRAMID && (newloc == place[PYRAMID] || newloc == place[EMERALD]))
\ 			continue;
\ 		if (at(j) && fixed[j] == 0)
\ 			carry(j, newloc);
\ 		if (toting(j))
\ 			drop(j, chloc);
\ 	}
\ 	dloc[6] = chloc;
\ 	odloc[6] = chloc;
\ 	dseen[6] = 0;
\ }
