"""
Create a block device image for TF boot.fs

Boot block 0 starts with TF followed by bootstrap forth code which is `evaluate`d
"""
from struct import unpack_from, pack
from warnings import warn
from itertools import accumulate

def nblocks(d):
    return (len(d) - 1) // 1024 + 1

sections = 'DIGRAMS VOCAB cond{ CAVES& CAVES MSGS& MSGS ITEMS& ITEMS'.split()

forth = open('data/advent_fpp.fs', 'rb').read() + bytes([0])
data = open('data/advent.dat', 'rb').read()

forth_len = len(forth)

(n,) = unpack_from("<H", data)
assert len(sections) == n, f"advblk: advent.dat has {n} sections, expected {len(sections)}"

sizes = unpack_from(f"<{n}H", data[2:])
offsets = list(accumulate(sizes))

data = data[2*(n+1):]           # drop the constants
data_len = len(data)
assert data_len == offsets[-1]

forth_blocks = nblocks(forth)
data_blocks = nblocks(data)

forth_blk = 1
data_blk = forth_blk + forth_blocks

data_start = 0xc000 - len(data)

addrs = [data_start + off for off in [0] + offsets[:-1]]
consts = '\n'.join([
    f"${addr:04x} constant {section}"
    for addr, section in zip(addrs, sections)
])

if 0x4000 + len(forth) > 0xc000:
    warn("Forth code @ $4000 overlaps IO space")

# read the boot template after fpp pre-processing
template = open('data/boot_fpp.fs').read()
boot = eval('f"""' + template + '"""')

open('data/boot_fpp_fmt.fs', 'w').write(boot)
if len(boot) + 2 > 1024:
    warn("Boot block too long: {len(boot)} > 1024")

out = bytearray(64*1024)
out[0:4+len(boot)] = 'TF'.encode('ascii') + pack('<h', len(boot)) + boot.encode('ascii')

off = forth_blk*1024
out[off:off+forth_len] = forth

# 'right align' the data to the end of the last block
off = data_blk*1024 + data_blocks*1024 - data_len
print(f"data at {off}:{off+data_len}")
out[off:off+data_len] = data

outfile = 'data/advent.blk'
open(outfile, 'wb').write(out)
print(f"advblk: wrote {len(forth)} source + {len(data)} data bytes to {outfile}")
