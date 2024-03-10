all: rom

clean:
	rm -f data/*_fpp.fs data/advent.dat data/advent.blk data/advent.rom

rom: build data/advent.rom

runtime:
	cd ../tali && $(MAKE) taliforth-adventure.bin

data/advent.rom: data/advent.blk
	dd bs=1024 skip=64 count=64 if=data/advent.blk > $@

build: data/advent.blk runtime
	../tali/c65/c65 -r ../tali/taliforth-adventure.bin -i 0xc004 -o 0xc001 -x 0xc010 -b $<

play: data/advent.rom
	TURNKEY=$(shell grep _turnkey: ../tali/docs/adventure-listing.txt | cut -c2-5); \
	py65mon -l $< -g $$TURNKEY -m 65c02 -i 0xc004 -o 0xc001

data/advent.blk: scripts/advblk.py data/boot_fpp.fs data/advent_fpp.fs data/advent.dat
	python scripts/advblk.py

data/advent.dat: scripts/advpack.py data/advent.json
	python scripts/advpack.py

data/advent_fpp.fs: src/*.fs

data/%_fpp.fs: src/%.fs
	python scripts/fpp.py --strip-whitespace --consts-inline $< -o $@
