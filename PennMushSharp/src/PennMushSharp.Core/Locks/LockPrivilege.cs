namespace PennMushSharp.Core.Locks;

public sealed class LockPrivilege
{
  public required string Name { get; init; }
  public char? Symbol { get; init; }
  public uint SetFlags { get; init; }
  public uint ClearFlags { get; init; }
}
