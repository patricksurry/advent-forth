C65 = ../tali/c65/c65
PYTHON = python3

all: rom

clean:
	rm -f data/*_fpp.fs data/advent.dat data/advent.blk data/advent.rom

rom: build data/advent.rom

runtime:
	cd ../micro-colossus && $(MAKE)

data/advent.rom: data/advent.blk
	dd bs=1024 skip=64 count=64 if=data/advent.blk > $@

build: data/advent.blk runtime
	echo 'no\nquit\ny\nbye' | \
	$(C65) -r ../micro-colossus/colossus.rom -m 0xc000 -b $<

play: data/advent.rom
	$(C65) -r data/advent.rom -m 0xc000

debug: data/advent.rom
	$(C65) -r data/advent.rom -m 0xc000 -l ../micro-colossus/colossus.lst -gg

data/advent.blk: scripts/advblk.py data/boot_fpp.fs data/advent_fpp.fs data/advent.dat
	$(PYTHON) scripts/advblk.py

data/advent.dat: scripts/advpack.py data/advent.json
	$(PYTHON) scripts/advpack.py

data/advent.json: scripts/advextract.py
	$(PYTHON) scripts/advextract.py

data/advent_fpp.fs: src/*.fs

data/%_fpp.fs: src/%.fs
	$(PYTHON) scripts/fpp.py --strip-whitespace --consts-inline $< -o $@

excursions = $(wildcard tests/excursion*.txt)

tests: $(patsubst %.txt,%.fs.log,$(excursions)) $(patsubst %.txt,%.c.log,$(excursions))

.FORCE:

%.fs.log: %.txt .FORCE
	grep -v '#C' $< | sed -E 's/ *(#.*)?$$//' | \
	$(C65) -r data/advent.rom -b data/advent.blk -m 0xc000 | \
	$(PYTHON) tests/canonical.py > $@

%.c.log: %.txt .FORCE
	grep -v '#F' $< | sed -E 's/ *(#.*)?$$//' | \
	../adventure-original/src/advent | \
	$(PYTHON) tests/canonical.py > $@
