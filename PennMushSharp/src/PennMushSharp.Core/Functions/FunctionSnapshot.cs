namespace PennMushSharp.Core.Functions;

public sealed class FunctionSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<FunctionDefinition> Functions { get; init; }
  public required IReadOnlyList<FunctionAlias> Aliases { get; init; }
}
