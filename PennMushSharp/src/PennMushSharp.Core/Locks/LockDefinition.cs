namespace PennMushSharp.Core.Locks;

public sealed class LockDefinition
{
  public required string Name { get; init; }
  public required string DefaultKeyExpression { get; init; }
  public long DefaultCreator { get; init; }
  public uint DefaultFlags { get; init; }
}
