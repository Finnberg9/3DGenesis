# 3DGenesis dev log

## 2026-09-26: The frame counter was lying, and that is why ultra was unplayable

Finn, on the build shipped a few hours earlier: *"art ultra ghraphics, it only
loads at 2 frames/sec but it says 30fps"*, *"its basically unplayable at ultra
graphics"*, and *"the rain is completely unplayable outside of the storm"*.

Two bugs. The first one is mine from years of convention; the second is mine
from this morning.

### The clock measured the wrong thing, and everything believed it

```js
ft = now - R2.lastT;          // gap between the STARTS of two renderFrame calls
```

That is the rate **JavaScript is pacing at**, not the rate the screen is being
painted at. `queue.submit` is asynchronous: the CPU can build and hand over frame
after frame at very nearly vsync while the GPU falls arbitrarily far behind, and
every queued frame is pure latency between the mouse and the picture. The HUD
read 32, the GPU was delivering 2, and **every system that exists to rescue a
slow machine reads that number**:

- the automatic quality stepper never stepped down — by its numbers the machine
  was coping,
- the resolution trim sat at 97% for the same reason,
- so the one thing whose entire job is to notice this was blind exactly when it
  was needed. The lie was not a cosmetic bug. It was the cause.

Now timed off `queue.onSubmittedWorkDone()`, which resolves when the queue has
actually drained — the only moment in the pipeline that corresponds to something
a person can see. The frame time everything acts on is `max(cpu, gpu)`.

**The averages are asymmetric now**: 0.34 toward bad news, 0.05 toward good.
A safety system that takes ten seconds to notice a machine is drowning is not a
safety system, and one that climbs back on a single fast frame oscillates. This
matters more than it sounds, because frames are *skipped* while the GPU is
behind, so there are fewer samples to converge with exactly when it counts.

**And it no longer runs more than two frames ahead of the GPU.** A third frame
buys nothing but staleness. The simulation still steps on a skipped frame, so
the world does not slow down with the picture — only the picture does, and the
picture was not arriving anyway.

Measured on the software renderer at ultra, in forest, before and after:

| | reported | actual |
|---|---|---|
| before | 32 fps, trim at 97% | ~2 fps |
| after | 1.7 fps, trim driven to its 0.6 floor | ~2 fps |

The HUD also says `· gpu bound` when the two clocks disagree by more than 8 ms,
which is how you tell a heavy scene from a heavy simulation at a glance.

**Ultra renders at native now** (`res` 1.15 → 1.00). It was rendering at 1.15x
the display size and scaling down: supersampling on top of the MSAA that was
already running. That is 32% of every pixel in the frame for very nearly nothing
you can see. Ultra keeps its draw distance, shadow cascades, foliage density and
effects — the things you actually look at.

A hand-picked level is still never overruled; that rule is tested and it stays.
But if you picked one and it is arriving under about 11 fps, the game now says
so once every 45 seconds with the number and the way out, instead of leaving you
to wonder whether it is broken.

### The rain was sized in world units

A streak was 1.6–5.0 world units long and 0.045–0.08 wide, in a box 80 units
across with the camera inside it. A drop two units from your eye therefore
subtended an enormous angle, and 14,000 of them at ultra made the white picket
fence in Finn's screenshot: you could not see the ground.

**A drop is the same size on the screen wherever it is.** Angular length and
width now, clamped at both ends: about 25–40 px long and 2–3 px wide at 1080p,
whatever distance the drop happens to be at. What reads as heavy rain is the
NUMBER of streaks, never the size of them.

- alpha per streak 0.85 → 0.38. A single drop is nearly transparent; a downpour
  is thousands of nearly transparent drops.
- the nearest drops fade right out (1.5 → 7 units). A drop passing a hand's
  width from your eye is too fast and too far out of focus to be anything but a
  smear.
- budgets cut about two thirds: ultra 14000 → 5000, high 9000 → 3000, medium
  4500 → 1600, low 1600 → 700. With screen-constant streaks the count is what
  carries the weather, and it no longer has to fight its own overdraw.
- no more near-drop widening, which was making the worst offenders worse.

### Also

`__G3D.br.perf()` now reports `fps, ftAvg, ftCpu, ftGpu, inFlight, skipped,
dyn, scale, drops, plants`, and `gfx.setRes / setPlants / setShadow / setCrit`
turn one cost knob at a time, so where a frame goes can be measured rather than
guessed at. It could not be used on this software renderer at ultra — one frame
every eight seconds, so a sixteen second window collects one sample — but on real
hardware it is a profiler.

Guards: `test_perf_steps`, `test_move_steps`, `test_fall_steps`,
`test_veil_steps`, `test_plans`, `test_core` all pass.

## 2026-09-25 (evening): Weather, and four things Finn asked for while it was being built

The sky was the last big system the renderer had a hole for. `fogCol.w` has been
sitting in the globals block unused since it was written, the brief called it the
global wetness channel, and nothing has ever set it. It sets it now, and rain is
what sets it.

Finn sent five more things mid-session; all of them are in.

### Weather is a pure function of the clock

```js
wxAt(bt) -> { rain, storm, gust }
```

`world.light` is a pure function of `world.bt` and always has been. So is the sky
now. A **weather cell** is a seventh of a day; each cell's character comes out of
`hash(seed, cell)`, so:

- a reloaded world gets back the weather it had,
- every client of a match sees the same sky without a packet crossing the wire,
- a fast-forwarded world still gets its rain in the right places.

Thresholds slide with `climate.moist`, which is already a slow wander on the same
clock: at the wet end of the cycle a bit over half of all cells carry rain, at the
dry end about one in six. Inside a cell the rain eases in over the first fifth and
out over the last quarter, because a hard edge on weather reads as a bug.

One derived quantity is NOT pure and cannot be: **surface wetness is an integral**,
rising over about twenty seconds of hard rain and drying over three minutes. It
lives in `world.wet`, converges from any starting value, and is therefore safe to
leave out of a save. Night dew puts a floor under it so dawn is damp with no
weather at all.

### What rain does, in the order it matters

The drops are the last and smallest part of it. In front of them:

- **The air thickens.** Fog density up to 4.8x, and the colour of the distance
  goes grey-green.
- **The light goes out of the sky**, down 55% in rain and another 22% in a storm.
- **Everything gets wet**, through `fogCol.w`:
  - *Skin*: the covering's own wetness doubles as how much rain STICKS. Fur beads
    it and reaches about a third soaked; chitin and plate sheet it and go to full.
    The clear coat, the moving specular and the two lobes were all already there
    from 09-25; they just had nothing to turn them on.
  - *Terrain*: darkens everywhere, and water RUNS OFF. Slopes stay merely dark;
    flat ground holds a film; hollows hold standing water. A puddle is not a
    texture, it is a patch whose **normal goes flat** and whose specular goes up,
    with the sky in it at a glancing angle. That is the only thing that reads as
    standing water.
  - *Foliage*: a leaf is waxy, so it keeps its colour and takes a hard little
    highlight. Bark is not, so it just soaks and goes dark.
- **Lightning** lights the whole world for about a tenth of a second, white with
  a little blue in it. Tinting the fog toward a colour instead made it read as a
  damage overlay, which is what the first cut did.
- **Thunder arrives late.** The gap is the distance: `d / 340`. A near strike is a
  crack with a tail, a far one is a long low roll.
- **Sound**: two bands. A broad hiss that rises with how hard it is coming down,
  and a high spattering band that only exists under leaves -- rain on your own
  head and rain on the canopy above you are different sounds.

### The drops themselves: six vertices and no buffers

Position, size and fall speed all come out of hashes on the instance index, and
each drop is wrapped into a box that follows the eye **in world space**:

```wgsl
let rel = home - g.camPos.xyz;
let p0  = g.camPos.xyz + (rel - BOX * round(rel / BOX));
```

so a drop belongs to the world and moves past you when you run, rather than
hanging on the lens. The streak lies along the drop's own velocity and is built
in the shader from the view vector, so there is no billboard maths on the CPU and
nothing to sort. `pass.draw(6, n)` and that is the whole draw call.

The box started at 260 units and the rain was invisible: spreading the budget
over a volume nobody can resolve buys distance you cannot see and costs the
density that sells it. 80 units, and it reads.

### The zone wall is a thunderhead now

Finn: *"make it more like a heavy storm cloud where there is dark clouds super
tall up cumulonimbus... and when youre in the storm its super rainy and loud and
flashing lights and hard to see and you slowly die."*

The wall was four layers and 520 units of grey. It is **seven levels and 1750
units** of storm cell:

- It **leans out as it climbs** (`CLOUD_SPREAD` 1.0 to 3.7), so the anvil
  overhangs the band and the light goes before the damage does.
- Puffs get **bigger with height**, which is what makes a tower read as scale.
- The **base is nearly black and the anvil is white** (`CLOUD_LUM` 0.16 to 1.30).
  Nothing else in this game has that range inside one object, and it is the
  reason it reads as weather rather than as smoke.
- **Lightning lights it from the inside**: the flash feeds each puff's own tint,
  low levels hardest.
- **The band IS the weather.** Inside it, rain and storm are forced to full, so
  the fog, the flashes, the thunder and the wetness are the same system the sky
  uses. The thunder delay becomes the distance to the edge, so the crack gets
  closer as the ring does. Rain reaches ~900 units ahead of the wall: you get wet
  before you get hurt.
- The puff budget now follows the quality setting (420 on low to 1500 on ultra).
  Seven levels is a lot more cloud than four, and every puff is a big soft
  alpha-blended sphere.

### VEIL is invisibility

Finn: *"purple to turn you completely invisible, where we can still faintly see
ourselves but others cant at all, they can only hear you and get damaged by you."*

It was a 0.25 multiplier on being noticed, which is a discount, not a spell.

- **`spellSeenMul()` returns 0.** Nothing sees you.
- The core's perception loop consults `world.seenMul(o)` -- one function, one
  place stealth lives. It is applied AFTER personal space, which is *felt*, not
  seen, so a hidden animal is still a solid object to walk into.
- **It can hear you.** A swing gives you away for 1.5 s, sprinting for 0.9 s, and
  while the sound is in the air you are back to 0.55.
- **You can see yourself**, faintly: a dithered discard in the skin shader,
  keyed off a ghost field packed above the skin field in the pattern float. It is
  stochastic transparency, so there is no second pipeline and no sort order, and
  the noise re-rolling at 14 Hz is what makes it shimmer instead of screen-door.
  The hard parts have no ghost channel, so they take the colour of the air
  instead -- washed to the fog, which at any distance is the same answer.
- A veiled body does not glow. A lit-up invisible animal is a contradiction.

### RAGE costs nothing

One line in `fightSpend`. Sprint, swing, dodge and guard are all free while it
runs, and it clears an exhaustion you were already in -- otherwise drinking it at
zero stamina would do nothing for twenty seconds.

### You walk down hills now

Finn: *"everytime i go downhills it just glitch jumps down it. You should only
fall if the slope is like 80 degrees or steeper."*

He was right and it was a real bug. What counted as the ground falling away was:

```js
const walkDrop = Math.max(2, (c.ph.speedLand || 40) * PAGE_SLOPE_TAN * dt * 1.6);
```

Your **top speed**, not the distance you actually covered, with a floor of two
world units. A slow animal, or any animal in a long frame, cleared that bar on
ordinary ground, got put in the air, and gravity dropped it back on -- the hop.

The honest test is the slope you just walked: the ground fell `drop` over the
`dh` you actually moved, and that is a fall only if `drop/dh` is steeper than
**tan 80**. Plus a lip of 0.35 body heights you can step off for free, or terrain
noise under a stationary animal reads as a cliff.

Measured: a 47 degree hill, 250 units of descent, **0 airborne frames out of 160,
peak hop 0.00**. Jumping still leaves the ground (17 frames, 19.5 units).

This terrain is a height field with a 60 degree slope limit, so there are no
faces steeper than 80 degrees anywhere on the island. That is the point: with
this rule the only way off the ground is a jump or a ledge.

**Fall damage starts three times higher**, as asked: `FALL_SAFE_H` 2.5 -> 7.5 body
heights, on the player and on every other animal. The fatal span past it is
unchanged.

### Guards

`test_veil_steps.js` and `test_fall_steps.js` are new and green. `test_plans`,
`test_bodies`, `test_silho_steps`, `test_nomerge_steps`, `test_move_steps`,
`test_br_steps`, `test_perf_steps`, `test_spell_steps` and `test_core` all pass.

`test_horror_steps` has **one pre-existing failure**: the wet detail on `m_jaws`
sits 0.055 outside the mouth. Checked against a build with today's three patches
removed -- it fails there too, so it is not from this pass. It is the frame trap
the 09-26 brief warned about: `m_jaws` uses +x up and +y along the muzzle, and
`drool`/`lollTongue` assume +x along the muzzle.

## 2026-09-25 (night): The body gets a skeleton, and three bugs that had nothing to do with design

Finn, on the ten shipped a few hours earlier: "they still look like shit, like cartoonish
characters, with limbs literally floating not even touching bodies"; "the colour palette is
way too bright"; "the legs are just literal cylinders"; "remove the ugly ass bumps for his
joints"; "the mesh builder you made is fucking stupid".

He was right about all of it, and three of those turned out to be bugs rather than taste.

### The floating parts were not floating

**Every part on all 48 plans was anchored half a world unit off the body**, and had been
since the body moved from metaballs to signed distance fields.

```js
skinField(sk, x, y, z) = SKIN_ISO - sdfSkin(...)     // SKIN_ISO = 0.5
```

Anything that asks "where is the surface" -- parts, limb hips, snapping -- asks `skinField`,
which answers *where sdfSkin equals 0.5*: half a unit OUTSIDE the primitives. The mesher
extracts the surface at `sdfSkin = 0`. So the drawn skin and the surface everything is
pinned to are two different surfaces, a fixed distance apart, and nothing scales that
distance with the animal.

Measured on plan 47: drawn half-width **0.168**, anchoring half-width **1.081**. The surface
parts were pinned to sat five and a half body radii out in open air.

0.5 is a leftover from the metaball field, where the iso was a threshold on a sum of falloffs
and 0.5 meant "about 0.8 of a ball radius". Against a true distance field it means half a
metre of nothing. The comment above `VISR` even says the radii are already the visible radii.
`SKIN_ISO` is zero now: the surface parts are pinned to is the surface that gets drawn.

My own parts were separately oversized -- the metasoma walks five segments plus a telson plus
a needle, about 3.6 local units, where every part written before today reaches 0.3 to 1.5,
and I then *raised* its `size` to 0.95. Roughly six times too big. Sized against real reach now.

### Two limb renderers. Again.

The 2026-09-24 entry in this file says, in as many words: *"There are TWO limb renderers and
I had only fixed one."* This morning I put the chiselled cross-section into `drawLimbTube` --
the palette icon and distant-crowd path -- so the **icons** got chiselled limbs and every
actual creature kept round cones. The game draws from `creaturePrims`. Same trap, written
down in advance, walked into anyway.

The limb is rebuilt in the path the game uses:

- **Three bones, not two.** Femur, tibia, metatarsal. Thigh-and-shin-straight-to-the-foot is
  a human leg, and a human leg on a quadruped reads as a cartoon.
- **A real ankle.** `digi` slides the hock up the limb and kicks it backward. There is no
  backwards knee in nature -- the rearward joint every animal shows is the ankle, sitting
  high with a long metatarsal below it. The game had no way to say that.
- **Chiselled sections.** Each bone is three flattened slabs whose flat face turns along its
  length. The slab's wide half-axis is the cone radius, so the envelope does not grow.
- **No beads.** Joints are plates *across* the bone -- wide crossways, nearly flat lengthways.
  The hip and knee swell came down from 1.34/1.42 to 1.13/1.17.

### The thinning pass could delete a leg

Plan 47 came back with no legs at all. Three causes compounding, one of them pre-existing:

1. `RMIN = 0.030 * limbScale * thin`. RMIN is the floor that keeps a shaft thick enough for
   the mesher to extract -- and it was being scaled by the very thinning factor it exists to
   protect against. A leg thinned to 0.42 got a floor 0.42 as high and vanished.
