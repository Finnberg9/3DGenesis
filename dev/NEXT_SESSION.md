# Next session brief — set 2026-09-25 (evening)

**Read `dev/CREATURE_RULES.md` before touching any shape.** 43 structural rules from the
2026-09-25 research pass, each tagged with its source and how much weight it carries.

## The instruments — use them, they are in the repo

Everything visual is judged by LOOKING.

- `node play.js tools/thumbs.js` then `python3 tools/sheet.py out.png plan38 plan40 ...`
  — the contact sheet. Right instrument for SILHOUETTE. Wrong one for surface: a rib 0.2
  units deep on a 6 unit animal is under a pixel at 192 px.
- `PLANS=46 node play.js tools/closeup.js` — full-viewport render of one plan. This is
  where skin, wetness, body sectioning and whether a thing reads as its animal get looked
  at.
- `node play.js tools/wxshot.js` — dry / rain / storm / lightning from the same open
  ground. New. It picks its own empty patch of plains, because the first three attempts
  were all taken from inside another animal's ribcage.
- `node play.js tools/wallshot.js` — the zone wall from outside and from inside the band.
  The inside view is good. **The outside view has never come out**: the harness keeps
  landing in forest and photographing a tree trunk. Fix it by spectating from height
  before trusting a judgement about the tower's shape.
- `node play.js tools/veilshot.js` — you, and then you veiled.
- `node tools/shapecheck.js 38 39 40 ...` — head clearance, head height, bounding aspect,
  leg lengths, mirror asymmetry.
- `node play.js test_nomerge_steps.js` — real drawn clearance between limbs, in shaft
  radii. Any new creature must pass it.
- `test_plans.js`, `test_bodies.js`, `test_silho_steps.js`, `test_horror_steps.js`,
  `test_veil_steps.js`, `test_fall_steps.js` — the guards.
- `node test_core.js` runs in plain node, NOT through `play.js`.
- Playwright needs a chromium matching its version; `PW_EXEC=<path to chrome>` overrides.
  On a software renderer the game runs at 3-10 fps, so **wall-clock waits are not
  simulated time** (dt is clamped to 0.05 per tick). Step the sim with `__G3D.step(0.05)`
  in a loop instead of waiting, or your assertion measures the renderer, not the game.

## Owed, in priority order

1. **Layering (rules L1-L3) is still the biggest unclaimed idea in the rules file.**
   Nothing yet does hard shell over visible soft core, or hanging torn skin. The stack
   bone → organs → muscle → tendon → fat → skin, revealed progressively by damage, is
   untouched. `s_blade` has its origin wound and that is all. **This is the top item and
   has been for two sessions.**
2. **The nested inner jaw (rule M2).** A second complete jaw with its own teeth and lips on
   a straight piston axis out of the first.
3. **The `m_jaws` frame trap, now confirmed by a failing test.** `test_horror_steps` fails
   on "the wet detail stays inside the mouth on m_jaws" (0.055 outside). It fails on a
   build with the 09-25 evening patches removed too, so it is older than them. Cause:
   `m_jaws`/`m_maw` use +x up and +y along the muzzle; `drool`, `droolStrand` and
   `lollTongue` all assume +x along the muzzle and -y down. Give the wet helpers a frame
   argument (`JAW_F_UP`) rather than special-casing the parts.
4. **The sire's wing rest pose.** Still owed, and still the thing keeping the drake from
   being unmistakable. `membraneWing` builds a good hand — four panels, a wrist claw,
   tears parallel to the spars — but it is built in ONE pose and `e.flap` only rotates the
   whole plane. At rest it lies out flat like a specimen. A dragon's wing is folded
   against the body or held, never spread. Needs `e.fold` (0 spread, 1 folded), driven
   from whether the animal is grounded: elbow tucked, wrist doubled back along the flank,
   the finger spars collapsed to a bundle that projects back past the hip. The plumbing is
   easy — `env.flap` is set in two places (`render_design` ~10715 and `render_mesh` ~12202)
   and `e.fold` goes in beside it.
5. **Rain has no splashes and no drips.** It stops dead at the ground. Ripple rings on
   water and on puddles, and drips off the canopy edge, are the obvious next half-day.
   `world.wet` is already there to drive them.
6. **Bloom's skull is still small under the collar.**
7. **`blend` is in and only two plans use it.** The sectioned, hard-creased body is what an
   arthropod is and what a starved thing is, and eight of the ten are still smooth unions.

## Standing reminders
- **Never renumber PLANS**, and never change a plan's `ns` or `seg`.
- `blend`, `ribs`, `keel`, `crook`, `view`, `narc`, `neckR`, `neckTap`, `tailBase`,
  `tailTap` all default to the old behaviour when absent.
- `crook` must never move a foot vertically, and must never close a gap between two legs.
- `meshPrims(prims, quality, fine)`: `fine` must be passed on BOTH the worker and the
  main-thread path or ribbed plans lose their relief at one quality level and not the other.
- **`fogCol.w` is the global wetness channel.** It is written every frame from `world.wet`
  and read by the skin, terrain and foliage shaders. The editor podium writes it too, so
  the builder shows you the weather you will be standing in.
- The skin instance's pattern float now packs **three** fields: `pat + 16*skin +
  1024*ghost`. Anything that writes it must keep that layout.
- Weather is a pure function of `(seed, world.bt)`. Do not give it state. `world.wet` is
  the one exception and it is an integral, so it converges and does not need saving.
- `__G3D.weather.set(rain, storm, wet)` freezes the sky for a screenshot;
  `.release()` hands it back to the clock.
