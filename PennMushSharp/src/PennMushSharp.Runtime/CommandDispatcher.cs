using Microsoft.Extensions.Logging;
using PennMushSharp.Commands;

namespace PennMushSharp.Runtime;

public sealed class CommandDispatcher
{
  private readonly CommandCatalog _catalog;
  private readonly ILogger<CommandDispatcher> _logger;

  public CommandDispatcher(CommandCatalog catalog, ILogger<CommandDispatcher> logger)
  {
    _catalog = catalog;
    _logger = logger;
  }

  public async ValueTask DispatchAsync(ICommandContext context, string input, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(input))
      return;

    var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var commandName = parts[0];
    var arguments = parts.Length > 1 ? parts[1] : string.Empty;

    if (!_catalog.TryGet(commandName, out var command) || command is null)
    {
      _logger.LogWarning("Unknown command '{Command}' for actor #{Actor}", commandName, context.Actor.DbRef);
      await context.Output.WriteLineAsync("Huh? (Try HELP)", cancellationToken);
      return;
    }

    await command.ExecuteAsync(context, arguments, cancellationToken);
  }
}
