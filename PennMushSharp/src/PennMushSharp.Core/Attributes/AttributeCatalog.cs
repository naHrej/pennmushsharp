using System.Reflection;
using System.Text.Json;

namespace PennMushSharp.Core.Attributes;

public sealed class AttributeCatalog
{
  public static AttributeCatalog Instance { get; } = Load();

  private readonly IReadOnlyDictionary<string, AttributeDefinition> _attributes;
  private readonly IReadOnlyDictionary<string, AttributeAlias> _aliases;

  private AttributeCatalog(AttributeSnapshot snapshot)
  {
    Snapshot = snapshot;
    _attributes = snapshot.Attributes.ToDictionary(a => a.Name, a => a, StringComparer.OrdinalIgnoreCase);
    _aliases = snapshot.Aliases.ToDictionary(a => a.Alias, a => a, StringComparer.OrdinalIgnoreCase);
  }

  public AttributeSnapshot Snapshot { get; }

  public IReadOnlyList<AttributeDefinition> Attributes => Snapshot.Attributes;
  public IReadOnlyList<AttributeAlias> Aliases => Snapshot.Aliases;

  public bool TryGetAttribute(string name, out AttributeDefinition? definition)
  {
    if (_attributes.TryGetValue(name, out definition))
      return true;

    if (_aliases.TryGetValue(name, out var alias) && _attributes.TryGetValue(alias.RealName, out definition))
      return true;

    definition = null;
    return false;
  }

  public bool TryResolveAlias(string alias, out AttributeAlias? aliasEntry) => _aliases.TryGetValue(alias, out aliasEntry);

  private static AttributeCatalog Load()
  {
    var assembly = Assembly.GetExecutingAssembly();
    const string resourceName = "PennMushSharp.Core.Generated.attributes.json";
    using var stream = assembly.GetManifestResourceStream(resourceName)
      ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. Run the attribute extractor to generate docs/generated/attributes.json and ensure it is embedded.");

    var snapshot = JsonSerializer.Deserialize<AttributeSnapshot>(stream)
      ?? throw new InvalidOperationException("Failed to deserialize attribute snapshot.");

    return new AttributeCatalog(snapshot);
  }
}