2. The 0.42 clamp was applied per pass over three passes, so the real floor was 0.42 cubed --
   about 7% of the original shaft.
3. I had just demanded ~30% more clearance, pushing far more pairs into the thinning path.

The floor no longer moves, thinning is clamped once against the original thickness, and the
joint plates are much smaller -- which is what Finn was pointing at anyway.

### The torso has bones in it now

The body was **one chain of spheres**, smooth-unioned. There is nowhere in that structure to
put a ribcage, a shoulder blade, a hip point or a vertebral process, which is why this
morning's `blend`/`ribs`/`keel` tuning produced nothing: I was trying to get anatomy out of a
system with no bones in it.

- **ribcage** -- real arcs leaving the backbone, bowing out at the middle and coming back in
  at the tip. **The ribs set the body's width.**
- **spineRow** -- dorsal processes standing off each vertebra, so a back has a ridge of bone
  rather than being the top of a curve.
- **scap / pelvis** -- shoulder blades and hip points.
- **flesh** -- how much meat sits over the cage. Low and the ribs read through as ridges with
  hollows between them; high and they are buried.

Everything defaults to the old behaviour, and it is emitted into the primitive list rather
than into `sk.balls`, so no saved design's part anchors move.

Three things this got wrong on the way, all caught by measuring rather than by eye:

- **An ellipsoid in the core SDF returns NaN.** `primDist` treats anything that is not a
  sphere as a round cone, so my ellipsoid shoulder blades poisoned the whole field and *every
  part and limb on all ten failed to anchor*. Blades are cones now.
- **A rib thinner than the grid is not a rib.** The cage went in at 0.16 of a vertebra's
  radius, about one mesh cell; marching cubes needs roughly two to make a visible ridge. Same
  "too skinny to create" failure as 09-24, on bone this time.
- **Ribs also need a GAP wider than a cell.** Six thick pairs close together weld into a
  smooth barrel. Four heavier ones with real air between them. The count is set by the grid,
  not by anatomy.

Measured dorsal relief, as a fraction of body radius: husk 75%, kite 70%, null 57%, gibbet
54%, thresher/wrack/sire 30%, pall 17%, bloom 3%, cradle 1%. The last two are deliberate --
bloom is armoured and the cradle's sac is smooth on purpose, which is the contrast with its
legs.

### Three of them were too thin to carry any detail at all

The mesher sizes its grid off the model's whole bounding span, so a plan that is mostly leg,
neck and tail gets very few cells across its torso. Null, kite and husk had body radii of
0.30, 0.28 and 0.40 against spans of five or six units -- about six cells across the entire
body. At that resolution a ribcage is a row of disconnected beads with holes between them,
which is exactly what the numbers said: relief above 100% of the body radius and five sample
points with no body at all. "Starved" has to mean thin flesh over a frame, not a wire.

### Colour

Every tooth, claw, spike and needle in the game was drawn in IVORY at 0.94 -- brighter than
anything else on the animal and pure enough to read as plastic, which is what makes a dark
creature look like a toy with white bits glued on. Bone and keratin on a living animal are
dull, yellowed and dirty. IVORY is 0.58, WHITE 0.72, and the new parts carry dark chitin
instead of pale sand.

### The guard was measuring the wrong thing

`test_nomerge_steps` only looked at `'c'` primitives. A limb's visible surface is now
flattened slabs, so a cone-only guard measures the thin core and reports clearance the animal
does not have. It reads slabs too -- and the moment it did, it reported that the **hip caps
had always been overlapping between paired legs**, which is the fused lump between the legs
Finn was looking at. Passing at 0.113 shaft radii.

### Still not right
- Plan 47 still has one marginal sample with almost no body at it.
- The sire's tail renders untextured against a scaled body.
- Bloom's four-part mandible is attached now but still reads as a crown rather than a face.
- The wet shading reads strongly on plated skins and weakly on smooth ones.


## 2026-09-25 (late): Ten new creatures, and the mesh that was stopping them

Finn's brief: research how horror games actually build creature bodies, then design ten
genuinely new creatures from that research. Not variations on the current ten.

His note partway through, which was the important one: **"dont COPY ANYONE"**, and later
**"those look so similar to the ones before. What was the point of all your research?"**

Both were right. What follows includes the part where I got it wrong.

### The research is a rules file, not a mood board
`dev/CREATURE_RULES.md`, 43 rules across silhouette, proportion, joints and limbs, heads,
mandibles/wings/tails, layering, and wet shading. Every rule is stated in a form that can
become a plan parameter, and every one is tagged with where it came from and how much
weight it carries: `[DEV]` a developer on the record, `[CRAFT]` a working artist's general
rule, `[ANALYSIS]` a critic's reading of a shipped asset rather than design intent,
`[ANAT]` comparative anatomy, and the numbers that are mine are marked derived.

Nothing below is a copy of a creature from another game. What was taken is structure --
where mass goes, how many joints, what replaces a face, how a membrane tears -- and all ten
are invented animals. The honest gap is written into the file too: no studio publishes
numeric proportion specs, so several of the ratios are my parameterisation of a qualitative
claim and are starting values to tune, not findings.

### The mistake, again, one level up
First pass: I wrote ten new plans in the PLANS table, rendered the contact sheet, and got
ten potatoes. Finn said so before I did.

The cause was not the numbers. The skin is a smooth union of round cones along one spine
with a **fixed** blend radius, `kb = 0.26 * b`. A smooth minimum at that radius melts every
joint in the chain, so whatever `a`, `b`, `hump` and `waist` say, the output is one lozenge
with a blob on the front. None of the layering rules can even be *said* in that mesh --
hard shell over soft core, plates over exposed muscle, starved concavity, sectioned body
segments -- so the best a plan could do was bolt small parts onto a smooth blob.

That is the 2026-09-25 morning mistake in a new form: I found a level I could measure and
worked there instead of at the level that was broken.

### Three things the mesh could not say
All three default to exactly the old behaviour, so no plan written before today moves by a
byte and no saved design changes shape.

- **`blend`** -- a multiplier on the smooth-minimum radius. At 1.0 nothing changes. At 0.08
  the chain stops melting and the body is visibly sectioned, with a hard crease at every
  joint. That is what an arthropod is, and what a starved thing is.
- **`ribs`** -- modulates girth per SEGMENT rather than along one smooth curve, so the
  ridges land on the joints instead of drifting between them. Concave where the eye
  predicts convex is a per-region error signal; one global "it is thin" is not (rule P4).
- **`keel`** -- a real row of five balls above the spine. A back can be a blade or a plated
  ridge instead of the top of a curve, and because it is geometry rather than a decal it
  survives the silhouette test. Emitted hidden at zero radius when `keel` is 0, so every
  existing part anchor keeps its ball index.

### A limb is not a pipe
Every shaft in the game was a round cone, so every leg was a pipe with two knuckles on it.
Each bone is three stacked slabs now and the flat face turns about 110 degrees from one end
to the other, so rotating the creature reveals a different profile (rule J7). The widest
half-axis of every slab is exactly the round radius it replaces, so the drawn envelope does
not grow and `test_nomerge_steps` measures the same clearance it did before.

### The ten
One structural idea each, and no two share a mass distribution, a limb architecture or a
head solution.

| | idea |
|---|---|
| **bloom** | all mass forward over the shoulders, nothing behind, and the whole face is a four-part mandible that unlatches and splays |
| **kite** | wing-dominant: a starved body hanging under a torn span, and a whip for a tail |
| **gibbet** | an enormous shoulder yoke with the hips gone, head slung forward and below the chest |
| **cradle** | a hanging belly sac on six long thin legs that visibly cannot carry it (P3) |
| **thresher** | thorax and abdomen with a hard pinch, blade forelimbs, a five-segment metasoma over the back |
| **pall** | a lid. Far wider than long, a flat shell over a starved core, eight crooked legs, nothing above the rim |
| **husk** | two thirds on the floor, the front third standing vertical, every segment separate |
| **wrack** | short broad wings on a pinched body: the deliberate opposite of the kite, so the fliers stop being one bird |
| **sire** | the dragon. The body is SHORT so the neck and the tail are long **on screen** rather than long in the numbers, which is exactly why the drake kept failing |
| **null** | two crooked hyper-extended legs, a head below the shoulder line, a blade for a spine, and nothing else |

The previous fourteen stay in the table -- renumbering turns every saved design into a
different animal -- they are simply no longer offered.

### Crooked legs, and the guard that caught me twice
`crook` breaks the mirror: fore-and-aft reach, lateral reach, knee bend and thickness all
jitter per limb from a deterministic hash. **Foot height is deliberately untouched**, so
every foot still reaches the floor -- a foot in the air reads as broken, not as crooked,
and the walk code plants on the shortest leg.

Two things it got wrong, both caught by `test_nomerge_steps` rather than by my eye:

- Jittering lateral reach in both directions let a leg wander in toward its mirror partner.
  The tightest pair in the offered set fell from 0.43 shaft radii to **0.04**. Lateral reach
  only ever widens now.
- A fore-and-aft jitter bigger than the gap between two rows walks adjacent legs into each
  other. Forcing the sign by row parity made it worse -- plan 43 went to **-0.16**, i.e.
  interpenetrating. The jitter is capped at a quarter of the distance to the nearest leg on
  the same side, measured before anything moves: legs far apart get the full crookedness,
  legs already close barely move.

Tightest pair in the offered set now: **0.32 shaft radii of clear air.**

### Parts, rebuilt and new
Finn: *"feel free to change the existing genome parts to better suit the creepy new
creatures."*

- **split mandible** (`m_bloom`) -- four mandibles, two upper and two lower, that CROSS when
  shut so opening is an unlatching rather than a hinge. It rests half open, because a mouth
  that only splays mid-attack shows a closed fist the rest of the time and the splay is the
  whole idea. What is revealed is a short soft throat with two small teeth in it: small and
  wet against the hard outer segments, because filling the middle with a second set of fangs
  is what kills the contrast (rule M1).
- **stinger tail** (`s_sting`) -- five hard flat-sided segments, each 0.88 of the one before,
  then a telson that BREAKS the taper by swelling back out before the needle. A monotonic
  taper to a point reads as an antenna; the re-widening reads as a delivery device (T1).
- **bone blades** (`s_blade`) -- flat plates on edge instead of cones, leaning so no two catch
  the light at the same angle, each with a visible origin wound at its base where it came
  through the skin (L2).
- **pit field** (`e_pit`) -- no eye at all. A sunken field of pits in a shallow dish. Senses
  further than any eye and gives nothing back, so you cannot tell whether it has seen you.
- **membrane wing** (`w_bat`), rebuilt -- a wing is a HAND with FOUR panels, not one: the
  propatagium leading edge (never torn, it is the load path), the inter-digit panels (thinnest,
  most translucent, where the tears cluster), and the plagiopatagium running back to the hip,
  which is the panel that makes it read as an animal rather than a kite. The claw hook is at
  the **wrist** -- the outermost forward bend -- and not at the elbow, which is where it kept
  being put. Tears start at the trailing margin and run *parallel* to the spars, never across
  them, capped so a continuous strip always survives (W1-W4).
- **torn membrane** (`w_ragged`) -- the same architecture starved: high aspect ratio, one
  unbroken leading edge, most of the trailing half gone.
- **shell wings** (`w_insect`), rebuilt -- a hard cross-ribbed case over a folded gossamer
  hindwing. Short, broad, and there is no hand in it at all.

**The four fliers read as one bird because every wing in the game was the same leaf with
veins in it.** Three architectures, not three sets of numbers, is the actual fix.

### Wet
Finn's explicit priority. The skin shader had one Blinn lobe at a fixed power, which is why
everything was either painted plastic or matte hide with nothing between.

- Wet is **darker diffuse and smoother specular**, driven by porosity taken straight off
  gloss (`porosity = saturate(-2.5*gloss + 1.25)`), so chitin and bone stay bright while
  exposed dermis goes dark. Without that split, wet armour just goes muddy.
- Fluid **pools in cavities**, and pooling is a normal-flattening operation rather than a
  texture: the surface normal blends back toward the geometric normal as fluid accumulates,
  so mucus in the creases between plates goes mirror-flat while the ridges keep their relief.
- The slime is a **clear coat** at the fixed f0 of 0.04 that an air/coat IOR of 1.5 gives,
  taking its cut of the light first, which is why wet flesh goes dark AND shiny at once
  instead of just brighter. Plus the sky the film reflects, so a wet back is bright even with
  the sun behind it.
- **Dual normals.** The coat normal is the fluid sheet: large, smooth, and moving. The base
  normal is scales and pores: small, static, high frequency. This is the single biggest
  "it looks wet" win, because lit together they read flat.
- **The specular moves** -- a flow-mapped normal on the coat only, two samples half a period
  apart cross-faded on a triangle wave so the sheet advects forever without stretching. The
  flesh normal stays still, so the body stays solid while the highlight crawls over it. That
  is the whole trick.
- **Two specular lobes**, broad and tight, with the tight one stretched along the body's flow
  so a row of scales catches the light AS a row (anisotropy aligned to the covering, X10).
- **Cavity drives specular occlusion**, reduced at grazing angles, or the wet highlight glows
  inside creases that should be dark.
- Transmission weighted to the published six-Gaussian profile's red tail: red travels about
  ten times further through flesh than blue, which is why thin parts glow red at the rim.

Scales were also invisible: ten to fifteen shingle rows over a whole body at a relief of
`0.12/sf` is under a pixel at any sane distance, so a "scaled" animal stayed matte hide.
Coarser rows, four times the relief, a dark seam and a bright free rim on every scale.

And the hides were poster paint -- saturated yellow on black is a wasp, not something in a
wet forest. All ten pulled toward dirty desaturated flesh and chitin with one small accent.

### Tooling
- `tools/thumbs.js` + `tools/sheet.py` -- the thumbnail dump and contact sheet, **in the repo
  now instead of in /tmp**, where it did not survive the session that wrote it.
- `tools/closeup.js` -- full-viewport render of one plan. The 192 px palette tile is the right
  instrument for a silhouette and the wrong one for surface: a rib 0.2 units deep on a 6 unit
  animal is under a pixel there. Every judgement about skin in this entry was made on these.
- `tools/shapecheck.js` -- head clearance, head height above the floor, bounding aspect, leg
  lengths and mirror asymmetry, straight off the real skeleton. It does not replace looking at
  the sheet; it stops me spending three minutes rendering a plan whose head is buried inside
  its own chest. It caught exactly that on five of the ten, and two heads hanging below their
  own feet.
- `window.__G3D.bones.offeredPlans()`. **`test_nomerge_steps` had the offered plan list pasted
  into it.** That is how a guard quietly stops guarding: the picker changes and the test
  carries on proving something about a set nobody can pick. It asks the page now -- and the
  moment it did, it started testing the new ten and immediately found the two crook bugs above.

### Still wrong, and named rather than hidden
- **The sire still does not read as a dragon at thumbnail size.** Side-on it has the neck and
  the tail; from any angle that shows the wing, both foreshorten to nothing. A per-plan tile
  camera helps and does not solve it. This is the same complaint, third session running, and
  it is now the one thing I would not claim is fixed.
- `ribs` is real in the mesh (radii alternate 0.35 / 0.12 on the husk) but at the palette's
  grid step it is barely two cells of relief, so the sectioning reads in game and not in the
  tile. The tile is not lying about the silhouette, only about the surface.
- Bloom's mandible at 2.6x is arguably past "oversized feature" into "detached from the head".
- The legs under bloom are still stumps you can barely see.

### Tests
`test_plans.js`, `test_bodies.js` and `test_fight_core.js` pass unchanged. `test_silho_steps`
updated for ten offered shapes instead of fourteen. `test_nomerge_steps` now reads the real
offered list and passes at 0.32 shaft radii minimum clearance.


## 2026-09-24: Caps not balls, and a hard rule against limbs welding together

