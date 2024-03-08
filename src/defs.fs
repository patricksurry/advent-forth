\ see advent.h

100 constant MAXOBJ     \ max # of objects in cave	(x2 for fixed)
140 constant MAXLOC     \ max # of cave locations
3   constant MAXDIE

\ vocabulary word types
0 constant MOTION-WORD
1 constant OBJECT-WORD
2 constant VERB-WORD
3 constant SPECIAL-WORD

\ motion ids
8  constant 'BACK
21 constant 'NULLX
57 constant 'LOOK
67 constant 'CAVE

\ action ids
1  constant 'TAKE
2  constant 'DROP
3  constant 'SAY
4  constant 'OPEN
5  constant 'NOTHING
6  constant 'LOCK
7  constant 'ON
8  constant 'OFF
9  constant 'WAVE
10 constant 'CALM
11 constant 'WALK
12 constant 'KILL
13 constant 'POUR
14 constant 'EAT
15 constant 'DRINK
16 constant 'RUB
17 constant 'THROW
18 constant 'QUIT
19 constant 'FIND
20 constant 'INVENTORY
21 constant 'FEED
22 constant 'FILL
23 constant 'BLAST
24 constant 'SCORE
25 constant 'FOO
26 constant 'BRIEF
27 constant 'READ
28 constant 'BREAK
29 constant 'WAKE
30 constant 'SUSPEND
31 constant 'HOURS
32 constant 'LOG
33 constant 'LOAD

\ location ids
$ff constant NOWHERE

\ object ids
1   constant 'KEYS
2   constant 'LAMP
3   constant 'GRATE
4   constant 'CAGE
5   constant 'ROD
6   constant 'ROD2
7   constant 'STEPS
8   constant 'BIRD
9   constant 'DOOR
11  constant 'SNAKE
12  constant 'FISSURE
13  constant 'TABLET
14  constant 'CLAM
15  constant 'OYSTER
16  constant 'MAGAZINE
17  constant 'DWARF
19  constant 'FOOD
20  constant 'BOTTLE
21  constant 'WATER
22  constant 'OIL
24  constant 'PLANT
25  constant 'PLANT2
23  constant 'MIRROR
31  constant 'DRAGON
33  constant 'TROLL
35  constant 'BEAR
36  constant 'MESSAGE
50  constant 'NUGGET
56  constant 'EGGS
58  constant 'VASE
62  constant 'RUG
64  constant 'CHAIN

\ bit mapping of "cond" array with location status
1   constant LIGHT
2   constant WATOIL
4   constant LIQUID
8   constant NOPIRAT
16  constant HINTC
32  constant HINTB
64  constant HINTS
128 constant HINTM
240 constant HINT        		\ HINT C+B+S+M */

variable verb
variable object
variable motion

\ set by analyze to repeat user words in context
2variable last-verb-cstr
2variable last-nonverb-cstr

variable turns   0 turns   !
variable wzdark  0 wzdark  !
variable closed  0 closed  !
variable closing 0 closing !
variable holding 0 holding !
variable detail  0 detail  !

\ timing variables
variable clock1 30 clock1  !
variable clock2 50 clock2  !
variable panic   0 panic   !

variable limit 100 limit   !
variable tally  15 tally   !
variable tally2  0 tally2  !

variable newloc  1 newloc  !
variable loc     3 loc     !
variable oldloc  3 oldloc  !
variable oldloc2 3 oldloc2 !

variable dflag   0 dflag   !
variable bonus   0 bonus   !
variable numdie  0 numdie  !
variable gaveup  0 gaveup  !
variable foobar  0 foobar  !

: 0,n ( n -- )
    ?dup 0> if 0 do 0 c, loop then
;

: 0pad ( n start -- )
    + here - 0,n
;

