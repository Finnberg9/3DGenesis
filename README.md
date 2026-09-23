# 3DGenesis

This version takes a different approach to the conventional Genesis game. Rather than being an outside observer, you are one of the creatures. You begin as a baby with a randomly assigned genome, and grow over time by aging and eating: every meal feeds an energy budget that gets spent on growth, so a well-fed creature reaches full size faster and starts fights it can actually win. As you explore you'll encounter other creatures, some of which are your direct relatives sharing part of your genome, others are total strangers from an entirely different starting lineage, and telling the two apart matters if you're trying to avoid inbreeding or looking to maximize genetic diversity in your offspring. Your job is simple: stay alive, mate, kill, and evolve.

Combat isn't animated by hand or picked from a fixed move list. Every attack animation is generated from whatever your genome actually built onto your body. The default is a head-butt, but grow a tail and you'll whip opponents with it, grow spikes and you'll stab them, grow a heavy jaw and you'll bite. Damage scales with your real anatomy: get bigger, add spikes, horns, tails, or jaws, and every one of those attacks hits harder. Every kill grows your genome space and lets you choose one of the victim's parts (a horn, an eye, a claw, a fin, wings, whatever it had) and take it. You get exactly the one you took: parts are counted, so four legs means four legs picked up or bought, not one leg copied four times. Parts you hold are added in the creature editor, and everything you add costs energy to keep alive, so a bigger, spikier body has to eat more. Anything that gets hit cannot eat for 5 seconds after the last hit. Plant eaters run from their attacker, meat eaters turn and fight back. If you find a creature you'd rather breed with than fight, press M to mate, and you become the offspring, a genetic hybrid of both parents' traits. The character you were playing a second ago doesn't disappear, it keeps living as an ordinary autonomous creature in the world while you continue on as its child, so your lineage keeps accumulating even after you "die" as any one individual. Whatever you inherit or evolve into changes what you can physically do: wings let you fly by holding space, fins let you swim, and extra legs make you run faster.

Explore the randomly generated map, upgrade your character through what you eat and who you kill, and survive in a completely autonomous, unpredictable ecosystem that keeps running whether you're paying attention to it or not. Unlike Spore and Subnautica, this game doesn't design its creatures ahead of time, it relies entirely on genetic algorithms and real Darwinian selection to determine what shapes and sizes show up. Nothing about the creatures is hard-coded. There are no preset species and no hand-placed templates; everything you see was produced by randomness and genome replication playing out over generations, which means no two playthroughs, and no two creatures, will ever be the same. Turn up founder diversity to seed the world with a wider spread of starting creatures, and adjust the other ecosystem parameters however you want to tune what kind of world emerges.


## Creature editor

Press **B** at any time (or the "edit creature" button) to open the creature editor. The simulation pauses while it is open.

- **Body:** pick one of ten base body shapes, then adjust body length, thickness, neck and tail. You always hatch as a baby, and every part grows in proportion with the body.
- **Parts:** drag mouths, eyes, horns, spikes, armour, fins, wings and details from the palette onto the body. They attach exactly where you drop them. Mirror mode (M) places a matching part on the other side.
- **Limbs:** drag a limb onto the body, then drag its glowing joints to bend and stretch it. A limb that reaches the ground walks as a leg. Any other limb is an arm. Drop feet, hands, claws and pincers onto the end of a limb. Claws and pincers add attack and a claw swipe, and hands let you reach food up high.
- **Paint:** set body, underside and pattern colours plus a pattern. Click any part to recolour, resize or rotate it, or drag it off the body to remove it. Ctrl+Z / Ctrl+Y undo and redo.
- **Genome space:** every part costs genome space. You start with a little free space, and each kill adds more. The stats panel shows how each change affects speed, attack, defence, vision, reach and energy use.
- **Inventory:** parts are counted, not flagged. You own a number of copies of each part, and you can wear at most that many at once: take one spike off a corpse and you have one spike, not a licence to grow a dozen. Each kill lets you take one part off the victim, which adds one copy; buying in the shop adds one copy and you can keep buying. Selling gives back one copy, and never one the creature is currently wearing. The body you walked in with stays rearrangeable whether or not you own its parts, so an inherited or evolved animal is never locked out of its own anatomy. Kills, copies and saved creatures are remembered in this browser. Add `?sandbox=1` to the URL for unlimited parts.

Your design is a real genome. Your kin and offspring inherit it with small mutations, and it breeds and evolves like everything else on the island.


## Combat

- **Stamina** powers sprinting, dodging, guarding and attacks. Run it dry and you are exhausted: no sprint or dodge, and your attacks land at half strength.
- **Telegraphed attacks:** an animal attacking you winds up first, and a marker over its head counts down. Bigger animals take longer to wind up.
- **Guard (hold Q)** blocks 70% of a frontal hit. Raise it just as the marker turns yellow to **parry**: you take no damage, the attacker staggers, and your next hits on it are critical.
- **Dodge (Z or tap Shift)** rolls you away, and you can't be hit during the first part of the roll.
- **Lock on (T or middle click)** keeps you facing a target and shows its health.
- **Sound is 3D.** On headphones, footsteps, growls and roars come from where the animal really is, including behind you.
- **Eat to grow.** Every kill you feed on and every carcass you eat makes you bigger, up to one and a half times your adult size.
- **Drowning.** Without fins you drown fast if your head goes under water.
- The dead leave skeletons shaped like their bodies, which slowly sink into the ground.
- Hits cause knockback, heavy hits stagger, and big wounds bleed. Wounded animals leave a blood trail you can follow.

## Online match

- **No grace period.** The gates open and the match is live: anyone can kill anyone from the first second.
- **The island empties five minutes after the gates.** Until then it is full of animals to hunt and skeletons to loot, so the opening is a choice between building your beast and going straight at people.
- **Every other player carries their name and their health** over their head. Terrain hides a plate, so it is not a wallhack, and so do distance and the closing fog.
- **The fog closes** at a constant rate against a walking speed four times faster, and the last ring is a real arena rather than a pinprick.
