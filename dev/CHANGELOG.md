# 3DGenesis dev log

Build: `dev/3dgenesis-dev-src.tgz` unpacks to the patch pipeline. `bash build.sh` (edit its `cd` to your folder) turns `g3.v3` + `src/patch_*.py` into `g3.html` (= `index.html`) and extracts `core.js` for node tests.

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
