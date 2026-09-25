# Creature brief — set by Finn, 2026-09-25

**Reference images: `dev/refs/`, one folder per creature, indexed in
`dev/refs/README.md`.** Read `dev/refs/00-rejected-output/` first — those are
screenshots of what the game actually produced and why it was rejected.

Reference images supplied per creature. **These are style and structure references,
not things to copy.** Several are recognisable creatures from other commercial games.
What gets taken is the structural spec below and the level of finish; what does not get
taken is the specific creature. Finn's own instruction earlier the same day: "dont COPY
ANYONE, not fully at least."

Target quality bar, in Finn's words: proper detail and realism, and **every part has to
feel organically placeable on the creature** — not bolted on.

---

## 1. RAPTOR / DRAGON HYBRID
- backwards-arching spikes off the neck (sweeping back over the shoulders, not vertical)
- a **reverse horn** off the head — curving forward/down rather than back
- bone tail: exposed vertebral segments rather than a fleshed tube
- small spikes running the length of the tail
- forward-bending legs (digitigrade: high ankle, long metatarsal)
- **usable claw hands** — grasping forelimbs, not decorative
- bipedal raptor stance, long low body, head carried forward

## 2. WOLF / DOG QUADRUPED
- flared **feather-blade frill** over the neck and shoulders, standing up and fanning out
- large scales along the back reading almost as **armour plates**
- heavy quadruped, wolf proportions, broad chest
- long muzzle, whiskered/spined cheeks

## 3. BRUTE
- large upper body, **huge gorilla arms**, noticeably smaller hind legs
- head carried **below the shoulder line** — hunchbacked
- thick long **fur** on the head and upper back, against bare hide below
- large curved **horns** on the head
- gorilla underbody: heavy chest and gut, knuckle stance

## 4. SERPENT
- no legs at all
- long coiling snake body
- aggressive **dinosaur / dragon face** — not a snake head: heavy brow, long fanged jaw
- spined dorsal ridge running the length of the back
- belly scutes distinct from the dorsal scale

## 5. SPIDER
- eight **furry** legs, each carrying sharp spikes down its length
- large forward spider **fangs** (chelicerae), held out and down
- large heavy rear abdomen ("butt"), soft and swollen against the hard front
- **armour plates over the face and cephalothorax** — crab-like, ridged, overhanging
  the eyes so the front reads as a shield rather than a face
- front pair of legs raised and held up, the way a spider threatens

## 6. STALKER (small, fast)
- **small** — rodent / cat scale, the only little one in the set so far
- **huge mouth and fangs taking up the whole front of the animal**: long curved
  fangs splaying outward, jaws wider than the skull behind them. No real face to
  speak of — the mouth IS the head
- **bird-like feet that sit flat** on the ground: long splayed toes, broad contact
- **very back-facing legs** — hard digitigrade rake, built for high jumping and
  fast running, hind limbs folded well behind the hips
- from the references: tufted pale fur over the shoulders and back against bare
  red muscle below, and a long thin whip tail with bulbous segments along it

## 7. PTERODACTYL (first flier)
- **relatively small body** carrying an enormous wingspan — the wing is the animal
- membrane stretched thin and **fabric-like**, spanning a single elongated finger
  spar, translucent where it is thinnest
- **the wings act as front feet.** Folded, the wrist knuckle plants on the ground
  and the animal walks and sits as a quadruped on wing-knuckles plus small hind
  legs. This is the defining feature of the walking pose, not a detail.
- **free clawed fingers at the wrist**, held clear of the membrane: talons inside
  the wing, usable
- **huge reverse horn on the head** — a big backward-and-upward swept crest off the
  skull, as long as the beak itself and leaning back over the neck. Same reverse
  sweep as creature 1's horn, but far bigger and blade-flat rather than conical
- long toothed beak, long stiff neck, small hind legs with bird feet
- folded on the ground the wing tips sweep back up and over the body

