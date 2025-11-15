using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PennMushSharp.Commands.Metadata;
using PennMushSharp.Core.Locks.Runtime;
using PennMushSharp.Core.Persistence;

namespace PennMushSharp.Commands;

public static class CommandCatalogBuilder
{
  public static CommandCatalog CreateDefault(IServiceProvider provider)
  {
    var catalog = new CommandCatalog();
    var registry = provider.GetRequiredService<ISessionRegistry>();
    var speech = provider.GetRequiredService<SpeechService>();
    var attributeService = provider.GetRequiredService<AttributeService>();
    var lockStore = provider.GetRequiredService<IMutableLockStore>();
    var gameState = provider.GetRequiredService<InMemoryGameState>();
    RegisterWithMetadata(catalog, new LookCommand(gameState));
    RegisterWithMetadata(catalog, new WhoCommand(registry));
    RegisterWithMetadata(catalog, new SayCommand(registry, speech));
    RegisterWithMetadata(catalog, new PoseCommand(registry, speech));
    RegisterWithMetadata(catalog, new SemiposeCommand(registry, speech));
    RegisterWithMetadata(catalog, new EmitCommand(registry, speech));
    RegisterWithMetadata(catalog, new PemitCommand(registry, speech));
    RegisterWithMetadata(catalog, new WhisperCommand(registry, speech));
    RegisterWithMetadata(catalog, new PageCommand(registry, speech));
    RegisterWithMetadata(catalog, new LockCommand(attributeService, lockStore));
    RegisterWithMetadata(catalog, new SetCommand(attributeService));
    RegisterWithMetadata(catalog, new ListCommand(attributeService));
    RegisterWithMetadata(catalog, new AtrLockCommand(attributeService));
    RegisterWithMetadata(catalog, new AtrChownCommand(attributeService));
    RegisterWithMetadata(catalog, new UnlockCommand(attributeService, lockStore));

    CommandMetadataCatalog.RegisterOverride(new CommandDefinition(
      name: "@EVAL",
      category: "utility",
      wizardOnly: false,
      aliases: Array.Empty<string>(),
      switches: Array.Empty<CommandSwitchDefinition>(),
      handler: "cmd_eval",
      typeFlags: CommandTypeFlags.NoParse | CommandTypeFlags.RsNoParse,
      flags: Array.Empty<string>(),
      powers: Array.Empty<string>()));
    var evalLogger = provider.GetRequiredService<ILogger<EvalCommand>>();
    RegisterWithMetadata(catalog, new EvalCommand(evalLogger));
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
