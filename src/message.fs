: spkz ( strz -- )
    DIGRAMS pad decode type CR
;

\ show 1-indexed message
\ database.c:rspeak
: speak-message ( i -- )
    1- 2* MSGS& + @ MSGS + spkz
;

\ english.c:getwords
: user-input ( -- addr n )
    ." > " pad dup 127 accept tolower
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

: dunno
    60 61 13 random abs 3 mod -1 do rot loop
    2drop speak-message
;
