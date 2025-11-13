public sealed class FlagDefinition
{
  public required string Name { get; init; }
  public char? Letter { get; init; }
  public uint TypeMask { get; init; }
  public long BitPosition { get; init; }
  public uint SetPermissions { get; init; }
  public uint ClearPermissions { get; init; }
}

public sealed class FlagSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<FlagDefinition> Flags { get; init; }
  public required IReadOnlyList<FlagDefinition> Powers { get; init; }
}