create cond{ MAXLOC 1+ here
	0 c,
	5 c, 1 c, 5 c, 5 c, 1 c, 1 c, 5 c, 17 c, 1 c, 1 c,  0 c, 0 c,      \ 1 ...
	32 c, 0 c, 0 c, 2 c, 0 c, 0 c, 64 c, 2 c,   	                   \ 13 ...
	2 c, 2 c, 0 c, 6 c, 0 c, 2 c,  0 c, 0 c, 0 c, 0 c,  	           \ 21 ...
	2 c, 2 c, 0 c, 0 c, 0 c, 0 c, 0 c, 4 c, 0 c, 2 c,  0 c, 	       \ 31 ...
	128 c, 128 c, 128 c, 128 c, 136 c, 136 c, 136 c, 128 c, 128 c, 	   \ 42 ...
	128 c, 128 c, 136 c, 128 c, 136 c, 0 c, 8 c, 0 c, 2 c, 	           \ 51 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	2 c, 128 c, 128 c, 136 c, 0 c, 0 c, 8 c, 136 c, 128 c, 0 c, 2 c, 2 c,  0 c, 0 c, 0 c, 0 c,  \ 79 ...
	4 c, 0 c, 0 c, 0 c, 0 c, 1 c, 			                           \ 95 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,  0 c, 0 c,
	4 c, 0 c, 1 c, 1 c,  0 c, 0 c, 0 c, 0 c, 0 c, 		               \ 113 ...
	8 c, 8 c, 8 c, 8 c, 8 c, 8 c, 8 c, 8 c, 8 c, 		               \ 122 ...
	0pad

create place{ MAXLOC 1+ here
    0 c,
	3 c, 3 c, 8 c, 10 c, 11 c, 0 c, 14 c, 13 c, 94 c, 96 c,   		       \ 1 ...
	19 c, 17 c, 101 c, 103 c, 0 c, 106 c, 0 c, 0 c, 3 c, 3 c,  0 c, 0 c,   \ 11 ...
	109 c, 25 c, 23 c, 111 c, 35 c, 0 c, 97 c,  0 c,    		           \ 23 ...
	119 c, 117 c, 117 c, 0 c, 130 c, 0 c, 126 c, 140 c, 0 c, 96 c, 	       \ 31 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	18 c, 27 c, 28 c, 29 c, 30 c, 	0 c, 					               \ 50 ...
	92 c, 95 c, 97 c, 100 c, 101 c, 0 c, 119 c, 127 c, 130 c,	           \ 56 ...
    0pad

create fixed{ MAXLOC 1+ here
	0 c, 0 c, 0 c,
	9 c, 0 c, 0 c, 0 c, 15 c, 0 c, NOWHERE c,  0 c, 				       \ 3 ...
	NOWHERE c, 27 c, NOWHERE c, 0 c, 0 c, 0 c, NOWHERE c,  0 c, 0 c, 0 c, 0 c, 0 c,        \ 11 ...
	NOWHERE c, NOWHERE c, 67 c, NOWHERE c, 110 c, 0 c, NOWHERE c, NOWHERE c, 		       \ 23 ...
	121 c, 122 c, 122 c, 0 c, NOWHERE c, NOWHERE c, NOWHERE c, NOWHERE c, 0 c, NOWHERE c,  \ 31 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,  0 c,
	121 c, NOWHERE c,
    0pad

create visited{ MAXLOC 1+ here 0pad         \ t/f if has been here
create prop{ MAXOBJ 1+ here 0pad            \ (signed) status of objects

\ unsigned char(c) and signed byte(b) array accessors
: c} ( off addr -- addr ) + ;
: b} + ;
: b@ ( addr -- b ) c@ dup 128 and if $ff00 or then ;
: b! c! ;
: c}@ ( off addr -- c ) + c@ ;
: b}@ ( off addr -- b ) + b@ ;
: c}! ( v off addr -- ) + c! ;
: b}! + c! ;

\ TODO could be part of init-play (see advent.c:initplay)
:noname
    MAXOBJ 50 do $ff i prop{ b}! loop
; execute
