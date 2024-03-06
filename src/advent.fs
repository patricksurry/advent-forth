\ TODO
\ - line wrapping with leading spaces (do embedded newlines work at all?)
\ - worth creataing an init-play function?  (hard to reinit anyway)
\ - save/restore (advent.c)
\ - links and check TODOs itverb.c verb.c


\ help debugging stack trace
: trace>name ( addr -- nt )
    \ find largest nt prior to addr
    >r 0 latestnt begin
        ?dup while
        2dup u< over r@ u< and if
            nip dup
        then
        2 + @
    repeat
    r> drop
    dup if
        dup u. dup wordsize u. dup name>string type space cr
    then
;

\ taliforth user_words reads and executes boot block
\ advblk.py generates boot block code to load and evaluate source and then relocate main data tables

8 nc-limit !        \ don't inline too much

\ database.c:bug
: bug ( n -- )
    ." Fatal error number " . CR
    abort
;

\ external word linkage

$80 constant RAND16
$f800 constant KERNEL
KERNEL $08 + constant NATIVE-RNG
KERNEL $1c + constant NATIVE-DECODE
KERNEL $77 + constant NATIVE-STRWRAP

42 RAND16 !     \ initialize random

: random ( -- n )
    NATIVE-RNG execute RAND16 @
;

\ database.c:pct
: pct ( n -- flag )             \ true with percent n
    random abs 100 mod <
;

: umin ( x y -- x|y ) 2dup u< if drop else nip then ;
: u<= u> invert ;
: <= > invert ;
: >= < invert ;

include defs.fs
include message.fs
include item.fs
include location.fs
include english.fs
include verb.fs
include dwarf.fs
include turn.fs

\ see advent.c:main
: main
    \TODO save/restore/debug
    65 1 0 yes-no if        \ instructions?
        1000 else 330
    then limit !
    begin turn again
;
