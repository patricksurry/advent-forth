Various python scripts used for the build process:

- advextract.py: extract text game data from C header files to data/advent.json
- advpack.py: compact data/advent.json to data/advent.dat using dizzy/woozy
- advblk.py: create a block device image to bootstrap compilation
- dizzy.py: dizzy and woozy routines implemented in python
- fpp.py: Preprocess and compact forth source to data/advent_fpp.fs

- test_dizzy.py: test dizzy compress/decompress
- test_prep.py: test woozy text compaction

- literals.py, opt.py: examine compiled game image for various optimization possibilities
