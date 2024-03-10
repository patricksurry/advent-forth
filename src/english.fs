\ each VOCAB entry is [ val typ len <chars> ], ending with [0 0 0]

\ english.c:outwords
: speak-vocab ( -- )
    \ output vocabulary for motion and verb types
    VOCAB begin
        dup 2 + c@ ?dup while               \ p len
        over 1+ c@ dup MOTION-WORD = swap VERB-WORD = or if    \ p len f
            over 3 + over type space then
        + 3 +
    repeat
    drop
;

\ check if string matches any of the words listed for a vocab entry
\ each vocab entry has bytes like: typ, val, n0, s0, n1, s1, ...
\ the last string has n1 hi bit set;
\ the last vocab entry has typ, val = $ff, $ff
: vocab-match ( addr n vptr -- addr n [vptr' | 0 ] [typ_val | -1] )
    dup @ dup -1 = if                   ( addr n vptr tv )
        nip 0 swap exit                 \ last entry? result is 0 -1
    then
    \ check each string until we find next header, noting if any match
    >r -rot 2>r 0 swap 2 +              ( f wptr  R: addr n tv )
    begin
        dup 1+ swap c@                  ( f s0 n0'  R: addr n tv )
        dup $80 and -rot $7f and        ( f last? s0 n0  R: addr n tv )
        2dup + -rot dup 2r@ rot over = if    ( f last? p1 s0 n0 addr n  R: addr n tv )
            compare 0=                  \ if string lengths match, compare 0 means matched
        else
            2drop 2drop 0               \ don't bother
        then                            ( f last? p1 match? )
        2swap -rot or -rot              ( f' p1 last? )
    until
    2r> 2swap r> rot 0= if              ( addr n vptr' tv )
        drop -1                         \ no match tv => -1
    then
;

\ find the vocab entry with matching word and lowest typ_val >= thresh
\ database.c:vocab
: vocab-best ( addr n typval-min -- typval-best | -1 )
    -1 swap 2swap VOCAB             ( -1 tvmin addr n vptr )
    begin
        vocab-match                 ( best tvmin addr n vptr' [tv | -1] )
        dup -1 <> if                \ found a hit?
            2swap 2>r 2swap         ( vptr' tv best tvmin   R: addr n )
            rot 2dup u<= if         ( vptr' best tvmin tv )
                rot umin swap
            else
                drop
            then                    ( vptr' best' tvmin )
            rot 2r> rot
        else
            drop
        then                        ( best' tvmin addr n vptr' )
        ?dup 0=
    until                           ( best tvmin addr n )
    2drop drop
;

2variable tmp-cstr

\ english.c:analyze
: analyze ( addr n -- [typ-val true | false] )
    2dup tmp-cstr 2!
    ?dup 0= if
        drop false exit then
    0 vocab-best dup -1 = if            \ typ-val
        drop false dunno exit           \ drop result and ptr
    then

    dup tmp-cstr 2@ rot                 \ ( tv addr n tv )
    unpack nip VERB-WORD = if           \ update either verb or nonverb
         last-verb-cstr else
         last-nonverb-cstr then
    2! true
;

: bad-grammar ( -- )
    ." bad grammar..." CR
;

\ english.c:english
: english ( -- true | false )
    0 verb ! 0 object ! 0 motion !

    user-input cleave analyze 0= if         \ addr n ( tv 1 | 0 )
        2drop false exit
    then
    dup [ 'SAY VERB-WORD pack ] literal = if
        drop 'SAY verb ! 1 object !         \ oddly requires you have keys?
        last-nonverb-cstr 2!                \ save remaining text
        true exit
    then                                    \ addr n tv
    -rot cleave 2swap 2drop                 \ tv1 addr n
    ?dup 0= if                              \ empty second word shouldn't fail
        drop -1 else                        \ tv1 -1
        analyze 0=                          \ unknown second word does fail
            if drop false exit then
    then                                    \ tv1 tv2
    2dup = over [ 51 SPECIAL-WORD pack ] literal = and if
        speak-vocab false exit
    then
    unpack rot unpack rot swap              \ v2 v1 t2 t1
    dup SPECIAL-WORD = if
        2drop nip  speak-message false exit
    then
    over SPECIAL-WORD = if
        2drop drop speak-message false exit
    then
    2dup MOTION-WORD = if
        MOTION-WORD = if
            bad-grammar 2drop 2drop false exit
        then
        2drop motion ! drop true exit else
        drop
    then
    over MOTION-WORD = if
        2drop drop motion ! true exit
    then
    dup OBJECT-WORD = if                \ v2 v1 t2 t1
        drop swap object !              \ v2 t2
        dup VERB-WORD = if
            drop verb ! true exit
        then
        nip OBJECT-WORD = if
            bad-grammar false
            true then
        exit
    then
    dup VERB-WORD = if                  \ v2 v1 t2 t1
        drop swap verb !                \ v2 t2
        dup OBJECT-WORD = if
            drop object ! true exit
        then
        nip VERB-WORD = if
            bad-grammar false else
            true then
        exit
    then
    36 bug
;
