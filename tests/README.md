These test cases aim to exercise all of the core functionality.
They're adapted from https://www.mipmip.org/dev/IFrescue/ajf/Universal350.html

Test inputs are found in `excursion#.txt`.
Lines commented as `#F` or `#C` are included only for Forth or C respectively.
Otherwise trailing comments following `#` are ignored.

It's hard to compare outputs precisely because (a) we use dynamic line wrapping in the Forth version
and (b) the random number generator is different so random events occur in different orders.
We use a fixed random seed to for the most part I've handled random events in the Forth flow
to make sure that things work as expected.

The top level `make tests` uses `canonical.py` here to do some standardization on the output
before writing to `excursion3.[fs|c].log`.   There's currently no automated correctness test
so we rely on visual comparison in an editor or a word-based compare like `wdiff -3 -n excursion2*log`.
