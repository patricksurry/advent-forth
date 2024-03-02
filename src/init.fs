\ this is not included in advent.fs but embedded in tali as startup code

$c010 constant BLKACT
BLKACT 1+ constant BLKSTAT
BLKACT 2 + constant BLKNUM
BLKACT 4 + constant BLKBUF

: blkio-status ( -- flag )
    1 BLKSTAT ! 0 BLKACT ! BLKSTAT @
;

: blkrw ( blk buf action -- )
    -rot BLKBUF ! BLKNUM ! BLKACT !
;

: blkio-read ( blk buf -- )
    1 blkrw
;

: blkio-write ( blk buf -- )
    2 blkrw
;

\ advent.dat 27001 bytes, 27 blocks
$bc00 1024 27 * - constant ADVDAT

\ data block start, data bytes, source block start, source bytes

0 $1000 blkio-read

: >blks ( bytes -- blks )
    1- 10 rshift 1+
;

: read-blocks ( blk addr n -- )
    0 do
        2dup blkio-read
        swap 1+ swap 1024 +
    loop
    2drop
;

$1000 @ ADVDAT $1002 @ >blks read-blocks
$2000 $1006 @ 2dup $1004 @ -rot >blks read-blocks
evaluate
