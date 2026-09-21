using Genesis.Core.Rng;

namespace Genesis.Core.Genome;

/// <summary>
/// One node of the generative body-plan tree. Ported from index.html's plain
/// object shape produced by newCore()/newNode() (line ~453):
/// <code>
/// { type, size, mode, repeat, angle, segs, segTaper, curl, radial, children }
/// </code>
///
/// PHASE 00 DELIBERATE DEVIATION -- third rotation axis:
/// the JS build is a 2D/pseudo-3D side-elevation renderer, so each node
/// carries exactly one rotation gene ("angle", used by develop()'s
/// place()/placeChain() for mirrored-pair swing and segmented-chain
/// curvature). Godot 4 is fully 3D, and per the production plan we add the
/// other two rotation axes NOW, while no creature has ever been saved and
/// nothing depends on save-file compatibility yet -- retrofitting this
/// after Finn has real save files would be a breaking migration.
/// <see cref="RotationY"/> is the direct successor of the JS "angle" gene
/// (same semantics, same mutation step size when mutation/selection is
/// ported); <see cref="RotationX"/> and <see cref="RotationZ"/> are new
/// genes with no JS equivalent, default to 0 so a freshly ported genome
/// renders/behaves identically to before this axis existed, and are inert
/// until Phase 01+ rendering and the mutation/selection system decide how
/// they are driven and mutated.
/// </summary>
public sealed class GenomeNode
{
    public required PartType Type { get; set; }

    /// <summary>Overall scale of this part instance. JS: size, clamped to [0.2, 2.5].</summary>
    public double Size { get; set; }

    /// <summary>Feeding strategy; only meaningful when Type == Mouth. JS: mode.</summary>
    public MouthMode? Mode { get; set; }

    /// <summary>How many instances this node renders as (mirrored pair count, etc). JS: repeat.</summary>
    public int Repeat { get; set; } = 1;

    /// <summary>
    /// Rotation about the JS build's single original axis (the "angle" gene).
    /// Radians, wrapped to [0, 2*pi). Direct successor of JS: angle.
    /// </summary>
    public double RotationY { get; set; }

    /// <summary>New Phase 00 gene: rotation about X, radians. No JS equivalent; default 0.</summary>
    public double RotationX { get; set; }

    /// <summary>New Phase 00 gene: rotation about Z, radians. No JS equivalent; default 0.</summary>
    public double RotationZ { get; set; }

    /// <summary>Segmented-chain length (tapering repeats of this node). JS: segs, clamped [1, MAX_SEGS].</summary>
    public int Segs { get; set; } = 1;

    /// <summary>Per-segment taper factor. JS: segTaper, clamped [0.4, 1.05], default 0.82.</summary>
    public double SegTaper { get; set; } = 0.82;

    /// <summary>Segment curl amount. JS: curl, clamped [-1.2, 1.2].</summary>
    public double Curl { get; set; }

    /// <summary>Radial-ring instance count (0 = mirrored-pair placement instead). JS: radial.</summary>
    public int Radial { get; set; }

    public List<GenomeNode> Children { get; set; } = new();

    /// <summary>
    /// Ported from index.html's newCore() (line 453): the single, implicit
    /// root of every body tree. Never mutated in shape (only its children
    /// change), never anyone's child, and its own angle/segs/radial genes
    /// are never read by develop() -- only a *child's* are.
    /// </summary>
    public static GenomeNode NewCore() => new()
    {
        Type = PartType.Core,
        Size = 1.0,
        Mode = null,
        Repeat = 1,
        RotationY = 0,
        Segs = 1,
        SegTaper = 0.82,
        Curl = 0,
        Radial = 0,
        Children = new List<GenomeNode>(),
    };

    /// <summary>
    /// Ported from index.html's newNode(type, rng) (line 454). Draws exactly
    /// the same rng() calls, in the same order, as the JS original:
    /// 1. size = clamp(0.6 + 0.5*rng(), 0.2, 2.5)
    /// 2. mode: only for MOUTH, draws rng() to index MOUTH_MODES
    /// 3. repeat: 1 for CORE/singleton types, else 1 + floor(rng()*2) (i.e. 1 or 2)
    /// Consuming the RNG in a different order or count than the JS would
    /// desynchronize every subsequent draw for the rest of a fixed-seed run,
    /// so this order is characterization-tested against real JS output
    /// (see GenomeNodeTests.NewNode_MatchesJsFixture_ForEveryPartType).
    /// New rotation genes (X/Z) and RotationY default to 0 -- newNode() in
    /// the JS build never randomizes angle/segs/segTaper/curl/radial either
    /// (comment at line 459: "all neutral defaults... until mutation
    /// actually explores these dimensions"), so no additional rng() draws
    /// are needed here to stay in lockstep with the JS sequence.
    /// </summary>
    public static GenomeNode NewNode(PartType type, GenesisRandom rng)
    {
        double size = Clamp(0.6 + 0.5 * rng.NextDouble(), 0.2, 2.5);
        MouthMode? mode = type == PartType.Mouth
            ? PartVocab.MouthModes[(int)(rng.NextDouble() * PartVocab.MouthModes.Count)]
            : null;
        bool fixedSingleRepeat = type == PartType.Core || PartVocab.IsSingleton(type);
        int repeat = fixedSingleRepeat ? 1 : 1 + (int)(rng.NextDouble() * 2);

        return new GenomeNode
        {
            Type = type,
            Size = size,
            Mode = mode,
            Repeat = repeat,
            RotationY = 0,
            RotationX = 0,
            RotationZ = 0,
            Segs = 1,
            SegTaper = 0.82,
            Curl = 0,
            Radial = 0,
            Children = new List<GenomeNode>(),
        };
    }

    /// <summary>
    /// Deep clone. Ported from index.html's cloneNode(n) (line 465). Must
    /// never share Children list instances or GenomeNode references with the
    /// source -- mutation and crossover both rely on clones being fully
    /// independent (see CloneNodeIndependence characterization test).
    /// </summary>
    public GenomeNode Clone()
    {
        return new GenomeNode
        {
            Type = Type,
            Size = Size,
            Mode = Mode,
            Repeat = Repeat,
            RotationY = RotationY,
            RotationX = RotationX,
            RotationZ = RotationZ,
            Segs = Segs,
            SegTaper = SegTaper,
            Curl = Curl,
            Radial = Radial,
            Children = Children.Select(c => c.Clone()).ToList(),
        };
    }

    private static double Clamp(double x, double lo, double hi) => x < lo ? lo : (x > hi ? hi : x);
}
