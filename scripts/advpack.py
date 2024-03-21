import struct
import json
import re
from itertools import accumulate
from dizzy import dizzy, undizzy, dizzy_squeeze, woozy, unwoozy, unwrap


def compact_word(lo: int, hi: int, words: list[str]) -> bytes:
    """
    byte 0: code % 1000 (<=147)
    byte 1: code // 1000 (<=3)
    byte 2: length of str 0 (<=15)  (hi bit set on last string)
    byte 3+: chars for str 0
    ...
    last string in group has high bit of length set, followed by next header
    last header is ff/ff
    """
    assert lo < 256 and hi < 4 and all([len(w) < 16 for w in words])
    n = len(words)
    cws = b''.join([
        bytes([len(w) + (0x80 if i == n-1 else 0)]) + w.encode('ascii')
        for i,w in enumerate(words)
    ])
    return bytes([lo, hi]) + cws

def compact_cave(long: bytes, short: bytes, travel: list[tuple[int,int,int,int,int]]):
    """
    compact representation for cave data
    the tuple looks like (dtyp, dest, verb, cflg, cobj) <= (2, 161, 307, 7, 95)
    so in bits dtyp:2, dest:8, verb:9, cflg:3, cobj:7 = 2+8+9+3+7 = 29
    which we represent in a uint32 as

    +-----------------+-----------------+-----------------+-----------------+
    | 7 6 5 4 3 2 1 0 | 7 6 5 4 3 2 1 0 | 7 6 5 4 3 2 1 0 | 7 6 5 4 3 2 1 0 |
    +------+-----+----+-----------------+--------------+--+-----------------+
    | . . .|  cf | dt |     dest        |     cobj     |          verb      |
    +------+-----+----+-----------------+--------------+--+-----------------+

    byte 0: offset to long (or 0 if none)
    byte 1: # travel
    byte 2: 4 x # travel uint32
    @short: strz
    [@long: strz]
    """
    dvcs = [
        v | (c << 9) | (d << 16) | (dt << 24) | (cf << 26)
        for (dt, d, v, cf, c) in travel
    ]

    if long == short:
        offset = 0
    else:
        offset = 2 + 4 * len(dvcs) + len(short)
        assert offset < 256

    data = struct.pack(f"<BB{len(dvcs)}I", offset, len(dvcs), *dvcs)
    data += short
    if offset:
        data += long
    return data

# fetch the extracted data
advent = json.load(open('data/advent.json'))

caves = advent['caves']

# was 27176
for c in caves:
    s = c['long']
    c['long'] = unwrap(c['long'])
    c['short'] = unwrap(c['short'])

corpus = (
    [c['long'] for c in caves]
    + [c['short'] for c in caves if c['short'] != c['long']]
    + advent['messages']
    + sum(advent['items'], [])
)

# make a digram lookup table
source = unwrap('\0'.join(corpus)+'\0')
print('corpus has', len(corpus), 'strings', len(source), 'total characters as strz')
open('scripts/corpus.asc', 'w').write(source)
data, digrams = dizzy(woozy(source))

print(f"dizzy compress {len(source)} source bytes to {len(data)} compressed bytes "
    f"with {len(digrams)} digrams. uncompress ok? {unwoozy(undizzy(data, digrams)) == source}")

max_sqz = dict(raw=0, woozy=0, sqz=0)
def sqz(s):
    w = woozy(unwrap(s))
    z = dizzy_squeeze(w, digrams) + b'\0'
    for (t, d) in zip(['raw', 'woozy', 'sqz'], [s, w+b'\0', z]):
        max_sqz[t] = max(max_sqz[t], len(d))
    return z

"""
print(', '.join(f'${x:02x},${y:02x}' for x,y in d.values()))
print(', '.join(f'${x:02x}' for x in z.split(b'\0')[0]))
print(src.split(b'\0')[0].decode('ascii'))
"""

def pack_index(zs: list[bytes]) -> bytes:
    offsets = list(accumulate((len(z) for z in zs), initial=0))[:-1]
    idx = struct.pack(f"<{len(offsets)}H", *offsets)
    assert len(offsets) == len(zs)
    return idx


# digrams
print(f"??? constant ADVDAT")
data = b''.join(digrams.values())
bin = data
header = struct.pack("<H", len(data))
print(f"DIGRAMS {len(data)} bytes")

# words
# first group as (lo, hi): [ words ]
wdict = {}
for (w, lo, hi) in advent['words']:
    wdict.setdefault((lo, hi), []).append(w)

data = b''.join(
    compact_word(lo, hi, ws) for (lo, hi), ws in wdict.items()
) + bytes([0xff, 0xff])

print(f"VOCAB   {len(data)} bytes")
bin += data
header += struct.pack("<H", len(data))

# caves
zs = [compact_cave(sqz(c['long']), sqz(c['short']), c['travel']) for c in advent['caves']]
idx = pack_index(zs)
data = b''.join(zs)
print(f"CAVES&  {len(idx)} bytes")
bin += idx
print(f"CAVES   {len(data)} bytes")
bin += data
header += struct.pack("<HH", len(idx), len(data))

# messages
zs = [sqz(msg) for msg in advent['messages']]
idx = pack_index(zs)
data = b''.join(zs)
print(f"MSGS&   {len(idx)} bytes")
bin += idx
print(f"MSGS    {len(data)} bytes")
bin += data
header += struct.pack("<HH", len(idx), len(data))

# items
zs = [b''.join(sqz(s) for s in states) for states in advent['items']]
idx = pack_index(zs)
data = b''.join(zs)
print(f"ITEMS&  {len(idx)} bytes")
bin += idx
print(f"ITEMS   {len(data)} bytes")
bin += data
header += struct.pack("<HH", len(idx), len(data))

bin = struct.pack("<H", len(header)//2) + header + bin
print(f"longest string {max_sqz['raw']}, woozy {max_sqz['woozy']}, packed {max_sqz['sqz']}")
open('data/advent.dat', 'wb').write(bin)
print(f"Wrote {len(bin)} bytes to advent.dat")
