import dizzy

# hexdump -s 18 -n 256 -C ../data/advent.dat | cut -c 11-60
raw = bytes(map(
    lambda s: int(s, 16),
    """
68 65 6f 75 72 65 54 80  69 6e 6e 64 6c 6c 2e 0b
49 6e 73 74 84 67 69 74  0b 79 65 72 61 72 4f 66
65 73 59 81 49 73 8c 81  6f 72 6f 77 61 74 61 6e
61 73 6f 6e 54 6f 6f 6d  74 80 0b 9c 76 65 6c 65
67 65 41 82 65 6e 41 85  61 86 65 64 93 a1 54 68
42 65 61 64 74 68 98 73  69 64 52 6f 27 82 65 82
41 4c 63 6b 6c 79 67 68  ab 61 69 73 63 68 43 97
41 74 6c 6f 4f 6e 53 74  a6 88 82 92 65 a9 50 b4
61 6d 65 74 49 74 75 74  93 ae 6f 74 8b 68 72 6f
57 c6 69 89 4d 61 74 9f  63 74 41 6e 72 61 8f 83
45 85 48 af 61 79 57 68  87 83 61 9e 63 65 69 72
76 8d 53 65 6c 64 45 78  65 89 55 70 69 99 ad 9b
8b cb 41 6c 9d bd 20 22  6b 65 69 b3 52 65 46 72
50 8b 61 89 72 79 88 83  e7 9b 6e 74 27 74 9a 83
ac 65 72 81 c4 88 45 e9  61 6c 4e 65 bf a0 57 dc
e5 74 90 2c 41 86 61 b1  bc 41 bf 67 44 69 96 65
    """.split()
))
digrams = {i+128: raw[i*2:i*2+2] for i in range(128)}
print(len(digrams))
s = "\n                              Welcome to \n\n                       Colossal Cave Adventure!\n\n\n               Original development by Willie Crowther.\n                  Major features added by Don Woods.\n                 Conversion to BDS C by J. R. Jaeger.\n                Unix standardization by Jerry D. Pohl.\n            Port to QNX 4 and bug fixes by James Lummel.\n\nWould you like instructions?"
# s = "The trees of the forest are large hardwood oak and maple,\nwith an occasional grove of pine or spruce.  There is quite\na bit of undergrowth, largely birch and ash saplings plus\nnondescript bushes of various sorts.  This time of year \nvisibility is quite restricted by all the leaves, but travel\nis quite easy if you detour around the spruce and berry\nbushes."
s = "\n                              Welcome to \n\n                       Colossal Cave Adventure!"

print(s)
us = dizzy.unwrap(s)
print(us)
print(' '.join(f'{v:02x}' for v in us.encode('ascii')), '00')
wus = dizzy.woozy(us)
print(' '.join(f'{v:02x}' for v in wus), '00')
print('Woozy roundtrip?', us == dizzy.unwoozy(wus))
dwus = dizzy.dizzy_squeeze(wus, digrams)
wus2 = dizzy.undizzy(dwus, digrams)
print('Dizzy roundtrip?', wus2 == wus)
us2 = dizzy.unwoozy(wus2)
print(us2)
print('Full roundtrip?', us2 == us)
