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
