\ 1-indexed arrays for action handlers up to 'LOAD = 33

create obj-act&  34 cells 0,n
create just-act& 34 cells 0,n
create act-msg[] 34 0,n

: set-actions ( action obj-act just-act msg -- )
    >r rot dup >r cells dup >r      \ obj-act just-act 2*act   R: msg act 2*act
    just-act& + ! r> obj-act& + !
    2r> act-msg[] + c!
;

: act-speak ( -- )
    verb @ dup 1 < over 31 > or if
        39 bug then

    act-msg[] + c@ if speak-message then
;

: say-nothing 54 speak-message ;

\ handle verbs with or without object

: verb-on
    'LAMP is-here if
        limit @ 0 < if
            184 speak-message
        else
            1 'LAMP prop[] + c!
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

: verb-off
    'LAMP is-here if
        0 'LAMP prop[] + c!
        40 speak-message
    else
        act-speak
    then
;

: verb-pour \ TODO
;

: verb-blast \ TODO
;

\ transitive verbs

: obj-rub
    object @ 'LAMP = if act-speak else 76 speak-message then
;

: obj-take
    object @
    dup is-toting if
        act-speak
    then

    \ special case objects and fixed objects
    25
    over 'PLANT = 'PLANT prop[] + c@ 0 <= and if
        drop 115
    then
    over 'BEAR = 'BEAR prop[] + c@ 1 = and if
        drop 169
    then
    over 'CHAIN = 'BEAR prop[] + c@ 0<> and if
        drop 170
    then
    over fixed[] + c@ if
        speak-message drop exit
    else
        drop
    then
            \ ( object )
\	/*
\	   special case for liquids
\	*/
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

	( object ) loc @ carry-item

\	/*
\	   handle liquid in bottle
\	*/
\	i = liq();
\	if (object == BOTTLE && i != 0)
\		place[i] = NOWHERE;

 	54 speak-message
;

: obj-drop \ TODO
;

: obj-open \ TODO
;

: obj-say \ TODO
;

: obj-wave \ TODO
;

: obj-kill \ TODO
;

: obj-eat \ TODO
;

: obj-drink \ TODO
;

: obj-throw \ TODO
;

: obj-feed \ TODO
;

: obj-find \ TODO
;

: obj-fill \ TODO
;

: obj-read \ TODO
;

: obj-break \ TODO
;

: obj-wake \ TODO
;

\ intransitive verb handlers

: need-obj
    last-verb-cstr 2@ type ."  what?" CR
;

: just-take \ TODO
;

: just-open \ TODO
;

: just-kill \ TODO
;

: just-eat \ TODO
;

: just-drink \ TODO
;

: just-quit \ TODO
;

: just-fill \ TODO
;

: just-score \ TODO
;

: just-foo \ TODO
;

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

: just-suspend  \ TODO   		saveflg = 1;
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
'READ      ' obj-read      0                195 set-actions
'BLAST     ' verb-blast    ' verb-blast     67  set-actions
'BREAK     ' obj-break     ' need-obj       146 set-actions
'WAKE      ' obj-wake      ' need-obj       110 set-actions


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
