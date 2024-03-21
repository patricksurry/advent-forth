\ TODO
\ - make defs an init function so we can restart (advent.c:initplay)
\ - save/restore (see advent.c)
\ - debug flag?
\ - open-adventure log random calls to replicate
\ - could save about 300 bytes by shortening all words to max 6 chars, about 650 to 2char (unreadable)
\ - hint data is present but code is missing?

\ - move c}@!; <= etc; blkrw and friends to native words
\ -  is c!@ useful enough?

\ taliforth user_words reads and executes boot block
\ advblk.py generates boot block code to load and evaluate source and then relocate main data tables

0 nc-limit !        \ don't inline too much

: umin ( x y -- x|y ) 2dup u< if drop else nip then ;
: modu ( x d -- ur )    \ always produce remainder in 0..d-1 even when x < 0
    dup >r mod r> over 0< if + else drop then ;
: u<= u> 0= ;
: u>= u< 0= ;
: <= > 0= ;
: >= < 0= ;

\ helper for debugging stack trace
\ : trace>name ( addr -- nt )
\     \ find largest nt prior to addr
\     >r 0 latestnt begin
\         ?dup while
\         2dup u< over r@ u< and if
\             nip dup
\         then
\         2 + @
\     repeat
\     r> drop dup if
\         dup u. dup wordsize u. dup name>string type space cr
\     then
\ ;

\ database.c:bug
: bug ( n -- )
    ." Fatal error number " . CR
    abort
;

\ database.c:pct
: pct ( n -- flag )             \ true with percent n
    random 100 modu <
;

include defs.fs
include message.fs
include item.fs
include location.fs
include english.fs
include special.fs
include verb.fs
include turn.fs

\ see advent.c:main
: main
    65 1 0 yes-no if        \ instructions?
        1000 else 330
    then limit !
    begin turn again
;
