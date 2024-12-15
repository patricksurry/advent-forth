: sayz ( strz -- )
    DIGRAMS typez
;

\ show 1-indexed message
\ database.c:rspeak
: say-msg ( i -- )
    1- 2* MSGS& + @ MSGS + sayz
;

\ english.c:getwords
: user-input ( -- addr n )
    ." > " $400 dup 127 accept 2dup lower CR
;

\ optionally prompt user and respond to a yes/no question returning flag
\ database.c:yes
: yes-no ( prompt-msg yes-msg no-msg -- flag )
    rot ?dup if say-msg then            \ say a non-zero prompt
    user-input if                       \ non-empty result?
        c@ [CHAR] n <>                  \ not 'n' or empty means yes
    else
        drop false
    then
    ( yes-msg no-msg yes? )
    dup >r if
        drop
    else
        nip
    then
    ?dup if say-msg then                \ say corresponding non-zero response
    r>
;

: say-thing
    2other 2@ type
;

: say-not-here
    ." I see no " say-thing ."  here." CR
;

: dunno
    60 61 13
    3 randint -1 do rot loop    \ shuffle
    2drop say-msg
;
