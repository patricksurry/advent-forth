: spkz ( strz -- )
    DIGRAMS typez
;

\ show 1-indexed message
\ database.c:rspeak
: speak-message ( i -- )
    1- 2* MSGS& + @ MSGS + spkz
;

\ english.c:getwords
: user-input ( -- addr n )
    ." > " $400 dup 127 accept tolower CR
;

\ optionally prompt user and respond to a yes/no question returning flag
\ database.c:yes
: yes-no ( prompt-msg yes-msg no-msg -- flag )
    rot ?dup if speak-message then      \ speak non-zero prompt
    user-input if                       \ non-empty result?
        c@ [CHAR] n <>                  \ not 'n' or empty means yes
    else
        drop false
    then
    ( yes-msg no-msg is-yes )
    dup >r if drop else nip then
    ?dup if speak-message then          \ speak relevant non-zero response
    r>
;

: say-last-thing
    last-nonverb-cstr 2@ type
;

: say-not-here
    ." I see no " say-last-thing ."  here." CR
;

: dunno
    60 61 13
    3 randint -1 do rot loop    \ shuffle
    2drop speak-message
;
