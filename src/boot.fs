\ read/write blk...blk+n-1 to/from addr
: rwblocks ( blk addr n action -- )
    rot 2swap                   ( action addr blk n )
    over + swap do              ( action buf )
        2dup i -rot swap blkrw  \ generate blk buf action leaving ( action buf )
        1024 +
    loop
    2drop
;

\ define data pointers
${data_start:04x} constant ADVDAT
{consts}

\ load and compile forth source
{forth_blk} $2000 {forth_blocks} 1 rwblocks
\ count length to nul
$2000 asciiz> dup

s" Compiling " type u. s" bytes ... " type CR
here -rot

evaluate

here swap -
s" ... used " type u. s" bytes" type CR
ADVDAT here - . s" bytes remain before ADVDAT" type CR

\ now we have space to load data; zeroing up to 1K trailing into accept buffers @ $bc00
{data_blk} ADVDAT {data_blocks} 1 rwblocks

\ turnkey entry point
' main $7e !
64 0 64 2 rwblocks

main
