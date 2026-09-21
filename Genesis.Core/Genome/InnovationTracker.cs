namespace Genesis.Core.Genome;

/// <summary>
/// World-scoped NEAT innovation-id bookkeeping. Ported from index.html's
/// innovForConn/nextHiddenId (lines 357-373) and the world fields they close
/// over (world.nextNodeInnov, world.nextConnInnov, world.innovCache -- see
/// makeWorld, line ~1461: `nextNodeInnov: 1000, nextConnInnov: 1, innovCache: new Map()`).
///
/// Purpose (from the JS source comment, line 306): every hidden node and
/// every connection carries a stable "innovation" id, assigned once, the
/// first time that exact structural feature (an edge from A to B; a
/// particular seed hidden node) occurs, and cached so the same feature
/// arising independently in two different genomes (e.g. every founder
/// genome's identical initial topology) still gets the SAME id -- this is
/// what lets brains from different lineages be aligned for NEAT-style
/// crossover/distance later.
///
/// SCOPE NOTE: only InnovForConn and NextHiddenId are ported here, because
/// those are all that seedHerbivore()/NewBrain() (this system's job) call.
/// innovForSplit (the add-node structural mutation's id assignment) is used
/// exclusively by mutateBrain, which belongs to the not-yet-ported
/// mutation/selection system -- it will be added to this class, not
/// duplicated elsewhere, when that system's characterization tests are written.
/// </summary>
public sealed class InnovationTracker
{
    /// <summary>JS: world.nextNodeInnov, seeded at 1000.</summary>
    public int NextNodeInnov { get; private set; } = 1000;

    /// <summary>JS: world.nextConnInnov, seeded at 1.</summary>
    public int NextConnInnov { get; private set; } = 1;

    // JS uses a single Map<string, id|id[]> shared across innovForConn,
    // innovForSplit and nextHiddenId, keyed by distinctly-prefixed strings
    // ("from:to", "split:connInnov", caller-supplied seed keys) so the three
    // key spaces never collide. Replicated with one Dictionary<string, int>
    // here since only integer connection/hidden ids are cached in this
    // system's scope (innovForSplit's int[] triples are deferred, see above).
    private readonly Dictionary<string, int> _cache = new();

    /// <summary>
    /// Ported from innovForConn(world, from, to) (line 357): returns the
    /// stable innovation id for the directed edge from -&gt; to, assigning a
    /// fresh one (and caching it) the first time this exact edge is requested.
    /// </summary>
    public int InnovForConn(int from, int to)
    {
        string key = $"{from}:{to}";
        if (_cache.TryGetValue(key, out int id)) return id;
        id = NextConnInnov++;
        _cache[key] = id;
        return id;
    }

    /// <summary>
    /// Ported from nextHiddenId(world, seedKey) (line 369): returns the
    /// stable node-innovation id for an arbitrary caller-chosen key (used by
    /// newBrain with keys "seed:0".."seed:9" for the initial hidden nodes),
    /// assigning a fresh one the first time this exact key is requested.
    /// </summary>
    public int NextHiddenId(string seedKey)
    {
        if (_cache.TryGetValue(seedKey, out int id)) return id;
        id = NextNodeInnov++;
        _cache[seedKey] = id;
        return id;
    }
}
