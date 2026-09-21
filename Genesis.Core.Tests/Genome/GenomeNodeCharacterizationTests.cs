using Genesis.Core.Rng;
using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;
using CoreGenome = Genesis.Core.Genome;

namespace Genesis.Core.Tests.Genome;

/// <summary>Characterizes GenomeNode.NewCore/NewNode/Clone against index.html's newCore/newNode/cloneNode (lines 453-471).</summary>
public sealed class GenomeNodeCharacterizationTests
{
    [Fact]
    public void NewCore_MatchesJsFixture()
    {
        TestSupport.AssertMatchesJs(FixtureLoader.Data.NewCoreSample, CoreGenome.GenomeNode.NewCore());
    }

    [Fact]
    public void NewNode_MatchesJsFixture_ForEveryPartType()
    {
        // Fixture was drawn from ONE shared rng(seed=2024) across all ten
        // part types, in this exact order -- so the rng must be re-created
        // fresh and driven through all ten calls in the same order for the
        // draws to line up. This exercises the "how many rng() calls does
        // NewNode consume, and in what order" contract, not just NewNode in
        // isolation.
        var rng = new GenesisRandom(2024u);
        foreach (var sample in FixtureLoader.Data.NewNodeSamples)
        {
            var type = TestSupport.MapPartType(sample.Type);
            var node = CoreGenome.GenomeNode.NewNode(type, rng);
            TestSupport.AssertMatchesJs(sample.Node, node, $"NewNode({sample.Type})");
        }
    }

    [Fact]
    public void CloneNode_IsDeepAndIndependent_MatchesJsFixture()
    {
        var fixture = FixtureLoader.Data.CloneNodeIndependence;
        var rng = new GenesisRandom(2024u);
        var orig = CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Leg, rng);
        orig.Children.Add(CoreGenome.GenomeNode.NewNode(CoreGenome.PartType.Spike, rng));

        Assert.Equal(fixture.OriginalSize, orig.Size, "sanity: original root size should match JS fixture before cloning");
        Assert.Equal(fixture.OriginalChildSize, orig.Children[0].Size, "sanity: original child size should match JS fixture before cloning");

        var clone = orig.Clone();
        clone.Size = -1;
        clone.Children[0].Size = -1;

        Assert.Equal(fixture.OriginalSize, orig.Size, "mutating the clone must not affect the original root");
        Assert.Equal(fixture.OriginalChildSize, orig.Children[0].Size, "mutating the clone must not affect the original's child");
        Assert.Equal(fixture.CloneSize, clone.Size, "clone root should reflect the post-mutation value");
        Assert.Equal(fixture.CloneChildSize, clone.Children[0].Size, "clone child should reflect the post-mutation value");
    }

    [Fact]
    public void NewNode_RotationAxes_AllDefaultToZero()
    {
        // Phase 00 deliberate deviation: no JS fixture for this (see
        // GenomeNode's class remarks) -- this directly verifies the new
        // 3-axis rotation genes start inert on every freshly-constructed
        // node, so porting the JS's single-axis "angle" behavior forward
        // doesn't change until mutation/selection (a later system) starts
        // driving RotationX/RotationZ.
        var rng = new GenesisRandom(1u);
        foreach (var type in Enum.GetValues<CoreGenome.PartType>())
        {
            var node = type == CoreGenome.PartType.Core
                ? CoreGenome.GenomeNode.NewCore()
                : CoreGenome.GenomeNode.NewNode(type, rng);
            Assert.Equal(0.0, node.RotationX, $"{type}: RotationX should default to 0");
            Assert.Equal(0.0, node.RotationY, $"{type}: RotationY should default to 0");
            Assert.Equal(0.0, node.RotationZ, $"{type}: RotationZ should default to 0");
        }
    }
}
