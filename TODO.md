Known issues
---

- batteries are broken in the C source I started from, and may not be complete in Forth

- "fee fie foe foo" might not be working?

- Currently the kernel uses completely reproducible "random" numbers which
  makes debugging and testing much easier.  A simple fix for much more randomness
  would increment the random seed during the KEY? busy loop.

TODO
---

- add more tests:
  - end game from https://www.mipmip.org/dev/IFrescue/ajf/Universal350.html
  - add a hint excursion to verify the resurrected hint code
  - automate tester.fs over all the words in the object array in Makefile
