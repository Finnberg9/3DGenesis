using Genesis.Core.Rng;
using Genesis.Core.Tests.Fixtures;
using Genesis.Core.Tests.TestKit;

namespace Genesis.Core.Tests.Genome;

/// <summary>
/// Characterizes GenesisRandom against index.html's makeRNG(seed) (line
/// 236). This generator underlies every single draw in the entire
/// simulation, so exact bit-for-bit reproduction (not just "close") is the
/// whole point: a fixed seed must produce an identical simulation. Fixtures
/// were captured by running the real JS under Node (see
/// PHASE00_PROGRESS.md); no value here was hand-derived from reading the
/// source.
/// </summary>
public sealed class RngCharacterizationTests
{
    [Fact]
    public void NextDouble_MatchesJs_ForOrdinarySeeds()
    {
        foreach (var seed in new uint[] { 1, 12345, 999999, 42 })
        {
            var expected = FixtureLoader.Data.RngSequences[seed.ToString()];
            var rng = new GenesisRandom(seed);
            for (int i = 0; i < expected.Length; i++)
            {
                double actual = rng.NextDouble();
                Assert.Equal(expected[i], actual, $"seed {seed}, draw {i}: expected {expected[i]:R}, got {actual:R}");
            }
        }
    }

    [Fact]
    public void NextDouble_MatchesJs_ForZeroSeed_SubstitutedWithOne()
    {
        // JS: (seed >>> 0) || 1 -- a zero seed becomes 1, verified against
        // the actual JS output for seed=0 (not merely asserted equal to the
        // seed=1 fixture by assumption).
        var expected = FixtureLoader.Data.RngSequences["0"];
        var rng = new GenesisRandom(0u);
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i], rng.NextDouble(), $"draw {i}");
        }
    }

    [Fact]
    public void NextDouble_MatchesJs_AtUint32Boundary()
    {
        // 4294967295 = 2^32 - 1 (uint.MaxValue) and 2147483648 = 2^31 (first
        // value that overflows a signed 32-bit int) are the two seeds most
        // likely to break a careless int/uint port -- both are exercised
        // here against real JS output.
        foreach (var seed in new uint[] { 4294967295u, 2147483648u })
        {
            var expected = FixtureLoader.Data.RngSequences[seed.ToString()];
            var rng = new GenesisRandom(seed);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], rng.NextDouble(), $"seed {seed}, draw {i}");
            }
        }
    }

    [Fact]
    public void NextDouble_AlwaysInZeroToOneRange()
    {
        var rng = new GenesisRandom(777u);
        for (int i = 0; i < 10000; i++)
        {
            double v = rng.NextDouble();
            Assert.InRange(v, 0.0, 1.0 - 1e-15, $"draw {i} = {v}");
        }
    }

    [Fact]
    public void Gauss_MatchesJs_WithinLibmTolerance()
    {
        foreach (var seed in new uint[] { 1, 12345, 999999 })
        {
            var expected = FixtureLoader.Data.GaussSequences[seed.ToString()];
            var rng = new GenesisRandom(seed);
            for (int i = 0; i < expected.Length; i++)
            {
                double actual = rng.Gauss();
                Assert.Equal(expected[i], actual, TestSupport.GaussTolerance, $"seed {seed}, draw {i}");
            }
        }
    }
}
