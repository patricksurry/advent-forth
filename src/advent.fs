\ The taliforth uc platform's `platform_forth.fs` executes a turnkey
\ word which is normally `block-boot`.   This reads and executes `boot.fs`
\ from the block device which compiles this source code and relocates the data tables.

0 nc-limit !        \ minimize code size by not inlining anything

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

: lower ( addr len -- ) bounds ?do i c@ $20 or i c! loop ;

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
46 emit
include message.fs
46 emit
include item.fs
46 emit
include location.fs
46 emit
include hints.fs
46 emit
include english.fs
46 emit
include special.fs
46 emit
include verb.fs
46 emit
include turn.fs

\ see advent.c:main
: play
    page
    65 1 0 yes-no if        \ instructions?
        cr 1000
    else
        330
    then
    limit !
    begin turn again
;
