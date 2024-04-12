\ see advent.h

65  constant MAXOBJ     \ max # of objects in cave
140 constant MAXLOC     \ max # of cave locations
3   constant MAXDIE     \ max resurrections
79  constant MAXTRS     \ number of scored treasures
7   constant MAXDWARF   \ number of dwarves

\ vocabulary word types
0 constant MOTION-WORD
1 constant OBJECT-WORD
2 constant VERB-WORD
3 constant SPECIAL-WORD

\ motion ids
8  constant 'BACK
21 constant 'NULLX
57 constant 'LOOK
63 constant 'DEPRESSION
64 constant 'ENTRANCE
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
10  constant 'PILLOW
11  constant 'SNAKE
12  constant 'FISSURE
13  constant 'TABLET
14  constant 'CLAM
15  constant 'OYSTER
16  constant 'MAGAZINE
17  constant 'DWARF
18  constant 'KNIFE
19  constant 'FOOD
20  constant 'BOTTLE
21  constant 'WATER
22  constant 'OIL
23  constant 'MIRROR
24  constant 'PLANT
25  constant 'PLANT2
28  constant 'AXE
31  constant 'DRAGON
32  constant 'CHASM
33  constant 'TROLL
34  constant 'TROLL2
35  constant 'BEAR
36  constant 'MESSAGE
38  constant 'VEND
39  constant 'BATTERIES

\ fixed locations
50  constant 'NUGGET
54  constant 'COINS
55  constant 'CHEST
56  constant 'EGGS
57  constant 'TRIDENT
58  constant 'VASE
59  constant 'EMERALD
60  constant 'PYRAMID
61  constant 'PEARL
62  constant 'RUG
63  constant 'SPICES
64  constant 'CHAIN

\ bit mapping of "cond" array with location status
1   constant LIGHT
2   constant WATOIL
4   constant LIQUID
8   constant NOPIRAT
\ bits 4-6 store a hint index 1-6 (or 0)
112 constant HINTMASK           \ x111xxxx = 16+32+64

variable verb
variable object
variable motion

\ set by analyze to repeat user words in context
2variable 2verb
2variable 2other

\ turns is the save/load mark, need prop{ MAXOBJ  1+ + turns - <= 1024

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

\ hint tracking
create hinted 0 c,          \ bit flag for each hint offered
create hintlc{ 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, \ turns at hint-worthy locations

\ dwarf locations; note dloc[DWARFMAX-1] = chloc
create dloc{   0 c, 19 c, 27 c, 33 c, 44 c, 64 c, 114 c,
create dseen{  0 c,  0 c,  0 c,  0 c,  0 c,  0 c,  0 c,     \ seen flag
create odloc{  0 c,  0 c,  0 c,  0 c,  0 c,  0 c,  0 c,     \ old locs
variable dkill  0  dkill    !       \ dwarves killed

variable daltloc 18 daltloc !       \ alternate appearance
variable limit 100 limit   !
variable tally  15 tally   !
variable tally2  0 tally2  !

variable newloc  1 newloc  !
variable loc     3 loc     !
variable oldloc  3 oldloc  !
variable oldloc2 3 oldloc2 !
variable knfloc  0 knfloc  !
variable chloc   114 chloc !
variable chloc2  140 chloc2 !

variable dflag   0 dflag   !
variable bonus   0 bonus   !
variable numdie  0 numdie  !
variable gaveup  0 gaveup  !
variable foobar  0 foobar  !
variable lmwarn  0 lmwarn  !

\ objects current location
create place{ MAXOBJ 1+ here
    0 c,
	3 c, 3 c, 8 c, 10 c, 11 c, 0 c, 14 c, 13 c, 94 c, 96 c,   		       \ 1 ...
	19 c, 17 c, 101 c, 103 c, 0 c, 106 c, 0 c, 0 c, 3 c, 3 c,  0 c, 0 c,   \ 11 ...
	109 c, 25 c, 23 c, 111 c, 35 c, 0 c, 97 c,  0 c,    		           \ 23 ...
	119 c, 117 c, 117 c, 0 c, 130 c, 0 c, 126 c, 140 c, 0 c, 96 c, 	       \ 31 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	18 c, 27 c, 28 c, 29 c, 30 c, 	0 c, 					               \ 50 ...
	92 c, 95 c, 97 c, 100 c, 101 c, 0 c, 119 c, 127 c, 130 c,	           \ 56 ...
    0pad

\ second object (fixed) locations
create fixed{ MAXOBJ 1+ here
	0 c, 0 c, 0 c,
	9 c, 0 c, 0 c, 0 c, 15 c, 0 c, NOWHERE c,  0 c, 				       \ 3 ...
	NOWHERE c, 27 c, NOWHERE c, 0 c, 0 c, 0 c, NOWHERE c,  0 c, 0 c, 0 c, 0 c, 0 c,        \ 11 ...
	NOWHERE c, NOWHERE c, 67 c, NOWHERE c, 110 c, 0 c, NOWHERE c, NOWHERE c, 		       \ 23 ...
	121 c, 122 c, 122 c, 0 c, NOWHERE c, NOWHERE c, NOWHERE c, NOWHERE c, 0 c, NOWHERE c,  \ 31 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,  0 c,
	121 c, NOWHERE c,
    0pad

\ reserve space but define consts after to keep save block under 1024 bytes
here MAXLOC 1+ over 0pad    \ visited{
here MAXOBJ 1+ over 0pad    \ prop{

constant prop{              \ (signed char) status of objects
constant visited{           \ flags user visited loc

\ unsigned char(c) and signed byte(b) array accessors
: c}@ ( off addr -- c ) + c@ ;
: c}! ( v off addr -- ) + c! ;

: prop{}@ ( off -- b )
    prop{ + cs@
;
: prop{}! ( b off -- )
    prop{ + c!
;

: loc@ loc @ ;

here
:noname
    \ 50 and above are fixed location
    MAXOBJ 50 do $ff i prop{}! loop
;
execute here - allot    \ run then drop this code
