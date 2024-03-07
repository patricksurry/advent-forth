"""
Create a block device image for TF boot.fs

Boot block 0 starts with TF followed by bootstrap forth code which is `evaluate`d
"""
from struct import unpack_from
from warnings import warn
from itertools import accumulate

def nblocks(d):
    return (len(d) - 1) // 1024 + 1

sections = 'DIGRAMS VOCAB CAVES& CAVES MSGS& MSGS ITEMS& ITEMS'.split()

data = open('data/advent.dat', 'rb').read()
forth = open('data/advent_fpp.fs', 'rb').read() + bytes([0])

(n,) = unpack_from("<H", data)
assert len(sections) == n, f"advblk: advent.dat has {n} sections, expected {len(sections)}"

sizes = unpack_from(f"<{n}H", data[2:])
offsets = list(accumulate(sizes))

data = data[2*(n+1):]
assert len(data) == offsets[-1]

data_blk = 1
forth_blk = data_blk + nblocks(data)
data_start = 0xbc00 - len(data)

addrs = [data_start + off for off in [0] + offsets[:-1]]
consts = '\n'.join([
    f"${addr:04x} constant {section}"
    for addr, section in zip(addrs, sections)
])

if 0x2000 + len(forth) > 0xbc00:
    warn("Forth code overlaps IO space")

if 0x4000 + len(data) > 0xbc00:
    warn("Data relocation overlaps IO space")

boot = f"""TF
: read-blocks ( blk addr n -- )
    0 do
        2dup 1 blkrw
        swap 1+ swap 1024 +
    loop
    2drop
;

${data_start:04x} constant ADVDAT
{consts}

: relocate-data
    here over < invert if ." no data space" then
    {data_blk} over {nblocks(data)} read-blocks
    ADVDAT {len(data)} move
;

{forth_blk} $2000 {nblocks(forth)} read-blocks
$2000 asciiz> evaluate
$4000 relocate-data
"""

out = bytearray(64*1024)
out[0:len(boot)] = boot.encode('ascii')

off = data_blk*1024
out[off:off+len(data)] = data
off = forth_blk*1024
out[off:off+len(forth)] = forth

outfile = 'data/advent.blk'
open(outfile, 'wb').write(out)
print(f"advblk: wrote {len(forth)} source + {len(data)} data bytes as {nblocks(out)} blocks to {outfile}")
