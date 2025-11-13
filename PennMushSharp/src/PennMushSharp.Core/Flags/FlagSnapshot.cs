namespace PennMushSharp.Core.Flags;

public sealed class FlagSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<FlagDefinition> Flags { get; init; }
  public required IReadOnlyList<FlagDefinition> Powers { get; init; }
}
