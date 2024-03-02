from struct import pack


def blocklen(d):
    return (len(d) - 1) // 1024 + 1


data = open('data/advent.dat', 'rb').read()
forth = open('data/advent_fpp.fs', 'rb').read()


data_blk = 1
forth_blk = 1 + blocklen(data)
header = pack("<HHHH", data_blk, len(data), forth_blk, len(forth))

out = bytearray(64*1024)

out[0:len(header)] = header
off = data_blk*1024
out[off:off+len(data)] = data
off = forth_blk*1024
out[off:off+len(forth)] = forth

outfile = 'data/advent.blk'
print(f"advblk: writing {blocklen(out)} blocks to {outfile}")
open(outfile, 'wb').write(out)
