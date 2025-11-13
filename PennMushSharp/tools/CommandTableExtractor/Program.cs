using System.Linq;
using System.Text.Json;
using PennMushSharp.Extraction;

var repoRoot = RepoLocator.FindRepositoryRoot();
var sourcePath = Path.Combine(repoRoot, "pennmush", "src", "command.c");
var commandHeader = Path.Combine(repoRoot, "pennmush", "hdrs", "command.h");

foreach (var path in new[] { sourcePath, commandHeader })
{
  if (!File.Exists(path))
    throw new FileNotFoundException("Required file missing", path);
}

var macros = new MacroTable();
macros.Load(commandHeader);
macros.ResolvePending();

var sourceText = File.ReadAllText(sourcePath);
sourceText = CommentUtilities.RemoveComments(sourceText);
sourceText = CommentUtilities.CombineContinuations(sourceText);
sourceText = PreprocessorUtilities.RemoveDirectives(sourceText);

var entries = InitializerParser.Parse(sourceText, "COMLIST commands", 6);
var records = new List<CommandRecord>();
foreach (var entry in entries)
{
  var record = CommandRecordFactory.Create(entry, macros);
  if (record is not null)
    records.Add(record);
}

var snapshot = new CommandMetadataSnapshot
{
  Source = Path.GetRelativePath(repoRoot, sourcePath).Replace('\\', '/'),
  GeneratedUtc = DateTime.UtcNow,
  Commands = records
    .OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase)
    .Select(r => new CommandMetadataRecord
    {
      Name = r.Name,
      Handler = r.Handler,
      Category = "builtin",
      Switches = r.Switches,
      TypeFlags = r.TypeFlags,
      Flags = r.Flags,
      Powers = r.Powers
    })
    .ToList()
};

var outputDir = Path.Combine(repoRoot, "PennMushSharp", "docs", "generated");
Directory.CreateDirectory(outputDir);
var outputPath = Path.Combine(outputDir, "commands.json");
await using var stream = File.Create(outputPath);
await JsonSerializer.SerializeAsync(stream, snapshot, new JsonSerializerOptions
{
  WriteIndented = true
});

Console.WriteLine($"Extracted {snapshot.Commands.Count} commands to {outputPath}");

static class CommandRecordFactory
{
  public static CommandRecord? Create(IReadOnlyList<string> fields, MacroTable macros)
  {
    if (fields.Count != 6)
      throw new InvalidOperationException($"Expected 6 fields for command entry but found {fields.Count}.");

    var nameToken = fields[0].Trim();
    if (string.Equals(nameToken, "NULL", StringComparison.Ordinal))
      return null;

    var name = LiteralParser.ParseString(fields[0]);
    var switchList = ParseList(fields[1]);
    var handler = fields[2].Trim();
    var typeFlags = macros.EvaluateUInt(fields[3]);
    var flags = ParseList(fields[4]);
    var powers = ParseList(fields[5]);

    return new CommandRecord(name, handler, typeFlags, switchList, flags, powers);
  }

  private static List<string> ParseList(string token)
  {
    var trimmed = token.Trim();
    if (string.Equals(trimmed, "NULL", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(trimmed, "0", StringComparison.Ordinal))
      return new List<string>();

    if (trimmed.StartsWith("\"", StringComparison.Ordinal))
    {
      var value = LiteralParser.ParseString(token);
      return value
        .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .ToList();
    }

    return new List<string>();
  }
}

public sealed record CommandRecord(
  string Name,
  string Handler,
  uint TypeFlags,
  IReadOnlyList<string> Switches,
  IReadOnlyList<string> Flags,
  IReadOnlyList<string> Powers);

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
  public required IReadOnlyList<string> Switches { get; init; }
  public uint TypeFlags { get; init; }
  public IReadOnlyList<string> Flags { get; init; } = Array.Empty<string>();
  public IReadOnlyList<string> Powers { get; init; } = Array.Empty<string>();
}
