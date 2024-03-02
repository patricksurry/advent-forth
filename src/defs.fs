\ see advent.h

100 constant MAXOBJ     \ max # of objects in cave	(x2 for fixed)
140 constant MAXLOC     \ max # of cave locations
17  constant MAXTRAV    \ 16 + 1 = max # of travel directions from loc

\ vocabulary word types
0 constant MOTION-TYPE
1 constant OBJECT-TYPE
2 constant VERB-TYPE
3 constant SPECIAL-TYPE

\ vocabulary ids
3 constant SAY

\ location ids
$ff constant NOWHERE

\ object ids
2   constant LAMP
7   constant STEPS
35  constant BEAR
50  constant NUGGET
62  constant RUG
64  constant CHAIN


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

variable turns   0 turns   !
variable limit 100 limit   !
variable tally  15 tally   !
variable tally2  0 tally2  !
variable newloc  1 newloc  !
variable loc     3 loc     !
variable oldloc  3 oldloc  !
variable oldloc2 3 oldloc2 !
variable holding 0 holding !
variable closed  0 closed  !
variable closing 0 closing !


: 0,n ( n -- )
    ?dup 0> if 0 do 0 c, loop then
;

: 0pad ( n start -- )
    + here - 0,n
;

create actmsg 32 here
	  0 c,  24 c,  29 c,   0 c,  33 c,   0 c,  33 c,  38 c,  38 c,  42 c,  14 c,   \  0 ...
	 43 c, 110 c,  29 c, 110 c,  73 c,  75 c,  29 c,  13 c,  59 c,  59 c,   	   \ 11 ...
	174 c, 109 c,  67 c,  13 c, 147 c, 155 c, 195 c, 146 c, 110 c,  13 c,  13 c,   \ 21 ...
    0pad

create cond MAXLOC here
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

create place MAXLOC here
	3 c, 3 c, 8 c, 10 c, 11 c, 0 c, 14 c, 13 c, 94 c, 96 c,   		       \ 1 ...
	19 c, 17 c, 101 c, 103 c, 0 c, 106 c, 0 c, 0 c, 3 c, 3 c,  0 c, 0 c,   \ 11 ...
	109 c, 25 c, 23 c, 111 c, 35 c, 0 c, 97 c,  0 c,    		           \ 23 ...
	119 c, 117 c, 117 c, 0 c, 130 c, 0 c, 126 c, 140 c, 0 c, 96 c, 	       \ 31 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	18 c, 27 c, 28 c, 29 c, 30 c, 	0 c, 					               \ 50 ...
	92 c, 95 c, 97 c, 100 c, 101 c, 0 c, 119 c, 127 c, 130 c,	           \ 56 ...
    0pad

create fixed MAXLOC here
	0 c, 0 c, 0 c,
	9 c, 0 c, 0 c, 0 c, 15 c, 0 c, NOWHERE c,  0 c, 				       \ 3 ...
	NOWHERE c, 27 c, NOWHERE c, 0 c, 0 c, 0 c, NOWHERE c,  0 c, 0 c, 0 c, 0 c, 0 c,        \ 11 ...
	NOWHERE c, NOWHERE c, 67 c, NOWHERE c, 110 c, 0 c, NOWHERE c, NOWHERE c, 		       \ 23 ...
	121 c, 122 c, 122 c, 0 c, NOWHERE c, NOWHERE c, NOWHERE c, NOWHERE c, 0 c, NOWHERE c,  \ 31 ...
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,
	0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c, 0 c,  0 c,
	121 c, NOWHERE c,
    0pad

create visited MAXLOC here 0pad     \ non-zero if has been here
create prop MAXOBJ here 0pad        \ status of objects


: pct ( n -- flag )             \ true with percent n
    random abs 100 mod <
;

: umin ( x y -- x|y )
    2dup u< if drop else nip then
;

: u<= u> invert ;

: <= > invert ;
