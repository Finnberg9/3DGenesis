# 3DGenesis dev log

Build: `dev/3dgenesis-dev-src.tgz` unpacks to the patch pipeline. `bash build.sh` (edit its `cd` to your folder) turns `g3.v3` + `src/patch_*.py` into `g3.html` (= `index.html`) and extracts `core.js` for node tests.

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
Hunger/thirst split + drinking, weather (rain/fog/lightning), graphics quality menu + resolution slider, world save between sessions, predator stalking AI, wounds visible on skin.
