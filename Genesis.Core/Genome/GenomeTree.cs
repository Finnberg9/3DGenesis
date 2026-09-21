namespace Genesis.Core.Genome;

/// <summary>One entry of a depth-first genome-tree walk (JS: {node, depth}).</summary>
public readonly record struct GenomeNodeEntry(GenomeNode Node, int Depth);

/// <summary>
/// Pure structural operations on a body-plan tree. Ported verbatim from
/// index.html (lines 472-525): countNodes, collectNodes, normalizeGenome.
/// These are read/repair operations on the tree shape only -- they never
/// draw from an RNG and are exact, deterministic ports with no floating
/// point sensitivity (only integer counts and a boolean/set membership
/// prune), so tests assert exact equality against JS fixtures throughout.
/// </summary>
public static class GenomeTree
{
    /// <summary>
    /// Ported from countNodes(n) (line 472): counts n itself plus every
    /// descendant, recursively.
    /// </summary>
    public static int CountNodes(GenomeNode n)
    {
        int c = 1;
        foreach (var ch in n.Children) c += CountNodes(ch);
        return c;
    }

    /// <summary>
    /// Ported from collectNodes(n, depth, out) (line 473): pre-order,
    /// left-to-right depth-first walk starting at n with the given depth,
    /// including n itself. Note this traversal order is DIFFERENT from
    /// NormalizeGenome's internal walk (which visits children right-to-left
    /// and never visits the tree root passed to it) -- the two are separate,
    /// independently-ordered walks in the original JS and must stay that way.
    /// </summary>
    public static List<GenomeNodeEntry> CollectNodes(GenomeNode n, int depth, List<GenomeNodeEntry>? into = null)
    {
        var result = into ?? new List<GenomeNodeEntry>();
        result.Add(new GenomeNodeEntry(n, depth));
        foreach (var ch in n.Children) CollectNodes(ch, depth + 1, result);
        return result;
    }

    /// <summary>
    /// Ported from normalizeGenome(body) (line 515): enforces singleton
    /// cardinality (at most one MOUTH, one GUT, one TAIL anywhere in the
    /// whole tree) by mutating the tree in place, then returns it.
    ///
    /// Exact behavior, preserved from the JS (verified against
    /// GenomeTreeCharacterizationTests.NormalizeGenome_MatchesJsFixture,
    /// which hand-builds a tree with THREE MOUTH nodes at different depths
    /// including one nested inside a duplicate GUT):
    ///
    ///  1. "seen" is tracked GLOBALLY across the whole tree (one HashSet for
    ///     the entire recursive walk), not per-parent -- two singleton-typed
    ///     nodes anywhere in the tree, even in unrelated branches, still
    ///     collide.
    ///  2. Each node's children are scanned RIGHT-TO-LEFT (last child
    ///     first). Combined with global "seen", this means the traversal
    ///     encounters -- and therefore KEEPS -- whichever occurrence of a
    ///     singleton type is LAST in left-to-right document order, and
    ///     deletes every occurrence encountered after that point in the
    ///     right-to-left scan (i.e. every occurrence earlier in document
    ///     order).
    ///  3. When a duplicate is deleted, its entire subtree is dropped
    ///     without being visited at all: the JS does `splice(...); continue`
    ///     which skips the walk(ch) recursion for that child. A second
    ///     singleton nested inside an already-doomed duplicate is never
    ///     independently checked -- it is simply discarded along with its
    ///     dead parent.
    ///  4. The FIRST (surviving, rightmost-in-document-order) occurrence of
    ///     each singleton type has its Repeat forced to 1, even if it had
    ///     drifted to something else beforehand.
    /// </summary>
    public static GenomeNode NormalizeGenome(GenomeNode body)
    {
        var seen = new HashSet<PartType>();
        Walk(body, seen);
        return body;

        static void Walk(GenomeNode n, HashSet<PartType> seen)
        {
            for (int i = n.Children.Count - 1; i >= 0; i--)
            {
                var ch = n.Children[i];
                if (PartVocab.IsSingleton(ch.Type))
                {
                    if (seen.Contains(ch.Type))
                    {
                        n.Children.RemoveAt(i);
                        continue;
                    }
                    seen.Add(ch.Type);
                    ch.Repeat = 1;
                }
                Walk(ch, seen);
            }
        }
    }

    /// <summary>
    /// Ported from typeCounts(body) (line 511): a flat count of every part
    /// type present anywhere in the tree. Used by tests and by the (later,
    /// not-yet-ported) mutation system to decide which singleton types are
    /// still free to add.
    /// </summary>
    public static Dictionary<PartType, int> TypeCounts(GenomeNode body)
    {
        var counts = new Dictionary<PartType, int>();
        foreach (var entry in CollectNodes(body, 0))
        {
            counts[entry.Node.Type] = counts.GetValueOrDefault(entry.Node.Type) + 1;
        }
        return counts;
    }
}
