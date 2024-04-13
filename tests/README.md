grep \_turnkey: ../tali/docs/adventure-listing.txt
.c14a \_turnkey:

egrep -v '^#|seed' tests/carrybird.log > tests/carrybird.cmd

cat tests/carrybird.cmd | ../adventure-original/src/advent > tests/carrybird.c.log

cat tests/carrybird.cmd | ../tali/c65/c65 -r data/advent.rom -g 0xc14a -i 0xc004 -o 0xc001 > tests/carrybird.f.log

```
 562 axebear.rchk
 237 axeorama.rchk
     badmagic.rchk
  10 barehands.rchk
 566 bigfail.rchk
 721 birdsnakewake.rchk
 148 birdweight.rchk
 562 boulder2.rchk
 721 breakmirror.rchk
     carrybird.rchk
  14 carryfreebird.rchk
     cheatresume.rchk
     cheatresume2.rchk
   1 death-jump.rchk
 719 defeat.rchk
  10 domefail.rchk
  27 dragon_secret5.rchk
   1 dropcagedbird.rchk
 360 drown.rchk
  60 dwarf.rchk
  10 dwarf_alternative.rchk
 650 eggs_done.rchk
  67 eggs_vanish.rchk
 721 endgame428.rchk
 403 endobjects.rchk
  13 fail_hint_maze.rchk
 671 fail_hint_ogre.rchk
  98 fail_hint_ogre2.rchk
     fail_hint_woods.rchk
     fillfail.rchk
 518 fillvase.rchk
 629 flyback.rchk
 397 footslip.rchk
 630 gemstates.rchk
 726 goback.rchk
   2 hint_dark.rchk
     hint_grate.rchk
 665 hint_jade.rchk
   5 hint_snake.rchk
     hint_urn.rchk
 728 hint_witt.rchk
     illformed.rchk
     illformed2.rchk
     intransitivecarry.rchk
     issue36.rchk
     issue37.rchk
 695 lampdim.rchk
 705 lampdim2.rchk
 699 lampdim3.rchk
   3 listen.rchk
 249 listenloud.rchk
 527 lockchain.rchk
     logopt.rchk
 590 magicwords.rchk
 704 mazealldiff.rchk
 233 mazehint.rchk
 399 notrident.rchk
  15 ogre_no_dwarves.rchk
 128 ogrehint.rchk
     oldstyle.rchk
 444 oilplant.rchk
 721 oysterbug.rchk
 720 panic.rchk
 722 panic2.rchk
  70 pirate_carry.rchk
 101 pirate_pyramid.rchk
 651 pirate_spotted.rchk
   3 pitfall.rchk
 356 placeholder.rchk
 524 plover.rchk
 396 reach_ledge_short.rchk
  11 reach_noclimb.rchk
 110 reach_planttop.rchk
   1 reincarnate.rchk
     resumefail.rchk
     resumefail2.rchk
     savefail.rchk
     saveresume.1.rchk
     saveresume.2.rchk
721  saveresume.3.rchk
     saveresume.4.rchk
     saveresumeopt.rchk
     savetamper.rchk
   1 snake_food.rchk
 497 softroom.rchk
     specials.rchk
 721 splatter.rchk
 721 stashed.rchk
 721 takebird.rchk
 557 tall.rchk
 459 trident.rchk
 478 troll_returns.rchk
     turnpenalties.rchk
 718 urntest.rchk
 623 urntest2.rchk
 625 urntest3.rchk
  14 vending.rchk
 721 wakedwarves.rchk
 721 wakedwarves2.rchk
 721 wakedwarves3.rchk
 260 water_plant2.rchk
     weirdbird.rchk
  49 weirddwarf.rchk
 721 win430.rchk
 830 wittsend.rchk
     woodshint.rchk
```
