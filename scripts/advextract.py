"""
source files

advent1.txt: long desc [MAXLOCJ=140, max 1299, total 17207]
advent2.txt: short desc [MAXLOC=140, max 124, total 5567]
advent3.txt: items [MAXOBJ=64, max 438, total 5042]
advent4.txt: messages [MAXMSG=201, max 1395, total 16976]

advcave.h: 140 x ..16 x u32

advword.h: word/code x 306
"""

import re
import json


def slurp_text(fname: str):
    n = 0
    s = ''
    chunks = {}
    for line in open(fname).readlines():
        if line.strip() == f"#{n+1}":
            if n:
                chunks[n] = s.rstrip()
            s = ''
            n += 1
        else:
            s += line
    chunks[n] = s.rstrip()      # drop trailing newline on everything
    assert list(chunks.keys()) == list(range(1, len(chunks)+1))
    return list(chunks.values())


def read_words(fname: str):
    # read lines like { "above", 29 },
    lines = open(fname).read().splitlines()
    lines = [line for line in lines if re.match(r'\s+{.*},?\s*', line)]
    data = [eval(line.replace('{', '(').replace('}', ')').rstrip(',')) for line in lines]
    data = [(word, code % 1000, code // 1000) for (word, code) in data]
    return data


def parse_caves(fname: str):
    # each cave is represented as a line like 	"129044000,124077000,126028000,",
    # each 9 digit vlaue is read as long int but split in three secitons:
    # xxxyyymzz is tdest = xxx, tverb = yyy, tcond = mzz (m = modifier, zz is object)
    # - tverb is an index in advword.h;
    # - tdest further splits into <=300 (normal), 301-500 (special), 501+ (msg)
    lines = open(fname).read().splitlines()
    # one line per cave
    lines = [line for line in lines if re.match(r'\s*"[0-9,]+"?,\s*', line)]
    rows = [
        # one list of nine-digit strings per cave
        [('0'*9 + s)[-9:] for s in line.strip().rstrip(',').strip('"').split(',') if s]
        for line in lines
    ]
    dvcs = [
        # one list of 3-digit tuples per cave
        [(int(s[:-6]), int(s[-6:-3]), int(s[-3:])) for s in cave]
        for cave in rows
    ]
    data = [
        # one list of 5-digit tuples per cave
        [
            (
                0 if d<=300 else 1 if d <=500 else 2,
                d if d<=300 else d-300 if d<=500 else d-500,
                v,
                c // 100,
                c % 100,
            )
            for (d, v, c) in cave
        ] for cave in dvcs
    ]
    return data


text = {}
for i,typ in enumerate(['long', 'short', 'items', 'messages']):
    fname = f"src/advent{i+1}.txt"
    chunks = slurp_text(fname)
    print(f"{fname} {typ} n={len(chunks)} maxlen={max(len(s) for s in chunks)} chrs={sum(len(s) for s in chunks)}")
    text[typ] = chunks

dups = sum(text['long'][i] == text['short'][i] for i in range(len(text['short'])))
print('short == long dups {dups}')
fname = 'src/advword.h'
words = read_words(fname)
print(
    f"{fname} n={len(words)} u={len(set(word for (word, _, _) in words))} "
    f"maxlen={max(len(word) for (word, _, _ ) in words)} maxlo={max(lo for (_,lo,_) in words)} "
    f"maxhi={max(hi for (_,_,hi) in words)}"
)

def maxtpl(ts: list[tuple]) -> tuple:
    return tuple(max(xs) for xs in zip(*ts))

fname = 'src/advcave.h'
travel = parse_caves(fname)
print(f"{fname} n={len(travel)} maxdir={max(len(cave) for cave in travel)} maxval={maxtpl(sum(travel, []))}")

from collections import Counter
print(dict(sorted(Counter(d for dirs in travel for (t,d,v,m,c) in dirs).items())))

items = [chunk.strip().strip('/').split('/') for chunk in text['items']]
items = [[s.rstrip() for s in states] for states in items]
print(f"items n={len(items)} maxstate={max(len(ds) for ds in items)} "
    f"maxlen={max(len(d) for d in sum(items, []))}")

advent = dict(
    words = words,
    messages = text['messages'],
    items = items,
    caves = [
        dict(
            long=text['long'][i],
            short=text['short'][i],
            travel=travel[i],
        )
        for i in range(len(travel))
    ],
)

json.dump(advent, open('data/advent.json', 'w'), indent=4)
