namespace Genesis.Core.Genome;

/// <summary>
/// Body-plan part vocabulary. Mirrors index.html's PART_TYPES / PART object
/// (line ~258) exactly -- order matters nowhere in the JS (it is only ever
/// looked up by key), but is kept identical to the source for readability.
/// </summary>
public enum PartType
{
    Core,
    Mouth,
    Gut,
    Tail,
    Eye,
    Leg,
    Fin,
    Wing,
    Spike,
    Horn,
    Gland,
}

/// <summary>Feeding strategy for a MOUTH node. Mirrors JS MOUTH_MODES.</summary>
public enum MouthMode
{
    Herb,
    Carn,
    Scav,
}

/// <summary>Per-part metabolic upkeep cost and cardinality rule.</summary>
public readonly record struct PartDef(double Upkeep, int MaxRepeat, bool Singleton);

/// <summary>
/// Static lookup tables ported verbatim from index.html's PART object,
/// SINGLETON set, APPENDAGE_TYPES list and CAPS/PLAN constants (lines
/// 258-300, 482). Values here are asserted against the running JS build's
/// actual exported constants in ConstantsCharacterizationTests, not just
/// re-typed from reading the source, so a future edit to index.html's
/// numbers is caught by a red test rather than silently drifting.
/// </summary>
public static class PartVocab
{
    public static readonly IReadOnlyDictionary<PartType, PartDef> Parts = new Dictionary<PartType, PartDef>
    {
        [PartType.Core] = new PartDef(0.45, 1, false),
        [PartType.Mouth] = new PartDef(0.16, 1, true),
        [PartType.Gut] = new PartDef(0.18, 1, true),
        [PartType.Tail] = new PartDef(0.12, 1, true),
        [PartType.Eye] = new PartDef(0.12, 4, false),
        [PartType.Leg] = new PartDef(0.20, 6, false),
        [PartType.Fin] = new PartDef(0.20, 6, false),
        [PartType.Wing] = new PartDef(0.24, 4, false),
        [PartType.Spike] = new PartDef(0.16, 8, false),
        [PartType.Horn] = new PartDef(0.22, 2, false),
        [PartType.Gland] = new PartDef(0.20, 4, false),
    };

    /// <summary>Types that may appear only once per body (JS SINGLETON set).</summary>
    public static readonly IReadOnlySet<PartType> Singleton = new HashSet<PartType>
    {
        PartType.Mouth, PartType.Gut, PartType.Tail,
    };

    /// <summary>JS APPENDAGE_TYPES: every non-CORE type mutation may attach.</summary>
    public static readonly IReadOnlyList<PartType> AppendageTypes = new[]
    {
        PartType.Mouth, PartType.Gut, PartType.Tail, PartType.Eye, PartType.Leg,
        PartType.Fin, PartType.Wing, PartType.Spike, PartType.Horn, PartType.Gland,
    };

    public static readonly IReadOnlyList<MouthMode> MouthModes = new[] { MouthMode.Herb, MouthMode.Carn, MouthMode.Scav };

    public static bool IsSingleton(PartType t) => Singleton.Contains(t);
}

/// <summary>
/// Hard safety caps, ported verbatim from index.html's CAPS object (line
/// 300) plus the standalone MAX_SEGS/MAX_RADIAL constants (line 491).
/// </summary>
public static class GenomeCaps
{
    public const int Pop = 2500;
    public const int Plants = 2400;
    public const int GenomeNodes = 24;
    public const int PartsRendered = 64;
    public const int TreeDepth = 6;
    public const int Species = 2500;
    public const int BrainHid = 48;
    public const int BrainConn = 336;

    /// <summary>Max instances a segmented-chain gene may unfold into (JS MAX_SEGS).</summary>
    public const int MaxSegs = 4;
    /// <summary>Max instances a radial-ring gene may unfold into (JS MAX_RADIAL).</summary>
    public const int MaxRadial = 8;
}

/// <summary>
/// Brain (NEAT) topology constants ported verbatim from index.html (line
/// 323-354). Declared alongside the genome types because a genome always
/// carries a brain (see <see cref="Genesis.Core.Genome.Genome"/>); the
/// actual NEAT forward-pass/mutation behavior is out of scope for the
/// "genome representation" system and is ported when the brain/behavior
/// system's characterization tests are written.
/// </summary>
public static class BrainConstants
{
    public const int In = 26;
    public const int Out = 8;
    /// <summary>Always-on pseudo-input id (JS BIAS_ID = IN).</summary>
    public const int BiasId = In;
    /// <summary>Output node id base; never collides with a hidden node's innovation id.</summary>
    public const int OutputBase = 100000;
    public const double RankOut = 1e9;

    // Output slot indices (JS O_MOVEX..O_GRAB).
    public const int OutMoveX = 0, OutMoveY = 1, OutPursue = 2, OutAvoid = 3, OutSocial = 4, OutMate = 5;
    public const int OutGait = 6, OutGrab = 7;

    public const double GaitBoost = 0.55;
    public const double GaitCost = 0.85;
    public const double GrabGain = 0.55;
    public const double GrabCost = 0.30;

    /// <summary>Initial hidden-node count for a freshly seeded genome (JS BRAIN_HID0).</summary>
    public const int InitialHiddenCount = 10;
}

/// <summary>
/// Continuous body-plan gene ranges. Ported verbatim from index.html's PLAN
/// object (line 482): elong = length:width ratio, neck = browse/strike
/// reach, legLen = land stride. Every one of these is a genuine trade-off
/// evolved by selection, not a free stat bonus (see develop() in the JS for
/// how each is paid for in upkeep/agility/stability).
/// </summary>
public static class BodyPlanRange
{
    public static readonly (double Min, double Max) Elong = (0.35, 3.4);
    public static readonly (double Min, double Max) Neck = (0.0, 2.6);
    public static readonly (double Min, double Max) LegLen = (0.25, 2.8);
}
