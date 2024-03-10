\ startup code used in tali user_words

\ boot from a block device if present
\ block 0 is read, and is `evaluate`d from offset 2 if offset 0 contains 'TF'

: blkrw ( blk buf action -- )
    -rot $c014 ! $c012 ! $c010 c!
;

: blkboot ( -- )
    -1 $c011 c! 0 $c010 c! $c011 c@ 0= if        \ block device available?
        0 $1000 1 blkrw
        $1000 @ $4654 = if                       \ starts with magic "TF" ?
            $1002 asciiz> evaluate else          \ run the block
            ." bad boot block" CR
        then else
        ." no block device" CR
    then
;

blkboot
