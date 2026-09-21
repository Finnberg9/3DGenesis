namespace Genesis.Core.Genome;

/// <summary>
/// A NEAT hidden node gene. Ported from index.html's brain node shape
/// { innov, rank } (e.g. newBrain, line 413). "rank" is the node's position
/// in feedforward evaluation order (0 = input-adjacent, RankOut = output);
/// it is meaningful only once the brain/behavior system's forward pass and
/// mutation are ported, but the field is part of the data shape a genome
/// carries, so it lives here.
/// </summary>
public sealed class NeatNode
{
    public required int Innov { get; set; }
    public double Rank { get; set; }

    public NeatNode Clone() => new() { Innov = Innov, Rank = Rank };
}

/// <summary>
/// A NEAT connection gene. Ported from index.html's brain connection shape
/// { innov, from, to, w, on } (e.g. newBrain, line 414).
/// </summary>
public sealed class NeatConn
{
    public required int Innov { get; set; }
    public required int From { get; set; }
    public required int To { get; set; }
    public double W { get; set; }
    public bool On { get; set; } = true;

    public NeatConn Clone() => new() { Innov = Innov, From = From, To = To, W = W, On = On };
}

/// <summary>
/// A creature's brain: variable-topology node/connection gene graph. Ported
/// from index.html's brain shape { nodes, conns } (line 419). Input/output
/// node ids are fixed and implicit (see BrainConstants) and never stored
/// here, matching the JS.
///
/// SCOPE NOTE: the JS brain object also carries a compiled "_plan" cache
/// (compileBrain, line 389) used only by brainForward's hot per-tick
/// evaluation loop. That is a forward-pass optimization detail, not part of
/// the genome's actual heritable data, so it is intentionally NOT ported
/// onto this type -- it will be added by the brain/behavior system, which
/// owns brainForward, when that system is ported.
/// </summary>
public sealed class Brain
{
    public List<NeatNode> Nodes { get; set; } = new();
    public List<NeatConn> Conns { get; set; } = new();

    /// <summary>
    /// Ported from cloneBrain(brain) (line 423), minus the compileBrain call
    /// (see class remarks: the compiled plan is a brain/behavior-system
    /// concern, deferred). Deep-clones both gene lists so mutating the clone
    /// can never affect the source brain.
    /// </summary>
    public Brain Clone() => new()
    {
        Nodes = Nodes.Select(n => n.Clone()).ToList(),
        Conns = Conns.Select(c => c.Clone()).ToList(),
    };
}
