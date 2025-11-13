using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;

namespace PennMushSharp.Commands.Metadata;

public static class CommandMetadataCatalog
{
  private static readonly ConcurrentDictionary<string, CommandDefinition> DefinitionsByName = new(
    LoadDefinitions());

  public static bool TryGet(string name, out CommandDefinition? definition)
  {
    return DefinitionsByName.TryGetValue(name, out definition);
  }

  public static void RegisterOverride(CommandDefinition definition)
  {
    ArgumentNullException.ThrowIfNull(definition);
    DefinitionsByName[definition.Name] = definition;
  }

  private static Dictionary<string, CommandDefinition> LoadDefinitions()
  {
    var assembly = typeof(CommandMetadataCatalog).Assembly;
    using var stream = assembly.GetManifestResourceStream("PennMushSharp.Commands.Generated.commands.json")
      ?? throw new InvalidOperationException("Embedded command metadata not found.");

    var snapshot = JsonSerializer.Deserialize<CommandMetadataSnapshot>(stream, new JsonSerializerOptions
    {
      PropertyNameCaseInsensitive = true
    }) ?? throw new InvalidOperationException("Failed to deserialize command metadata snapshot.");

    var map = new Dictionary<string, CommandDefinition>(StringComparer.OrdinalIgnoreCase);
    foreach (var record in snapshot.Commands)
    {
      var definition = Convert(record);
      map[definition.Name] = definition;
    }

    return map;
  }

  private static CommandDefinition Convert(CommandMetadataRecord record)
  {
    var wizardOnly = record.Flags.Any(f => string.Equals(f, "WIZARD", StringComparison.OrdinalIgnoreCase))
      || record.Powers.Any()
      || (record.TypeFlags & 0x08000000) != 0; // CMD_T_GOD

    var switches = record.Switches
      .Select(name => new CommandSwitchDefinition(name, requiresArgument: false))
      .ToList();

    return new CommandDefinition(
      record.Name,
      record.Category ?? "builtin",
      wizardOnly,
      Array.Empty<string>(),
      switches,
      record.Handler,
      record.TypeFlags,
      record.Flags,
      record.Powers);
  }
}
