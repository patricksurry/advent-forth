\ Bootstrap forth source and adventure data to create turnkey image.
\ Block device image contains the preprocessed source and data blob
\ with pre-calculated data pointers
\ Our job is to:
\ - read the source code as a string and evaluate it;
\ - move the binary data into place;
\ - update the turnkey entry point and reset vector;
\ - dump the memory image back to the block device;
\ - start the game

\ define data pointers
${data_start:04x} constant ADVDAT
{consts}

\ load and compile forth source
$4000 {forth_blk} {forth_blocks} block-read-n
here $4000 {forth_len}
( at addr n )
here
s" Compiling {forth_len} bytes ... " type cr
here - allot    \ drop the string
evaluate
here dup rot -
( cp len )
s" ... used " type u. s" bytes" type cr
ADVDAT here - . s" bytes free before data blob" type cr

' prop{{ turns -
s" Save/restore uses " type u. s" bytes (max 1024)" type cr
here - allot    \ drop strings
.s cr           \ stack should be empty

\ now we have space to load data...
\ read data blocks to ADVDAT rounded down to a whole block since
\ the data is 'right-aligned' to the end of the last block

\ read first block and shuffle the data down to
\ avoid overwriting code with padding if we're tight
ADVDAT {data_blk} block-read
ADVDAT dup $fc00 and -          \ calculate leading space in first block
$400 over -                     \ data size in first block
swap ADVDAT + ADVDAT rot move   \ move data down
\ read remaining blocks
ADVDAT $fc00 and $400 +  {data_blk} 1+  {data_blocks} 1-  block-read-n

\ save turnkey entry point and dump image
' play $fff8 !
\ bump reset vector +3 to do warm start
$fffc @ 3 + $fffc !
0 64 64 block-write-n
\ leave the XT to execute once string eval is complete
' play
