using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Core;

/// <summary>
/// Immutable snapshot of a PennMUSH object exposed to runtime subsystems.
/// </summary>
public sealed class GameObject
{
  public GameObject(
    int dbRef,
    string name,
    GameObjectType type,
    int owner,
    int? location,
    IReadOnlyCollection<string> flags,
    IReadOnlyDictionary<string, string> attributes,
    IReadOnlyDictionary<string, string> locks)
  {
    DbRef = dbRef;
    Name = name;
    Type = type;
    Owner = owner;
    Location = location;
    Flags = flags;
    Attributes = attributes;
    Locks = locks;
  }

  public int DbRef { get; }
  public string Name { get; }
  public GameObjectType Type { get; }
  public int Owner { get; }
  public int? Location { get; }
  public IReadOnlyCollection<string> Flags { get; }
  public IReadOnlyDictionary<string, string> Attributes { get; }
  public IReadOnlyDictionary<string, string> Locks { get; }

  public static GameObject FromRecord(GameObjectRecord record)
  {
    ArgumentNullException.ThrowIfNull(record);
    var name = string.IsNullOrWhiteSpace(record.Name) ? $"#{record.DbRef}" : record.Name;
    var owner = record.Owner ?? record.DbRef;

    var flags = record.Flags.ToArray();
    var attributes = record.Attributes.Values.ToDictionary(a => a.Name, a => a.Value, StringComparer.OrdinalIgnoreCase);
    var locks = record.Locks.Values.ToDictionary(l => l.Name, l => l.Key, StringComparer.OrdinalIgnoreCase);

    return new GameObject(
      record.DbRef,
      name,
      record.Type,
      owner,
      record.Location,
      flags,
      attributes,
      locks);
  }
}
