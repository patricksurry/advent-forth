This is a 65c02 Forth port of Crowther & Wood's [Colossal Cave Adventure](https://en.wikipedia.org/wiki/Colossal_Cave_Adventure) targeting a 64K system with 48K of RAM and 16K of ROM.
My [&micro;65c02 hardware](https://github.com/patricksurry/micro-colossus) demonstrates
a working setup but the code should be adaptable to similar systems.

TL;DR
---

You can skip the hardware and play on a simulator using a prebuilt
[64K memory image](data/advent.rom).  Two options are
[py65mon](https://github.com/mnaberez/py65) which is easier
to install, and
[c65](https://github.com/SamCoVT/TaliForth2/tree/master-64tass/c65)
which is much faster and supports external storage.

1. Download a copy of [`data/advent.rom`](data/advent.rom)
2. Pick a simulator:
  - (easier) install [py65](https://github.com/mnaberez/py65)
  - clone the [Taliforth repo](https://github.com/SamCoVT/TaliForth2) and build
    the standalone [c65 simulator](https://github.com/SamCoVT/TaliForth2/tree/master-64tass/c65);
3. Run with one of:
  - `py65mon -m 65c02 -r advent.rom -i ffe4 -o ffe1`
  - `c65 -r advent.rom -m 0xffe0`

If you want to save and load games you'll need to use `c65` and create an empty block device:
```sh
touch advent.blk
c65 -r advent.rom -m 0xffe0 -b advent.blk
```

Background
---

I started from [Wiberg's C-port](https://github.com/troglobit/adventure) with some later
adaptations and fixes from [Gillogly's earlier C-port](https://www.ifarchive.org/indexes/if-archive/games/source/)
which contains some more direct translation of the original(?) FORTRAN code.
Other useful references were [Raymond's Open Adventure](https://gitlab.com/esr/open-adventure) and
and the [Universal Adventure 350 Walkthrough](https://www.mipmip.org/dev/IFrescue/ajf/Universal350.html).

The main challenge was size, given the 64K target:  the original `glorkz` data file that
drives the game is already 56K before adding any logic.
Wiberg's port extracts most string data to `advent?.txt`
which together weight in at 47K but also embeds some data in the source files.

I first created some scripts to extract and reorganize the cave description,
connectivity and object data.  This is compressed using some simple preprocessing
followed by a recursive [digram coding scheme](https://en.wikipedia.org/wiki/Byte_pair_encoding).
The compression ratio is about 50% which results in the 27K
binary data file `data/advent.dat`.
The two stage decompression was straightforward to implement in assembly,
though I used a streaming approach to avoid the need for fixed size buffers in memory.
Space is tight!
See [scripts/README.md](scripts/README.md) for more details

Compression left about 37K for the Forth kernel plus the ported game source code.
I used the awesome [TaliForth2](https://github.com/SamCoVT/TaliForth2) project
for my kernel, but the vanilla build wants nearly 24K of ROM.  With some adaptation
I was able to reduce to a "minimal" build that used about 12K of my 16K ROM.
This left just enough space for my kernel hardware drivers
along with a few assembler routines like decompression to support the game.
I've currently got a couple of pages of ROM free.

The remaining 21K RAM budget (48K less 27K of data) was tight but doable for the game code.
The raw Forth source is nearly 64K of ascii text, but compacts to about 27K
with some light pre-processing to inline constants and strip comments and excess whitespace
(see [scripts/fpp.py](fpp.py)).

A little dance then ensues to build the game image.   The source is compacted
and written to a block device image, along with the binary data file.
A tiny Forth bootloader reads the source
into high memory from the block device and compiles it, using about 19K.
The source is discarded and the data file is written above the code
aligned to the end of RAM.  This leaves the game ready to play.

Currently the loader also dumps the entire 64K memory image back to the block device
in order to support simulator play without compilation or block storage.

HOW IT WORKS
---

There are a lot of moving parts here.  We currently assume the "hardware"
has RAM from $0-bfff, IO from $c000-c0ff and ROM from $c100-ffff.
Both hardware and simulator support an external block device for reading
or writing 1K blocks.  This is required for building from source, but
we can grab a prebuilt 64K memory image which is enough for playing in a simulator.

The [`micro-colossus`](https://github.com/patricksurry/micro-colossus)
repo implements a [Taliforth2 platform](https://github.com/SamCoVT/TaliForth2/tree/master-64tass/platform)
which depends on my [`adventure` branch of Taliforth](https://github.com/patricksurry/TaliForth2/tree/adventure).
The &micro;65c02 `Makefile` generates both `uc.rom` for my hardware
and `ucs.rom` for the simulators, along with symbol files for debugging.

The ROM kernel initializes the hardware, displays a splash message
and starts Forth.
The simulator runs almost identical
code for debugging purposes but is largely a no-op.
Forth checks for a turnkey routine to run on startup.
This is normally the `block-boot` word which tries to read block zero
from the SD card (or simulated block device).
On success it verifies a magic byte pair and evaluates the
block content as Forth code, finally executing the resulting word
to bootstrap the rest of the loading process.

The Colossal Cave loader in `boot.fs` loads and compiles the game source and
data, and swaps the turnkey word to point at the compiled game
so we can dump a prepared memory image.
The loader includes various constants calculated while preparing
the game data which are injected by `scripts/advblk.py`.
The data itself is extracted and compressed by `scripts/advextract.py`
and `scripts/advpack.py` using the compression routines in `scripts/dizzy.py`.

The game source itself is rooted at `src/advent.fs` and is pre-processed
to `data/advent_fpp.fs` by `scripts/fpp.py` which resolves includes and
inlines all constants to save space in the compiled image.

KNOWN ISSUES
---

- batteries are broken in the C source I started from, and may not be complete in Forth

- "fee fie foe foo" might not be working?

- Currently the kernel uses completely reproducible "random" numbers which
  makes debugging and testing much easier.  A simple fix for much more randomness
  would increment the random seed during the KEY? busy loop.

TODO
---

- tests:

  - end game from https://www.mipmip.org/dev/IFrescue/ajf/Universal350.html
  - add a hint excursion to verify the resurrected hint code
  - automate tester.fs over all the words in the object array in Makefile
