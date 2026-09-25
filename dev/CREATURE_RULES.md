# Structural rules for creature bodies
Researched 2026-09-25. These are **rules**, not references: nothing below is a
copy of a named creature from another game. Each line is a structural principle
with the place it was observed, stated in a form that can become a plan
parameter. Where a number is mine rather than a source's it is marked (derived).

Provenance: [DEV] developer/art-director on the record. [CRAFT] working artist's
general rule. [ANALYSIS] critic reading of a shipped asset, not developer intent.
[ANAT] comparative anatomy. Anything else is derived and marked.

## 1. Silhouette — what reads as wrong at a glance

S1. **Break the count in exactly one system, leave the rest of the body plan
    intact.** A legible base category ("quadruped", "larva") makes the surplus
    read as an error in a known thing rather than as a new species the viewer
    would simply learn. [ANALYSIS: Bloodborne Amygdala/Rom]
    → plan parameter: never violate more than one of {limb count, joint count,
    head position, symmetry} per creature.
S2. **Never mirror.** If no pair on the body matches, the viewer's
    "healthy conspecific" classifier never terminates and they keep rescanning.
    [DEV: Amnesia, Olof Strand — "the concept design demanded it to be
    completely unique in all of its parts"]
    → plan parameter: `crook`, a per-limb-pair length and rake asymmetry.
S3. **Asymmetry below ~10% reads as a rigging bug; above ~25% with a matching
    postural compensation reads as authored.** The compensation (shoulder drop,
    hip hike, head tilt toward the short side) is the load-bearing part, not the
    delta. (derived from the contrast in S2/S9)
S4. **Detail density follows mass: one dense zone, two visual resting zones.**
    "Having detail everywhere is as good as having no detail at all."
    [CRAFT: Riot, Tommy Gunardi Teguh]
S5. **Aspect ratio of the footprint is a threat-class signal** — a low wide
    silhouette tells the player to back up, a tall narrow one to strafe, before
    they have identified the creature. [DEV: Dead Space, Ben Wanat]
S6. **The test is a flat black fill at 64 px from front, three-quarter and
    side; the limb count must be countable in all three.** If it is not, every
    other rule here is invisible in play. (derived from [DEV] + [CRAFT])
S7. **Cut one manufactured shape into the organic mass** — a true circle, a
    seam, a grid. Flesh does not make Euclidean primitives, so their presence
    implies something worked on this body deliberately. [ANALYSIS: Silent Hill 2]

## 2. Proportion — where mass goes, what gets starved

P1. **Skew the back's high point over the shoulders and let the hips fall
    away.** A symmetric arch puts the peak at the middle of the animal, which is
    a grazing animal. (already implemented as `hump`)
P2. **Two lobes, not one.** A thorax and an abdomen with a pinch between them
    cannot be expressed by a single girth curve. (implemented as `waist`)
P3. **Hang the dominant mass low and support it on limbs visibly too thin for
    the load** — pendulous mass 40–50% of body volume, supporting shaft under
    10% of that mass's width. The eye computes load-bearing automatically, and a
    limb that cannot carry its body reads as parasitic or diseased.
    [ANALYSIS + ANAT; numbers derived]
P4. **Starve until the skeleton is the silhouette.** Replace convex muscle with
    concave spans of draped skin: concavity where the eye predicts convexity is
    a per-region error signal, not one global "it is thin." [ANALYSIS]
P5. **Grow reach, not bulk.** Make the arms long enough that the top of the idle
    silhouette is arm, not skull — the threat vector then sits above the height
    players use to judge clearance. [DEV: Dead Space, Wanat]
P6. **Long limbs need a shrunken torso, not a normal one.** Lengthening limbs
    alone reads as a tall animal; shrinking the torso at the same time breaks the
    head-height system the viewer uses to estimate scale, so no consistent body
    size can be assigned. [DEV/ANALYSIS: Little Nightmares, Tarsier]
P7. **Head-to-shoulder is one of the most over-learned ratios humans hold.**
    Errors of 30–40% are detectable before any detail resolves, which is why a
    wrong head survives darkness and distance. (derived)

## 3. Joints and limbs

J1. **+1 segment per limb.** Three visible segments is the human/mammal count;
    four still parses as "limb" and so gets assigned to the known category and
    then flagged. Five reads as a tentacle and stops being disturbing. [ANALYSIS]
J2. **Put the extra segment where an arachnid has it** — between knee and ankle,
    at roughly 0.5–0.7× thigh length — and it reads as arthropod rather than as
    an injury. [ANAT: spider patella and metatarsus]
J3. **Arthropod joints are single-plane hinges, and that is the real tell, not
    the count.** Alternate the hinge plane between adjacent segments and you get
    the folding, origami collapse the viewer has only ever seen on insects.
    [ANAT]
J4. **There is no backwards knee in nature; the rearward joint is the ankle.**
    To get the silhouette honestly: raise the ankle to 55–70% of leg height,
    lengthen the metatarsal block to 0.8–1.0× the tibia, keep the femur short and
    tucked. [ANAT: digitigrade/unguligrade]
J5. **A wrong joint that produces a wrong behaviour beats a wrong outline.** A
    foot fused into a peg forces permanent counterweighting with the forelimbs.
    [ANALYSIS: Dead Space Slasher]
J6. **Hyperextension is cumulative.** One joint 10–15° past neutral reads as an
    injured animal; four or more joints each past neutral reads as another
    species. [ANAT: the Beighton score works exactly this way]
J7. **Rotate the cross-section along the shaft rather than only tapering it** —
    wide-flat at the joint, narrow-flat at mid-shaft — so turning the creature
    reveals a different profile. A constant circular sweep is what makes a limb
    read as pipe. [CRAFT]
J8. **Put the hard edge where a tendon or bone is actually superficial.**
    Arbitrary faceting reads as low-poly; faceting on a real subdermal landmark
    reads as starvation. (derived from [ANALYSIS])
J9. **Emergence point beats limb shape.** A normal limb from a wrong socket is
    more alarming than a strange limb from a right one. [ANALYSIS: limbs from
    the scapular mass rather than the shoulder]
J10. **Odd totals.** Bilateral symmetry is the default assumption for anything
    moving toward you; an odd count makes that assumption unsatisfiable whichever
    midline the viewer picks. [ANALYSIS]
J11. **When limb count exceeds about six, give the viewer a count of torsos
    instead.** People parse "three bodies" far faster than "twelve limbs."
    (derived from [ANALYSIS])
J12. **Raptorial forelimb geometry that reads as a tool from every angle:**
    thick grooved femur, thin blade tibia that folds back into the groove,
    terminal hook. [ANAT: mantis]

## 4. Heads and faces

H1. **Hide the face, but do not use a mask.** If the occluder still has eye
    holes, a chin line or a jaw edge, it has failed — replace the whole cranial
    volume with a shape that has no facial logic. [DEV: Masahiro Ito]
H2. **With no face, the silhouette's angular content carries the affect.** One
    dominant acute angle per creature; soft ovoids read as suffocation, acute
    angles as pain. [DEV: Ito]
H3. **Eyelessness makes the head a pointing organ.** With no eyes the player
    infers attention from head-aim and hold-poses, so the neck needs a clear
    forward axis and a deliberate re-aim. [DEV: Giger on the xenomorph —
    "it would be even more frightening if there are no eyes"]
H4. **Eye clusters work as a second state** — hidden behind a lattice at rest,
    erupting through the gaps on attack — not as a permanent feature.
    [ANALYSIS]
H5. **A displaced eye beats more eyes.** One eye somewhere with no clear purpose,
    with the original sockets left empty and dark. [ANALYSIS]
H6. **Remove the neck, or remove the place a head would go.** The neck is the
    primary cue for facing; without it the player cannot tell whether they have
    been noticed. [ANALYSIS]
H7. **A blind head needs a visible compensating organ,** and that organ should be
    smooth, not serrated — implied lethality disturbs more than spikes.
    [ANALYSIS]

## 5. Split mandibles, wings, tails

M1. **Four mandibles — two upper, two lower — crossed when closed, so opening is
    an unlatching rather than a hinge.** [ANAT/design: arthropod and film
    lineage] What is revealed inside is soft, pink, wet and small against the
    hard outer segments; filling the interior with a second set of fangs
    destroys the contrast, which is the whole effect.
M2. **A nested inner jaw must have its own complete tooth set and its own lips,
    and travel on a straight piston axis.** It is a second head, not a tongue.
W1. **Bat wing = four membrane panels, not one:** propatagium (neck to first
    digit), dactylopatagium (between digits), plagiopatagium (last digit to the
    hindlimb), uropatagium (between hindlimbs). Build only the inter-digit panel
    and you get an umbrella, not a wing. [ANAT]
W2. **The claw hook sits at the wrist — the outermost forward bend — not at the
    elbow.** [ANAT: the bat thumb stays free and clawed at the leading edge]
W3. **Aspect ratio b²/S: 5–11 reads as a wing.** Below ~4 it reads as a fin or a
    cape; above ~12 it reads as a needle at thumbnail size because the membrane
    subtends too few pixels to register as filled area. To keep a high-AR wing
    legible, hold the leading edge as one unbroken curve. [ANAT: bat flight;
    thresholds derived]
W4. **Tatter the trailing edge only.** Tears start at the trailing margin and run
    *parallel* to the digit spars, never across them, because the spars are the
    load path; depth capped at ~40% of the chord so each panel keeps a
    continuous strip; clustered in the distal panels which take the most
    flutter. (derived from [ANAT])
W5. **Translucency is strongest in the inter-digit panels and weakest at the
    leading edge,** where the muscle and thumb mass sit. (derived)
T1. **Stinger tail: five hard segments, each 0.85–0.9× the one before, then a
    telson that BREAKS the taper — widening back to roughly segment-2 diameter
    before the needle.** A monotonic taper to a point reads as an antenna; the
    re-widening reads as a delivery device. [ANAT: scorpion metasoma; ratios
    derived]
T2. **Whip tail: twelve or more bones, per-joint bend capped near 15°,
    curvature spread evenly.** A kink at one joint reads as a break; the same
    total curvature over twelve joints reads as muscle. [CRAFT: tentacle IK]
T3. **Papillate the long tail.** A smooth taper at distance is a cable; a needled
    one holds a legible width because the fuzz silhouettes wider than the core.
    [ANALYSIS]

## 6. Layering

L1. **Build the stack in order: bone → organs → muscle → tendon → fat → skin,**
    and reveal later layers as damage rather than as static art. A creature that
    starts with bone showing has spent its whole vocabulary on frame one.
    [DEV: Dead Space Remake, Mike Yazijian]
L2. **No imported armour.** Every plate or blade must have a visible origin
    wound at its base, with skin retracted around where it tore out. [DEV]
L3. **One member must read as two systems at once** — bone and machined
    component simultaneously. The moment you can point at where one ends and the
    other begins, it stops working. [ANALYSIS: the biomechanical tradition]
L4. **Name the mechanical job before adding the organ.** "People just want it to
    be weird for the sake of weird, and that is not what we are doing."
    [DEV: Scorn, Ljubomir Peklar]

## 7. Wet and slimy shading — the implementable part

X1. **Wet = darker diffuse + smoother specular, driven by porosity.**
    `factor = mix(1.0, 0.2, porosity); diffuse *= mix(1.0, factor, wet);`
    Fully wet, fully porous drives diffuse to 0.2×. Non-porous regions (chitin,
    bone, plate) must be exempted or wet armour goes muddy.
    [Lagarde, physically based wet surfaces]
X2. **Porosity can come from gloss without a map:** `porosity = saturate(-2.5 *
    gloss + 1.25)`. [Lagarde]
X3. **Fluid pooling is a normal-flattening operation, not a texture.** Blend the
    surface normal toward the geometric normal as accumulated fluid rises, driven
    by cavity and by a downward-facing normal — so mucus sits in the creases
    between plates and those go mirror-flat while the ridges keep their micro
    detail. [Lagarde]
X4. **The slime layer is a clear coat at a fixed f0 of 0.04** (air/coat IOR 1.5),
    composited as `(diffuse + spec) * (1 - Fc) + clearcoatBRDF`. [Filament]
X5. **Dual normals are the single biggest "it looks wet" win:** the coat normal
    is the fluid sheet — large, smooth, flowing, animated — and the base normal
    is pores and scales, small, static, high frequency. Without the split, the
    light hits both together and the surface reads flat. [Epic]
X6. **"The specular moves" = a flow-mapped normal on the COAT layer only,
    sampled twice at half-period offset and cross-faded** so it never stretches
    without bound. The flesh normal stays static, so the body stays solid while
    the highlight crawls over it. That is the whole trick.
    `w = abs(1 - 2*frac(t)); n = mix(sample(uv - flow*frac(t)), sample(uv -
    flow*frac(t+0.5)), w)`
X7. **Two specular lobes, not one** — a soft broad lobe plus a tight one. A
    single-lobe highlight always reads as plastic. [Epic]
X8. **Red travels about ten times further than blue through flesh** (six-Gaussian
    skin profile: the 28 mm² tail carries weight 0.090 in red against 0.032 green
    and 0.056 blue), which is why thin flesh — membranes, torn flaps, ear edges —
    glows red at the rim. [GPU Gems 3 ch.14]
X9. **Scales follow the body by ROTATING the tile, not by advecting it.**
    `dir = normalize(flow); uv = mat2(dir.y, -dir.x, dir.x, dir.y) * uv;`
    and the normal's derivatives must be rotated by the same matrix or the
    highlight direction disagrees with the visual direction of the scale. That
    second step is the one everyone skips. [Catlike Coding, directional flow]
X10. **Scales catch light individually with anisotropic roughness aligned to the
    same flow tangent:** `at = a*(1+aniso); ab = a*(1-aniso)`, so each row's
    highlight stretches along the row. [Filament]
X11. **Cavity drives specular occlusion, reduced at Fresnel edges** — otherwise
    the wet highlight glows inside creases that should be dark. [Epic]

## Honest gaps
- No studio publishes numeric proportion specs. Every ratio above that is not
  from an anatomy source is derived and is a starting value to tune by eye.
- Several widely repeated "facts" about specific creatures are critic analysis
  of the shipped asset, not design intent. Tagged [ANALYSIS] where so.
