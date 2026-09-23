# 3DGenesis roadmap: future game modes (Finn's vision, noted 2026-09-22)

Recorded so every daily run knows where the game is headed and keeps the code compatible with it.

## STATUS (2026-09-23)
- **Match spawn fixed** (2026-09-23, fixes). Entering a match dropped the player out of the sky and the landing killed them: the fall tracker was holding the ground height from the world the match had just replaced. Any discontinuous move of the player now clears it, and the tracker detects a teleport on its own so a future one cannot reintroduce it.
- **The cloud wall is solid where you stand** (2026-09-23, fixes). Its puff budget was being spent on the far side of the ring, leaving a hole directly ahead of the player. Nearest-first now, with the bank's leading rows inside the ring line so it rolls over you.
- **No grace period** (2026-09-23, latest). The gates open straight into open combat. The island's wildlife now thins out on its own five-minute clock (`BR_WILD_SECS`) rather than as the end of a truce, so the early match is a choice between hunting parts and hunting people.
- **Nameplates: built** (2026-09-23, latest). Name plus health bar over every other player, projected through the frame's own `vpM`, with terrain occlusion, a 1400-unit range and a fog cut-off so it is not a wallhack. `br.nameWhy(slot)` says why any given plate is missing. Still to do: the plate should carry the pack badge and the kill count once packs are used in earnest, and a server will have to decide what health a client is allowed to be told about a rival at all.
- **Parts inventory: built** (2026-09-23, late). Parts are counted, not flagged: `PROG.owned` holds copies, one per kill or purchase, and `stockOf`/`spareOf` in the editor cap what can be worn. Closes Finn's standing complaint that one spike unlocked unlimited spikes. Still to do: a visible bag/inventory screen separate from the palette, and dropping a part you do not want in the world for someone else to pick up.
- **Battle royale: built, single-machine.** The match runs end to end against bots: cages with bay numbers, gates, the five-minute grace, open combat, looting a kill's parts, the pack-or-spectate choice, the closing fog with its kill band, and a recorded win. Every world parameter is locked for the duration and the island is built from the match seed, so the fight is the same fight for everyone in it. See `src/br.js` and `src/brmesh.js`.
- **Bones: built.** Tiers, prices, sell-back, the shop in the builder, and tier-matched rarity in the wild with a per-world ceiling. See `src/bones.js`.
- **Map: doubled to 51200 x 32000** (2026-09-23), which needed the terrain chunked and culled first. Drawing four times the world now costs a quarter less per frame than the old one did.
- **Rivers: built** (2026-09-23). Routed by steepest descent, carved into the heightmap before the render mesh, with steep banks that the terrain shader renders as rock cliffs for free, and a flowing ribbon surface. Still to do: the simulation does not know about them yet (`riverSurfaceAt()` is the hook), so you cannot swim or drown in one; waterfalls where the bed drops sharply; and the surface is a little washed out where the foam term piles up.
- **Water world: mostly built.** No trees in water, underwater absorption and caustics, Snell's window, kelp/coral/sea-fan/rubble beds, fish shoals and diver bubbles. Still missing: underwater caves (needs real geometry beyond the heightmap), and sand ripples.

### What the online mode still needs (the only part that cannot be built here)
This repo is one static HTML file with no server, so the twenty-three opponents are bots. Everything a server would decide already goes through one seam:
- `BR_NET` — the transport contract, with `brNetLocal()` as the only implementation. `join / ready / send / poll / leave`, plain JSON events. A socket implementation is a drop-in.
- `brHitAllowed(victim, attacker)` — the single place that decides who may hit whom (cages, packs, self). Client and server call the same function, so they cannot disagree.
- `BR_RULES` — the frozen ruleset a match runs on, handed down by `join()`. A server replaces it per match; nothing else in the page may write it.
- `BONES.remote` — set it to an object with `credit/debit` and the local balance is never touched again. Until then balances live in the player's own browser and are trivially editable, which is exactly why a real economy needs the server.
### Measured, 2026-09-23, so nobody has to guess again
`core.js` runs in Node unmodified. A full 15-minute match simulated server-side on one core of this container:

