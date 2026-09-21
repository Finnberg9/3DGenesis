using Genesis.Core.Rng;
using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;
using CoreGenome = Genesis.Core.Genome;

namespace Genesis.Core.Tests.Genome;

/// <summary>Characterizes GenomeTree.CountNodes/CollectNodes/NormalizeGenome against index.html (lines 472-525).</summary>
public sealed class GenomeTreeCharacterizationTests
{
    private static CoreGenome.Genome BuildSeedHerbivore(uint seed)
    {
        var tracker = new CoreGenome.InnovationTracker();
        var rng = new GenesisRandom(seed);
        return CoreGenome.GenomeFactory.SeedHerbivore(tracker, rng);
    }

    [Fact]
    public void CountNodes_MatchesJsFixture_ForSeededHerbivores()
    {
        foreach (var seed in new uint[] { 1, 12345, 999999 })
        {
            var genome = BuildSeedHerbivore(seed);
            int expected = FixtureLoader.Data.NodeCounts[seed.ToString()];
            Assert.Equal(expected, CoreGenome.GenomeTree.CountNodes(genome.Body), $"seed {seed}");
        }
    }

    [Fact]
    public void CollectNodes_MatchesJsFixture_TraversalOrderAndDepths()
    {
        foreach (var seed in new uint[] { 1, 12345, 999999 })
        {
            var genome = BuildSeedHerbivore(seed);
            var expected = FixtureLoader.Data.CollectedDepths[seed.ToString()];
            var actual = CoreGenome.GenomeTree.CollectNodes(genome.Body, 0);

            Assert.Equal(expected.Count, actual.Count, $"seed {seed}: entry count");
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(TestSupport.MapPartType(expected[i].Type), actual[i].Node.Type, $"seed {seed}, entry {i}: type");
                Assert.Equal(expected[i].Depth, actual[i].Depth, $"seed {seed}, entry {i}: depth");
            }
        }
    }

    [Fact]
    public void NormalizeGenome_KeepsLastDocumentOrderSingleton_DropsEarlierOnesAndTheirSubtrees()
    {
        // Hand-builds the exact tree the JS fixture generator built (three
        // MOUTH nodes at different depths -- one nested inside a duplicate
        // GUT -- and two GUT nodes, plus a duplicated non-singleton EYE
        // which must survive both instances untouched), then asserts our
        // NormalizeGenome produces byte-for-byte the same surviving tree the
        // real JS normalizeGenome produced. See GenomeTree.NormalizeGenome's
        // doc comment for the exact rule this exercises (global "seen" set,
        // right-to-left scan, dead subtrees never visited).
        var rng = new GenesisRandom(9001u);
        var core = CoreGenome.GenomeNode.NewCore();
        var m1 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Mouth, rng); m1.Mode = CoreGenome.MouthMode.Herb;
        var m2 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Mouth, rng); m2.Mode = CoreGenome.MouthMode.Carn;
        var g1 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Gut, rng);
        var g2 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Gut, rng);
        var e1 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Eye, rng);
        var e2 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Eye, rng);
        var m3 = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Mouth, rng); m3.Mode = CoreGenome.MouthMode.Scav;
        g1.Children.Add(m3);
        core.Children.Add(m1);
        core.Children.Add(g1);
        core.Children.Add(m2);
        core.Children.Add(g2);
        core.Children.Add(e1);
        core.Children.Add(e2);

        // Sanity: the tree we just built matches the JS fixture's "before" snapshot exactly.
        TestSupport.AssertMatchesJs(FixtureLoader.Data.NormalizeGenome.Before, core);

        var normalized = CoreGenome.GenomeTree.NormalizeGenome(core);

        TestSupport.AssertMatchesJs(FixtureLoader.Data.NormalizeGenome.After, normalized);
        Assert.Equal(FixtureLoader.Data.NormalizeGenome.AfterNodeCount, CoreGenome.GenomeTree.CountNodes(normalized));

        var typeCounts = CoreGenome.GenomeTree.TypeCounts(normalized);
        foreach (var (jsType, expectedCount) in FixtureLoader.Data.NormalizeGenome.AfterTypeCounts)
        {
            var type = TestSupport.MapPartType(jsType);
            Assert.Equal(expectedCount, typeCounts.GetValueOrDefault(type), $"type count for {jsType}");
        }
    }

    [Fact]
    public void NormalizeGenome_NeverLeavesMoreThanOneOfAnySingleton()
    {
        // Property-style guard on top of the exact fixture match above: no
        // matter how many singletons are stacked in, the invariant
        // normalizeGenome exists to enforce must hold afterward.
        var rng = new GenesisRandom(4242u);
        var core = CoreGenome.GenomeNode.NewCore();
        for (int i = 0; i < 5; i++) core.Children.Add(CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Mouth, rng));
        for (int i = 0; i < 3; i++) core.Children.Add(CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Tail, rng));

        CoreGenome.GenomeTree.NormalizeGenome(core);
        var counts = CoreGenome.GenomeTree.TypeCounts(core);
        Assert.True(counts.GetValueOrDefault(CoreGenome.PartType.Mouth) <= 1, "at most one MOUTH must survive");
        Assert.True(counts.GetValueOrDefault(CoreGenome.PartType.Tail) <= 1, "at most one TAIL must survive");
    }
}
