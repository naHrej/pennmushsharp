namespace PennMushSharp.Core.Attributes;

public sealed class AttributeDefinition
{
  public required string Name { get; init; }
  public uint Flags { get; init; }
  public long Creator { get; init; }
  public ulong DefaultChunkReference { get; init; }
}
