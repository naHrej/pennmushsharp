using Microsoft.Extensions.Logging;
using PennMushSharp.Commands;
using PennMushSharp.Commands.Metadata;
using PennMushSharp.Commands.Parsing;

namespace PennMushSharp.Runtime;

public sealed class CommandDispatcher
{
  private readonly CommandCatalog _catalog;
  private readonly CommandParser _parser;
  private readonly ILogger<CommandDispatcher> _logger;

  public CommandDispatcher(CommandCatalog catalog, CommandParser parser, ILogger<CommandDispatcher> logger)
  {
    _catalog = catalog;
    _parser = parser;
    _logger = logger;
  }

  public async ValueTask DispatchAsync(ICommandContext context, string input, CancellationToken cancellationToken = default)
  {
    var invocations = _parser.Parse(input);
    if (invocations.Count == 0)
      return;

    foreach (var invocation in invocations)
    {
      if (!_catalog.TryGet(invocation.Name, out var command) || command is null)
      {
        _logger.LogWarning("Unknown command '{Command}' for actor #{Actor}", invocation.Name, context.Actor.DbRef);
        await context.Output.WriteLineAsync("Huh? (Try HELP)", cancellationToken);
        continue;
      }

      if (CommandMetadataCatalog.TryGet(command.Name, out var metadata) && metadata is not null)
      {
        if (metadata.WizardOnly)
        {
          await context.Output.WriteLineAsync("Permission denied.", cancellationToken);
          continue;
        }

        if (!ValidateSwitches(metadata.Switches, invocation.Switches, context, cancellationToken))
          continue;
      }

      await command.ExecuteAsync(context, invocation, cancellationToken);
    }
  }

  private static bool ValidateSwitches(
    IReadOnlyList<CommandSwitchDefinition> definitions,
    IReadOnlyList<CommandSwitch> switches,
    ICommandContext context,
    CancellationToken cancellationToken)
  {
    foreach (var commandSwitch in switches)
    {
      var definition = definitions.FirstOrDefault(d => d.Name.Equals(commandSwitch.Name, StringComparison.OrdinalIgnoreCase));
      if (definition is null)
      {
        context.Output.WriteLineAsync($"Unknown switch /{commandSwitch.Name}", cancellationToken);
        return false;
      }

      if (definition.RequiresArgument && string.IsNullOrEmpty(commandSwitch.Argument))
      {
        context.Output.WriteLineAsync($"Switch /{definition.Name} requires an argument.", cancellationToken);
        return false;
      }
    }

    return true;
  }
}
