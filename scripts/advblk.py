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
data_start = 0xc000 - len(data)

addrs = [data_start + off for off in [0] + offsets[:-1]]
consts = '\n'.join([
    f"${addr:04x} constant {section}"
    for addr, section in zip(addrs, sections)
])

if 0x4000 + len(forth) > 0xc000:
    warn("Forth code @ $4000 overlaps IO space")

template = open('data/boot_fpp.fs').read()
data_blocks=nblocks(data)
data_tail=(len(data) % 1024) or 1024
forth_blocks=nblocks(forth)
forth_len=len(forth)
boot = eval('f"""' + template + '"""')

open('data/boot_fpp_fmt.fs', 'w').write(boot)
if len(boot) + 2 > 1024:
    warn("Boot block too long: {len(boot)} > 1024")

out = bytearray(64*1024)
out[0:4+len(boot)] = 'TF'.encode('ascii') + pack('<h', len(boot)) + boot.encode('ascii')

off = data_blk*1024
out[off:off+len(data)] = data
off = forth_blk*1024
out[off:off+len(forth)] = forth

outfile = 'data/advent.blk'
open(outfile, 'wb').write(out)
print(f"advblk: wrote {len(forth)} source + {len(data)} data bytes to {outfile}")
