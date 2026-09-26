# THE REDO — shared contract
Set 2026-09-25 (night) after Finn's instruction: *"Make SURE YOU DONT JUST USE THE
EXISTING BODY PLANS, I WANT AN ENTIRE RE-DO, AND ALL NEW PARTS."*

Three agents build this in parallel. This file is the only thing they share. If
you need something from another agent that is not written here, it does not
exist yet — write to this contract's interface and let the integrator wire it.

---

## 0. Why the old system cannot be reparameterised

This is the reason the last three sessions all produced "the same animal".

**A body is one spine of balls.** `PLANS[i]` is a flat parameter list —
`a, b, seg, prof, arch, droop, neck, nx, ny, ns, head, tail, ts, stand, hump,
waist, crook…` — that builds a single chain of spheres from nose to tail tip.
Every creature in the game is that tube with the dials moved. A spider is a
cephalothorax with a heavy abdomen slung behind it on a narrow pedicel; a
pterodactyl walks on its wing knuckles with a tiny pelvis; a brute has no hips
to speak of and a head hung below the shoulders. **None of those are a tube.**
No value of `waist` makes a spider.

**A limb has exactly one knee.** `{ k: kneeOffset, f: footOffset }`. Two bones.
The brief asks for arthropod legs with an extra segment between knee and ankle
(rules J1/J2), high-kneed insect legs that fold *above* the back line, wing
knuckles that plant on the ground, and extra-jointed over-long arms. **None of
those are two bones.**

So the redo replaces both. Everything else in the engine — the SDF mesher, the
skin shader, the wetness channel, the fight system, the inventory — stays and
gets fed by the new structures.

## 1. What is being built

Eleven creatures, specced in `CREATURE_BRIEF.md`, referenced in `refs/`. Read
`refs/00-rejected-output/` first: that is what the old system produced and why
it was rejected.

**New numbering.** The new plans are appended at index 48+ and the old 0..47 are
hidden from the builder. Nothing is renumbered — saved designs and the test
guards depend on the old indices staying put — but the player only ever sees the
new set, which is what "an entire re-do" means to the person holding the
controller. Old plans stay reachable only for loading an old save.

**All new parts.** New part ids live in a new namespace and no new creature may
use an old `VIS` entry. Old ids keep working for old designs.

## 1b. THE ROSTER IS LOCKED — Finn, 2026-09-25 night

> *"so its 9 land, 3 air. 1 no leg, 5 quadrapeds, 1 biped, and 8 octoped."*

Eleven creatures. The insect is counted twice on purpose: it is a ground animal
that can fly, so it is in both the 9 and the 3. That is the only double-count,
and it is why 9 + 3 = 12 against a roster of 11.

| # | creature | locomotion | legs | ground / air |
|---|---|---|---|---|
| 1 | raptor / dragon hybrid | **biped** | 2 + 2 grasping arms | land |
| 2 | wolf / frill | quadruped | 4 | land |
| 3 | brute | quadruped (knuckle) | 2 huge arms + 2 small legs | land |
| 4 | serpent | **no legs** | 0 | land |
| 5 | spider | **octoped** | 8 | land |
| 6 | stalker (small, fast) | quadruped | 4 | land |
| 7 | pterodactyl | wing-walker | 2 wing knuckles + 2 small legs | **air** |
| 8 | dragon | quadruped + wings | 4 near-equal | **air** |
| 9 | starved / husked | quadruped + extra arms | 4 + 2 thin | land |
| 10 | armadillo tank | quadruped | 4 short columns | land |
| 11 | insect | hexapod | 6 long | land **and air** |

**The count is a test, not a note.** Totals must come out at: legless 1,
biped 1, quadruped 5 (2, 3, 6, 9, 10), hexapod 1, octoped 1, wing-walker 1,
winged quadruped 1 — 9 that live on the ground, 3 that can fly. Write it as an
assertion in the plan guard. If a build drifts (a quadruped picking up a fifth
limb, the insect losing its wings) the guard fails rather than the contact sheet
being re-judged by eye three sessions later.

Note the brute counts as a quadruped: it moves on four contact points, two of
them fists. Its arms are arms in every other respect — grasping, striking — so
they are limbs with hand tips that the gait treats as front legs, which is
exactly the `classifyLimbs` trap in 7 seen from the other side.