Finn, on yesterday's creepy pass: "way too much of knuckles, it looks like a bunch of round balls", "some creatures the arms are embedded into their bodies", "some limbs are too skinny to create", and then, with a picture of a creature standing on one fused trunk with a single foot pad on it: "you also need to add a hard definition which doesn't allow limbs to merge to one limb".

Four complaints. None of them was a matter of taste; each had one specific cause.

### 1. A sphere next to a sphere is a bead, whatever size you make it

I had been tuning the wrong number. Joints were spheres -- hip, knee, ankle, three per limb -- and a sphere threaded onto a shaft reads as a bead at 1.38x as surely as at 1.85x. Shrinking them only makes smaller beads.

Joints are **ellipsoids squashed hard along the bone** now: 0.54 of their width at the hip, 0.44 at the knee. Across the limb a joint still swells, so it reads as a knuckle; along the limb it is flat, so it reads as a crease. The **ankle ball is gone entirely** -- a third bead per limb doing nothing the taper was not already doing.

Swell is also divided down as the limb thickens:

```js
const cap = (amt) => 1 + (amt - 1) / (0.55 + 0.45 * clamp(th, 0.5, 2.2));
```

The ratio that looks skeletal on a thin leg looks like a boulder on a thick one, which is exactly what happened to the heavy classes.

### 2. The arms were not embedded. The body had grown over them.

A limb anchor is stored against **one** skeleton ball, at that ball's nominal surface. The skin the game draws is the smooth union of the whole chain, and a smooth union **bulges outward wherever two balls overlap**, by roughly the blend radius. So an anchor that is exactly on the surface by the editor's arithmetic is buried under the surface in the mesh, and the top of the limb is inside the torso.

The drawn hip now slides out along its own normal until it clears that bulge. Only the drawn hip moves: knee, foot and every bone pivot stay put, so gait, reach and stance are untouched.

### 3. "Too skinny to create" was literal

The mesher sizes its grid from the model's overall bounding span. A shaft thinner than about one cell does not survive surface extraction at all -- it comes out as disconnected lumps, or as nothing.

Yesterday's shafts were 0.55 and 0.66 of their old radius, which put the thin classes under that floor. That is why the stilt-walker's legs were threads with gaps in them and the centipede had no legs at all.

Shafts are back to 0.82 with a hard floor, measured against the **whole animal** and not the torso:

```js
G._limbScale = Math.max(1.2, 2 * sk.a, 2 * sk.b, 1.6 * reach);
const RMIN = 0.030 * G._limbScale;
```

Measuring off the torso was the first thing I tried and it does not work: a stilt-walker is mostly leg, so the span the grid comes from is set by the legs, not the body. The centipede went from a bare worm to a twelve-legged thing on this change alone.

### 4. The hard rule: no limb may merge into another

Finn is right that this needs to be a rule and not a tuning value, because the merge is **geometric**. The skin is one smooth union over every primitive in the animal, so two legs whose shafts pass within a shaft's width of each other are not two legs any more. They are one trunk with one foot on the end of it, and no blend setting fixes that.

So it is now enforced before anything is drawn. Every pair of limbs is measured along its whole length, at its **drawn** radius -- including the grid floor above and the foot pad, which is much wider than the shaft it sits on -- and pairs are pushed apart until there is real air between them. A pair that cannot be separated far enough, because the body is not wide enough to put them anywhere else, is **thinned** until it clears instead. A thin leg is a worse leg than a fat one, but a leg is better than half of a fused trunk.

Two things this got wrong on the way, both caught by the new test rather than by my eye:

- It measured the limbs at their *plan* radius while the renderer drew them at the grid floor, which is fatter. Legs were parted correctly and drawn touching.
- It measured from the original anchor, then the hip-clearing step above moved the limbs afterwards -- sometimes straight back into each other.

Both are now the same number in both places.

**`test_nomerge_steps.js`** is the guard. It builds every offered shape, reads the primitives the renderer actually emits, groups them by which limb's bones they are bound to, and measures surface-to-surface clearance between every pair of shafts from different limbs. It reports in shaft radii, so the number means something: 0 is touching. The tightest pair in the offered set is now **0.43 shaft radii of clear air**; before this pass, two shapes were at **-0.26 and -0.19**, i.e. interpenetrating.

### The torso was a pile of boulders too

Not something Finn named, but visible in the shot he sent. The spine is a chain of cones and those crease nicely. But **six satellite spheres** -- shoulder and hip masses -- were bolted onto that chain with a tight blend, and spheres bolted onto a tube with a tight blend look like rocks glued to an animal. They are blended hard now (`kb * 3.0`) so they read as muscle over the ribcage, while the creases between chain segments, the ones that make a body look sectioned rather than moulded, stay.

### Still owed from the art brief

Untouched today: the split mandible snout on the lurker, tattered translucent wing membranes, claw hooks at the wing bend, whip-segmented stinger tails, crooked uneven leg lengths on the stilt-walker, angular chiselled limb planes. **The four fliers still read as one bird**, and the drake still does not read as a dragon.

## 2026-09-25 (later): The skeleton learns to hunch, and I start looking at what I build

Finn, on the fourteen shapes I shipped this morning: "these all read as little insect, not scary creatures", and "they look nothing like the reference images I sent you". Both true.

### The mistake worth writing down
My distinctness test compared **plan parameters** -- body length-to-width, stand height, leg count, neck and tail length, wingspan. Fourteen sets of numbers, all different, test green, ship. It never looked at the rendered shape, and a test that cannot see the picture cannot tell you the picture is wrong.

There is now a **thumbnail dump**: the palette renders every plan to a data URL, so those can be pulled out of the running page, written as PNGs and put on one contact sheet. Everything below was decided by looking at that sheet.

### Three things the skeleton could not say
The real reason no amount of tuning helped: every body was one smooth sausage with a neck on top, because that is all `buildDesignSkeleton` could describe.

- **`hump`** -- `arch` was a symmetric sine, so the high point of the back was always the middle of the animal. Skewing the sine moves the peak over the shoulders and lets the hips fall away behind it. Every one of Finn's references is built on that silhouette. A plan with no hump is byte-identical to what it was.
- **`waist`** -- one girth curve means one mass. A thorax and an abdomen with a pinch between them, which is most of the reference sheet, could not be expressed at all. A gaussian notch in the profile gives two lobes.
- **A head that hangs.** `ny` -- the neck's vertical direction -- was **positive in all thirty plans**, because I had never once written a negative one. Nothing in the code required it. A negative `ny` slings the skull low and forward off the shoulder mass, which is the difference between a grazing animal and something that hunts you. Eleven of the fourteen are negative now. This needed no code change at all and was the single biggest cause of the problem.

Long necks also had to get **thicker**: neck radius was a flat `0.34 - 0.10u` regardless of body size, so on a big animal a long neck tapered to a thread and read as a snout stuck on a log. It scales off the body now.

### Too many spiders
Six of the ten land shapes stood on thin splayed legs, so the set read as a bag of insects. Thin legs only mean something if most things do not have them.

Three keep them because they are arthropods and it is their whole silhouette -- **scorpion, centipede, stilt-walker** -- and **hollow** stays gaunt because gaunt is the point. The other six went heavy: thick limbs, fewer of them, planted under the body rather than splayed beside it. The crab is an armoured tank on stumps. The mantis stands on one heavy digitigrade pair with its blades up. The thing carrying a head twice its size has legs that could carry it.

### The ribcage closes
The ribs arced up but never came back: `z = w * (1 - cos(th))` only ever grows, so every rib swept outward for its whole length and a carcass read as a row of spokes off a stick. A rib now bows out at the middle of its arc and comes back in at the tip -- `z = w sin(th)`, `y = h (1 - cos(th))/2` -- so the two sides reach toward each other over the spine, stopping just short of meeting. Seven segments instead of four, so the sweep is a curve rather than four faceted tubes.

### The picker was hiding the creature
The single biggest thing wrong with the shape tiles was not the shapes. It was this line in the thumbnail renderer:

```js
d.parts = d.parts.filter(q => sim.t === 'FIN' || sim.t === 'WING')
```

Every plan tile threw away all its parts except wings and fins. No eyes, no jaws, no horns, no spikes, no crests, no tendrils -- fourteen bare torsos. That is why they read as blobs and sticks, and why all four fliers looked identical: the one part any of them was allowed to keep was the wing. The tile draws the whole animal now, still flat grey so it reads as a silhouette rather than a painted creature.

**And the wings were being looked at edge-on.** A wing is a flat membrane; from the side it is a line, which is exactly what all four fliers showed -- one long needle and a lump. Winged plans are now viewed from above and in front, so the wing shows its area.

### Four rebuilt off the references rather than off my own names
Lurker, maw, hollow and hydra all failed for one shared structural reason, obvious once the contact sheet existed: **the head was inside the body.** The skin is a smooth union of capsules, so a head whose centre sits within a body radius of the torso is not a head, it is a bulge. The lurker had a skull 80% of its body radius parked 0.28 units past the end of a body 0.82 long -- there was never going to be a face on it.

- **LURKER** is the horned ape now: heavy, hunched, thick arms, a blunt head carried clear and low, and a beard of tendrils under the jaw.
- **MAW** is the white xeno: a back that peaks hard over the shoulders, a small skull slung down and forward well clear of the chest, a long bent hind leg and a long spiked tail.
- **HOLLOW** is the horned brute: upright, enormous shoulders over small hips, long heavy arms reaching past the feet, tusks.
- **HYDRA became STALKER**, the spider-xeno: a low hunched thorax with a pinched waist, the head tucked under its leading edge, bladed forelimbs held up in front.

### Still wrong, and named rather than hidden
**The four fliers are still too close to each other.** With the parts restored and the camera moved they are now visibly four different animals -- the flit is round and crested, the moth carries two wing pairs on a fat pinched body, the skimmer has the long beak, the drake has the neck and the tail -- but at thumbnail size they still read as variations on one bird. The drake in particular does not read as a dragon: its neck and tail are long in the numbers and short on screen. That is the next job.

### The creepy pass: joints, eyes and bone through the back
Finn's art brief, worked through. Three of these apply to every creature at once, which is why they beat any amount of per-plan tuning.

**Knobby joints, starved limbs.** A limb was a smooth pipe. It is knuckles with bone between them now: the joints swell well past the shaft, the shafts are two-thirds the old thickness, the knee is an ellipsoid squashed along the bone so it reads as a knee CAP rather than a ball, a spur rides on it, and what little flesh there is sits high near the joint.

**There are TWO limb renderers and I had only fixed one.** `drawLimbTube` is the instanced path; the fused creature mesh -- which is what the game actually draws -- builds its own, and that one had **no knee joint at all**: a root ball and two smooth cones straight to the foot. That is why in-game limbs stayed plastic however much the other path was sharpened. It has a knee, an ankle and a tight blend at each now.

**The body stops melting.** Spine capsules were unioned with a smooth minimum of 0.32 of the body radius, which is a very molten join: every segment rounded away into the next and the result was one bean. At 0.20 the segments meet with a visible crease.

**Eyes.** Large round side-facing eyes are what prey has -- rabbits, birds -- and were most of why these read as friendly. The set is blind, hooded or beaded now: clusters of small irregular beads on ten of the fourteen, slits on the rest, nothing large and round anywhere.

**Bone through the back.** The heavy classes had a smooth curved spine. Their vertebrae break the skin as a row of spurs that grows over the hump and dies away toward the hips, so the hunch has an edge on it.

One tuning pass was needed after looking: the first cut swelled the joints to 1.85 and 2.05 times the shaft, which turned the thick-limbed classes into strings of beads. 1.38 and 1.52 reads as a knuckle.

### Still owed from the brief
Not done yet, and named rather than buried: the split mandible snout for the lurker, tattered translucent wing membranes, claw hooks at the wing bend, whip-segmented stinger tails, crooked uneven leg lengths on the stilt-walker, and angular chiselled limb planes. The drake still does not read as a dragon.

### Tested
- `HP_REF_MASS` raised twice in one session as the shapes got heavier (28 -> 32 -> 36): the guard asserting that it still covers the heaviest startable body caught both, which is exactly what it is for.
- Replayed green: `test_bodies`, `test_plans`, `test_hp`, `test_core`, `test_combat`.

## 2026-09-25: Three tabs, fourteen shapes, and a kill worth a quarter

### The builder has three tabs
It had twelve: kit, shop, body, proportions, paint, and seven category tabs that -- since the kit/shop split -- only ever listed what you already owned, which is what the kit tab is. Seven tabs to say what one tab already said.

- **YOUR KIT** everything you own, one list, draggable onto the body
- **THE SHOP** everything you do not, by rarity, with prices
- **BODY** the shape, and directly under it the sizing sliders and the colours

The sizing and paint controls were **lifted into functions** the body tab calls rather than copied into it: two copies of a slider is two places for it to drift.

### The starting pack is a working animal
It was a mouth and one random weapon, which is not a creature: with no eyes you cannot see and with no spare limbs you cannot change how you stand, so everybody walked out of the cage with the body they were handed.

The pack is now **eyes, limbs and a mouth** -- the three things a body cannot work without -- from the common tier, plus one extra about half the time. **Everything in it comes as a pair**, because a body is bilateral and one of anything cannot be placed symmetrically.

### A kill is worth a quarter of what you killed
Growth was `gain = 0.02 + 0.10 * ratio` added to every cell, every kill: no relationship to any number a player could reason about, and three or four kills made you enormous.

The rule is one sentence now: **killing something adds a quarter of its mass to yours.** Mass rises with cell growth, so the whole body is multiplied by exactly `(1 + 0.25 * preyMass/yourMass)`, which lands the new mass on `yourMass + 0.25 * preyMass` and nowhere else -- measured exact in the browser. It is also self-limiting in a way the old curve was not: the bigger you get, the less any one kill is worth, because the ratio falls as you grow. A newborn is capped at 4x prey-to-self so it cannot triple off one lucky kill.

### Fourteen body shapes, and nothing else
"They are a lot too similar and too dinosaur looking." Thirty plans, most of them the same animal with the neck a bit longer.

The picker offers **ten that walk and four that fly**, each built around one feature allowed to be absurdly oversized:

| | |
|---|---|
| **LURKER** | one enormous body, a face across the whole front, legs you can barely see under it |
| **MAW** | a head bigger than the animal carrying it. A walking mouth |
| **HOLLOW** | tall, gaunt and vertical, arms that hang almost to the floor, a head far too small for the frame |
| **CENTIPEDE** | very long, very low, twelve legs down a segmented body |
| **SCORPION** | wide armoured carapace, pincers, a tail over its own back |
| **STILT-WALKER** | a tiny body carried absurdly high, head fused into the torso, stub tail |
| **CRAB** | flat and far wider than it is long, legs splayed around it |
| **BRUTE** | shoulders and arms, almost no hips |
| **MANTIS** | upright and narrow, folded blades, a long abdomen behind |
| **HYDRA** | a low heavy body under one enormous neck |
| **FLIT** | small, light and quick |
| **MOTHWING** | enormous wings on almost nothing, and no legs at all |
| **SKIMMER** | wings that are its arms, folded to move on the ground |
| **DRAKE** | long neck, long tail, four legs and a wingspan over all of it |

**The other sixteen plans stay in the table.** Renumbering would turn every saved design into a different animal; they are simply not offered.

Three things the tests caught that would have shipped otherwise:

- **The drake could not fly.** Lift needs wing area against mass and a drake is heavy: at mass 30 its wingspan was under the threshold, so the dragon walked. Wingspan raised until it actually beats its own weight -- 79 air speed now.
- **The hollow had no arms.** `classifyLimbs` calls a limb a leg when its foot lands within 72% of the mean foot depth, and the walking physics reads the same function -- so arms that hung nearly to the floor quietly became a fifth and sixth leg. They hang clearly shorter than that now, and it has 4 legs and 2 arms.
- **The drake broke the 100 hp invariant** at mass 30.5 against a reference of 28. `HP_REF_MASS` is 32, and the guard in `test_plans.js` that asserts it still covers the heaviest startable body is what flagged it.

The distinctness check also needed fixing rather than the shapes: it scored a winged drake and a wingless mantis as near neighbours because **wingspan was not one of the axes it compared**, which is the single most visible thing about a silhouette that has one.

