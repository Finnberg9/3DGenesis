# 3DGenesis dev log

Build: `dev/3dgenesis-dev-src.tgz` unpacks to the patch pipeline. `bash build.sh` (edit its `cd` to your folder) turns `g3.v3` + `src/patch_*.py` into `g3.html` (= `index.html`) and extracts `core.js` for node tests.

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
