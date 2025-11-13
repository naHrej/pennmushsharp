using System.Reflection;
using System.Text.Json;

namespace PennMushSharp.Core.Locks;

public sealed class LockCatalog
{
  public static LockCatalog Instance { get; } = Load();

  private readonly IReadOnlyDictionary<string, LockDefinition> _locksByName;

  private LockCatalog(LockSnapshot snapshot)
  {
    Snapshot = snapshot;
    _locksByName = snapshot.Locks.ToDictionary(l => l.Name, l => l, StringComparer.OrdinalIgnoreCase);
  }

  public LockSnapshot Snapshot { get; }

  public IReadOnlyList<LockDefinition> Locks => Snapshot.Locks;
  public IReadOnlyList<LockPrivilege> Privileges => Snapshot.Privileges;

  public bool TryGetLock(string name, out LockDefinition? definition) => _locksByName.TryGetValue(name, out definition);

  private static LockCatalog Load()
  {
    var assembly = Assembly.GetExecutingAssembly();
    const string resourceName = "PennMushSharp.Core.Generated.locks.json";
    using var stream = assembly.GetManifestResourceStream(resourceName)
      ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' not found. Run the metadata extractors to generate docs/generated/locks.json.");

    var snapshot = JsonSerializer.Deserialize<LockSnapshot>(stream)
      ?? throw new InvalidOperationException("Failed to deserialize lock snapshot.");

    return new LockCatalog(snapshot);
  }
}