### Tested
- `test_bodies.js` (node, new, 21 checks): all fourteen exist, none develops a broken number, every one reads exactly 100 hp, every land shape walks, the moth has none and the other three fliers stand, all four fliers fly and no land shape does, all can see and all have mouths, no two share a silhouette and the closest pair still differs on several axes, each shape's headline feature is measurably extreme, a world full of them runs for ninety seconds, and twenty generations of mutation never breaks one.
- Browser checks for the three-tab builder (shapes, four sizing sliders and the colours all on the body tab) and for the exact growth arithmetic.
- Two stale assertions updated to the new contract rather than the game bent to fit them: the pile check still expected two parts, and the silhouette check still expected thirty shapes.
- Replayed green: `test_core`, `test_fight_core`, `test_combat`, `test_hp`, `test_plans`, `test_bones`, `test_loadout_steps`, `test_brstart_steps`, `test_silho_steps`.

### Still not in this build
Stealth, sneaking and tall grass; rarity-glow loot beacons; ruined buildings and cover walls; the leg-intersection bug; 50 players; the wet-hide shader; and the horror pass on the body rather than the mouth.

## 2026-09-24 (late): Ribcages, one loot per body, and nothing bites you over its shoulder

Three from playing it.

### A carcass is a ribcage, not a stain
Bodies were drawn at the size of the animal that made them and started sinking into the soil almost immediately, so what should be a landmark you cross a valley for read as a mark on the ground.

- **Drawn at 1.8x the animal**, on purpose. A field body is a loot landmark, and a ribcage you can pick out from across a valley is worth more to the player than an accurate one you walk past.
- **The soil takes them late and slowly.** Sinking used to start at a quarter of the way through a body's life; it starts at 55% now and takes 30% less of the height when it does. A body at half its life is not sunk at all.
- **The ribs stand up.** The spine sits at 1.05 of the body radius instead of 0.62, the rib sweep is tightened from 1.85 to 1.42 radians so they arc upward instead of folding flat along the ground, the outward splay is more than halved, and the bone is thicker so a big ribcage does not read as wire at distance.

### One body, one loot
Killing something opened the part picker AND left a body you could then search for a second pair. Two rewards for one kill, through two separate code paths -- which is also exactly how they came to disagree with each other.

**A kill pays genome space and bones and no parts at all.** It leaves a body, and the body is searched with E like every other body on the island. The toast says how many parts are on it. The same now goes for killing a player in a match: their body is at your feet and you search it.

One loot, one path, one rule -- and it is the rule that was already there for the streamed carcass field.

### Nothing hits you without looking at you
Heading is eased toward the direction of **travel**, and only while moving. An animal that stopped walking in order to attack kept whatever way it happened to be pointing and bit you over its own shoulder.

- **It must be facing you to commit.** `fightHitFilter` refuses the wind-up outright if the attacker is more than about 110 degrees off you. Refusing rather than letting it fly is what turns "it bit me sideways" into "it turned, then bit me": the refusal costs it the tick, it spends that tick turning, and it tries again.
- **It must still be facing you when the strike lands**, within about 75 degrees -- tighter than the commit angle, so something that turns away mid-swing throws it.
- **Turning is behaviour, not rendering.** `fightFaceTick` runs in the fight tick, every frame, for anything that wants to hit you, whether or not it is on screen. A rule that only runs inside the renderer is a rule that stops the moment the creature leaves the frame.
- **Heavy bodies swing round slowly** (4.2 rad/s falling to 1.3 with mass), which is what makes getting behind a big one worth doing.
- The renderer stops easing toward the direction of travel while a creature is committed to facing you, or stepping sideways would drag it off target every frame.

Facing is read off `_yaw3` -- the same heading the body is actually drawn with -- so what the player sees is what the rule uses. A creature that has never been drawn is not punished for a heading it does not have yet.

### Tested
- `test_fixes3_steps.js` (browser, new, 18 checks): bodies exist and are drawn at 1.8x, a body is unsunk fresh and at half its life and only part buried when old; a kill opens no picker and hands over no parts, the body it leaves can be searched, searching is what offers the parts, taking one is what grants it, and the same body cannot be searched twice; facing is true head-on and false side-on, away and at a wide angle, the commit angle is wider than the landing angle, and something that wants to hit you turns from facing away to facing you.
- A stale surface caught by this: the test harness's `eat()` still pointed at the old food function, so it could have passed against code the player can no longer reach. It runs the E key's real action now.
- Replayed green: `test_core`, `test_fight_core`, `test_combat`, `test_hp`, `test_plans`, `test_bones`, `test_carcass_steps`, `test_brstart_steps`, `test_loadout_steps`.

## 2026-09-24 (evening): The loadout screen, the horror pass, and a machine that could not keep up

All of this came in from Finn while playing.

### The builder is a loadout screen now
It was a bright blue box with green buttons and rounded frames round everything: a settings dialog with an animal in it. The read is a weapon loadout now. Near black, hairline rules instead of boxed frames, nothing rounded, no gradients pretending to be glass, flat stencil type with wide letter-spacing, and one hot accent (dried blood) against bone. Panels have no edges at all: they bleed off into the dark and are separated by single-pixel rules, which is what stops it reading as a web form. Over the top, a vignette, a fine grain and a slow scan line, all three pure CSS.

It is one override block appended after the editor's stylesheet rather than forty separate edits to it, so the original rules stay readable underneath as the structure this is a skin over.

**The body shapes are bone-white silhouettes on black rather than black on light.** Finn asked for black outlines; on a near-black screen a black silhouette is an empty square, so it is the same idea the right way round, and it is how a loadout screen renders a weapon.

### KIT and SHOP
"Right now it's so hard to see what you have or not." It was: every category tab showed every part in the game, owned and unowned mixed together, told apart by a small padlock and a line of grey text.

- **KIT** is the first tab and the one the builder opens on. Every part you own, all categories at once, one row each: how many you have, how many are on the body, how many are spare, rarest first, drag straight out of it onto the creature.
- **SHOP** is everything you do not own, cheapest first, grouped by rarity with its price.
- **The category tabs now list only what you own.** Nothing is shown anywhere that you cannot use, except in the shop, where being unable to use it is the point.

The body you walked in with counts as kit whether or not you own copies of its parts, which is the existing rule that stops an inherited animal being locked out of its own anatomy.

### V was two keys
`if (k === 'v')` appeared twice in the same keydown handler and both ran: one detached the camera, the other searched the body at your feet. Pressing V over a corpse did both.

Searching is a pick-up and E was already the pick-up key, so the body joined that list rather than getting a key of its own. **E now takes whatever is in front of you**, in the order you cannot come back for: the cage pile, then a body, then a mushroom. V is the camera and nothing else.

The handler also **reads its own source at load** and warns if any key is ever handled twice again. It scans the code rather than a list somebody has to remember to update, so it cannot go stale.

### Mushrooms: a third the size, a ninth as dense
The cap was wider than the animal picking it up and there was one every 760 units. Cell is 1900 now and the odds are 0.30, which is about a ninth of the first cut; the cap is 5.6 by 4.6 against 15 by 13. Pick-up reach came down with it, or you hoover one up from a body length away.

**The whole screen carries the colour of what is running.** One layer per spell, so two at once reads as two colours rather than being averaged into a third that means nothing. It breathes slowly and flashes faster in the last few seconds.

**All four pouch slots are always on the HUD**, dark and empty-looking when you hold none. Stacking always worked -- POWER plus RAGE is x3 and the test has asserted it since it was built -- but the only way to find out you were not holding a WARD was a toast you missed.

### Health comes back
Ten seconds after the last thing that hurts you, then 10 a second, for everyone in a match rather than just the player. The delay is a countdown on the body rather than a timestamp against a clock, because there are three clocks in this page and a regeneration rule that reads the wrong one behaves differently at different sim speeds.

The first cut dropped the leftover fraction of the frame at the boundary, which made the heal rate depend on the frame rate (69.90 against 70.00 over the same fifteen seconds) and let a float residue silently eat a tick. The remainder is carried through now, and the test asserts the same result from 10 ms steps and from 1 s steps.

### No fish in a deathmatch
The zone closes onto dry land, so a body that cannot leave the water is not a competitor, it is a slow drowning.

The rule is anatomical rather than a list of banned shapes, because a list goes stale the moment a plan is added: **a match body must have legs**, checked by building it and asking `classifyLimbs` -- the same function the walking physics uses -- how many came out. Eight of the thirty plans genuinely cannot walk; each is swapped for the nearest walking plan, keeping your colours and proportions, and you are told in the cage. Bots **re-roll** rather than swap, so a lobby stays a spread of real land shapes instead of collapsing onto one substitute. The picker hides water shapes while a match is armed.

### The horror pass: what a mouth does when it is not biting
Reference: xeno sculpts and arthropod horror. What makes those read as frightening is almost never polygon count. It is that the mouth is alive when nothing is happening.

- **Drool.** Strands off the tooth line, each on its own phase, thinning as they fall and ending in a bead, with a slow stretch-and-snap cycle -- plus one bead per mouth that has already let go and is falling. Saliva that never parts is a rubber band and the eye notices.
- **A tongue that lolls**, in segments, further out the wider the gape, dragging sideways as it goes, and wet at the tip.
- **An inner jaw**: a second, smaller set of jaws deep in the throat that thrusts forward as the first set closes, so a bite reads as two bites. You only see it when the mouth opens, which is exactly what makes it land.
- **Lip tendrils**: cilia at the corners, each on its own phase so they never move as a block.

Grinding beaks and duck bills get none of it. A duck bill with a lolling tongue and an inner jaw is a comedy, not a horror.

The first cut reached for `g.tube()`, which exists on the ordinary part builder and **not** on the wrapper the jaws draw through -- so it worked in the palette icon and threw in the game. Everything is cones and balls now, which every builder has.

Measured on a crushing jaw: 92 pieces without the wet detail, **139 with**, 48 at distance, 154 in the palette icon.

### The quality setting drops itself
"My computer basically can't run ultra at all, every frame takes like half a second."

There was an automatic system and it was solving the wrong problem: it scaled the render **resolution** between 100% and 60% and nothing else. At half a second a frame the cost is eighteen thousand plants, three shadow cascades and a two-hundred-thousand instance budget -- rendering all of that at 60% of the pixels is still all of that.

The automatic system moves the **preset** now, with the resolution trim as the fine adjustment underneath it.

- **Down fast, up slow.** Two levels at once if the first seconds are dire (over 55 ms), one at a time above the 22 ms budget with a four-second cooldown, and a wide dead band so it cannot oscillate. Climbing needs twelve unbroken seconds under 11 ms *and* the resolution already back at full, so it can never fight the trim.
- **It never overrules you.** Touching the quality buttons turns automatic stepping off for the session. Measured: a player who picks ultra keeps ultra through eight seconds of 500 ms frames.
- **A machine nobody has measured starts at medium, not high.** First impressions of a frame rate are permanent.
- It says what it did, once, naming the level and how to stop it, and the readout carries the level beside the fps.
- **The horror geometry rides the same setting**: drool, tendrils, tongues and inner jaws are high and ultra only. Palette icons keep them at every level, because a still image costs nothing and it is the one place anybody looks at a mouth closely.

### Tested
- `test_loadout_steps.js` (browser, new, 18 checks): the builder opens on the kit, no key is double-bound, nothing is rounded, the play button is not green, grain and vignette and scan are present, the shop lists only what you do not own and none of it is draggable, what you buy moves from one list to the other, a kit row states its count and its spares, and a category tab shows only what you own.
- `test_perf_steps.js` (browser, new, 12 checks): four levels, an unmeasured machine starts at medium, 500 ms frames drop two levels at once, 30 ms frames drop exactly one, 18 ms frames change nothing, a few fast seconds do not climb but forty do, and a player's own choice survives eight seconds of dire frames.
- `test_horror_steps.js` (browser, new, 7 checks): the new geometry throws nothing, the creature still draws, and the piece count goes 92 -> 139 with the wet detail on, 48 at distance, 154 in the icon.
- `test_hp.js` extended with six regeneration checks including frame-rate independence.
- `test_brstart_steps.js` extended with seven checks that no body without legs can enter a match, that the swap keeps your colours, and that not one bot in the lobby is a fish.
- Replayed green: `test_core`, `test_fight_core`, `test_combat`, `test_bones`, `test_plans`, `test_spell_steps`, `test_brstart_steps`, `test_loadout_steps`.

### Still not in this build
Stealth, sneaking and tall grass you can hide in; Fortnite-style rarity-glow loot beacons floating over bodies; ruined concrete buildings and cover walls; the leg-intersection bug; 50 players; the wet-hide shader work (object-space triplanar detail, clear-coat, translucency); and the rest of the horror pass on the BODY rather than the mouth -- exposed ribs, muscle striation, blood that stays on the hide after a fight.

## 2026-09-24 (pivot): It is a fighting game

Finn called it mid-session: the food was the problem, not the food's art. "Having to constantly eat food is super annoying and dumb. It should be a fighting game." Everything below follows from that.

### Health and damage are designed numbers now, not ecosystem leftovers
Health was the food tank. That is why a fight had no shape: how long you lived was how recently you had eaten, and how hard you hit was a stat built for deciding whether a herbivore starved.

**Health is its own pool.** `c.hp`, with `ph.hpMax` beside it. Energy stays exactly what it was and runs the free-roam ecosystem; it never decides a fight again.

- **Every starting body reads exactly 100.** Not approximately: all ten stock body plans measure 100.0, and so does a newborn. The reference mass is the *heaviest* stock plan rather than an average one, which is what makes "whatever shape you start as, you have 100 hp" literally true rather than nearly true.
- **Only a genuinely large body goes above it**, sub-linearly: mass 40 reads 190, mass 160 reads 537, and the ceiling is 1500 so nothing is unkillable.
- **Growth keeps the wound, not the number.** `hpOf` carries the same fraction across a change of maximum, so getting bigger mid-match heals you in proportion instead of leaving you on a sliver of a much larger bar.
- Writing health straight onto a creature is now safe. The first cut treated a missing `_hpMax` record as "full" and silently healed anything that set `c.hp` by hand — found by a nameplate test reading 100% off a body that had just been halved.

**Damage is the part you are carrying.** A bare body hits for exactly 10. A body wearing the rarest weapon in the game, fully kitted, hits for exactly 100.

- The rarity table moved *into the core* and bones now reads the core's copy. Price, wild spawn odds and damage were about to be three tables that agreed by hand.
- Weapon quality is scored once at compile time from the parts actually worn: the single best weapon sets three quarters of it, everything else fills the rest. Measured ladder for one weapon on a stock body: weak 10.5, solid 14.0, super 22.7, ultra 39.2, alpha 60.6, full kit 100.
- **Armour is a fraction, never a subtraction.** It cannot take a hit to zero and cannot be out-scaled into irrelevance: the ceiling is 55%, and defence 400 sits exactly on it.
- **No fight can stall.** Every hit is at least hpMax/30, so thirty clean hits ends any matchup in the game. Swept 4000 random pairings: worst case 30.0.
- Two stock creatures now settle it in about **9 hits**. Kitted-against-stock runs a median of 18 damage and a p95 of 58.

### The self-damage, which was two bugs wearing one coat
"I swear as I'm dealing damage I am also taking damage." Both real.

1. **Every swing charged you the target's `counter` in full, silently.** Bite, claw, tail sweep, anything — you paid for their spikes on contact you never made. It now only answers a body slam or a headbutt, is capped at a tenth of what you dealt, and says so out loud when it happens.
2. **Match bots wrote into your health directly.** `best.c.energy -= dmg` walked straight past the wind-up, the guard, the parry window and the dodge — against the *only* opponents a match has. Their strikes go through `world.hitFilter` like everything else now, so the defence the game gives you actually works.

Killing something no longer poisons you either. You are not eating it.

### Food is gone
No eat key, no food prompt, no bites, no reach check on a bush, no carrion meal, no poison, no illness, no starving. The player's body is held full so the only number that can kill you is the one a fight moves. The island's own metabolism is untouched — free roam still has an ecosystem, and a match has no wildlife in it to care.

