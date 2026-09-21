using System.Collections;

namespace Genesis.Core.Tests.TestKit;

/// <summary>Thrown by every Assert.* failure; the runner catches this specifically to report a clean failure line (vs. an unexpected exception).</summary>
public sealed class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}

/// <summary>
/// Minimal assertion library mirroring xUnit's Assert surface (same method
/// names/argument order: Equal(expected, actual), True/False, InRange,
/// Throws&lt;T&gt;, ...) so tests written against this can be ported to real
/// xUnit later by changing only the using directive / project references.
/// </summary>
public static class Assert
{
    public static void True(bool condition, string? message = null)
    {
        if (!condition) throw new AssertionException(message ?? "Expected condition to be true, was false.");
    }

    public static void False(bool condition, string? message = null)
    {
        if (condition) throw new AssertionException(message ?? "Expected condition to be false, was true.");
    }

    public static void Equal<T>(T expected, T actual, string? message = null)
    {
        if (!Equals(expected, actual))
        {
            throw new AssertionException(message ?? $"Expected: {Format(expected)}\nActual:   {Format(actual)}");
        }
    }

    /// <summary>Exact double equality. Use only for values with no transcendental-function dependency (RNG stream, integer-derived doubles) -- see Equal(double,double,double) for anything derived from Math.Log/Cos/Sqrt.</summary>
    public static void Equal(double expected, double actual, string? message = null)
    {
        if (expected != actual)
        {
            throw new AssertionException(message ?? $"Expected: {expected:R}\nActual:   {actual:R}");
        }
    }

    /// <summary>Tolerance-based double equality. Use for any value whose computation passed through Math.Log/Cos/Sqrt (e.g. Gauss()-derived brain weights), since V8 and .NET's libm can differ by a few ULPs on the same inputs.</summary>
    public static void Equal(double expected, double actual, double tolerance, string? message = null)
    {
        double diff = Math.Abs(expected - actual);
        if (diff > tolerance)
        {
            throw new AssertionException(message ?? $"Expected: {expected:R}\nActual:   {actual:R}\nDiff:     {diff:R} (tolerance {tolerance:R})");
        }
    }

    public static void InRange(double value, double low, double high, string? message = null)
    {
        if (value < low || value > high)
        {
            throw new AssertionException(message ?? $"Expected {value} to be in range [{low}, {high}]");
        }
    }

    public static void NotNull(object? value, string? message = null)
    {
        if (value is null) throw new AssertionException(message ?? "Expected non-null value.");
    }

    public static void Null(object? value, string? message = null)
    {
        if (value is not null) throw new AssertionException(message ?? $"Expected null, got: {Format(value)}");
    }

    public static void Empty(IEnumerable collection, string? message = null)
    {
        foreach (var _ in collection) throw new AssertionException(message ?? "Expected collection to be empty.");
    }

    public static void Contains<T>(T expected, IEnumerable<T> collection, string? message = null)
    {
        foreach (var item in collection) if (Equals(item, expected)) return;
        throw new AssertionException(message ?? $"Expected collection to contain: {Format(expected)}");
    }

    public static void Throws<TException>(Action action, string? message = null) where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new AssertionException(message ?? $"Expected {typeof(TException).Name}, but got {ex.GetType().Name}: {ex.Message}");
        }
        throw new AssertionException(message ?? $"Expected {typeof(TException).Name}, but no exception was thrown.");
    }

    public static void Fail(string message) => throw new AssertionException(message);

    private static string Format(object? v) => v switch
    {
        null => "null",
        string s => $"\"{s}\"",
        double d => d.ToString("R"),
        _ => v.ToString() ?? "null",
    };
}
