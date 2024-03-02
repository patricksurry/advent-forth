8 nc-limit !        \ don't inline too much

: bug ( n -- )
    ." Fatal error number " . CR
    abort
;

include data.fs
include defs.fs
include message.fs
include item.fs
include location.fs
include english.fs
include turn.fs