| what the server simulates | ms/tick | matches per core (60% load) |
|---|---|---|
| full island, 200 creatures, 20Hz | 11.53 | 2 |
| full island, 200 creatures, 10Hz | 19.93 | 3 |
| players only, 24 creatures, 20Hz | 0.83 | 36 |
| players only, 24 creatures, 10Hz | 1.28 | 46 |

World generation is about 1.7 s. The 20x gap is the whole hosting-cost question, and the answer is **players-only authority**: the server owns the 24 players and the match rules, wildlife stays client-local and cosmetic. The island is a pure function of `seed + BR_RULES`, so terrain costs no bandwidth at all, and nobody can exploit a fish being two metres out of place in a deathmatch.

Rough shape when it is time: Node plus `ws` (or Colyseus, whose room-per-match model fits almost exactly) on a ~$5/month VPS. The genuinely hard part is melee lag compensation, not the networking. Accounts and bones need a database or the balance stays editable.

**Decision 2026-09-23: not yet.** Finn is keeping the daily runs on the game itself. The seam stays where it is.

One thing the seam cannot do locally: the parameter lock stops the controls being a cheat, but nothing running in a player's own browser can stop that player editing `world.params` from a console. Only a server that runs the simulation itself can, which is the strongest argument for making the server authoritative over the sim rather than just over matchmaking.
Still to decide and build elsewhere: the game server itself, matchmaking, accounts, and the public win/leaderboard record. Damage is already deterministic and the core sim is DOM-free, so a server can run it headlessly.

## Two modes from the main menu
1. **Free roam** (single player): the current game. Live on the island, evolve, mate, die and continue your line.
2. **Online**: a Fortnite-style battle royale. Matches last about 15 minutes, and a new one starts right after.

## Online match flow
1. **Cages (pre-match lobby).** Every player starts in their own dim-lit, enclosed metal cage. The cages sit on a huge ring around the entire map. Each cage has a bay number on top and a giant gate. You can walk around inside your cage until the match starts.
2. **Gates open.** Everyone leaves their cage at once.
3. **Open combat, immediately.** There is no grace period: from the moment the gates open, players can attack and kill each other as well as the island's creatures. (Superseded the original five-minute truce, 2026-09-23, at Finn's call.)
4. **The island empties** five minutes after the gates. Until then it is full of creatures to kill and skeletons to loot for parts (legs, horns, jaws, etc.), so the early match is a choice between building your beast and going straight at people.
5. **Killing a player:**
   - You eat them and grow slightly in size.
   - You can equip parts from their inventory.
   - The loser chooses to either **spectate** you, or **join your pack** as a baby descendant of yours.
   - Pack members cannot hurt each other and must fight together to win.
6. **The fog (shrinking zone).** A thick fog slowly creeps in from the edges of the island toward the centre.
   - You can walk into it, but deep inside it starts killing you.
   - Visibility in the fog is only about 2 feet.
   - This shrinks the playable map over time and forces fights.
7. **Victory.** The last player (or last pack) alive wins the round. The win is recorded on their public account, together with their winning creature.
8. The next match begins.

## Implications to keep in mind now
- **Parts as inventory:** loot and equipped body parts must be data that can be sent over the network. Designs are already plain JSON (`genome.design`); keep it that way. The counted inventory (`PROG.owned`, a plain id-to-count object) is the thing a server has to own, alongside the bones balance: it is as editable from the console as the balance is, and for the same reason.
- **Skeleton remains as loot:** each remains entry should carry its creature's design parts so they can be picked up (the `REMAINS` list in `src/remains.js` is the hook).
- **Rules the server decides:** damage must stay deterministic, with the server settling hits so players can't cheat.
  - The core sim is already DOM-free.
  - Combat goes through `world.hitFilter` (`src/fight.js`), which is the natural place for the cage and pack rules.
- **Fog zone:** needs a zone radius over time, a damage-per-second band, and very dense fog rendering (this can reuse the drowning damage path and the fog uniforms).
- **Online backend (to be decided):** game server, matchmaking, and accounts/leaderboard (wins and winning creatures).

