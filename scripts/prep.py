import re

def encword(m):
    w = m.group(1)
    if w.capitalize() == w:
        case = '\x01 '
    elif w.upper() == w:
        case = '\x02 '
    else:
        case = ''
    return case + (w.lower() if case else w) + ' '

s = open('alice.asc').read()

# get rid of wrapping newlines
s = re.sub(r'\n(\w)', r' \1', s)

# s = re.sub(r'\b(\w+)\b', encword, s)

# capitalized word
# all caps word
# other caps
s = re.sub(r'\b[A-Z]+\b', lambda m: '\x02' + m.group().lower(), s)
s = re.sub(r'[A-Z]', lambda m: '\x01' + m.group().lower(), s)
s = re.sub(' \x01?[a-z]', lambda m: m.group()[1:-1] + m.group()[-1].upper(), s)
print(repr(s[:512]))
