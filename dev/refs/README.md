# Reference images — creature brief, 2026-09-25

Supplied by Finn alongside `dev/CREATURE_BRIEF.md`. Grouped by creature, in the
order they were sent. Downscaled to 1400px and JPEG-encoded: 21 MB of original
screenshots down to 2.7 MB, because reference art does not need to be in git at
full resolution.

**These are style and structure references, not things to copy.** Several are
recognisable creatures from other commercial games (ARK, Half-Life, Alien). What
gets taken is the structural spec written in `CREATURE_BRIEF.md` and the level of
finish. What does not get taken is the specific creature. Finn's own instruction
on the same day: "dont COPY ANYONE, not fully at least." Where a spec sits close
to a protected character, `CREATURE_BRIEF.md` names exactly which identifying
marks are being given their own answer instead — see creature 9.

| folder | creature | what to look at |
|---|---|---|
| `01-raptor-dragon` | raptor / dragon hybrid | back-swept neck spikes, the reverse horn, exposed-vertebra tail, grasping claw hands |
| `02-wolf-frill` | wolf / dog quadruped | the feather-blade frill fanning off neck and shoulders, armour-plate scales down the back |
| `03-brute` | brute | gorilla arm-to-leg ratio, head carried below the shoulder line, fur on head and upper back against bare hide |
| `04-serpent` | serpent | that it is a dinosaur skull on a snake body, not a snake head; the dorsal spine ridge |
| `05-spider` | spider | the crab-like armoured face shield, furry spiked legs, heavy soft abdomen against the hard front |
| `06-small-fast` | small fast hunter | the mouth occupying the entire front of the animal, flat splayed bird feet, hard back-raked legs |
| `07-pterodactyl` | pterodactyl | **the walking pose** — wing knuckles planted as front feet — free clawed fingers at the wrist, the big reverse head crest |
| `08-dragon` | dragon | four near-equal limbs (contrast with 07's tiny hind legs), four spar tips past the membrane edge, horned skull |
| `09-starved-alien` | starved stalker | bone and muscle through a tight hide, extra-jointed over-long limbs, the backswept skull mass |
| `10-armadillo-tank` | armadillo / lizard tank | interlocking rocky back plates with real gaps, short columnar legs, low round bulk |
| `11-insect` | insect | long high-kneed legs against a compact rounded body, the small fast wings that do NOT dominate the silhouette |

## `00-rejected-output` — read this one first

Not references. These are Finn's screenshots of what the game **actually
produced** on 2026-09-25, sent with "those look like shit, like cartoonish
characters, with limbs literally floating not even touching bodies" and "they
look nearly identical to before".

They are kept deliberately. They are the fastest way to see the gap between the
brief and the output, and most of what is wrong in them turned out to be bugs
rather than taste:

- parts anchored half a world unit off the body (`SKIN_ISO` was 0.5 against a
  distance field the mesher extracts at 0) — this is the "floating" in every shot
- limbs drawn as round cones with ellipsoid beads at the joints, because the
  chisel work went into the palette-icon renderer and not the one the game uses
- a creature vocabulary of exactly two shapes, both round, with no boolean
  subtraction anywhere — which made smooth blobs mathematically unavoidable
- every tooth and claw drawn in near-white ivory, so dark creatures read as toys
  with white bits glued on

All four are fixed as of the 2026-09-25 (night) changelog entry. If the output
still looks like these shots, something regressed — start there.