## 2. BODY v2 — a module graph

A plan is no longer a parameter row. It is a small graph:

```js
{ name: 'spider', v: 2,
  modules: [
    { id: 'thorax', shape: 'e', at: [0,0,0],     size: [1.05, 0.62, 0.95], rot: [0,0,0], blend: 0.35 },
    { id: 'abdomen', shape: 'e', at: [-1.5,0.1,0], size: [1.35,1.1,1.25], blend: 0.10,
      link: { to: 'thorax', r: 0.16 } },        // the pedicel: a thin waist, not a smooth union
    { id: 'head',   shape: 'b', at: [1.05,0.12,0], size: [0.38,0.26,0.62], blend: 0.25, corner: 0.10 },
  ],
  sockets: [
    { id: 'leg1', on: 'thorax', at: [0.55, -0.05, 0.62], dir: [0.25,-0.3,1], mirror: true },
    …
  ],
  stance: { … }, skin: 'chitin', innate: [ … ] }
```

- `shape`: `'s'` sphere, `'e'` ellipsoid, `'b'` oriented rounded box, `'c'` round
  cone. All four already exist in the core SDF as of 2026-09-25 night, and `sub:
  true` cuts instead of adding. Use the box and the cut — the round-only
  vocabulary is why everything came out a blob.
- `blend` is per module: a low number gives a hard crease between two masses
  (arthropod, starved), a high one a smooth union (fat tank).
- `link` makes a narrow connector between two modules, which is how you get a
  pedicel, a neck that is visibly a neck, or a tail base.
- `sockets` are named attachment points. Limbs and parts attach to a socket id,
  NOT to a ball index. This is what makes a part "organically placeable" instead
  of bolted on: the socket carries a surface point and an outward normal that
  the module's own shape defines.

**Compatibility:** `buildDesignSkeleton` must still emit the `sk.balls` array
and `headIdx`/`tailStart` that everything downstream reads, derived from the
module graph. v1 plans keep their v1 path untouched. Do not renumber, do not
change any v1 plan's `ns` or `seg`.

## 3. LIMB v2 — a segment chain

```js
{ id, socket: 'leg1',
  chain: [ { len: 0.62, dir: [0.3,-0.9,0.2], r: 0.10, plane: 'x' },   // femur
           { len: 0.55, dir: [0.1,-0.95,0], r: 0.075, plane: 'z' },   // tibia
           { len: 0.40, dir: [-0.2,-0.9,0], r: 0.055, plane: 'x' },   // metatarsal
           … ],
  tip: { id: 'f_spread', s: 1 }, th: 1, m: mirrorKey }
```

- 2 to 5 segments. Three is a mammal leg; four reads as arthropod; five reads as
  a tentacle and stops being disturbing (rule J1).
- `plane` is the hinge plane of that joint. Alternating it between adjacent
  segments is the real arthropod tell, not the segment count (rule J3).
- The renderer already draws chiselled three-slab bones with a rotating
  cross-section; that stays, applied per chain segment.
- v1 limbs (`k`/`f`) keep working. `chain` takes precedence when present.

## 4. Innate parts — Finn's rule

> *"each creature starts out with the shit it has. if a choose a bird i expect
> to start with wings, not have to collect them later. If my creature has a
> horn, i start with that horn."*

Every plan declares `innate: ['w_membrane','h_reverse','m_split', …]`.

