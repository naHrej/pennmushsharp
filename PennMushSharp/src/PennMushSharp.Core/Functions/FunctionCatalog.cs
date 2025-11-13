using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace PennMushSharp.Core.Functions;

public sealed class FunctionCatalog
{
  public static FunctionCatalog Instance { get; } = Load();

  private readonly IReadOnlyDictionary<string, FunctionDefinition> _functionsByName;
  private readonly IReadOnlyDictionary<string, FunctionDefinition> _functionsByAlias;
  private readonly IReadOnlyDictionary<string, FunctionAlias> _aliasEntries;

  private FunctionCatalog(FunctionSnapshot snapshot)
  {
    Snapshot = snapshot;
    _functionsByName = snapshot.Functions
      .GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
      .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

    var aliasMap = new Dictionary<string, FunctionDefinition>(StringComparer.OrdinalIgnoreCase);
    var aliasEntries = new Dictionary<string, FunctionAlias>(StringComparer.OrdinalIgnoreCase);
    foreach (var alias in snapshot.Aliases)
    {
      if (_functionsByName.TryGetValue(alias.FunctionName, out var definition))
      {
        aliasMap[alias.Alias] = definition;
        aliasEntries[alias.Alias] = alias;
      }
    }
    _functionsByAlias = aliasMap;
    _aliasEntries = aliasEntries;
  }

  public FunctionSnapshot Snapshot { get; }

  public IReadOnlyList<FunctionDefinition> Functions => Snapshot.Functions;
  public IReadOnlyList<FunctionAlias> Aliases => Snapshot.Aliases;

  public bool TryGetFunction(string name, out FunctionDefinition? definition)
  {
    if (_functionsByName.TryGetValue(name, out definition))
      return true;

    if (_functionsByAlias.TryGetValue(name, out definition))
      return true;

    definition = null;
    return false;
  }

  public bool TryResolveAlias(string alias, out FunctionAlias? aliasEntry) =>
    _aliasEntries.TryGetValue(alias, out aliasEntry);

  private static FunctionCatalog Load()
  {
    var assembly = Assembly.GetExecutingAssembly();
    const string resourceName = "PennMushSharp.Core.Generated.functions.json";
    using var stream = assembly.GetManifestResourceStream(resourceName)
      ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. Run the metadata extractors to generate docs/generated/functions.json.");

    var snapshot = JsonSerializer.Deserialize<FunctionSnapshot>(stream)
      ?? throw new InvalidOperationException("Failed to deserialize function snapshot.");

    return new FunctionCatalog(snapshot);
  }
}
