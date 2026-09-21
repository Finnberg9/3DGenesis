using Genesis.Core.Genome;
using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;

namespace Genesis.Core.Tests.Genome;

/// <summary>Shared helpers for comparing the C# genome object model against JS fixture DTOs.</summary>
internal static class TestSupport
{
    public static PartType MapPartType(string jsType) => jsType switch
    {
        "CORE" => PartType.Core,
        "MOUTH" => PartType.Mouth,
        "GUT" => PartType.Gut,
        "TAIL" => PartType.Tail,
        "EYE" => PartType.Eye,
        "LEG" => PartType.Leg,
        "FIN" => PartType.Fin,
        "WING" => PartType.Wing,
        "SPIKE" => PartType.Spike,
        "HORN" => PartType.Horn,
        "GLAND" => PartType.Gland,
        _ => throw new ArgumentOutOfRangeException(nameof(jsType), jsType, "unknown JS part type"),
    };

    public static MouthMode? MapMouthMode(string? jsMode) => jsMode switch
    {
        null => null,
        "herb" => MouthMode.Herb,
        "carn" => MouthMode.Carn,
        "scav" => MouthMode.Scav,
        _ => throw new ArgumentOutOfRangeException(nameof(jsMode), jsMode, "unknown JS mouth mode"),
    };

    /// <summary>
    /// Structurally compares a GenomeNode against its JS fixture equivalent.
    /// RotationY is compared against the JS "angle" field (its direct
    /// successor -- see GenomeNode's class remarks on the Phase 00 rotation
    /// axis addition); RotationX/RotationZ have no JS equivalent and are
    /// asserted to be exactly 0 (their required default for a genome ported
    /// from a pre-3-axis JS source).
    /// </summary>
    public static void AssertMatchesJs(JsGenomeNode expected, GenomeNode actual, string path = "root")
    {
        Assert.Equal(MapPartType(expected.Type), actual.Type, $"{path}: Type mismatch");
        Assert.Equal(expected.Size, actual.Size, $"{path}: Size mismatch ({expected.Size} vs {actual.Size})");
        Assert.Equal(MapMouthMode(expected.Mode), actual.Mode, $"{path}: Mode mismatch");
        Assert.Equal(expected.Repeat, actual.Repeat, $"{path}: Repeat mismatch");
        Assert.Equal(expected.Angle, actual.RotationY, $"{path}: RotationY (JS angle) mismatch");
        Assert.Equal(0.0, actual.RotationX, $"{path}: RotationX should default to 0 (no JS equivalent)");
        Assert.Equal(0.0, actual.RotationZ, $"{path}: RotationZ should default to 0 (no JS equivalent)");
        Assert.Equal(expected.Segs, actual.Segs, $"{path}: Segs mismatch");
        Assert.Equal(expected.SegTaper, actual.SegTaper, $"{path}: SegTaper mismatch");
        Assert.Equal(expected.Curl, actual.Curl, $"{path}: Curl mismatch");
        Assert.Equal(expected.Radial, actual.Radial, $"{path}: Radial mismatch");
        Assert.Equal(expected.Children.Count, actual.Children.Count, $"{path}: Children count mismatch");
        for (int i = 0; i < expected.Children.Count; i++)
        {
            AssertMatchesJs(expected.Children[i], actual.Children[i], $"{path}/children[{i}]");
        }
    }

    /// <summary>
    /// Brain weight comparison uses a tolerance because weights are drawn
    /// via Gauss(), which runs Math.Log/Cos/Sqrt through .NET's libm rather
    /// than V8's -- see GenesisRandom.Gauss()'s doc comment.
    /// </summary>
    public const double GaussTolerance = 1e-9;

    public static void AssertBrainMatchesJs(JsBrain expected, Genesis.Core.Genome.Brain actual, string path = "brain")
    {
        Assert.Equal(expected.Nodes.Count, actual.Nodes.Count, $"{path}: node count mismatch");
        for (int i = 0; i < expected.Nodes.Count; i++)
        {
            Assert.Equal(expected.Nodes[i].Innov, actual.Nodes[i].Innov, $"{path}/nodes[{i}]: innov mismatch");
            Assert.Equal(expected.Nodes[i].Rank, actual.Nodes[i].Rank, $"{path}/nodes[{i}]: rank mismatch");
        }

        Assert.Equal(expected.Conns.Count, actual.Conns.Count, $"{path}: conn count mismatch");
        for (int i = 0; i < expected.Conns.Count; i++)
        {
            var e = expected.Conns[i];
            var a = actual.Conns[i];
            Assert.Equal(e.Innov, a.Innov, $"{path}/conns[{i}]: innov mismatch");
            Assert.Equal(e.From, a.From, $"{path}/conns[{i}]: from mismatch");
            Assert.Equal(e.To, a.To, $"{path}/conns[{i}]: to mismatch");
            Assert.Equal(e.On, a.On, $"{path}/conns[{i}]: on mismatch");
            Assert.Equal(e.W, a.W, GaussTolerance, $"{path}/conns[{i}]: weight mismatch beyond libm tolerance");
        }
    }
}
