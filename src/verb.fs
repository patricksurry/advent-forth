\ 1-indexed arrays for action handlers up to 'LOAD = 33

create obj-act&  34 cells 0,n
create just-act& 34 cells 0,n
create act-msg{  34 0,n

: set-actions ( action obj-act just-act msg -- )
    >r rot dup >r cells dup >r
    ( obj-act just-act 2*act   R: msg act 2*act )
    just-act& + ! r> obj-act& + !
    2r> act-msg{ c}!
;


\ turn.c:trverb,trobj
: transitive-verb ( -- )  \ act on an object
    verb @ ?dup if
        cells obj-act& + @
        ( xt )
        ?dup 0= if
            ." This verb is not implemented yet." CR
        else
            object @ swap execute
        then
    else
        ." What do you want to do with the " say-last-thing ." ?" CR
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

\ verb.c:actspk
: just-speak ( -- )
    verb @ dup 1 < over 31 > or if
        39 bug then

    act-msg{ c}@ ?dup if speak-message then
;
: obj-speak ( obj -- )
    drop just-speak
;

: just-nothing 54 speak-message ;
: obj-nothing drop just-nothing ;

\ verb.c:needobj
: need-obj ( -- )
    last-verb-cstr 2@ type ."  what?" CR
;

\ handle verbs with or without object

\ verb.c:von
: just-on
    'LAMP is-here if
        limit @ 0 < if
            184 speak-message
        else
            1 'LAMP prop{}!
            39 speak-message
            wzdark @ if
                0 wzdark !
                describe-location
            then
        then
    else
        just-speak
    then
;
: obj-on drop just-on ;

\ verb.c:voff
: just-off
    'LAMP is-here if
        0 'LAMP prop{}!
        40 speak-message
    else
        just-speak
    then
;
: obj-off drop just-off ;

\ verb.c:vpour
: obj-pour ( obj -- )
    dup 'BOTTLE = over 0= or if
        drop bottle-liquid dup object !
    then
    dup 0= if
        drop need-obj exit
    then
    dup is-toting 0= if
        obj-speak exit
    then
    dup 'OIL <> over 'WATER <> and if
        drop 78 speak-message exit
    then
    1 'BOTTLE prop{}!
    0 over place{ c}!

    'PLANT is-at if
        dup 'WATER <> if
            112 speak-message
        else
            'PLANT dup prop{}@ swap over 1+ speak-item
            2 + 6 mod dup 'PLANT prop{}!
            2/ 'PLANT2 prop{}@
            describe-location
        then
    else
        'DOOR is-at if
            dup 'OIL = 1 and dup
            'DOOR prop{}!
            113 +
        else
            77
        then
        speak-message
    then
    drop
;
: just-pour object @ obj-pour ;

\ verb.c:vblast
: just-blast
    'ROD2 prop{}@ 0< closed @ 0= or if
        just-speak
    else
        133
        115 loc@ = if drop 134 then
        'ROD2 is-here if drop 135 then
        dup speak-message
        bonus !
        normal-end
    then
;
: obj-blast drop just-blast ;

\ transitive verbs

: obj-rub ( obj -- )
    'LAMP = if
        just-speak
    else
        76 speak-message
    then
;

\ verb.c:vdrop
: obj-drop ( obj -- )

    \ check for dynamite
    'ROD2 is-toting over 'ROD = and over is-toting 0= and if
        drop 'ROD2 dup object !
    then

    dup is-toting 0= if
        obj-speak exit
    then

 	\ snake and bird
    dup 'BIRD = 'SNAKE is-here and if
        30 speak-message
        closed @ if
            dwarf-end
        then
        'SNAKE destroy-item
        -1 'SNAKE prop{}!
    then

    \ coins and vending machine
    dup 'COINS = 'VEND is-here and if
        destroy-item
    	'BATTERIES dup loc@ drop-item
    	speak-item
    	exit
    then

    dup 'BIRD = 'DRAGON is-at and 'DRAGON prop{}@ 0= and if
        154 speak-message
        'BIRD destroy-item
        0 'BIRD prop{}!
        'SNAKE place{ c}@ if
            1 tally2 +!
        then
        exit
    then

    \ Bear and troll
    dup 'BEAR = 'TROLL is-at and if
       163 speak-message
        'TROLL dup 0 move-item
        MAXOBJ + 0 move-item
        'TROLL2 dup 117 move-item
        MAXOBJ + 122 move-item
        'CHASM juggle-item
        2 'TROLL prop{}!
    then

    \ vase
    dup 'VASE = if
        96 loc@ = if
            54 speak-message
        else
            'PILLOW is-at if 0 else 2 then
            'VASE prop{}!
            'VASE dup prop{}@ dup -rot 1+
            ( prop[VASE] VASE prop[VASE]+1 )
            speak-message
            if
                NOWHERE 'VASE fixed{ c}!
            then
        then
    then

	\ handle liquid and bottle
	bottle-liquid
	( obj liq )

	2dup = if
	   'BOTTLE object !
	then
	over 'BOTTLE = over and if
	   0 swap place{ c}!
	else
	   drop
    then
    ( obj )

	\  handle bird and cage
	dup 'CAGE = 'BIRD prop{}@ and if
	   'BIRD loc@ drop-item
	then
	dup 'BIRD = if
	   0 'BIRD prop{}!
    then

    loc@ drop-item
;

\ verb.c:vopen
: obj-open ( obj -- )
    33
    ( obj msg )

    over dup 'CLAM = swap 'OYSTER = or if
        drop dup 'OYSTER =
        ( obj oyclam )
        verb @ 'LOCK = if
            drop 61
        else 'TRIDENT is-toting 0= if
            122 +
        else over is-toting if
            120 +
        else
            124 +
            'CLAM destroy-item
            'OYSTER loc@ drop-item
            'PEARL 105 drop-item
        then then then
    then

    over 'DOOR = if
        drop
        'DOOR prop{}@ 1 = if
            54 else 111
        then
    then

    over 'CAGE = if drop 32 then

    over 'KEYS = if drop 55 then

    over 'CHAIN = if
        drop
        'KEYS not-here if
            31
        else 'LOCK verb @ = if
            'CHAIN prop{}@ if
                34
            else 130 loc@ <> if
                173
            else
                172
                2 'CHAIN prop{}!
                'CHAIN is-toting if
                    'CHAIN loc@ drop-item
                then
                NOWHERE 'CHAIN fixed{ c}!
            then then
        else
            'BEAR prop{}@ 0= if
                41
            else 'CHAIN prop{}@ 0= if
                37
            else
                171
                0 'CHAIN 2dup prop{}! fixed{ c}!
                'BEAR prop{}@ 3 <> if
                    2 'BEAR prop{}!
                then
                2 'BEAR prop{}@ - 'BEAR fixed{ c}!
            then then
        then then
    then

    over 'GRATE = if
        drop
        'KEYS not-here if
            31
        else closing @ if
            130
            panic @ 0= if
                15 clock2 !
                1 panic !
            then
        else
            34 'GRATE prop{}@ +
            'LOCK verb @ = if 0 else 1 then
            dup 'GRATE prop{}!
            2* +
        then then
    then

    nip speak-message
;

\ verb.c:vsay
: obj-say ( obj -- )
    drop
    ." Okay." CR say-last-thing CR
;

\ verb.c:vwave
: obj-wave ( obj -- )
    dup 'ROD <> 'ROD2 is-toting 0= or over is-toting 0= and if
        29 speak-message
    else
        dup 'ROD <> 'FISSURE is-at 0= or over is-toting 0= or closing @ or if
            just-speak
        else
            \ prop[FISSURE] = 1 - prop[FISSURE];
            \ pspeak(FISSURE, 2 - prop[FISSURE]);
            1 'FISSURE prop{}@ - dup 'FISSURE prop{}!
            'FISSURE 2 rot - speak-item
        then
    then
    drop
;

: fight-dragon
    49 0 0 yes-no 0= if
        exit
    then
    'DRAGON 1 speak-item
    2 'DRAGON prop{}!
    0 'RUG prop{}!
    MAXOBJ 'DRAGON + NOWHERE move-item
    MAXOBJ 'RUG + 0 move-item
    'DRAGON 120 move-item
    'RUG 120 move-item
    MAXOBJ 1- 1 do
        i place{ c}@ dup 119 = swap 121 = or if
            i 120 move-item
        then
    loop
    120 newloc !
;

\ verb.c:vkill
: obj-kill ( obj -- )

    case
        'BIRD of
            closed @ if
                137
            else
                'BIRD destroy-item
                0 'BIRD prop{}!
                'SNAKE place{ c}@ 19 = if
                    1 tally2 +!
                then
                45
            then endof
        0 of
            44 endof
        'CLAM of
            150 endof
        'OYSTER of
            150 endof
        'SNAKE of
            46 endof
        'DWARF of
            closed @ if
                dwarf-end
            then
            49 endof
        'TROLL of
            157 endof
        'BEAR of
            'BEAR prop{}@ 1+ 2/ 165 +
            endof
        'DRAGON of
            'DRAGON prop{}@ if
                167
            else
                fight-dragon exit
            then
        endof
        0 swap
    endcase

    ?dup if
        speak-message
    else
        just-speak
    then
;

\ verb.c:veat
: obj-eat ( obj -- )
    'FOOD over = if
        destroy-item
        72 speak-message exit
    then

    false
    over 'BIRD = or
    over 'SNAKE = or
    over 'CLAM = or
	over 'OYSTER = or
	over 'DWARF = or
	over 'DRAGON = or
	over 'TROLL = or
	swap 'BEAR = or
    ( any? )

	if
	   71 speak-message
	else
	   just-speak
	then
;

\ verb.c:vdrink
: obj-drink ( obj -- )
    'WATER <> if
        110 speak-message
    else
        'WATER bottle-liquid <> 'BOTTLE not-here or if
            just-speak
        else
            1 'BOTTLE prop{}!
            0 'WATER place{ c}!
            74 speak-message
        then
    then
;

\ verb.c:vfeed
: obj-feed ( obj -- )

    case
        'BIRD of 100 endof
        'DWARF of
            'FOOD not-here if
                just-speak
                exit
            then
            1 dflag +!
            103
        endof
        'BEAR of
            'FOOD not-here if
                'BEAR prop{}@ dup 0= if
                    drop 102
                else
                    3 = if 110
                else
                    just-speak
                    exit
                then then
            else
                'FOOD destroy-item
                1 'BEAR prop{}!
                0 'AXE prop{}!
                0 'AXE fixed{ c}!
                168
            then
        endof
        'DRAGON of
            'DRAGON prop{}@ if 110 else 102 then
        endof
        'TROLL of 182 endof
        'SNAKE of
            closed @ 'BIRD not-here or if
                102
            else
                102
                'BIRD destroy-item
                0 'BIRD prop{}!
                1 tally2 +!
            then
        endof
        14 swap
    endcase
    speak-message
;

\ verb.c:vthrow
: obj-throw ( obj -- )

    dup 'ROD = 'ROD2 is-toting and 'ROD is-toting 0= and if
        drop 'ROD2 dup object !
    then
    dup is-toting 0= if
        obj-speak
        exit
    then

    \ treasure to troll
    dup 50 >= over MAXOBJ < and 'TROLL is-at and if
        159 speak-message
        0 drop-item
        'TROLL 0 move-item
        'TROLL MAXOBJ + 0 move-item
        'TROLL2 117 drop-item
        'TROLL2 MAXOBJ + 122 drop-item
        'CHASM juggle-item
        exit
    then

    \ feed the bears...
    dup 'FOOD = 'BEAR is-here and if
        drop 'BEAR dup object ! obj-feed
        exit
    then

    \ if not axe, same as drop...
    dup 'AXE <> if
        obj-drop
        exit
    then

    drop

    \ AXE is THROWN
    \ at a dwarf...

    dwarf-check ?dup if
        48
        33 pct if
            drop 47
            over 0 swap
            ( i msg 0 i )
            2dup dseen{ c}! dloc{ c}!
            ( i msg )
            dkill @ dup 1+ dkill ! 0= if
                drop 149
            then
        then
        nip
    then

    \  at a dragon...
    'DRAGON is-at 'DRAGON prop{}@ 0= and if
        152
    \ at the troll...
    else 'TROLL is-at if
        158
    \ at the bear...
    else 'BEAR is-here 'BEAR prop{}@ 0= and if
        164 speak-message
        'AXE loc@ drop-item
        NOWHERE 'AXE fixed{ c}!
        1 'AXE prop{}!
        'BEAR juggle-item
        exit
    \ otherwise it is an attack
    else
        'KILL verb !
        0 object !
        intransitive-verb
        exit
    then then then

    \ handle the left over axe...
    speak-message
    'AXE loc@ drop-item
    describe-location
;

\ INVENTORY, FIND etc.
\ verb.c:vfind
: obj-find ( obj -- )
    r> 0
    r@ is-toting if
        drop 24
    else
        closed @ if
            drop 138
        else
            r@ 'DWARF = dwarf-check and dflag @ 2 >= and if
                drop 94
            else
                \ ?? does short-circuit || matter?
                r@ bottle-liquid = 'BOTTLE is-here and
                r@ is-at or r@ loc@ liquid-at = or if
                    drop 94
                then
            then
        then
    then
    r> drop
    ?dup if
        speak-message
    else
        just-speak
    then
;

\ verb.c:vfill
: obj-fill ( obj -- )
    case
    'BOTTLE of
        bottle-liquid if 105
        else loc@ liquid-at 0= if 106
        else
            loc@ cond{ c}@ WATOIL and 'BOTTLE prop{}!
            bottle-liquid
            'BOTTLE is-toting if
                NOWHERE over place{ c}!
            then
            'OIL = if 108 else 107 then
        then then
        endof
    'VASE of
        loc@ liquid-at 0= if
            144
        else 'VASE is-toting 0= if
            29
        else
            145 speak-message
            'VASE obj-drop
            exit
        then then
        endof
    29 swap
    endcase
    speak-message
;

\ verb.c:vtake
: obj-take ( obj -- )
    dup is-toting if
        obj-speak
        exit
    then

    \ special case objects and fixed objects
    25
    over 'PLANT = 'PLANT prop{}@ 0 <= and if
        drop 115
    then
    over 'BEAR = 'BEAR prop{}@ 1 = and if
        drop 169
    then
    over 'CHAIN = 'BEAR prop{}@ 0<> and if
        drop 170
    then
    over fixed{ c}@ if
        speak-message drop exit
    else
        drop
    then
    ( obj )

    \ special case for liquids

    dup 'WATER = over 'OIL = or if
        'BOTTLE dup object ! swap
        ( 'BOTTLE oldobj )
        bottle-liquid <> 'BOTTLE not-here or if
            'BOTTLE is-toting 'BOTTLE prop{}@ 1 = and if
                obj-fill exit
            then

            is-toting 'BOTTLE 0= if
                104
            else 'BOTTLE prop{}@ 1 <> if
                105
            else
                25
            then then
            speak-message drop exit
        then
        ( 'BOTTLE )
    then
    ( obj )

    holding @ 7 >= if
        drop 92 speak-message
        exit
    then

    \ special case for bird.
    dup 'BIRD = 'BIRD prop{}@ 0= and if
        'ROD is-toting if
            26 speak-message drop exit
        then
        'CAGE is-toting 0= if
            27 speak-message drop exit
        then
        1 'BIRD prop{}!
    then
    dup 'BIRD = over 'CAGE = or 'BIRD prop{}@ 0<> and if
        \ carry the other item
        'BIRD 'CAGE + over - loc@ carry-item
    then

	( obj )
	dup loc@ carry-item

    \  handle liquid in bottle
    bottle-liquid swap 'BOTTLE = over 0<> and if
        NOWHERE swap place{ c}!
    else
        drop
    then

 	54 speak-message
;

\ verb.c:vread
: obj-read
    is-dark if
        drop say-not-here exit
    then

    'OYSTER = if
	    'OYSTER is-toting closed and if
      		192 193 54 yes-no drop exit
        then
    then

	0 object @ case
	    'MAGAZINE of drop 190 endof
	    'TABLET of drop 196 endof
        'MESSAGE of drop 191 endof
    endcase
	?dup if
	   speak-message
	else
	   just-speak
	then
;

\ verb.c:vbreak
: obj-break ( obj -- )
    dup 'MIRROR = if
        closed @ if
            197 speak-message
            dwarf-end
        then
        drop 148
    else
        'VASE = 'VASE prop{}@ 0= and if
            198
            'VASE is-toting if
                'VASE loc@ drop-item
            then
            2 'VASE prop{}!
            NOWHERE 'VASE fixed{ c}!
        else
            just-speak
            exit
        then
    then
    speak-message
;

\ verb.c:vwake
: obj-wake ( obj -- )
    'DWARF = closed and if
        199 speak-message
        dwarf-end
    else
        just-speak
    then
    ;

\ intransitive verb handlers

\ track count and latest obj where f is true
\ itverb.c:addobj
: ?add-obj ( last count obj f -- new count )
    if
        -rot nip 1+
    else
        drop
    then
;

\ itverb.c:ivtake
: just-take
    0 0
    MAXOBJ 1- 1 do
        i loc@ over place{ c}@ = ?add-obj
    loop

    1 <> dwarf-check dflag 2 >= and or if
        drop need-obj
    else
        dup object ! obj-take
    then
;

\ itverb.c:ivopen
: just-open
    0 0
    'CLAM   dup is-here ?add-obj
    'OYSTER dup is-here ?add-obj
    'DOOR   dup is-at   ?add-obj
    'GRATE  dup is-at   ?add-obj
    'CHAIN  dup is-here ?add-obj
    ( obj count )

    ?dup 0= if
        drop 28 speak-message
    else 1 > if
        drop need-obj
    else
        dup object ! obj-open
    then then
;


\ itverb.c:ivkill
: just-kill
    \ check exactly one target is here
    0 0
    ( obj count )

    dwarf-check dflag @ 2 >= and if
        'DWARF object !
    then

	'SNAKE dup is-here ?add-obj
	'DRAGON dup is-at over prop{}@ 0= and ?add-obj
	'TROLL dup is-at ?add-obj
    'BEAR dup is-here over prop{}@ 0= and ?add-obj

    dup 1 > if                  \ more than one match?
        2drop need-obj exit
    then
    dup 1 = if
        drop dup object ! obj-kill exit
    then

    'BIRD dup is-here 'THROW verb @ <> and ?add-obj
    'CLAM dup is-here 'OYSTER is-here or ?add-obj
    1 > if
        drop need-obj exit
    then
    dup object ! obj-kill
;

\ itverb.c:iveat
: just-eat
    'FOOD is-here if
        'FOOD dup object !
        obj-eat
    else
        need-obj
    then
;

\ itverb.c:ivdrink
: just-drink
    loc@ liquid-at 'WATER <>
    bottle-liquid 'WATER <>
    'BOTTLE not-here or and if
        need-obj
    else
        'WATER dup object ! obj-drink
    then
;

\ itverb.c:ivquit
: just-quit
    22 54 54 yes-no 1 and dup gaveup ! if
        normal-end
    then
;

\ itverb.c:ivfill
: just-fill
    'BOTTLE is-here if
        'BOTTLE dup object !
        obj-fill
    else
        need-obj
    then
;

\ fee fie foe fum
\ itverb.c:ivfoo
: just-foo
    42
    last-verb-cstr 2@ [ 0 SPECIAL-WORD pack ] literal
    vocab-best unpack drop          \ get the value part

    ( msg val )
    1 over - foobar @ <> if
        drop
        foobar @ if drop 151 then
        speak-message exit
    then

    dup foobar ! 4 <> if
        drop exit
    then

    ( msg )
    0 foobar !

    'EGGS place{ c}@ 92 =
        'EGGS is-toting loc@ 92 = and
    or if
        speak-message
        exit
    else
        drop
    then

    'EGGS place{ c}@ 0= 'TROLL place{ c}@ 0= and 'TROLL prop{}@ 0= and if
        1 'TROLL prop{}!
    then

    'EGGS is-here if 1
    else 92 loc@ = if 0
    else 2 then then

    'EGGS 92 move-item
    'EGGS swap speak-item
;

\ itverb.c:ivread
: just-read
    0 0
    'MAGAZINE dup is-here ?add-obj
    'TABLET dup is-here ?add-obj
    'MESSAGE dup is-here ?add-obj
    1 <> is-dark or if
        drop need-obj
    else
        dup object !
        obj-read
    then
;

\ itverb.c:inventory
: just-inventory
	98             \ message
	65 1 do
	    i is-toting i 'BEAR <> and if
			( msg )
	        if 99 speak-message 0 then
			i -1 speak-item
	    then
	loop
    'BEAR is-toting if
        drop 141
    then
    ?dup if speak-message then
;

: just-suspend
\ TODO just-suspend
\ saveflg = 1;
;

: just-load
\ TODO just-load
;

'CALM      ' obj-speak     ' need-obj       14  set-actions
'WALK      ' obj-speak     ' just-speak     43  set-actions
'QUIT      ' obj-speak     ' just-quit      13  set-actions
'SCORE     ' obj-speak     ' score          13  set-actions
'FOO       ' obj-speak     ' just-foo       147 set-actions
'BRIEF     ' obj-speak     0                155 set-actions
'SUSPEND   ' obj-speak     ' just-suspend   13  set-actions
'LOAD      0               ' just-load      0   set-actions
'HOURS     ' obj-speak     0                13  set-actions
'LOG       ' obj-speak     0                0   set-actions
'TAKE      ' obj-take      ' just-take      24  set-actions
'DROP      ' obj-drop      ' need-obj       29  set-actions
'OPEN      ' obj-open      ' just-open      33  set-actions
'LOCK      ' obj-open      ' just-open      33  set-actions
'SAY       ' obj-say       ' need-obj       0   set-actions
'NOTHING   ' obj-nothing   ' just-nothing   0   set-actions
'ON        ' obj-on        ' just-on        38  set-actions
'OFF       ' obj-off       ' just-off       38  set-actions
'WAVE      ' obj-wave      ' need-obj       42  set-actions
'KILL      ' obj-kill      ' just-kill      110 set-actions
'POUR      ' obj-pour      ' just-pour      29  set-actions
'EAT       ' obj-eat       ' just-eat       110 set-actions
'DRINK     ' obj-drink     ' just-drink     73  set-actions
'RUB       ' obj-rub       ' need-obj       75  set-actions
'THROW     ' obj-throw     ' need-obj       29  set-actions
'FEED      ' obj-feed      ' need-obj       174 set-actions
'FIND      ' obj-find      ' need-obj       59  set-actions
'INVENTORY ' obj-find      ' just-inventory 59  set-actions
'FILL      ' obj-fill      ' just-fill      109 set-actions
'READ      ' obj-read      ' just-read      195 set-actions
'BLAST     ' obj-blast     ' just-blast     67  set-actions
'BREAK     ' obj-break     ' need-obj       146 set-actions
'WAKE      ' obj-wake      ' need-obj       110 set-actions
