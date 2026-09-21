using Genesis.Core.Rng;

namespace Genesis.Core.Genome;

/// <summary>
/// Genome construction. Ported verbatim from index.html: newBrain (line
/// 409) and seedHerbivore (line 500).
///
/// RNG-ORDER WARNING: every method here draws from the shared
/// <see cref="GenesisRandom"/> in EXACTLY the order and count the JS source
/// does, including draws whose result is immediately discarded (e.g.
/// seedHerbivore builds each node via NewNode -- which always draws for
/// size, and for MOUTH also draws for mode -- and then overwrites those
/// fields with fixed herbivore-template values; the draws still happened
/// and still advanced the shared RNG in the JS build, so skipping them here
/// would desynchronize every subsequent draw for the rest of a fixed-seed
/// run). This is verified end-to-end by
/// GenomeFactoryCharacterizationTests.SeedHerbivore_MatchesJsFixture, which
/// compares the full resulting genome (including every brain connection
/// weight) against real JS output for five different seeds.
/// </summary>
public static class GenomeFactory
{
    /// <summary>
    /// Ported from newBrain(world, rng) (line 409): builds the initial NEAT
    /// topology every founder genome starts with -- BrainConstants.InitialHiddenCount
    /// (10) hidden nodes, each densely connected from every input, the bias
    /// pseudo-input, and to every output, plus a direct bias-to-output
    /// connection for every output. Node ids come from
    /// <paramref name="innovations"/>.NextHiddenId with keys "seed:0".."seed:9"
    /// (matching JS `'seed:' + h`); connection ids come from InnovForConn.
    /// </summary>
    public static Brain NewBrain(InnovationTracker innovations, GenesisRandom rng)
    {
        var nodes = new List<NeatNode>();
        var conns = new List<NeatConn>();

        for (int h = 0; h < BrainConstants.InitialHiddenCount; h++)
        {
            int hId = innovations.NextHiddenId($"seed:{h}");
            nodes.Add(new NeatNode { Innov = hId, Rank = 0.5 });

            for (int i = 0; i < BrainConstants.In; i++)
            {
                conns.Add(new NeatConn
                {
                    Innov = innovations.InnovForConn(i, hId),
                    From = i,
                    To = hId,
                    W = rng.Gauss() * 0.05,
                    On = true,
                });
            }

            conns.Add(new NeatConn
            {
                Innov = innovations.InnovForConn(BrainConstants.BiasId, hId),
                From = BrainConstants.BiasId,
                To = hId,
                W = rng.Gauss() * 0.05,
                On = true,
            });

            for (int k = 0; k < BrainConstants.Out; k++)
            {
                int outId = BrainConstants.OutputBase + k;
                conns.Add(new NeatConn
                {
                    Innov = innovations.InnovForConn(hId, outId),
                    From = hId,
                    To = outId,
                    W = rng.Gauss() * 0.05,
                    On = true,
                });
            }
        }

        for (int k = 0; k < BrainConstants.Out; k++)
        {
            int outId = BrainConstants.OutputBase + k;
            conns.Add(new NeatConn
            {
                Innov = innovations.InnovForConn(BrainConstants.BiasId, outId),
                From = BrainConstants.BiasId,
                To = outId,
                W = rng.Gauss() * 0.05,
                On = true,
            });
        }

        return new Brain { Nodes = nodes, Conns = conns };
    }

    /// <summary>
    /// Ported from seedHerbivore(world, rng) (line 500): the founder genome
    /// every new world's initial population is drawn from -- a CORE with a
    /// herbivorous MOUTH, a GUT, a pair of LEGs, and one EYE, plus a fresh
    /// NEAT brain and the herbivore-template scalar genes.
    /// </summary>
    public static Genome SeedHerbivore(InnovationTracker innovations, GenesisRandom rng)
    {
        var core = GenomeNode.NewCore();

        var mouth = GenomeNode.NewNode(PartType.Mouth, rng);
        mouth.Mode = MouthMode.Herb;
        mouth.Size = 0.7;

        var gut = GenomeNode.NewNode(PartType.Gut, rng);
        gut.Size = 0.8;

        var leg = GenomeNode.NewNode(PartType.Leg, rng);
        leg.Size = 0.7;
        leg.Repeat = 2;

        var eye = GenomeNode.NewNode(PartType.Eye, rng);
        eye.Size = 0.6;
        eye.Repeat = 1;

        core.Children.Add(mouth);
        core.Children.Add(gut);
        core.Children.Add(leg);
        core.Children.Add(eye);

        var brain = NewBrain(innovations, rng);
        double hue = rng.NextDouble();

        return new Genome
        {
            Body = core,
            Brain = brain,
            Hue = hue,
            Scale = 1.0,
            Life = 1.0,
            Elong = 1.0,
            Neck = 0.2,
            LegLen = 1.0,
        };
    }
}