## Water world (noted 2026-09-22, next up for the daily runs)
- **No trees in water.** Trees (mangroves and palms included) must never grow in water.
  - Today the static forest can place mangroves in `marsh`/`shallow`, and some trees end up standing in the sea.
  - Fix in `buildForest`: skip any cell below water level.
- **Pure swimmers.** There should almost always be at least one fully aquatic species that cannot go on land at all and lives only in the water.
  - Seed aquatic founders (fish body plan, fins, gills, no legs).
  - Land access comes from anatomy, not a species flag. A body with no legs cannot leave the water. But a fish that kills a shore animal and takes its legs (or evolves them) can crawl out onto land.
  - Legless fish stay water-bound; the move from water to land is earned through parts.
  - The player gets the same rule: a fish player who steals legs can walk ashore.
  - Protect them so the population doesn't die out, e.g. reseed if the aquatic population hits zero.
  - Other water animals: tiny schooling fish (ambient, huge numbers, cheap to draw) as well as larger swimmers and predators.
- **Underwater detail at the same level as above ground:**
  - coral reefs in warm shallows
  - seaweed and kelp swaying with the current
  - rocks, sand ripples and shells on the sea floor
  - light shafts (caustics) through the surface
  - an underwater fog and colour grade once the camera is below the surface
  - bubbles
- **Underwater caves** to explore, which means real geometry beyond the heightmap: cave meshes or carved-out volumes.
- **Swimming for the player:** diving and surfacing, and a breath meter for anything without gills (this ties into the existing drowning system). Gills should be a collectable body part.

## Prehistoric ecosystem (noted 2026-09-22, top of the queue for the daily runs)
Reference: ZBrush-style dinosaur sculpt sets (T-rex, triceratops, ankylosaur, stegosaur, hadrosaur/parasaurolophus, sauropod, spinosaur/sail-back, pterosaur, plesiosaur, mosasaur). The aim is a prehistoric ecosystem of dinosaur-inspired creatures, not generic blobs.

### More starting body shapes (the builder's body tab)
Current: 10. Add at least these, each with its own proportions, stance, neck/tail and default limb pose:
- **Theropod** (T-rex): huge head, deep jaw, short arms, thick digitigrade legs, heavy counterweight tail.
- **Raptor**: small theropod, long stiff tail, lean legs, sickle-claw feet.
- **Sauropod**: barrel body, very long neck and tail, column legs.
- **Ceratopsian** (triceratops): heavy shoulders, short tail, frill and horn anchors on the skull.
- **Ankylosaur**: low and wide, armoured back, tail club anchor.
- **Stegosaur**: arched back, small head, plate row along the spine, tail spikes.
- **Hadrosaur**: duck-billed, crest anchor, walks on twos or fours.
- **Sail-back** (spinosaur/dimetrodon): long jaw, tall sail along the spine.
- **Pterosaur**: light body, huge wing membranes on long finger bones, long crested head, folds its wings to walk.
- **Shark** (water only): torpedo body, tall dorsal fin, crescent tail, stiff pectorals.
- **Whale** (water only): huge smooth body, horizontal tail fluke, blowhole, flippers.
- **Plesiosaur / mosasaur** (water only): long neck with paddles, or a heavy-jawed swimmer with a fluked tail.
Water-only shapes have no legs, so they are water-bound until they take legs from something (see the water world section).

### Mouths and jaws
Today's mouths look like a lump. They should be real jaws:
- An upper and lower jaw with actual thickness, a hinge at the back, and a gape angle that animates (the jaw bone already exists in the skinned rig).
- Rows of teeth that follow the jaw line: sizes vary by species (dagger teeth on hunters, leaf teeth on grazers, a beak on others), with a tongue and a dark throat inside.
- The gape opens when biting, roaring, eating and threatening, and the teeth interlock when closed.
- Bigger jaws bite harder, which the damage numbers already read from the parts.