- `defaultDesign(plan)` places exactly those, on their sockets.
- **`brBareDesign` currently strips every part except eyes when a match starts.**
  That is the bug behind Finn's complaint — find it at the `// keep only the
  eyes` comment. It must keep the plan's innate list as well.
- The inventory allowance stays `max(owned, what the body carried when the
  editor opened)`, so innate parts are rearrangeable but not farmable into
  stock: they are not added to `PROG.owned`.
- Everything NOT in `innate` is still earned off corpses. That is the whole
  point — you pick up other creatures' parts to customise your own.

## 5. Colour and material direction

> *"not playful RGB, but instead creepy dark colours with different textures."*

- Base hides sit at **value 0.06–0.22**. No saturated primaries. The palette is
  bone, ash, char, dried blood, bile, drowned green, bruise purple — all
  desaturated and dark.
- Contrast comes from **material, not hue**: wet vs matte, plate vs membrane,
  fur vs bare muscle. The wet/clear-coat/anisotropic-scale shading landed on
  2026-09-25 and is driven per covering; use it.
- One hot accent per creature at most, small and localised (an eye, a gum line,
  a display membrane), never a body-wide colour.
- Bone, tooth and claw are dull keratin, never white enamel. That was one of the
  four things that made the rejected output read as toys.

## 6. The rare skin — LAVA (low priority, do last)

Finn: a skin where it looks like lava flows through the body, like the dynamic
Black Ops gun camo. Build it as a covering mode in the skin shader, not as a
plan:
- a **crack field** (worley ridges) that is dark char on the surface,
- an emissive flow **inside** the cracks, scrolling along the body's flow
  tangent with the same two-sample half-period cross-fade the wetness coat uses,
- brightness pulsing slowly and unevenly, hotter in the creases and at the
  joints where the body flexes.
Only after the eleven read correctly. If you are the shading agent and you are
short of time, this is the thing that gets cut.

## 7. Ownership — who may touch what

Three agents work in parallel on separate trees. Deliver **new files**; never
edit another agent's file. Every change to `g3.html` goes through a NEW
`src/patch_<yours>.py` appended to `build.sh`, using the `sub1(s, a, b)`
exact-match idiom so a moved anchor fails loudly instead of silently.

| | agent | owns | anchors |
|---|---|---|---|
| A | fighting physics & movement | gait, IK, ground contact, `classifyLimbs`, strike/hit resolution, stance | `movePlayer`, `updateJump`, `fightTick`, `attackRig`, `legRanks`, limb pose |
| B | skeleton, mesh & skin | BODY v2 + LIMB v2 builders, SDF modules, socket solver, mesher, skin shader, palettes | `buildDesignSkeleton`, `creaturePrims`, `meshPrims`, `WGSL_SKIN`, `PLAN_COLORS` |
| C | creature design | the 11 plans, the new parts catalogue, `defaultDesign`, innate kits, builder UI listing | `PLANS`, `VIS`, `defaultDesign`, `PLAN_SKIN`, editor palette tabs |

Conflicts to expect and avoid: all three want `defaultDesign`. It belongs to C.
A and B expose functions; C calls them.

## 8. Build and verify

```
bash build.sh                      # patch chain -> g3.html, prints a line per patch
python3 -m http.server <PORT> &    # your own port, see your prompt
PORT=<PORT> PW_EXEC=/opt/pw-browsers/chromium-1194/chrome-linux/chrome node play.js <steps.js>
```

- `node test_core.js` runs in plain node, not through play.js.
- The guards that must stay green: `test_plans`, `test_bodies`, `test_silho_steps`,
  `test_nomerge_steps`, `test_move_steps`, `test_veil_steps`, `test_fall_steps`,
  `test_core`. `test_horror_steps` has one KNOWN pre-existing failure
  (`m_jaws` wet detail 0.055 outside the mouth) — do not treat it as yours.
- **Look at what you build.** `tools/closeup.js` renders one plan full-viewport;
  `tools/thumbs.js` + `tools/sheet.py` makes the contact sheet. A judgement about
  a shape that was not made by looking at a render is not a judgement.
- On a software renderer the game runs at 3–10 fps and `dt` is clamped to 0.05
  per tick, so **wall-clock waits are not simulated time**. Step the sim with
  `__G3D.step(0.05)` in a loop instead of waiting.

## 9. Definition of done for this phase

Not all eleven. This phase is the foundation plus proof:

- BODY v2 and LIMB v2 exist, are tested, and v1 still builds byte-identically.
- Physics walks, stands and fights on a v2 limb chain.
- **Four creatures** built on v2, chosen because they are the four the old tube
  could not express: **spider (05), pterodactyl (07), brute (03), raptor (01)**.
- They are judged from a rendered contact sheet, not from plan numbers.
- Innate parts work: picking one of the four in the builder and entering a match
  starts you wearing its defining kit.
