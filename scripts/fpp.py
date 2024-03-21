import re
from os import path
import sys
from optparse import OptionParser


consts = {}
referenced = set()


def define_const(m):
    val = m.group(1)
    val = inline_consts(val)
    if ' ' in val:
        val = re.sub(r'\$([0-9a-fA-F]+)\b', r'0x\1', val)
        # only allow v1 v2 op
        try:
            a, b, c = val.split(' ')
            val = str(eval(f"{a} {c} {b}"))
        except:
            print(f"fpp:error: failed to evaluate constant in \"{m.group(0)}\"")
            sys.exit()

    consts[m.group(2)] = val
    return ''

warned = set()

def ref_const(m):
    c = m.group(0)
    if c not in consts:
        if c not in warned:
            warned.add(c)
            print(f"fpp:warning: no definition for constant {c}")
        return c
    referenced.add(c)
    return consts[c]

def extract_consts(s):
    return re.sub(r"^(.*?)\s+constant\s+(['A-Z][A-Z&-_]+)$", define_const, s)

def inline_consts(s):
    return re.sub(r"['A-Z][A-Z&-_]+", ref_const, s)

def strip_comments(s: str) -> list[str]:
    lines = s.splitlines()
    return [
        re.sub(r'\(\s.*?\s\)(?!\S)', ' ',       # strip ( comments )
            re.sub(r'(\s|^)\s*\\(?!\S).*', '', line).rstrip()  # strip \ comments
        )
        for line in lines
    ]

def include_files(lines, basedir):
    out = []
    for line in lines:
        m = re.match(r'include (\S+)', line.strip())
        if m:
            out += read_lines(m.group(1), basedir)
        else:
            out.append(line)
    return out

def read_lines(fname, basedir=''):
    fname = path.join(basedir, fname)
    return include_files(strip_comments(open(fname).read()), basedir)


parser = OptionParser()
parser.add_option("-o", "--outfile", dest="outfile",
                  help="write to FILE", metavar="FILE")
parser.add_option("-c", "--consts-inline", action="store_true", default=False, dest="inline",
                  help="inline forth constants to save dictionary space")
parser.add_option("-s", "--strip-whitespace", action="store_true", default=False, dest="strip",
                  help="inline forth constants to save dictionary space")

(options, args) = parser.parse_args()

if len(args) != 1:
    parser.print_usage()
    sys.exit()

infile = args[0]
outfile = options.outfile
if not outfile:
    base, ext = path.splitext(infile)
    outfile = f"{base}_fpp{ext}"

print(f"fpp: {infile} => {outfile}")

lines = read_lines(path.basename(infile), basedir = path.dirname(infile))

if options.inline:
    lines = map(inline_consts, map(extract_consts, lines))

if options.strip:
    out = '\n'.join(line.strip() for line in lines if line)
else:
    out = '\n'.join(line for line in lines if line)

n = open(outfile, "w").write(out + '\n')
print(f"fpp: wrote {n} bytes")
