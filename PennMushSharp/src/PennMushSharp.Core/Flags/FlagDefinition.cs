namespace PennMushSharp.Core.Flags;

public sealed class FlagDefinition
{
  public required string Name { get; init; }
  public char? Letter { get; init; }
  public uint TypeMask { get; init; }
  public long BitPosition { get; init; }
  public uint SetPermissions { get; init; }
  public uint ClearPermissions { get; init; }
}
