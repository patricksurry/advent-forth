This is a 65c02 Forth port of Crowther & Wood's [Colossal Cave Adventure](https://en.wikipedia.org/wiki/Colossal_Cave_Adventure) targeting a 64K system with 48K of RAM and 16K of ROM.
You can play the
[64K memory image](data/advent.rom) using a simulator like
[c65](https://github.com/SamCoVT/TaliForth2/tree/master-64tass/c65)
or [py65mon](https://github.com/mnaberez/py65)
or adapt the code for your own hardware.

I started from [Wiberg's C-port](https://github.com/troglobit/adventure) with some later
adaptations and fixes from [Gillogly's earlier C-port](https://www.ifarchive.org/indexes/if-archive/games/source/)
which contains some more direct translation of the original(?) FORTRAN code.
Other useful references were [Raymond's Open Adventure](https://gitlab.com/esr/open-adventure) and
and the [Universal Adventure 350 Walkthrough](https://www.mipmip.org/dev/IFrescue/ajf/Universal350.html).

The main challenge was size, given the 64K target:  the original `glorkz` data file that
drives the game is already 56K before adding any logic.
Wiberg's port extracts most string data to `advent?.txt`
which weigh in at 47K but also embeds some data in the source files.

I first created some scripts to extract and reorganize the cave description,
connectivity and object data and then compress it using some simple preprocessing
followed by a recursive digram coding scheme.
The compression ratio is about 50% compression resulting
in binary data file of about 27K in `data/advent.dat`.
The two stage decompression was straightforward to implement in assembly,
though I used a streaming approach to avoid the need for fixed size buffers in memory.
Space is tight!
See [scripts/README.md](scripts/README.md) for more details

Compression left about 37K for the Forth core and the ported source code.
I started with [TaliForth2](https://github.com/SamCoVT/TaliForth2) which
originally wanted about 24K within a 32K ROM image.  With some adaption
I settled on a "minimal" build that halved that, using about 12K
within 16K of ROM.  This left space for a few assembler extensions like
decompression and various kernel routines, with 2-3K of ROM free.

The remaining 21K RAM budget (48K - 27K of data) was tight but doable for the game code.
The raw Forth source is nearly 64K of ascii text, but compacts to about 27K
with some light pre-processing to inline constants and strip comments and excess whitespace
(see [scripts/fpp.py](fpp.py)).

A little dance then ensues to build the game image.   The source is compacted
and written to a block device image, along with the binary data file.
A tiny forth bootloader reads the source
into high memory from the block device and compiles it, using about 19K.
The source is discarded and the data file is written above the code
aligned with the end of RAM.   The loader then updates a turnkey startup
address and dumps the entire 64K back to the block device.
This lets us play from the memory image directly in a simulator without a block device,
or read a pre-compiled image from block storage.


## KNOWN ISSUES

- batteries are broken in C, not sure if they're complete in Forth

- fee fie foe foo not working?

## TODO

- update this README, split out TODO.md

- real kernel should increment rand16 l/h (skip 0) on peekc busy loop

- tests:

  - end game from https://www.mipmip.org/dev/IFrescue/ajf/Universal350.html
  - hint excursion
  - automate tester.fs over all the words in the object array in Makefile

- decompress:

  - split out dizzy to separate repo; optional decomp w/o wrap
  - how does dizzy do compressing forth?

- further optimization possibilities:
  - redo the bliteral/literal to share runtime
  - stack depth check cost?
  - could save about 300 bytes by shortening all words to max 6 chars, about 650 to 2char (unreadable)

## HOW IT WORKS

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




Write block image to SD card (OS X)

  > diskutil list  # see mounted drives

  # insert SD card

  > diskutil list # so we can find the SD card, for example:

  ...

  /dev/disk4 (external, physical):
    #:                       TYPE NAME                    SIZE       IDENTIFIER
    0:     FDisk_partition_scheme                        *31.9 GB    disk4
    1:             Windows_FAT_32 PRODOS1                 31.9 GB    disk4s1

  # unmount all numbered partitions, e.g. disk4s1 here:

  > diskutil unmountDisk /dev/disk4s1

  Unmount of all volumes on disk4 was successful

  # write to the *raw* disk using /dev/rdisk... instead of /dev/disk...
  # BE CAREFUL!

  sudo dd if=data/advent.blk of=/dev/rdisk4
