namespace PennMushSharp.Core.Locks;

public sealed class LockSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<LockDefinition> Locks { get; init; }
  public required IReadOnlyList<LockPrivilege> Privileges { get; init; }
}
