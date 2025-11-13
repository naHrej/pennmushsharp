namespace PennMushSharp.Core.Persistence;

public sealed class GameObjectRecord
{
  public required int DbRef { get; init; }
  public string? Name { get; set; }
  public int? Owner { get; set; }
  public List<string> Flags { get; } = new();
  public Dictionary<string, string> Attributes { get; } = new(StringComparer.OrdinalIgnoreCase);
  public Dictionary<string, string> Locks { get; } = new(StringComparer.OrdinalIgnoreCase);
}
