namespace Genesis.Core.Rng;

/// <summary>
/// Deterministic, seedable PRNG. Bit-for-bit port of the "mulberry32" style
/// generator in index.html's <c>makeRNG(seed)</c> (GENESIS v9 core, line
/// ~236). Every simulation system (genome draws, mutation, speciation,
/// terrain generation, ...) in the JS build is driven exclusively through
/// this generator, so reproducing its exact output sequence is what makes a
/// fixed-seed world reproducible across the JS and C# implementations.
///
/// JS source (verbatim):
/// <code>
/// function makeRNG(seed) {
///   let a = (seed >>> 0) || 1;
///   return function () {
///     a |= 0; a = (a + 0x6D2B79F5) | 0;
///     let t = Math.imul(a ^ (a >>> 15), 1 | a);
///     t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
///     return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
///   };
/// }
/// </code>
///
/// Porting notes: JS's bitwise operators (|, ^, &lt;&lt;, &gt;&gt;) coerce
/// operands to signed 32-bit integers, and <c>Math.imul</c> is defined as
/// 32-bit integer multiplication with silent overflow (the low 32 bits of
/// the true product). C#'s <c>int</c> arithmetic in an <c>unchecked</c>
/// context has exactly the same width and wraparound semantics, so the
/// translation below is a direct, exact port: no behavior is approximated.
/// (This is unlike <see cref="Gauss"/>, which depends on Math.Log/Cos/Sqrt
/// and can differ from V8 by a few ULPs since those are transcendental
/// functions computed by different libm implementations.)
/// </summary>
public sealed class GenesisRandom
{
    private int _a;

    /// <param name="seed">
    /// Any 32-bit value. JS treats the seed as unsigned (`seed >>> 0`) and
    /// substitutes 1 for a zero seed, since a zero internal state would
    /// otherwise make the first mix step degenerate; replicated here.
    /// </param>
    public GenesisRandom(uint seed)
    {
        uint s = seed == 0 ? 1u : seed;
        _a = unchecked((int)s);
    }

    /// <summary>Convenience overload for ordinary (non-negative) seeds.</summary>
    public GenesisRandom(int seed) : this(unchecked((uint)seed)) { }

    /// <summary>
    /// Returns the next value in [0, 1), advancing internal state by one step.
    /// Equivalent to calling the JS closure returned by makeRNG().
    /// </summary>
    public double NextDouble()
    {
        unchecked
        {
            _a = _a + unchecked((int)0x6D2B79F5);
            int a = _a;
            int t = (a ^ (int)((uint)a >> 15)) * (1 | a);
            t = (t + (t ^ (int)((uint)t >> 7)) * (61 | t)) ^ t;
            uint result = (uint)(t ^ (int)((uint)t >> 14));
            return result / 4294967296.0;
        }
    }

    /// <summary>
    /// Standard-normal sample via Box-Muller, matching index.html's
    /// <c>gauss(rng)</c> (line ~245) term for term:
    /// <code>
    /// function gauss(rng) {
    ///   let u = 0, v = 0; while (u === 0) u = rng(); while (v === 0) v = rng();
    ///   return Math.sqrt(-2 * Math.log(u)) * Math.cos(2 * Math.PI * v);
    /// }
    /// </code>
    /// Draws u, v rejecting exact zero (matching the JS while-loops, which
    /// exist so log(u) never hits -Infinity), then applies the standard
    /// Box-Muller transform. NOT guaranteed bit-identical to the JS result
    /// for the same rng stream (Math.Log/Cos/Sqrt run through .NET's libm,
    /// not V8's) -- tests compare this to JS fixtures with a small numeric
    /// tolerance, never with exact equality.
    /// </summary>
    public double Gauss()
    {
        double u = 0, v = 0;
        while (u == 0) u = NextDouble();
        while (v == 0) v = NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u)) * Math.Cos(2.0 * Math.PI * v);
    }
}