**Rig note, not a mesh note:** `classifyLimbs` calls a limb a LEG when its foot
lands within 72% of the mean foot depth, and the walking physics reads the same
function. A wing that plants its knuckle on the ground will therefore be counted
as a third and fourth leg unless the wing knuckle is handled explicitly. The
`hollow` plan already has a comment about exactly this trap, where its arms had
to be kept deliberately short of the ground to stop them becoming legs.

## 8. DRAGON
Explicitly a contrast piece against 7. Same wing architecture, everything else
heavier — the pair has to read as two different animals, not one at two sizes.

- **much scarier, more defined head** than the pterodactyl: heavy brow, horn rack
  sweeping back off the skull, spined jaw, visible teeth when closed
- **a properly built body that walks easily on the ground** — not a flier that
  can barely stand. Deep chest, muscled shoulders and haunches
- **four near-equally-sized limbs.** This is the key structural difference from 7,
  where the hind legs were tiny: here the wing-arms and the hind legs carry
  comparable mass, so it stands as a real quadruped and the wings fold as arms
- **four sharp points along each wing** — the finger spars projecting past the
  membrane's trailing edge as hard claw tips, not a smooth scalloped edge
- **long heavy tail**, carried out behind for balance, thick at the base
- spined dorsal ridge from skull to tail tip
- scaled hide, plated on the chest and belly

## 9. HUSKED / STALKER-ALIEN
Finn's note: "almost directly copying the alien from alien movies."

**Built to the anatomy, not to the character.** Everything Finn typed as the spec
is a generic archetype and gets built as written:

- **long, slender body**, no fur anywhere
- **visible bone and muscle** through the skin: ribs, scapulae, spine, the long
  tendons of the limbs all reading through a tight hide with no fat over it
- very long and very skinny overall — the most starved thing in the set
- **massive head that curves backwards**, elongated well past the neck join
- **giant tail**, longer than the body, carried out and curling
- **long backward-facing legs** — extreme digitigrade rake, hock high and far back
- extra-jointed, over-long arms and a second pair of thin limbs off the torso
- wet, dark, translucent-looking hide; spined processes standing off the shoulders

What is deliberately NOT reproduced, because these are the specific identifying
marks of a protected character rather than anatomy: the smooth eyeless domed
cranium with its exact crown ridging, the telescoping inner pharyngeal jaw, the
four segmented dorsal back-tubes, and the specific banded blade tail. The
creature gets its own answers to each — a low ridged skull with sunken pit-fields
instead of the dome, a split mandible instead of the inner jaw, bare scapular
blades instead of the back-tubes, and an exposed-vertebra tail.

## 10. ARMADILLO-LIZARD (the tank)
Armadillo vibes crossed with a lizard.

- **fat and round** — low, broad, wider than it is tall
- **four smaller thick legs**, short and columnar, carrying a lot of mass
- **large rocky armour over the back**: big irregular interlocking plates with
  real gaps between them, not a scale texture. The plates sit proud of the hide
  with soft flesh visible in the seams
- **the armour is toxic** — it hurts what attacks it
- blunt heavy lizard skull, wide jaw, tusks at the corners
- short thick tail, plated

**Stats, not just looks** (the first spec in this set with gameplay attached):
the most HP in the game, the slowest, and the hardest to kill. That should fall
out of the existing mass-driven attribute system rather than being hand-set —
heavier total mass already lowers speed and raises defence — plus the toxic
plates as a damage-reflect on the armour.

---

## The set as a whole

The failure mode three sessions running was creatures that all read as one
animal. Worth recording that this brief does not have that problem, because
Finn specced the variety rather than leaving it to me:

