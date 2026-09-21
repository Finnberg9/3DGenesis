namespace Genesis.Core.Genome;

/// <summary>
/// A complete, heritable genome. Ported from index.html's genome shape
/// (comment at line 475): "full genome = { body, brain:{nodes,conns}, hue:0..1 }",
/// plus the scalar genes cloneGenome (line 492) also copies: scale, life,
/// elong, neck, legLen.
/// </summary>
public sealed class Genome
{
    public required GenomeNode Body { get; set; }
    public required Brain Brain { get; set; }

    /// <summary>Color hue, JS range [0, 1). JS: hue.</summary>
    public double Hue { get; set; }

    /// <summary>
    /// Exponential overall body size multiplier, JS clamp [0.12, 16], no
    /// practical ceiling ("EXPONENTIAL evolvable body size", line 884).
    /// JS: scale, defaults to 1 when absent.
    /// </summary>
    public double Scale { get; set; } = 1.0;

    /// <summary>Inheritable lifespan multiplier, JS clamp [0.5, 4]. JS: life, defaults to 1.</summary>
    public double Life { get; set; } = 1.0;

    /// <summary>Length:width ratio body-plan gene, JS clamp [0.35, 3.4]. JS: elong.</summary>
    public double Elong { get; set; } = 1.0;

    /// <summary>Neck/reach body-plan gene, JS clamp [0, 2.6]. JS: neck, defaults to 0.</summary>
    public double Neck { get; set; } = 0.0;

    /// <summary>Leg length body-plan gene, JS clamp [0.25, 2.8]. JS: legLen.</summary>
    public double LegLen { get; set; } = 1.0;

    /// <summary>
    /// Ported from planOf(g, k) (line 483): reads a body-plan gene with its
    /// JS default (0 for neck, 1 for elong/legLen) substituted when the
    /// stored value is null/absent, then clamps to that gene's valid range.
    /// C# has no genome-field-absent state (the properties always hold a
    /// double), so PlanOf here just (re-)clamps the current value -- the
    /// "substitute default when absent" half of the JS behavior is captured
    /// by the property defaults above (Elong/LegLen = 1, Neck = 0) applying
    /// automatically to a freshly-constructed Genome. GenomeCharacterizationTests
    /// asserts this clamping matches the JS planOf() fixtures exactly,
    /// including its out-of-range inputs (e.g. elong: 10 -> 3.4, elong: -5 -> 0.35).
    /// </summary>
    public double PlanOf(string key) => key switch
    {
        "elong" => Clamp(Elong, BodyPlanRange.Elong.Min, BodyPlanRange.Elong.Max),
        "neck" => Clamp(Neck, BodyPlanRange.Neck.Min, BodyPlanRange.Neck.Max),
        "legLen" => Clamp(LegLen, BodyPlanRange.LegLen.Min, BodyPlanRange.LegLen.Max),
        _ => throw new ArgumentOutOfRangeException(nameof(key), key, "must be one of: elong, neck, legLen"),
    };

    /// <summary>
    /// Ported from cloneGenome(g) (line 492): deep-clones the body tree and
    /// brain, and re-derives elong/neck/legLen through PlanOf exactly as the
    /// JS does (so an out-of-range value acquired some other way is
    /// silently clamped back into range on every clone, not just on read).
    /// </summary>
    public Genome Clone()
    {
        var clone = new Genome
        {
            Body = Body.Clone(),
            Brain = Brain.Clone(),
            Hue = Hue,
            Scale = Scale,
            Life = Life,
            Elong = Elong,
            Neck = Neck,
            LegLen = LegLen,
        };
        clone.Elong = clone.PlanOf("elong");
        clone.Neck = clone.PlanOf("neck");
        clone.LegLen = clone.PlanOf("legLen");
        return clone;
    }

    private static double Clamp(double x, double lo, double hi) => x < lo ? lo : (x > hi ? hi : x);
}
