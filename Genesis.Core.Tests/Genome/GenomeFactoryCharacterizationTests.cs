using Genesis.Core.Rng;
using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;
using CoreGenome = Genesis.Core.Genome;

namespace Genesis.Core.Tests.Genome;

/// <summary>
/// End-to-end characterization of GenomeFactory.SeedHerbivore/NewBrain
/// against index.html's seedHerbivore/newBrain (lines 409, 500). This is
/// the strongest test in the suite: it exercises the full RNG-consumption
/// order across node construction AND brain construction in one seeded run,
/// so any drift in how many rng()/gauss() calls a step makes -- even one
/// that doesn't change that step's own visible output -- shows up as a
/// mismatch in everything drawn afterward (the brain's 358 weights, then hue).
/// </summary>
public sealed class GenomeFactoryCharacterizationTests
{
    [Fact]
    public void SeedHerbivore_MatchesJsFixture_ForEveryTestedSeed()
    {
        foreach (var seed in new uint[] { 1, 12345, 999999, 42, 7 })
        {
            var expected = FixtureLoader.Data.SeedHerbivores[seed.ToString()];
            var tracker = new CoreGenome.InnovationTracker();
            var rng = new GenesisRandom(seed);
            var genome = CoreGenome.GenomeFactory.SeedHerbivore(tracker, rng);

            TestSupport.AssertMatchesJs(expected.Body, genome.Body, $"seed {seed} body");
            TestSupport.AssertBrainMatchesJs(expected.Brain, genome.Brain, $"seed {seed} brain");
            Assert.Equal(expected.Hue, genome.Hue, $"seed {seed}: hue");
            Assert.Equal(expected.Scale, genome.Scale, $"seed {seed}: scale");
            Assert.Equal(expected.Life, genome.Life, $"seed {seed}: life");
            Assert.Equal(expected.Elong, genome.Elong, $"seed {seed}: elong");
            Assert.Equal(expected.Neck, genome.Neck, $"seed {seed}: neck");
            Assert.Equal(expected.LegLen, genome.LegLen, $"seed {seed}: legLen");
        }
    }

    [Fact]
    public void SeedHerbivore_BodyShape_IsAlwaysCoreWithFourFixedChildren()
    {
        var tracker = new CoreGenome.InnovationTracker();
        var rng = new GenesisRandom(31337u);
        var genome = CoreGenome.GenomeFactory.SeedHerbivore(tracker, rng);

        Assert.Equal(CoreGenome.PartType.Core, genome.Body.Type);
        Assert.Equal(4, genome.Body.Children.Count);
        Assert.Equal(CoreGenome.PartType.Mouth, genome.Body.Children[0].Type);
        Assert.Equal(CoreGenome.MouthMode.Herb, genome.Body.Children[0].Mode);
        Assert.Equal(0.7, genome.Body.Children[0].Size);
        Assert.Equal(CoreGenome.PartType.Gut, genome.Body.Children[1].Type);
        Assert.Equal(0.8, genome.Body.Children[1].Size);
        Assert.Equal(CoreGenome.PartType.Leg, genome.Body.Children[2].Type);
        Assert.Equal(0.7, genome.Body.Children[2].Size);
        Assert.Equal(2, genome.Body.Children[2].Repeat);
        Assert.Equal(CoreGenome.PartType.Eye, genome.Body.Children[3].Type);
        Assert.Equal(0.6, genome.Body.Children[3].Size);
        Assert.Equal(1, genome.Body.Children[3].Repeat);
    }

    [Fact]
    public void SeedHerbivore_BrainTopology_HasExpectedInitialShape()
    {
        var tracker = new CoreGenome.InnovationTracker();
        var rng = new GenesisRandom(2718u);
        var genome = CoreGenome.GenomeFactory.SeedHerbivore(tracker, rng);

        // 10 hidden nodes; (26 inputs + 1 bias) * 10 + 8 outputs * 10 + 8 direct bias->output = 358 conns.
        Assert.Equal(CoreGenome.BrainConstants.InitialHiddenCount, genome.Brain.Nodes.Count);
        int expectedConns = (CoreGenome.BrainConstants.In + 1) * CoreGenome.BrainConstants.InitialHiddenCount
                             + CoreGenome.BrainConstants.Out * CoreGenome.BrainConstants.InitialHiddenCount
                             + CoreGenome.BrainConstants.Out;
        Assert.Equal(358, expectedConns, "sanity check on the arithmetic itself");
        Assert.Equal(expectedConns, genome.Brain.Conns.Count);
        Assert.True(genome.Brain.Conns.TrueForAll(c => c.On), "every initial connection must start enabled");
    }

    [Fact]
    public void SeedHerbivore_IsDeterministic_SameSeedProducesIdenticalGenome()
    {
        var g1 = CoreGenome.GenomeFactory.SeedHerbivore(new CoreGenome.InnovationTracker(), new GenesisRandom(555u));
        var g2 = CoreGenome.GenomeFactory.SeedHerbivore(new CoreGenome.InnovationTracker(), new GenesisRandom(555u));

        Assert.Equal(g1.Hue, g2.Hue);
        Assert.Equal(g1.Brain.Conns.Count, g2.Brain.Conns.Count);
        for (int i = 0; i < g1.Brain.Conns.Count; i++)
        {
            Assert.Equal(g1.Brain.Conns[i].W, g2.Brain.Conns[i].W, "same seed must produce byte-identical brain weights (no hidden global state leaking between calls)");
        }
    }
}
