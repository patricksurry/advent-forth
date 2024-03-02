
: describe ( -- )               \ describe current location
    BEAR is-toting if
        141 speak-message then
    is-dark if
        16 speak-message else
        loc @ dup visited + c@ invert speak-location then
    33 loc @ = 25 pct and closing @ invert and if
        8 speak-message then
;

: do-move ( -- ) ; \ TODO
: do-object ( -- ) ; \ TODO
: intransitive-verb ( -- ) ; \ TODO

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
            1 loc @ visited + +!
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

\	if (dbugflg)
\		printf("loc = %d, verb = %d, object = %d, \
\		motion = %d\n",
\		       loc, verb, object, motion);

    motion ?dup if
        do-move else        \ execute player instructions
        object ?dup if
            do-object else
            intransitive-verb
        then
    then
;