`test_food_steps.js` is deleted rather than repaired. It tested a mechanic that no longer exists.

### Four mushrooms you carry and spend
What replaces food. Big glowing caps standing on the ground across the island, placed the way the carcass field is placed: one candidate per 760-unit cell, presence and kind rolled out of the world seed, so every player in a match walks onto the same mushrooms without a byte crossing the network. Built only near you, dropped again beyond 2600 units, and a picked cap regrows after 150 seconds so a long match does not end on a stripped map.

| key | | | what it does |
|---|---|---|---|
| 1 | POWER | yellow, 30s | every hit you land is doubled |
| 2 | WARD | green, 20s | nothing can hurt you at all, fog included |
| 3 | VEIL | purple, 30s | 0.45x size, a quarter as noticeable |
| 4 | RAGE | red, 20s | x1.5 damage and health, 1.35x size, 1.3x speed |

You **glow the colour of whatever is running**, pulsing, and flashing faster in the last four seconds so it running out is never a surprise. VEIL and RAGE really do change the body's size and speed rather than the HUD claiming they did. RAGE's extra health arrives by raising the ceiling, which means the fraction carries and you are healed in proportion — and when it ends, exactly that much goes back.

Different spells stack (POWER plus RAGE is x3). The same spell cannot be stacked on itself. You carry at most three of a kind. **Nothing survives the round**: the pouch is not in `PROG`, and it is emptied on entering a match and on leaving one.

### A match starts everyone at nothing
- **Bare bodies.** Your shape comes in; your kit does not. Legs stay, because a creature that cannot walk is not a fighter, and their tips are reset to a plain foot so a bought claw cannot ride in on the end of a leg. Eyes stay, because being blind is not a fair start. Measured across a full lobby: 24 players, maximum weapon quality 0, 100 hp each.
- **Your own parts are stashed, not wiped.** Held aside for the length of the match and handed back exactly as they were, so a match can never cost you what you own.
- **A pile on your cage floor** holding a mouth and one random spike, claw or horn, rolled per player from the match seed so every bay holds an equivalent heap and nobody can reroll theirs. Press E. That is your entire head start, and the thirty seconds before the gate is what it is for.
- **No wildlife at all.** Not thinned out over five minutes: none, ever. The ruleset builds the island empty and a per-tick sweep keeps it that way whatever else in the page decides to spawn something. The old bodies are still lying around to be searched, so there is still loot on the ground and nothing breathing that is not somebody.
- **The cage holds you.** The confinement ran *before* your movement did, so every frame you were pulled back to the wall and then walked straight out of it again — which is exactly why you could leave through any of the three barred faces. It is applied after the move now, and the gate face only opens once the gate has actually lifted.

### The first screen is two words
CAMPAIGN and ONLINE, and a third line for everything that is not a choice about what to play. Continue, new game, saved creatures, controls, options and about are all still there, one click deeper, no longer competing with the only two decisions a player arrives wanting to make. Story mode is not built; CAMPAIGN is the island it will be set in.

### Eight new body plans, and a picker with no words on it
"The original creature shapes are too generic, and too dinosaur looking. They need to just be natural creatures, not necessarily existing creatures." Reference: arthropod sculpts and silhouette studies.

**Scorpion, stilt-walker, hydra, mantis, crab, brute, drifter, wyrm.** Appended at indices 22-29, never renumbering the existing twenty-two, because a saved design stores its plan by index and renumbering would quietly turn somebody's creature into a different animal. The mythical shapes are simply what the picker offers first now.

- The **scorpion** needed one new skeleton parameter: `tailUp`, which sweeps the tail up and over the back instead of trailing it. It is zero for every plan that existed before, and the test checks all 22 of them tail-ball by tail-ball against the closed form the tails were built with — 0 balls moved, worst error 0.00e+0. Eight splayed legs, a low armoured carapace, heavy pincer arms carried out front, and a stinger on the tip.
- The **stilt-walker** stands at 3.1 against the brute's 1.7: a tiny body carried absurdly high on thin legs. The **crab** is 0.88 wide against a 0.95 half-length. The **hydra** has a neck of 2.95, longer than anything else in the game. The **drifter** and the **wyrm** have no legs at all, so they are water-bound until they take legs off something.
- A **`legRow` helper** came out of this. The splayed paired-leg loop had been written out by hand three times already; the new plans would have made it six.

**The body picker shows black silhouettes and no names.** The thumbnail is already a render of the real body on a transparent ground, so `brightness(0)` turns it into exactly a black shape of the animal you would actually get. Nothing is described. You pick the shape you like the look of.

**The health calibration moved with them.** The scorpion's default body is mass 27.3, heavier than the old reference of 17, so it was reading 143 hp — breaking the invariant the whole model rests on. `HP_REF_MASS` is 28 now, and `test_plans.js` asserts both that it still covers the heaviest startable body *and* that every one of the 30 plans reads exactly 100. If a heavier plan is ever added, that test fails loudly rather than the promise quietly becoming untrue.

### Tested
- `test_hp.js` (node, new, 27 checks): the 100 on every stock plan and on a newborn, the sub-linear climb above it and the 1500 cap, the exact 10 and the exact 100, the rarity ladder's monotonicity, armour's ceiling, the 30-hit guarantee swept over 4000 random pairings, the stock-vs-stock median, that a bite costs the attacker nothing and a ram costs at most a tenth, and that health written by hand is respected.
- `test_spell_steps.js` (browser, new, 24 checks): the field exists and is stable and of four kinds, the renderer queues one instance per cap in its own colour, walking onto one and taking it, the carry cap, every multiplier of every spell, RAGE's proportional heal and its proportional un-heal, WARD refusing damage outright, spells expiring on their own clock, two stacking, and a reset emptying everything.
- `test_brstart_steps.js` (browser, new, 20 checks): bare on spawn with 100 hp and eyes and legs and plain feet, every bot the same, your own parts stashed and restored, the pile in your bay and its contents and that it cannot be taken twice, zero wildlife at the start and zero two minutes later, all four cage walls holding while the gate is down, and the gate opening on its own clock.
- `test_menu_steps.js` (browser, new, 11 checks): three lines, the right two first, nothing else competing, and every page behind MORE still reachable and working.
- `test_plans.js` (node, new, 20 checks): every new plan compiles, develops finite physiology and reads exactly 100 hp; the walkers have legs and the drifter and wyrm have none; the scorpion's tail really does rise over its back and ends in a stinger and carries pincers; no two of the eight share a silhouette; twenty-two existing tails are provably unmoved; the health reference still covers the heaviest startable body; a world full of them runs for ninety seconds; and twenty generations of mutation never produces a broken body.
- `test_silho_steps.js` (browser, new, 6 checks): all 30 shapes shown, all as silhouettes, zero names, rendered pure black, mythical first, scorpion present.
- Replayed green: `test_core`, `test_eco`, `test_combat` (updated to read health rather than the food tank), `test_fight_core`, `test_bones`, `test_food_eco`, `test_inv_steps`, `test_carcass_steps`, `test_spawn_steps`, `test_fly_steps`, `test_pack_steps`, `test_br_steps`, `test_shop_steps`.
- Two stale tests repaired rather than the game bent to fit them: `test_carcass_steps` asserted one copy per pickup when pickups have paired since yesterday, and `test_inv_steps`' sell check bought four legs onto a body already wearing four — every copy worn, so the sale was correctly refused. It buys an unworn limb now.

### What Finn asked for that is NOT in this build
Said plainly so nothing looks finished that is not: Fortnite-style rarity-glow loot beacons floating over bodies; stealth, sneaking and tall grass you can hide in; the creature horror pass (drool, long tongues, inner jaws, tendril beards, wet chitin); the new mythical body plans and the scorpion; ruined concrete buildings and cover walls; the leg-intersection bug; 50 players instead of 24; and the shader work (object-space triplanar detail, clear-coat, translucency). All queued at the top of the roadmap.

## 2026-09-24 (later): Pickups come in pairs

Taking one spike off a corpse gave you one spike. A body is bilateral, so a symmetrical change needed two separate kills, and mirror mode in the editor (the obvious way to place anything) was refused on the very part you had just earned. Every pickup now yields a **pair**.

`choosePicked` is the single funnel all three pickup paths run through (killing a creature, killing a player, searching a carcass), so pairing there cannot come out inconsistent between them. The constant is `PICKUP_PAIR` next to `ownedGain`. All three offers now read "take a pair of one of their parts", and the toast tells you to mirror it.

**Buying is deliberately not paired.** The shop charges per copy, so doubling the goods at one price would halve every price in the game. Selling is unchanged at one copy at a time.

Known red: `test_inv_steps.js` fails one of its fifteen assertions, "selling removes one copy, not the lot". The sell is refused because the creature in that run wears all four legs it owns, which is the rule working; the assertion assumes a spare exists. The sell path shares no code with this change. It needs the test fixed rather than the game, next run.

## 2026-09-24: Food is food now, not a pickup

A plant used to be a number in a list. You walked into it, the whole thing vanished, an identical one respawned somewhere else, and nothing about it was worth looking at. This turns the island's food into something with a kind, a crop, a season and a consequence.

### Plants have a kind, and it is decided once
Every plant now rolls a **kind** out of its own position hash the way `isTree` always has, so what you see, what you can eat, and what it does to you are one decision that cannot drift apart: **grass, berry bush, fungus, tuber, cactus, kelp**, weighted by biome. A fungus or a rare berry can be **poisonous**. Each carries how many **bites** it is worth and how far off the ground the food sits.

The position hash needed fixing first. `x * 7919 ^ y * 104729` is about 1e8 anywhere on the map, so bits 27 and up never move: the first cut of this had every fungus on the island poisonous. The hash is mixed properly before new fields come out of it.

### Crop, and regrowth that costs nothing
A plant holds a **crop** from 0 to 1. Eating takes one bite of it, not the whole plant, and the plant is **never deleted**. Crop is not stored live and there is no per-tick loop over plants: it is resolved on read from the crop left at the last bite and the world clock then, scaled by the season. 2400 plants cost exactly what 20 do.

Calibration: with nothing deleted, the spawn loop saturates at `maxPlants` and regrowth becomes the entire food influx, so `regrow = plantRate / CAPS.PLANTS` holds the old energy budget independent of map size. Strict parity made a grazed plant take about twenty minutes of wall time to return, which reads as "the food is gone" rather than as grazing, so it runs at 3x that. It is the knob to turn if the island runs hot.

### Grazing, and herds that move on
Animals take bites, one per plant per tick, so a herd crops a bush over several ticks instead of stripping it in one step. A patch grazed below 0.15 crop **stops attracting foragers**, so herds drift off cropped ground, with the bar dropped to 0.03 for a starving animal so the rule can never become a new way to starve.

Measured over 20000 steps: the stock holds at **1427 of 1440 plants** with **zero shrink events**, 1409 of them grazed at least once, and the mean standing crop settles at **0.171**, just under the perception bar. That is a grazing equilibrium rather than a stripped map or an untouched one. The herbivore population runs at **101 against a pre-change baseline of 12** on the same seed. Step cost with 2528 plants and 180 creatures: **7.6 ms**, against a 50 ms budget at 20Hz.

### Poison, rot, and being ill
- **Toxic plants cost energy**, resisted by gut and by a scavenger-tuned mouth, capped so nothing is immune. Measured at toxin 0.9: **12.15** energy off a low-gut eater, **0.81** off a high-gut one.
- **Carrion rots on its own clock.** `carrionRot()` reads the decay counter the body already ticks, so rot cannot drift out of step with when the body disappears. Yield runs **1.0 down to 0.25** across fresh to putrid for a generalist and **0.30 up to 1.45** for a scavenger, so the niche is a real trade in both directions. Measured on a body worth 90: fresh favours the generalist, putrid pays the scavenger **15x** what it pays anyone else.
- **Sickness is a status.** Eating poison or rot starts one: a steady energy drain, stamina regeneration cut by up to 55%, a sickly vignette and a HUD line. Severity and duration scale off gut and scavenger, it ticks down and clears, and it is cleared whenever the player becomes a different creature. Measured on a grazer eating a toxic mushroom: severity **0.885**, **40 s**, stamina regen **11.87 against 24.20** over one second.

### Eating, and a prompt that does not lie
`tryEat` was still deleting plants whole. It takes bites now, through the same functions the animals use.
- **Reach is real.** Food held up in a bush or on a cactus needs height. Too short and the prompt says so and names the fix: *rear up with G*. Measured: a bush at 16.8 units is refused by a 12-unit reach and taken when reared.
- **One scan feeds both the key and the panel**, ranked edible over out-of-reach over bare, so the prompt can never offer a bite that E will refuse. A grazed plant says how long until it is worth eating again.
- The line **names the food in plain words** and counts the bites left. A creature with a good enough gut and nose is warned that something **smells wrong** before it eats a poisonous one. A creature without that sense is not.
- **Old carcasses are food.** The streamed carcass field could only be searched for parts with V. E feeds on one now: rotten, so it is a gamble for a grazer and a meal for a scavenger, once per body. Measured on a body of mass 9.1 at rot 0.90: **+24 energy and 33.6 s of illness** for a generalist, **+72 and no illness** for a scavenger.

### Tested
- `test_food_eco.js` (node): plants are grazed and not removed, crop recovers, the stock holds near the cap over 20000 steps on two seeds against measured baselines, herbivores survive, toxins cost what they should, and rot pays a scavenger more and a generalist less.
- `test_food_steps.js` (browser, 34 checks): a bite reduces crop and raises energy without removing the plant, a bare plant refuses and says why, high food is refused standing and taken reared, a toxic mushroom sickens and the sickness clears, a rotten carcass sickens a generalist, the prompt names the food, and the smell warning appears only for a body that can smell.
- Replayed green: `test_core`, `test_eco`, `test_combat`, `test_fight_core`, `test_bones`, `test_inv_steps`, `test_carcass_steps`, `test_spawn_steps`, `test_fly_steps`, `test_pack_steps`, `test_br_steps`, `test_shop_steps`.

### Left on the bench this session
Finn asked for three more things mid-session that are **not in this build**: no wildlife at all in a match, 50 players instead of 24, and a hits-to-kill ceiling of 30 with most fights landing near 10. The food-plant art (real berry bushes, mushroom caps, cactus fruit in place of the ellipsoid tufts the food still renders as) was also started and not finished. All four are first up next run.

Build: `dev/3dgenesis-dev-src.tgz` unpacks to the patch pipeline. `bash build.sh` (it cds to its own folder now, so it runs wherever you unpack it) turns `g3.v3` + `src/patch_*.py` into `g3.html` (= `index.html`) and extracts `core.js` for node tests.

## 2026-09-23 (evening): Packs, carcasses, the zone, flight, and a clock running double

All of these came in from Finn while playing.

### You cannot kill your own, and now the interface says so
The rule had been in `brHitAllowed` since packs were built, but nothing in front of the player said so: you were shown an attack prompt, a red marker and a red dot for a packmate, you swung, and nothing happened. A rule the interface contradicts is a rule the player does not believe in.

One predicate, `packFriend()`, answers whose a creature is, and `canAttackTarget()` gates the affordance on **the same filter the damage goes through**, so what the interface offers and what a swing can do are one decision rather than two that can drift apart.
- **The attack key is not there** on a packmate. No F, no X, no "hits to kill". The line reads `YOUR PACK · <their name>` and, where the keys were, *you cannot hurt your own*.
- Swinging at one is refused outright and costs no stamina, rather than playing the animation into nothing.
- The marker over them is green, the lock-on reticle is green, and **on the minimap they are blue dots, a size larger, drawn over everything else**. A red dot means something you can kill; yours are never that.

### Carcasses, and searching a body
The island only had bones where something had died while you were watching, which in the opening minutes of a match is nowhere.

There is a **carcass field** now, streamed and deterministic the way the foliage is: one candidate per 1150-unit cell, presence and position and body rolled from the world seed, built only while you are near and dropped again when you leave, one skeleton a frame so walking into new ground never costs a hitch. That covers the whole 51200 x 32000 island for the cost of the four or five in sight, and every player in a match finds the same bodies in the same places.

