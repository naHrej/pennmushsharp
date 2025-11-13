namespace PennMushSharp.Core.Functions;

public sealed class FunctionDefinition
{
  public required string Name { get; init; }
  public required string Handler { get; init; }
  public int MinArgs { get; init; }
  public int MaxArgs { get; init; }
  public uint Flags { get; init; }
}
