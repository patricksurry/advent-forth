\ 1-indexed arrays for action handlers up to 'LOAD = 33

create act-obj&  34 cells here 0pad
create act-it& 34 cells here 0pad
create act-msg{  34 here 0pad

: act! ( action act-obj act-it msg -- )
    2swap swap >r
    ( act-it msg act-obj  R: action )
    r@ cells act-obj& + !
    r@ act-msg{ c}!
    r> cells act-it& + !
;

\ turn.c:trverb,trobj
: obj-verb ( -- )  \ act on an object
    verb @ ?dup if
        cells act-obj& + @
        ( xt )
        ?dup 0= if
            ." This verb is not implemented yet." CR
        else
            object @ swap execute
        then
    else
        ." What do you want to do with the " say-thing ." ?" CR
    then
;

\ itverb.c:itverb
: just-verb ( -- )  \ just do it
    verb @ ?dup if
        cells act-it& + @
        ?dup 0= if
            ." This intransitive not implemented yet." CR
        else
            execute
        then
    then
;

\ verb.c:actspk
: say-it ( -- )
    verb @ dup 1 < over 31 > or if
        39 bug then

    act-msg{ c}@ ?dup if say-msg then
;
: say-obj   ( obj -- )
    drop say-it
;

: nada 54 say-msg ;
: nada-obj drop nada ;

\ verb.c:needobj
: need-obj ( -- )
    2verb 2@ type ."  what?" CR
;

\ handle verbs with or without object

\ verb.c:von
: on-it
    'LAMP here? if
        limit @ 0 < if
            184 say-msg
        else
            1 'LAMP prop{}!
            39 say-msg
            wzdark @ if
                0 wzdark !
                desc-loc
            then
        then
    else
        say-it
    then
;
: on-obj drop on-it ;

\ verb.c:voff
: off-it
    'LAMP here? if
        0 'LAMP prop{}!
        40 say-msg
    else
        say-it
    then
;
: off-obj drop off-it ;

\ verb.c:vpour
: pour-obj ( obj -- )
    dup 'BOTTLE = over 0= or if
        drop liquid-in dup object !
    then
    dup 0= if
        drop need-obj exit
    then
    dup toting? 0= if
        say-obj   exit
    then
    dup 'OIL <> over 'WATER <> and if
        drop 78 say-msg exit
    then
    1 'BOTTLE prop{}!
    0 over place{ c}!

    'PLANT at? if
        dup 'WATER <> if
            112 say-msg
        else
            'PLANT dup prop{}@ swap over 1+ say-item
            2 + 6 mod dup 'PLANT prop{}!
            2/ 'PLANT2 prop{}@
            desc-loc
        then
    else
        'DOOR at? if
            dup 'OIL = 1 and dup
            'DOOR prop{}!
            113 +
        else
            77
        then
        say-msg
    then
    drop
;
: pour-it object @ pour-obj ;

\ verb.c:vblast
: blast-it
    'ROD2 prop{}@ 0< closed @ 0= or if
        say-it
    else
        133
        115 loc@ = if drop 134 then
        'ROD2 here? if drop 135 then
        dup say-msg
        bonus !
        normal-end
    then
;
: blast-obj drop blast-it ;

\ transitive verbs

: rub-obj ( obj -- )
    'LAMP = if
        say-it
    else
        76 say-msg
    then
;

\ verb.c:vdrop
: drop-obj ( obj -- )

    \ check for dynamite
    'ROD2 toting? over 'ROD = and over toting? 0= and if
        drop 'ROD2 dup object !
    then

    dup toting? 0= if
        say-obj   exit
    then

 	\ snake and bird
    dup 'BIRD = 'SNAKE here? and if
        30 say-msg
        closed @ if
            dwarf-end
        then
        'SNAKE destroy-item
        -1 'SNAKE prop{}!
    then

    \ coins and vending machine
    dup 'COINS = 'VEND here? and if
        destroy-item
    	'BATTERIES dup loc@ drop-item
    	say-item
    	exit
    then

    dup 'BIRD = 'DRAGON at? and 'DRAGON prop{}@ 0= and if
        154 say-msg
        'BIRD destroy-item
        0 'BIRD prop{}!
        'SNAKE place{ c}@ if
            1 tally2 +!
        then
        exit
    then

    \ Bear and troll
    dup 'BEAR = 'TROLL at? and if
        163 say-msg
        troll!
        2 'TROLL prop{}!
    then

    \ vase
    dup 'VASE = if
        96 loc@ = if
            54 say-msg
        else
            'PILLOW at? if 0 else 2 then
            'VASE prop{}!
            'VASE dup prop{}@ dup -rot 1+
            ( prop[VASE] VASE prop[VASE]+1 )
            say-msg
            if
                NOWHERE 'VASE fixed{ c}!
            then
        then
    then

	\ handle liquid and bottle
	liquid-in
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
: open-obj ( obj -- )
    33
    ( obj msg )

    over dup 'CLAM = swap 'OYSTER = or if
        drop dup 'OYSTER =
        ( obj oyclam )
        verb @ 'LOCK = if
            drop 61
        else 'TRIDENT toting? 0= if
            122 +
        else over toting? if
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
                'CHAIN toting? if
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

    nip say-msg
;

\ verb.c:vsay
: echo-obj ( obj -- )
    drop
    ." Okay." CR say-thing CR
;

\ verb.c:vwave
: wave-obj ( obj -- )
    dup 'ROD <> 'ROD2 toting? 0= or over toting? 0= and if
        29 say-msg
    else
        dup 'ROD <> 'FISSURE at? 0= or over toting? 0= or closing @ or if
            say-it
        else
            \ prop[FISSURE] = 1 - prop[FISSURE];
            \ pspeak(FISSURE, 2 - prop[FISSURE]);
            1 'FISSURE prop{}@ - dup 'FISSURE prop{}!
            'FISSURE 2 rot - say-item
        then
    then
    drop
;

: fight-dragon
    49 0 0 yes-no 0= if
        exit
    then
    'DRAGON 1 say-item
    2 'DRAGON prop{}!
    0 'RUG prop{}!
    'DRAGON 120 NOWHERE move-2item
    'RUG 120 0 move-2item
    MAXOBJ 1- 1 do
        i place{ c}@ dup  119 =  swap 121 =  or if
            i 120 move-item
        then
    loop
    120 newloc !
;

\ verb.c:vkill
: kill-obj ( obj -- )

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
        say-msg
    else
        say-it
    then
;

\ verb.c:veat
: eat-obj ( obj -- )
    'FOOD over = if
        destroy-item
        72 say-msg exit
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
	   71 say-msg
	else
	   say-it
	then
;

\ verb.c:vdrink
: drink-obj ( obj -- )
    'WATER <> if
        110 say-msg
    else
        'WATER liquid-in <> 'BOTTLE not-here or if
            say-it
        else
            1 'BOTTLE prop{}!
            0 'WATER place{ c}!
            74 say-msg
        then
    then
;

\ verb.c:vfeed
: feed-obj ( obj -- )

    case
        'BIRD of 100 endof
        'DWARF of
            'FOOD not-here if
                say-it
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
                    say-it
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
    say-msg
;

\ verb.c:vthrow
: throw-obj ( obj -- )

    dup 'ROD = 'ROD2 toting? and 'ROD toting? 0= and if
        drop 'ROD2 dup object !
    then
    dup toting? 0= if
        say-obj
        exit
    then

    \ treasure to troll
    dup 50 >= over MAXOBJ < and 'TROLL at? and if
        159 say-msg
        0 drop-item
        'TROLL 0 0 move-2item
        'TROLL2 117 drop-item
        'TROLL2 MAXOBJ + 122 drop-item
        \ 'CHASM juggle-item            \ no-op in this version
        exit
    then

    \ feed the bears...
    dup 'FOOD = 'BEAR here? and if
        drop 'BEAR dup object ! feed-obj
        exit
    then

    \ if not axe, same as drop...
    dup 'AXE <> if
        drop-obj
        exit
    then

    drop
    ( )

    \ AXE is THROWN
    \ at a dwarf...
    dwarf? ?dup if
        48
        ( i msg )
        33 pct  if
            drop 47
            over 0 swap 2dup
            ( i msg 0 i 0 i)
            dseen{ c}! dloc{ c}!

            dkill @ dup  1+ dkill !
            0= if
                drop 149
            then
            ( i msg )
        then
        nip
    \  at a dragon...
    else 'DRAGON at? 'DRAGON prop{}@ 0= and if
        152
    \ at the troll...
    else 'TROLL at? if
        158
    \ at the bear...
    else 'BEAR here? 'BEAR prop{}@ 0= and if
        164 say-msg
        'AXE loc@ drop-item
        NOWHERE 'AXE fixed{ c}!
        1 'AXE prop{}!
        \ 'BEAR juggle-item     \ no-op in this version
        exit
    \ otherwise it is an attack
    else
        'KILL verb !
        0 object !
        just-verb
        exit
    then then then then

    \ handle the left over axe...
    say-msg
    'AXE loc@ drop-item
    desc-loc
;

\ INVENTORY, FIND etc.
\ verb.c:vfind
: find-obj ( obj -- )
    r> 0
    r@ toting? if
        drop 24
    else
        closed @ if
            drop 138
        else
            r@ 'DWARF = dwarf? and dflag @ 2 >= and if
                drop 94
            else
                \ ?? does short-circuit || matter?
                r@ liquid-in = 'BOTTLE here? and
                r@ at? or r@ loc@ liquid-at = or if
                    drop 94
                then
            then
        then
    then
    r> drop
    ?dup if
        say-msg
    else
        say-it
    then
;

\ verb.c:vfill
: fill-obj ( obj -- )
    case
    'BOTTLE of
        liquid-in if 105
        else loc@ liquid-at 0= if 106
        else
            loc@ cond{ c}@ WATOIL and 'BOTTLE prop{}!
            liquid-in
            'BOTTLE toting? if
                NOWHERE over place{ c}!
            then
            'OIL = if 108 else 107 then
        then then
        endof
    'VASE of
        loc@ liquid-at 0= if
            144
        else 'VASE toting? 0= if
            29
        else
            145 say-msg
            'VASE drop-obj
            exit
        then then
        endof
    29 swap
    endcase
    say-msg
;

\ verb.c:vtake
: take-obj ( obj -- )
    dup toting? if
        say-obj
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
        say-msg drop exit
    else
        drop
    then
    ( obj )

    \ special case for liquids

    dup 'WATER = over 'OIL = or if
        'BOTTLE dup object ! swap
        ( 'BOTTLE oldobj )
        liquid-in <> 'BOTTLE not-here or if
            'BOTTLE toting? 'BOTTLE prop{}@ 1 = and if
                fill-obj exit
            then

            toting? 'BOTTLE 0= if
                104
            else 'BOTTLE prop{}@ 1 <> if
                105
            else
                25
            then then
            say-msg drop exit
        then
        ( 'BOTTLE )
    then
    ( obj )

    holding @ 7 >= if
        drop 92 say-msg
        exit
    then

    \ special case for bird.
    dup 'BIRD = 'BIRD prop{}@ 0= and if
        'ROD toting? if
            26 say-msg drop exit
        then
        'CAGE toting? 0= if
            27 say-msg drop exit
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
    liquid-in swap 'BOTTLE = over 0<> and if
        NOWHERE swap place{ c}!
    else
        drop
    then

 	54 say-msg
;

\ verb.c:vread
: read-obj
    dark? if
        drop say-not-here exit
    then

    'OYSTER = if
	    'OYSTER toting? closed and if
      		192 193 54 yes-no drop exit
        then
    then

	0 object @ case
	    'MAGAZINE of drop 190 endof
	    'TABLET of drop 196 endof
        'MESSAGE of drop 191 endof
    endcase
	?dup if
	   say-msg
	else
	   say-it
	then
;

\ verb.c:vbreak
: break-obj ( obj -- )
    dup 'MIRROR = if
        closed @ if
            197 say-msg
            dwarf-end
        then
        drop 148
    else
        'VASE = 'VASE prop{}@ 0= and if
            198
            'VASE toting? if
                'VASE loc@ drop-item
            then
            2 'VASE prop{}!
            NOWHERE 'VASE fixed{ c}!
        else
            say-it
            exit
        then
    then
    say-msg
;

\ verb.c:vwake
: wake-obj ( obj -- )
    'DWARF = closed and if
        199 say-msg
        dwarf-end
    else
        say-it
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
: take-it
    0 0
    MAXOBJ 1- 1 do
        i loc@ over place{ c}@ = ?add-obj
    loop

    1 <> dwarf? dflag 2 >= and or if
        drop need-obj
    else
        dup object ! take-obj
    then
;

\ itverb.c:ivopen
: open-it
    0 0
    'CLAM   dup here? ?add-obj
    'OYSTER dup here? ?add-obj
    'DOOR   dup at?   ?add-obj
    'GRATE  dup at?   ?add-obj
    'CHAIN  dup here? ?add-obj
    ( obj count )

    ?dup 0= if
        drop 28 say-msg
    else 1 > if
        drop need-obj
    else
        dup object ! open-obj
    then then
;


\ itverb.c:ivkill
: kill-it
    \ check exactly one target is here
    0 0
    ( obj count )

    dwarf? dflag @ 2 >= and if
        'DWARF object !
    then

	'SNAKE dup here? ?add-obj
	'DRAGON dup at? over prop{}@ 0= and ?add-obj
	'TROLL dup at? ?add-obj
    'BEAR dup here? over prop{}@ 0= and ?add-obj

    dup 1 > if                  \ more than one match?
        2drop need-obj exit
    then
    dup 1 = if
        drop dup object ! kill-obj exit
    then

    'BIRD dup here? 'THROW verb @ <> and ?add-obj
    'CLAM dup here? 'OYSTER here? or ?add-obj
    1 > if
        drop need-obj exit
    then
    dup object ! kill-obj
;

\ itverb.c:iveat
: eat-it
    'FOOD here? if
        'FOOD dup object !
        eat-obj
    else
        need-obj
    then
;

\ itverb.c:ivdrink
: drink-it
    loc@ liquid-at 'WATER <>
    liquid-in 'WATER <>
    'BOTTLE not-here or and if
        need-obj
    else
        'WATER dup object ! drink-obj
    then
;

\ itverb.c:ivquit
: quit-it
    22 54 54 yes-no 1 and dup gaveup ! if
        normal-end
    then
;

\ itverb.c:ivfill
: fill-it
    'BOTTLE here? if
        'BOTTLE dup object !
        fill-obj
    else
        need-obj
    then
;

\ fee fie foe fum
\ itverb.c:ivfoo
: foo-it
    42
    2verb 2@ [ 0 SPECIAL-WORD pack ] literal
    vocab-best unpack drop          \ get the value part

    ( msg val )
    1 over - foobar @ <> if
        drop
        foobar @ if drop 151 then
        say-msg exit
    then

    dup foobar ! 4 <> if
        drop exit
    then

    ( msg )
    0 foobar !

    'EGGS place{ c}@ 92 =
        'EGGS toting? loc@ 92 = and
    or if
        say-msg
        exit
    else
        drop
    then

    'EGGS place{ c}@ 0= 'TROLL place{ c}@ 0= and 'TROLL prop{}@ 0= and if
        1 'TROLL prop{}!
    then

    'EGGS here? if 1
    else 92 loc@ = if 0
    else 2 then then

    'EGGS 92 move-item
    'EGGS swap say-item
;

\ itverb.c:ivread
: read-it
    0 0
    'MAGAZINE dup here? ?add-obj
    'TABLET dup here? ?add-obj
    'MESSAGE dup here? ?add-obj
    1 <> dark? or if
        drop need-obj
    else
        dup object !
        read-obj
    then
;

\ itverb.c:inventory
: inventory
	98             \ default message
	65 1 do
	    i toting? i 'BEAR <> and if
			( msg )
	        if
				99 say-msg
			then
			0
			i -1 say-item
	    then
	loop
    'BEAR toting? if
        drop 141
    then
    ?dup if say-msg then
;

: save
    63 turns blk-write
    ." Saved game." cr
    abort
;

: load
    63 turns blk-read
    ." Restored game." cr
;

'CALM      ' say-obj       ' need-obj    14     act!
'WALK      ' say-obj       ' say-it      43     act!
'QUIT      ' say-obj       ' quit-it     13     act!
'SCORE     ' say-obj       ' score       13     act!
'FOO       ' say-obj       ' foo-it     147     act!
'BRIEF     ' say-obj       0            155     act!
'SUSPEND   ' say-obj       ' save        13     act!
'LOAD      0               ' load         0     act!
'HOURS     ' say-obj       0             13     act!
'LOG       ' say-obj       0              0     act!
'TAKE      ' take-obj      ' take-it     24     act!
'DROP      ' drop-obj      ' need-obj    29     act!
'OPEN      ' open-obj      ' open-it     33     act!
'LOCK      ' open-obj      ' open-it     33     act!
'SAY       ' echo-obj      ' need-obj     0     act!
'NOTHING   ' nada-obj      ' nada         0     act!
'ON        ' on-obj        ' on-it       38     act!
'OFF       ' off-obj       ' off-it      38     act!
'WAVE      ' wave-obj      ' need-obj    42     act!
'KILL      ' kill-obj      ' kill-it    110     act!
'POUR      ' pour-obj      ' pour-it     29     act!
'EAT       ' eat-obj       ' eat-it     110     act!
'DRINK     ' drink-obj     ' drink-it    73     act!
'RUB       ' rub-obj       ' need-obj    75     act!
'THROW     ' throw-obj     ' need-obj    29     act!
'FEED      ' feed-obj      ' need-obj   174     act!
'FIND      ' find-obj      ' need-obj    59     act!
'INVENTORY ' find-obj      ' inventory   59     act!
'FILL      ' fill-obj      ' fill-it    109     act!
'READ      ' read-obj      ' read-it    195     act!
'BLAST     ' blast-obj     ' blast-it    67     act!
'BREAK     ' break-obj     ' need-obj   146     act!
'WAKE      ' wake-obj      ' need-obj   110     act!