Each one carries a real design from the same generator wild animals come from, at the world's own rarity ceiling, so what is lying there is a plausible dead animal and the parts on it are worth having. Measured on one island: bodies carrying 4, 8, 8 and 10 parts within sight of the spawn.

**V searches a body.** You get one part off it, it goes into your inventory as a copy, and the body is picked clean for good. A pale marker floats over an unsearched body in reach, and the action prompt carries `V search the body at your feet · 8 parts` even while something living is also in front of you. Fresh kills work the same way: `remainsAdd` now records the design, which is the roadmap's own hook finally wired up.

### The zone is not a circle, and it does not end in the middle
It was a perfect circle centred on the exact middle of the map, so every match ended on the same ground.
- **A seeded wobble runs around the edge** — three lobes of different periods, each turning at its own rate — so the wall bulges on one bearing and lags on another and the shape lives. The edge is now a function of bearing as well as time, and the damage, the cloud wall and the minimap all ask the same function, so they cannot disagree.
- **The final ring lands somewhere else every match**: a seeded point chosen for dry land, and the centre **drifts** from where it started to where it will finish as the wall closes, so the safe ground slides across the island and standing still is never the answer.
- The start is off-centre too.
- The minimap draws the real shape, 96 points around the true boundary, including where it is heading in 45 seconds — drift and all.

**The fog stopped killing anyone, and that was this change's fault.** Moving the ring off-centre pushed the furthest bay further out, while the close rate was a fixed 11 units a second chosen against the old, tighter start. Measured: a full fifteen-minute match ended at radius 10852 with not one player ever outside the wall. The opening hold is cut from 150 seconds to 75 (the cages last thirty; holding for a hundred and fifty left the first sixth of the match static) and the speed limit is raised to 22 units a second — still about half a walking pace, so it remains outrunnable, but it now finishes the job. Measured after: the ring closes 20219 to 4967, players are in the fog from the ten-minute mark on, and the field goes from 24 to 1.

**The fog on the minimap is weather, not a diagram.** It was a half-transparent wash with a hard white ring on top, which read as a compass circle over a map you could still see through. It is a solid grey mass now — under the cloud there is nothing to see, so the map shows nothing — and its inner boundary is a soft outline rather than a drawn ring.

### Flying followed the ground
Altitude is stored as height **above the ground**, which is the right thing to store, but nothing took the ground back out of it while you were airborne. Crossing a hill added the hill to your altitude and crossing a valley took it away: you rode the terrain instead of flying over it, and jolted up and down at every ridge. Whatever the ground does under you is now subtracted the moment it happens, so a level glide is level in the world. Measured: over ground that rose 354 units, a level glide moved 26 units — the wing's own sink — with a worst frame-to-frame step of 1 unit, where before the flight path tracked the hill exactly.

**One press, one flap.** Holding space beat the wings on a timer, which is a helicopter. A flap is an event now: press, the wings beat once, you gain height, and you glide until you want another. Lift per beat goes hard with the wing — a big wing moves a lot of air in one stroke and a small one does not — and its beat period is the refractory, so a slow heavy wing cannot be spammed. Measured: 267 units of climb per beat at wing 0.55, 849 at wing 2.17.

### The match clock ran at double speed in the creature editor
`tick()` already advances the match — it has to, since a dropped frame must not be a dropped second — and the editor branch of the loop called `brTick` **again** right after it. Every frame the builder was open cost the match two seconds: the ring closed twice as fast and the clock on the HUD ran away from you while you were looking at parts. The editor branch now does what the playing branch does — tick once, then draw the HUD.

### Tested
Four new browser suites, and the four existing ones replayed:
- `test_pack_steps.js` — a rival is attackable and the prompt carries the key; a packmate is not, the key is gone, the hits-to-kill line is gone, the swing is refused, the damage filter agrees in both directions, and leaving the pack makes them a target again.
- `test_carcass_steps.js` — bodies are there, they carry real parts, none start looted, searching offers what is on the body, taking one adds exactly one copy, a searched body is picked clean, the next one along is not, and the same seed lays out the same bodies.
- `test_fly_steps.js` — the hill does not lift you, the path is smooth frame to frame, a press lifts you and holding does not flap again, and a bigger wing lifts more per beat.
- `test_spawn_steps.js`, `test_inv_steps.js`, `test_shop_steps.js`, `test_br_steps.js` — all green, including a full fifteen-minute match.

## 2026-09-23 (fixes): Spawned in the sky, a sand floor, and a hole in the fog

Three things Finn hit playing the build, all reported in one go.

### CRITICAL: entering a match dropped you out of the sky and killed you
The fall tracker remembers the ground height under your last position (`player._gWas`) and reads a drop in it as the ground falling away beneath you. That is what makes walking off a ledge a fall rather than a teleport, and it was added last night.

Entering a match **rebuilds the island** and then puts you in a cage on the ring. The remembered height therefore belonged to a world that no longer existed, and the difference between the two islands was read as a fall from that height: you were lifted into the air by it, and the landing killed you. On a run from high ground the drop was nearly two thousand feet.

Fixed in two layers, because one of them can be forgotten and the other cannot:
- `fallResetGround()` is called wherever the player is put down somewhere new: the match spawn, `teleport()`, `becomeDesigned()`, and the start of `rebuildWorld()`.
- The tracker also notices **by itself**. It records where it took its reading, and a move further than anything could have walked in that frame is a teleport, not a step, so the ground it left behind means nothing. A relocation added later cannot bring this back.

### The death screen was lying about what killed you
Every death with no killer was headlined **THE FOG KILLED YOU**, including this one, which is why the bug read as a fog death inside a cage thirty seconds into a match. The cause was already being passed to `brOnPlayerKilled`; it just was not carried into `BR.deadChoice`. It is now, and the screen says THE FALL KILLED YOU, YOU DROWNED, YOU STARVED or THE FOG KILLED YOU as appropriate.

### The cage floor was a plate on a beach
The floor plate was exactly the cage's half-width, so it stopped at the inside of the bars and every bay had a ring of sand showing inside it and under the gate. It runs out to 1.24 times the cage now, past the bars, on a raised kerb and a shade proud of the pad so the two surfaces cannot fight over the same depth. A bay is a steel box on the ground, not a rug.

### The fog wall had a hole in it exactly where you were walking
`cloudBuild` collected the arc of the wall near enough to see, sorted it **back to front** for the alpha blend, and then kept the first `CLOUD_MAX` of that list. Sorted back to front, the first entries are the FURTHEST ones. The budget was being spent on the far side of the ring and every puff near the camera was thrown away, so you saw banks away to the left and right, clear ground straight ahead, and then you were simply inside the fog with nothing having arrived.

Nearest first, cut to the budget, and only then sorted back to front for the blend. Measured after the fix: 256 puffs at 400 units from the edge, 900 (the cap) from 2000 out, where before the near view was empty.

With the wall actually drawn where you are standing, the rest could be made to read as weather:
- **Five rows instead of three, and the first two are INSIDE the ring line** (-260 and -60). The leading edge of the bank rolls over you before the air starts hurting, so it arrives rather than being a pane you step through.
- **Four height layers** over a taller wall (520), thin at the leading edge, solid through the body, thinning again at the back and the top, so the bank has a front to watch coming.
- **Bigger puffs** (185 to 395 rather than 150 to 320) and a ring spacing that tightens from 260 to 110 units as the wall gets close to you, so it is solid where you are and cheap where you are not.
- **The cut-off follows the camera's height.** It was a flat 5200 units, which meant the wall vanished completely from the spectator camera after you died, and from anything flying. It now adds four units of sight for every unit the eye is above the ground.

### Tested
`test_spawn_steps.js`: stand on the highest ground in a free-roam world, enter a match, and check you are on the ground with no fall on the frame the match starts, still on the ground and unhurt twelve seconds later, standing in your own bay, and that a fall is reported as a fall. Plus the existing suites, all green: inventory, palette shop, and a full fifteen-minute battle royale.

## 2026-09-23 (latest): No grace period, and everyone is named

Both of these came in from Finn mid-run.

### The grace period is gone
The gates go up and the match is live. Any player can hit any other from the first second: `brHitAllowed` no longer has a phase that refuses a player-on-player strike, and the `'grace'` phase does not exist. A match is now `cage -> open -> over`.

What the grace period was actually buying was a **stocked island** to loot, and that is a different thing from a truce, so it is now its own clock. `BR_WILD_SECS` (five minutes past the gates) is when the wildlife thins out and the island empties, whoever is left standing. Walking into open combat with animals still everywhere is the point: you can spend the early match hunting parts, or you can spend it hunting people, and that is a choice rather than a schedule. The HUD says which it is: **LIVE · island empties in 212s**, then **LIVE · players only**.

Bots hunt from the gate rather than from the five-minute mark, so the first minutes have real kills in them now.

### Nameplates
Every other player in the match carries their name and the health they have left, over their head.

It is a DOM layer over the canvas, not geometry: text is the one thing the instanced renderer cannot draw. It is projected through **the same view-projection matrix the frame was rendered with**, read straight out of `vpM` after the submit, so a plate cannot drift from the body underneath it — no second camera, no interpolation, no lag of one frame.

Three rules keep it from being a wallhack:
- **Terrain occludes it.** Fourteen height samples along the line from the eye to the top of that animal's head. If the ground rises through that line, no plate. The heightmap is smooth at this scale, so fourteen lookups is enough and costs nothing next to one draw call.
- **Range.** Nothing past 1400 units (about thirty-five seconds of walking), fading out over the last 420 rather than popping.
- **The fog.** Once the closing wall is thick enough that you could not see them, the plate goes too.

A packmate's plate is green and a rival's is red, the bar turns bright red under a quarter health, and the plate shrinks and fades with distance so a crowd at range does not shout. Nothing is drawn while the world is paused or the builder is open. Nodes are pooled and only touched when the text or the width actually changes, which matters at 60 fps with 23 of them.

`br.nameWhy(slot)` answers, in one word, why a given player has no plate — `far 2180`, `fog`, `occluded`, `behind`, `offscreen`, `shown`. It exists because the first version of this silently drew nothing and there was no way to tell which rule had eaten it.

### Tested
`test_br_steps.js` now also checks: the gates open straight into open combat; a player may hit another the moment they do; the wildlife is still there to hunt at that point; the island's own emptying clock is five minutes; a rival across the island has no plate; a rival in view has one, with their name and their real health on it; and a rival behind the camera has none. It replays a full fifteen-minute match as before.

## 2026-09-23 (late): Parts are counted

The oldest wrong thing in the game is fixed. A part used to be a yes/no: the first spike you took off a corpse unlocked "spike" for good, and from then on you could cover a body in them for the price of genome space alone. That is not what taking a part off a dead animal means, and it made the whole loot economy decorative. Kill one animal with one horn and you had horns, plural, forever.

**Parts are an inventory now.** `PROG.owned` maps a part id to how many copies you hold. One kill, one copy. One purchase, one copy. You may wear at most as many as you own.

### The rule, exactly
```
stock(id)  = max(copies you own, copies the body carried when the editor opened)
spare(id)  = stock(id) - copies currently on the design
```
The second half of `stock` is the only exception, and it is there because an inherited, evolved or wild-born creature carries parts you never earned: without it the editor would refuse to let you move your own animal's legs. That grant is recomputed from the open design every time and is **never written to storage**, so it cannot be farmed into permanent stock. Everything else in the editor already asked `edCanUse(id, n)` before placing, so mirror mode, limb tips and the random-creature roller all inherited the new arithmetic for free: with one spike, mirror placement is refused and says why.

### What changed around it
- **The shop sells copies.** A card no longer flips from *buy* to *sell* the moment you own one. It reads *buy another*, keeps its price, and you can buy a fourth leg. Selling takes back one copy at the usual quarter, and refuses a copy the creature is wearing rather than the part outright, so wearing two of three still lets you sell the spare.
- **The card shows the count.** A `×N` badge on the tile, and a line under it: *you own 3 · 1 to place*, or *you own none*. The tooltip says the same, and distinguishes what you own from what the body arrived with.
- **Every kill is worth looting.** The match loot picker used to filter out parts you already had and could end with "nothing on it you did not already have". A duplicate is a second copy now, which is the whole point, so the full list is offered and the picker reads *you have 2 · +1*.
- **Old saves migrate rather than collapse.** A pre-v2 save holds a flat `unlocked` list with no counts. Each entry becomes as many copies as the largest creature you saved actually wears, and never fewer than one, so nothing you already built becomes illegal overnight. A save carrying a four-legged design comes back with four legs. (The pre-v2 free starter parts are still dropped on migration, as they were before.)
- `PROG.unlocked` is gone entirely, and the build asserts the string appears zero times in the output so nothing can quietly keep using it.

### Tested
`test_inv_steps.js`, 25 checks against the real page through the real editor: a part you own none of has no stock; one copy places once and refuses the second with the reason; the refusal is the stock and not the genome budget; a second copy lets a second one on; taking one off returns it to the shelf; four purchases give four copies; selling removes one copy and never a worn one; the counts survive a reload; and a v1 save migrates to the counts its own saved body needs and nothing more. `test_shop_steps.js` updated for the new card (buy stays, sell appears) and `test_br_steps.js` replayed a full fifteen-minute match unchanged.

## 2026-09-23 (night): The map doubles, and rivers

### The terrain is chunked and culled
The terrain was one static mesh covering the whole world, drawn in full every frame, with nothing culled including the half of the world behind you. That was 1.4M triangles at the old size and would have been 5.7M at the new one.

The vertex buffer is unchanged and still shared. Only the **index buffer** is reorganised: laid out chunk by chunk in row-major order, each chunk owning a contiguous range, so drawing a chunk is an offset into the same buffer. Nothing is duplicated, no LOD is needed, and because chunks are emitted row-major, runs of visible neighbours merge into single draw calls.

Culling is distance plus a horizontal frustum test that accounts for each chunk's own angular size, so a chunk is only dropped when the whole of it is off the side of the screen. The shadow cascade asks for a much smaller set than the camera does.

### The map is four times the area
**51200 x 32000**, doubled on each axis. Two supporting changes made it cheaper than the old map rather than more expensive:
- terrain draw distance capped at 12000 units, where the fog is already 96% opaque, so ground past it was costing triangles to render something very nearly fog-coloured
- `GRID_STEP` 24 to 28: a 17% coarser silhouette for a 27% cut in triangles, and the terrain shader adds its own fine detail on top of the mesh anyway

| | old map | doubled map |
|---|---|---|
| area | 25600 x 16000 | 51200 x 32000 |
| terrain triangles, total | 1.42M | 4.18M |
| **drawn per frame** | **1.42M (all of it)** | **1.05M (25%)** |
| draw calls | 1 | 9 to 18 |

Four times the world, a quarter less terrain drawn per frame than before.

### Rivers
Water that runs downhill in one direction, from high ground to the sea, in a channel it cut for itself with rock banks standing over it. Three stages, and the order matters:

1. **Route.** Steepest descent from a spring, looking around a ring at each step for the lowest neighbour, with a straightening bias so it meanders rather than zig-zagging and never doubles back. It stops when it reaches the sea, when it finds a bowl, or when it has stopped descending: a river that has stopped dropping has become a marsh, and without that check one wandered 41,000 units across a plain for 52 units of fall.
2. **Carve**, into the heightmap, **before the render mesh is built** (the same seam the cage pads use). A flat bed, then banks that rise **steeply** over a short run. Steep is the whole point: the terrain shader already draws anything past about 22 degrees as layered rock, so a bank cut like this **is** a rock cliff with no extra geometry and no extra draw call. Depth and width both wander along the route, so a river is a gorge in one reach and a shallow braid in the next, which is where the varying cliff heights come from. The bed is only ever cut down, never built up, so a channel cannot dam a valley.
3. **Ribbon.** A strip of quads following the route at the water's surface, each vertex carrying how far along it is, so the shader scrolls the flow **one way** and breaks it into whitewater where the bed steepens or the water drags along the banks. Three ripple layers at different rates, sky in the surface, sun glint down its length, green at the shallows and blue-green down the middle.