| | mass distribution | limbs | head solution |
|---|---|---|---|
| 1 raptor | rear-balanced biped | 2 legs + 2 grasping arms | horned, reverse |
| 2 wolf | even heavy quadruped | 4 | frilled, long muzzle |
| 3 brute | front-heavy, hips gone | 2 huge arms + 2 small legs | below the shoulders, horned |
| 4 serpent | uniform, legless | 0 | dragon skull on a snake |
| 5 spider | rear-heavy abdomen | 8 | armoured face shield |
| 6 small fast | tiny, light | 4, hard back-rake | the mouth IS the head |
| 7 pterodactyl | wing-dominant | 2 wings walking as feet + 2 tiny legs | crested beak |
| 8 dragon | even, four equal limbs | 4 + wings | horned rack |
| 9 starved | very long, very thin | extra-jointed, over-long | big backswept ridge |
| 10 tank | fat, low, round | 4 short columns | blunt armoured |
| 11 insect | compact bulb on stilts | 6 long + 2 small wing pairs | small hard mandibled head |

Eleven in total. No two share a mass distribution, a limb architecture or a head
solution. That
is the test that matters, and for once it is satisfied by the brief itself.

**Gap:** nothing aquatic. The game has sea plans and a swim/breath system; none
of the ten uses them.

## 11. INSECT (ground, with flight)
- **mainly a ground animal.** The wings are a secondary ability, not the body plan
- **long creepy legs** — thin, over-long, high-kneed, lifting the body well clear
  of the ground and folding above the back line
- **rounded body** — a compact bulb of a thorax/abdomen, smooth against the
  angular legs
- **two sets of small wings**, buzzing insect wings rather than membrane sails:
  short, narrow, fast-beating, held out past the body but nowhere near large
  enough to dominate the silhouette
- from the references: clustered simple eyes, mandibles held forward, a hard
  ridged dorsal shield over the thorax, and bristles along the leg segments

**Distinct from 5 (spider):** the spider is eight legs, no wings, a heavy hanging
abdomen and an armoured crab face shield. This one is six legs, flies, has a
compact rounded body and a small hard head. If these two ever start reading as
the same animal on the contact sheet, that is the bug.

**Distinct from 7 and 8:** both of those are wing-dominant, where the wing IS the
animal. Here the wing is a small fast appendage on something that mostly walks.
Three fliers in the set, three completely different wing architectures.

## Balance rule: flight costs health

**Every flier in the set has low HP.** Flight is already the strongest ability in
the game — it breaks line of sight, ignores terrain, escapes any ground fight and
reaches food nothing else can — so it is paid for in survivability. That applies
to 7 (pterodactyl), 8 (dragon) and 11 (insect), and to any flier added later.

Implementation note: do NOT hand-set an hp number per plan. HP already derives
from mass, and `test_plans` asserts that **every body plan in the game starts on
exactly 100 hp** — hand-setting would break that guard and desync the whole
health reference. The right lever is the existing mass-driven attribute system:
a wing carries real weight and lift, so a flier that can actually beat its own
weight has to be built light, and light means fragile. If the numbers do not come
out low enough on their own, the fix is a flight-capable modifier on defence, not
a magic hp constant.

Note 8 (dragon) is the tension case: it is specced as heavy, walks easily on the
ground, and has four near-equal limbs — so it will not come out fragile on mass
alone. It probably needs the biggest explicit flight penalty of the three, or it
becomes the best creature in the game at everything.

---

## Engine work these depend on (done 2026-09-25 night)
Before any of this was buildable the creature vocabulary was **two shapes, both round**
(sphere, round cone) with no boolean subtraction anywhere. Added:
- `'b'` oriented rounded box — flat faces, hard edges, adjustable corner radius
- `'e'` ellipsoid in the core SDF (it previously returned NaN and poisoned the field)
- `sub: true` on any primitive — it CUTS instead of adding, with the same blend radius

Verified numerically: a box's face is flat to 3 decimal places across its whole span, and
a sphere cut into a slab drops the surface from 0.300 to 0.000.

What that unlocks, per creature above: armour plates with real edges (2), exposed
vertebrae (1), horn sockets and carved eye pits (3), belly scutes distinct from dorsal
scale (4), and chiselled facets on every limb rather than round pipes.
