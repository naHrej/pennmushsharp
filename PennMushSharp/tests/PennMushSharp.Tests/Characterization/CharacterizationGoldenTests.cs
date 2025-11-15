using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PennMushSharp.Tests.Characterization;

public sealed class CharacterizationGoldenTests
{
  private static readonly string RepositoryRoot = LocateRepositoryRoot();
  private static readonly string CharacterizationRoot = Path.Combine(RepositoryRoot, "tests", "characterization");
  private static readonly string GoldenDirectory = Path.Combine(CharacterizationRoot, "golden");
  private static readonly string ScenarioDirectory = Path.Combine(CharacterizationRoot, "scenarios");

  private static string LocateRepositoryRoot()
  {
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "PennMushSharp.sln")))
      directory = directory.Parent;

    if (directory is null)
      throw new InvalidOperationException("Unable to find PennMushSharp.sln relative to the test binary.");

    return directory.FullName;
  }

  public static IEnumerable<object[]> GoldenFiles
  {
    get
    {
      if (!Directory.Exists(GoldenDirectory))
        yield break;

      foreach (var path in Directory.EnumerateFiles(GoldenDirectory, "*.log").OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        yield return new object[] { path };
    }
  }

  [Theory]
  [MemberData(nameof(GoldenFiles))]
  public void GoldenTranscript_IsWellFormed(string path)
  {
    var transcript = CharacterizationTranscript.Load(path);

    Assert.NotEmpty(transcript.Lines);
    Assert.True(transcript.Metadata.TryGetValue("name", out var name) && !string.IsNullOrWhiteSpace(name),
      $"Transcript '{path}' is missing the scenario name banner.");
  }

  [Fact]
  public void EveryScenarioDeclaresAGoldenTranscript()
  {
    if (!Directory.Exists(ScenarioDirectory))
      return;

    var missing = new List<string>();
    foreach (var scenario in ScenarioFile.LoadAll(ScenarioDirectory))
    {
      var expected = Path.Combine(GoldenDirectory, $"{scenario.Name}.log");
      if (!File.Exists(expected))
        missing.Add(scenario.Name);
    }

    Assert.True(missing.Count == 0, "Missing golden transcript for scenario(s): " + string.Join(", ", missing));
  }

  private sealed record ScenarioFile(string Name)
  {
    public static IEnumerable<ScenarioFile> LoadAll(string root)
    {
      foreach (var path in Directory.EnumerateFiles(root, "*.scenario"))
      {
        yield return new ScenarioFile(ParseName(path));
      }
    }

    private static string ParseName(string path)
    {
      foreach (var line in File.ReadLines(path, Encoding.UTF8))
      {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal))
          continue;
        if (trimmed.StartsWith("NAME=", StringComparison.Ordinal))
          return Unquote(trimmed["NAME=".Length..].Trim());
      }

      throw new InvalidOperationException($"Scenario '{path}' does not define NAME.");
    }

    private static string Unquote(string input)
    {
      if (input.Length >= 2 && input[0] == '"' && input[^1] == '"')
        return input[1..^1];
      if (input.Length >= 2 && input[0] == '\'' && input[^1] == '\'')
        return input[1..^1];
      return input;
    }
  }

  private sealed class CharacterizationTranscript
  {
    private CharacterizationTranscript(IReadOnlyDictionary<string, string> metadata, IReadOnlyList<string> lines)
    {
      Metadata = metadata;
      Lines = lines;
    }

    public IReadOnlyDictionary<string, string> Metadata { get; }
    public IReadOnlyList<string> Lines { get; }

    public static CharacterizationTranscript Load(string path)
    {
      var lines = File.ReadAllLines(path, Encoding.UTF8)
        .Select(l => l.TrimEnd('\r'))
        .Where(l => l.Length > 0)
        .ToArray();

      if (lines.Length == 0)
        throw new InvalidOperationException($"Transcript '{path}' is empty.");

      var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
      TryParseBanner(lines[0], metadata);

      return new CharacterizationTranscript(new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase), lines);
    }

    private static void TryParseBanner(string header, IDictionary<string, string> metadata)
    {
      if (!header.StartsWith("***", StringComparison.Ordinal))
        return;

      // Expected format: *** Something :: scenario ***
      var trimmed = header.Trim('*', ' ', '\t');
      var parts = trimmed.Split(new[] { "::" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
      if (parts.Length >= 2)
        metadata["name"] = parts[^1];
    }
  }
}