Seven rivers per world, 800 to 3000 units long, dropping 50 to 820 units from spring to mouth. Measured channels: bed cut 15 to 66 units below the running surface, banks standing 55 to 107 units over it.

### Fixed
- **Fall distance was wildly overstated.** The drop in the ground was being folded into your height on *every* frame, including while already airborne, so moving horizontally during a fall kept topping it up from the ground sliding by underneath. A short drop off a ledge reported as an enormous one. Only the moment you leave the ground counts now, and a drop that a walk down a slope could have produced is not a fall at all: running down a 56 degree slope now costs 0%.
- **Falls are measured in the animal's own height**, which is the only scale that means anything in a game whose bodies run from a lizard to a sauropod. Free under about two and a half times your standing height; five times that is fatal. No message either way: you felt the ground, you saw the screen flash, and your health bar moved.

## 2026-09-23 (evening): Ground rules, and a long list of things that were wrong

### The ground
- **A real slope limit.** Nothing that walks goes up anything steeper than **60 degrees**. A body built to scramble earns its way toward **80**: the limit comes from what its legs end in (claws best, then talons, then hooves) and how heavy it is, because holding yourself on a wall costs strength that goes as area while the weight goes as volume. In practice the stock body plans land between 60 and 69 degrees and 80 is reserved for something small and fully clawed that you built on purpose. It replaces the old "one block per step" rule, and it applies to every animal in the world, not just you.
- **Fall damage.** One world unit is one foot. Under 20 feet costs nothing; past that the damage goes as the height, so about ninety feet kills most bodies outright, and a heavier body fares worse for it. Ground that falls away under you is a fall now rather than a teleport: you leave the edge, gravity takes over, and what you hit at the bottom is measured from the highest point of the drop. **Water takes the fall instead**, as long as it is deep enough to take the animal rather than just wet its feet. Wild animals pay the same price on the same numbers.

### Aiming
- **A strike only lands on what you are pointed at**: within **10 degrees** of the nose, widened by however much of the view the target itself fills, so a sauropod's flank at arm's length is a broad thing to hit and a lizard across the clearing is not. It used to accept anything inside 75 degrees, which meant you could gut something standing almost beside you. A locked target has to be in front of you too. Mating and the other soft interactions keep the old generous cone: only damage is aimed.

### Standing still
- Idle animals were slowly paddling their feet. The stride had a fixed floor the gait amount never scaled away, so a creature doing nothing still swept its legs through a small cycle forever. The floor is gone and the gait clock stops when the body does, so a stopped animal holds its stance.

### The cages
- **They sit on level ground now.** The heightmap gets a flat pad cut under each bay during the world build, before the render mesh is made from it (`terrain.carveFlat`, which is also the hook rivers will use). Worst relief under a cage across all 24 bays went from 61 feet to 2.6.
- **You cannot stand outside the bars any more.** The cage is drawn rotated to face the island but the confinement was an axis-aligned box, so at 45 degrees its corners sat a long way past the walls. It clamps in the cage's own axes now.
- The bay number stands on a mast off the roof instead of floating beside the cage with nothing holding it up.

### The closing zone
- **It is weather now, not a fog setting.** A wall of large soft cloud puffs stands on the ring, three rows deep and three layers high, boiling on its own noise and creeping inward as the ring closes. Its own alpha-blended pipeline, sorted back to front, about 600 puffs. The old global distance fog still takes your sight away once you are *inside* the band, which is what makes it deadly, but from outside you now see weather coming for you instead of a flat grey pane hanging in the air.
- **The minimap shows it.** Everything under cloud is greyed out, the edge is drawn, and a dashed ring shows where it will be in 45 seconds.
- **It was too fast to escape.** Three things were wrong: it started at the corner of the map and spent the first minutes closing over open ocean nobody was standing in; the eased curve peaked at one and a half times its own average exactly when it mattered; and the damage band was 90 units wide, so brushing it was instant. Now it starts just outside the furthest bay, holds for 150 seconds, then closes at a **constant 11 units a second against a walking speed of about 40**, and the band that hurts is 420 units deep. Measured peak closing rate across a full match: 11.0.
- The final ring is a real arena rather than a pinprick.

### Matches
- **The island empties when the fight starts.** At open season the wild animals thin out over thirty seconds, leaving their bodies behind as food. What is left is players hunting players.
- The HUD says "players" rather than "alive", which is what it always counted.
- **Nothing respawns you inside a match.** Dying used to hand you a newborn of the nearest wild kin, which is how a match ended up with a creature in it that had never been drafted into it.
- **Entering a match no longer founds a deme.** Taking a creature in used to spawn six grown adults of your own design beside you. In a match it spawns exactly one: you.
- **The builder drops its shop inside a match**: no buying on the start line, no selling, and nothing saved out. It says so in the toolbar.

### Everything else
- **Esc opens a pause menu**, with a way back to the main menu. There was no way out of a game at all before this: once you were playing, you were playing. Quitting a match forfeits it and says so.
- **The world keeps running while the creature builder is open.** It used to stop dead, which made the builder a free pause button you could open mid-hunt. In exchange **nothing can touch you in there**: no hit lands, and nothing that ticks away at a body is allowed to finish you either.
- **The camera starts over the shoulder** rather than 150 units back, and the default distance is proportional to the body so a sauropod and a lizard are framed the same way.

## 2026-09-23 (later still): The cages are metal

The cages were being drawn through the foliage shader's **bark** path, which was putting tree furrows, moss and lichen on steel. That is why they read as flat blue-grey slabs. They have their own material now.

- **Dark grey steel**: plate with a slow mill grain, fine pitting, and a tight highlight on whatever is still clean. The base greys are warmed slightly, because the scene's ambient light is strongly blue and a neutral grey reads as blue steel once it is in shade.
- **Rust** blooms out of the joints and off the faces that hold water (top faces go first), and runs downward in streaks below wherever it has taken hold. Rust kills the highlight, so the weathered parts go matte while the bare metal still catches the sun.
- **The lamps and the bay numerals** get a lit material instead, so they are not weathered with the rest of the cage and they carry at night.
- The material slot in the foliage vertex format now reads: 1 two-sided leaf, 0 bark, -1 rock, -2 weathered steel, -3 lit glass.

Two tuning passes were needed. The first was far too dark and the grain ran at 1.7 cycles per world unit vertically, which put about eighty bands up a 46-unit bar and read as diagonal hatching on anything thin. The grain is slow now and the bump off it is gentle.

## 2026-09-23 (later still): Buying moved onto the part cards

The shop was a separate panel, which was wrong: it made buying a place you go rather than something you do while you are looking at the part. It is gone.

- Every card in the right-hand palette now carries its own price and its own button. A locked card reads its genome cost, its tier (colour-coded: weak, solid, super, ultra, alpha) and a **buy** button with the price. Click it and the part unlocks in place: the lock badge comes off the tile, the card turns into a **sell** and you can drag it onto your creature immediately.
- A price you cannot afford is dimmed and refuses, rather than letting you click into a failure.
- Selling takes two clicks on the same button: the first arms it and it reads "sell for 25?", the second does it. A part is too dear to lose to a slip. A part your creature is currently wearing still cannot be sold at all.
- The balance lives in the builder toolbar where the shop button used to be. The bundle prices (100 for $2, 500 for $5, 10000 for $30) are its tooltip, with the note that they need an account server this build does not have.
- `test_shop_steps.js`: eleven checks that the separate panel is gone, that buying and selling work off the card, that the balance follows, that the sell confirm holds, that a worn part is protected and that an unaffordable card refuses.

## 2026-09-23 (later): Match settings are locked

A match is a fair fight or it is nothing, so none of the world is the player's to set any more.

### Locked
- **The island itself.** Entering a match is now three steps, not one: `join()` hands down the seed and the ruleset, the island is rebuilt from exactly those, and only then does the match begin. Everyone in a match is on the same island because the island is built from the match's own seed rather than from whatever world the player happened to be standing in.
- **Every control.** While a match runs, the world panel's sliders and checkboxes do not reach `world.params`: `panelOpts()` ignores the DOM entirely and answers with the match ruleset however the controls have been dragged, `panelFlags()` and `panelWarmup()` do the same, and the controls are put back where the match says they are, disabled and greyed, with a banner across the top of the panel saying why. The panel is re-asserted once a second so nothing that reaches in and edits a control can leave the display disagreeing with the match.
- **The seed box and its lock**, which now have no say in what island a match builds.
- **Reset World**, from the HUD button and from the panel. Both refuse and say so instead of doing nothing.
- **The clock.** Time scale, day length, season length and the pre-run are all fixed. One match is about one in-game day.
- **Climate drift is off on purpose.** It re-classifies biomes as the match runs, so a player whose ground shifted under them would gain or lose cover through no choice of their own.

The ruleset lives in one frozen object, `BR_RULES`, and is delivered through `join()` — which is `brNetLocal()` today and a server tomorrow. The locked values: 200 founders, 30 biological seconds of pre-run so the island is not empty when the gates open, mutation 0.8, time scale 0.01, day 10, year 5, plant rate 20, plant energy 10, carrion 50 / 0.5, founder diversity 0.8, patchiness 0.5, terrain and tree density 1, and brains / sexual / day-night / seasons / toxins on with climate off.

To be plain about what this is: it stops the controls being a cheat, and it makes every player's island the same island. It is not anti-cheat against someone editing `world.params` from a console — nothing running in the player's own browser can be. That is a reason the real thing needs a server, and the roadmap says so.

### Also
- The free-roam goal box and the Reset World button are hidden during a match. Neither belongs in one.
- Bots that only ever ran for the middle of the ring never met, so the last minutes were the fog killing everyone rather than a fight. A bot whose ring has closed below 1400 units, or who has a rival within 260, stops running and starts hunting. A match now finishes with real player-versus-player kills in the feed (a sample run: 6 of 19 deaths by a player, the winner with 4 kills).
- The match HUD is drawn from the match tick rather than the frame loop, so it can never fall behind the state it reports — including the death offer, which has to appear the instant you are killed.
- The end condition is checked every tick, not only when a kill reports one, so a side that quietly stops existing still ends the match.
- `test_br_steps.js` is now a real suite: 17 checks covering the lock (no slider, flag, seed or reset reaches a running match), who may hit whom in a cage, the gates, the closing zone, the recorded result, and the panel coming back when the match is over. It plays a full fifteen-minute match through the simulation clock.

## 2026-09-23: Battle royale, the bones economy, and the underwater world

### Battle royale (new game mode: BATTLE ROYALE on the main menu)
- A fifteen-minute match. Twenty-four creatures, one island, a fog that closes in until one side is left.
- **The cages.** Every player starts locked in their own steel cage on a ring around the island: plate floor, barred walls and roof, a gate on the face turned toward the middle, and a lit bay number on a plate above it so "bay 7" means something you can find. You walk around inside for thirty seconds. The ring is laid out at the map edge and then walked inward until each bay finds dry land, so the cages stand on the coast facing in rather than out in the sea. Nothing can touch you in the cage and nothing can wear you down: no hunger, no drowning, no wildlife.
- **Gates open** together and slide up out of their frames.
- **Grace, five minutes.** No player can hurt another. The island's own animals are fair game both ways, and killing them is how you build your beast before the fighting starts.
- **Open season.** Everyone can kill everyone.
- **Killing a player** feeds you (you grow through the same path any meal uses), pays 5 bones, and offers you one part off their body to keep.
- **The loser chooses**: watch the rest of the match, or come back as their killer's young. A pack cannot hurt itself, fights together, and wins together. A dead leader frees its pack.
- **The fog** starts ninety seconds in and closes on an ease so the last minutes shut fast. Inside it visibility collapses to a couple of body lengths and the world washes grey-green, and it takes 7 health a second at its worst. The HUD tells you how far you are from safety, or how far into the fog you already are.
- **Winning** is recorded (wins and best kill count), and pays 25 bones on top of what you took.
- If time runs out with more than one side standing, the match is decided on kills, then on how much fight is left. It never ends in a draw.

### What is real and what is not
This build has no game server, and it is one static HTML file. The match, the zone, the grace period, the looting, the packs and the win are all real and run to the end; the other twenty-three are bots on your machine, and nothing talks to the network. Every rule is settled through one transport interface (`BR_NET`, with `brNetLocal()` as the only implementation today), so pointing it at a real server is a swap rather than a rewrite. `brHitAllowed()` is the single place that decides who may hit whom, so a client and a server cannot disagree about it. The online menu page says all of this plainly rather than implying a lobby that is not there.

### Bones: the parts economy
- **5 bones for every kill**, kept with the rest of your progress.
- **Five tiers price every part**: weak 50, solid 100, super 200, ultra 500, alpha 2000. All 52 parts are in the table, and the build fails loudly if one is ever missing rather than quietly pricing it wrong.
- **A shop in the builder** (the "shop" button in the toolbar): every part in the game grouped by tier, with its price, what it does, and a buy button. A part you buy is unlocked exactly as a kill would unlock it.
- **Selling** gives back a quarter, rounded down. A part your creature is currently wearing cannot be sold out from under it.
- **Locked palette cards now show their price**, and the tooltip tells you both ways to get one.
- **Rarity in the wild matches the tier.** Weak parts are on half the animals you meet; an alpha part is born on roughly one hunter in fifteen hundred. Wild bodies now draw from the whole ladder, so a founder can be born with a crushing jaw, a sail-back snout, antlers or a shell.
- **Per-world rarity roll.** Every island rolls a ceiling from its seed: about 18% of worlds grow no ultra or alpha parts at all, about 64% grow no alpha, and about 18% can grow anything. The same seed is always the same world. The ceiling governs what the roll may add to a body, not the parts a body plan is made of: a pterosaur has wings in every world.
- **Bundles are listed with their prices and cannot be bought.** Taking money needs a payment processor and an account server to credit, and a button that handed out bones for free would wreck the economy the rest of the system exists to keep honest. The shop says so.

### The underwater world
- **No tree grows in water any more.** The biome map calls the tidal fringe "shallow" and "marsh", so biome alone had been planting mangroves and palms out in the sea; the height against the water line is what decides now.
- **Underwater light.** Only the stretch of the view ray that actually runs through water absorbs, so a fish seen from the bank greens out with its own depth rather than with how far off it is, and air fog is charged only for the dry stretch. Red is gone in about sixty units, blue carries two hundred and fifty, which is why everything deep goes blue. Sunlight is spent on the way down, so the floor darkens with depth.
- **Caustics**: two crossing ripple fields focus the sun into moving filaments on anything facing up, strongest just under the surface.
- **The surface from below** is a mirror everywhere except a bright disc straight overhead. That disc is Snell's window, and the ripples make its edge crawl. The sun burns through it, smeared.
- **Sea-floor flora**, placed by depth and by how warm the water is: giant kelp on the cold shelf (shrunk to fit the water column so a bed never pokes out of the sea), sea grass and weed in the wash, coral heads with staghorn thickets and anemones on warm reefs, sea fans standing on edge, and shells, urchins and rubble on the sand.
- **Shoals of small fish** drift through the water column, each fish wiggling on its own phase, and **a diver trails bubbles** — far more of them if you are holding your breath than if you have gills.

### Tests and tooling
- `test_bones.js`: the tier table must cover exactly the parts the simulation knows, prices must be the published ladder, and the per-world ceiling must actually gate what the wild rolls. Nineteen assertions, all from the shipping source rather than a copy.
- `test_br_steps.js`: a harness script that plays a whole match through the sim clock and prints the zone, the survivor count and the result each minute.
- The headless harness could build a creature but had no way to leave the main menu, so every automated screenshot was of the menu. `__G3D.beginPlay()` fixes that, and `__G3D.bones` / `__G3D.br` expose the two new systems to it.
- Google Fonts is blocked in the harness, which made every screenshot wait thirty seconds for it. The harness now refuses that request outright.
- The match clock advances with the simulation step, not the frame loop, so a dropped frame is not a dropped second.

