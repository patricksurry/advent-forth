\ each VOCAB entry is [ val typ len <chars> ], ending with [0 0 0]

: speak-vocab ( -- )
    \ output VOCABulary for motion and verb types
    VOCAB begin
        dup 2 + c@ ?dup while               \ p len
        over 1+ c@ dup MOTION-TYPE = swap VERB-TYPE = or if    \ p len f
            over 3 + over type space then
        + 3 +
    repeat
    drop
;

: vocab-best ( addr n typval-min -- typval-best|-1 )
    \ find matching word with lowest typ_val >= thresh
    -rot 2>r -1 swap VOCAB          \ -1 vmin p  R: addr n
    begin
        dup 2 + c@                  \ best vmin p len
        ?dup while                  \ end of VOCAB?
        over 3 + swap 2dup + -rot   \ best vmin p q sp len
        2r@ compare 0=              \ best vmin p q flag
            if >r @ 2dup u<=        \ on match test against vmin, then take min or drop
                if rot umin swap else drop then r>
            else nip
            then                    \ best vmin q
    repeat
    2drop 2r> 2drop
;

: analyze ( addr n -- typ-val 1  ... -- 0)
    tolower
    dup 0= if
        nip exit then
    0 vocab-best dup
    -1 = if dunno 0 else 1 then
;

: bad-grammar ( -- )
    ." bad grammar..." CR
;

: english ( -- 1|0 )
    MSGBUF dup 255 accept                   \ addr n
    cleave analyze 0= if                    \ addr n ( tv 1 | 0 )
        2drop 0 exit
    then
    dup [ VERB-TYPE SAY pack ] literal = if
        SAY verb ! 1 object !
        2drop 1 exit
    then                                    \ addr n tv
    -rot cleave 2swap 2drop                 \ tv1 addr n
    ?dup 0= if                              \ empty second word shouldn't fail
        drop -1 else                        \ tv1 -1
        analyze 0=                          \ unknown second word does fail
            if drop 0 exit then
    then                                    \ tv1 tv2
    2dup = over [ SPECIAL-TYPE 51 pack ] literal = and if
        speak-vocab 0 exit
    then
    unpack -rot unpack rot swap             \ v2 v1 t2 t1
    2dup SPECIAL-TYPE = swap SPECIAL-TYPE = or if
        speak-message 2drop 2drop 0 exit
    then
    2dup MOTION-TYPE = if
        MOTION-TYPE = if
            bad-grammar 2drop 2drop 0 exit
        then
        2drop motion ! drop 1 exit else
        drop
    then
    over MOTION-TYPE = if
        2drop drop motion ! 1 exit
    then
    dup OBJECT-TYPE = if
        drop swap object !
        VERB-TYPE = if
            verb ! 1 exit
        then
        bad-grammar drop 0 exit
    then
    dup VERB-TYPE = if
        drop swap verb !
        OBJECT-TYPE = if
            object ! 1 exit
        then
        bad-grammar drop 0 exit
    then
    36 bug
;