### Flight
The current flight (hold space to climb, ctrl to dive, otherwise sink) is too simple. Replace with a real flight model:
- **Flapping** costs stamina and gives thrust and lift; hold or tap space to flap, with the wingbeat animation and sound driven by it.
- **Gliding** when not flapping: you trade height for speed and can ride the air.
- **Swooping**: pitch down to dive and pick up speed, then pull up to convert that speed back into height.
- **Banking turns** that roll the body into the turn instead of turning flat.
- **Thermals** rising over open sunlit ground and cliffs, so a big flier can circle upward without flapping.
- **Stall** when too slow, and a run-up or a cliff needed to take off with a heavy body.
- Wing area and body mass decide climb rate, turn radius and how long you can stay up. A pterosaur-sized flier should feel heavy and fast, not like a hovering insect.
- Landing: flare, fold the wings, and walk.

### Colour and skin for the prehistoric look
Second reference set (coloured models): the palette and skin matter as much as the shapes.
- Muted, natural hides: olive and moss greens, dust browns, sand and grey, with darker backs fading to pale bellies (countershading).
- Markings: stripes down the back and tail, dappled spots, a bright warning flash on crests, frills, sails and throats.
- Skin reads at a distance: pebbled scales on big bodies, fine scales on small ones, a scute row along the spine, wrinkles gathering at the joints and neck.
- Crests, frills and sails carry the brightest colour, as they do on real display animals.

### Mouth reference detail (ZBrush skull sculpts: T-rex, carnotaurus, spinosaur, hadrosaur)
What those heads have that the current mouths do not:
- **The jaw is part of the skull, not a bolted-on lump.** The muzzle is a long box that narrows to the snout, with a distinct upper lip line, a cheek behind it, and a lower jaw that tucks inside the upper when shut.
- **Teeth vary along the jaw:** long fangs at the front, smaller ones toward the back, slightly curved and angled backward, upper and lower rows interlocking. A spinosaur has conical, evenly-spaced, forward-flared teeth; a hadrosaur has a toothless beak in front and grinding rows behind; a carnotaurus has a short deep muzzle with small teeth.
- **The gape is deep.** A hunter's mouth opens 40 to 60 degrees, showing the whole tooth row, a pale tongue, gum lines and a dark throat.
- **Around the mouth:** nostril openings on top of the snout, a bony brow ridge over the eye, cheek and jaw muscle bulges, loose skin folding under the chin and along the throat, and horn or knob ridges over the eyes and nose (carnotaurus horns, T-rex brow knobs).
- **Skin over the skull** is heavy pebbled scale with big scutes on the snout, packing tighter around the eye and lips, and wrinkles gathering where the jaw hinges.
- **Scale with the body:** the same jaw on a large body should read as massive (thicker bone, blunter teeth), and on a small body as needle-toothed and quick.
- Jaw types to build as separate mouth parts: **deep crushing jaw** (T-rex), **long fish-catching jaw** (spinosaur/croc), **short deep muzzle** (carnotaurus), **duck beak with grinding rows** (hadrosaur), plus the beaks and grazing mouths already in the game.

## Bones: parts economy and paid bundles (noted 2026-09-22)
- **Bones** are the currency. You earn **5 bones per kill**. They persist on your account.
- **Unlock prices by part tier:** weak 50, solid 100, super 200, ultra 500, alpha 2000 bones.
- **Bundles** (real money): 100 bones $2, 500 bones $5, 10000 bones $30.
- **Selling:** any part you own and do not want sells back for 25% of its bone value.
- **Rarity in the wild matches the tier:** weak parts are common on wild animals, alpha parts are almost never seen, so the same ladder governs both what you find and what you pay.
- **Per-world rarity roll:** some islands/matches generate with no alpha parts at all, and some with no ultra parts either, so a run with an alpha part in it is an event.
- Fits what exists now: parts are already unlocked one at a time from kills and stored per browser (`PROG.unlocked`, `PROG.kills`). Bones need a tier table per item, a balance, a shop screen in the builder (buy, sell, prices), and eventually server-side accounts so the balance and purchases are not local.
