all: rom

clean:
	rm -f data/*_fpp.fs data/advent.dat data/advent.blk data/advent.rom

rom: build data/advent.rom

runtime:
	cd ../tali && $(MAKE) taliforth-adventure.bin

data/advent.rom: data/advent.blk
	dd bs=1024 skip=64 count=64 if=data/advent.blk > $@

build: data/advent.blk runtime
	../tali/c65/c65 -r ../tali/taliforth-adventure.bin -m 0xc000 -b $<

play: data/advent.rom
	TURNKEY=$(shell grep _turnkey: ../tali/docs/adventure-listing.txt | cut -c2-5); \
	py65mon -l $< -g $$TURNKEY -m 65c02 -i 0xc004 -o 0xc001

data/advent.blk: scripts/advblk.py data/boot_fpp.fs data/advent_fpp.fs data/advent.dat
	python scripts/advblk.py

data/advent.dat: scripts/advpack.py data/advent.json
	python scripts/advpack.py

data/advent.json: scripts/advextract.py
	python scripts/advextract.py

data/advent_fpp.fs: src/*.fs

data/%_fpp.fs: src/%.fs
	python scripts/fpp.py --strip-whitespace --consts-inline $< -o $@

excursions = $(wildcard tests/excursion*.txt)

tests: $(patsubst %.txt,%.fs.log,$(excursions)) $(patsubst %.txt,%.c.log,$(excursions))

%.fs.log: %.txt data/advent.rom tests/canonical.py
	grep -v '#C' $< | grep -o '^[^#]*' | ../tali/c65/c65 -g 0xc14a -r data/advent.rom -b data/advent.blk -m 0xc000 | python tests/canonical.py > $@

%.c.log: %.txt
	grep -v '#F' $< | grep -o '^[^#]*' | ../adventure-original/src/advent | python tests/canonical.py > $@
