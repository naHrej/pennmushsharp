using System.Text.Json;
using PennMushSharp.Extraction;

var repoRoot = RepoLocator.FindRepositoryRoot();
var pennmushRoot = Path.Combine(repoRoot, "pennmush");
var headerPath = Path.Combine(pennmushRoot, "hdrs", "flag_tab.h");
var flagsHeaderPath = Path.Combine(pennmushRoot, "hdrs", "flags.h");
var oldFlagsHeaderPath = Path.Combine(pennmushRoot, "hdrs", "oldflags.h");

if (!File.Exists(headerPath))
  throw new FileNotFoundException("flag_tab.h not found", headerPath);
if (!File.Exists(flagsHeaderPath))
  throw new FileNotFoundException("flags.h not found", flagsHeaderPath);
if (!File.Exists(oldFlagsHeaderPath))
  throw new FileNotFoundException("oldflags.h not found", oldFlagsHeaderPath);

var macroTable = new MacroTable();
macroTable.Load(flagsHeaderPath);
macroTable.Load(oldFlagsHeaderPath);
macroTable.ResolvePending();

var headerText = CommentUtilities.RemoveComments(File.ReadAllText(headerPath));
headerText = CommentUtilities.CombineContinuations(headerText);
var flagEntries = InitializerParser.Parse(headerText, "flag_table", 6);
var powerEntries = InitializerParser.Parse(headerText, "power_table", 6);

var flags = flagEntries
  .Select(entry => FlagDefinitionFactory.Create(entry, macroTable))
  .Where(def => def is not null)
  .Cast<FlagDefinition>()
  .ToList();

var powers = powerEntries
  .Select(entry => FlagDefinitionFactory.Create(entry, macroTable))
  .Where(def => def is not null)
  .Cast<FlagDefinition>()
  .ToList();

var snapshot = new FlagSnapshot
{
  Source = Path.GetRelativePath(repoRoot, headerPath).Replace('\\', '/'),
  GeneratedUtc = DateTime.UtcNow,
  Flags = flags,
  Powers = powers
};

var outputDir = Path.Combine(repoRoot, "PennMushSharp", "docs", "generated");
Directory.CreateDirectory(outputDir);
var outputPath = Path.Combine(outputDir, "flags.json");

var json = JsonSerializer.Serialize(snapshot, new JsonSerializerOptions
{
  WriteIndented = true
});
File.WriteAllText(outputPath, json);

Console.WriteLine($"Extracted {flags.Count} flags and {powers.Count} powers to {outputPath}");

static class FlagDefinitionFactory
{
  public static FlagDefinition? Create(IReadOnlyList<string> fields, MacroTable macros)
  {
    if (fields.Count != 6)
      throw new InvalidOperationException($"Each entry must contain 6 fields, found {fields.Count}.");

    var nameToken = fields[0];
    if (string.Equals(nameToken.Trim(), "NULL", StringComparison.Ordinal))
      return null;

    var name = LiteralParser.ParseString(nameToken);
    var letter = LiteralParser.ParseChar(fields[1]);
    var normalizedLetter = letter is null or '\0' ? (char?)null : letter;
    var typeMask = macros.EvaluateUInt(fields[2]);
    var bitPosition = macros.EvaluateInt64(fields[3]);
    var setPerms = macros.EvaluateUInt(fields[4]);
    var clearPerms = macros.EvaluateUInt(fields[5]);

    return new FlagDefinition
    {
      Name = name,
      Letter = normalizedLetter,
      TypeMask = typeMask,
      BitPosition = bitPosition,
      SetPermissions = setPerms,
      ClearPermissions = clearPerms
    };
  }
}
