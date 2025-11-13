namespace PennMushSharp.Core.Attributes;

public sealed class AttributeSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<AttributeDefinition> Attributes { get; init; }
  public required IReadOnlyList<AttributeAlias> Aliases { get; init; }
}
