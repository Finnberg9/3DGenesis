using System.Text.Json;

namespace Genesis.Core.Tests.Fixtures;

/// <summary>
/// Loads genome_fixtures.json -- ground-truth output captured by running the
/// REAL, unmodified simulation JS (extracted verbatim from index.html's
/// first &lt;script&gt; block) under Node. See PHASE00_PROGRESS.md for the
/// exact generation script and how to regenerate it if index.html's genome
/// logic ever changes.
/// </summary>
public static class FixtureLoader
{
    private static readonly Lazy<GenomeFixtures> _cached = new(Load);

    public static GenomeFixtures Data => _cached.Value;

    private static GenomeFixtures Load()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "genome_fixtures.json");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Characterization fixture not found at {path}. " +
                "This file must be copied into Genesis.Core.Tests/Fixtures/ and the .csproj must " +
                "list it as a CopyToOutputDirectory item (see PHASE00_PROGRESS.md for the Node " +
                "generation script that produces it from index.html's real JS).", path);
        }

        string json = File.ReadAllText(path);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<GenomeFixtures>(json, options)
               ?? throw new InvalidDataException("genome_fixtures.json deserialized to null.");
    }
}
