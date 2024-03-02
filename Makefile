all: data/advent.blk

clean:
	rm data/advent_fpp.fs data/advent.dat data/advent.blk

data/advent.blk: data/advent_fpp.fs data/advent.dat
	python scripts/advblk.py

data/advent.dat: data/advent.json
	python scripts/advpack.py

data/%_fpp.fs: src/%.fs
	python scripts/fpp.py $< -o $@
