all: rom

clean:
	rm -f data/*_fpp.fs data/advent.dat data/advent.blk data/advent.rom

rom: data/advent.rom

data/advent.rom: build
	dd bs=1024 skip=64 count=64 if=data/advent.blk > $@

build: data/advent.blk
	../tali/c65/c65 -r ../breadboard-rom/advent.bin -i 0xc004 -o 0xc001 -x 0xc010 -b $<

play: data/advent.rom
	py65mon -l $< -g 0xc14a -m 65c02 -i 0xc004 -o 0xc001

data/advent.blk: scripts/advblk.py data/boot_fpp.fs data/advent_fpp.fs data/advent.dat
	python scripts/advblk.py

data/advent.dat: scripts/advpack.py data/advent.json
	python scripts/advpack.py

data/advent_fpp.fs: src/*.fs

data/%_fpp.fs: src/%.fs
	python scripts/fpp.py --strip-whitespace --consts-inline $< -o $@
