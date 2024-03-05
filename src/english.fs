\ each VOCAB entry is [ val typ len <chars> ], ending with [0 0 0]

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

: vocab-best ( addr n typval-min -- typval-best | -1 )
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

2variable tmp-cstr

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
    ." english " .s CR
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
