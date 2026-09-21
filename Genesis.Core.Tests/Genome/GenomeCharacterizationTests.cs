using Genesis.Core.Rng;
using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;
using CoreGenome = Genesis.Core.Genome;

namespace Genesis.Core.Tests.Genome;

/// <summary>Characterizes Genome.PlanOf/Clone against index.html's planOf (line 483) and cloneGenome (line 492).</summary>
public sealed class GenomeCharacterizationTests
{
    [Fact]
    public void PlanOf_MatchesJsFixture_IncludingOutOfRangeClamping()
    {
        foreach (var sample in FixtureLoader.Data.PlanOfSamples)
        {
            var genome = new CoreGenome.Genome
            {
                Body = CoreGenome.GenomeNode.NewCore(),
                Brain = new CoreGenome.Brain(),
            };
            // JS planOf's "absent -> default" behavior (0 for neck, 1 for
            // elong/legLen) is captured by Genome's own property defaults;
            // only apply the fixture's override when the sample's g object
            // actually specifies one, so the "g: {}" cases exercise those
            // defaults rather than overwriting them with 0.
            if (sample.G.TryGetValue("elong", out var elongOverride)) genome.Elong = elongOverride;
            if (sample.G.TryGetValue("neck", out var neckOverride)) genome.Neck = neckOverride;
            if (sample.G.TryGetValue("legLen", out var legLenOverride)) genome.LegLen = legLenOverride;

            double actual = genome.PlanOf(sample.Key);
            Assert.Equal(sample.Result, actual, $"PlanOf({sample.Key}) with g={string.Join(",", sample.G)}");
        }
    }

    [Fact]
    public void PlanOf_UnknownKey_Throws()
    {
        var genome = new CoreGenome.Genome { Body = CoreGenome.GenomeNode.NewCore(), Brain = new CoreGenome.Brain() };
        Assert.Throws<ArgumentOutOfRangeException>(() => genome.PlanOf("wingspan"));
    }

    [Fact]
    public void Clone_IsIndependent_MatchesJsFixture()
    {
        var fixture = FixtureLoader.Data.CloneIndependence;
        var tracker = new CoreGenome.InnovationTracker();
        var rng = new GenesisRandom(12345u);
        var genome = CoreGenome.GenomeFactory.SeedHerbivore(tracker, rng);

        Assert.Equal(fixture.OriginalSize, genome.Body.Children[0].Size, "sanity: pre-clone size should match JS fixture");

        var clone = genome.Clone();
        clone.Body.Children[0].Size = 999.5;

        Assert.True(fixture.OriginalUnchanged, "fixture sanity: JS's own original was unchanged by cloning");
        Assert.Equal(fixture.OriginalSize, genome.Body.Children[0].Size, "mutating the clone's body must not affect the original genome");
        Assert.Equal(999.5, clone.Body.Children[0].Size, "clone should reflect its own mutation");
    }

    [Fact]
    public void Clone_DeepClonesBrain_NotJustBody()
    {
        var tracker = new CoreGenome.InnovationTracker();
        var rng = new GenesisRandom(555u);
        var genome = CoreGenome.GenomeFactory.SeedHerbivore(tracker, rng);

        var clone = genome.Clone();
        clone.Brain.Conns[0].W = 12345.0;

        Assert.True(genome.Brain.Conns[0].W != 12345.0, "mutating a clone's brain connection must not affect the original's brain");
        Assert.True(!ReferenceEquals(genome.Brain, clone.Brain), "clone must have its own Brain instance");
        Assert.True(!ReferenceEquals(genome.Brain.Conns, clone.Brain.Conns), "clone must have its own Conns list");
    }
}
