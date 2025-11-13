using System.Text.Json;
using PennMushSharp.Extraction;

var repoRoot = RepoLocator.FindRepositoryRoot();
var sourcePath = Path.Combine(repoRoot, "pennmush", "src", "function.c");
var functionHeader = Path.Combine(repoRoot, "pennmush", "hdrs", "function.h");
var mushtypeHeader = Path.Combine(repoRoot, "pennmush", "hdrs", "mushtype.h");

foreach (var path in new[] { sourcePath, functionHeader, mushtypeHeader })
{
  if (!File.Exists(path))
    throw new FileNotFoundException("Required file missing", path);
}

var macros = new MacroTable();
macros.Load(functionHeader);
macros.Load(mushtypeHeader);
macros.Load(sourcePath);
macros.SetValue("INT_MAX", (ulong)int.MaxValue);
macros.SetValue("INT_MIN", unchecked((ulong)int.MinValue));
macros.SetValue("MAX_STACK_ARGS", 30);
macros.ResolvePending();

var sourceText = File.ReadAllText(sourcePath);
sourceText = CommentUtilities.RemoveComments(sourceText);
sourceText = CommentUtilities.CombineContinuations(sourceText);
sourceText = PreprocessorUtilities.RemoveDirectives(sourceText);

var aliasEntries = InitializerParser.Parse(sourceText, "FUNALIAS faliases", 2);
var functionEntries = InitializerParser.Parse(sourceText, "FUNTAB flist", 5);

var aliases = aliasEntries
  .Select(FunctionAliasFactory.Create)
  .Where(static alias => alias is not null)
  .Cast<FunctionAlias>()
  .ToList();

var functions = functionEntries
  .Select(fields => FunctionDefinitionFactory.Create(fields, macros))
  .Where(static def => def is not null)
  .Cast<FunctionDefinition>()
  .ToList();

var snapshot = new FunctionSnapshot
{
  Source = Path.GetRelativePath(repoRoot, sourcePath).Replace('\\', '/'),
  GeneratedUtc = DateTime.UtcNow,
  Functions = functions,
  Aliases = aliases
};

var outputDir = Path.Combine(repoRoot, "PennMushSharp", "docs", "generated");
Directory.CreateDirectory(outputDir);
var outputPath = Path.Combine(outputDir, "functions.json");

await using var stream = File.Create(outputPath);
await JsonSerializer.SerializeAsync(stream, snapshot, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine($"Extracted {functions.Count} functions and {aliases.Count} aliases to {outputPath}");

static class FunctionDefinitionFactory
{
  public static FunctionDefinition? Create(IReadOnlyList<string> fields, MacroTable macros)
  {
    if (fields.Count != 5)
      throw new InvalidOperationException($"Expected 5 fields per function entry but found {fields.Count}.");

    var nameToken = fields[0].Trim();
    if (string.Equals(nameToken, "NULL", StringComparison.Ordinal))
      return null;

    var name = LiteralParser.ParseString(fields[0]);
    var handler = fields[1].Trim();
    var minArgs = checked((int)macros.EvaluateInt64(fields[2]));
    var maxArgs = checked((int)macros.EvaluateInt64(fields[3]));
    var flags = macros.EvaluateUInt(fields[4]);

    return new FunctionDefinition
    {
      Name = name,
      Handler = handler,
      MinArgs = minArgs,
      MaxArgs = maxArgs,
      Flags = flags
    };
  }
}

static class FunctionAliasFactory
{
  public static FunctionAlias? Create(IReadOnlyList<string> fields)
  {
    if (fields.Count != 2)
      throw new InvalidOperationException($"Expected 2 fields for function alias but found {fields.Count}.");

    var nameToken = fields[0].Trim();
    if (string.Equals(nameToken, "NULL", StringComparison.Ordinal))
      return null;

    return new FunctionAlias
    {
      FunctionName = LiteralParser.ParseString(fields[0]),
      Alias = LiteralParser.ParseString(fields[1])
    };
  }
}

public sealed class FunctionDefinition
{
  public required string Name { get; init; }
  public required string Handler { get; init; }
  public int MinArgs { get; init; }
  public int MaxArgs { get; init; }
  public uint Flags { get; init; }
}

public sealed class FunctionAlias
{
  public required string FunctionName { get; init; }
  public required string Alias { get; init; }
}

public sealed class FunctionSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<FunctionDefinition> Functions { get; init; }
  public required IReadOnlyList<FunctionAlias> Aliases { get; init; }
}
