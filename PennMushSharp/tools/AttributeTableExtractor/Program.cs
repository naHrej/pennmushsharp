using System.Text.Json;
using PennMushSharp.Extraction;

var repoRoot = RepoLocator.FindRepositoryRoot();
var headerPath = Path.Combine(repoRoot, "pennmush", "hdrs", "atr_tab.h");
var attribHeaderPath = Path.Combine(repoRoot, "pennmush", "hdrs", "attrib.h");
var chunkHeaderPath = Path.Combine(repoRoot, "pennmush", "hdrs", "chunk.h");

foreach (var path in new[] { headerPath, attribHeaderPath, chunkHeaderPath })
{
  if (!File.Exists(path))
    throw new FileNotFoundException("Required header not found", path);
}

var macros = new MacroTable();
macros.Load(attribHeaderPath);
macros.Load(chunkHeaderPath);
macros.Load(headerPath);
macros.ResolvePending();

var headerText = File.ReadAllText(headerPath);
headerText = CommentUtilities.RemoveComments(headerText);
headerText = CommentUtilities.CombineContinuations(headerText);

var attrEntries = InitializerParser.Parse(headerText, "ATTR attr", 4);
var aliasEntries = InitializerParser.Parse(headerText, "ATRALIAS attralias", 2);

var attributes = attrEntries
  .Select(entry => AttributeDefinitionFactory.Create(entry, macros))
  .Where(static def => def is not null)
  .Cast<AttributeDefinition>()
  .ToList();

var aliases = aliasEntries
  .Select(AttributeAliasFactory.Create)
  .Where(static alias => alias is not null)
  .Cast<AttributeAlias>()
  .ToList();

var snapshot = new AttributeSnapshot
{
  Source = Path.GetRelativePath(repoRoot, headerPath).Replace('\\', '/'),
  GeneratedUtc = DateTime.UtcNow,
  Attributes = attributes,
  Aliases = aliases
};

var outputDir = Path.Combine(repoRoot, "PennMushSharp", "docs", "generated");
Directory.CreateDirectory(outputDir);
var outputPath = Path.Combine(outputDir, "attributes.json");

var options = new JsonSerializerOptions { WriteIndented = true };
await using var stream = File.Create(outputPath);
await JsonSerializer.SerializeAsync(stream, snapshot, options);

Console.WriteLine($"Extracted {attributes.Count} attributes and {aliases.Count} aliases to {outputPath}");

static class AttributeDefinitionFactory
{
  public static AttributeDefinition? Create(IReadOnlyList<string> fields, MacroTable macros)
  {
    if (fields.Count != 4)
      throw new InvalidOperationException($"Expected 4 fields per attribute entry but found {fields.Count}.");

    var nameToken = fields[0];
    if (string.Equals(nameToken.Trim(), "NULL", StringComparison.Ordinal))
      return null;

    var name = LiteralParser.ParseString(nameToken);
    var flags = macros.EvaluateUInt(fields[1]);
    var creator = macros.EvaluateInt64(fields[2]);
    var chunkRef = macros.EvaluateUInt64(fields[3]);

    return new AttributeDefinition
    {
      Name = name,
      Flags = flags,
      Creator = creator,
      DefaultChunkReference = chunkRef
    };
  }
}

static class AttributeAliasFactory
{
  public static AttributeAlias? Create(IReadOnlyList<string> fields)
  {
    if (fields.Count != 2)
      throw new InvalidOperationException($"Expected 2 fields per alias entry but found {fields.Count}.");

    var aliasToken = fields[0].Trim();
    if (string.Equals(aliasToken, "NULL", StringComparison.Ordinal))
      return null;

    return new AttributeAlias
    {
      Alias = LiteralParser.ParseString(fields[0]),
      RealName = LiteralParser.ParseString(fields[1])
    };
  }
}

public sealed class AttributeDefinition
{
  public required string Name { get; init; }
  public uint Flags { get; init; }
  public long Creator { get; init; }
  public ulong DefaultChunkReference { get; init; }
}

public sealed class AttributeAlias
{
  public required string Alias { get; init; }
  public required string RealName { get; init; }
}

public sealed class AttributeSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<AttributeDefinition> Attributes { get; init; }
  public required IReadOnlyList<AttributeAlias> Aliases { get; init; }
}
