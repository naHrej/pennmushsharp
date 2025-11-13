using System.Collections.Concurrent;

namespace PennMushSharp.Commands.Metadata;

public static class CommandMetadataCatalog
{
  private static readonly IReadOnlyList<CommandDefinition> Definitions = new List<CommandDefinition>
  {
    new(
      "LOOK",
      category: "information",
      wizardOnly: false,
      aliases: new[] { "L" },
      switches: Array.Empty<CommandSwitchDefinition>()),
    new(
      "WHO",
      category: "information",
      wizardOnly: false,
      aliases: Array.Empty<string>(),
      switches: Array.Empty<CommandSwitchDefinition>()),
    new(
      "@PAGE",
      category: "communication",
      wizardOnly: false,
      aliases: new[] { "PAGE", "P" },
      switches: new[]
      {
        new CommandSwitchDefinition("quiet", requiresArgument: false),
        new CommandSwitchDefinition("silent", requiresArgument: false),
        new CommandSwitchDefinition("override", requiresArgument: false)
      }),
    new(
      "@DIG",
      category: "building",
      wizardOnly: false,
      aliases: Array.Empty<string>(),
      switches: new[]
      {
        new CommandSwitchDefinition("teleport", requiresArgument: false),
        new CommandSwitchDefinition("quiet", requiresArgument: false)
      })
  };

  private static readonly ConcurrentDictionary<string, CommandDefinition> DefinitionsByName = new(
    Definitions.ToDictionary(d => d.Name, StringComparer.OrdinalIgnoreCase));

  public static bool TryGet(string name, out CommandDefinition? definition)
  {
    return DefinitionsByName.TryGetValue(name, out definition);
  }
}
