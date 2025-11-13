namespace PennMushSharp.Commands.Metadata;

public sealed class CommandMetadataSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<CommandMetadataRecord> Commands { get; init; }
}

public sealed class CommandMetadataRecord
{
  public required string Name { get; init; }
  public required string Handler { get; init; }
  public string? Category { get; init; }
  public IReadOnlyList<string> Switches { get; init; } = Array.Empty<string>();
  public uint TypeFlags { get; init; }
  public IReadOnlyList<string> Flags { get; init; } = Array.Empty<string>();
  public IReadOnlyList<string> Powers { get; init; } = Array.Empty<string>();
}
