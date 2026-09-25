# Next session brief — set 2026-09-26

**Read `dev/CREATURE_RULES.md` before touching any shape.** 43 structural rules from the
2026-09-25 research pass, each tagged with its source and how much weight it carries.

## The instruments — use them, they are in the repo

Everything visual is judged by LOOKING.

- `node play.js tools/thumbs.js` then `python3 tools/sheet.py out.png plan38 plan40 ...`
  — the contact sheet. Right instrument for SILHOUETTE. Wrong one for surface: a rib 0.2
  units deep on a 6 unit animal is under a pixel at 192 px.
- `PLANS=46 node play.js tools/closeup.js` — full-viewport render of one plan. This is
  where skin, wetness, body sectioning and whether a thing reads as its animal get looked
  at. Every judgement in the 2026-09-26 entry came from here.
- `node tools/shapecheck.js 38 39 40 ...` — head clearance, head height, bounding aspect,
  leg lengths, mirror asymmetry. Caught a buried head before a single frame was rendered.
- `node play.js test_nomerge_steps.js` — real drawn clearance between limbs, in shaft
  radii. Any new creature must pass it.
- `test_plans.js`, `test_bodies.js`, `test_silho_steps.js`, `test_horror_steps.js` — the
  shape and detail guards. All green as of 2026-09-26.
- `node test_core.js` runs in plain node, NOT through `play.js`.
- Playwright needs a chromium matching its version; `PW_EXEC=<path to chrome>` overrides.

## Owed, in priority order

1. **Layering (rules L1-L3) is still the biggest unclaimed idea in the rules file.**
   Nothing yet does hard shell over visible soft core, or hanging torn skin. The stack
   bone → organs → muscle → tendon → fat → skin, revealed progressively by damage, is
   untouched. `s_blade` has its origin wound and that is all.
2. **The nested inner jaw (rule M2).** A second complete jaw with its own teeth and lips on
   a straight piston axis out of the first. Nothing in the game does this. Note the frame
   trap found on 2026-09-26: `m_jaws`/`m_maw` use +x up and +y along the muzzle, the
   dinosaur jaws use +x along the muzzle and +y up. Pass `JAW_F_UP` to any of the wet
   helpers used from a hand-built mouth.
3. **The sire's wing rest pose.** The neck, jaw and tail now read; the wing is the last
   thing keeping it from being unmistakable. It is a flat spread plane at rest. A dragon's
   wing is either folded against the body or held, never laid out like a specimen. Needs a
   fold state driven by whether the animal is grounded.
4. **Rain, and the wetness hook that is waiting for it.** `world.wet` feeds the global
   wetness channel (`fogCol.w`) and nothing sets it. Rain should drive it to 1 and decay it
   over minutes after the rain stops, and the same weather state should thicken fog and
   darken the sky. Dew and standing water already feed it.
5. **Bloom's skull is still small under the collar.** Better than detached, but the head
   reads as a mount for the mouth rather than a head that opens.
6. **`blend` is in and only two plans use it.** The sectioned, hard-creased body is what an
   arthropod is and what a starved thing is, and eight of the ten are still smooth unions.

## Standing reminders
- **Never renumber PLANS**, and never change a plan's `ns` or `seg`. Both move `headIdx`
  and every part anchor after it, which silently rewrites every saved design. Growing `ts`
  is safe (the tail is last); shrinking it is not.
- `blend`, `ribs`, `keel`, `crook`, `view`, `narc`, `neckR`, `neckTap`, `tailBase`,
  `tailTap` all default to the old behaviour when absent.
- `crook` must never move a foot vertically, and must never close a gap between two legs.
- `meshPrims(prims, quality, fine)`: `fine` comes off `prims.fine`, set in `creaturePrims`
  from the plan's `ribs` and `keel`. It must be passed on BOTH the worker and the
  main-thread path or ribbed plans lose their relief at one quality level and not the other.
