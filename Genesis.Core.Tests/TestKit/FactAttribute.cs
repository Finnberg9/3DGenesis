namespace Genesis.Core.Tests.TestKit;

/// <summary>
/// Marks a public, parameterless, void (or Task-returning) instance method
/// as a test case. Deliberately named/shaped like xUnit's [Fact] -- see the
/// NuGet-blocked-in-this-sandbox note in Genesis.Core.Tests.csproj for why
/// this hand-rolled kit exists and how to swap it for real xUnit later.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class FactAttribute : Attribute
{
    public string? DisplayName { get; init; }
}
