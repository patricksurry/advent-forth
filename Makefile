all: data/advent.blk

clean:
	rm data/advent_fpp.fs data/advent.dat data/advent.blk

data/advent.blk: scripts/advblk.py data/advent_fpp.fs data/advent.dat
	python scripts/advblk.py

data/advent.dat: scripts/advpack.py data/advent.json
	python scripts/advpack.py

data/%_fpp.fs: src/*.fs
	python scripts/fpp.py $< -o $@
