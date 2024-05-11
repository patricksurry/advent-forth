import sys
import re

prev = ''
for line in sys.stdin:
    line = line.strip()

    if re.match(r'> .*[a-z]', line):     # C version doesn't echo Y/N
        print('> ')
        prev = line[2:]
        continue

    # strip debug info
    if line.startswith('#') or 'c65:' in line:
        continue

    if not line.startswith('>') and (prev or (len(line) > 49 and line[-1] not in '!.')):
        if prev:
            prev += ' ' + line
        else:
            prev = line
        continue

    if prev:
        print(prev)
        prev = ''

    print(line)

if prev:
    print(prev)
