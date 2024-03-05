: spkz ( strz -- )
    DIGRAMS pad NATIVE-DECODE execute type CR
;

: speak-message ( i -- )           \ show 1-indexed message
    1- 2* MSGS& + @ MSGS + spkz
;

: user-input ( -- addr n )
    ." > " pad dup 127 accept tolower
;

: yes-no ( prompt-msg yes-msg no-msg -- flag )
    rot ?dup if speak-message then      \ speak non-zero prompt
    user-input if                       \ non-empty result?
        c@ [CHAR] n <> else             \ not 'n' or empty means yes
        drop false then                 \ ( yes-msg no-msg is-yes )
    dup >r if drop else nip then
    ?dup if speak-message then          \ speak relevant non-zero response
    r>
;

: dunno
    60 61 13 random abs 3 mod -1 do rot loop
    2drop speak-message
;
