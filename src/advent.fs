\ taliforth user_words reads and executes boot block
\ advblk.py generates boot block code to load and evaluate source and then relocate main data tables

0 nc-limit !        \ don't inline too much

\ random integer in [0, n) without modulo bias
\ : randint ( n -- k )
\     dup $ffff 0 rot um/mod
\     ( n rem quo )
\     begin
\         nip
\         over random 1- 0 rot um/mod
\         ( n quo k quo' )
\         rot tuck <>
\     until \ while max quotient, retry
\     drop nip
\ ;

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
    100 randint  <
;

: 0pad ( n start -- )
    \ allocate and erase remainder of n byte region beginning at start
    + here swap over -
    ( here k )
    dup allot erase
;

include defs.fs
include message.fs
include item.fs
include location.fs
include hints.fs
include english.fs
include special.fs
include verb.fs
include turn.fs

\ see advent.c:main
: main
    65 1 0 yes-no if        \ instructions?
        cr 1000
    else
        330
    then
    limit !
    begin turn again
;
