\ define data pointers
${data_start:04x} constant ADVDAT
{consts}

\ load and compile forth source
{forth_blk} $4000 {forth_blocks} blk-read-n
\ count length to nul
$4000 {forth_len} dup
s" Compiling " type u. s" bytes ... " type CR
here -rot
( at addr n )
evaluate
here swap -
s" ... used " type u. s" bytes" type CR
ADVDAT here - . s" bytes remain before ADVDAT" type CR
.s cr       \ stack should be empty after evaluate

' prop{{ turns -
s" Save/restore state " type u. s" bytes" type CR  \ should be < 1024

\ now we have space to load data...

\ copy last block first and move tail up 1024*(blks-1) to avoid overwriting "ROM"
{data_blk} {data_blocks} + 1- ADVDAT blk-read
ADVDAT {data_start} {data_blocks} 1- 10 lshift + {data_tail} move

\ then read the remaining whole blocks
{data_blk} ADVDAT {data_blocks} 1- blk-read-n

\ save turnkey entry point and dump image
' main $fff8 !
\ bump reset vector +3 to do warm start
$fffc @ 3 + $fffc !
64 0 64 blk-write-n

main
