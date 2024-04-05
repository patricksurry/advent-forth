\ TODO items
\ - run tester.fs on all the words in the object array
\ - make defs an init function so we can restart (advent.c:initplay)
\ - save/restore (see advent.c)
\ - debug flag?
\ - open-adventure log random calls to replicate
\ - could save about 300 bytes by shortening all words to max 6 chars, about 650 to 2char (unreadable)
\ - move c}@!; <= etc; blkrw and friends to native words
\ - is c!@ useful enough?

There are a lot of moving parts here.

We assume I/O is in the $c000 page, currently with a magic block read device.

TaliForth2 is compiled to org at $c100 using its normal Makefile. This
includes some startup user words based on forth/init.fs (see below).

    cd ..\TaliForth2
    make

This generates taliforth-py65mon.bin and taliforth-py65mon.sym which is used
as the baseline for our ROM image, and for our shims to link
to Tali compiled words

We add our kernel routes shimmed in Tali's user word gap at $f900
via

    ../cc65/bin/cl65 --target none -vm -m advent.map -l advent.lst -o advent.bin --config advent.cfg txt.asm

Note this depends on the .bin and .sym files produced earlier.
The .map file produced here shows the location of our native routines
which are configured in data.fs. These words could just
be compiled directly into Tali but we want them for our standalone kernel
as well using cc65.

The TaliForth user words (essentially init.fs) bootstrap
from the block device. This reads advent.dat to ADVDAT
and advent_fpp.fs from advent.blk, and then evaluates the source.
ADVDAT must be configured so that the block length of the data file
doesn't overwrite the Tali's accept buffer space at $BC00.
We build advent_fpp.fs with forth/fpp.py and
advent.dat with scripts/advpack.py, and combine to produce
the block input file advent.blk using forth/advblk.py.

    python forth/fpp.py forth/advent.fs   # => forth/advent_fpp.fs
    python scripts/advpack.py             # => data/advent.dat
    python forth/advblk.py                # => data/advent.blk

Finally we run the ROM image using prof65 (a C-based 6502 simulator) which we build (once) like:

      gcc -I../MyLittle6502 -o prof65 -lreadline prof65.c

and then run pointing to the block file like:

    ../prof65/prof65 -r ../breadboard-rom/advent.bin -i 0xc004 -o 0xc001 -x 0xc010 -b data/advent.blk

Older was running with pymon (lacks block device for startup)

      py65mon -m 65c02 -i c004 -o c001 -b forth/advent.mon

Tali word list

drop dup swap ! @ over >r r> r@ nip rot
-rot tuck , c@ c! +! execute emit type . u. u.r .r d. d.r ud. ud.r ? false
true space 0 1 2 2dup ?dup + - 1- 1+ 2* 2/ abs dabs and or xor rshift lshift
pick char [char] char+ chars cells cell+ here = <> < u< u> > 0= 0<> 0> 0< min
max 2drop 2swap 2over 2! 2@ 2variable 2constant 2literal 2r@ 2r> 2>r invert
negate dnegate c, bounds spaces bl -trailing -leading /string refill accept
input>r r>input unused depth key allot create does> variable constant value
to s>d d>s d- d+ erase blank fill find-name ' ['] name>int int>name
name>string >body defer latestxt latestnt parse-name parse execute-parsing
source source-id : ; :noname compile, [ ] literal sliteral ." s" s\" postpone
immediate compile-only never-native always-native allow-native nc-limit
strip-underflow abort abort" do ?do i j loop +loop exit unloop leave recurse
quit begin again state evaluate base digit? number >number hex decimal count
m* um\* _ um/mod sm/rem fm/mod / /mod mod _/mod \*/ \ move cmove> cmove pad
cleave hexstore within >in <# # #s #> hold sign output input cr page at-xy
marker words wordsize aligned align bell dump .s compare search find word (
.( if then else repeat until while case of endof endcase defer@ defer! is
action-of useraddr buffer: buffstatus buffblocknum blkbuffer scr blk
block-write block-write-vector block-read block-read-vector save-buffers
block update buffer empty-buffers flush load thru list see cold bye ok
