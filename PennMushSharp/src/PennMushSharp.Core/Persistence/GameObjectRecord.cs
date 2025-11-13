using System.Collections.Generic;

namespace PennMushSharp.Core.Persistence;

public sealed class GameObjectRecord
{
  public required int DbRef { get; init; }
  public string? Name { get; set; }
  public GameObjectType Type { get; set; } = GameObjectType.Unknown;
  public int? Owner { get; set; }
  public int? Location { get; set; }
  public int? Contents { get; set; }
  public int? Exits { get; set; }
  public int? Next { get; set; }
  public int? Parent { get; set; }
  public int? Home { get; set; }
  public int? Zone { get; set; }
  public int? Dropto { get; set; }
  public int? Pennies { get; set; }
  public ISet<string> Flags { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  public Dictionary<string, AttributeRecord> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
  public Dictionary<string, LockRecord> Locks { get; } = new(StringComparer.OrdinalIgnoreCase);

  public AttributeRecord SetAttribute(string name, string value, int? owner = null, string? flags = null, int derefs = 0)
  {
    var attribute = new AttributeRecord(name)
    {
      Owner = owner ?? Owner ?? DbRef,
      Flags = flags ?? string.Empty,
      Derefs = derefs,
      Value = value
    };
    Attributes[name] = attribute;
    return attribute;
  }

  public LockRecord SetLock(string name, string key, int? creator = null, string? flags = null, int derefs = 0)
  {
    var record = new LockRecord(name)
    {
      Creator = creator ?? Owner ?? DbRef,
      Flags = flags ?? string.Empty,
      Derefs = derefs,
      Key = key
    };
    Locks[name] = record;
    return record;
  }
}
