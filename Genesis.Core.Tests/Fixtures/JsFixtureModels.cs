using System.Text.Json.Serialization;

namespace Genesis.Core.Tests.Fixtures;

// DTOs matching /tmp/gen_fixtures.js's JSON.stringify output exactly (see
// FixtureLoader.cs for the loader, and README notes in
// Genesis.Core.Tests.csproj / PHASE00_PROGRESS.md for how these fixtures
// were produced: by running the REAL, unmodified game JS, extracted
// verbatim from index.html, under Node -- never hand-derived).

public sealed class JsGenomeNode
{
    public string Type { get; set; } = "";
    public double Size { get; set; }
    public string? Mode { get; set; }
    public int Repeat { get; set; }
    public double Angle { get; set; }
    public int Segs { get; set; }
    public double SegTaper { get; set; }
    public double Curl { get; set; }
    public int Radial { get; set; }
    public List<JsGenomeNode> Children { get; set; } = new();
}

public sealed class JsNeatNode
{
    public int Innov { get; set; }
    public double Rank { get; set; }
}

public sealed class JsNeatConn
{
    public int Innov { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public double W { get; set; }
    public bool On { get; set; }
}

public sealed class JsBrain
{
    public List<JsNeatNode> Nodes { get; set; } = new();
    public List<JsNeatConn> Conns { get; set; } = new();
}

public sealed class JsGenome
{
    public JsGenomeNode Body { get; set; } = new();
    public JsBrain Brain { get; set; } = new();
    public double Hue { get; set; }
    public double Scale { get; set; }
    public double Life { get; set; }
    public double Elong { get; set; }
    public double Neck { get; set; }
    public double LegLen { get; set; }
}

public sealed class JsDepthEntry
{
    public string Type { get; set; } = "";
    public int Depth { get; set; }
}

public sealed class JsCloneIndependence
{
    public bool OriginalUnchanged { get; set; }
    public bool CloneChanged { get; set; }
    public double OriginalSize { get; set; }
}

public sealed class JsNormalizeGenomeFixture
{
    public JsGenomeNode Before { get; set; } = new();
    public JsGenomeNode After { get; set; } = new();
    public int AfterNodeCount { get; set; }
    public Dictionary<string, int> AfterTypeCounts { get; set; } = new();
}

public sealed class JsNewNodeSample
{
    public string Type { get; set; } = "";
    public JsGenomeNode Node { get; set; } = new();
}

public sealed class JsCloneNodeIndependence
{
    public double OriginalSize { get; set; }
    public double OriginalChildSize { get; set; }
    public double CloneSize { get; set; }
    public double CloneChildSize { get; set; }
}

public sealed class JsPlanOfSample
{
    public Dictionary<string, double> G { get; set; } = new();
    public string Key { get; set; } = "";
    public double Result { get; set; }
}

public sealed class JsPartDef
{
    public double Upkeep { get; set; }
    public int MaxRepeat { get; set; }
    public bool Singleton { get; set; }
}

public sealed class JsConstants
{
    // PropertyNameCaseInsensitive only ignores case, it does not bridge
    // snake_case JSON keys to PascalCase properties -- the JS fixture dump
    // uses the JS source's own ALL_CAPS_WITH_UNDERSCORES constant names
    // verbatim (PART_TYPES, MOUTH_MODES, MAX_SEGS, MAX_RADIAL), so those
    // four need an explicit mapping or they silently deserialize to
    // empty/default (caught by ConstantsCharacterizationTests, which is
    // exactly what it's for).
    public Dictionary<string, JsPartDef> Part { get; set; } = new();
    [JsonPropertyName("PART_TYPES")] public List<string> PartTypes { get; set; } = new();
    [JsonPropertyName("MOUTH_MODES")] public List<string> MouthModes { get; set; } = new();
    public List<string> Singleton { get; set; } = new();
    public Dictionary<string, int> Caps { get; set; } = new();
    [JsonPropertyName("MAX_SEGS")] public int MaxSegs { get; set; }
    [JsonPropertyName("MAX_RADIAL")] public int MaxRadial { get; set; }
}

public sealed class GenomeFixtures
{
    public Dictionary<string, double[]> RngSequences { get; set; } = new();
    public Dictionary<string, double[]> GaussSequences { get; set; } = new();
    public Dictionary<string, JsGenome> SeedHerbivores { get; set; } = new();
    public Dictionary<string, int> NodeCounts { get; set; } = new();
    public Dictionary<string, List<JsDepthEntry>> CollectedDepths { get; set; } = new();
    public JsCloneIndependence CloneIndependence { get; set; } = new();
    public JsNormalizeGenomeFixture NormalizeGenome { get; set; } = new();
    public List<JsNewNodeSample> NewNodeSamples { get; set; } = new();
    public JsGenomeNode NewCoreSample { get; set; } = new();
    public JsCloneNodeIndependence CloneNodeIndependence { get; set; } = new();
    public List<JsPlanOfSample> PlanOfSamples { get; set; } = new();
    public Dictionary<string, double[]> Plan { get; set; } = new();
    public JsConstants Constants { get; set; } = new();
}