### Fixed
- The menu track now loops, fades out over two seconds when you leave the menu, and stops on NEW GAME as well. Its autoplay had lost its fallback, so it would never start until something else happened to trigger it; it tries on load and again on the first click or key.
- The start-population slider and its number box disagreed (200 against 100). The mutation rate, pre-run, season length, founder diversity and population defaults set by hand in `index.html` are now in the build, so a rebuild stops reverting them. The season-length slider's floor was above its own default value, which the browser silently clamped away.

## 2026-09-22 (night): Title and menu background

- GENESIS is now the title of the screen, much larger than the menu heading under it: cast-concrete lettering built from fractal noise lit from the upper left, with grit, a displaced rough edge, and a short extrusion so it stands slightly off the page.
- Three claw gouges rake diagonally across the word. They are cut out of the stone face, so the dark interior shows through, with torn light edges along the upper side of each cut.
- Menu background rebuilt as a misty night swamp: a pale band of lit fog at eye level behind the trees, every rank of silhouettes washed in that fog so the far ones dissolve into it, five drifting ribbons of ground mist, a black foreground and a heavy vignette.

## 2026-09-22 (night): Diving, and finer parts

### Diving
- The surface is no longer a ceiling. A swimmer in deep water goes where it looks: pitch the nose down and hold forward to descend, space to rise, ctrl (or C) to sink straight down. Depth and the controls are on the HUD.
- A gill breather is neutrally buoyant and stays wherever you leave it, all the way to the sea floor. An air breather is buoyant, drifts back up when you stop working at it, and starts drowning once its head goes under, so a diving lungfish has to time its trips.
- Fixed with it: a swimmer floating at the surface of deep water was counted as submerged and drowned. Only an animal that has actually dived counts as under now.

### Jaws
- Fixed the muzzle: the flesh was built from six ellipsoids spaced further apart than they were wide, so a long jaw came out as a string of separate lumps with teeth between them. Segment count now follows jaw length and each blob overlaps its neighbour, so the snout is one smooth piece.
- Teeth are smaller and there are more of them, capped against the depth of the jaw they sit in, so they read as a tooth row instead of a handful of white spikes. Gums are a thin line along the jaw rather than a bead per tooth.

### Fins, sails and wings
- The webbing between fin rays is built from twice as many, thinner sections, tapering and darkening toward the edge, so a sail or a fluke reads as a sheet instead of a row of beads. Rays are slimmer and taper to the edge.

## 2026-09-22 (later still): Sea life, breathing, and fixes

### Breathing
- Gills and lungs decide where you can be. A body with no legs and real fins is a water animal: it breathes water, never drowns however deep it goes, and suffocates if it ends up on dry land. Anything that walks breathes air and drowns with its head under water, as before.
- Two parts change that: **gills** (breathe water) and **lungs** (breathe air), both in the details tab. A body with both is amphibious and safe everywhere. The builder shows a "breathes" line (water only / air only / water and air) and warns you when you are water only.
- You can always take the organ your kill breathed with: lungs from anything that breathed air, gills from anything that breathed water. That is how a shark gets ashore.
- This is a shape test, not a strength test, so a calf is the same kind of animal as its mother and never suffocates while it is still growing. Water animals also swim at any age.
- Water species now start, are born and are founded in the water. Wild ones will not wander onto land; you can, and you will suffocate for it, which is your choice to make.
- Death screen says "You suffocated out of water" when that is what killed you.

### Sea start
- Choosing a sea shape (fish, shark, whale, plesiosaur) drops you in deep water, with your kin around you, and you can swim from the first second.

### Body shape menu
- Split into **land**, **air** and **sea**, each with a line saying what it means. Every shape has its name on the tile.
- The shapes are flat grey silhouettes now, with no parts, colours, skin or eyes: you pick the skeleton, then paint it.

### Jaws
- Fixed: the jaws were built on their own axes and came out pointing straight up out of the face. They point forward now, and the mandible hinges up and down instead of rotating around the snout.
- All the jaws are bigger, so a crushing jaw reads as a real skull on a theropod's head rather than a row of teeth sunk into it.

### Menu and palette
- NEW GAME (and CONTINUE, and loading a saved creature) now builds a brand new island with a new seed instead of dropping you into the world that was generated behind the menu.
- The name field is free text; the forced "-saurus" is gone, though the roll button still favours it.
- The icon strip above the palette is gone: the category bar with its arrows is the only category control, and the toolbar no longer runs underneath the palette.
- Locked parts are readable (small lock badge in the corner) instead of blacked out, and jaw icons show their muzzle instead of floating teeth.

## 2026-09-22 (late): Prehistoric pass

Twelve new body plans, real jaws, a flight model that actually flies, rearing up, a main menu, and a rebuilt builder palette.

### Body plans
- Twelve prehistoric starting shapes on top of the originals: theropod, raptor, sauropod, ceratopsian, ankylosaur, stegosaur, hadrosaur, sail-back (spinosaur), pterosaur, shark, whale and plesiosaur. Shark and whale are water-only: no legs, so they cannot leave the water until they take legs from something they kill.
- Founder species now draw from all 22 plans, so a new world has a much wider spread of animals in it.

### Jaws
- One jaw builder drives every mouth: an upper jaw that is part of the muzzle, a mandible hinged at the back that actually swings open, teeth that vary along the jaw line and interlock when shut, gums, a tongue and a dark throat so an open mouth reads as a hole.
- Four new mouths: crushing jaw (deep, bone-cracking, heaviest damage), fishing jaw (long crocodile snout, extra reach, good in water), short muzzle (fast strong bites up close) and duck bill (broad beak with grinding rows, strips plants fast). The old jaws and fanged maw were rebuilt on the same builder.
- Brow knobs over the eyes on the deep jaws, croc ridges along the snout on the fishing jaw, nostrils on top.

### Flight
- Rebuilt as a glider instead of a lift. You fly where you look: diving trades height for speed, pulling up trades speed back for height, so you can swoop. Space flaps (costs stamina, buys thrust and lift; big wings beat slower and carry more). A and D bank, and the bank is what turns you. W tucks, S flares. Thermals rise over open sunlit ground and steep slopes so a big flier can circle up without a wingbeat. Too slow and you stall: the nose drops until speed comes back. Walking off a drop puts you in the air. Landing fast staggers you.
- Altitude, airspeed, stall and lift are on the HUD.

### Rearing up
- Hold G to rise onto the hind legs: nearly double reach so you can strip the canopy, and you are taller in a fight. Costs stamina to hold, and you can barely move while up there. Needs two or more hind legs and no wings in use.

### Main menu
- A proper front end: CONTINUE (your last creature), NEW GAME, LOAD CREATURE, CONTROLS, OPTIONS, ABOUT. The highlighted line gets a torn brush mark behind it, its name set large above, and a line of description at the bottom. Mouse or arrow keys plus Enter.
- CONTROLS lists every key. OPTIONS has graphics quality, auto resolution, sound and wiping saved progress.

### Naming
- You name your species in the builder, or hit "roll" for one. The name is free text, not a forced -saurus; rolled names often end in -saurus anyway. It shows on your HUD and on your saved creatures.

### Builder palette
- The right-hand palette is one category at a time with a bar across the top: category name, left and right arrows to move between them, and a line saying what the category is for. The old icon strip is gone.
- Categories are in build order: body shape, proportions, colour and pattern, skin, limbs, feet, eyes, mouths and jaws, horns and plates, wings, fins, tails, extras.
- Every part is a card with a rendered image, its name, what it is for and what it costs in genome space. Locked parts stay readable (a small lock badge in the corner) instead of being blacked out.
- The body-shape tab shows plain grey outlines of each shape instead of finished coloured creatures, so you pick the shape and then paint it.
- The palm tree that rendered in front of the creature in the builder is gone, and the toolbar no longer runs underneath the palette.

## 2026-09-22: Random creature button, darker nights
- "random" button in the creature builder toolbar: rolls a whole random creature (body plan, parts, limbs, colours) from what you are allowed to build with, which is every unlocked part plus as many of each locked part as your current body already has. Mirrored pairs stay together, it always keeps a mouth, and it trims itself to fit your genome space. Undo restores the previous design.
- Night is much darker: moonlight cut to about a third of what it was, so nights are genuinely dark and the moon is a light in the sky rather than a floodlight.

## 2026-09-22 (night): World detail pass
- Sky: sun disk, moon with phases (8-day cycle), stars, drifting clouds, sunrise/sunset colours. The sun rises east, arcs over, sets west; noon height follows the season (about 88 deg in summer, 32 deg in winter). Shadows and light follow the sun by day and the moon by night.
- Terrain: ~2000 boulders (solid, clustered, mostly small with a few huge), pebbles, stones and driftwood as ground cover; rock material with strata, cracks and lichen; bare rock on steep slopes; wet sand at the waterline; large-scale colour variation; softer, less neon ground colours.
- Water: depth-aware (clear turquoise shallows you can see the sand through, deep blue further out), sun glint, waves breaking as foam on the shore. Lakes in cold country render as ice (visual only; not yet walkable).
- Exposure lowered, filmic (ACES) tone curve. AO speckle removed (ordered sampling plus blur).
- Ultra: much further draw distance (plants 18000, trees full detail to 2300, far plane 26000); high raised a little.
- Time scale default 0.01.
- First person: eye moved to the top of the head with the head and neck folded away on skinned bodies. UNTESTED in the headless browser; may still need work.
- V (detached camera): your body is left to its own slow wandering and grazing instead of moving with the camera.
- Newborn grace: for your first 45 s your own species will not attack you (fixes being killed over and over by a huge parent on respawn).
- Not done: rivers, frozen lakes you can walk on, tree impostors, TAA.

## 2026-09-22 (evening): Render 2
- Sun shadows: cascaded shadow map (near cascade 380 units for creatures and undergrowth, far cascade 2400 units for trees and hills), 3x3 soft filtering, texel-snapped so they do not shimmer. Trees, grass, creatures, skeletons and terrain cast; everything lit receives. Medium: 1 cascade at 1024. High: 2 at 2048. Ultra: 2 at 4096. Low: off.
- Post-processing: half-res ambient occlusion from depth, screen-space sun shafts, quarter-res bloom, filmic grade (highlight roll-off, contrast, saturation, split toning), vignette, dithering.
- Render scale: the scene renders below screen resolution and is upscaled with contrast-adaptive sharpening (low 65%, medium 80%, high 90%, ultra 115% supersampled). The canvas itself is always native resolution so the HUD stays crisp.
- Auto resolution (on by default, "auto res" toggle): lowers the render scale down to 60% when frames take over ~21 ms, raises it again when there is headroom. The graphics bar shows fps and the current render scale.
- View-frustum culling: creatures, food plants, skeletons, trees and ground cover outside the view are not drawn at all (trees within 380 units stay so they can still cast shadows into view).
- Bark fixed: the per-pixel sparkle came from projecting world position onto the normal's tangent (huge coordinate swings per pixel); bark now uses triplanar projection, with fine grain only up close.
- Not done: temporal anti-aliasing (needs motion vectors through every pipeline), terrain chunk culling, tree impostors, moving creature building to the GPU.

## 2026-09-22 (later): Feedback pass
- Removed the grey/black ground rings around creatures (attack arc, threat ring, telegraph ring). No on-screen proximity indicator; danger is heard, not drawn.
- 3D audio: every animal sound goes through an HRTF panner at its real position, with the listener following your head, so on headphones sounds come from behind, above and around you. Distance loudness and muffling are unchanged.
- Footsteps for the 8 nearest walking animals: step rate from leg length and speed, weight from mass (big animals thud), surface from the ground (leaf litter, grass, mud, rock, snow, sand, water). Wingbeats for fliers. Stalking meat eaters growl more often as they close in.
- Graphics selector bottom right (low / medium / high / ultra, remembered): render resolution, ground cover radius, food plant and tree LOD distances, creature detail distance, fog, instance budget. Fixed distant trees vanishing when the foliage buffer overflowed (it now fills partially, trees first; buffer raised to 200k).
- Growing by eating: every kill you feed on and every carcass you eat makes you bigger. Juveniles get a growth spurt; adults bulk past their genetic size up to x1.5 (stats and upkeep scale with it). Size shown in the HUD.
- Eyes rebuilt: bulging eyeball on a raised orbital mound, layered iris, round or slit pupil, catch-lights, upper and lower lids in skin colour that close on a blink. Spider eye clusters are glossy black domes standing clear of the skin.
- First person shows your own body: the camera sits just in front of your head (your own eyes and mouth hidden), so looking down shows your legs and arms.
- Trees: procedural bark in the foliage shader (furrowed plates, grain, bump-lit ridges, moss on shaded and upward faces, lichen), thick sinuous rounded buttress roots instead of flat fins, trunk base flare, more trunk sides.
- Blood fixed: splats were drawn standing on edge (the dark "disc"). Now flat, small, irregular pools and drips.
- Skeletons: every animal that dies near you leaves a skeleton built from its real body plan (spine, ribs across its real width, skull, neck, tail, every limb splayed from its socket). Rotting flesh shrinks away over 45 s, bones yellow, then sink into the soil over 7 minutes.
- Animals commit to a heading for 1 to 1.8 s and turn at a limited rate (faster when fleeing, fighting or hunting): no more jittery walking.
- Drowning: animals without fins will not wade in over their heads. If your head is under water you lose 12% of your health per second, with a red and blue vignette, a warning and bubbling sounds.
- "YOU DIED" screen with the cause on every death.
- Start screen: no box; text drifts down over an animated dark rainforest silhouette with mist, and pairs of eyes open in the undergrowth, blink and follow your cursor. It lists all keys and moves; clicking opens the creature builder, and finishing the design starts the game.
- Always-visible Reset World button (click twice to confirm; keeps your last design).
- Defaults: population 100, time scale 0.05, plant regrowth 20, plant energy 10, day length 10, season length 10, corpse lifespan 50, yield 0.5, patchiness 0.5, founder diversity 0.6, start plants 0. Control panel hidden until opened (Tab or the controls button).
- Fixes: the predator warning no longer hides under the goal box; the goal box is hidden in the builder; builder camera can no longer go below the ground; both wings beat together; bodies stand on their shortest leg so every foot touches the ground (no dangling legs); prey you wounded that bleeds out within 25 s counts as your kill; "energy" is now "health".

## 2026-09-22: Combat v2
- Stamina bar (HUD). Sprint, dodge, guard and every attack cost stamina; regen slows when starving. Empty = exhausted until 30 (no sprint/dodge, attacks do half damage).
- Telegraphed incoming strikes: an animal attacking you winds up (0.40 to 0.85 s, longer for heavier animals). A marker over it counts down, red then yellow in the parry window. The hit only lands if you are still in reach.
- Q hold = guard (front 160 deg): 30% damage through, costs stamina; out of stamina = guard break + stagger. Raise guard in the last 0.22 s = PARRY: no damage, attacker staggered 1.8 s (your hits on it crit x1.6).
- Z or tap Shift = dodge roll with 0.28 s invulnerability (hold Shift still sprints).
- T / middle click = lock-on: camera tracks target, swings go to it, 3D health bar + reticle over it.
- Your swings land on the animation's strike frame, not on the key press. Hits from behind x1.35. Heavy hits interrupt the target's wind-up.
- Knockback (mass ratio based, stops at cliffs/trees) and stagger on heavy hits, both directions.
- Bleeding wounds (core sim, all creatures): a hit > 10% of storage bleeds ~35% extra over time, stacks, capped. Wounded animals leave a blood drip trail you can track (45 s).
- Blood spray on every visible hit, pooled splats that dry and fade. Red damage vignette, pulsing while you bleed.
- Attack circle replaced by a ground arc showing the exact hit cone and the selected move's reach, filling back in as the attack recovers.
- Fix: the body you steer no longer auto-attacks through its brain (free invisible damage).
- Tests: `node test_fight_core.js` (11 checks), `node play.js s_fight.js` (deterministic stepped browser test of every mechanic). `s_stress.js` updated for delayed swings.

## Next up (candidates)
Hunger/thirst split + drinking, weather (rain/fog/lightning), world save between sessions, predator stalking AI, wounds visible on skin, gills as a part, swimming with a breath meter.
