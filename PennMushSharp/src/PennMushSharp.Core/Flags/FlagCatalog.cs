using System.Reflection;
using System.Text.Json;

namespace PennMushSharp.Core.Flags;

public sealed class FlagCatalog
{
  public static FlagCatalog Instance { get; } = Load();

  private readonly IReadOnlyDictionary<string, FlagDefinition> _flagsByName;
  private readonly IReadOnlyDictionary<char, FlagDefinition> _flagsByLetter;

  private FlagCatalog(FlagSnapshot snapshot)
  {
    Snapshot = snapshot;
    _flagsByName = snapshot.Flags.ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);
    _flagsByLetter = snapshot.Flags
      .Where(f => f.Letter is not null)
      .GroupBy(f => char.ToUpperInvariant(f.Letter!.Value))
      .ToDictionary(g => g.Key, g => g.First());
  }

  public FlagSnapshot Snapshot { get; }

  public IReadOnlyList<FlagDefinition> Flags => Snapshot.Flags;
  public IReadOnlyList<FlagDefinition> Powers => Snapshot.Powers;

  public bool TryGetFlag(string name, out FlagDefinition? definition) => _flagsByName.TryGetValue(name, out definition);

  public bool TryGetFlagByLetter(char letter, out FlagDefinition? definition)
  {
    return _flagsByLetter.TryGetValue(char.ToUpperInvariant(letter), out definition);
  }

  private static FlagCatalog Load()
  {
    var assembly = Assembly.GetExecutingAssembly();
    const string resourceName = "PennMushSharp.Core.Generated.flags.json";
    using var stream = assembly.GetManifestResourceStream(resourceName)
      ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. Ensure the flag snapshot is embedded in the PennMushSharp.Core project.");

    var snapshot = JsonSerializer.Deserialize<FlagSnapshot>(stream)
      ?? throw new InvalidOperationException("Failed to deserialize flag snapshot.");
    return new FlagCatalog(snapshot);
  }
}
