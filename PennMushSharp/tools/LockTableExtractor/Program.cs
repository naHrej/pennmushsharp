using System.Text.Json;
using PennMushSharp.Extraction;

var repoRoot = RepoLocator.FindRepositoryRoot();
var headerPath = Path.Combine(repoRoot, "pennmush", "hdrs", "lock_tab.h");
var lockHeader = Path.Combine(repoRoot, "pennmush", "hdrs", "lock.h");
foreach (var path in new[] { headerPath, lockHeader })
{
  if (!File.Exists(path))
    throw new FileNotFoundException("Required header missing", path);
}

var macros = new MacroTable();
macros.Load(lockHeader);
macros.Load(headerPath);
macros.SetValue("TRUE_BOOLEXP", 0);
macros.SetValue("GOD", 1);
macros.ResolvePending();

var headerText = File.ReadAllText(headerPath);
headerText = CommentUtilities.RemoveComments(headerText);
headerText = CommentUtilities.CombineContinuations(headerText);

var lockEntries = InitializerParser.Parse(headerText, "lock_list lock_types", 5);
var privilegeEntries = InitializerParser.Parse(headerText, "PRIV lock_privs", 4);

var locks = lockEntries
  .Select(fields => LockDefinitionFactory.Create(fields, macros))
  .Where(static def => def is not null)
  .Cast<LockDefinition>()
  .ToList();

var privileges = privilegeEntries
  .Select(fields => LockPrivilegeFactory.Create(fields, macros))
  .Where(static priv => priv is not null)
  .Cast<LockPrivilege>()
  .ToList();

var snapshot = new LockSnapshot
{
  Source = Path.GetRelativePath(repoRoot, headerPath).Replace('\\', '/'),
  GeneratedUtc = DateTime.UtcNow,
  Locks = locks,
  Privileges = privileges
};

var outputDir = Path.Combine(repoRoot, "PennMushSharp", "docs", "generated");
Directory.CreateDirectory(outputDir);
var outputPath = Path.Combine(outputDir, "locks.json");

await using var stream = File.Create(outputPath);
await JsonSerializer.SerializeAsync(stream, snapshot, new JsonSerializerOptions { WriteIndented = true });

Console.WriteLine($"Extracted {locks.Count} locks and {privileges.Count} lock privilege rows to {outputPath}");

static class LockDefinitionFactory
{
  public static LockDefinition? Create(IReadOnlyList<string> fields, MacroTable macros)
  {
    if (fields.Count != 5)
      throw new InvalidOperationException($"Expected 5 fields for lock definition but found {fields.Count}.");

    var nameToken = fields[0].Trim();
    if (string.Equals(nameToken, "NULL", StringComparison.Ordinal))
      return null;

    var name = LiteralParser.ParseString(fields[0]);
    var defaultKey = fields[1].Trim();
    var creator = macros.EvaluateInt64(fields[2]);
    var flags = macros.EvaluateUInt(fields[3]);

    return new LockDefinition
    {
      Name = name,
      DefaultKeyExpression = defaultKey,
      DefaultCreator = creator,
      DefaultFlags = flags
    };
  }
}

static class LockPrivilegeFactory
{
  public static LockPrivilege? Create(IReadOnlyList<string> fields, MacroTable macros)
  {
    if (fields.Count != 4)
      throw new InvalidOperationException($"Expected 4 fields for lock privileges but found {fields.Count}.");

    var nameToken = fields[0].Trim();
    if (string.Equals(nameToken, "NULL", StringComparison.Ordinal))
      return null;

    var symbolToken = fields[1].Trim();
    char? symbol = null;
    if (!string.Equals(symbolToken, "'\\0'", StringComparison.Ordinal) &&
        !string.Equals(symbolToken, "NULL", StringComparison.Ordinal) &&
        symbolToken != "0")
    {
      symbol = LiteralParser.ParseChar(symbolToken) ?? null;
    }

    return new LockPrivilege
    {
      Name = LiteralParser.ParseString(fields[0]),
      Symbol = symbol,
      SetFlags = macros.EvaluateUInt(fields[2]),
      ClearFlags = macros.EvaluateUInt(fields[3])
    };
  }
}

public sealed class LockDefinition
{
  public required string Name { get; init; }
  public required string DefaultKeyExpression { get; init; }
  public long DefaultCreator { get; init; }
  public uint DefaultFlags { get; init; }
}

public sealed class LockPrivilege
{
  public required string Name { get; init; }
  public char? Symbol { get; init; }
  public uint SetFlags { get; init; }
  public uint ClearFlags { get; init; }
}

public sealed class LockSnapshot
{
  public required string Source { get; init; }
  public required DateTime GeneratedUtc { get; init; }
  public required IReadOnlyList<LockDefinition> Locks { get; init; }
  public required IReadOnlyList<LockPrivilege> Privileges { get; init; }
}
