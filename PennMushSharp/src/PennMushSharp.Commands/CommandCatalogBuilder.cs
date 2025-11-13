using Microsoft.Extensions.DependencyInjection;
using PennMushSharp.Commands.Metadata;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Commands;

public static class CommandCatalogBuilder
{
  public static CommandCatalog CreateDefault(IServiceProvider provider)
  {
    var catalog = new CommandCatalog();
    var registry = provider.GetRequiredService<ISessionRegistry>();
    var gameState = provider.GetRequiredService<InMemoryGameState>();
    RegisterWithMetadata(catalog, new LookCommand(gameState));
    RegisterWithMetadata(catalog, new WhoCommand(registry));

    CommandMetadataCatalog.RegisterOverride(new CommandDefinition(
      name: "@EVAL",
      category: "utility",
      wizardOnly: false,
      aliases: Array.Empty<string>(),
      switches: Array.Empty<CommandSwitchDefinition>(),
      handler: "cmd_eval",
      typeFlags: 0,
      flags: Array.Empty<string>(),
      powers: Array.Empty<string>()));
    RegisterWithMetadata(catalog, new EvalCommand());
    return catalog;
  }

  private static void RegisterWithMetadata(CommandCatalog catalog, ICommand command)
  {
    catalog.Register(command);
    if (CommandMetadataCatalog.TryGet(command.Name, out var definition) && definition is not null)
    {
      foreach (var alias in definition.Aliases)
      {
        if (!string.IsNullOrWhiteSpace(alias))
          catalog.RegisterAlias(alias, command);
      }
    }
  }
}
